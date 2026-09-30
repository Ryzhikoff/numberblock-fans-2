using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Draws exact unit counts from the original One prefab. A milestone result is
/// scattered from the two colliding Numberblocks, then rebuilt in its source grid.
/// </summary>
public class S83_UnitBurst : MonoBehaviour
{
    public const int MaximumUnitCount = 1000;
    public const float ScatterDuration = 0.82f;
    public const float FormationDuration = 1.48f;

    public GameObject onePrefab;
    public float floorY = 0.12f;

    public bool IsPrepared { get; private set; }
    public bool Active { get; private set; }
    public bool FormationComplete { get; private set; }
    public int ActiveUnitCount => Active ? activeCount : 0;
    public int LastCompletedCount { get; private set; }

    private const int MaximumInstancesPerDraw = 1023;
    private readonly List<RenderPart> renderParts = new List<RenderPart>(8);
    private readonly Matrix4x4[] batchMatrices = new Matrix4x4[MaximumInstancesPerDraw];
    private readonly Vector3[] positions = new Vector3[MaximumUnitCount];
    private readonly Vector3[] velocities = new Vector3[MaximumUnitCount];
    private readonly Vector3[] gatherStarts = new Vector3[MaximumUnitCount];
    private readonly Vector3[] targets = new Vector3[MaximumUnitCount];
    private readonly Vector3[] spinAxes = new Vector3[MaximumUnitCount];
    private readonly Quaternion[] rotations = new Quaternion[MaximumUnitCount];
    private readonly float[] spinSpeeds = new float[MaximumUnitCount];
    private readonly float[] scaleVariations = new float[MaximumUnitCount];
    private readonly Matrix4x4[] unitMatrices = new Matrix4x4[MaximumUnitCount];

