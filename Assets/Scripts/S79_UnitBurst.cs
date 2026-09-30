using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Draws the six real faces from the existing One prefab 10,000 times and gives
/// every resulting cube an independent deterministic burst trajectory.
/// </summary>
public class S79_UnitBurst : MonoBehaviour
{
    public const int GridX = 20;
    public const int GridY = 50;
    public const int GridZ = 10;
    public const int TotalUnitCount = GridX * GridY * GridZ;

    public GameObject onePrefab;
    public Collider groundCollider;

    public bool IsPrepared { get; private set; }
    public bool Active { get; private set; }
    public int RenderedUnitCount => Active ? TotalUnitCount : 0;
    public int SettledUnitCount { get; private set; }

    private const int MaximumInstancesPerDraw = 1023;
    private readonly List<RenderPart> renderParts = new List<RenderPart>(6);
    private readonly Matrix4x4[] batchMatrices = new Matrix4x4[MaximumInstancesPerDraw];

    private Vector3[] startPositions;
    private Vector3[] currentPositions;
    private Vector3[] velocities;
    private Vector3[] spinAxes;
    private Quaternion[] rotations;
    private float[] spinSpeeds;
    private float[] scaleVariations;
    private bool[] settled;
    private Matrix4x4[] unitMatrices;
    private float cellSize;
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
            Debug.LogError("[S79_BURST] One prefab is missing.", this);
            return;
        }
        if (!SystemInfo.supportsInstancing)
        {
            Debug.LogError("[S79_BURST] This graphics device does not support GPU instancing.", this);
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
                        name = $"S79 Instanced {materials[submesh].name}",
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
            Debug.LogError("[S79_BURST] The One prefab has no instanced mesh parts.", this);
            return;
        }

        startPositions = new Vector3[TotalUnitCount];
        currentPositions = new Vector3[TotalUnitCount];
        velocities = new Vector3[TotalUnitCount];
        spinAxes = new Vector3[TotalUnitCount];
        rotations = new Quaternion[TotalUnitCount];
        spinSpeeds = new float[TotalUnitCount];
        scaleVariations = new float[TotalUnitCount];
        settled = new bool[TotalUnitCount];
        unitMatrices = new Matrix4x4[TotalUnitCount];
        IsPrepared = true;
    }

    public void Begin(Vector3 center, float unitSize)
    {
        Prepare();
        if (!IsPrepared)
        {
            return;
        }

        cellSize = unitSize;
        int index = 0;
        for (int y = 0; y < GridY; y++)
        {
            for (int z = 0; z < GridZ; z++)
            {
                for (int x = 0; x < GridX; x++)
                {
                    Vector3 offset = new Vector3(
                        (x + 0.5f - GridX * 0.5f) * cellSize,
                        (y + 0.5f - GridY * 0.5f) * cellSize,
                        (z + 0.5f - GridZ * 0.5f) * cellSize);
                    startPositions[index] = center + offset;
                    currentPositions[index] = startPositions[index];

                    Vector3 normalizedOffset = new Vector3(
                        offset.x / (GridX * cellSize * 0.5f),
                        offset.y / (GridY * cellSize * 0.5f),
                        offset.z / (GridZ * cellSize * 0.5f));
                    Vector3 jitter = new Vector3(
                        Hash01(index * 3 + 11) - 0.5f,
                        Hash01(index * 3 + 23) - 0.5f,
                        Hash01(index * 3 + 47) - 0.5f) * 0.42f;
                    Vector3 direction = (normalizedOffset + jitter).normalized;
                    float speed = Mathf.Lerp(4.2f, 11.8f, Hash01(index * 5 + 71));
                    velocities[index] = direction * speed + Vector3.up * 1.1f;

                    Vector3 axis = new Vector3(
                        Hash01(index * 7 + 5) - 0.5f,
                        Hash01(index * 7 + 17) - 0.5f,
                        Hash01(index * 7 + 29) - 0.5f);
                    spinAxes[index] = axis.sqrMagnitude > 0.001f ? axis.normalized : Vector3.up;
                    rotations[index] = Quaternion.identity;
                    spinSpeeds[index] = Mathf.Lerp(105f, 480f, Hash01(index * 11 + 101));
                    scaleVariations[index] = Mathf.Lerp(0.94f, 1.06f, Hash01(index * 13 + 131));
                    settled[index] = false;
                    index++;
                }
            }
        }

        burstStartTime = Time.time;
        SettledUnitCount = 0;
        Active = true;
    }

    private void LateUpdate()
    {
        if (!Active || !IsPrepared)
        {
            return;
        }

        float elapsed = Time.time - burstStartTime;
        float revealPulse = elapsed < 0.22f
            ? 1f + Mathf.Sin(Mathf.Clamp01(elapsed / 0.22f) * Mathf.PI) * 0.08f
            : 1f;

        if (elapsed >= 0.12f)
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
            float scale = cellSize * scaleVariations[i] * revealPulse;
            unitMatrices[i] = Matrix4x4.TRS(
                currentPositions[i],
                rotations[i],
                Vector3.one * scale);
        }

        for (int partIndex = 0; partIndex < renderParts.Count; partIndex++)
        {
            RenderPart part = renderParts[partIndex];
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
        const float gravity = 6.8f;
        const float bounce = 0.24f;
        const float groundFriction = 0.66f;

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

            float halfSize = cellSize * scaleVariations[i] * 0.5f;
            float restingHeight = groundHeight + halfSize;
            if (elapsed > 7.6f)
            {
                position.y = restingHeight;
                settled[i] = true;
                velocity = Vector3.zero;
                spinSpeeds[i] = 0f;
                rotations[i] = SnapToRightAngles(rotations[i]);
                SettledUnitCount++;
                currentPositions[i] = position;
                velocities[i] = velocity;
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
                    spinSpeeds[i] *= 0.7f;
                }

                float horizontalSpeedSquared = velocity.x * velocity.x + velocity.z * velocity.z;
                if ((velocity.y < 0.42f && horizontalSpeedSquared < 0.42f) || elapsed > 7.2f)
                {
                    settled[i] = true;
                    velocity = Vector3.zero;
                    spinSpeeds[i] = 0f;
                    rotations[i] = SnapToRightAngles(rotations[i]);
                    SettledUnitCount++;
                }
            }

            currentPositions[i] = position;
            velocities[i] = velocity;
        }
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
        for (int i = 0; i < renderParts.Count; i++)
        {
            if (renderParts[i].Material != null)
            {
                Destroy(renderParts[i].Material);
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
