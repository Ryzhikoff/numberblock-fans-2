using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Scene 79: ten Thousands land in two five-block columns, become Ten Thousand,
/// then burst into exactly 10,000 individually animated Ones.
/// </summary>
public class S79_Main : MonoBehaviour
{
    public const int ThousandCount = 10;
    public const int BlocksPerColumn = 5;
    public const float SequenceDuration = 34f;
    public const float FirstImpactTime = 1.55f;
    public const float ImpactInterval = 2.15f;
    public const float FallDuration = 0.9f;
    public const float MergeStartTime = 22.15f;
    public const float MergeDuration = 0.85f;
    public const float BurstTime = 25.8f;

    private const string OriginalFaceName = "S78 Original Face Overlay";
    private const string SmirkFaceName = "S78 Smirk Face Overlay";

    [Header("Original Numberblocks")]
    public GameObject onePrefab;
    public GameObject thousandPrefab;
    public GameObject tenThousandPrefab;

    [Header("Scene staging")]
    public float blockScale = 0.34f;
    public float dropHeight = 11f;
    public float structureDepth = 0.6f;
    public Material dustMaterial;
    public Material flashMaterial;
    public S79_UnitBurst unitBurst;

    [Header("Existing project audio")]
    public AudioClip impactClip;
    public AudioClip impactAccentClip;
    public AudioClip transformClip;
    public AudioClip burstClip;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public int ImpactCount { get; private set; }
    public bool TransformationTriggered { get; private set; }
    public bool BurstTriggered { get; private set; }

    private readonly List<LandingBlock> blocks = new List<LandingBlock>(ThousandCount);
    private readonly List<DustPuff> dustPuffs = new List<DustPuff>(ThousandCount * 12);
    private readonly List<FlashPulse> flashPulses = new List<FlashPulse>(3);

    private Camera shotCamera;
    private AudioSource effectsSource;
    private GameObject tenThousand;
    private Vector3 tenThousandBaseScale;
    private Vector3 tenThousandBottomCenter;
    private Vector3 tenThousandFinalPosition;
    private Vector3 blockSize;
    private float sequenceStartTime;
    private float latestImpactTime = -10f;
    private bool initialized;

    private sealed class LandingBlock
    {
        public GameObject Root;
        public Transform OriginalFace;
        public Transform SmirkFace;
        public Vector3 BaseScale;
        public Vector3 FinalPosition;
        public Vector3 LandingBottomCenter;
        public Vector3 FallStartPosition;
        public bool Impacted;
        public bool SmirkShown;
    }

    private sealed class DustPuff
    {
        public Transform Transform;
        public Vector3 Start;
        public Vector3 Velocity;
        public float StartTime;
        public float Size;
    }

    private sealed class FlashPulse
    {
        public Transform Transform;
        public Renderer Renderer;
        public Material RuntimeMaterial;
        public float StartTime;
        public float Duration;
        public float MaximumSize;
    }

    private struct CameraPose
    {
        public Vector3 Position;
        public Quaternion Rotation;
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
            Debug.LogError("[S79] Scene needs a camera tagged MainCamera.", this);
            return;
        }

        shotCamera.aspect = 16f / 9f;
        ConfigureAudio();
        SpawnStructure();
        if (blocks.Count != ThousandCount || tenThousand == null)
        {
            return;
        }

        sequenceStartTime = Time.time;
        SequenceTime = 0f;
        SequenceComplete = false;
        ImpactCount = 0;
        TransformationTriggered = false;
        BurstTriggered = false;
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

        AnimateThousands(time);
        AnimateDust(time);
        AnimateTransformation(time);
        AnimateFlashPulses(time);
        AnimateCamera(time);

        if (!BurstTriggered && time >= BurstTime)
        {
            TriggerBurst();
        }