    private int activeCount;
    private float unitScale;
    private float burstStartTime;
    private bool gatherCaptured;

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
            Debug.LogError("[S83_BURST] Original One prefab is missing.", this);
            return;
        }
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogError("[S83_BURST] GPU instancing is not supported.", this);
            return;
        }

        GameObject template = Instantiate(onePrefab, Vector3.zero, Quaternion.identity);
        template.name = "S83 Original One Render Template";
        try
        {
            Renderer[] allRenderers = template.GetComponentsInChildren<Renderer>(true);
            Bounds templateBounds = BoundsOf(allRenderers, template.transform.position);
            Vector3 localCenter = template.transform.InverseTransformPoint(templateBounds.center);

            foreach (MeshRenderer meshRenderer in template.GetComponentsInChildren<MeshRenderer>(true))
            {
                MeshFilter filter = meshRenderer.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null)
                {
                    continue;
                }

                Matrix4x4 localMatrix = Matrix4x4.Translate(-localCenter) *
                    template.transform.worldToLocalMatrix * meshRenderer.transform.localToWorldMatrix;
                Material[] materials = meshRenderer.sharedMaterials;
                int partCount = Mathf.Min(materials.Length, filter.sharedMesh.subMeshCount);
                for (int submesh = 0; submesh < partCount; submesh++)
                {
                    if (materials[submesh] == null)
                    {
                        continue;
                    }

                    Material runtimeMaterial = new Material(materials[submesh])
                    {
                        name = $"S83 Instanced {materials[submesh].name}",
                        enableInstancing = true
                    };
                    renderParts.Add(new RenderPart
                    {
                        Mesh = filter.sharedMesh,
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
            Debug.LogError("[S83_BURST] Original One prefab has no renderable parts.", this);
            return;
        }
        IsPrepared = true;
    }

    public void Begin(
        Bounds leftBounds,
        Vector3Int leftGrid,
        int leftCount,
        Bounds rightBounds,
        Vector3Int rightGrid,
        int rightCount,
        Bounds resultBounds,
        Vector3Int resultGrid)
    {
        Prepare();
        int total = leftCount + rightCount;
        if (!IsPrepared || total < 1 || total > MaximumUnitCount)
        {
            Debug.LogError($"[S83_BURST] Invalid exact unit count: {total}.", this);
            return;
        }
        if (GridVolume(leftGrid) != leftCount || GridVolume(rightGrid) != rightCount ||
            GridVolume(resultGrid) != total)
        {
            Debug.LogError(
                $"[S83_BURST] Grid mismatch: {leftCount} + {rightCount} != {total}.", this);
            return;
        }

        activeCount = total;
        float cellX = resultBounds.size.x / Mathf.Max(1, resultGrid.x);
        float cellY = resultBounds.size.y / Mathf.Max(1, resultGrid.y);
        float cellZ = resultBounds.size.z / Mathf.Max(1, resultGrid.z);
        unitScale = Mathf.Min(cellX, Mathf.Min(cellY, cellZ));

        FillInitialGrid(0, leftBounds, leftGrid, -1f);
        FillInitialGrid(leftCount, rightBounds, rightGrid, 1f);
        FillTargetGrid(resultBounds, resultGrid);

        burstStartTime = Time.time;
        gatherCaptured = false;
        FormationComplete = false;
        Active = true;
    }

    public void Hide()
    {
        if (FormationComplete)
        {
            LastCompletedCount = activeCount;
        }
        Active = false;
    }

    private void FillInitialGrid(int firstIndex, Bounds bounds, Vector3Int grid, float side)
    {
        int localIndex = 0;
        for (int z = 0; z < grid.z; z++)
        {
            for (int y = 0; y < grid.y; y++)
            {
                for (int x = 0; x < grid.x; x++)
                {
                    int index = firstIndex + localIndex;
                    positions[index] = CellPosition(bounds, grid, x, y, z);
                    Vector3 jitter = new Vector3(
                        Hash01(index * 7 + 11) - 0.5f,
                        Hash01(index * 7 + 29) * 0.8f,
                        Hash01(index * 7 + 47) - 0.5f);
                    Vector3 direction = new Vector3(side * 1.45f, 0.9f, 0.15f) + jitter;
                    velocities[index] = direction.normalized *
                        Mathf.Lerp(3.6f, 7.4f, Hash01(index * 11 + 71));
                    Vector3 axis = new Vector3(
                        Hash01(index * 13 + 3) - 0.5f,
                        Hash01(index * 13 + 19) - 0.5f,
                        Hash01(index * 13 + 43) - 0.5f);
                    spinAxes[index] = axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.up;
                    rotations[index] = Quaternion.identity;
                    spinSpeeds[index] = Mathf.Lerp(100f, 390f, Hash01(index * 17 + 97));
                    scaleVariations[index] = Mathf.Lerp(0.9f, 1.07f, Hash01(index * 19 + 131));
                    localIndex++;
                }
            }
        }
    }

    private void FillTargetGrid(Bounds bounds, Vector3Int grid)
    {
        int index = 0;
        for (int z = 0; z < grid.z; z++)
        {
            for (int y = 0; y < grid.y; y++)
            {
                for (int x = 0; x < grid.x; x++)
                {
                    targets[index++] = CellPosition(bounds, grid, x, y, z);
                }
            }
        }
    }

    private static Vector3 CellPosition(Bounds bounds, Vector3Int grid, int x, int y, int z)
    {
        return new Vector3(
            bounds.min.x + (x + 0.5f) * bounds.size.x / grid.x,
            bounds.min.y + (y + 0.5f) * bounds.size.y / grid.y,
            bounds.min.z + (z + 0.5f) * bounds.size.z / grid.z);
    }

    private void LateUpdate()
    {
        if (!Active || !IsPrepared)
        {
            return;
        }

        float elapsed = Time.time - burstStartTime;
        float gatherProgress = 0f;
        if (elapsed < ScatterDuration)
        {
            SimulateScatter(Mathf.Min(Time.deltaTime, 0.06f));
        }
        else
        {
            if (!gatherCaptured)
            {
                for (int i = 0; i < activeCount; i++)
                {
                    gatherStarts[i] = positions[i];
                }
                gatherCaptured = true;
            }

            gatherProgress = Mathf.Clamp01((elapsed - ScatterDuration) / FormationDuration);
            float eased = EaseInOutCubic(gatherProgress);
            for (int i = 0; i < activeCount; i++)
            {
                float phase = Hash01(i * 23 + 151) * Mathf.PI * 2f;
                float arc = Mathf.Sin(gatherProgress * Mathf.PI) *
                    (0.22f + Hash01(i * 29 + 181) * 0.65f);
                Vector3 swirl = new Vector3(
                    Mathf.Cos(phase + gatherProgress * Mathf.PI * 3f),
                    Mathf.Sin(phase + gatherProgress * Mathf.PI * 2f) * 0.65f,
                    Mathf.Sin(phase) * 0.42f) * arc;
                positions[i] = Vector3.Lerp(gatherStarts[i], targets[i], eased) + swirl;
                rotations[i] = Quaternion.Slerp(rotations[i], Quaternion.identity, eased);
            }
            FormationComplete = gatherProgress >= 1f;
        }

        float uniformity = EaseInOutCubic(gatherProgress);
        for (int i = 0; i < activeCount; i++)
        {
            float scale = unitScale * Mathf.Lerp(scaleVariations[i], 1f, uniformity);
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

    private void SimulateScatter(float deltaTime)
    {
        for (int i = 0; i < activeCount; i++)
        {
            velocities[i] += Physics.gravity * 0.7f * deltaTime;
            positions[i] += velocities[i] * deltaTime;
            if (positions[i].y < floorY)
            {
                positions[i].y = floorY;
                velocities[i].y = Mathf.Abs(velocities[i].y) * 0.46f;
                velocities[i].x *= 0.84f;
                velocities[i].z *= 0.84f;
            }
            rotations[i] = Quaternion.AngleAxis(
                spinSpeeds[i] * deltaTime,
                spinAxes[i]) * rotations[i];
        }
    }

    private static int GridVolume(Vector3Int grid)
    {
        return Mathf.Max(1, grid.x) * Mathf.Max(1, grid.y) * Mathf.Max(1, grid.z);
    }

    private static Bounds BoundsOf(Renderer[] renderers, Vector3 fallback)
    {
        bool found = false;
        Bounds bounds = new Bounds(fallback, Vector3.one);
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
            {
                continue;
            }
            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        return bounds;
    }

    private static float Hash01(int value)
    {
        uint x = (uint)value;
        x ^= x >> 16;
        x *= 0x7feb352d;
        x ^= x >> 15;
        x *= 0x846ca68b;
        x ^= x >> 16;
        return (x & 0x00ffffff) / 16777215f;
    }

    private static float EaseInOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value < 0.5f
            ? 4f * value * value * value
            : 1f - Mathf.Pow(-2f * value + 2f, 3f) * 0.5f;
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
}
