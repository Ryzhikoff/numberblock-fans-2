using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Draws an exact Numberblock volume as original Ones, then sends every unit
/// hopping and running away from the impact point. GPU instancing keeps the
/// 5,000-unit finale light enough for a vertical short.
/// </summary>
public class S84_UnitBurst : MonoBehaviour
{
    public const int GridSize = 10;
    public const int ThousandUnitCount = 1000;
    public const int FiveThousandUnitCount = 5000;
    public const int MaximumUnitCount = FiveThousandUnitCount;

    public GameObject onePrefab;
    public Collider groundCollider;

    public bool IsPrepared { get; private set; }
    public bool Active { get; private set; }
    public int ActiveUnitCount => Active ? activeCount : 0;
    public int LastBurstCount { get; private set; }
    public int EscapedUnitCount { get; private set; }

    private const int MaximumInstancesPerDraw = 1023;
    private readonly List<RenderPart> renderParts = new List<RenderPart>(8);
    private readonly Matrix4x4[] batchMatrices = new Matrix4x4[MaximumInstancesPerDraw];
    private readonly Vector3[] positions = new Vector3[MaximumUnitCount];
    private readonly Vector3[] velocities = new Vector3[MaximumUnitCount];
    private readonly Vector3[] runDirections = new Vector3[MaximumUnitCount];
    private readonly Vector3[] spinAxes = new Vector3[MaximumUnitCount];
    private readonly Quaternion[] rotations = new Quaternion[MaximumUnitCount];
    private readonly float[] spinSpeeds = new float[MaximumUnitCount];
    private readonly float[] runSpeeds = new float[MaximumUnitCount];
    private readonly float[] hopSpeeds = new float[MaximumUnitCount];
    private readonly float[] unitSizes = new float[MaximumUnitCount];
    private readonly float[] scaleVariations = new float[MaximumUnitCount];
    private readonly bool[] landed = new bool[MaximumUnitCount];
    private readonly bool[] escaped = new bool[MaximumUnitCount];
    private readonly Matrix4x4[] unitMatrices = new Matrix4x4[MaximumUnitCount];

    private int activeCount;
    private float burstStartTime;

    private sealed class RenderPart
    {
        public Mesh Mesh;
        public int SubmeshIndex;
        public Material Material;
        public Matrix4x4 LocalMatrix;
    }

    public void Prepare()
    {
        if (IsPrepared)
        {
            return;
        }
        if (onePrefab == null)
        {
            Debug.LogError("[S84_BURST] Original One prefab is missing.", this);
            return;
        }
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogError("[S84_BURST] GPU instancing is not supported.", this);
            return;
        }

        GameObject template = Instantiate(onePrefab, Vector3.zero, Quaternion.identity);
        template.name = "S84 Original One Render Template";
        try
        {
            Renderer[] allRenderers = template.GetComponentsInChildren<Renderer>(true);
            Bounds templateBounds = BoundsOf(allRenderers, template.transform.position);
            Vector3 localCenter = template.transform.InverseTransformPoint(templateBounds.center);

            foreach (MeshRenderer meshRenderer in template.GetComponentsInChildren<MeshRenderer>(true))
            {
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter == null || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                Matrix4x4 localMatrix = Matrix4x4.Translate(-localCenter) *
                    template.transform.worldToLocalMatrix * meshRenderer.transform.localToWorldMatrix;
                Material[] materials = meshRenderer.sharedMaterials;
                int partCount = Mathf.Min(materials.Length, meshFilter.sharedMesh.subMeshCount);
                for (int submesh = 0; submesh < partCount; submesh++)
                {
                    if (materials[submesh] == null)
                    {
                        continue;
                    }

                    Material runtimeMaterial = new Material(materials[submesh])
                    {
                        name = $"S84 Instanced {materials[submesh].name}",
                        enableInstancing = true
                    };
                    renderParts.Add(new RenderPart
                    {
                        Mesh = meshFilter.sharedMesh,
                        SubmeshIndex = submesh,
                        Material = runtimeMaterial,
                        LocalMatrix = localMatrix
                    });
                }
            }
        }
        finally
        {
            template.SetActive(false);
            Destroy(template);
        }

