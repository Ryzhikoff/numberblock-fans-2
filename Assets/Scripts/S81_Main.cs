using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Scene 81: a decimal forge in which ten equal Numberblocks physically assemble
/// into the next power of ten, from One to One Million.
/// </summary>
public class S81_Main : MonoBehaviour
{
    public const int PowerCount = 7;
    public const int StageCount = 6;
    public const int PiecesPerStage = 10;

    public const float IntroDuration = 2f;
    public const float StageDuration = 5.8f;
    public const float PreparationDuration = 0.65f;
    public const float PieceFlightDuration = 2.35f;
    public const float PieceStagger = 0.11f;
    public const float MergeTime = 4.05f;
    public const float RevealTime = 4.32f;
    public const float FinaleDuration = 5.2f;
    public const float SequenceDuration = IntroDuration + StageCount * StageDuration + FinaleDuration;

    private const float SeedDisplayScale = 1.55f;
    private const float MaximumDisplayWidth = 5.2f;
    private const float MaximumDisplayHeight = 4.5f;
    private const float MaximumDisplayDepth = 4.15f;

    private static readonly long[] PowerValues =
    {
        1L,
        10L,
        100L,
        1000L,
        10000L,
        100000L,
        1000000L
    };

    private static readonly Vector3[] LogicalDimensions =
    {
        new Vector3(1f, 1f, 1f),
        new Vector3(2f, 5f, 1f),
        new Vector3(10f, 10f, 1f),
        new Vector3(10f, 10f, 10f),
        new Vector3(20f, 50f, 10f),
        new Vector3(100f, 100f, 10f),
        new Vector3(100f, 100f, 100f)
    };

    private static readonly Vector3Int[] StageLayouts =
    {
        new Vector3Int(2, 5, 1),
        new Vector3Int(5, 2, 1),
        new Vector3Int(1, 1, 10),
        new Vector3Int(2, 5, 1),
        new Vector3Int(5, 2, 1),
        new Vector3Int(1, 1, 10)
    };

    private static readonly string[] GeometryLabels =
    {
        "2 × 5 GRID",
        "5 × 2 GRID",
        "10 LAYERS DEEP",
        "2 × 5 GRID",
        "5 × 2 GRID",
        "10 LAYERS DEEP"
    };

    private static readonly Vector3[] CameraPositions =
    {
        new Vector3(3.2f, 4.1f, -14.7f),
        new Vector3(-3.5f, 4.35f, -14.5f),
        new Vector3(5.35f, 4.65f, -14.15f),
        new Vector3(3.2f, 4.1f, -14.7f),
        new Vector3(-3.5f, 4.35f, -14.5f),
        new Vector3(5.35f, 4.65f, -14.15f)
    };

    [Header("Seven original Numberblocks")]
    public GameObject[] powerPrefabs;

    [Header("Decimal forge")]
    public Transform portalRig;
    public Transform mergePulseRig;
    public Light energyLight;
    public Material sparkMaterial;

    [Header("Interface")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI equationText;
    public TextMeshProUGUI geometryText;
    public TextMeshProUGUI progressText;
    public CanvasGroup fadeCanvasGroup;

    [Header("Existing project audio")]
    public AudioClip musicClip;
    public AudioClip flightClip;
    public AudioClip mergeClip;
    public AudioClip mergeAccentClip;
    public AudioClip[] numberVoiceClips;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public int CompletedMerges { get; private set; }
    public int ActiveStage { get; private set; } = -1;
    public int ActivePieceCount { get; private set; }
    public bool MillionRevealed { get; private set; }

    private sealed class PieceRuntime
    {
        public Transform Root;
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public Quaternion StartRotation;
    }

    private sealed class StageRuntime
    {
        public GameObject Group;
        public PieceRuntime[] Pieces;
        public GameObject Result;
        public float DisplayScale;
    }

    private sealed class SparkRuntime
    {
        public Transform Transform;
        public float Angle;
        public float RadiusOffset;
        public float Speed;
    }

    private readonly List<SparkRuntime> celebrationSparks = new List<SparkRuntime>(36);
    private readonly bool[] flightSoundPlayed = new bool[StageCount];
    private readonly bool[] mergeSoundPlayed = new bool[StageCount];
    private readonly StageRuntime[] stages = new StageRuntime[StageCount];

    private GameObject seed;
    private Camera shotCamera;
    private AudioSource musicSource;
    private AudioSource effectsSource;
    private AudioSource voiceSource;
    private Vector3 introCameraPosition;
    private float sequenceStartTime;
    private bool seedVoicePlayed;
    private bool initialized;

    public static long GetPowerValue(int powerIndex)
    {
        if (powerIndex < 0 || powerIndex >= PowerValues.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(powerIndex));
        }
        return PowerValues[powerIndex];
    }

