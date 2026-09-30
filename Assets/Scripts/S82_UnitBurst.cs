using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Draws the exact 50 + 100 unit composition after the collision, scatters the
/// original One geometry, then attracts all 150 units into a 15 x 10 result.
/// </summary>
public class S82_UnitBurst : MonoBehaviour
{
    public const int FiftyColumns = 5;
    public const int HundredColumns = 10;
    public const int RowCount = 10;
    public const int FiftyUnitCount = FiftyColumns * RowCount;
    public const int HundredUnitCount = HundredColumns * RowCount;
    public const int TotalUnitCount = FiftyUnitCount + HundredUnitCount;
    public const int ResultColumns = FiftyColumns + HundredColumns;

    public const float ScatterDuration = 1.65f;
    public const float FormationDuration = 3.15f;

    public GameObject onePrefab;
    public Collider groundCollider;

    public bool IsPrepared { get; private set; }
    public bool Started { get; private set; }
    public bool Active { get; private set; }
    public bool FormationComplete { get; private set; }
    public int RenderedUnitCount => Active ? TotalUnitCount : 0;

    private const int MaximumInstancesPerDraw = 1023;
    private readonly List<RenderPart> renderParts = new List<RenderPart>(6);
    private readonly Matrix4x4[] batchMatrices = new Matrix4x4[MaximumInstancesPerDraw];

