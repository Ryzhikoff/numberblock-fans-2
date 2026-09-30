using UnityEngine;

/// <summary>Adds a transparent face decal without changing the block's original materials.</summary>
public class S74_FaceOverlay : MonoBehaviour
{
    public Texture2D faceTexture;
    [Range(0.1f, 1f)] public float sizeOnFront = 0.82f;
    [Range(0f, 1f)] public float faceCenterY = 0.5f;
    public float frontOffset = 0.01f;

    private GameObject overlay;

    private void OnEnable()
    {
        if (faceTexture == null || overlay != null)
        {
            return;
        }

        Renderer[] bodyRenderers = GetComponentsInChildren<Renderer>();
        if (bodyRenderers.Length == 0)
        {
            return;
        }

        Bounds bounds = bodyRenderers[0].bounds;
        for (int i = 1; i < bodyRenderers.Length; i++)
        {
            bounds.Encapsulate(bodyRenderers[i].bounds);
        }

        overlay = GameObject.CreatePrimitive(PrimitiveType.Quad);
        overlay.name = "Face Overlay";
        Destroy(overlay.GetComponent<Collider>());
        overlay.transform.SetParent(transform, false);

        Vector3 worldPosition = bounds.center;
        worldPosition.y = bounds.min.y + bounds.size.y * faceCenterY;
        worldPosition.z = bounds.min.z - frontOffset;
        overlay.transform.localPosition = transform.InverseTransformPoint(worldPosition);
        overlay.transform.localRotation = Quaternion.identity;

        float faceSize = Mathf.Min(bounds.size.x, bounds.size.y) * sizeOnFront;
        Vector3 lossyScale = transform.lossyScale;
        overlay.transform.localScale = new Vector3(
            faceSize / Mathf.Max(0.0001f, Mathf.Abs(lossyScale.x)),
            faceSize / Mathf.Max(0.0001f, Mathf.Abs(lossyScale.y)),
            1f);

        Shader shader = Shader.Find("Unlit/Transparent");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material faceMaterial = new Material(shader)
        {
            name = faceTexture.name + " (Instance)",
            mainTexture = faceTexture
        };
        MeshRenderer renderer = overlay.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = faceMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = 10;
    }
}
