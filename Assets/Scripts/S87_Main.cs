using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Scene 87: Numberblocks 50 and 100 hop down stairs, trip together, burst into
/// exactly 150 Ones and rebuild as Numberblock 150. There is no dialogue.
/// </summary>
public class S87_Main : MonoBehaviour
{
    public const float SequenceDuration = 18.2f;
    public const float HopStartTime = 0.45f;
    public const float WarningTime = 5.45f;
    public const float TripTime = 5.8f;
    public const float BurstTime = 7.9f;
    public const float ResultRevealTime = 12.65f;
    public const float FinalJumpTime = 14.15f;
    public const float FinalLandingTime = 15.25f;

    public const string FiftyOriginalFaceName = "S87 50 Original Face";
    public const string FiftyStartledFaceName = "S87 50 Startled Face";
    public const string HundredOriginalFaceName = "S87 100 Original Face";
    public const string HundredStartledFaceName = "S87 100 Startled Face";

    [Header("Original Numberblocks and scene-local variants")]
    public GameObject onePrefab;
    public GameObject fiftyPrefab;
    public GameObject hundredPrefab;
    public GameObject oneFiftyPrefab;

    [Header("Staging")]
    public float characterScale = 0.45f;
    public Transform brokenStep;
    public Material finalFaceMaterial;
    public Material dustMaterial;
    public Material flashMaterial;
    public S87_UnitBurst unitBurst;

    [Header("Existing project audio")]
    public AudioClip hopClip;
    public AudioClip crackClip;
    public AudioClip burstClip;
    public AudioClip successClip;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public int CompletedHops { get; private set; }
    public bool TripTriggered { get; private set; }
    public bool BurstTriggered { get; private set; }
    public bool ResultRevealed { get; private set; }
    public bool FinalLandingTriggered { get; private set; }

    private static readonly Vector3[] LandingPoints =
    {
        new Vector3(0f, 7.15f, 7.45f),
        new Vector3(0f, 5.95f, 5.05f),
        new Vector3(0f, 4.75f, 2.65f),
        new Vector3(0f, 3.55f, 0.25f),
        new Vector3(0f, 2.35f, -2.15f)
    };

    private const float HopDuration = 1.15f;
    private static readonly Vector3 ImpactPoint = new Vector3(0f, 0.05f, -2.15f);
    private readonly List<Particle> particles = new List<Particle>(30);
    private Actor fiftyActor;
    private Actor hundredActor;
    private Transform resultRoot;
    private Bounds resultBodyBounds;
    private Camera shotCamera;
    private AudioSource effectsSource;
    private float sequenceStartTime;
    private int lastLandingIndex;
    private bool warningTriggered;
    private float shakeStrength;
    private float shakeUntil;
    private Transform brokenStepLeft;
    private Transform brokenStepRight;
    private Vector3 brokenStepLeftStartPosition;
    private Vector3 brokenStepRightStartPosition;
    private Quaternion brokenStepLeftStartRotation;
    private Quaternion brokenStepRightStartRotation;
    private bool initialized;

    private sealed class Actor
    {
        public Transform Root;
        public GameObject Visual;
        public Transform OriginalFace;
        public Transform StartledFace;
        public float X;
    }