    public static Vector3 GetLogicalDimensions(int powerIndex)
    {
        if (powerIndex < 0 || powerIndex >= LogicalDimensions.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(powerIndex));
        }
        return LogicalDimensions[powerIndex];
    }

    public static Vector3Int GetStageLayout(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= StageLayouts.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(stageIndex));
        }
        return StageLayouts[stageIndex];
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
            Debug.LogError("[S81] Scene needs a camera tagged MainCamera.", this);
            return;
        }

        shotCamera.aspect = 16f / 9f;
        introCameraPosition = shotCamera.transform.position;
        ConfigureAudio();
        BuildDecimalSequence();
        BuildCelebrationSparks();

        sequenceStartTime = Time.time;
        SequenceTime = 0f;
        SequenceComplete = false;
        CompletedMerges = 0;
        ActiveStage = -1;
        ActivePieceCount = 1;
        MillionRevealed = false;
        initialized = true;

        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        float time = Mathf.Min(SequenceDuration, Time.time - sequenceStartTime);
        SequenceTime = time;

        RenderSequence(time);
        TriggerAudioEvents(time);
        AnimateForge(time);
        AnimateCamera(time);
        UpdateInterface(time);

        if (!SequenceComplete && time >= SequenceDuration)
        {
            SequenceComplete = true;
            SequenceTime = SequenceDuration;
            Debug.Log(
                "[S81] COMPLETE — six exact ten-to-one merges reached 1,000,000.",
                this);
        }
    }

    private bool ValidateReferences()
    {
        List<string> missing = new List<string>();
        if (powerPrefabs == null || powerPrefabs.Length != PowerCount)
        {
            missing.Add($"{nameof(powerPrefabs)}[{PowerCount}]");
        }
        else
        {
            for (int i = 0; i < powerPrefabs.Length; i++)
            {
                if (powerPrefabs[i] == null)
                {
                    missing.Add($"{nameof(powerPrefabs)}[{i}]");
                }
            }
        }

        if (portalRig == null) missing.Add(nameof(portalRig));
        if (mergePulseRig == null) missing.Add(nameof(mergePulseRig));
        if (energyLight == null) missing.Add(nameof(energyLight));
        if (sparkMaterial == null) missing.Add(nameof(sparkMaterial));
        if (titleText == null) missing.Add(nameof(titleText));
        if (equationText == null) missing.Add(nameof(equationText));
        if (geometryText == null) missing.Add(nameof(geometryText));
        if (progressText == null) missing.Add(nameof(progressText));
        if (fadeCanvasGroup == null) missing.Add(nameof(fadeCanvasGroup));

        if (missing.Count == 0)
        {
            return true;
        }

        Debug.LogError($"[S81] Missing scene references: {string.Join(", ", missing)}.", this);
        return false;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        string[] prefabPaths =
        {
            "Assets/Prefabs/Blocks/1.prefab",
            "Assets/Prefabs/Blocks/10.prefab",
            "Assets/Prefabs/Blocks/100.prefab",
            "Assets/Prefabs/Blocks/1000.prefab",
            "Assets/Prefabs/Blocks/prefabTenThousand.prefab",
            "Assets/Prefabs/Blocks/OneHundredThousand.prefab",
            "Assets/Prefabs/Blocks/OneMillion.prefab"
        };

        if (powerPrefabs == null || powerPrefabs.Length != PowerCount)
        {
            GameObject[] previousPrefabs = powerPrefabs;
            powerPrefabs = new GameObject[PowerCount];
            if (previousPrefabs != null)
            {
                Array.Copy(
                    previousPrefabs,
                    powerPrefabs,
                    Mathf.Min(previousPrefabs.Length, powerPrefabs.Length));
            }
        }
        for (int i = 0; i < powerPrefabs.Length; i++)
        {
            powerPrefabs[i] = LoadEditorAssetIfMissing(prefabPaths[i], powerPrefabs[i]);
        }

        musicClip = LoadEditorAssetIfMissing(
            "Assets/Sound/Future Glider - Brian Bolger.mp3",
            musicClip);
        flightClip = LoadEditorAssetIfMissing("Assets/portal.mp3", flightClip);
        mergeClip = LoadEditorAssetIfMissing("Assets/Sound/collCube.wav", mergeClip);
        mergeAccentClip = LoadEditorAssetIfMissing(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav",
            mergeAccentClip);

        if (numberVoiceClips == null || numberVoiceClips.Length != PowerCount)
        {
            AudioClip[] previousVoiceClips = numberVoiceClips;
            numberVoiceClips = new AudioClip[PowerCount];
            if (previousVoiceClips != null)
            {
                Array.Copy(
                    previousVoiceClips,
                    numberVoiceClips,
                    Mathf.Min(previousVoiceClips.Length, numberVoiceClips.Length));
            }
        }
        numberVoiceClips[0] = LoadEditorAssetIfMissing(
            "Assets/Resources/sound/1.wav",
            numberVoiceClips[0]);
        numberVoiceClips[1] = LoadEditorAssetIfMissing(
            "Assets/Resources/sound/10.wav",
            numberVoiceClips[1]);
        numberVoiceClips[2] = LoadEditorAssetIfMissing(
            "Assets/Sound/100.wav",
            numberVoiceClips[2]);
        numberVoiceClips[3] = LoadEditorAssetIfMissing(
            "Assets/Sound/1000.wav",
            numberVoiceClips[3]);
        numberVoiceClips[6] = LoadEditorAssetIfMissing(
            "Assets/Sound/1000000.wav",
            numberVoiceClips[6]);
#endif
    }

