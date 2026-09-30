using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene 80: two Thousands collide three times. Their surprise escalates after
/// each impact and the final collision breaks both characters into 2,000 Ones.
/// </summary>
public class S80_Main : MonoBehaviour
{
    public const float SequenceDuration = 15.5f;
    public const float FirstImpactTime = 1.75f;
    public const float FirstRetreatStartTime = 2.2f;
    public const float FirstRetreatEndTime = 3.3f;
    public const float SecondChargeStartTime = 3.75f;
    public const float SecondImpactTime = 4.75f;
    public const float SecondRetreatStartTime = 5.25f;
    public const float SecondRetreatEndTime = 6.65f;
    public const float FinalChargeStartTime = 7.15f;
    public const float FinalImpactTime = 8.95f;

    public const string OriginalFaceName = "S80 Original Face Overlay";
    public const string SurprisedFaceName = "S80 Surprised Face Overlay";
    public const string ExtremeFaceName = "S80 Extreme Surprise Face Overlay";
    public const string DeterminedFaceName = "S80 Determined Face Overlay";

    [Header("Original Numberblocks")]
    public GameObject onePrefab;
    public GameObject thousandPrefab;

    [Header("Scene staging")]
    public float thousandScale = 0.29f;
    public float offscreenDistance = 6.35f;
    public float insideRetreatExtra = 0.78f;
    public Material dustMaterial;
    public Material flashMaterial;
    public S80_UnitBurst unitBurst;

    [Header("Existing project audio")]
    public AudioClip whooshClip;
    public AudioClip impactClip;
    public AudioClip impactAccentClip;
    public AudioClip finalImpactClip;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public int CollisionCount { get; private set; }
    public int ExpressionLevel { get; private set; }
    public bool BurstTriggered { get; private set; }

    private readonly List<DustPuff> dustPuffs = new List<DustPuff>(40);
    private readonly List<FlashPulse> flashPulses = new List<FlashPulse>(4);

    private Actor leftActor;
    private Actor rightActor;
    private Camera shotCamera;
    private AudioSource whooshSource;
    private AudioSource impactSource;
    private Vector3 cameraBasePosition;
    private Vector3 cameraLookTarget;
    private float contactDistance;
    private float insideDistance;
    private float sequenceStartTime;
    private bool secondChargeSoundPlayed;
    private bool finalChargeSoundPlayed;
    private bool initialized;

    private sealed class Actor
    {
        public Transform Root;
        public GameObject Visual;
        public Transform OriginalFace;
        public Transform SurprisedFace;
        public Transform ExtremeFace;
        public Transform DeterminedFace;
        public Vector3 Size;
    }

    private sealed class DustPuff
    {
        public Transform Transform;
        public Vector3 Start;
        public Vector3 Velocity;
        public float StartTime;
        public float Lifetime;
        public float Size;
    }

    private sealed class FlashPulse
    {
        public Transform Transform;
        public Material RuntimeMaterial;
        public float StartTime;
        public float Duration;
        public float MaximumSize;
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
            Debug.LogError("[S80] Scene needs a camera tagged MainCamera.", this);
            return;
        }

        shotCamera.aspect = 9f / 16f;
        cameraBasePosition = shotCamera.transform.position;
        cameraLookTarget = new Vector3(0f, 1.52f, 0.3f);
        ConfigureAudio();

        leftActor = SpawnActor("Thousand — Left", false);
        rightActor = SpawnActor("Thousand — Right", true);
        if (leftActor == null || rightActor == null)
        {
            return;
        }

        contactDistance = Mathf.Max(leftActor.Size.x, rightActor.Size.x) * 0.5f;
        insideDistance = contactDistance + insideRetreatExtra;
        unitBurst.Prepare();

        sequenceStartTime = Time.time;
        SequenceTime = 0f;
        SequenceComplete = false;
        CollisionCount = 0;
        ExpressionLevel = 0;
        BurstTriggered = false;
        initialized = true;

