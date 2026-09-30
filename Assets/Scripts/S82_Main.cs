using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Scene 82: original Numberblocks 50 and 100 collide twice, burst into exactly
/// 150 Ones, then reassemble as a wide 15 x 10 Numberblock 150.
/// </summary>
public class S82_Main : MonoBehaviour
{
    public const float SequenceDuration = 15.5f;
    public const float DeterminedTime = 0.85f;
    public const float FirstImpactTime = 2.65f;
    public const float FirstRecoveryTime = 3.25f;
    public const float FinalChargeTime = 3.95f;
    public const float FinalImpactTime = 5.15f;
    public const float ResultRevealTime = 10.05f;
    public const float FinalJumpTime = 11.25f;
    public const float FinalLandingTime = 12.35f;

    public const string FiftyOriginalFaceName = "S82 50 Original Face";
    public const string FiftyDeterminedFaceName = "S82 50 Determined Face";
    public const string HundredOriginalFaceName = "S82 100 Original Face";
    public const string HundredDeterminedFaceName = "S82 100 Determined Face";
    public const string HundredShockedFaceName = "S82 100 Shocked Face";

    [Header("Original Numberblocks and scene-local variants")]
    public GameObject onePrefab;
    public GameObject fiftyPrefab;
    public GameObject hundredPrefab;

    [Header("Scale and composition")]
    public float characterScale = 0.52f;
    public Material finalFaceMaterial;
    public Material dustMaterial;
    public Material flashMaterial;
    public Material crackMaterial;
    public S82_UnitBurst unitBurst;
    public TextMesh equationText;

    [Header("Existing project audio")]
    public AudioClip whooshClip;
    public AudioClip impactClip;
    public AudioClip impactAccentClip;
    public AudioClip explosionClip;
    public AudioClip successClip;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public int CollisionCount { get; private set; }
    public bool BurstTriggered { get; private set; }
    public bool ResultRevealed { get; private set; }
    public bool GroundCracked { get; private set; }

    private readonly List<DustPuff> dustPuffs = new List<DustPuff>(30);
    private readonly List<FlashPulse> flashPulses = new List<FlashPulse>(4);

    private Actor fiftyActor;
    private Actor hundredActor;
    private Transform resultRoot;
    private Bounds resultBodyBounds;
    private Camera shotCamera;
    private AudioSource whooshSource;
    private AudioSource effectsSource;
    private Vector3 cameraBasePosition;
    private float fiftyContactX;
    private float hundredContactX;
    private float fiftyReadyX;
    private float hundredReadyX;
    private float sequenceStartTime;
    private bool determinedTriggered;
    private bool recoveredFromFirstImpact;
    private bool finalChargeTriggered;
    private bool finalLandingTriggered;
    private bool initialized;

    private sealed class Actor
    {
        public Transform Root;
        public GameObject Visual;
        public Transform OriginalFace;
        public Transform DeterminedFace;
        public Transform ShockedFace;
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
            Debug.LogError("[S82] Scene needs a camera tagged MainCamera.", this);
            return;
        }
        shotCamera.aspect = 9f / 16f;
        cameraBasePosition = shotCamera.transform.position;
        ConfigureAudio();

        fiftyActor = SpawnActor(
            "Numberblock 50 — Left",
            fiftyPrefab,
            FiftyOriginalFaceName,
            FiftyDeterminedFaceName,
            null);
        hundredActor = SpawnActor(
            "Numberblock 100 — Right",
            hundredPrefab,
            HundredOriginalFaceName,
            HundredDeterminedFaceName,
            HundredShockedFaceName);
        if (fiftyActor == null || hundredActor == null)
        {
            return;
        }

        fiftyContactX = -hundredActor.Size.x * 0.5f;
        hundredContactX = fiftyActor.Size.x * 0.5f;
        fiftyReadyX = fiftyContactX - 1.25f;
        hundredReadyX = hundredContactX + 1.25f;
        SetFiftyExpression(false);
        SetHundredExpression(0);

        BuildResultCharacter();
        unitBurst.Prepare();
        if (!unitBurst.IsPrepared)
        {
            return;
        }

