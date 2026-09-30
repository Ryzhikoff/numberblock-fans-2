using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene 78: seven happy Ones stroll together until Thousand drops from the sky,
/// blasts them out of frame, and changes to a smug transparent face overlay.
/// </summary>
public class S78_Main : MonoBehaviour
{
    public const float SequenceDuration = 10f;
    public const float DropStartTime = 3f;
    public const float ImpactTime = 3.95f;
    public const float SmirkTime = 4.35f;

    public const string OriginalFaceName = "S78 Original Face Overlay";
    public const string SmirkFaceName = "S78 Smirk Face Overlay";

    [Header("Original Numberblocks")]
    public GameObject onePrefab;
    public GameObject thousandPrefab;

    [Header("Scene staging")]
    [Range(3, 10)] public int oneCount = 7;
    public float thousandScale = 0.55f;
    public float dropHeight = 14f;
    public Vector3 impactPoint = new Vector3(0.25f, 0f, 0.55f);
    public GameObject impactShadow;
    public Material dustMaterial;

    [Header("Existing project audio")]
    public AudioClip happyWalkClip;
    public AudioClip impactClip;
    public AudioClip impactAccentClip;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public bool ImpactTriggered { get; private set; }
    public bool SmirkVisible { get; private set; }

    private readonly List<GameObject> ones = new List<GameObject>(10);
    private readonly List<Vector3> oneStarts = new List<Vector3>(10);
    private readonly List<Vector3> scatterStarts = new List<Vector3>(10);
    private readonly List<Vector3> scatterVelocities = new List<Vector3>(10);
    private readonly List<DustPuff> dustPuffs = new List<DustPuff>(14);

    private Camera shotCamera;
    private AudioSource musicSource;
    private AudioSource effectsSource;
    private GameObject thousand;
    private Transform originalFace;
    private Transform smirkFace;
    private Vector3 thousandBaseScale;
    private Vector3 thousandLandingPosition;
    private Vector3 thousandLandingBottomCenter;
    private Vector3 thousandFallStartPosition;
    private Vector3 cameraStartPosition;
    private Quaternion cameraStartRotation;
    private Vector3 cameraClosePosition;
    private Quaternion cameraCloseRotation;
    private float sequenceStartTime;
    private bool initialized;

    private sealed class DustPuff
    {
        public Transform Transform;
        public Vector3 Start;
        public Vector3 Velocity;
        public float Size;
    }

    private void Start()
    {
        ResolveEditorReferences();
        if (!ValidateReferences())
        {
            return;
        }

        shotCamera = Camera.main;
        if (shotCamera == null)
        {
            Debug.LogError("[S78] Scene needs a camera tagged MainCamera.", this);
            return;
        }
        shotCamera.aspect = 9f / 16f;

        ConfigureAudio();
        SpawnCast();
        if (originalFace == null || smirkFace == null)
        {
            return;
        }
        ConfigureCameraShots();

        sequenceStartTime = Time.time;
        SequenceTime = 0f;
        SequenceComplete = false;
        ImpactTriggered = false;
        SmirkVisible = false;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || SequenceComplete)
        {
            return;
        }

        float time = Mathf.Min(SequenceDuration, Time.time - sequenceStartTime);
        SequenceTime = time;

        AnimateShadow(time);
        AnimateThousand(time);

        if (!ImpactTriggered && time >= ImpactTime)
        {
            TriggerImpact();
        }

        if (ImpactTriggered)
        {
            AnimateScatteredOnes(time - ImpactTime);
            AnimateDust(time - ImpactTime);
        }
        else
        {
            AnimateHappyWalk(time);
        }

        if (!SmirkVisible && time >= SmirkTime)
        {
            ShowSmirk();
        }

        AnimateCamera(time);