    private sealed class Particle
    {
        public Transform Transform;
        public Vector3 Start;
        public Vector3 Velocity;
        public float StartTime;
        public float Lifetime;
        public float StartScale;
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
            Debug.LogError("[S87] Scene needs a MainCamera.", this);
            return;
        }
        shotCamera.aspect = 9f / 16f;
        foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer layer in
                 shotCamera.GetComponents<UnityEngine.Rendering.PostProcessing.PostProcessLayer>())
        {
            layer.enabled = false;
        }
        shotCamera.allowHDR = false;
        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;

        fiftyActor = SpawnActor(
            "Numberblock 50 — Stair Partner",
            fiftyPrefab,
            FiftyOriginalFaceName,
            FiftyStartledFaceName,
            -2.05f);
        hundredActor = SpawnActor(
            "Numberblock 100 — Stair Partner",
            hundredPrefab,
            HundredOriginalFaceName,
            HundredStartledFaceName,
            1.35f);
        if (fiftyActor == null || hundredActor == null)
        {
            return;
        }

        ApplySharedPose(LandingPoints[0], 0f, 0f, Vector3.one);
        BuildResultCharacter();
        unitBurst.Prepare();
        if (!unitBurst.IsPrepared)
        {
            return;
        }

        if (brokenStep != null)
        {
            brokenStepLeft = brokenStep.Find("Breaking Half Left");
            brokenStepRight = brokenStep.Find("Breaking Half Right");
            if (brokenStepLeft == null || brokenStepRight == null)
            {
                CreateRuntimeBreakingHalves();
            }
            if (brokenStepLeft == null || brokenStepRight == null)
            {
                Debug.LogError("[S87] The final step could not be split into two halves.", brokenStep);
                return;
            }
            brokenStepLeftStartPosition = brokenStepLeft.localPosition;
            brokenStepRightStartPosition = brokenStepRight.localPosition;
            brokenStepLeftStartRotation = brokenStepLeft.localRotation;
            brokenStepRightStartRotation = brokenStepRight.localRotation;
        }
        sequenceStartTime = Time.time;
        initialized = true;
    }

    private void CreateRuntimeBreakingHalves()
    {
        Renderer sourceRenderer = brokenStep.GetComponent<Renderer>();
        if (sourceRenderer == null)
        {
            return;
        }

        Transform source = brokenStep;
        Transform parent = source.parent;
        Vector3 sourcePosition = source.position;
        Quaternion sourceRotation = source.rotation;
        Vector3 sourceScale = source.lossyScale;
        Material sourceMaterial = sourceRenderer.sharedMaterial;
        int sourceLayer = source.gameObject.layer;

        GameObject root = new GameObject("Runtime Breakable Final Step");
        root.transform.SetParent(parent, true);
        root.transform.position = sourcePosition;
        root.transform.rotation = sourceRotation;
        root.transform.localScale = Vector3.one;

        brokenStepLeft = CreateBreakingHalf(
            "Breaking Half Left",
            root.transform,
            Vector3.left * (sourceScale.x * 0.2525f),
            new Vector3(sourceScale.x * 0.495f, sourceScale.y, sourceScale.z),
            sourceMaterial,
            sourceLayer);
        brokenStepRight = CreateBreakingHalf(
            "Breaking Half Right",
            root.transform,
            Vector3.right * (sourceScale.x * 0.2525f),
            new Vector3(sourceScale.x * 0.495f, sourceScale.y, sourceScale.z),
            sourceMaterial,
            sourceLayer);
        source.gameObject.SetActive(false);
        brokenStep = root.transform;
        Debug.Log("[S87] Converted the legacy solid final step into two runtime breaking halves.", this);
    }

    private static Transform CreateBreakingHalf(
        string objectName,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Material material,
        int layer)
    {
        GameObject half = GameObject.CreatePrimitive(PrimitiveType.Cube);
        half.name = objectName;
        half.layer = layer;
        half.transform.SetParent(parent, false);
        half.transform.localPosition = localPosition;
        half.transform.localRotation = Quaternion.identity;
        half.transform.localScale = localScale;
        Renderer renderer = half.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        foreach (Collider collider in half.GetComponents<Collider>())
        {
            Destroy(collider);
        }
        return half.transform;
    }

    private void Update()
    {
        if (!initialized || SequenceComplete)
        {
            return;
        }

        float time = Mathf.Min(SequenceDuration, Time.time - sequenceStartTime);
        SequenceTime = time;
        AnimateCharacters(time);
        AnimateBrokenStep(time);
        AnimateResult(time);
        TriggerEvents(time);
        AnimateParticles(time);
        AnimateCamera(time);

        if (time >= SequenceDuration)
        {
            SequenceComplete = true;
            SequenceTime = SequenceDuration;
            Debug.Log(
                "[S87] COMPLETE — 50 and 100 descended, tripped, became 150 Ones, and rebuilt as 150.",
                this);
        }
    }

    private bool ValidateReferences()
    {
        bool valid = onePrefab != null && fiftyPrefab != null && hundredPrefab != null &&
            oneFiftyPrefab != null &&
            finalFaceMaterial != null && dustMaterial != null && flashMaterial != null &&
            unitBurst != null && brokenStep != null;
        if (!valid)
        {
            Debug.LogError("[S87] Scene references are incomplete.", this);
        }
        return valid;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorAsset("Assets/Prefabs/Blocks/1.prefab", onePrefab);
        fiftyPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene87/Prefabs/Fifty_Stairs.prefab",
            fiftyPrefab);
        hundredPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene87/Prefabs/Hundred_Stairs.prefab",
            hundredPrefab);
        oneFiftyPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene87/Prefabs/OneFifty_Stairs.prefab",
            oneFiftyPrefab);
        finalFaceMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene87/Materials/OneFifty_Delighted_Transparent.mat",
            finalFaceMaterial);
        hopClip = LoadEditorAsset("Assets/Sound/zvuk-priblijeniya.mp3", hopClip);
        crackClip = LoadEditorAsset("Assets/Sound/collCube.wav", crackClip);
        burstClip = LoadEditorAsset("Assets/Sound/destroy_blocks.mp3", burstClip);
        successClip = LoadEditorAsset(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav",
            successClip);