        UpdateEquation(false);
        sequenceStartTime = Time.time;
        SequenceTime = 0f;
        SequenceComplete = false;
        CollisionCount = 0;
        BurstTriggered = false;
        ResultRevealed = false;
        GroundCracked = false;
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
        if (!BurstTriggered)
        {
            AnimateActors(time);
        }
        AnimateResult(time);
        TriggerTimelineEvents(time);
        AnimateDust(time);
        AnimateFlashes(time);
        AnimateCamera(time);

        if (time >= SequenceDuration)
        {
            SequenceTime = SequenceDuration;
            SequenceComplete = true;
            Debug.Log(
                "[S82] COMPLETE — 50 and 100 collided, became 150 Ones, and rebuilt as 150.",
                this);
        }
    }

    private bool ValidateReferences()
    {
        List<string> missing = new List<string>();
        if (onePrefab == null) missing.Add(nameof(onePrefab));
        if (fiftyPrefab == null) missing.Add(nameof(fiftyPrefab));
        if (hundredPrefab == null) missing.Add(nameof(hundredPrefab));
        if (finalFaceMaterial == null) missing.Add(nameof(finalFaceMaterial));
        if (dustMaterial == null) missing.Add(nameof(dustMaterial));
        if (flashMaterial == null) missing.Add(nameof(flashMaterial));
        if (crackMaterial == null) missing.Add(nameof(crackMaterial));
        if (unitBurst == null) missing.Add(nameof(unitBurst));
        if (equationText == null) missing.Add(nameof(equationText));

        if (missing.Count == 0)
        {
            return true;
        }
        Debug.LogError($"[S82] Missing scene references: {string.Join(", ", missing)}.", this);
        return false;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorAsset("Assets/Prefabs/Blocks/1.prefab", onePrefab);
        fiftyPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene82/Prefabs/Fifty_Collision.prefab", fiftyPrefab);
        hundredPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene82/Prefabs/Hundred_Collision.prefab", hundredPrefab);
        finalFaceMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene82/Materials/OneFifty_Happy_Transparent.mat",
            finalFaceMaterial);
        dustMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene82/Materials/S82_Dust.mat", dustMaterial);
        flashMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene82/Materials/S82_Flash.mat", flashMaterial);
        crackMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene82/Materials/S82_Crack.mat", crackMaterial);
        whooshClip = LoadEditorAsset("Assets/Sound/zvuk-priblijeniya.mp3", whooshClip);
        impactClip = LoadEditorAsset("Assets/Sound/collCube.wav", impactClip);
        impactAccentClip = LoadEditorAsset("Assets/Sound/boom_metal.wav", impactAccentClip);
        explosionClip = LoadEditorAsset("Assets/Sound/destroy_blocks.mp3", explosionClip);
        successClip = LoadEditorAsset(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav", successClip);
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
        whooshSource.volume = 0.65f;

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.volume = 0.9f;
    }

    private Actor SpawnActor(
        string objectName,
        GameObject prefab,
        string originalFaceName,
        string determinedFaceName,
        string shockedFaceName)
    {
        GameObject root = new GameObject(objectName);
        root.transform.SetParent(transform, false);
        GameObject visual = Instantiate(prefab, root.transform);
        visual.name = objectName + " Visual";
        PrepareForAnimation(visual);
        visual.transform.localScale = Vector3.one * characterScale;

        Bounds bounds = BoundsOf(visual);
        Vector3 bottomCenter = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        visual.transform.position -= bottomCenter;
        bounds = BoundsOf(visual);

        Actor actor = new Actor
        {
            Root = root.transform,
            Visual = visual,
            OriginalFace = FindChild(visual.transform, originalFaceName),
            DeterminedFace = FindChild(visual.transform, determinedFaceName),
            ShockedFace = string.IsNullOrEmpty(shockedFaceName)
                ? null
                : FindChild(visual.transform, shockedFaceName),
            Size = bounds.size
        };

        if (actor.OriginalFace == null || actor.DeterminedFace == null ||
            (!string.IsNullOrEmpty(shockedFaceName) && actor.ShockedFace == null))
        {
            Debug.LogError($"[S82] {objectName} is missing a face overlay.", visual);
            Destroy(root);
            return null;
        }
        return actor;
    }

    private void BuildResultCharacter()
    {
        GameObject rootObject = new GameObject("Numberblock 150 — Original 50 + 100");
        rootObject.transform.SetParent(transform, false);
        resultRoot = rootObject.transform;

        GameObject fiftyPart = Instantiate(fiftyPrefab, resultRoot);
        fiftyPart.name = "Original Numberblock 50 Part";
        PrepareForAnimation(fiftyPart);
        DisableAllFaces(fiftyPart);
        fiftyPart.transform.localScale = Vector3.one * characterScale;

        GameObject hundredPart = Instantiate(hundredPrefab, resultRoot);
        hundredPart.name = "Original Numberblock 100 Part";
        PrepareForAnimation(hundredPart);
        DisableAllFaces(hundredPart);
        hundredPart.transform.localScale = Vector3.one * characterScale;

        Bounds fiftyBounds = BoundsOf(fiftyPart);
        Bounds hundredBounds = BoundsOf(hundredPart);
        PlaceBottomCenter(fiftyPart, new Vector3(-hundredBounds.size.x * 0.5f, 0f, 0f));
        PlaceBottomCenter(hundredPart, new Vector3(fiftyBounds.size.x * 0.5f, 0f, 0f));
        resultBodyBounds = Encapsulate(BoundsOf(fiftyPart), BoundsOf(hundredPart));

        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
        face.name = "S82 150 Combined Happy Face";
        face.transform.SetParent(resultRoot, true);
        face.transform.position = new Vector3(
            resultBodyBounds.center.x,
            resultBodyBounds.center.y,
            resultBodyBounds.min.z - 0.018f);
        face.transform.localScale = new Vector3(
            resultBodyBounds.size.x,
            resultBodyBounds.size.y,
            1f);
        foreach (Collider collider in face.GetComponents<Collider>())
        {
            Destroy(collider);
        }
        Renderer renderer = face.GetComponent<Renderer>();
        renderer.sharedMaterial = finalFaceMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = 30;

        resultRoot.gameObject.SetActive(false);
    }

    private static void PlaceBottomCenter(GameObject target, Vector3 desired)
    {
        Bounds bounds = BoundsOf(target);
        Vector3 current = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        target.transform.position += desired - current;
    }

    private static void DisableAllFaces(GameObject target)
    {
        foreach (Transform child in target.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.StartsWith("S82 ") && child.name.EndsWith(" Face"))
            {
                child.gameObject.SetActive(false);
            }
        }
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
        float leftX;
        float rightX;
        if (time < DeterminedTime)
        {
            float t = EaseOutCubic(time / DeterminedTime);
            leftX = Mathf.Lerp(-7.2f, fiftyReadyX, t);
            rightX = Mathf.Lerp(7.2f, hundredReadyX, t);
        }
        else if (time < FirstImpactTime)
        {
            float t = EaseInCubic(InverseLerp(DeterminedTime, FirstImpactTime, time));
            leftX = Mathf.Lerp(fiftyReadyX, fiftyContactX, t);
            rightX = Mathf.Lerp(hundredReadyX, hundredContactX, t);
        }
        else if (time < FirstRecoveryTime)
        {
            float rebound = Mathf.Sin(InverseLerp(FirstImpactTime, FirstRecoveryTime, time) * Mathf.PI);
            leftX = fiftyContactX - rebound * 0.82f;
            rightX = hundredContactX + rebound * 0.82f;
        }
        else if (time < FinalChargeTime)
        {
            float t = EaseOutCubic(InverseLerp(FirstRecoveryTime, FinalChargeTime, time));
            leftX = Mathf.Lerp(fiftyContactX, fiftyReadyX - 0.55f, t);
            rightX = Mathf.Lerp(hundredContactX, hundredReadyX + 0.55f, t);
        }
        else
        {
            float t = EaseInCubic(InverseLerp(FinalChargeTime, FinalImpactTime, time));
            leftX = Mathf.Lerp(fiftyReadyX - 0.55f, fiftyContactX, t);
            rightX = Mathf.Lerp(hundredReadyX + 0.55f, hundredContactX, t);
        }

        float bob = Mathf.Abs(Mathf.Sin(time * (time >= FinalChargeTime ? 18f : 11f))) * 0.07f;
        Vector3 squash = Vector3.Scale(
            ImpactScaleAt(time, FirstImpactTime, 0.28f),
            ImpactScaleAt(time, FinalImpactTime, 0.48f));
        float wobble = WobbleAt(time, FirstImpactTime, 7f) +
            WobbleAt(time, FinalImpactTime, 12f);
        ApplyActorPose(fiftyActor, leftX, bob, squash, wobble);
        ApplyActorPose(hundredActor, rightX, bob, squash, -wobble);
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

    private void AnimateResult(float time)
    {
        if (!ResultRevealed || resultRoot == null)
        {
            return;
        }

        float reveal = Mathf.Clamp01((time - ResultRevealTime) / 0.72f);
        float scale = reveal < 0.72f
            ? Mathf.Lerp(0.04f, 1.08f, EaseOutBack(reveal / 0.72f))
            : Mathf.Lerp(1.08f, 1f, (reveal - 0.72f) / 0.28f);
        float jumpHeight = 0f;
        if (time >= FinalJumpTime && time <= FinalLandingTime)
        {
            float jump = InverseLerp(FinalJumpTime, FinalLandingTime, time);
            jumpHeight = Mathf.Sin(jump * Mathf.PI) * 1.1f;
            scale *= 1f + Mathf.Sin(jump * Mathf.PI) * 0.035f;
        }

        float landingSquash = Mathf.Exp(-Mathf.Pow((time - FinalLandingTime) / 0.1f, 2f));
        resultRoot.position = new Vector3(0f, jumpHeight, 0f);
        resultRoot.localScale = new Vector3(
            scale * (1f + landingSquash * 0.12f),
            scale * (1f - landingSquash * 0.16f),
            scale);
    }

    private void TriggerTimelineEvents(float time)
    {
        if (!determinedTriggered && time >= DeterminedTime)
        {
            determinedTriggered = true;
            SetFiftyExpression(true);
            SetHundredExpression(1);
            PlayWhoosh(1.04f, 0.72f);
        }

        if (CollisionCount == 0 && time >= FirstImpactTime)
        {
            CollisionCount = 1;
            SetHundredExpression(2);
            StopWhoosh();
            PlayEffect(impactClip, 1.05f, 0.78f);
            CreateImpactEffects(1);
        }

        if (!recoveredFromFirstImpact && time >= FirstRecoveryTime)
        {
            recoveredFromFirstImpact = true;
            SetFiftyExpression(true);
            SetHundredExpression(1);
        }

        if (!finalChargeTriggered && time >= FinalChargeTime)
        {
            finalChargeTriggered = true;
            PlayWhoosh(0.84f, 0.92f);
        }

        if (CollisionCount == 1 && time >= FinalImpactTime)
        {
            CollisionCount = 2;
            TriggerFinalImpact();
        }

        if (!ResultRevealed && time >= ResultRevealTime && unitBurst.FormationComplete)
        {
            ResultRevealed = true;
            unitBurst.Hide();
            resultRoot.gameObject.SetActive(true);
            resultRoot.position = Vector3.zero;
            resultRoot.localScale = Vector3.one * 0.04f;
            UpdateEquation(true);
            PlayEffect(successClip, 1.04f, 0.85f);
            CreateImpactEffects(1);
        }

        if (!finalLandingTriggered && time >= FinalLandingTime && ResultRevealed)
        {
            finalLandingTriggered = true;
            GroundCracked = true;
            CreateGroundCracks();
            CreateImpactEffects(2);
            PlayEffect(impactAccentClip, 0.78f, 0.65f);
        }
    }

    private void TriggerFinalImpact()
    {
        StopWhoosh();
        ApplyActorPose(fiftyActor, fiftyContactX, 0f, Vector3.one, 0f);
        ApplyActorPose(hundredActor, hundredContactX, 0f, Vector3.one, 0f);
        Bounds fiftyBounds = BoundsOf(fiftyActor.Visual);
        Bounds hundredBounds = BoundsOf(hundredActor.Visual);

        PlayEffect(explosionClip, 0.9f, 0.95f);
        PlayEffect(impactAccentClip, 0.72f, 0.8f);
        CreateImpactEffects(3);
        fiftyActor.Visual.SetActive(false);
        hundredActor.Visual.SetActive(false);
        unitBurst.Begin(fiftyBounds, hundredBounds, resultBodyBounds);
        BurstTriggered = unitBurst.Active;
        if (!BurstTriggered)
        {
            Debug.LogError("[S82] The 150-One burst could not start.", this);
        }
    }

    private void SetFiftyExpression(bool determined)
    {
        fiftyActor.OriginalFace.gameObject.SetActive(!determined);
        fiftyActor.DeterminedFace.gameObject.SetActive(determined);
    }

    private void SetHundredExpression(int expression)
    {
        hundredActor.OriginalFace.gameObject.SetActive(expression == 0);
        hundredActor.DeterminedFace.gameObject.SetActive(expression == 1);
        hundredActor.ShockedFace.gameObject.SetActive(expression == 2);
    }

    private void UpdateEquation(bool solved)
    {
        equationText.text = solved ? "50 + 100 = 150!" : "50 + 100 = ?";
        equationText.color = solved
            ? new Color(1f, 0.86f, 0.18f)
            : Color.white;
    }

    private void PlayWhoosh(float pitch, float volume)
    {
        if (whooshClip == null)
        {
            return;
        }
        StopWhoosh();
        whooshSource.clip = whooshClip;
        whooshSource.pitch = pitch;
        whooshSource.volume = volume;
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

    private void PlayEffect(AudioClip clip, float pitch, float volume)
    {
        if (clip == null || effectsSource == null)
        {
            return;
        }
        effectsSource.pitch = pitch;
        effectsSource.PlayOneShot(clip, volume);
    }

    private void CreateImpactEffects(int strength)
    {
        float time = SequenceTime;
        float seam = (fiftyContactX + hundredContactX) * 0.5f;
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = $"S82 Collision Flash {CollisionCount}";
        flash.transform.SetParent(transform, false);
        flash.transform.position = new Vector3(
            ResultRevealed ? 0f : seam,
            ResultRevealed ? resultBodyBounds.center.y : Mathf.Min(fiftyActor.Size.y, hundredActor.Size.y) * 0.55f,
            -0.28f);
        flash.transform.localScale = Vector3.zero;
        foreach (Collider collider in flash.GetComponents<Collider>())
        {
            Destroy(collider);
        }
        Renderer renderer = flash.GetComponent<Renderer>();
        Material runtimeMaterial = new Material(flashMaterial)
        {
            name = $"S82 Runtime Flash {CollisionCount}"
        };
        renderer.sharedMaterial = runtimeMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        flashPulses.Add(new FlashPulse
        {
            Transform = flash.transform,
            RuntimeMaterial = runtimeMaterial,
            StartTime = time,
            Duration = 0.28f + strength * 0.07f,
            MaximumSize = 1.1f + strength * 0.75f
        });

        int count = 6 + strength * 5;
        for (int i = 0; i < count; i++)
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = $"S82 Impact Dust {CollisionCount}-{i + 1:00}";
            puff.transform.SetParent(transform, false);
            foreach (Collider collider in puff.GetComponents<Collider>())
            {
                Destroy(collider);
            }
            Renderer puffRenderer = puff.GetComponent<Renderer>();
            puffRenderer.sharedMaterial = dustMaterial;
            puffRenderer.shadowCastingMode = ShadowCastingMode.Off;
            puffRenderer.receiveShadows = false;

            float side = Hash01(i * 17 + strength * 41) - 0.5f;
            float angle = Mathf.Lerp(20f, 160f, count <= 1 ? 0f : i / (float)(count - 1)) * Mathf.Deg2Rad;
            Vector3 start = new Vector3(ResultRevealed ? side : seam + side * 0.25f, 0.08f, -0.12f);
            float size = Mathf.Lerp(0.1f, 0.22f, Hash01(i * 29 + strength * 67));
            puff.transform.position = start;
            puff.transform.localScale = Vector3.one * size;
            dustPuffs.Add(new DustPuff
            {
                Transform = puff.transform,
                Start = start,
                Velocity = new Vector3(
                    Mathf.Cos(angle) * (1.5f + strength * 0.7f),
                    Mathf.Sin(angle) * (1.2f + strength * 0.35f),
                    side * 1.6f),
                StartTime = time,
                Lifetime = 0.55f + strength * 0.13f,
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
                if (puff.Transform != null) Destroy(puff.Transform.gameObject);
                dustPuffs.RemoveAt(i);
                continue;
            }

            Vector3 position = puff.Start + puff.Velocity * elapsed;
            position.y = Mathf.Max(0.04f, position.y - 2.5f * elapsed * elapsed);
            puff.Transform.position = position;
            float size = puff.Size * Mathf.Lerp(1f, 2.2f, normalized) * (1f - normalized * 0.72f);
            puff.Transform.localScale = Vector3.one * size;
        }
    }

    private void AnimateFlashes(float time)
    {
        for (int i = flashPulses.Count - 1; i >= 0; i--)
        {
            FlashPulse pulse = flashPulses[i];
            float normalized = (time - pulse.StartTime) / pulse.Duration;
            if (normalized >= 1f)
            {
                if (pulse.Transform != null) Destroy(pulse.Transform.gameObject);
                if (pulse.RuntimeMaterial != null) Destroy(pulse.RuntimeMaterial);
                flashPulses.RemoveAt(i);
                continue;
            }
            float size = Mathf.Sin(Mathf.Clamp01(normalized) * Mathf.PI) * pulse.MaximumSize;
            pulse.Transform.localScale = Vector3.one * size;
        }
    }

    private void CreateGroundCracks()
    {
        for (int i = 0; i < 9; i++)
        {
            float angle = i * 360f / 9f + Hash01(i * 31 + 223) * 14f;
            float length = Mathf.Lerp(0.8f, 2.15f, Hash01(i * 37 + 251));
            GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crack.name = $"S82 Ground Crack {i + 1:00}";
            crack.transform.position = Quaternion.Euler(0f, angle, 0f) *
                new Vector3(0f, 0.018f, length * 0.47f);
            crack.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            crack.transform.localScale = new Vector3(0.055f, 0.018f, length);
            crack.GetComponent<Renderer>().sharedMaterial = crackMaterial;
            Destroy(crack.GetComponent<Collider>());
        }
    }

    private void AnimateCamera(float time)
    {
        float strength = ShakeEnvelope(time, FirstImpactTime, 0.42f) * 0.12f;
        strength += ShakeEnvelope(time, FinalImpactTime, 0.62f) * 0.32f;
        strength += ShakeEnvelope(time, FinalLandingTime, 0.48f) * 0.2f;
        Vector3 offset = new Vector3(
            Mathf.Sin(time * 79f),
            Mathf.Sin(time * 103f + 0.7f),
            0f) * strength;
        shotCamera.transform.position = cameraBasePosition + offset;
    }

    private static float ShakeEnvelope(float time, float eventTime, float duration)
    {
        float elapsed = time - eventTime;
        if (elapsed < 0f || elapsed > duration)
        {
            return 0f;
        }
        return 1f - elapsed / duration;
    }

    private static Vector3 ImpactScaleAt(float time, float impactTime, float strength)
    {
        float elapsed = time - impactTime;
        if (elapsed < -0.08f || elapsed > 0.4f)
        {
            return Vector3.one;
        }
        float compression = Mathf.Exp(-Mathf.Pow(elapsed / 0.085f, 2f));
        return new Vector3(
            1f - compression * strength,
            1f + compression * strength * 0.55f,
            1f + compression * strength * 0.18f);
    }

    private static float WobbleAt(float time, float impactTime, float amplitude)
    {
        float elapsed = time - impactTime;
        if (elapsed < 0f || elapsed > 0.55f)
        {
            return 0f;
        }
        return Mathf.Sin(elapsed * 38f) * amplitude * Mathf.Exp(-elapsed * 5.5f);
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

    private static Bounds Encapsulate(Bounds first, Bounds second)
    {
        first.Encapsulate(second);
        return first;
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

    private static float InverseLerp(float from, float to, float value)
    {
        return Mathf.Clamp01((value - from) / Mathf.Max(0.0001f, to - from));
    }

    private static float EaseInCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * value;
    }

    private static float EaseOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        return 1f - Mathf.Pow(1f - value, 3f);
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
