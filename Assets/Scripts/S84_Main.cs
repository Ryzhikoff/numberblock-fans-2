using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene 84: silent vertical short. A smug 5000 stamps on a smiling 1000,
/// becomes five surprised Thousands, and finally breaks into 5,000 Ones.
/// </summary>
public class S84_Main : MonoBehaviour
{
    public const float SequenceDuration = 15.2f;
    public const float ShadowStartTime = 1.15f;
    public const float GiantRevealTime = 2.15f;
    public const float StompContactTime = 4.15f;
    public const float CrushBurstTime = 5.05f;
    public const float GiantSurpriseTime = 7.05f;
    public const float SplitTime = 8f;
    public const float StackSurpriseTime = 9.15f;
    public const float FinalBurstTime = 10.05f;
    public const float CrushedHeightScale = 0.22f;

    public const string ThousandOriginalFaceName = "S84 Thousand Original Face";
    public const string ThousandSurprisedFaceName = "S84 Thousand Surprised Face";
    public const string FiveThousandCunningFaceName = "S84 Five Thousand Cunning Face";
    public const string FiveThousandSurprisedFaceName = "S84 Five Thousand Surprised Face";

    [Header("Original and scene-local Numberblocks")]
    public GameObject onePrefab;
    public GameObject thousandPrefab;
    public GameObject fiveThousandPrefab;

    [Header("Staging")]
    public S84_UnitBurst unitBurst;
    public Transform approachingShadow;
    public Material dustMaterial;
    public Material flashMaterial;
    public float characterScale = 0.255f;

    [Header("Existing non-verbal audio")]
    public AudioClip approachClip;
    public AudioClip stompClip;
    public AudioClip burstClip;
    public AudioClip runClip;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public bool CrushStarted { get; private set; }
    public bool FirstBurstTriggered { get; private set; }
    public bool SplitTriggered { get; private set; }
    public bool StackSurprised { get; private set; }
    public bool FinalBurstTriggered { get; private set; }
    public int VisibleThousandCount { get; private set; }

    private readonly List<Actor> thousandStack = new List<Actor>(5);
    private readonly List<Puff> puffs = new List<Puff>(48);
    private readonly List<Flash> flashes = new List<Flash>(4);

    private Actor thousand;
    private Actor fiveThousand;
    private Camera shotCamera;
    private AudioSource effectsSource;
    private AudioSource movementSource;
    private Material shadowMaterialInstance;
    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private float cameraFov;
    private float sequenceStartTime;
    private float shakeStartTime;
    private float shakeDuration;
    private float shakeStrength;
    private bool initialized;
    private bool approachPlayed;
    private bool giantSurprised;
    private bool runSoundPlayed;

    private sealed class Actor
    {
        public Transform Root;
        public GameObject Visual;
        public Transform PrimaryFace;
        public Transform SurprisedFace;
        public Vector3 Size;
        public Vector3 BasePosition;
    }

    private sealed class Puff
    {
        public Transform Transform;
        public Vector3 Start;
        public Vector3 Velocity;
        public float StartTime;
        public float Lifetime;
        public float Size;
    }

    private sealed class Flash
    {
        public Transform Transform;
        public Material RuntimeMaterial;
        public float StartTime;
        public float Duration;
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
            Debug.LogError("[S84] Scene needs a camera tagged MainCamera.", this);
            return;
        }

        shotCamera.aspect = 9f / 16f;
        ConfigureAudio();
        ConfigureShadow();

        thousand = SpawnActor(
            thousandPrefab,
            "Smiling Numberblock 1000",
            ThousandOriginalFaceName,
            ThousandSurprisedFaceName);
        fiveThousand = SpawnActor(
            fiveThousandPrefab,
            "Cunning Numberblock 5000",
            FiveThousandCunningFaceName,
            FiveThousandSurprisedFaceName);
        if (thousand == null || fiveThousand == null)
        {
            return;
        }