        if (time >= SequenceDuration)
        {
            SequenceTime = SequenceDuration;
            SequenceComplete = true;
            Debug.Log(
                "[S79] COMPLETE — ten Thousands became Ten Thousand and burst into 10,000 Ones.",
                this);
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
        if (tenThousandPrefab == null)
        {
            missing.Add(nameof(tenThousandPrefab));
        }
        if (dustMaterial == null)
        {
            missing.Add(nameof(dustMaterial));
        }
        if (flashMaterial == null)
        {
            missing.Add(nameof(flashMaterial));
        }
        if (unitBurst == null)
        {
            missing.Add(nameof(unitBurst));
        }

        if (missing.Count == 0)
        {
            return true;
        }

        Debug.LogError($"[S79] Missing scene references: {string.Join(", ", missing)}.", this);
        return false;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorAsset("Assets/Prefabs/Blocks/1.prefab", onePrefab);
        thousandPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene78/Thousand_Smirk.prefab",
            thousandPrefab);
        tenThousandPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/prefabTenThousand.prefab",
            tenThousandPrefab);
        dustMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene79/Materials/S79_Dust.mat",
            dustMaterial);
        flashMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene79/Materials/S79_Flash.mat",
            flashMaterial);
        impactClip = LoadEditorAsset(
            "Assets/Sound/topSound/jg-032316-sfx-distant-meteor-crash-impact-2.mp3",
            impactClip);
        impactAccentClip = LoadEditorAsset("Assets/Sound/collCube.wav", impactAccentClip);
        transformClip = LoadEditorAsset("Assets/Sound/boom_metal.wav", transformClip);
        burstClip = LoadEditorAsset("Assets/Sound/Big Explosion Cut Off.mp3", burstClip);
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
        effectsSource.volume = 0.85f;
        effectsSource.spatialBlend = 0f;
    }

    private void SpawnStructure()
    {
        GameObject first = InstantiatePrepared(thousandPrefab, "Thousand 01 — Left 1");
        first.transform.localScale = Vector3.one * blockScale;
        blockSize = BoundsOf(first).size;

        for (int i = 0; i < ThousandCount; i++)
        {
            GameObject instance = i == 0
                ? first
                : InstantiatePrepared(thousandPrefab, $"Thousand {i + 1:00}");
            instance.transform.localScale = Vector3.one * blockScale;

            int column = i / BlocksPerColumn;
            int row = i % BlocksPerColumn;
            float x = column == 0 ? -blockSize.x * 0.5f : blockSize.x * 0.5f;
            Vector3 landingBottom = new Vector3(x, row * blockSize.y, structureDepth);
            PlaceAtBottomCenter(instance, landingBottom);

            LandingBlock block = new LandingBlock
            {
                Root = instance,
                OriginalFace = FindChild(instance.transform, OriginalFaceName),
                SmirkFace = FindChild(instance.transform, SmirkFaceName),
                BaseScale = instance.transform.localScale,
                FinalPosition = instance.transform.position,
                LandingBottomCenter = landingBottom,
                FallStartPosition = instance.transform.position + Vector3.up * dropHeight
            };

            if (block.OriginalFace == null || block.SmirkFace == null)
            {
                Debug.LogError(
                    $"[S79] {instance.name} is missing the Scene 78 face overlays.",
                    instance);
                return;
            }

            block.OriginalFace.gameObject.SetActive(true);
            block.SmirkFace.gameObject.SetActive(false);
            block.Root.SetActive(false);
            blocks.Add(block);
        }

        tenThousand = InstantiatePrepared(tenThousandPrefab, "Ten Thousand — Combined");
        tenThousand.transform.localScale = Vector3.one * blockScale;
        tenThousandBottomCenter = new Vector3(0f, 0f, structureDepth);
        PlaceAtBottomCenter(tenThousand, tenThousandBottomCenter);
        tenThousandBaseScale = tenThousand.transform.localScale;
        tenThousandFinalPosition = tenThousand.transform.position;
        tenThousand.SetActive(false);

        unitBurst.onePrefab = onePrefab;
        unitBurst.Prepare();
        shotCamera.transform.SetPositionAndRotation(
            PoseForVisibleCount(1).Position,
            PoseForVisibleCount(1).Rotation);
    }

    private void AnimateThousands(float time)
    {
        if (TransformationTriggered)
        {
            return;
        }

        for (int i = 0; i < blocks.Count; i++)
        {
            LandingBlock block = blocks[i];
            float impactTime = ImpactTimeFor(i);
            float dropStart = impactTime - FallDuration;

            if (time < dropStart)
            {
                continue;
            }

            if (!block.Root.activeSelf)
            {
                block.Root.SetActive(true);
                block.Root.transform.localScale = block.BaseScale;
                block.Root.transform.position = block.FallStartPosition;
                block.Root.transform.rotation = Quaternion.identity;
            }

            if (time < impactTime)
            {
                float fall = Mathf.Clamp01((time - dropStart) / FallDuration);
                float accelerated = fall * fall;
                block.Root.transform.localScale = block.BaseScale;
                block.Root.transform.rotation = Quaternion.identity;
                block.Root.transform.position = Vector3.Lerp(
                    block.FallStartPosition,
                    block.FinalPosition,
                    accelerated);
                continue;
            }

            if (!block.Impacted)
            {
                TriggerImpact(block, impactTime, i);
            }

            AnimateLandedBlock(block, time - impactTime, i);
            if (!block.SmirkShown && time >= impactTime + 0.34f)
            {
                block.SmirkShown = true;
                block.OriginalFace.gameObject.SetActive(false);
                block.SmirkFace.gameObject.SetActive(true);
            }
        }
    }

    private void TriggerImpact(LandingBlock block, float impactTime, int index)
    {
        block.Impacted = true;
        block.Root.transform.SetPositionAndRotation(block.FinalPosition, Quaternion.identity);
        block.Root.transform.localScale = block.BaseScale;
        ImpactCount++;
        latestImpactTime = impactTime;

        Play(impactClip, index == ThousandCount - 1 ? 0.82f : 0.55f);
        Play(impactAccentClip, 0.72f);
        CreateDustPuffs(block.LandingBottomCenter, impactTime, index);
    }

    private void AnimateLandedBlock(LandingBlock block, float sinceImpact, int index)
    {
        Vector3 factor;
        float angle;
        if (sinceImpact < 0.11f)
        {
            float t = Mathf.SmoothStep(0f, 1f, sinceImpact / 0.11f);
            factor = Vector3.Lerp(Vector3.one, new Vector3(1.13f, 0.7f, 1.13f), t);
            angle = Mathf.Lerp(0f, index % 2 == 0 ? -2.6f : 2.6f, t);
        }
        else if (sinceImpact < 0.3f)
        {
            float t = Mathf.SmoothStep(0f, 1f, (sinceImpact - 0.11f) / 0.19f);
            factor = Vector3.Lerp(
                new Vector3(1.13f, 0.7f, 1.13f),
                new Vector3(0.97f, 1.08f, 0.97f),
                t);
            angle = Mathf.Lerp(index % 2 == 0 ? -2.6f : 2.6f, -0.8f, t);
        }
        else if (sinceImpact < 0.64f)
        {
            float t = Mathf.SmoothStep(0f, 1f, (sinceImpact - 0.3f) / 0.34f);
            factor = Vector3.Lerp(new Vector3(0.97f, 1.08f, 0.97f), Vector3.one, t);
            angle = Mathf.Lerp(-0.8f, 0f, t);
        }
        else
        {
            factor = Vector3.one;
            angle = 0f;
        }

        block.Root.transform.localScale = Vector3.Scale(block.BaseScale, factor);
        block.Root.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        block.Root.transform.position = block.FinalPosition;
        AnchorAtBottomCenter(block.Root, block.LandingBottomCenter);
    }

    private void AnimateTransformation(float time)
    {
        if (time >= MergeStartTime && !TransformationTriggered)
        {
            float amount = Mathf.Clamp01((time - MergeStartTime) / MergeDuration);
            float pulse = Mathf.Sin(amount * Mathf.PI);
            Vector3 structureCenter = tenThousandBottomCenter + Vector3.up * blockSize.y * 2.5f;

            for (int i = 0; i < blocks.Count; i++)
            {
                LandingBlock block = blocks[i];
                Vector3 blockCenter = BoundsOf(block.Root).center;
                Vector3 inward = (structureCenter - blockCenter) * (0.035f * pulse);
                block.Root.transform.position = block.FinalPosition + inward;
                block.Root.transform.localScale = block.BaseScale * (1f + pulse * 0.035f);
            }
        }

        if (!TransformationTriggered && time >= MergeStartTime + MergeDuration)
        {
            TriggerTransformation(time);
        }

        if (!TransformationTriggered || BurstTriggered)
        {
            return;
        }

        float revealTime = time - (MergeStartTime + MergeDuration);
        float scaleFactor;
        if (revealTime < 0.18f)
        {
            scaleFactor = Mathf.Lerp(0.72f, 1.08f, Mathf.SmoothStep(0f, 1f, revealTime / 0.18f));
        }
        else if (revealTime < 0.46f)
        {
            scaleFactor = Mathf.Lerp(1.08f, 1f, Mathf.SmoothStep(0f, 1f, (revealTime - 0.18f) / 0.28f));
        }
        else
        {
            scaleFactor = 1f + Mathf.Sin((revealTime - 0.46f) * 2.1f) * 0.006f;
        }

        tenThousand.transform.localScale = tenThousandBaseScale * scaleFactor;
        tenThousand.transform.position = tenThousandFinalPosition;
        AnchorAtBottomCenter(tenThousand, tenThousandBottomCenter);
    }

    private void TriggerTransformation(float time)
    {
        TransformationTriggered = true;
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].Root.SetActive(false);
        }

        tenThousand.SetActive(true);
        tenThousand.transform.localScale = tenThousandBaseScale * 0.72f;
        tenThousand.transform.position = tenThousandFinalPosition;
        AnchorAtBottomCenter(tenThousand, tenThousandBottomCenter);
        Play(transformClip, 0.9f);

        Vector3 center = BoundsOf(tenThousand).center;
        CreateFlashPulse(center, time, 0.48f, blockSize.y * 3.4f);
        CreateFlashPulse(center, time + 0.12f, 0.58f, blockSize.y * 4.4f);
    }

    private void TriggerBurst()
    {
        BurstTriggered = true;
        Bounds combinedBounds = BoundsOf(tenThousand);
        tenThousand.SetActive(false);
        Play(burstClip, 1f);
        CreateFlashPulse(combinedBounds.center, SequenceTime, 0.7f, blockSize.y * 5.4f);
        unitBurst.Begin(combinedBounds.center, blockScale);
        Debug.Log("[S79] The combined block released exactly 10,000 instanced Ones.", this);
    }

    private void CreateDustPuffs(Vector3 landingBottom, float startTime, int impactIndex)
    {
        Vector3 origin = landingBottom + Vector3.up * 0.08f;
        origin.z -= blockSize.z * 0.5f + 0.08f;
        const int puffCount = 12;
        for (int i = 0; i < puffCount; i++)
        {
            float angle = i * Mathf.PI * 2f / puffCount + impactIndex * 0.27f;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = $"Landing {impactIndex + 1:00} Dust {i + 1:00}";
            DisableCollider(puff);
            Renderer renderer = puff.GetComponent<Renderer>();
            renderer.sharedMaterial = dustMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            puff.transform.position = origin;
            puff.transform.localScale = Vector3.one * 0.01f;

            dustPuffs.Add(new DustPuff
            {
                Transform = puff.transform,
                Start = origin,
                Velocity = direction * (2f + (i % 4) * 0.37f) +
                    Vector3.up * (0.5f + (i % 3) * 0.22f),
                StartTime = startTime,
                Size = 0.2f + (i % 5) * 0.045f
            });
        }
    }

    private void AnimateDust(float time)
    {
        const float life = 0.82f;
        for (int i = 0; i < dustPuffs.Count; i++)
        {
            DustPuff puff = dustPuffs[i];
            float age = time - puff.StartTime;
            if (age < 0f || !puff.Transform.gameObject.activeSelf)
            {
                continue;
            }
            if (age >= life)
            {
                puff.Transform.gameObject.SetActive(false);
                continue;
            }

            float normalized = age / life;
            Vector3 position = puff.Start + puff.Velocity * age;
            position.y -= age * age * 0.7f;
            puff.Transform.position = position;
            float size = puff.Size * Mathf.Sin(normalized * Mathf.PI);
            puff.Transform.localScale = new Vector3(size * 1.25f, size, size);
        }
    }

    private void CreateFlashPulse(Vector3 center, float startTime, float duration, float maximumSize)
    {
        GameObject pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pulse.name = "Golden Transformation Pulse";
        DisableCollider(pulse);
        Renderer renderer = pulse.GetComponent<Renderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        Material runtimeMaterial = new Material(flashMaterial);
        renderer.sharedMaterial = runtimeMaterial;
        pulse.transform.position = center;
        pulse.transform.localScale = Vector3.zero;

        flashPulses.Add(new FlashPulse
        {
            Transform = pulse.transform,
            Renderer = renderer,
            RuntimeMaterial = runtimeMaterial,
            StartTime = startTime,
            Duration = duration,
            MaximumSize = maximumSize
        });
    }

    private void AnimateFlashPulses(float time)
    {
        for (int i = 0; i < flashPulses.Count; i++)
        {
            FlashPulse pulse = flashPulses[i];
            float age = time - pulse.StartTime;
            if (age < 0f)
            {
                continue;
            }
            if (age >= pulse.Duration)
            {
                pulse.Transform.gameObject.SetActive(false);
                continue;
            }

            float normalized = age / pulse.Duration;
            float size = Mathf.SmoothStep(0f, pulse.MaximumSize, normalized);
            pulse.Transform.localScale = Vector3.one * size;
            Color color = new Color(1f, 0.86f, 0.28f, (1f - normalized) * 0.36f);
            SetMaterialColor(pulse.RuntimeMaterial, color);
        }
    }

    private void AnimateCamera(float time)
    {
        CameraPose pose = PoseForVisibleCount(1);
        for (int nextIndex = 1; nextIndex < ThousandCount; nextIndex++)
        {
            float moveStart = ImpactTimeFor(nextIndex - 1) + 0.72f;
            float moveEnd = ImpactTimeFor(nextIndex) - FallDuration - 0.12f;
            CameraPose nextPose = PoseForVisibleCount(nextIndex + 1);

            if (time >= moveEnd)
            {
                pose = nextPose;
                continue;
            }
            if (time > moveStart)
            {
                float amount = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(moveStart, moveEnd, time));
                pose.Position = Vector3.Lerp(pose.Position, nextPose.Position, amount);
                pose.Rotation = Quaternion.Slerp(pose.Rotation, nextPose.Rotation, amount);
            }
            break;
        }

        if (time >= BurstTime - 0.35f)
        {
            CameraPose burstPose = PoseForVisibleCount(ThousandCount);
            Vector3 lookTarget = tenThousandBottomCenter + Vector3.up * blockSize.y * 2.5f;
            burstPose.Position += new Vector3(0f, 0.6f, -5.2f);
            burstPose.Rotation = Quaternion.LookRotation(lookTarget - burstPose.Position);
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(BurstTime - 0.35f, BurstTime + 1.1f, time));
            pose.Position = Vector3.Lerp(pose.Position, burstPose.Position, amount);
            pose.Rotation = Quaternion.Slerp(pose.Rotation, burstPose.Rotation, amount);
        }

        float sinceImpact = time - latestImpactTime;
        if (sinceImpact >= 0f && sinceImpact < 0.42f)
        {
            float strength = (1f - sinceImpact / 0.42f) * 0.13f;
            pose.Position += new Vector3(
                Mathf.Sin(sinceImpact * 95f) * strength,
                Mathf.Cos(sinceImpact * 81f) * strength,
                0f);
            pose.Rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(sinceImpact * 86f) * strength * 4f) *
                pose.Rotation;
        }

        shotCamera.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
    }

    private CameraPose PoseForVisibleCount(int visibleCount)
    {
        int count = Mathf.Clamp(visibleCount, 1, ThousandCount);
        int visibleRows = Mathf.Min(count, BlocksPerColumn);
        bool rightColumnVisible = count > BlocksPerColumn;
        float centerX = rightColumnVisible ? 0f : -blockSize.x * 0.5f;
        float height = visibleRows * blockSize.y;
        float verticalHalfAngle = shotCamera.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float heightDistance = height / (2f * Mathf.Tan(verticalHalfAngle) * 0.78f);
        float visibleWidth = rightColumnVisible ? blockSize.x * 2f : blockSize.x;
        float horizontalTangent = Mathf.Tan(verticalHalfAngle) * shotCamera.aspect;
        float widthDistance = visibleWidth / (2f * horizontalTangent * 0.8f);
        float distance = Mathf.Max(11.5f, heightDistance, widthDistance);
        Vector3 target = new Vector3(centerX, height * 0.5f, structureDepth);
        Vector3 position = new Vector3(centerX, target.y + 0.45f, structureDepth - distance);
        return new CameraPose
        {
            Position = position,
            Rotation = Quaternion.LookRotation(target - position)
        };
    }

    private static float ImpactTimeFor(int index)
    {
        return FirstImpactTime + index * ImpactInterval;
    }

    private void Play(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            effectsSource.PlayOneShot(clip, volume);
        }
    }

    private static GameObject InstantiatePrepared(GameObject prefab, string objectName)
    {
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.name = objectName;
        foreach (Animator animator in instance.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
        }
        foreach (Rigidbody body in instance.GetComponentsInChildren<Rigidbody>(true))
        {
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
        }
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
        return instance;
    }

    private static void PlaceAtBottomCenter(GameObject target, Vector3 desiredBottomCenter)
    {
        Vector3 current = BottomCenter(BoundsOf(target));
        target.transform.position += desiredBottomCenter - current;
    }

    private static void AnchorAtBottomCenter(GameObject target, Vector3 desiredBottomCenter)
    {
        Vector3 current = BottomCenter(BoundsOf(target));
        target.transform.position += desiredBottomCenter - current;
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

    private static void DisableCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
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
}