    private Vector3[] positions;
    private Vector3[] velocities;
    private Vector3[] gatherStarts;
    private Vector3[] targets;
    private Vector3[] spinAxes;
    private Quaternion[] rotations;
    private float[] spinSpeeds;
    private float[] scaleVariations;
    private Matrix4x4[] unitMatrices;
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
            Debug.LogError("[S82_BURST] Original One prefab is missing.", this);
            return;
        }
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogError("[S82_BURST] GPU instancing is not supported.", this);
            return;
        }

        GameObject template = Instantiate(onePrefab, Vector3.zero, Quaternion.identity);
        template.name = "S82 One Render Template";
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
                        name = $"S82 Instanced {materials[submesh].name}",
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
            Debug.LogError("[S82_BURST] Original One prefab has no renderable mesh parts.", this);
            return;
        }

        positions = new Vector3[TotalUnitCount];
        velocities = new Vector3[TotalUnitCount];
        gatherStarts = new Vector3[TotalUnitCount];
        targets = new Vector3[TotalUnitCount];
        spinAxes = new Vector3[TotalUnitCount];
        rotations = new Quaternion[TotalUnitCount];
        spinSpeeds = new float[TotalUnitCount];
        scaleVariations = new float[TotalUnitCount];
        unitMatrices = new Matrix4x4[TotalUnitCount];
        IsPrepared = true;
    }

    public void Begin(Bounds fiftyBounds, Bounds hundredBounds, Bounds resultBounds)
    {
        Prepare();
        if (!IsPrepared)
        {
            return;
        }

        unitScale = Mathf.Min(
            resultBounds.size.x / ResultColumns,
            Mathf.Min(resultBounds.size.y / RowCount, resultBounds.size.z));
        FillInitialGrid(0, fiftyBounds, FiftyColumns, -1f);
        FillInitialGrid(FiftyUnitCount, hundredBounds, HundredColumns, 1f);
        FillTargetGrid(resultBounds);

        burstStartTime = Time.time;
        gatherCaptured = false;
        FormationComplete = false;
        Started = true;
        Active = true;
    }

    public void Hide()
    {
        Active = false;
    }

    private void FillInitialGrid(int firstIndex, Bounds bounds, int columns, float side)
    {
        float cellWidth = bounds.size.x / columns;
        float cellHeight = bounds.size.y / RowCount;
        int localIndex = 0;
        for (int y = 0; y < RowCount; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                int index = firstIndex + localIndex;
                positions[index] = new Vector3(
                    bounds.min.x + (x + 0.5f) * cellWidth,
                    bounds.min.y + (y + 0.5f) * cellHeight,
                    bounds.center.z);

                Vector3 jitter = new Vector3(
                    Hash01(index * 5 + 11) - 0.5f,
                    Hash01(index * 5 + 23) * 0.7f,
                    Hash01(index * 5 + 47) - 0.5f);
                Vector3 direction = new Vector3(side * 1.35f, 0.9f, 0f) + jitter;
                float speed = Mathf.Lerp(3.8f, 7.8f, Hash01(index * 7 + 71));
                velocities[index] = direction.normalized * speed;

                Vector3 axis = new Vector3(
                    Hash01(index * 11 + 3) - 0.5f,
                    Hash01(index * 11 + 17) - 0.5f,
                    Hash01(index * 11 + 41) - 0.5f);
                spinAxes[index] = axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.up;
                rotations[index] = Quaternion.identity;
                spinSpeeds[index] = Mathf.Lerp(120f, 420f, Hash01(index * 13 + 97));
                scaleVariations[index] = Mathf.Lerp(0.92f, 1.06f, Hash01(index * 17 + 131));
                localIndex++;
            }
        }
    }

    private void FillTargetGrid(Bounds bounds)
    {
        float cellWidth = bounds.size.x / ResultColumns;
        float cellHeight = bounds.size.y / RowCount;
        int index = 0;
        for (int y = 0; y < RowCount; y++)
        {
            for (int x = 0; x < ResultColumns; x++)
            {
                targets[index] = new Vector3(
                    bounds.min.x + (x + 0.5f) * cellWidth,
                    bounds.min.y + (y + 0.5f) * cellHeight,
                    bounds.center.z);
                index++;
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
        float gatherProgress = 0f;
        if (elapsed < ScatterDuration)
        {
            SimulateScatter(Mathf.Min(Time.deltaTime, 0.08f));
        }
        else
        {
            if (!gatherCaptured)
            {
                for (int i = 0; i < TotalUnitCount; i++)
                {
                    gatherStarts[i] = positions[i];
                }
                gatherCaptured = true;
            }

            gatherProgress = Mathf.Clamp01((elapsed - ScatterDuration) / FormationDuration);
            float eased = EaseInOutCubic(gatherProgress);
            for (int i = 0; i < TotalUnitCount; i++)
            {
                float phase = Hash01(i * 19 + 151) * Mathf.PI * 2f;
                float arc = Mathf.Sin(gatherProgress * Mathf.PI) * (0.35f + Hash01(i * 23 + 181) * 0.8f);
                Vector3 swirl = new Vector3(
                    Mathf.Cos(phase + gatherProgress * Mathf.PI * 3f),
                    Mathf.Sin(phase + gatherProgress * Mathf.PI * 2f) * 0.65f,
                    Mathf.Sin(phase) * 0.5f) * arc;
                positions[i] = Vector3.Lerp(gatherStarts[i], targets[i], eased) + swirl;
                rotations[i] = Quaternion.Slerp(rotations[i], Quaternion.identity, eased);
            }

            if (gatherProgress >= 1f)
            {
                FormationComplete = true;
            }
        }

        float uniformity = EaseInOutCubic(gatherProgress);
        for (int i = 0; i < TotalUnitCount; i++)
        {
            float scale = unitScale * Mathf.Lerp(scaleVariations[i], 1f, uniformity);
            unitMatrices[i] = Matrix4x4.TRS(positions[i], rotations[i], Vector3.one * scale);
        }

        foreach (RenderPart part in renderParts)
        {
            int count = TotalUnitCount;
            for (int i = 0; i < count; i++)
            {
                batchMatrices[i] = unitMatrices[i] * part.LocalMatrix;
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

    private void SimulateScatter(float deltaTime)
    {
        float groundHeight = groundCollider != null && groundCollider.enabled
            ? groundCollider.bounds.max.y
            : 0f;
        float remaining = deltaTime;
        while (remaining > 0f)
        {
            float step = Mathf.Min(0.025f, remaining);
            for (int i = 0; i < TotalUnitCount; i++)
            {
                Vector3 velocity = velocities[i];
                velocity.y -= 7.6f * step;
                Vector3 position = positions[i] + velocity * step;
                rotations[i] = Quaternion.AngleAxis(spinSpeeds[i] * step, spinAxes[i]) * rotations[i];

                float restingHeight = groundHeight + unitScale * 0.5f;
                if (position.y < restingHeight)
                {
                    position.y = restingHeight;
                    velocity.y = Mathf.Abs(velocity.y) * 0.34f;
                    velocity.x *= 0.82f;
                    velocity.z *= 0.82f;
                }

                positions[i] = position;
                velocities[i] = velocity;
            }
            remaining -= step;
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

    private static float EaseInOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value < 0.5f
            ? 4f * value * value * value
            : 1f - Mathf.Pow(-2f * value + 2f, 3f) * 0.5f;
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
