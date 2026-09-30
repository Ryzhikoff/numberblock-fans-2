using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Scene 74: every Numberblock discovers an even taller Numberblock.</summary>
public class S74_Main : MonoBehaviour
{
    [Serializable]
    public class CharacterSet
    {
        public string label;
        public string displayText;
        public AudioClip voiceClip;
        public GameObject surprisedPrefab;
        public GameObject lookingUpRightPrefab;
        public GameObject scaredPrefab;
    }

    [Header("Strictly increasing-height chain")]
    public CharacterSet[] characters;

    [Header("Timing per meeting")]
    public float introFadeDuration = 2f;
    public float walkDuration = 0.8f;
    public float anticipationDuration = 0.3f;
    public float fallDuration = 0.55f;
    public float reactionDuration = 0.45f;
    public float cameraRevealDuration = 1.4f;
    public float cameraFocusDuration = 2.2f;
    public float newBlockHoldDuration = 1.5f;
    public float finalHoldDuration = 1.8f;

    [Header("Optional audio")]
    public AudioClip landingClip;

    [Header("Number label appearance")]
    public TMP_FontAsset numberFont;
    public float numberFontSize = 82f;
    public FontStyles numberFontStyle = FontStyles.Bold;
    public Color numberColor = Color.white;
    public Color numberOutlineColor = Color.black;
    [Range(0f, 1f)] public float numberOutlineWidth = 0.25f;
    public Vector2 numberPosition = new Vector2(0f, -70f);
    public float numberLabelHeight = 130f;

    private Camera shotCamera;
    private AudioSource audioSource;
    private GameObject current;
    private GameObject newcomer;
    private TextMeshProUGUI numberLabel;
    private Image introOverlay;

    private IEnumerator Start()
    {
        if (characters == null || characters.Length < 2)
        {
            Debug.LogError("Scene 74 needs at least two character sets.", this);
            yield break;
        }

        shotCamera = Camera.main;
        audioSource = gameObject.AddComponent<AudioSource>();
        CreateNumberLabel();
        current = SpawnGrounded(characters[0].surprisedPrefab, Vector3.zero, characters[0].label);
        FrameNow(BoundsOf(current), 1.35f);
        yield return FadeFromBlack();
        Announce(characters[0]);
        yield return new WaitForSeconds(newBlockHoldDuration);

        for (int index = 0; index < characters.Length - 1; index++)
        {
            yield return WalkCurrentToOrigin();
            SwapCurrent(characters[index].lookingUpRightPrefab, characters[index].label + " (Looking Up Right)");
            yield return new WaitForSeconds(anticipationDuration);
            yield return DropNewcomer(characters[index + 1]);
            SwapCurrent(characters[index].scaredPrefab, characters[index].label + " (Scared)");
            yield return FrightenedReaction();
            yield return RevealBoth();
            yield return FocusNewcomer();
            Announce(characters[index + 1]);
            yield return new WaitForSeconds(newBlockHoldDuration);
            PromoteNewcomer();
        }

        yield return FinalBounce();
    }

