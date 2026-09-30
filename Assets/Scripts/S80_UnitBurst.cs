using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Renders the exact 10 x 10 x 10 composition of each Thousand as real One
/// geometry and gives all 2,000 units deterministic collision trajectories.
/// </summary>
public class S80_UnitBurst : MonoBehaviour
{
    public const int GridSize = 10;
    public const int UnitsPerThousand = GridSize * GridSize * GridSize;
    public const int ThousandCount = 2;
    public const int TotalUnitCount = UnitsPerThousand * ThousandCount;

    public GameObject onePrefab;
    public Collider groundCollider;

    public bool IsPrepared { get; private set; }
    public bool Active { get; private set; }
    public int RenderedUnitCount => Active ? TotalUnitCount : 0;
    public int SettledUnitCount { get; private set; }

    private const int MaximumInstancesPerDraw = 1023;
    private readonly List<RenderPart> renderParts = new List<RenderPart>(6);
    private readonly Matrix4x4[] batchMatrices = new Matrix4x4[MaximumInstancesPerDraw];

    private Vector3[] currentPositions;
    private Vector3[] velocities;
    private Vector3[] spinAxes;
    private Quaternion[] rotations;
    private float[] spinSpeeds;
    private float[] scaleVariations;
    private float[] unitSizes;
    private bool[] settled;
    private Matrix4x4[] unitMatrices;
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
            Debug.LogError("[S80_BURST] One prefab is missing.", this);
            return;
        }
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogError("[S80_BURST] This graphics device does not support GPU instancing.", this);
            return;
        }

        GameObject template = Instantiate(onePrefab, Vector3.zero, Quaternion.identity);
        template.name = "One Render Template";
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
                    template.transform.worldToLocalMatrix *
                    meshRenderer.transform.localToWorldMatrix;
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
                        name = $"S80 Instanced {materials[submesh].name}",
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
            Debug.LogError("[S80_BURST] The One prefab has no instanced mesh parts.", this);
            return;
        }

        currentPositions = new Vector3[TotalUnitCount];
        velocities = new Vector3[TotalUnitCount];
        spinAxes = new Vector3[TotalUnitCount];
        rotations = new Quaternion[TotalUnitCount];
        spinSpeeds = new float[TotalUnitCount];
        scaleVariations = new float[TotalUnitCount];
        unitSizes = new float[TotalUnitCount];
        settled = new bool[TotalUnitCount];
        unitMatrices = new Matrix4x4[TotalUnitCount];
        IsPrepared = true;
    }

    public void Begin(Bounds leftThousand, Bounds rightThousand)
    {
        Prepare();
        if (!IsPrepared)
        {
            return;
        }

        FillThousand(0, leftThousand, -1f);
        FillThousand(UnitsPerThousand, rightThousand, 1f);
        burstStartTime = Time.time;
        SettledUnitCount = 0;
        Active = true;
    }

    private void FillThousand(int firstIndex, Bounds bounds, float side)
    {
        float unitSize = Mathf.Min(bounds.size.x, Mathf.Min(bounds.size.y, bounds.size.z)) / GridSize;
        Vector3 center = bounds.center;
        int localIndex = 0;

        for (int y = 0; y < GridSize; y++)
        {
            for (int z = 0; z < GridSize; z++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    int index = firstIndex + localIndex;
                    Vector3 offset = new Vector3(
                        (x + 0.5f - GridSize * 0.5f) * unitSize,
                        (y + 0.5f - GridSize * 0.5f) * unitSize,
                        (z + 0.5f - GridSize * 0.5f) * unitSize);
                    currentPositions[index] = center + offset;

                    Vector3 normalizedOffset = offset / Mathf.Max(unitSize * GridSize * 0.5f, 0.001f);
                    Vector3 jitter = new Vector3(
                        Hash01(index * 3 + 13) - 0.5f,
                        Hash01(index * 3 + 31) - 0.35f,
                        Hash01(index * 3 + 59) - 0.5f) * 0.7f;
                    Vector3 direction = normalizedOffset + jitter + new Vector3(side * 0.92f, 0.58f, 0f);
                    direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.up;
                    float speed = Mathf.Lerp(4.8f, 11.6f, Hash01(index * 5 + 83));
                    velocities[index] = direction * speed + Vector3.up * 1.15f;

                    Vector3 axis = new Vector3(
                        Hash01(index * 7 + 7) - 0.5f,
                        Hash01(index * 7 + 19) - 0.5f,
                        Hash01(index * 7 + 37) - 0.5f);
                    spinAxes[index] = axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.up;
                    rotations[index] = Quaternion.identity;
                    spinSpeeds[index] = Mathf.Lerp(130f, 540f, Hash01(index * 11 + 107));
                    scaleVariations[index] = Mathf.Lerp(0.93f, 1.07f, Hash01(index * 13 + 149));
                    unitSizes[index] = unitSize;
                    settled[index] = false;
                    localIndex++;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (!Active || !IsPrepared)
        {
            return;
        }

        float elapsed = Time.time - burstStartTime;
        float revealPulse = elapsed < 0.2f
            ? 1f + Mathf.Sin(Mathf.Clamp01(elapsed / 0.2f) * Mathf.PI) * 0.14f
            : 1f;

        if (elapsed >= 0.1f)
        {
            float remainingTime = Mathf.Min(Time.deltaTime, 0.12f);
            float groundHeight = groundCollider != null && groundCollider.enabled
                ? groundCollider.bounds.max.y
                : 0f;
            while (remainingTime > 0f)
            {
                float step = Mathf.Min(0.025f, remainingTime);
                SimulateStep(step, groundHeight, elapsed);
                remainingTime -= step;
            }
        }

        for (int i = 0; i < TotalUnitCount; i++)
        {
            float scale = unitSizes[i] * scaleVariations[i] * revealPulse;
            unitMatrices[i] = Matrix4x4.TRS(
                currentPositions[i],
                rotations[i],
                Vector3.one * scale);
        }

        foreach (RenderPart part in renderParts)
        {
            for (int first = 0; first < TotalUnitCount; first += MaximumInstancesPerDraw)
            {
                int count = Mathf.Min(MaximumInstancesPerDraw, TotalUnitCount - first);
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

    private void SimulateStep(float deltaTime, float groundHeight, float elapsed)
    {
        const float gravity = 7.8f;
        const float bounce = 0.28f;
        const float groundFriction = 0.7f;

        for (int i = 0; i < TotalUnitCount; i++)
        {
            if (settled[i])
            {
                continue;
            }

            Vector3 velocity = velocities[i];
            velocity.y -= gravity * deltaTime;
            Vector3 position = currentPositions[i] + velocity * deltaTime;
            rotations[i] = Quaternion.AngleAxis(
                spinSpeeds[i] * deltaTime,
                spinAxes[i]) * rotations[i];

            float halfSize = unitSizes[i] * scaleVariations[i] * 0.5f;
            float restingHeight = groundHeight + halfSize;
            if (elapsed > 6.25f)
            {
                position.y = restingHeight;
                SetSettled(i, position);
                continue;
            }

            if (position.y <= restingHeight)
            {
                position.y = restingHeight;
                if (velocity.y < 0f)
                {
                    velocity.y = -velocity.y * bounce;
                    velocity.x *= groundFriction;
                    velocity.z *= groundFriction;
                    spinSpeeds[i] *= 0.68f;
                }

                float horizontalSpeedSquared = velocity.x * velocity.x + velocity.z * velocity.z;
                if ((velocity.y < 0.38f && horizontalSpeedSquared < 0.36f) || elapsed > 5.85f)
                {
                    SetSettled(i, position);
                    continue;
                }
            }

            currentPositions[i] = position;
            velocities[i] = velocity;
        }
    }

    private void SetSettled(int index, Vector3 position)
    {
        if (!settled[index])
        {
            settled[index] = true;
            SettledUnitCount++;
        }
        currentPositions[index] = position;
        velocities[index] = Vector3.zero;
        spinSpeeds[index] = 0f;
        rotations[index] = SnapToRightAngles(rotations[index]);
    }

    private static Quaternion SnapToRightAngles(Quaternion rotation)
    {
        Vector3 euler = rotation.eulerAngles;
        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;
        return Quaternion.Euler(euler);
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