        if (time >= SequenceDuration)
        {
            SequenceTime = SequenceDuration;
            SequenceComplete = true;
            Debug.Log("[S78] COMPLETE — the ten-second Thousand landing short reached 10.000 seconds.", this);
        }
    }

    private bool ValidateReferences()
    {
        List<string> missing = new List<string>();
        if (onePrefab == null)
        {
            missing.Add(nameof(onePrefab));
        }
        if (thousandPrefab == null)
        {
            missing.Add(nameof(thousandPrefab));
        }
        if (dustMaterial == null)
        {
            missing.Add(nameof(dustMaterial));
        }
        if (impactShadow == null)
        {
            missing.Add(nameof(impactShadow));
        }

        if (missing.Count == 0)
        {
            return true;
        }

        Debug.LogError($"[S78] Missing scene references: {string.Join(", ", missing)}.", this);
        return false;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorAsset("Assets/Prefabs/Blocks/1.prefab", onePrefab);
        thousandPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene78/Thousand_Smirk.prefab",
            thousandPrefab);
        dustMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene78/Materials/S78_Dust.mat",
            dustMaterial);
        happyWalkClip = LoadEditorAsset(
            "Assets/Sound/March of the Hares - Nathan Moore (1).mp3",
            happyWalkClip);
        impactClip = LoadEditorAsset(
            "Assets/Sound/topSound/jg-032316-sfx-distant-meteor-crash-impact-2.mp3",
            impactClip);
        impactAccentClip = LoadEditorAsset("Assets/Sound/collCube.wav", impactAccentClip);
#endif
    }

#if UNITY_EDITOR
    private static T LoadEditorAsset<T>(string path, T current) where T : Object
    {
        T expected = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        return expected != null ? expected : current;
    }