#endif
    }

#if UNITY_EDITOR
    private static T LoadEditorAsset<T>(string path, T current) where T : Object
    {
        T expected = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        return expected != null ? expected : current;
    }
#endif

    private Actor SpawnActor(
        string objectName,
        GameObject prefab,
        string originalFaceName,
        string startledFaceName,
        float x)
    {
        GameObject rootObject = new GameObject(objectName);
        rootObject.transform.SetParent(transform, false);
        GameObject visual = Instantiate(prefab, rootObject.transform);
        visual.name = objectName + " Visual";
        PrepareForAnimation(visual);
        visual.transform.localScale = Vector3.one * characterScale;
        Bounds bounds = BoundsOf(visual);
        visual.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        Actor actor = new Actor
        {
            Root = rootObject.transform,
            Visual = visual,
            OriginalFace = FindChild(visual.transform, originalFaceName),
            StartledFace = FindChild(visual.transform, startledFaceName),
            X = x
        };
        if (actor.OriginalFace == null || actor.StartledFace == null)
        {
            Debug.LogError($"[S87] {objectName} is missing its face layers.", visual);
            Destroy(rootObject);
            return null;
        }
        SetStartled(actor, false);
        return actor;
    }

    private void BuildResultCharacter()
    {
        GameObject result = Instantiate(oneFiftyPrefab, transform);
        result.name = "Numberblock 150 — Rebuilt from 50 + 100";
        PrepareForAnimation(result);
        resultRoot = result.transform;
        resultRoot.position = ImpactPoint;
        resultRoot.rotation = Quaternion.identity;
        resultRoot.localScale = Vector3.one;
        resultBodyBounds = BoundsOf(result);
        resultRoot.gameObject.SetActive(false);
    }

    private void AnimateCharacters(float time)
    {
        if (BurstTriggered)
        {
            return;
        }

        if (time < HopStartTime)
        {
            float settle = Mathf.Sin(time / HopStartTime * Mathf.PI) * 0.08f;
            ApplySharedPose(LandingPoints[0] + Vector3.up * settle, 0f, 0f, Vector3.one);
            return;
        }

        float hopTimeline = time - HopStartTime;
        int hopIndex = Mathf.FloorToInt(hopTimeline / HopDuration);
        if (hopIndex < LandingPoints.Length - 1 && time < WarningTime)
        {
            float progress = Mathf.Clamp01((hopTimeline - hopIndex * HopDuration) / HopDuration);
            Vector3 point = Vector3.Lerp(
                LandingPoints[hopIndex],
                LandingPoints[hopIndex + 1],
                EaseInOutCubic(progress));
            point.y += Mathf.Sin(progress * Mathf.PI) * 0.9f;
            float tilt = Mathf.Sin(progress * Mathf.PI) * 5f;
            float squash = Mathf.Exp(-Mathf.Pow((progress - 1f) / 0.08f, 2f));
            Vector3 scale = new Vector3(1f + squash * 0.07f, 1f - squash * 0.1f, 1f);
            ApplySharedPose(point, tilt, -tilt, scale);
            return;
        }

        if (time < TripTime)
        {
            float wobble = Mathf.Sin((time - WarningTime) * 24f) * 2.4f;
            ApplySharedPose(LandingPoints[LandingPoints.Length - 1], wobble, -wobble, Vector3.one);
            return;
        }

        float fall = Mathf.Clamp01((time - TripTime) / (BurstTime - TripTime));
        Vector3 fallingPoint = Vector3.Lerp(
            LandingPoints[LandingPoints.Length - 1],
            ImpactPoint,
            EaseInCubic(fall));
        float fiftyTilt = Mathf.Lerp(4f, 9f, fall);
        float hundredTilt = Mathf.Lerp(-4f, -9f, fall);
        ApplySharedPose(fallingPoint, fiftyTilt, hundredTilt, Vector3.one);
    }

    private void ApplySharedPose(Vector3 sharedPoint, float fiftyTilt, float hundredTilt, Vector3 scale)
    {
        ApplyActorPose(fiftyActor, sharedPoint, fiftyTilt, scale);
        ApplyActorPose(hundredActor, sharedPoint, hundredTilt, scale);
    }

    private static void ApplyActorPose(Actor actor, Vector3 sharedPoint, float tilt, Vector3 scale)
    {
        actor.Root.position = sharedPoint + Vector3.right * actor.X;
        actor.Root.rotation = Quaternion.Euler(0f, 0f, tilt);
        actor.Root.localScale = scale;
    }

    private void AnimateBrokenStep(float time)
    {
        if (brokenStepLeft == null || brokenStepRight == null)
        {
            return;
        }
        if (time < WarningTime)
        {
            brokenStepLeft.gameObject.SetActive(true);
            brokenStepRight.gameObject.SetActive(true);
            brokenStepLeft.localPosition = brokenStepLeftStartPosition;
            brokenStepRight.localPosition = brokenStepRightStartPosition;
            brokenStepLeft.localRotation = brokenStepLeftStartRotation;
            brokenStepRight.localRotation = brokenStepRightStartRotation;
            return;
        }

        float progress = Mathf.Clamp01((time - WarningTime) / 0.7f);
        float fall = progress * progress;
        brokenStepLeft.localPosition = brokenStepLeftStartPosition +
            new Vector3(-1.8f * progress, -4.8f * fall, 0.15f * progress);
        brokenStepRight.localPosition = brokenStepRightStartPosition +
            new Vector3(1.8f * progress, -4.8f * fall, 0.15f * progress);
        brokenStepLeft.localRotation = brokenStepLeftStartRotation *
            Quaternion.Euler(0f, 0f, 24f * progress);
        brokenStepRight.localRotation = brokenStepRightStartRotation *
            Quaternion.Euler(0f, 0f, -24f * progress);
        if (progress >= 1f)
        {
            brokenStepLeft.gameObject.SetActive(false);
            brokenStepRight.gameObject.SetActive(false);
        }
    }

    private void AnimateResult(float time)
    {
        if (!ResultRevealed || resultRoot == null)
        {
            return;
        }

        float reveal = Mathf.Clamp01((time - ResultRevealTime) / 0.7f);
        float pop = reveal < 0.72f
            ? Mathf.Lerp(0.04f, 1.08f, EaseOutBack(reveal / 0.72f))
            : Mathf.Lerp(1.08f, 1f, (reveal - 0.72f) / 0.28f);
        float jumpHeight = 0f;
        if (time >= FinalJumpTime && time <= FinalLandingTime)
        {
            float jump = Mathf.InverseLerp(FinalJumpTime, FinalLandingTime, time);
            jumpHeight = Mathf.Sin(jump * Mathf.PI) * 0.95f;
        }
        float squash = Mathf.Exp(-Mathf.Pow((time - FinalLandingTime) / 0.11f, 2f));
        resultRoot.position = ImpactPoint + Vector3.up * jumpHeight;
        resultRoot.localScale = new Vector3(
            pop * (1f + squash * 0.11f),
            pop * (1f - squash * 0.14f),
            pop);
    }

    private void TriggerEvents(float time)
    {
        int landingIndex = time < HopStartTime
            ? 0
            : Mathf.Clamp(Mathf.FloorToInt((time - HopStartTime) / HopDuration), 0, 4);
        if (landingIndex > lastLandingIndex && time < WarningTime)
        {
            lastLandingIndex = landingIndex;
            CompletedHops = landingIndex;
            PlayEffect(hopClip, 0.92f + landingIndex * 0.035f, 0.32f);
        }

        if (!warningTriggered && time >= WarningTime)
        {
            warningTriggered = true;
            SetStartled(fiftyActor, true);
            SetStartled(hundredActor, true);
            PlayEffect(crackClip, 0.8f, 0.78f);
            Shake(0.12f, 0.45f);
            CreateParticles(LandingPoints[4] + Vector3.down * 0.05f, 10, 0.12f);
        }

        if (!TripTriggered && time >= TripTime)
        {
            TripTriggered = true;
            PlayEffect(hopClip, 1.25f, 0.5f);
        }

        if (!BurstTriggered && time >= BurstTime)
        {
            TriggerBurst();
        }

        if (!ResultRevealed && time >= ResultRevealTime && unitBurst.FormationComplete)
        {
            ResultRevealed = true;
            unitBurst.Hide();
            resultRoot.gameObject.SetActive(true);
            resultRoot.position = ImpactPoint;
            resultRoot.localScale = Vector3.one * 0.04f;
            PlayEffect(successClip, 1.02f, 0.85f);
            Shake(0.08f, 0.34f);
            CreateParticles(ImpactPoint, 14, 0.16f);
        }

        if (!FinalLandingTriggered && time >= FinalLandingTime && ResultRevealed)
        {
            FinalLandingTriggered = true;
            PlayEffect(crackClip, 0.92f, 0.55f);
            Shake(0.11f, 0.38f);
            CreateParticles(ImpactPoint, 18, 0.2f);
        }
    }

    private void TriggerBurst()
    {
        ApplySharedPose(ImpactPoint, 9f, -9f, Vector3.one);
        Bounds fiftyBounds = BoundsOf(fiftyActor.Visual);
        Bounds hundredBounds = BoundsOf(hundredActor.Visual);
        PlayEffect(burstClip, 0.96f, 0.95f);
        Shake(0.22f, 0.65f);
        CreateParticles(ImpactPoint, 24, 0.23f);
        fiftyActor.Visual.SetActive(false);
        hundredActor.Visual.SetActive(false);
        unitBurst.Begin(fiftyBounds, hundredBounds, resultBodyBounds);
        BurstTriggered = unitBurst.Active;
        if (!BurstTriggered)
        {
            Debug.LogError("[S87] The exact 150-One burst could not start.", this);
        }
    }

    private void CreateParticles(Vector3 center, int count, float size)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = $"S87 Dust {particles.Count + 1:00}";
            sphere.transform.SetParent(transform, false);
            foreach (Collider collider in sphere.GetComponents<Collider>())
            {
                Destroy(collider);
            }
            Renderer renderer = sphere.GetComponent<Renderer>();
            renderer.sharedMaterial = i % 4 == 0 ? flashMaterial : dustMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            float angle = Mathf.Lerp(15f, 165f, count <= 1 ? 0f : i / (float)(count - 1));
            float radians = angle * Mathf.Deg2Rad;
            float speed = Mathf.Lerp(1.5f, 4.2f, Hash01(i * 31 + particles.Count * 7));
            sphere.transform.position = center;
            sphere.transform.localScale = Vector3.one * size;
            particles.Add(new Particle
            {
                Transform = sphere.transform,
                Start = center,
                Velocity = new Vector3(
                    Mathf.Cos(radians) * speed,
                    Mathf.Sin(radians) * speed,
                    (Hash01(i * 17 + 9) - 0.5f) * 1.6f),
                StartTime = SequenceTime,
                Lifetime = Mathf.Lerp(0.45f, 0.8f, Hash01(i * 13 + 5)),
                StartScale = size
            });
        }
    }

    private void AnimateParticles(float time)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            Particle particle = particles[i];
            float age = time - particle.StartTime;
            if (age >= particle.Lifetime)
            {
                if (particle.Transform != null)
                {
                    Destroy(particle.Transform.gameObject);
                }
                particles.RemoveAt(i);
                continue;
            }
            float progress = age / particle.Lifetime;
            particle.Transform.position = particle.Start + particle.Velocity * age +
                Vector3.down * (2.6f * age * age);
            particle.Transform.localScale = Vector3.one *
                (particle.StartScale * Mathf.Lerp(1f, 0.05f, progress));
        }
    }

    private void AnimateCamera(float time)
    {
        Vector3 target;
        if (time < TripTime)
        {
            float progress = Mathf.InverseLerp(0f, TripTime, time);
            target = Vector3.Lerp(new Vector3(0f, 5.4f, 3.9f), new Vector3(0f, 3.3f, -0.5f), progress);
        }
        else if (time < BurstTime)
        {
            float progress = Mathf.InverseLerp(TripTime, BurstTime, time);
            target = Vector3.Lerp(new Vector3(0f, 3.3f, -0.5f), new Vector3(0f, 2.35f, -1.9f), progress);
        }
        else
        {
            target = new Vector3(0f, 2.45f, -2.15f);
        }

        Vector3 shake = Time.time < shakeUntil
            ? new Vector3(
                Mathf.Sin(Time.time * 73f),
                Mathf.Cos(Time.time * 91f),
                0f) * shakeStrength
            : Vector3.zero;
        shotCamera.transform.position = target + new Vector3(0f, 3.25f, -22.5f) + shake;
        shotCamera.transform.LookAt(target);
    }

    private void Shake(float strength, float duration)
    {
        shakeStrength = Mathf.Max(shakeStrength, strength);
        shakeUntil = Mathf.Max(shakeUntil, Time.time + duration);
    }

    private void PlayEffect(AudioClip clip, float pitch, float volume)
    {
        if (clip == null || effectsSource == null)
        {
            return;
        }
        effectsSource.pitch = pitch;
        effectsSource.PlayOneShot(clip, volume);
    }

    private static void SetStartled(Actor actor, bool startled)
    {
        actor.OriginalFace.gameObject.SetActive(!startled);
        actor.StartledFace.gameObject.SetActive(startled);
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

    public static Bounds BoundsOf(GameObject target)
    {
        return BoundsOf(target.GetComponentsInChildren<Renderer>(true), target.transform.position);
    }

    public static Bounds BoundsOf(Renderer[] renderers, Vector3 fallback)
    {
        bool found = false;
        Bounds bounds = new Bounds(fallback, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
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

    private static Transform FindChild(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child;
            }
        }
        return null;
    }

    private static float EaseInCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * value;
    }

    private static float EaseInOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value < 0.5f
            ? 4f * value * value * value
            : 1f - Mathf.Pow(-2f * value + 2f, 3f) * 0.5f;
    }

    private static float EaseOutBack(float value)
    {
        value = Mathf.Clamp01(value);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(value - 1f, 3f) + c1 * Mathf.Pow(value - 1f, 2f);
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
}
