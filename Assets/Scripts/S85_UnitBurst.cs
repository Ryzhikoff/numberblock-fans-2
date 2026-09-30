using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Original-sized Ones: exact through 5,000, bounded sampling above it, no per-unit objects.</summary>
[DefaultExecutionOrder(100)]
public class S85_UnitBurst : MonoBehaviour
{
    public const int MaximumVisibleUnits = 5000;
    private const int BatchSize = 1023;
    public GameObject OnePrefab;
    public bool IsPrepared { get; private set; }
    public bool Active { get; private set; }
    public bool AllowEscapeCompletion { get; set; }
    public int LastSourceNumber { get; private set; }
    public int LastVisibleCount { get; private set; }
    public int EscapedUnitCount { get; private set; }
    public int ActiveUnitCount => Active ? LastVisibleCount - EscapedUnitCount : 0;
    public bool AllEscaped => Active && EscapedUnitCount == LastVisibleCount;

    private readonly List<RenderPart> parts = new List<RenderPart>();
    private readonly Vector3[] positions = new Vector3[MaximumVisibleUnits];
    private readonly Vector3[] velocities = new Vector3[MaximumVisibleUnits];
    private readonly float[] sides = new float[MaximumVisibleUnits];
    private readonly float[] speeds = new float[MaximumVisibleUnits];
    private readonly float[] phases = new float[MaximumVisibleUnits];
    private readonly bool[] landed = new bool[MaximumVisibleUnits];
    private readonly bool[] escaped = new bool[MaximumVisibleUnits];
    private readonly Matrix4x4[] matrices = new Matrix4x4[MaximumVisibleUnits];
    private readonly Matrix4x4[] batch = new Matrix4x4[BatchSize];
    private Camera escapeCamera;
    private Vector3 oneSize;
    private float elapsed;
    private float runSpeedScale;
    private float frontZ;
    private CommandBuffer drawBuffer;

    private sealed class RenderPart
    {
        public Mesh Mesh;
        public int Submesh;
        public Material Material;
        public Matrix4x4 LocalMatrix;
    }