    private GameObject SpawnGrounded(GameObject prefab, Vector3 position, string objectName)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
        instance.name = objectName;
        Bounds bounds = BoundsOf(instance);
        instance.transform.position += Vector3.up * (position.y - bounds.min.y);
        return instance;
    }

    private IEnumerator WalkCurrentToOrigin()
    {
        Bounds bounds = BoundsOf(current);
        Vector3 start = current.transform.position;
        Vector3 end = start;
        float elapsed = 0f;

        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / walkDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            Vector3 position = Vector3.Lerp(start, end, eased);
            position.y += Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4f)) * Mathf.Max(0.08f, bounds.size.y * 0.045f);
            current.transform.position = position;
            current.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * 3f);
            FrameNow(BoundsOf(current), 1.35f);
            yield return null;
        }

        current.transform.position = end;
        current.transform.rotation = Quaternion.identity;
    }

    private IEnumerator DropNewcomer(CharacterSet next)
    {
        Bounds currentBounds = BoundsOf(current);
        newcomer = SpawnGrounded(next.surprisedPrefab, Vector3.zero, next.label);
        Bounds initialBounds = BoundsOf(newcomer);
        float gap = Mathf.Max(3f, currentBounds.size.x * 0.08f);
        newcomer.transform.position += Vector3.right * (currentBounds.max.x + gap - initialBounds.min.x);
        Vector3 groundPosition = newcomer.transform.position;
        Bounds newcomerBounds = BoundsOf(newcomer);
        Vector3 startPosition = groundPosition + Vector3.up * (newcomerBounds.size.y + Mathf.Max(5f, currentBounds.size.y));
        newcomer.transform.position = startPosition;

        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);
            newcomer.transform.position = Vector3.Lerp(startPosition, groundPosition, t * t);
            yield return null;
        }

        newcomer.transform.position = groundPosition;
        if (landingClip != null)
        {
            audioSource.PlayOneShot(landingClip);
        }
        yield return CameraShake(0.18f, Mathf.Min(0.45f, newcomerBounds.size.y * 0.02f));
    }

    private void SwapCurrent(GameObject prefab, string objectName)
    {
        Vector3 position = current.transform.position;
        Quaternion rotation = current.transform.rotation;
        Destroy(current);
        current = Instantiate(prefab, position, rotation, transform);
        current.name = objectName;
    }

    private IEnumerator FrightenedReaction()
    {
        Bounds bounds = BoundsOf(current);
        Vector3 start = shotCamera.transform.position;
        Vector3 close = Frame(bounds, 1.15f);
        float elapsed = 0f;

        while (elapsed < reactionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / reactionDuration));
            shotCamera.transform.position = Vector3.Lerp(start, close, t);
            shotCamera.transform.LookAt(bounds.center);
            current.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(elapsed * 38f) * 1.5f * (1f - t));
            yield return null;
        }
        current.transform.rotation = Quaternion.identity;
    }

    private IEnumerator RevealBoth()
    {
        Bounds combined = BoundsOf(current);
        combined.Encapsulate(BoundsOf(newcomer));
        Vector3 fromPosition = shotCamera.transform.position;
        Quaternion fromRotation = shotCamera.transform.rotation;
        Vector3 toPosition = Frame(combined, 1.18f);
        Quaternion toRotation = Quaternion.LookRotation(combined.center - toPosition);
        float elapsed = 0f;

        while (elapsed < cameraRevealDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / cameraRevealDuration));
            shotCamera.transform.position = Vector3.Lerp(fromPosition, toPosition, t);
            shotCamera.transform.rotation = Quaternion.Slerp(fromRotation, toRotation, t);
            yield return null;
        }
    }

    private IEnumerator FocusNewcomer()
    {
        Bounds bounds = BoundsOf(newcomer);
        Vector3 fromPosition = shotCamera.transform.position;
        Quaternion fromRotation = shotCamera.transform.rotation;
        Vector3 toPosition = Frame(bounds, 1.35f);
        Quaternion toRotation = Quaternion.LookRotation(bounds.center - toPosition);
        float elapsed = 0f;

        while (elapsed < cameraFocusDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / cameraFocusDuration));
            shotCamera.transform.position = Vector3.Lerp(fromPosition, toPosition, t);
            shotCamera.transform.rotation = Quaternion.Slerp(fromRotation, toRotation, t);
            yield return null;
        }

        shotCamera.transform.position = toPosition;
        shotCamera.transform.rotation = toRotation;
    }

    private void PromoteNewcomer()
    {
        current = newcomer;
        newcomer = null;
    }

    private void CreateNumberLabel()
    {
        GameObject canvasObject = new GameObject("Number Label UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject labelObject = new GameObject("Number", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(canvasObject.transform, false);
        numberLabel = labelObject.GetComponent<TextMeshProUGUI>();
        if (numberFont != null)
        {
            numberLabel.font = numberFont;
        }
        numberLabel.alignment = TextAlignmentOptions.Center;
        numberLabel.fontSize = numberFontSize;
        numberLabel.fontStyle = numberFontStyle;
        numberLabel.color = numberColor;
        numberLabel.outlineColor = numberOutlineColor;
        numberLabel.outlineWidth = numberOutlineWidth;

        RectTransform rect = numberLabel.rectTransform;
        rect.anchorMin = new Vector2(0.1f, 1f);
        rect.anchorMax = new Vector2(0.9f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = numberPosition;
        rect.sizeDelta = new Vector2(0f, numberLabelHeight);
        numberLabel.text = string.Empty;

        GameObject overlayObject = new GameObject("Intro Fade", typeof(RectTransform), typeof(Image));
        overlayObject.transform.SetParent(canvasObject.transform, false);
        introOverlay = overlayObject.GetComponent<Image>();
        introOverlay.color = Color.black;
        introOverlay.raycastTarget = false;

        RectTransform overlayRect = introOverlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
    }

    private IEnumerator FadeFromBlack()
    {
        float duration = Mathf.Max(0f, introFadeDuration);
        if (duration <= 0f)
        {
            introOverlay.gameObject.SetActive(false);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            introOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        introOverlay.gameObject.SetActive(false);
    }

    private void Announce(CharacterSet character)
    {
        numberLabel.text = character.displayText;
        if (character.voiceClip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(character.voiceClip);
        }
    }

    private IEnumerator FinalBounce()
    {
        Vector3 basePosition = current.transform.position;
        float elapsed = 0f;
        while (elapsed < finalHoldDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / finalHoldDuration;
            current.transform.position = basePosition + Vector3.up * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)) * Mathf.Max(0.12f, BoundsOf(current).size.y * 0.012f);
            yield return null;
        }
        current.transform.position = basePosition;
    }

    private IEnumerator CameraShake(float duration, float strength)
    {
        Vector3 origin = shotCamera.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            shotCamera.transform.position = origin + (Vector3)UnityEngine.Random.insideUnitCircle * strength;
            yield return null;
        }
        shotCamera.transform.position = origin;
    }

    private Vector3 Frame(Bounds bounds, float padding)
    {
        float verticalHalf = bounds.extents.y * padding;
        float horizontalHalf = bounds.extents.x * padding / Mathf.Max(0.01f, shotCamera.aspect);
        float requiredHalf = Mathf.Max(verticalHalf, horizontalHalf);
        float distance = requiredHalf / Mathf.Tan(shotCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        return bounds.center + Vector3.back * (distance + bounds.extents.z + 1f);
    }

    private void FrameNow(Bounds bounds, float padding)
    {
        shotCamera.transform.position = Frame(bounds, padding);
        shotCamera.transform.LookAt(bounds.center);
    }

    private static Bounds BoundsOf(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }
}