        if (renderParts.Count == 0)
        {
            Debug.LogError("[S84_BURST] Original One prefab has no renderable mesh parts.", this);
            return;
        }

        IsPrepared = true;
    }

    public void Begin(Bounds sourceBounds, Vector3Int sourceGrid, int unitCount, float directionBias)
    {
        Prepare();
        if (!IsPrepared)
        {
            return;
        }
        if (unitCount < 1 || unitCount > MaximumUnitCount || GridVolume(sourceGrid) != unitCount)
        {
            Debug.LogError(
                $"[S84_BURST] Invalid exact grid {sourceGrid.x}x{sourceGrid.y}x{sourceGrid.z} " +
                $"for {unitCount} units.",
                this);
            return;
        }

        activeCount = unitCount;
        LastBurstCount = unitCount;
        EscapedUnitCount = 0;
        float cellX = sourceBounds.size.x / sourceGrid.x;
        float cellY = sourceBounds.size.y / sourceGrid.y;
        float cellZ = sourceBounds.size.z / sourceGrid.z;
        // The crushed Thousand has compressed vertical spacing at the instant
        // it pops. Keep each resulting One at its real horizontal cell size so
        // the units overlap under pressure and then burst outward at full size.
        float unitSize = Mathf.Min(cellX, cellZ);
        int index = 0;

        for (int y = 0; y < sourceGrid.y; y++)
        {
            for (int z = 0; z < sourceGrid.z; z++)
            {
                for (int x = 0; x < sourceGrid.x; x++)
                {
                    Vector3 normalized = new Vector3(
                        (x + 0.5f) / sourceGrid.x * 2f - 1f,
                        (y + 0.5f) / sourceGrid.y * 2f - 1f,
                        (z + 0.5f) / sourceGrid.z * 2f - 1f);
                    positions[index] = new Vector3(
                        sourceBounds.min.x + (x + 0.5f) * cellX,
                        sourceBounds.min.y + (y + 0.5f) * cellY,
                        sourceBounds.min.z + (z + 0.5f) * cellZ);

                    float side = Hash01(index * 17 + 5) < 0.5f ? -1f : 1f;
                    float biasedSide = Mathf.Abs(directionBias) > 0.01f ? directionBias : side;
                    Vector3 burstDirection = new Vector3(
                        normalized.x * 0.5f + biasedSide * 0.72f + side * 0.32f,
                        0.62f + Hash01(index * 19 + 11) * 0.72f,
                        normalized.z * 0.46f + (Hash01(index * 23 + 17) - 0.5f) * 0.5f);
                    burstDirection.Normalize();
                    float burstSpeed = Mathf.Lerp(
                        unitCount == ThousandUnitCount ? 4.8f : 5.4f,
                        unitCount == ThousandUnitCount ? 8.4f : 9.6f,
                        Hash01(index * 29 + 31));
                    velocities[index] = burstDirection * burstSpeed;

                    Vector3 runDirection = new Vector3(
                        side * Mathf.Lerp(0.8f, 1.2f, Hash01(index * 31 + 41)),
                        0f,
                        (Hash01(index * 37 + 47) - 0.5f) * 1.15f);
                    runDirections[index] = runDirection.normalized;
                    runSpeeds[index] = Mathf.Lerp(4.2f, 7.4f, Hash01(index * 41 + 59));
                    hopSpeeds[index] = Mathf.Lerp(1.75f, 3.15f, Hash01(index * 43 + 67));

                    Vector3 axis = new Vector3(
                        Hash01(index * 47 + 71) - 0.5f,
                        Hash01(index * 47 + 79) - 0.5f,
                        Hash01(index * 47 + 89) - 0.5f);
                    spinAxes[index] = axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.up;
                    rotations[index] = Quaternion.identity;
                    spinSpeeds[index] = Mathf.Lerp(150f, 520f, Hash01(index * 53 + 97));
                    unitSizes[index] = unitSize;
                    scaleVariations[index] = Mathf.Lerp(0.94f, 1.06f, Hash01(index * 59 + 107));
                    landed[index] = false;
                    escaped[index] = false;
                    index++;
                }
            }
        }

        burstStartTime = Time.time;
        Active = true;
    }

    public void Hide()
    {
        Active = false;
        activeCount = 0;
    }

    private void LateUpdate()
    {
        if (!Active || !IsPrepared || activeCount == 0)
        {
            return;
        }

        float elapsed = Time.time - burstStartTime;
        float remainingTime = Mathf.Min(Time.deltaTime, 0.12f);
        float groundHeight = groundCollider != null && groundCollider.enabled
            ? groundCollider.bounds.max.y
            : 0f;
        while (remainingTime > 0f)
        {
            float step = Mathf.Min(0.025f, remainingTime);
            SimulateStep(step, groundHeight);
            remainingTime -= step;
        }

        float revealScale = elapsed < 0.18f
            ? Mathf.Lerp(1.18f, 1f, elapsed / 0.18f)
            : 1f;
        for (int i = 0; i < activeCount; i++)
        {
            float scale = unitSizes[i] * scaleVariations[i] * revealScale;
            unitMatrices[i] = Matrix4x4.TRS(positions[i], rotations[i], Vector3.one * scale);
        }

        foreach (RenderPart part in renderParts)
        {
            for (int first = 0; first < activeCount; first += MaximumInstancesPerDraw)
            {
                int count = Mathf.Min(MaximumInstancesPerDraw, activeCount - first);
                for (int i = 0; i < count; i++)
                {
                    batchMatrices[i] = unitMatrices[first + i] * part.LocalMatrix;
                }

#pragma warning disable 618
                Graphics.DrawMeshInstanced(
                    part.Mesh,
                    part.SubmeshIndex,
                    part.Material,
                    batchMatrices,
                    count,
                    null,
                    ShadowCastingMode.Off,
                    false,
                    gameObject.layer);
#pragma warning restore 618
            }
        }
    }

    private void SimulateStep(float deltaTime, float groundHeight)
    {
        const float gravity = 12.5f;

        for (int i = 0; i < activeCount; i++)
        {
            Vector3 velocity = velocities[i];
            if (landed[i])
            {
                Vector3 targetHorizontal = runDirections[i] * runSpeeds[i];
                velocity.x = Mathf.MoveTowards(velocity.x, targetHorizontal.x, deltaTime * 11f);
                velocity.z = Mathf.MoveTowards(velocity.z, targetHorizontal.z, deltaTime * 11f);
            }

            velocity.y -= gravity * deltaTime;
            Vector3 position = positions[i] + velocity * deltaTime;
            rotations[i] = Quaternion.AngleAxis(
                spinSpeeds[i] * deltaTime,
                spinAxes[i]) * rotations[i];

            float halfSize = unitSizes[i] * scaleVariations[i] * 0.5f;
            float floorY = groundHeight + halfSize;
            if (position.y <= floorY)
            {
                position.y = floorY;
                landed[i] = true;
                velocity.y = hopSpeeds[i];
                spinSpeeds[i] *= 0.985f;
            }

            positions[i] = position;
            velocities[i] = velocity;
            if (!escaped[i] && (Mathf.Abs(position.x) > 6.2f || Mathf.Abs(position.z) > 8.5f))
            {
                escaped[i] = true;
                EscapedUnitCount++;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (RenderPart part in renderParts)
        {
            if (part.Material != null)
            {
                Destroy(part.Material);
            }
        }
    }

    private static int GridVolume(Vector3Int grid)
    {
        return grid.x * grid.y * grid.z;
    }

    private static Bounds BoundsOf(Renderer[] renderers, Vector3 fallbackCenter)
    {
        if (renderers.Length == 0)
        {
            return new Bounds(fallbackCenter, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private static float Hash01(int value)
    {
        unchecked
        {
            uint hash = (uint)value;
            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            hash *= 0x846ca68b;
            hash ^= hash >> 16;
            return (hash & 0x00ffffff) / 16777215f;
        }
    }
}