#endif

    private void ConfigureAudio()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = 0.24f;
        musicSource.clip = happyWalkClip;

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.volume = 0.9f;

        if (happyWalkClip != null)
        {
            musicSource.Play();
        }
    }

    private void SpawnCast()
    {
        int count = Mathf.Clamp(oneCount, 3, 10);
        float spacing = 0.88f;
        float firstX = -spacing * (count - 1) * 0.5f - 0.35f;

        for (int i = 0; i < count; i++)
        {
            float z = ((i % 3) - 1) * 0.28f - 0.15f;
            Vector3 position = new Vector3(firstX + i * spacing, 0f, z);
            GameObject one = SpawnGrounded(onePrefab, position, $"Happy One {i + 1}", 1f);
            ones.Add(one);
            oneStarts.Add(one.transform.position);
        }

        thousand = SpawnGrounded(
            thousandPrefab,
            impactPoint,
            "One Thousand — Falling",
            thousandScale);
        Bounds centeredBounds = BoundsOf(thousand);
        thousand.transform.position += new Vector3(
            impactPoint.x - centeredBounds.center.x,
            0f,
            impactPoint.z - centeredBounds.center.z);
        thousandBaseScale = thousand.transform.localScale;
        thousandLandingPosition = thousand.transform.position;
        thousandLandingBottomCenter = BottomCenter(BoundsOf(thousand));
        thousandFallStartPosition = thousandLandingPosition + Vector3.up * dropHeight;
        thousand.transform.position = thousandFallStartPosition;

        originalFace = FindChild(thousand.transform, OriginalFaceName);
        smirkFace = FindChild(thousand.transform, SmirkFaceName);
        if (originalFace == null || smirkFace == null)
        {
            Debug.LogError("[S78] Thousand is missing its exact-position face overlays.", thousand);
            return;
        }

        originalFace.gameObject.SetActive(true);
        smirkFace.gameObject.SetActive(false);
        impactShadow.SetActive(false);
    }

    private static GameObject SpawnGrounded(
        GameObject prefab,
        Vector3 position,
        string objectName,
        float uniformScale)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.name = objectName;
        instance.transform.localScale = Vector3.one * uniformScale;
        PrepareForAnimation(instance);

        Bounds bounds = BoundsOf(instance);
        instance.transform.position += Vector3.up * (position.y - bounds.min.y);
        return instance;
    }

    private static void PrepareForAnimation(GameObject target)
    {
        foreach (Animator animator in target.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
        }

        foreach (Rigidbody body in target.GetComponentsInChildren<Rigidbody>(true))
        {
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        foreach (Collider collider in target.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
    }

    private void ConfigureCameraShots()
    {
        cameraStartPosition = shotCamera.transform.position;
        cameraStartRotation = shotCamera.transform.rotation;
        cameraClosePosition = new Vector3(0.25f, 3.35f, -14.2f);
        cameraCloseRotation = Quaternion.LookRotation(new Vector3(0.25f, 2.8f, 0.45f) - cameraClosePosition);
    }

    private void AnimateHappyWalk(float time)
    {
        float walkTime = Mathf.Min(time, DropStartTime);
        float travel = Mathf.SmoothStep(0f, 1f, walkTime / DropStartTime);

        for (int i = 0; i < ones.Count; i++)
        {
            float phase = i * 0.72f;
            float step = Mathf.Sin(walkTime * 9.2f + phase);
            Vector3 position = oneStarts[i] + Vector3.right * travel;
            position.y += Mathf.Abs(step) * 0.17f;
            ones[i].transform.position = position;
            ones[i].transform.rotation = Quaternion.Euler(0f, 0f, step * 6.5f);
        }
    }

    private void AnimateShadow(float time)
    {
        const float shadowStart = 2.75f;
        if (time < shadowStart)
        {
            impactShadow.SetActive(false);
            return;
        }

        float amount;
        if (time < ImpactTime)
        {
            amount = Mathf.SmoothStep(0f, 1f, (time - shadowStart) / (ImpactTime - shadowStart));
        }
        else
        {
            amount = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((time - ImpactTime) / 0.45f));
        }

        impactShadow.SetActive(amount > 0.01f);
        impactShadow.transform.localScale = new Vector3(5.4f, 0.018f, 3.7f) * amount;
    }

    private void AnimateThousand(float time)
    {
        if (time < ImpactTime)
        {
            float fall = Mathf.Clamp01((time - DropStartTime) / (ImpactTime - DropStartTime));
            float accelerated = fall * fall;
            thousand.transform.localScale = thousandBaseScale;
            thousand.transform.rotation = Quaternion.identity;
            thousand.transform.position = Vector3.Lerp(
                thousandFallStartPosition,
                thousandLandingPosition,
                accelerated);
            return;
        }

        float sinceImpact = time - ImpactTime;
        Vector3 factor;
        float angle;
        if (sinceImpact < 0.12f)
        {
            float t = Mathf.SmoothStep(0f, 1f, sinceImpact / 0.12f);
            factor = Vector3.Lerp(Vector3.one, new Vector3(1.13f, 0.72f, 1.13f), t);
            angle = Mathf.Lerp(0f, -2.5f, t);
        }
        else if (sinceImpact < 0.32f)
        {
            float t = Mathf.SmoothStep(0f, 1f, (sinceImpact - 0.12f) / 0.2f);
            factor = Vector3.Lerp(new Vector3(1.13f, 0.72f, 1.13f), new Vector3(0.97f, 1.07f, 0.97f), t);
            angle = Mathf.Lerp(-2.5f, 1.4f, t);
        }
        else if (sinceImpact < 0.68f)
        {
            float t = Mathf.SmoothStep(0f, 1f, (sinceImpact - 0.32f) / 0.36f);
            factor = Vector3.Lerp(new Vector3(0.97f, 1.07f, 0.97f), Vector3.one, t);
            angle = Mathf.Lerp(1.4f, 0f, t);
        }
        else
        {
            factor = Vector3.one;
            angle = SmirkVisible ? Mathf.Sin((sinceImpact - 0.68f) * 1.7f) * 0.45f : 0f;
        }

        thousand.transform.localScale = Vector3.Scale(thousandBaseScale, factor);
        thousand.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        AnchorThousandToLanding();

        if (sinceImpact > 0.68f)
        {
            thousand.transform.position += Vector3.up * Mathf.Abs(Mathf.Sin((sinceImpact - 0.68f) * 1.7f)) * 0.025f;
        }
    }

    private void AnchorThousandToLanding()
    {
        Vector3 currentBottomCenter = BottomCenter(BoundsOf(thousand));
        thousand.transform.position += thousandLandingBottomCenter - currentBottomCenter;
    }

    private void TriggerImpact()
    {
        ImpactTriggered = true;
        thousand.transform.position = thousandLandingPosition;
        musicSource.Stop();
        Play(impactClip, 1f);
        Play(impactAccentClip, 0.8f);

        scatterStarts.Clear();
        scatterVelocities.Clear();
        float burstFrontZ = BoundsOf(thousand).min.z - 0.18f;
        for (int i = 0; i < ones.Count; i++)
        {
            Vector3 burstStart = ones[i].transform.position;
            burstStart.z = burstFrontZ;
            scatterStarts.Add(burstStart);
            float normalized = ones.Count == 1 ? 0f : i / (float)(ones.Count - 1);
            float horizontal = Mathf.Lerp(-5.4f, 5.4f, normalized);
            if (Mathf.Abs(horizontal) < 1.2f)
            {
                horizontal = i % 2 == 0 ? -1.8f : 1.8f;
            }
            float vertical = 6.8f + (i % 4) * 0.75f;
            float depth = ((i * 2) % 5 - 2) * 0.72f;
            scatterVelocities.Add(new Vector3(horizontal, vertical, depth));
        }

        CreateDustPuffs();
    }

    private void AnimateScatteredOnes(float time)
    {
        const float gravity = 10.8f;
        for (int i = 0; i < ones.Count; i++)
        {
            GameObject one = ones[i];
            if (!one.activeSelf)
            {
                continue;
            }

            Vector3 position = scatterStarts[i] + scatterVelocities[i] * time;
            position.y -= 0.5f * gravity * time * time;
            one.transform.position = position;
            float spinDirection = i % 2 == 0 ? -1f : 1f;
            one.transform.rotation = Quaternion.Euler(
                time * (110f + i * 9f),
                time * (55f + i * 7f),
                spinDirection * time * (210f + i * 13f));

            if (time > 1.1f && (position.y < -2.2f || Mathf.Abs(position.x) > 9f))
            {
                one.SetActive(false);
            }
        }
    }

    private void ShowSmirk()
    {
        SmirkVisible = true;
        originalFace.gameObject.SetActive(false);
        smirkFace.gameObject.SetActive(true);
    }

    private void CreateDustPuffs()
    {
        Vector3 dustOrigin = thousandLandingBottomCenter + Vector3.up * 0.12f;
        dustOrigin.z = BoundsOf(thousand).min.z - 0.12f;
        for (int i = 0; i < 14; i++)
        {
            float angle = i * Mathf.PI * 2f / 14f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = $"Impact Dust {i + 1}";
            Collider collider = puff.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            puff.GetComponent<Renderer>().sharedMaterial = dustMaterial;
            puff.transform.position = dustOrigin;
            puff.transform.localScale = Vector3.one * 0.05f;

            dustPuffs.Add(new DustPuff
            {
                Transform = puff.transform,
                Start = puff.transform.position,
                Velocity = direction * (2.5f + (i % 4) * 0.45f) + Vector3.up * (0.7f + (i % 3) * 0.25f),
                Size = 0.3f + (i % 5) * 0.055f
            });
        }
    }

    private void AnimateDust(float time)
    {
        float life = 0.95f;
        for (int i = 0; i < dustPuffs.Count; i++)
        {
            DustPuff puff = dustPuffs[i];
            if (time >= life)
            {
                puff.Transform.gameObject.SetActive(false);
                continue;
            }

            float normalized = Mathf.Clamp01(time / life);
            Vector3 position = puff.Start + puff.Velocity * time;
            position.y -= time * time * 0.8f;
            puff.Transform.position = position;
            float size = puff.Size * Mathf.Sin(normalized * Mathf.PI);
            puff.Transform.localScale = new Vector3(size * 1.25f, size, size);
        }
    }

    private void AnimateCamera(float time)
    {
        const float pushStart = 6.1f;
        const float pushEnd = 8.15f;
        float push = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(pushStart, pushEnd, time));
        Vector3 basePosition = Vector3.Lerp(cameraStartPosition, cameraClosePosition, push);
        Quaternion baseRotation = Quaternion.Slerp(cameraStartRotation, cameraCloseRotation, push);

        float sinceImpact = time - ImpactTime;
        if (sinceImpact >= 0f && sinceImpact < 0.48f)
        {
            float strength = (1f - sinceImpact / 0.48f) * 0.19f;
            basePosition += new Vector3(
                Mathf.Sin(sinceImpact * 94f) * strength,
                Mathf.Cos(sinceImpact * 77f) * strength,
                0f);
            baseRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(sinceImpact * 88f) * strength * 5f) * baseRotation;
        }

        shotCamera.transform.SetPositionAndRotation(basePosition, baseRotation);
    }

    private void Play(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            effectsSource.PlayOneShot(clip, volume);
        }
    }

    private static Transform FindChild(Transform root, string objectName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child;
            }
        }
        return null;
    }

    private static Vector3 BottomCenter(Bounds bounds)
    {
        return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
    }

    private static Bounds BoundsOf(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(target.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }
}