        PlayWhoosh(1.02f, 0.62f);
    }

    private void Update()
    {
        if (!initialized || SequenceComplete)
        {
            return;
        }

        float time = Mathf.Min(SequenceDuration, Time.time - sequenceStartTime);
        SequenceTime = time;

        if (!BurstTriggered)
        {
            AnimateActors(time);
        }

        TriggerTimelineEvents(time);
        AnimateDust(time);
        AnimateFlashPulses(time);
        AnimateCamera(time);

        if (time >= SequenceDuration)
        {
            SequenceTime = SequenceDuration;
            SequenceComplete = true;
            Debug.Log(
                "[S80] COMPLETE — three collisions finished and two Thousands became 2,000 Ones.",
                this);
        }
    }

    private bool ValidateReferences()
    {
        List<string> missing = new List<string>();
        if (onePrefab == null) missing.Add(nameof(onePrefab));
        if (thousandPrefab == null) missing.Add(nameof(thousandPrefab));
        if (dustMaterial == null) missing.Add(nameof(dustMaterial));
        if (flashMaterial == null) missing.Add(nameof(flashMaterial));
        if (unitBurst == null) missing.Add(nameof(unitBurst));

        if (missing.Count == 0)
        {
            return true;
        }

        Debug.LogError($"[S80] Missing scene references: {string.Join(", ", missing)}.", this);
        return false;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorAsset("Assets/Prefabs/Blocks/1.prefab", onePrefab);
        thousandPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene80/Thousand_Collision.prefab",
            thousandPrefab);
        dustMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene80/Materials/S80_Dust.mat",
            dustMaterial);
        flashMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene80/Materials/S80_Flash.mat",
            flashMaterial);
        whooshClip = LoadEditorAsset("Assets/Sound/zvuk-priblijeniya.mp3", whooshClip);
        impactClip = LoadEditorAsset("Assets/Sound/collCube.wav", impactClip);
        impactAccentClip = LoadEditorAsset("Assets/Sound/boom_metal.wav", impactAccentClip);
        finalImpactClip = LoadEditorAsset("Assets/Sound/Big Explosion Cut Off.mp3", finalImpactClip);
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
        whooshSource = gameObject.AddComponent<AudioSource>();
        whooshSource.playOnAwake = false;
        whooshSource.spatialBlend = 0f;
        whooshSource.loop = false;
        whooshSource.volume = 1f;

        impactSource = gameObject.AddComponent<AudioSource>();
        impactSource.playOnAwake = false;
        impactSource.spatialBlend = 0f;
        impactSource.loop = false;
        impactSource.volume = 0.9f;
    }

    private Actor SpawnActor(string objectName, bool mirrorDeterminedFace)
    {
        GameObject root = new GameObject(objectName);
        root.transform.SetParent(transform, false);

        GameObject visual = Instantiate(thousandPrefab, root.transform);
        visual.name = "Numberblock 1000 Visual";
        PrepareForAnimation(visual);
        visual.transform.localScale = Vector3.one * thousandScale;

        Bounds bounds = BoundsOf(visual);
        Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        visual.transform.position -= bottomCenter;
        bounds = BoundsOf(visual);

        Actor actor = new Actor
        {
            Root = root.transform,
            Visual = visual,
            OriginalFace = FindChild(visual.transform, OriginalFaceName),
            SurprisedFace = FindChild(visual.transform, SurprisedFaceName),
            ExtremeFace = FindChild(visual.transform, ExtremeFaceName),
            DeterminedFace = FindChild(visual.transform, DeterminedFaceName),
            Size = bounds.size
        };

        if (actor.OriginalFace == null || actor.SurprisedFace == null ||
            actor.ExtremeFace == null || actor.DeterminedFace == null)
        {
            Debug.LogError($"[S80] {objectName} is missing one or more expression overlays.", visual);
            Destroy(root);
            return null;
        }

        if (mirrorDeterminedFace)
        {
            Vector3 faceScale = actor.DeterminedFace.localScale;
            faceScale.x = -Mathf.Abs(faceScale.x);
            actor.DeterminedFace.localScale = faceScale;
        }

        SetExpression(actor, 0);
        return actor;
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

    private void AnimateActors(float time)
    {
        float magnitude = EvaluateDistance(time);
        float travelBob = EvaluateTravelBob(time);
        float impactLift = EvaluateImpactLift(time);
        Vector3 squash = EvaluateImpactScale(time);
        float wobble = EvaluateImpactWobble(time);

        ApplyActorPose(leftActor, -magnitude, travelBob + impactLift, squash, wobble);
        ApplyActorPose(rightActor, magnitude, travelBob + impactLift, squash, -wobble);
    }

    private float EvaluateDistance(float time)
    {
        if (time < FirstImpactTime)
        {
            return Mathf.Lerp(offscreenDistance, contactDistance, EaseInCubic(time / FirstImpactTime));
        }
        if (time < FirstRetreatStartTime)
        {
            return contactDistance + ImpactSeparation(time - FirstImpactTime, 0.1f);
        }
        if (time < FirstRetreatEndTime)
        {
            return Mathf.Lerp(
                contactDistance,
                insideDistance,
                EaseOutCubic(InverseLerp(FirstRetreatStartTime, FirstRetreatEndTime, time)));
        }
        if (time < SecondChargeStartTime)
        {
            return insideDistance;
        }
        if (time < SecondImpactTime)
        {
            return Mathf.Lerp(
                insideDistance,
                contactDistance,
                EaseInCubic(InverseLerp(SecondChargeStartTime, SecondImpactTime, time)));
        }
        if (time < SecondRetreatStartTime)
        {
            return contactDistance + ImpactSeparation(time - SecondImpactTime, 0.18f);
        }
        if (time < SecondRetreatEndTime)
        {
            return Mathf.Lerp(
                contactDistance,
                offscreenDistance,
                EaseInOutCubic(InverseLerp(SecondRetreatStartTime, SecondRetreatEndTime, time)));
        }
        if (time < FinalChargeStartTime)
        {
            return offscreenDistance;
        }

        return Mathf.Lerp(
            offscreenDistance,
            contactDistance,
            EaseInCubic(InverseLerp(FinalChargeStartTime, FinalImpactTime, time)));
    }

    private static float ImpactSeparation(float elapsed, float amount)
    {
        float t = Mathf.Clamp01(elapsed / 0.42f);
        return Mathf.Sin(t * Mathf.PI) * amount;
    }

    private float EvaluateTravelBob(float time)
    {
        if (time < FirstImpactTime)
        {
            return Mathf.Abs(Mathf.Sin(time * 9.5f)) * 0.055f;
        }
        if (time >= FirstRetreatStartTime && time < FirstRetreatEndTime)
        {
            return Mathf.Abs(Mathf.Sin((time - FirstRetreatStartTime) * 10f)) * 0.08f;
        }
        if (time >= SecondChargeStartTime && time < SecondImpactTime)
        {
            return Mathf.Abs(Mathf.Sin((time - SecondChargeStartTime) * 13f)) * 0.075f;
        }
        if (time >= SecondRetreatStartTime && time < SecondRetreatEndTime)
        {
            return Mathf.Abs(Mathf.Sin((time - SecondRetreatStartTime) * 15f)) * 0.13f;
        }
        if (time >= FinalChargeStartTime && time < FinalImpactTime)
        {
            return Mathf.Abs(Mathf.Sin((time - FinalChargeStartTime) * 18f)) * 0.11f;
        }
        return 0f;
    }

    private static float EvaluateImpactLift(float time)
    {
        float lift = ReactionHop(time, FirstImpactTime, 0.16f, 0.42f);
        lift += ReactionHop(time, SecondImpactTime, 0.28f, 0.48f);
        return lift;
    }

    private static float ReactionHop(float time, float impactTime, float height, float duration)
    {
        float elapsed = time - impactTime;
        if (elapsed < 0f || elapsed > duration)
        {
            return 0f;
        }
        return Mathf.Sin(elapsed / duration * Mathf.PI) * height;
    }

    private static Vector3 EvaluateImpactScale(float time)
    {
        Vector3 scale = Vector3.one;
        scale = Vector3.Scale(scale, ImpactScaleAt(time, FirstImpactTime, 0.2f));
        scale = Vector3.Scale(scale, ImpactScaleAt(time, SecondImpactTime, 0.34f));
        scale = Vector3.Scale(scale, ImpactScaleAt(time, FinalImpactTime, 0.48f));
        return scale;
    }

    private static Vector3 ImpactScaleAt(float time, float impactTime, float strength)
    {
        float elapsed = time - impactTime;
        if (elapsed < -0.08f || elapsed > 0.42f)
        {
            return Vector3.one;
        }

        float compression = Mathf.Exp(-Mathf.Pow(elapsed / 0.085f, 2f));
        float rebound = elapsed > 0f
            ? Mathf.Sin(Mathf.Clamp01(elapsed / 0.42f) * Mathf.PI * 2f) *
              Mathf.Exp(-elapsed * 5.2f)
            : 0f;
        return new Vector3(
            1f - compression * strength + rebound * strength * 0.26f,
            1f + compression * strength * 0.58f - rebound * strength * 0.18f,
            1f + compression * strength * 0.2f);
    }

    private static float EvaluateImpactWobble(float time)
    {
        float wobble = WobbleAt(time, FirstImpactTime, 5f);
        wobble += WobbleAt(time, SecondImpactTime, 8f);
        wobble += WobbleAt(time, FinalImpactTime, 12f);
        return wobble;
    }

    private static float WobbleAt(float time, float impactTime, float amplitude)
    {
        float elapsed = time - impactTime;
        if (elapsed < 0f || elapsed > 0.56f)
        {
            return 0f;
        }
        return Mathf.Sin(elapsed * 36f) * amplitude * Mathf.Exp(-elapsed * 5.8f);
    }

    private static void ApplyActorPose(
        Actor actor,
        float x,
        float height,
        Vector3 scale,
        float zRotation)
    {
        actor.Root.position = new Vector3(x, height, 0f);
        actor.Root.localScale = scale;
        actor.Root.rotation = Quaternion.Euler(0f, 0f, zRotation);
    }

    private void TriggerTimelineEvents(float time)
    {
        if (CollisionCount == 0 && time >= FirstImpactTime)
        {
            CollisionCount = 1;
            SetExpressionLevel(1);
            TriggerImpact(1);
        }

        if (!secondChargeSoundPlayed && time >= SecondChargeStartTime)
        {
            secondChargeSoundPlayed = true;
            PlayWhoosh(0.93f, 0.74f);
        }

        if (CollisionCount == 1 && time >= SecondImpactTime)
        {
            CollisionCount = 2;
            SetExpressionLevel(2);
            TriggerImpact(2);
        }

        if (!finalChargeSoundPlayed && time >= FinalChargeStartTime)
        {
            finalChargeSoundPlayed = true;
            SetExpressionLevel(3);
            PlayWhoosh(0.78f, 0.95f);
        }

        if (CollisionCount == 2 && time >= FinalImpactTime)
        {
            CollisionCount = 3;
            TriggerFinalImpact();
        }
    }

    private void SetExpressionLevel(int level)
    {
        ExpressionLevel = Mathf.Clamp(level, 0, 3);
        SetExpression(leftActor, ExpressionLevel);
        SetExpression(rightActor, ExpressionLevel);
    }

    private static void SetExpression(Actor actor, int level)
    {
        actor.OriginalFace.gameObject.SetActive(level == 0);
        actor.SurprisedFace.gameObject.SetActive(level == 1);
        actor.ExtremeFace.gameObject.SetActive(level == 2);
        actor.DeterminedFace.gameObject.SetActive(level >= 3);
    }

    private void TriggerImpact(int strength)
    {
        StopWhoosh();
        float pitch = strength == 1 ? 1.08f : 0.9f;
        PlayImpactClip(impactClip, pitch, strength == 1 ? 0.68f : 0.88f);
        if (strength >= 2)
        {
            PlayImpactClip(impactAccentClip, 1.02f, 0.62f);
        }
        CreateImpactEffects(strength);
    }

    private void TriggerFinalImpact()
    {
        StopWhoosh();
        leftActor.Root.position = new Vector3(-contactDistance, 0f, 0f);
        rightActor.Root.position = new Vector3(contactDistance, 0f, 0f);
        leftActor.Root.localScale = Vector3.one;
        rightActor.Root.localScale = Vector3.one;
        leftActor.Root.rotation = Quaternion.identity;
        rightActor.Root.rotation = Quaternion.identity;
        Bounds leftBounds = BoundsOf(leftActor.Visual);
        Bounds rightBounds = BoundsOf(rightActor.Visual);
        PlayImpactClip(finalImpactClip, 0.82f, 0.95f);
        PlayImpactClip(impactAccentClip, 0.72f, 0.78f);
        CreateImpactEffects(3);

        leftActor.Visual.SetActive(false);
        rightActor.Visual.SetActive(false);
        unitBurst.Begin(leftBounds, rightBounds);
        BurstTriggered = unitBurst.Active;

        if (!BurstTriggered)
        {
            Debug.LogError("[S80] The final impact could not start the 2,000-One burst.", this);
        }
    }

    private void PlayWhoosh(float pitch, float volume)
    {
        if (whooshClip == null || whooshSource == null)
        {
            return;
        }

        StopWhoosh();
        whooshSource.pitch = pitch;
        whooshSource.clip = whooshClip;
        whooshSource.volume = volume * 0.5f;
        whooshSource.Play();
    }

    private void StopWhoosh()
    {
        if (whooshSource == null)
        {
            return;
        }

        whooshSource.Stop();
        whooshSource.clip = null;
    }

    private void PlayImpactClip(AudioClip clip, float pitch, float volume)
    {
        if (clip == null || impactSource == null)
        {
            return;
        }
        impactSource.pitch = pitch;
        impactSource.PlayOneShot(clip, volume);
    }

    private void CreateImpactEffects(int strength)
    {
        float time = SequenceTime;
        GameObject flashObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flashObject.name = $"Collision Flash {CollisionCount}";
        flashObject.transform.SetParent(transform, false);
        flashObject.transform.position = new Vector3(0f, Mathf.Max(1.1f, leftActor.Size.y * 0.52f), -0.2f);
        flashObject.transform.localScale = Vector3.zero;
        foreach (Collider collider in flashObject.GetComponents<Collider>())
        {
            Destroy(collider);
        }

        Renderer flashRenderer = flashObject.GetComponent<Renderer>();
        Material runtimeMaterial = new Material(flashMaterial)
        {
            name = $"S80 Runtime Collision Flash {CollisionCount}"
        };
        flashRenderer.sharedMaterial = runtimeMaterial;
        flashRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        flashRenderer.receiveShadows = false;
        flashPulses.Add(new FlashPulse
        {
            Transform = flashObject.transform,
            RuntimeMaterial = runtimeMaterial,
            StartTime = time,
            Duration = strength == 3 ? 0.48f : 0.3f + strength * 0.04f,
            MaximumSize = 1.2f + strength * 0.75f
        });

        int puffCount = 7 + strength * 5;
        for (int i = 0; i < puffCount; i++)
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = $"Impact Dust {CollisionCount}-{i + 1:00}";
            puff.transform.SetParent(transform, false);
            foreach (Collider collider in puff.GetComponents<Collider>())
            {
                Destroy(collider);
            }
            Renderer renderer = puff.GetComponent<Renderer>();
            renderer.sharedMaterial = dustMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            float spread = puffCount <= 1 ? 0f : i / (float)(puffCount - 1);
            float angle = Mathf.Lerp(18f, 162f, spread) * Mathf.Deg2Rad;
            float sideVariation = Hash01(i + strength * 101) - 0.5f;
            Vector3 velocity = new Vector3(
                Mathf.Cos(angle) * (1.7f + strength * 0.75f),
                Mathf.Sin(angle) * (1.1f + strength * 0.38f),
                sideVariation * (1f + strength * 0.4f));
            float size = Mathf.Lerp(0.12f, 0.24f, Hash01(i * 17 + strength * 211));
            Vector3 start = new Vector3(sideVariation * 0.28f, 0.08f, -0.08f);
            puff.transform.position = start;
            puff.transform.localScale = Vector3.one * size;
            dustPuffs.Add(new DustPuff
            {
                Transform = puff.transform,
                Start = start,
                Velocity = velocity,
                StartTime = time,
                Lifetime = 0.62f + strength * 0.13f,
                Size = size
            });
        }
    }

    private void AnimateDust(float time)
    {
        for (int i = dustPuffs.Count - 1; i >= 0; i--)
        {
            DustPuff puff = dustPuffs[i];
            float elapsed = time - puff.StartTime;
            float normalized = elapsed / puff.Lifetime;
            if (normalized >= 1f)
            {
                if (puff.Transform != null)
                {
                    Destroy(puff.Transform.gameObject);
                }
                dustPuffs.RemoveAt(i);
                continue;
            }

            Vector3 position = puff.Start + puff.Velocity * elapsed;
            position.y -= 2.8f * elapsed * elapsed;
            position.y = Mathf.Max(0.05f, position.y);
            puff.Transform.position = position;
            float scale = puff.Size * Mathf.Lerp(1f, 2.25f, normalized) * (1f - normalized * 0.7f);
            puff.Transform.localScale = Vector3.one * scale;
        }
    }

    private void AnimateFlashPulses(float time)
    {
        for (int i = flashPulses.Count - 1; i >= 0; i--)
        {
            FlashPulse pulse = flashPulses[i];
            float normalized = (time - pulse.StartTime) / pulse.Duration;
            if (normalized >= 1f)
            {
                if (pulse.Transform != null)
                {
                    Destroy(pulse.Transform.gameObject);
                }
                if (pulse.RuntimeMaterial != null)
                {
                    Destroy(pulse.RuntimeMaterial);
                }
                flashPulses.RemoveAt(i);
                continue;
            }

            float size = Mathf.Sin(Mathf.Clamp01(normalized) * Mathf.PI) * pulse.MaximumSize;
            pulse.Transform.localScale = Vector3.one * size;
            Color color = GetMaterialColor(pulse.RuntimeMaterial);
            color.a = Mathf.Lerp(0.72f, 0f, normalized);
            SetMaterialColor(pulse.RuntimeMaterial, color);
        }
    }

    private void AnimateCamera(float time)
    {
        float burstPullback = time > FinalImpactTime
            ? EaseOutCubic(Mathf.Clamp01((time - FinalImpactTime) / 1.8f))
            : 0f;
        Vector3 position = cameraBasePosition + Vector3.back * burstPullback * 1.9f;

        float shake = CameraShakeAt(time, FirstImpactTime, 0.08f, 0.34f);
        shake += CameraShakeAt(time, SecondImpactTime, 0.14f, 0.46f);
        shake += CameraShakeAt(time, FinalImpactTime, 0.25f, 0.68f);
        position += new Vector3(
            Mathf.Sin(time * 69f) * shake,
            Mathf.Sin(time * 91f + 0.8f) * shake * 0.72f,
            0f);

        shotCamera.transform.position = position;
        shotCamera.transform.rotation = Quaternion.LookRotation(cameraLookTarget - position);
    }

    private static float CameraShakeAt(float time, float impactTime, float amount, float duration)
    {
        float elapsed = time - impactTime;
        if (elapsed < 0f || elapsed > duration)
        {
            return 0f;
        }
        return amount * (1f - elapsed / duration);
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

    private static float InverseLerp(float start, float end, float value)
    {
        return Mathf.Clamp01((value - start) / Mathf.Max(0.0001f, end - start));
    }

    private static float EaseInCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * value;
    }

    private static float EaseOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        float inverse = 1f - value;
        return 1f - inverse * inverse * inverse;
    }

    private static float EaseInOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value < 0.5f
            ? 4f * value * value * value
            : 1f - Mathf.Pow(-2f * value + 2f, 3f) * 0.5f;
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }
        return material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
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
