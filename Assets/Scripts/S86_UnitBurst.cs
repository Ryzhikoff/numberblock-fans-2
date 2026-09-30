using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Draws the exact original Ones released by the three collisions in Scene 86.
/// A small GPU-instanced simulation keeps all 160 cubes independent without
/// creating a Rigidbody/GameObject hierarchy for every unit.
/// </summary>
public class S86_UnitBurst : MonoBehaviour
{
    public const int MaximumUnitCount = 160;

    public GameObject onePrefab;
    public float floorY = 0.02f;

    public bool IsPrepared { get; private set; }
    public int BurstCount { get; private set; }
    public int TotalSpawned { get; private set; }
    public int ActiveUnitCount
    {
        get
        {
            int count = 0;
            foreach (BurstState burst in bursts)
            {
                count += burst.Count;
            }
            return count;
        }
    }

    private const int MaximumInstancesPerDraw = 1023;
    private const float BurstLifetime = 9f;
    private readonly List<RenderPart> renderParts = new List<RenderPart>(8);
    private readonly List<BurstState> bursts = new List<BurstState>(3);
    private readonly Matrix4x4[] batchMatrices = new Matrix4x4[MaximumInstancesPerDraw];

    private sealed class RenderPart
    {
        public Mesh Mesh;
        public int SubmeshIndex;
        public Material Material;
        public Matrix4x4 LocalMatrix;
    }

    private sealed class BurstState
    {
        public int Count;
        public float UnitScale;
        public float Age;
        public Vector3[] Positions;
        public Vector3[] Velocities;
        public Quaternion[] Rotations;
        public Vector3[] SpinAxes;
        public float[] SpinSpeeds;
        public Matrix4x4[] Matrices;
    }

    public void Prepare()
    {
        if (IsPrepared)
        {
            return;
        }
        if (onePrefab == null)
        {
            Debug.LogError("[S86_BURST] Original One prefab is missing.", this);
            return;
        }
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogError("[S86_BURST] GPU instancing is not supported.", this);
            return;
        }