    public void Prepare()
    {
        if (IsPrepared) return;
        if (OnePrefab == null || !SystemInfo.supportsInstancing)
        {
            Debug.LogError("[S85_BURST] Original One and GPU instancing are required.", this);
            return;
        }
        GameObject template = Instantiate(OnePrefab, Vector3.zero, Quaternion.identity);
        foreach (MonoBehaviour behaviour in template.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled = false;
        Bounds bounds = S85_Main.BoundsOf(template);
        oneSize = bounds.size;
        foreach (MeshRenderer renderer in template.GetComponentsInChildren<MeshRenderer>(true))
        {
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) continue;
            Material[] materials = renderer.sharedMaterials;
            for (int submesh = 0; submesh < Mathf.Min(filter.sharedMesh.subMeshCount, materials.Length); submesh++)
            {
                if (materials[submesh] == null) continue;
                Material material = new Material(materials[submesh])
                {
                    name = "S85 Instanced " + materials[submesh].name,
                    enableInstancing = true
                };
                if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0f);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0f);
                if (material.HasProperty("_SpecularHighlights"))
                {
                    material.SetFloat("_SpecularHighlights", 0f);
                    material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                }
                if (material.HasProperty("_GlossyReflections"))
                {
                    material.SetFloat("_GlossyReflections", 0f);
                    material.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
                }
                parts.Add(new RenderPart
                {
                    Mesh = filter.sharedMesh,
                    Submesh = submesh,
                    Material = material,
                    // Retain the complete original One geometry and scale; only translate its pivot.
                    LocalMatrix = Matrix4x4.Translate(-bounds.center) * renderer.transform.localToWorldMatrix
                });
            }
        }
        template.SetActive(false);
        Destroy(template);
        IsPrepared = parts.Count > 0;
    }

    public void Begin(Bounds source, Vector3Int grid, int number, Camera camera, float winnerFrontZ)
    {
        Prepare();
        if (!IsPrepared) return;
        if ((long)grid.x * grid.y * grid.z != number)
        {
            Debug.LogError($"[S85_BURST] Unit grid does not match {number}.", this);
            return;
        }
        LastSourceNumber = number;
        LastVisibleCount = Mathf.Min(number, MaximumVisibleUnits);
        EscapedUnitCount = 0;
        AllowEscapeCompletion = false;
        escapeCamera = camera;
        frontZ = winnerFrontZ;
        if (drawBuffer == null)
        {
            drawBuffer = new CommandBuffer { name = "S85 Original One Crowd" };
            escapeCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, drawBuffer);
        }
        elapsed = 0f;
        runSpeedScale = Mathf.Clamp(source.size.x / 16f, 1f, 3.5f);
        for (int i = 0; i < LastVisibleCount; i++)
        {
            // Stratify over the full true grid instead of creating tens of thousands of hidden units.
            int cell = number <= MaximumVisibleUnits ? i : Mathf.Min(number - 1,
                (int)((i + Hash01(i * 43 + 23)) * ((double)number / LastVisibleCount)));
            int x = cell % grid.x;
            int z = cell / grid.x % grid.z;
            int y = cell / (grid.x * grid.z);
            positions[i] = source.min + new Vector3(
                (x + 0.5f) * source.size.x / grid.x,
                (y + 0.5f) * source.size.y / grid.y,
                (z + 0.5f) * source.size.z / grid.z);
            float side = Hash01(i * 17 + 3) < 0.5f ? -1f : 1f;
            sides[i] = side;
            float depth = (Hash01(i * 29 + 7) - 0.5f) * 3f;
            velocities[i] = new Vector3(side * Mathf.Lerp(3f, 6f, Hash01(i * 19 + 5)),
                Mathf.Lerp(3.8f, 7f, Hash01(i * 23 + 11)), Mathf.Min(depth, (frontZ - positions[i].z) / 1.8f));
            speeds[i] = Mathf.Lerp(4.5f, 8f, Hash01(i * 31 + 13));
            phases[i] = Hash01(i * 37 + 17) * Mathf.PI * 2f;
            landed[i] = false;
            escaped[i] = false;
        }
        Active = true;
        Debug.Log($"[S85_BURST] {number} => {LastVisibleCount} original-sized Ones; " +
            $"{(number <= MaximumVisibleUnits ? "exact" : "sampled")}, no unit GameObjects/Rigidbodies.");
    }

    public void Hide()
    {
        Active = false;
        AllowEscapeCompletion = false;
        drawBuffer?.Clear();
    }

    private void LateUpdate()
    {
        if (!Active || !IsPrepared) return;
        float remaining = Mathf.Min(Time.deltaTime, 0.5f);
        while (remaining > 0f)
        {
            float step = Mathf.Min(remaining, 0.025f);
            elapsed += step;
            Simulate(step);
            remaining -= step;
        }
        int count = 0;
        for (int i = 0; i < LastVisibleCount; i++)
        {
            if (escaped[i]) continue;
            Quaternion rotation = landed[i]
                ? Quaternion.Euler(0f, sides[i] * -12f, Mathf.Sin(elapsed * 13f + phases[i]) * 12f)
                : Quaternion.Euler(0f, 0f, elapsed * sides[i] * 120f);
            matrices[count++] = Matrix4x4.TRS(positions[i], rotation, Vector3.one);
        }
        drawBuffer.Clear();
        foreach (RenderPart part in parts)
        {
            for (int first = 0; first < count; first += BatchSize)
            {
                int batchCount = Mathf.Min(BatchSize, count - first);
                for (int j = 0; j < batchCount; j++) batch[j] = matrices[first + j] * part.LocalMatrix;
                drawBuffer.DrawMeshInstanced(part.Mesh, part.Submesh, part.Material, 0, batch, batchCount);
            }
        }
    }

    private void Simulate(float delta)
    {
        float floor = oneSize.y * 0.5f;
        Vector3 cameraPosition = escapeCamera != null ? escapeCamera.transform.position : Vector3.zero;
        Vector3 cameraForward = escapeCamera != null ? escapeCamera.transform.forward : Vector3.forward;
        float widthPerDepth = escapeCamera != null
            ? 2f * Mathf.Tan(escapeCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * escapeCamera.aspect : 0f;
        for (int i = 0; i < LastVisibleCount; i++)
        {
            if (escaped[i]) continue;
            Vector3 velocity = velocities[i];
            if (landed[i])
            {
                float runSpeed = speeds[i] * runSpeedScale;
                float acceleration = 16f;
                if (AllowEscapeCompletion && escapeCamera != null)
                {
                    // Keep a readable run across the frame as the rising camera widens its view.
                    float frameWidth = Mathf.Max(1f, Vector3.Dot(positions[i] - cameraPosition, cameraForward) * widthPerDepth);
                    float frameSpeed = Mathf.Lerp(0.3f, 0.4f, (speeds[i] - 4.5f) / 3.5f);
                    runSpeed = Mathf.Max(runSpeed, frameWidth * frameSpeed);
                    acceleration = Mathf.Max(acceleration, frameWidth * 0.65f);
                }
                velocity.x = Mathf.MoveTowards(velocity.x, sides[i] * runSpeed, delta * acceleration);
                // Move toward the front of the winner, then continue toward either screen edge.
                velocity.z = Mathf.MoveTowards(velocity.z, -1.7f * runSpeedScale, delta * 4f);
            }
            else if (positions[i].z <= frontZ)
            {
                velocity.z = Mathf.MoveTowards(velocity.z, -1.7f * runSpeedScale, delta * 24f);
            }
            velocity.y -= 12.5f * delta;
            Vector3 position = positions[i] + velocity * delta;
            if (position.y < floor)
            {
                position.y = floor;
                landed[i] = true;
                velocity.y = 1.6f + Hash01(i * 41 + 19) * 1.4f;
            }
            positions[i] = position;
            velocities[i] = velocity;
            if (AllowEscapeCompletion && landed[i] && escapeCamera != null)
            {
                Vector3 viewport = escapeCamera.WorldToViewportPoint(position);
                // Include the entire spinning cube, with a conservative radius and extra border.
                Vector3 rightExtent = escapeCamera.WorldToViewportPoint(position + escapeCamera.transform.right * oneSize.magnitude * 0.5f);
                float radius = Mathf.Abs(rightExtent.x - viewport.x);
                bool outside = sides[i] < 0f ? viewport.x < -radius - 0.035f : viewport.x > 1f + radius + 0.035f;
                if (outside || viewport.z < -oneSize.magnitude)
                {
                    escaped[i] = true;
                    EscapedUnitCount++;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (drawBuffer != null)
        {
            if (escapeCamera != null) escapeCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, drawBuffer);
            drawBuffer.Release();
        }
        foreach (RenderPart part in parts) if (part.Material != null) Destroy(part.Material);
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