        thousand.BasePosition = new Vector3(-1.35f, 0f, 0f);
        thousand.Root.position = thousand.BasePosition;
        fiveThousand.BasePosition = new Vector3(4.8f, 0f, 0.15f);
        fiveThousand.Root.position = fiveThousand.BasePosition;
        fiveThousand.Root.gameObject.SetActive(false);
        SetExpression(thousand, false);
        SetExpression(fiveThousand, false);

        unitBurst.Prepare();
        if (!unitBurst.IsPrepared)
        {
            return;
        }

        sequenceStartTime = Time.time;
        SequenceTime = 0f;
        SequenceComplete = false;
        CrushStarted = false;
        FirstBurstTriggered = false;
        SplitTriggered = false;
        StackSurprised = false;
        FinalBurstTriggered = false;
        VisibleThousandCount = 1;
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
        AnimateThousand(time);
        AnimateShadow(time);
        AnimateFiveThousand(time);
        TriggerTimelineEvents(time);
        AnimateStack(time);
        AnimatePuffs(time);
        AnimateFlashes(time);
        AnimateCamera(time);

        if (time >= SequenceDuration)
        {
            SequenceTime = SequenceDuration;
            SequenceComplete = true;
            Debug.Log(
                "[S84] COMPLETE — silent stomp, exact 1,000-unit escape, five-Thousand split, " +
                "and exact 5,000-unit escape finished.",
                this);
        }
    }

    private bool ValidateReferences()
    {
        List<string> missing = new List<string>();
        if (onePrefab == null) missing.Add(nameof(onePrefab));
        if (thousandPrefab == null) missing.Add(nameof(thousandPrefab));
        if (fiveThousandPrefab == null) missing.Add(nameof(fiveThousandPrefab));
        if (unitBurst == null) missing.Add(nameof(unitBurst));
        if (approachingShadow == null) missing.Add(nameof(approachingShadow));
        if (dustMaterial == null) missing.Add(nameof(dustMaterial));
        if (flashMaterial == null) missing.Add(nameof(flashMaterial));

        if (missing.Count == 0)
        {
            return true;
        }

        Debug.LogError($"[S84] Missing scene references: {string.Join(", ", missing)}.", this);
        return false;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorAsset("Assets/Prefabs/Blocks/1.prefab", onePrefab);
        thousandPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene84/Prefabs/Thousand_Surprise.prefab",
            thousandPrefab);
        fiveThousandPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene84/Prefabs/FiveThousand_Expressions.prefab",
            fiveThousandPrefab);
        dustMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene84/Materials/S84_Dust.mat",
            dustMaterial);
        flashMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene84/Materials/S84_Flash.mat",
            flashMaterial);
        approachClip = LoadEditorAsset("Assets/Sound/zvuk-priblijeniya.mp3", approachClip);
        stompClip = LoadEditorAsset("Assets/Sound/boom_metal.wav", stompClip);
        burstClip = LoadEditorAsset("Assets/Sound/destroy_blocks.mp3", burstClip);
        runClip = LoadEditorAsset("Assets/Sound/runaway.mp3", runClip);
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
        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.volume = 0.88f;

        movementSource = gameObject.AddComponent<AudioSource>();
        movementSource.playOnAwake = false;
        movementSource.spatialBlend = 0f;
        movementSource.volume = 0.48f;
    }

    private void ConfigureShadow()
    {
        Renderer shadowRenderer = approachingShadow.GetComponent<Renderer>();
        if (shadowRenderer != null && shadowRenderer.sharedMaterial != null)
        {
            shadowMaterialInstance = new Material(shadowRenderer.sharedMaterial)
            {
                name = "S84 Runtime Approaching Shadow"
            };
            shadowRenderer.sharedMaterial = shadowMaterialInstance;
        }
        approachingShadow.gameObject.SetActive(false);
    }

    private Actor SpawnActor(
        GameObject prefab,
        string objectName,
        string primaryFaceName,
        string surprisedFaceName)
    {
        GameObject root = new GameObject(objectName);
        root.transform.SetParent(transform, false);

        GameObject visual = Instantiate(prefab, root.transform);
        visual.name = objectName + " Visual";
        PrepareForAnimation(visual);
        visual.transform.localScale = Vector3.one * characterScale;

        Bounds bounds = BoundsOf(visual);
        visual.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        bounds = BoundsOf(visual);

        Actor actor = new Actor
        {
            Root = root.transform,
            Visual = visual,
            PrimaryFace = FindChild(visual.transform, primaryFaceName),
            SurprisedFace = FindChild(visual.transform, surprisedFaceName),
            Size = bounds.size
        };
        if (actor.PrimaryFace == null || actor.SurprisedFace == null)
        {
            Debug.LogError($"[S84] {objectName} is missing its expression overlays.", visual);
            Destroy(root);
            return null;
        }

        return actor;
    }

    private static void PrepareForAnimation(GameObject target)
    {
        foreach (Animator animator in target.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
        }
        foreach (MonoBehaviour behaviour in target.GetComponentsInChildren<MonoBehaviour>(true))
        {
            behaviour.enabled = false;
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

    private void AnimateThousand(float time)
    {
        if (FirstBurstTriggered || thousand == null)
        {
            return;
        }

        if (time < StompContactTime)
        {
            float calm = Mathf.Clamp01((GiantRevealTime - time) / 0.35f);
            float bob = Mathf.Abs(Mathf.Sin(time * 3.4f)) * 0.055f * calm;
            thousand.Root.position = thousand.BasePosition + Vector3.up * bob;
            thousand.Root.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Sin(time * 2.7f) * 1.2f * calm);
            thousand.Root.localScale = Vector3.one;
            return;
        }

        float crush = EaseInOutCubic(InverseLerp(StompContactTime, CrushBurstTime, time));
        float strain = Mathf.Sin((time - StompContactTime) * 26f) * (1f - crush) * 1.35f;
        thousand.Root.position = thousand.BasePosition;
        thousand.Root.rotation = Quaternion.Euler(0f, 0f, strain);
        thousand.Root.localScale = new Vector3(
            Mathf.Lerp(1f, 1.34f, crush),
            Mathf.Lerp(1f, CrushedHeightScale, crush),
            Mathf.Lerp(1f, 1.2f, crush));
    }

    private void AnimateShadow(float time)
    {
        if (time < ShadowStartTime || time >= StompContactTime || approachingShadow == null)
        {
            if (approachingShadow != null && approachingShadow.gameObject.activeSelf)
            {
                approachingShadow.gameObject.SetActive(false);
            }
            return;
        }

        if (!approachingShadow.gameObject.activeSelf)
        {
            approachingShadow.gameObject.SetActive(true);
        }

        float progress = EaseOutCubic(InverseLerp(ShadowStartTime, GiantRevealTime + 0.35f, time));
        approachingShadow.position = new Vector3(
            Mathf.Lerp(1.35f, -0.12f, progress),
            0.035f,
            0.18f);
        approachingShadow.localScale = new Vector3(
            Mathf.Lerp(0.05f, 1.72f, progress),
            0.018f,
            Mathf.Lerp(0.05f, 0.86f, progress));
        SetMaterialAlpha(shadowMaterialInstance, Mathf.Lerp(0f, 0.42f, progress));
    }

    private void AnimateFiveThousand(float time)
    {
        if (fiveThousand == null || SplitTriggered || time < GiantRevealTime)
        {
            return;
        }

        if (!fiveThousand.Root.gameObject.activeSelf)
        {
            fiveThousand.Root.gameObject.SetActive(true);
        }

        Vector3 position;
        float tilt;
        Vector3 scale = Vector3.one;
        if (time < 3.05f)
        {
            float progress = EaseOutCubic(InverseLerp(GiantRevealTime, 3.05f, time));
            position = new Vector3(
                Mathf.Lerp(4.8f, 2.25f, progress),
                Mathf.Sin(progress * Mathf.PI) * 0.42f,
                0.15f);
            tilt = Mathf.Lerp(-7f, 3f, progress);
        }
        else if (time < StompContactTime)
        {
            float progress = EaseInCubic(InverseLerp(3.05f, StompContactTime, time));
            position = new Vector3(
                Mathf.Lerp(2.25f, -1.35f, progress),
                Mathf.Lerp(0f, thousand.Size.y, progress) +
                    Mathf.Sin(progress * Mathf.PI) * 0.88f,
                Mathf.Lerp(0.15f, 0f, progress));
            tilt = Mathf.Lerp(3f, -4f, progress);
        }
        else if (time < CrushBurstTime)
        {
            float progress = EaseInOutCubic(
                InverseLerp(StompContactTime, CrushBurstTime, time));
            float thousandHeight = thousand.Size.y *
                Mathf.Lerp(1f, CrushedHeightScale, progress);
            float contactPulse = Mathf.Exp(
                -Mathf.Pow((time - StompContactTime) / 0.16f, 2f));
            position = new Vector3(-1.35f, thousandHeight, 0f);
            tilt = Mathf.Lerp(-4f, 0f, progress) +
                Mathf.Sin((time - StompContactTime) * 20f) * (1f - progress) * 1.1f;
            scale = new Vector3(
                1f + contactPulse * 0.1f,
                1f - contactPulse * 0.075f,
                1f + contactPulse * 0.04f);
        }
        else
        {
            float settle = EaseOutCubic(
                InverseLerp(CrushBurstTime, GiantSurpriseTime - 0.15f, time));
            float footDrop = EaseOutCubic(
                InverseLerp(CrushBurstTime, CrushBurstTime + 0.24f, time));
            position = new Vector3(
                Mathf.Lerp(-1.35f, 0f, settle),
                Mathf.Lerp(thousand.Size.y * CrushedHeightScale, 0f, footDrop),
                0f);
            float impactElapsed = time - CrushBurstTime;
            float squash = Mathf.Exp(-Mathf.Pow(impactElapsed / 0.16f, 2f));
            scale = new Vector3(1f + squash * 0.2f, 1f - squash * 0.18f, 1f + squash * 0.08f);
            tilt = Mathf.Sin(impactElapsed * 12f) * Mathf.Exp(-impactElapsed * 4f) * 5f;
        }

        fiveThousand.Root.position = position;
        fiveThousand.Root.rotation = Quaternion.Euler(0f, 0f, tilt);
        fiveThousand.Root.localScale = scale;
    }

    private void TriggerTimelineEvents(float time)
    {
        if (!approachPlayed && time >= GiantRevealTime)
        {
            approachPlayed = true;
            PlayMovement(approachClip, 0.72f);
            TriggerShake(1.85f, 0.075f);
        }

        if (!CrushStarted && time >= StompContactTime)
        {
            CrushStarted = true;
            SetExpression(thousand, true);
            PlayEffect(stompClip, 0.82f);
            SpawnPuffs(
                thousand.BasePosition + Vector3.up * 0.12f,
                10,
                0.72f);
            TriggerShake(0.42f, 0.2f);
        }

        if (!FirstBurstTriggered && time >= CrushBurstTime)
        {
            FirstBurstTriggered = true;
            Bounds thousandBounds = BoundsOf(thousand.Visual);
            thousand.Root.gameObject.SetActive(false);
            VisibleThousandCount = 0;
            unitBurst.Begin(
                thousandBounds,
                new Vector3Int(10, 10, 10),
                S84_UnitBurst.ThousandUnitCount,
                -1f);
            PlayEffect(burstClip, 0.68f);
            SpawnPuffs(thousandBounds.center, 18, 1.1f);
            SpawnFlash(thousandBounds.center, 3.4f, 0.3f);
            TriggerShake(0.62f, 0.28f);
        }

        if (!giantSurprised && time >= GiantSurpriseTime)
        {
            giantSurprised = true;
            unitBurst.Hide();
            SetExpression(fiveThousand, true);
            TriggerShake(0.32f, 0.075f);
        }

        if (!SplitTriggered && time >= SplitTime)
        {
            SplitTriggered = true;
            unitBurst.Hide();
            Vector3 stackCenter = new Vector3(0f, 0f, 0f);
            fiveThousand.Root.gameObject.SetActive(false);
            SpawnThousandStack(stackCenter);
            SpawnFlash(new Vector3(0f, fiveThousand.Size.y * 0.5f, -0.25f), 5.2f, 0.42f);
            PlayEffect(burstClip, 0.32f);
            TriggerShake(0.42f, 0.1f);
        }

        if (!StackSurprised && time >= StackSurpriseTime)
        {
            StackSurprised = true;
            foreach (Actor actor in thousandStack)
            {
                SetExpression(actor, true);
            }
            TriggerShake(0.34f, 0.095f);
        }

        if (!FinalBurstTriggered && time >= FinalBurstTime)
        {
            FinalBurstTriggered = true;
            Bounds stackBounds = BoundsOfStack();
            foreach (Actor actor in thousandStack)
            {
                actor.Root.gameObject.SetActive(false);
            }
            VisibleThousandCount = 0;
            unitBurst.Begin(
                stackBounds,
                new Vector3Int(10, 50, 10),
                S84_UnitBurst.FiveThousandUnitCount,
                0f);
            PlayEffect(stompClip, 0.72f);
            PlayEffect(burstClip, 0.88f);
            SpawnPuffs(stackBounds.center, 30, 1.55f);
            SpawnFlash(stackBounds.center, 6.8f, 0.48f);
            TriggerShake(0.82f, 0.34f);
        }

        if (!runSoundPlayed && time >= FinalBurstTime + 0.35f)
        {
            runSoundPlayed = true;
            PlayMovement(runClip, 0.5f);
        }
    }

    private void SpawnThousandStack(Vector3 bottomCenter)
    {
        for (int i = 0; i < 5; i++)
        {
            Actor actor = SpawnActor(
                thousandPrefab,
                $"Split Thousand {i + 1}",
                ThousandOriginalFaceName,
                ThousandSurprisedFaceName);
            if (actor == null)
            {
                continue;
            }

            actor.BasePosition = bottomCenter + Vector3.up * (actor.Size.y * i);
            actor.Root.position = actor.BasePosition;
            actor.Root.localScale = Vector3.zero;
            SetExpression(actor, false);
            thousandStack.Add(actor);
        }
        VisibleThousandCount = thousandStack.Count;
    }

    private void AnimateStack(float time)
    {
        if (!SplitTriggered || FinalBurstTriggered)
        {
            return;
        }

        float revealProgress = InverseLerp(SplitTime, StackSurpriseTime - 0.15f, time);
        for (int i = 0; i < thousandStack.Count; i++)
        {
            Actor actor = thousandStack[i];
            float staggered = Mathf.Clamp01(revealProgress * 1.42f - i * 0.105f);
            float scale = BackOut(staggered);
            float surpriseHop = 0f;
            if (time >= StackSurpriseTime)
            {
                float elapsed = time - StackSurpriseTime - i * 0.025f;
                if (elapsed >= 0f && elapsed <= 0.58f)
                {
                    surpriseHop = Mathf.Sin(elapsed / 0.58f * Mathf.PI) * 0.18f;
                }
            }
            actor.Root.localScale = Vector3.one * scale;
            actor.Root.position = actor.BasePosition + Vector3.up * surpriseHop;
            actor.Root.rotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Sin((time - SplitTime) * 7f + i * 0.7f) * (1f - staggered) * 5f);
        }
    }

    private void AnimateCamera(float time)
    {
        Vector3 closePosition = new Vector3(0f, 2.25f, -16.5f);
        Vector3 widePosition = new Vector3(0f, 6.25f, -25f);
        Vector3 closeTarget = new Vector3(-0.65f, 1.25f, 0f);
        Vector3 wideTarget = new Vector3(0f, 6.1f, 0f);
        float reveal = EaseInOutCubic(InverseLerp(ShadowStartTime + 0.18f, GiantRevealTime + 0.58f, time));
        cameraPosition = Vector3.Lerp(closePosition, widePosition, reveal);
        Vector3 target = Vector3.Lerp(closeTarget, wideTarget, reveal);
        cameraRotation = Quaternion.LookRotation(target - cameraPosition, Vector3.up);
        cameraFov = Mathf.Lerp(33f, 36f, reveal);

        float pressureLift = time < CrushBurstTime
            ? EaseInOutCubic(InverseLerp(3.25f, StompContactTime, time))
            : 1f - EaseInOutCubic(InverseLerp(CrushBurstTime, CrushBurstTime + 0.55f, time));
        Vector3 framingLift = Vector3.up * (pressureLift * 1.55f);
        cameraPosition += framingLift;
        target += framingLift;
        cameraRotation = Quaternion.LookRotation(target - cameraPosition, Vector3.up);

        float elapsed = Time.time - shakeStartTime;
        Vector3 shake = Vector3.zero;
        float roll = 0f;
        if (elapsed >= 0f && elapsed < shakeDuration)
        {
            float fade = 1f - elapsed / shakeDuration;
            float strength = shakeStrength * fade;
            shake = new Vector3(
                (Mathf.PerlinNoise(elapsed * 35f, 2.1f) - 0.5f) * strength,
                (Mathf.PerlinNoise(4.3f, elapsed * 39f) - 0.5f) * strength,
                0f);
            roll = (Mathf.PerlinNoise(elapsed * 31f, 8.6f) - 0.5f) * strength * 4.5f;
        }

        shotCamera.transform.position = cameraPosition + shake;
        shotCamera.transform.rotation = cameraRotation * Quaternion.Euler(0f, 0f, roll);
        shotCamera.fieldOfView = cameraFov;
    }

    private void TriggerShake(float duration, float strength)
    {
        shakeStartTime = Time.time;
        shakeDuration = duration;
        shakeStrength = strength;
    }

    private void SpawnPuffs(Vector3 center, int count, float spread)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject puffObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puffObject.name = "S84 Dust Puff";
            puffObject.transform.SetParent(transform, false);
            Collider collider = puffObject.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            Renderer renderer = puffObject.GetComponent<Renderer>();
            renderer.sharedMaterial = dustMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            float angle = i / (float)Mathf.Max(1, count) * Mathf.PI * 2f;
            float speed = Mathf.Lerp(1.2f, 3.8f, Hash01(i * 13 + count * 7));
            Puff puff = new Puff
            {
                Transform = puffObject.transform,
                Start = center + new Vector3(0f, -center.y + 0.12f, -0.15f),
                Velocity = new Vector3(
                    Mathf.Cos(angle) * speed * spread,
                    Mathf.Lerp(0.7f, 1.8f, Hash01(i * 17 + 5)),
                    Mathf.Sin(angle) * speed * 0.32f),
                StartTime = Time.time,
                Lifetime = Mathf.Lerp(0.52f, 0.92f, Hash01(i * 19 + 9)),
                Size = Mathf.Lerp(0.08f, 0.23f, Hash01(i * 23 + 15)) * spread
            };
            puffs.Add(puff);
        }
    }

    private void AnimatePuffs(float time)
    {
        for (int i = puffs.Count - 1; i >= 0; i--)
        {
            Puff puff = puffs[i];
            float elapsed = Time.time - puff.StartTime;
            if (elapsed >= puff.Lifetime)
            {
                Destroy(puff.Transform.gameObject);
                puffs.RemoveAt(i);
                continue;
            }

            float normalized = elapsed / puff.Lifetime;
            puff.Transform.position = puff.Start + puff.Velocity * elapsed +
                Vector3.down * (1.4f * elapsed * elapsed);
            float scale = puff.Size * Mathf.Sin(normalized * Mathf.PI);
            puff.Transform.localScale = Vector3.one * scale;
        }
    }

    private void SpawnFlash(Vector3 position, float size, float duration)
    {
        GameObject flashObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flashObject.name = "S84 Impact Flash";
        flashObject.transform.SetParent(transform, false);
        flashObject.transform.position = new Vector3(position.x, position.y, -0.42f);
        Collider collider = flashObject.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
        Renderer renderer = flashObject.GetComponent<Renderer>();
        Material runtimeMaterial = new Material(flashMaterial)
        {
            name = "S84 Runtime Flash"
        };
        renderer.sharedMaterial = runtimeMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        flashes.Add(new Flash
        {
            Transform = flashObject.transform,
            RuntimeMaterial = runtimeMaterial,
            StartTime = Time.time,
            Duration = duration,
            Size = size
        });
    }

    private void AnimateFlashes(float time)
    {
        for (int i = flashes.Count - 1; i >= 0; i--)
        {
            Flash flash = flashes[i];
            float elapsed = Time.time - flash.StartTime;
            if (elapsed >= flash.Duration)
            {
                Destroy(flash.RuntimeMaterial);
                Destroy(flash.Transform.gameObject);
                flashes.RemoveAt(i);
                continue;
            }

            float normalized = elapsed / flash.Duration;
            float scale = Mathf.Sin(normalized * Mathf.PI) * flash.Size;
            flash.Transform.localScale = Vector3.one * scale;
            SetMaterialAlpha(flash.RuntimeMaterial, (1f - normalized) * 0.76f);
        }
    }

    private Bounds BoundsOfStack()
    {
        bool hasBounds = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (Actor actor in thousandStack)
        {
            foreach (Renderer renderer in actor.Visual.GetComponentsInChildren<Renderer>(true))
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
        }
        return hasBounds ? bounds : new Bounds(Vector3.up * 6.375f, new Vector3(2.55f, 12.75f, 2.55f));
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

    private static Transform FindChild(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }
        return null;
    }

    private static void SetExpression(Actor actor, bool surprised)
    {
        if (actor == null)
        {
            return;
        }
        actor.PrimaryFace.gameObject.SetActive(!surprised);
        actor.SurprisedFace.gameObject.SetActive(surprised);
    }

    private void PlayEffect(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            effectsSource.PlayOneShot(clip, volume);
        }
    }

    private void PlayMovement(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            movementSource.PlayOneShot(clip, volume);
        }
    }

    private static void SetMaterialAlpha(Material material, float alpha)
    {
        if (material == null)
        {
            return;
        }
        Color color;
        if (material.HasProperty("_BaseColor"))
        {
            color = material.GetColor("_BaseColor");
        }
        else if (material.HasProperty("_Color"))
        {
            color = material.GetColor("_Color");
        }
        else
        {
            return;
        }
        color.a = Mathf.Clamp01(alpha);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private void OnDestroy()
    {
        if (shadowMaterialInstance != null)
        {
            Destroy(shadowMaterialInstance);
        }
    }

    private static float InverseLerp(float from, float to, float value)
    {
        return Mathf.Clamp01((value - from) / Mathf.Max(0.0001f, to - from));
    }

    private static float EaseInCubic(float value)
    {
        return value * value * value;
    }

    private static float EaseOutCubic(float value)
    {
        float inverse = 1f - value;
        return 1f - inverse * inverse * inverse;
    }

    private static float EaseInOutCubic(float value)
    {
        return value < 0.5f
            ? 4f * value * value * value
            : 1f - Mathf.Pow(-2f * value + 2f, 3f) * 0.5f;
    }

    private static float BackOut(float value)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float shifted = value - 1f;
        return 1f + c3 * shifted * shifted * shifted + c1 * shifted * shifted;
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