        GameObject template = Instantiate(onePrefab, Vector3.zero, Quaternion.identity);
        template.name = "S86 Original One Render Template";
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
                        name = $"S86 Instanced {materials[submesh].name}",
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
            Debug.LogError("[S86_BURST] Original One prefab has no renderable meshes.", this);
            return;
        }
        IsPrepared = true;
    }

    public void Begin(Bounds sourceBounds, Vector3Int grid, int exactCount, int seed)
    {
        Prepare();
        if (!IsPrepared || exactCount < 1 || TotalSpawned + exactCount > MaximumUnitCount)
        {
            Debug.LogError($"[S86_BURST] Invalid cumulative unit count: {TotalSpawned} + {exactCount}.", this);
            return;
        }
        if ((long)grid.x * grid.y * grid.z != exactCount)
        {
            Debug.LogError($"[S86_BURST] Grid {grid} does not contain {exactCount} units.", this);
            return;
        }

        BurstState burst = new BurstState
        {
            Count = exactCount,
            Positions = new Vector3[exactCount],
            Velocities = new Vector3[exactCount],
            Rotations = new Quaternion[exactCount],
            SpinAxes = new Vector3[exactCount],
            SpinSpeeds = new float[exactCount],
            Matrices = new Matrix4x4[exactCount]
        };
        float cellX = sourceBounds.size.x / grid.x;
        float cellY = sourceBounds.size.y / grid.y;
        float cellZ = sourceBounds.size.z / grid.z;
        burst.UnitScale = Mathf.Min(cellX, Mathf.Min(cellY, cellZ));

        int index = 0;
        for (int z = 0; z < grid.z; z++)
        {
            for (int y = 0; y < grid.y; y++)
            {
                for (int x = 0; x < grid.x; x++)
                {
                    Vector3 position = new Vector3(
                        sourceBounds.min.x + (x + 0.5f) * cellX,
                        sourceBounds.min.y + (y + 0.5f) * cellY,
                        sourceBounds.min.z + (z + 0.5f) * cellZ);
                    float side = Hash01(seed + index * 17 + 3) < 0.5f ? -1f : 1f;
                    Vector3 direction = new Vector3(
                        side * Mathf.Lerp(0.55f, 1.35f, Hash01(seed + index * 19 + 5)),
                        Mathf.Lerp(0.7f, 1.45f, Hash01(seed + index * 23 + 7)),
                        Mathf.Lerp(-0.75f, 0.75f, Hash01(seed + index * 29 + 11)));
                    burst.Positions[index] = position;
                    burst.Velocities[index] = direction.normalized *
                        Mathf.Lerp(5.2f, 11.5f, Hash01(seed + index * 31 + 13));
                    Vector3 axis = new Vector3(
                        Hash01(seed + index * 37 + 17) - 0.5f,
                        Hash01(seed + index * 41 + 19) - 0.5f,
                        Hash01(seed + index * 43 + 23) - 0.5f);
                    burst.SpinAxes[index] = axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.up;
                    burst.SpinSpeeds[index] = Mathf.Lerp(160f, 510f, Hash01(seed + index * 47 + 29));
                    burst.Rotations[index] = Quaternion.identity;
                    index++;
                }
            }
        }

        bursts.Add(burst);
        BurstCount++;
        TotalSpawned += exactCount;
        Debug.Log($"[S86_BURST] Burst {BurstCount}: exact {exactCount} original Ones; total {TotalSpawned}.", this);
    }

    private void LateUpdate()
    {
        if (!IsPrepared)
        {
            return;
        }

        float deltaTime = Mathf.Min(Time.deltaTime, 0.05f);
        for (int burstIndex = bursts.Count - 1; burstIndex >= 0; burstIndex--)
        {
            BurstState burst = bursts[burstIndex];
            burst.Age += deltaTime;
            if (burst.Age > BurstLifetime)
            {
                bursts.RemoveAt(burstIndex);
                continue;
            }
            Simulate(burst, deltaTime);
            Draw(burst);
        }
    }

    private void Simulate(BurstState burst, float deltaTime)
    {
        float radius = burst.UnitScale * 0.5f;
        for (int i = 0; i < burst.Count; i++)
        {
            Vector3 velocity = burst.Velocities[i];
            velocity += Vector3.down * 16f * deltaTime;
            Vector3 position = burst.Positions[i] + velocity * deltaTime;
            if (position.y - radius < floorY)
            {
                position.y = floorY + radius;
                if (velocity.y < 0f)
                {
                    velocity.y *= -0.48f;
                    velocity.x *= 0.93f;
                    velocity.z *= 0.93f;
                }
            }
            burst.Positions[i] = position;
            burst.Velocities[i] = velocity;
            // Derive rotation from absolute age so repeated quaternion products do
            // not accumulate enough floating-point error to invalidate TRS.
            burst.Rotations[i] = Quaternion.AngleAxis(burst.SpinSpeeds[i] * burst.Age, burst.SpinAxes[i]);
            burst.Matrices[i] = Matrix4x4.TRS(position, burst.Rotations[i], Vector3.one * burst.UnitScale);
        }
    }

    private void Draw(BurstState burst)
    {
        foreach (RenderPart part in renderParts)
        {
            int first = 0;
            while (first < burst.Count)
            {
                int count = Mathf.Min(MaximumInstancesPerDraw, burst.Count - first);
                for (int i = 0; i < count; i++)
                {
                    batchMatrices[i] = burst.Matrices[first + i] * part.LocalMatrix;
                }
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
                first += count;
            }
        }
    }

    private static Bounds BoundsOf(Renderer[] renderers, Vector3 fallback)
    {
        bool found = false;
        Bounds bounds = new Bounds(fallback, Vector3.zero);
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