#if UNITY_EDITOR
    private static T LoadEditorAssetIfMissing<T>(string path, T current)
        where T : UnityEngine.Object
    {
        return current != null
            ? current
            : UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif

    private void ConfigureAudio()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 0.2f;

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.loop = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.volume = 0.72f;

        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f;
        voiceSource.volume = 0.92f;
    }

    private void BuildDecimalSequence()
    {
        seed = CreateCenteredDisplay(powerPrefabs[0], "Seed — One", SeedDisplayScale, transform);
        seed.SetActive(true);

        for (int stageIndex = 0; stageIndex < StageCount; stageIndex++)
        {
            StageRuntime stage = new StageRuntime();
            stage.DisplayScale = CalculateDisplayScale(LogicalDimensions[stageIndex + 1]);
            stage.Group = new GameObject($"Stage {stageIndex + 1} — ten {FormatNumber(PowerValues[stageIndex])}");
            stage.Group.transform.SetParent(transform, false);
            stage.Pieces = new PieceRuntime[PiecesPerStage];

            Vector3Int layout = StageLayouts[stageIndex];
            Vector3 inputSize = LogicalDimensions[stageIndex];
            Vector3 outputSize = LogicalDimensions[stageIndex + 1];

            int pieceIndex = 0;
            for (int z = 0; z < layout.z; z++)
            {
                for (int y = 0; y < layout.y; y++)
                {
                    for (int x = 0; x < layout.x; x++)
                    {
                        GameObject pieceObject = CreateCenteredDisplay(
                            powerPrefabs[stageIndex],
                            $"{FormatNumber(PowerValues[stageIndex])} — piece {pieceIndex + 1:00}",
                            stage.DisplayScale,
                            stage.Group.transform);
                        PieceRuntime piece = new PieceRuntime
                        {
                            Root = pieceObject.transform,
                            StartPosition = GetPieceStartPosition(stageIndex, pieceIndex),
                            TargetPosition = new Vector3(
                                (-outputSize.x * 0.5f + (x + 0.5f) * inputSize.x) * stage.DisplayScale,
                                y * inputSize.y * stage.DisplayScale,
                                (-outputSize.z * 0.5f + (z + 0.5f) * inputSize.z) * stage.DisplayScale),
                            StartRotation = Quaternion.Euler(
                                18f + pieceIndex * 13f,
                                -42f + pieceIndex * 39f + stageIndex * 17f,
                                pieceIndex % 2 == 0 ? -22f : 26f)
                        };
                        piece.Root.localPosition = piece.StartPosition;
                        piece.Root.localRotation = piece.StartRotation;
                        piece.Root.localScale = Vector3.zero;
                        piece.Root.gameObject.SetActive(false);
                        stage.Pieces[pieceIndex] = piece;
                        pieceIndex++;
                    }
                }
            }

            stage.Result = CreateCenteredDisplay(
                powerPrefabs[stageIndex + 1],
                $"Result — {FormatNumber(PowerValues[stageIndex + 1])}",
                stage.DisplayScale,
                transform);
            stage.Result.SetActive(false);
            stage.Group.SetActive(false);
            stages[stageIndex] = stage;
        }
    }

    private GameObject CreateCenteredDisplay(
        GameObject prefab,
        string objectName,
        float uniformScale,
        Transform parent)
    {
        GameObject root = new GameObject(objectName);
        root.transform.SetParent(parent, false);

        GameObject visual = Instantiate(prefab, root.transform);
        visual.name = prefab.name + " Visual";
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * uniformScale;
        PrepareVisual(visual);

        Bounds bounds = BoundsOf(visual);
        visual.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        return root;
    }

    private static void PrepareVisual(GameObject visual)
    {
        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.useGravity = false;
            body.detectCollisions = false;
        }
        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
        foreach (Animator animator in visual.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
        }
        foreach (MonoBehaviour behaviour in visual.GetComponentsInChildren<MonoBehaviour>(true))
        {
            behaviour.enabled = false;
        }
        foreach (AudioSource source in visual.GetComponentsInChildren<AudioSource>(true))
        {
            source.enabled = false;
        }
    }

    private static Bounds BoundsOf(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private static float CalculateDisplayScale(Vector3 targetDimensions)
    {
        return Mathf.Min(
            MaximumDisplayWidth / targetDimensions.x,
            Mathf.Min(
                MaximumDisplayHeight / targetDimensions.y,
                MaximumDisplayDepth / targetDimensions.z));
    }

    private static Vector3 GetPieceStartPosition(int stageIndex, int pieceIndex)
    {
        if (pieceIndex == 0)
        {
            return Vector3.zero;
        }

        float angle = (pieceIndex * 137.5f + stageIndex * 31f) * Mathf.Deg2Rad;
        float radius = 6.5f + (pieceIndex % 3) * 0.45f;
        return new Vector3(
            Mathf.Cos(angle) * radius,
            2.35f + Mathf.Sin(angle) * 4.25f,
            3.8f + (pieceIndex % 4) * 0.35f);
    }

    private void BuildCelebrationSparks()
    {
        GameObject root = new GameObject("Million Celebration — 36 Rays");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = new Vector3(0f, 2.15f, 1.6f);

        for (int i = 0; i < 36; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spark.name = $"Celebration Ray {i + 1:00}";
            spark.transform.SetParent(root.transform, false);
            spark.transform.localScale = new Vector3(0.045f, 0.42f + (i % 4) * 0.08f, 0.045f);
            Renderer renderer = spark.GetComponent<Renderer>();
            renderer.sharedMaterial = sparkMaterial;
            Destroy(spark.GetComponent<Collider>());
            spark.SetActive(false);

            celebrationSparks.Add(new SparkRuntime
            {
                Transform = spark.transform,
                Angle = i * (360f / 36f) * Mathf.Deg2Rad,
                RadiusOffset = (i % 3) * 0.16f,
                Speed = 0.84f + (i % 5) * 0.07f
            });
        }
    }

    private void RenderSequence(float time)
    {
        float introProgress = Mathf.Clamp01(time / IntroDuration);
        seed.SetActive(time < IntroDuration + PreparationDuration);
        seed.transform.localPosition = Vector3.zero;
        seed.transform.localRotation = Quaternion.Euler(0f, Mathf.Sin(time * 1.8f) * 8f, 0f);
        seed.transform.localScale = Vector3.one * EaseOutBack(Mathf.Clamp01(introProgress * 1.35f));

        ActiveStage = -1;
        ActivePieceCount = 1;
        CompletedMerges = Mathf.Clamp(
            Mathf.FloorToInt((time - IntroDuration - RevealTime) / StageDuration) + 1,
            0,
            StageCount);
        MillionRevealed = CompletedMerges == StageCount;

        for (int stageIndex = 0; stageIndex < StageCount; stageIndex++)
        {
            float stageStart = IntroDuration + stageIndex * StageDuration;
            float localTime = time - stageStart;
            StageRuntime stage = stages[stageIndex];

            stage.Group.SetActive(false);
            stage.Result.SetActive(false);

            if (localTime < 0f)
            {
                continue;
            }

            if (stageIndex < StageCount - 1 &&
                localTime >= StageDuration + PreparationDuration)
            {
                continue;
            }

            if (localTime <= StageDuration || stageIndex == StageCount - 1)
            {
                ActiveStage = stageIndex;
            }

            if (localTime < PreparationDuration)
            {
                GameObject previous = stageIndex == 0 ? seed : stages[stageIndex - 1].Result;
                previous.SetActive(true);
                float previousScale = stageIndex == 0
                    ? SeedDisplayScale
                    : stages[stageIndex - 1].DisplayScale;
                float scaleRatio = stage.DisplayScale / previousScale;
                float p = Smooth01(localTime / PreparationDuration);
                previous.transform.localScale = Vector3.one * Mathf.Lerp(1f, scaleRatio, p);
                previous.transform.localRotation = Quaternion.Euler(0f, Mathf.Lerp(0f, 24f, p), 0f);
                continue;
            }

            if (localTime < RevealTime)
            {
                stage.Group.SetActive(true);
                AnimateStagePieces(stageIndex, localTime);
                continue;
            }

            ActivePieceCount = PiecesPerStage;
            stage.Result.SetActive(true);
            float revealProgress = Mathf.Clamp01((localTime - RevealTime) / 0.42f);
            float revealScale = EaseOutBack(revealProgress);
            stage.Result.transform.localScale = Vector3.one * revealScale;
            stage.Result.transform.localPosition = new Vector3(0f, Mathf.Sin(time * 2.1f) * 0.035f, 0f);
            stage.Result.transform.localRotation = Quaternion.Euler(
                0f,
                Mathf.Sin((time - RevealTime) * 0.9f) * (stageIndex == 2 || stageIndex == 5 ? 13f : 6f),
                0f);

            if (stageIndex == StageCount - 1)
            {
                AnimateCelebration(time - (stageStart + RevealTime));
            }
        }

        AnimateMergePulse(time);
    }

    private void AnimateStagePieces(int stageIndex, float localTime)
    {
        StageRuntime stage = stages[stageIndex];
        int visibleCount = 0;

        for (int i = 0; i < stage.Pieces.Length; i++)
        {
            PieceRuntime piece = stage.Pieces[i];
            float flightStart = PreparationDuration + i * PieceStagger;
            float p = Mathf.Clamp01((localTime - flightStart) / PieceFlightDuration);
            bool visible = localTime >= flightStart;
            piece.Root.gameObject.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            visibleCount++;
            float eased = Smooth01(p);
            Vector3 control = new Vector3(
                piece.StartPosition.x * 0.24f + Mathf.Sin((i + 1) * 1.7f) * 1.1f,
                5.25f + (i % 3) * 0.32f,
                -2.4f + (i % 2) * 0.7f);
            Vector3 position = QuadraticBezier(
                piece.StartPosition,
                control,
                piece.TargetPosition,
                eased);
            float coil = Mathf.Sin(p * Mathf.PI) * (0.42f + (i % 3) * 0.12f);
            position += new Vector3(
                Mathf.Cos((p * 2.5f + i) * Mathf.PI) * coil,
                Mathf.Sin((p * 2f + i * 0.2f) * Mathf.PI) * coil * 0.45f,
                0f);

            piece.Root.localPosition = position;
            piece.Root.localRotation = Quaternion.Slerp(piece.StartRotation, Quaternion.identity, eased);
            float arrivalBounce = p < 1f
                ? Mathf.Clamp01(p / 0.16f)
                : 1f + Mathf.Sin((p - 1f) * Mathf.PI * 3f) * 0.02f;
            piece.Root.localScale = Vector3.one * EaseOutBack(arrivalBounce);
        }

        ActivePieceCount = visibleCount;
    }

    private void AnimateMergePulse(float time)
    {
        bool active = false;
        for (int stageIndex = 0; stageIndex < StageCount; stageIndex++)
        {
            float local = time - (IntroDuration + stageIndex * StageDuration);
            if (local < MergeTime - 0.16f || local > RevealTime + 0.58f)
            {
                continue;
            }

            active = true;
            float p = Mathf.Clamp01((local - (MergeTime - 0.16f)) / 0.85f);
            mergePulseRig.localScale = Vector3.one * Mathf.Lerp(0.22f, 1.72f, EaseOutCubic(p));
            mergePulseRig.localRotation = Quaternion.Euler(0f, 0f, p * 62f + stageIndex * 17f);
            break;
        }

        mergePulseRig.gameObject.SetActive(active);
        if (energyLight != null)
        {
            energyLight.intensity = active ? 5.2f : 1.55f + Mathf.Sin(time * 2.3f) * 0.25f;
        }
    }

    private void AnimateCelebration(float finaleTime)
    {
        float p = Mathf.Clamp01(finaleTime / 1.3f);
        for (int i = 0; i < celebrationSparks.Count; i++)
        {
            SparkRuntime spark = celebrationSparks[i];
            spark.Transform.gameObject.SetActive(finaleTime >= i * 0.018f);
            float angle = spark.Angle + finaleTime * 0.24f * (i % 2 == 0 ? 1f : -1f);
            float radius = Mathf.Lerp(0.35f, 4.2f + spark.RadiusOffset, EaseOutCubic(p)) * spark.Speed;
            spark.Transform.localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0.3f + Mathf.Sin(angle * 2f) * 0.25f);
            spark.Transform.localRotation = Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg);
            float fadeScale = finaleTime > 2.1f
                ? Mathf.Clamp01(1f - (finaleTime - 2.1f) / 2.4f)
                : Mathf.Clamp01(finaleTime / 0.2f);
            spark.Transform.localScale = new Vector3(
                0.045f * fadeScale,
                (0.42f + (i % 4) * 0.08f) * fadeScale,
                0.045f * fadeScale);
        }
    }

    private void TriggerAudioEvents(float time)
    {
        if (!seedVoicePlayed && time >= 0.35f)
        {
            seedVoicePlayed = true;
            PlayVoice(0);
        }

        for (int stageIndex = 0; stageIndex < StageCount; stageIndex++)
        {
            float local = time - (IntroDuration + stageIndex * StageDuration);
            if (!flightSoundPlayed[stageIndex] && local >= PreparationDuration)
            {
                flightSoundPlayed[stageIndex] = true;
                if (flightClip != null)
                {
                    effectsSource.PlayOneShot(flightClip, 0.44f);
                }
            }

            if (!mergeSoundPlayed[stageIndex] && local >= RevealTime)
            {
                mergeSoundPlayed[stageIndex] = true;
                if (mergeClip != null)
                {
                    effectsSource.PlayOneShot(mergeClip, 0.72f);
                }
                if (mergeAccentClip != null)
                {
                    effectsSource.PlayOneShot(mergeAccentClip, 0.42f + stageIndex * 0.055f);
                }
                PlayVoice(stageIndex + 1);
            }
        }
    }

    private void PlayVoice(int powerIndex)
    {
        if (numberVoiceClips == null || powerIndex < 0 ||
            powerIndex >= numberVoiceClips.Length || numberVoiceClips[powerIndex] == null)
        {
            return;
        }
        voiceSource.PlayOneShot(numberVoiceClips[powerIndex], 0.9f);
    }

    private void AnimateForge(float time)
    {
        if (portalRig != null)
        {
            portalRig.localRotation = Quaternion.Euler(0f, 0f, time * 7.5f);
            for (int i = 0; i < portalRig.childCount; i++)
            {
                Transform ring = portalRig.GetChild(i);
                float direction = i % 2 == 0 ? 1f : -1f;
                ring.localRotation = Quaternion.Euler(0f, 0f, direction * time * (11f + i * 5f));
                float breathe = 1f + Mathf.Sin(time * (1.25f + i * 0.17f) + i) * 0.035f;
                ring.localScale = Vector3.one * breathe;
            }
        }
    }

    private void AnimateCamera(float time)
    {
        Vector3 desiredPosition = introCameraPosition;
        Vector3 lookTarget = new Vector3(0f, 2.12f, 0.3f);
        float shake = 0f;

        if (time >= IntroDuration)
        {
            int stageIndex = Mathf.Clamp(
                Mathf.FloorToInt((time - IntroDuration) / StageDuration),
                0,
                StageCount - 1);
            float local = time - (IntroDuration + stageIndex * StageDuration);
            Vector3 from = stageIndex == 0 ? introCameraPosition : CameraPositions[stageIndex - 1];
            float transition = Smooth01(Mathf.Clamp01(local / PreparationDuration));
            desiredPosition = Vector3.Lerp(from, CameraPositions[stageIndex], transition);

            float impactDistance = Mathf.Abs(local - RevealTime);
            if (impactDistance < 0.24f)
            {
                shake = (1f - impactDistance / 0.24f) * (0.055f + stageIndex * 0.012f);
            }

            if (stageIndex == StageCount - 1 && local >= RevealTime)
            {
                float finale = Mathf.Clamp01((local - RevealTime) / FinaleDuration);
                desiredPosition += new Vector3(-0.7f * finale, 0.32f * finale, 0.75f * finale);
            }
        }

        Vector3 noise = new Vector3(
            Mathf.Sin(time * 83f),
            Mathf.Cos(time * 71f),
            Mathf.Sin(time * 59f)) * shake;
        shotCamera.transform.position = desiredPosition + noise;
        shotCamera.transform.LookAt(lookTarget + new Vector3(noise.x * 0.2f, noise.y * 0.15f, 0f));
    }

    private void UpdateInterface(float time)
    {
        float fadeIn = Mathf.Clamp01(time / 0.55f);
        float fadeOut = Mathf.Clamp01((SequenceDuration - time) / 0.7f);
        fadeCanvasGroup.alpha = Mathf.Min(fadeIn, fadeOut);

        int visiblePower = 0;
        if (time < IntroDuration)
        {
            titleText.text = "POWER OF TEN";
            equationText.text = "1";
            geometryText.text = "ONE SMALL BLOCK";
            visiblePower = 0;
        }
        else
        {
            int stageIndex = Mathf.Clamp(
                Mathf.FloorToInt((time - IntroDuration) / StageDuration),
                0,
                StageCount - 1);
            float local = time - (IntroDuration + stageIndex * StageDuration);
            bool revealed = local >= RevealTime;
            visiblePower = revealed ? stageIndex + 1 : stageIndex;

            titleText.text = stageIndex == StageCount - 1 && revealed
                ? "ONE MILLION"
                : "TEN BECOME ONE";
            equationText.text =
                $"10 × {FormatNumber(PowerValues[stageIndex])} = " +
                $"{FormatNumber(PowerValues[stageIndex + 1])}";
            geometryText.text = revealed
                ? GeometryLabels[stageIndex]
                : $"ASSEMBLING  {Mathf.Clamp(ActivePieceCount, 1, PiecesPerStage)} / 10";
        }

        progressText.text = BuildProgressText(visiblePower);
    }

    private static string BuildProgressText(int activePower)
    {
        StringBuilder builder = new StringBuilder(180);
        for (int i = 0; i < PowerValues.Length; i++)
        {
            if (i > 0)
            {
                builder.Append("  ›  ");
            }
            if (i == activePower)
            {
                builder.Append("<color=#FFD45B><b>");
            }
            else if (i < activePower)
            {
                builder.Append("<color=#5CF2FF>");
            }
            else
            {
                builder.Append("<color=#71809E>");
            }
            builder.Append(FormatCompactNumber(PowerValues[i]));
            builder.Append("</color>");
            if (i == activePower)
            {
                builder.Append("</b>");
            }
        }
        return builder.ToString();
    }

    private static string FormatNumber(long value)
    {
        return value.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string FormatCompactNumber(long value)
    {
        if (value == 1000L) return "1K";
        if (value == 10000L) return "10K";
        if (value == 100000L) return "100K";
        if (value == 1000000L) return "1M";
        return value.ToString();
    }

    private static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float inverse = 1f - t;
        return inverse * inverse * a + 2f * inverse * t * b + t * t * c;
    }

    private static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }

    private static float EaseOutCubic(float value)
    {
        value = Mathf.Clamp01(value);
        float inverse = 1f - value;
        return 1f - inverse * inverse * inverse;
    }

    private static float EaseOutBack(float value)
    {
        value = Mathf.Clamp01(value);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float shifted = value - 1f;
        return 1f + c3 * shifted * shifted * shifted + c1 * shifted * shifted;
    }
}
