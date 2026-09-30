using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene 88: Thousand presses x10, nine more Thousands fly in and combine into
/// Ten Thousand. Ten Thousand lands on the same button after it flips to /10,
/// returning the shot to its opening pose. There is no dialogue.
/// </summary>
public class S88_Main : MonoBehaviour
{
    public const int ThousandCount = 10;
    public const float SequenceDuration = 18.4f;
    public const float FirstPressTime = 2.85f;
    public const float SummonStartTime = 3.25f;
    public const float AssemblyCompleteTime = 7.65f;
    public const float TransformTime = 8.55f;
    public const float DivideLabelTime = 11.45f;
    public const float GiantLandingTime = 13.45f;
    public const float DivideTime = 14.15f;
    public const float ReturnStartTime = 15.15f;
    public const float ReturnCompleteTime = 18.05f;
    public const float MusicLoopDuration = 9.2f;

    [Header("Original Numberblocks")]
    public GameObject thousandPrefab;
    public GameObject tenThousandPrefab;

    [Header("Staging")]
    public float blockScale = 0.22f;
    public Transform buttonTop;
    public Transform multiplyLabel;
    public Transform divideLabel;
    public Material sparkMaterial;
    public Material flashMaterial;

    [Header("Existing project audio")]
    public AudioClip pressClip;
    public AudioClip summonClip;
    public AudioClip transformClip;
    public AudioClip divideClip;

    [Header("Background music — two exact 9.2-second cycles")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float musicVolume = 0.18f;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public int VisibleThousandCount { get; private set; }
    public bool FirstPressTriggered { get; private set; }
    public bool TransformationTriggered { get; private set; }
    public bool DivisionTriggered { get; private set; }
    public bool SummonStoppedAtTransformation { get; private set; }
    public bool MusicConfigured { get; private set; }
    public float MusicPitch { get; private set; }

    private readonly List<ThousandActor> thousands = new List<ThousandActor>(ThousandCount);
    private readonly List<Spark> sparks = new List<Spark>(48);
    private readonly List<Flash> flashes = new List<Flash>(4);

    private Camera shotCamera;
    private AudioSource effectsSource;
    private AudioSource summonSource;
    private AudioSource musicSource;
    private GameObject tenThousand;
    private Vector3 tenThousandBaseScale;
    private Vector3 tenThousandFinalPosition;
    private Vector3 thousandSize;
    private Vector3 buttonTopStartPosition;
    private Vector3 buttonTopStartScale;
    private Canvas buttonCaptionCanvas;
    private RectTransform buttonCaptionPanel;
    private TMP_Text buttonCaptionText;
    private float sequenceStartTime;
    private float shakeUntil;
    private float shakeStrength;
    private bool divideLabelShown;
    private bool divideLabelTriggered;
    private bool initialized;

    private static readonly Vector3 OpeningBottom = new Vector3(-1.3f, 0.48f, 0f);
    private static readonly Vector3 ButtonBottom = new Vector3(1.2f, 0.86f, -2.2f);
    private static readonly Vector3 StructureBottom = new Vector3(0f, 0.48f, 0f);

    private sealed class ThousandActor
    {
        public GameObject Root;
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public Vector3 BaseScale;
        public float ArrivalTime;
    }

    private sealed class Spark
    {
        public Transform Transform;
        public Vector3 Origin;
        public Vector3 Velocity;
        public float StartTime;
        public float Lifetime;
        public float Size;
    }

    private sealed class Flash
    {
        public Transform Transform;
        public Renderer Renderer;
        public Material RuntimeMaterial;
        public float StartTime;
        public float Duration;
        public float MaximumScale;
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
            Debug.LogError("[S88] Scene needs a camera tagged MainCamera.", this);
            return;
        }

        shotCamera.aspect = 9f / 16f;
        shotCamera.allowHDR = false;
        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.volume = 0.88f;
        summonSource = gameObject.AddComponent<AudioSource>();
        summonSource.playOnAwake = false;
        summonSource.spatialBlend = 0f;
        summonSource.volume = 0.58f;
        ConfigureMusic();
        buttonTopStartPosition = buttonTop.localPosition;
        buttonTopStartScale = buttonTop.localScale;
        ConfigurePhysicalButton();
        CreateButtonCaption();

        SpawnCharacters();
        if (thousands.Count != ThousandCount || tenThousand == null)
        {
            return;
        }

        SetButtonLabel("×10");
        sequenceStartTime = Time.time;
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

        AnimateButton(time);
        AnimateThousands(time);
        AnimateTenThousand(time);
        AnimateSummonAudio(time);
        TriggerTimelineEvents(time);
        AnimateSparks(time);
        AnimateFlashes(time);
        AnimateCamera(time);
        UpdateButtonCaption();

        if (time >= SequenceDuration)
        {
            ApplyOpeningPose();
            if (musicSource != null)
            {
                musicSource.Stop();
            }
            SequenceTime = SequenceDuration;
            SequenceComplete = true;
            Debug.Log(
                "[S88] COMPLETE — x10 button, ten Thousands, Ten Thousand, /10 return, " +
                "and an exact two-cycle 18.4-second music loop.",
                this);
        }
    }

    private bool ValidateReferences()
    {
        List<string> missing = new List<string>();
        if (thousandPrefab == null) missing.Add(nameof(thousandPrefab));
        if (tenThousandPrefab == null) missing.Add(nameof(tenThousandPrefab));
        if (buttonTop == null) missing.Add(nameof(buttonTop));
        if (multiplyLabel == null) missing.Add(nameof(multiplyLabel));
        if (divideLabel == null) missing.Add(nameof(divideLabel));
        if (sparkMaterial == null) missing.Add(nameof(sparkMaterial));
        if (flashMaterial == null) missing.Add(nameof(flashMaterial));
        if (musicClip == null) missing.Add(nameof(musicClip));
        if (missing.Count == 0) return true;

        Debug.LogError($"[S88] Missing scene references: {string.Join(", ", missing)}.", this);
        return false;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        thousandPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/1000.prefab",
            thousandPrefab);
        tenThousandPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/prefabTenThousand.prefab",
            tenThousandPrefab);
        sparkMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene88/Materials/S88_Spark.mat",
            sparkMaterial);
        flashMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene88/Materials/S88_Flash.mat",
            flashMaterial);
        pressClip = LoadEditorAsset("Assets/Sound/collCube.wav", pressClip);
        summonClip = LoadEditorAsset("Assets/Sound/zvuk-priblijeniya.mp3", summonClip);
        transformClip = LoadEditorAsset("Assets/Sound/boom_metal.wav", transformClip);
        divideClip = LoadEditorAsset(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav",
            divideClip);
        musicClip = LoadEditorAsset(
            "Assets/Sound/487685__gr8horizon__dragon-power-training-loop.wav",
            musicClip);
#endif
    }

#if UNITY_EDITOR
    private static T LoadEditorAsset<T>(string path, T current) where T : Object
    {
        T expected = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        return expected != null ? expected : current;
    }
#endif

    private void SpawnCharacters()
    {
        GameObject first = InstantiatePrepared(thousandPrefab, "Thousand 01 — Button Finder");
        first.transform.localScale = Vector3.one * blockScale;
        PlaceAtBottomCenter(first, OpeningBottom);
        thousandSize = BoundsOf(first).size;

        for (int i = 0; i < ThousandCount; i++)
        {
            GameObject instance = i == 0
                ? first
                : InstantiatePrepared(thousandPrefab, $"Thousand {i + 1:00} — Summoned");
            instance.transform.localScale = Vector3.one * blockScale;

            int column = i / 5;
            int row = i % 5;
            Vector3 targetBottom = StructureBottom + new Vector3(
                (column == 0 ? -0.5f : 0.5f) * thousandSize.x,
                row * thousandSize.y,
                0f);
            PlaceAtBottomCenter(instance, targetBottom);
            Vector3 targetPosition = instance.transform.position;

            Vector3 startBottom = i == 0 ? OpeningBottom : SummonStartBottom(i);
            PlaceAtBottomCenter(instance, startBottom);
            Vector3 startPosition = instance.transform.position;
            instance.SetActive(i == 0);

            thousands.Add(new ThousandActor
            {
                Root = instance,
                StartPosition = startPosition,
                TargetPosition = targetPosition,
                BaseScale = Vector3.one * blockScale,
                ArrivalTime = i == 0 ? SummonStartTime : SummonStartTime + 0.16f * i
            });
        }

        tenThousand = InstantiatePrepared(tenThousandPrefab, "Ten Thousand — x10 Result");
        tenThousand.transform.localScale = Vector3.one * blockScale;
        PlaceAtBottomCenter(tenThousand, StructureBottom);
        tenThousandBaseScale = tenThousand.transform.localScale;
        tenThousandFinalPosition = tenThousand.transform.position;
        tenThousand.SetActive(false);
        VisibleThousandCount = 1;
    }

    private Vector3 SummonStartBottom(int index)
    {
        float side = index % 2 == 0 ? -1f : 1f;
        float x = side * (4.1f + (index % 3) * 0.55f);
        float y = 2.1f + (index % 5) * 2.15f;
        float z = 0.2f + (index % 3) * 0.08f;
        return new Vector3(x, y, z);
    }

    private void AnimateButton(float time)
    {
        float thousandPress = LandingPressPulse(time, FirstPressTime, 0.34f);
        float tenThousandPress = LandingPressPulse(time, GiantLandingTime, 0.52f);
        float depression = thousandPress * 0.1f + tenThousandPress * 0.23f;
        float compression = thousandPress * 0.22f + tenThousandPress * 0.5f;
        float bulge = thousandPress * 0.035f + tenThousandPress * 0.085f;
        buttonTop.localPosition = buttonTopStartPosition + Vector3.down * depression;

        float pulse = 1f;
        if (time > 1.15f && time < FirstPressTime)
        {
            pulse += Mathf.Sin((time - 1.15f) * 8f) * 0.035f;
        }
        if (time > DivideLabelTime && time < GiantLandingTime)
        {
            pulse += Mathf.Sin((time - DivideLabelTime) * 10f) * 0.045f;
        }
        buttonTop.localScale = Vector3.Scale(
            buttonTopStartScale,
            new Vector3(pulse + bulge, 1f - compression, pulse + bulge));
    }

    private void ConfigureMusic()
    {
        if (musicClip == null)
        {
            Debug.LogError("[S88] Background music clip is missing.", this);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.loop = true;
        musicSource.clip = musicClip;
        musicSource.volume = musicVolume;
        MusicPitch = musicClip.length / MusicLoopDuration;
        musicSource.pitch = MusicPitch;
        musicSource.Play();
        MusicConfigured = true;
        Debug.Log(
            $"[S88] Music '{musicClip.name}' stretched from {musicClip.length:F3}s to " +
            $"{MusicLoopDuration:F1}s (pitch {MusicPitch:F4}) and looped exactly twice.",
            this);
    }

    private static float LandingPressPulse(float time, float landingTime, float duration)
    {
        float age = time - landingTime;
        if (age < 0f || age >= duration)
        {
            return 0f;
        }

        const float compressDuration = 0.07f;
        if (age < compressDuration)
        {
            return Mathf.SmoothStep(0f, 1f, age / compressDuration);
        }

        float release = (age - compressDuration) / (duration - compressDuration);
        return 1f - Mathf.SmoothStep(0f, 1f, release);
    }

    private void AnimateThousands(float time)
    {
        ThousandActor hero = thousands[0];
        if (time < SummonStartTime)
        {
            AnimateHeroApproach(hero, time);
            return;
        }

        if (time >= TransformTime)
        {
            foreach (ThousandActor actor in thousands)
            {
                actor.Root.SetActive(false);
            }
            VisibleThousandCount = 0;
            return;
        }

        int visible = 0;
        for (int i = 0; i < thousands.Count; i++)
        {
            ThousandActor actor = thousands[i];
            float start = actor.ArrivalTime;
            float duration = i == 0 ? 1.05f : 1.28f;
            if (time < start)
            {
                actor.Root.SetActive(i == 0);
                continue;
            }

            actor.Root.SetActive(true);
            visible++;
            float t = Mathf.Clamp01((time - start) / duration);
            float eased = EaseOutBack(Mathf.SmoothStep(0f, 1f, t));
            Vector3 assemblyStart = i == 0
                ? PositionForBottom(actor.Root, ButtonBottom)
                : actor.StartPosition;
            Vector3 position = Vector3.LerpUnclamped(assemblyStart, actor.TargetPosition, eased);
            position += Vector3.up * Mathf.Sin(t * Mathf.PI) * (i == 0 ? 1.2f : 1.75f);
            actor.Root.transform.position = position;

            float pop = i == 0 ? 1f : Mathf.Clamp01(t / 0.22f);
            float settle = t > 0.76f ? 1f + Mathf.Sin((t - 0.76f) * Mathf.PI / 0.24f) * 0.08f : 1f;
            actor.Root.transform.localScale = actor.BaseScale * pop * settle;
            actor.Root.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI) * -12f * Mathf.Sign(actor.StartPosition.x));
        }
        VisibleThousandCount = visible;

        if (time >= AssemblyCompleteTime)
        {
            float squeeze = Mathf.InverseLerp(AssemblyCompleteTime, TransformTime, time);
            float wave = Mathf.Sin(squeeze * Mathf.PI * 3f) * (1f - squeeze);
            foreach (ThousandActor actor in thousands)
            {
                actor.Root.transform.localScale = Vector3.Scale(
                    actor.BaseScale,
                    new Vector3(1f - wave * 0.06f, 1f + wave * 0.08f, 1f));
                actor.Root.transform.rotation = Quaternion.identity;
            }
        }
    }

    private void AnimateHeroApproach(ThousandActor hero, float time)
    {
        hero.Root.SetActive(true);
        hero.Root.transform.localScale = hero.BaseScale;
        hero.Root.transform.rotation = Quaternion.identity;

        if (time < 1.2f)
        {
            hero.Root.transform.position = hero.StartPosition +
                Vector3.up * Mathf.Abs(Mathf.Sin(time * 2.8f)) * 0.09f;
            return;
        }

        float t = Mathf.Clamp01((time - 1.2f) / (FirstPressTime - 1.2f));
        Vector3 buttonPosition = PositionForBottom(hero.Root, ButtonBottom);
        Vector3 position = Vector3.Lerp(hero.StartPosition, buttonPosition, Mathf.SmoothStep(0f, 1f, t));
        position += Vector3.up * Mathf.Sin(t * Mathf.PI) * 1.05f;
        hero.Root.transform.position = position;
        hero.Root.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI) * -7f);

        if (time > FirstPressTime - 0.12f)
        {
            float squash = Mathf.InverseLerp(FirstPressTime - 0.12f, FirstPressTime + 0.18f, time);
            hero.Root.transform.localScale = Vector3.Scale(
                hero.BaseScale,
                Vector3.Lerp(Vector3.one, new Vector3(1.16f, 0.78f, 1.08f), Mathf.Sin(squash * Mathf.PI)));
        }
    }

    private void AnimateTenThousand(float time)
    {
        if (time < TransformTime || time >= DivideTime)
        {
            tenThousand.SetActive(false);
            return;
        }

        tenThousand.SetActive(true);
        float reveal = Mathf.Clamp01((time - TransformTime) / 0.62f);
        float pop = EaseOutBack(reveal);
        tenThousand.transform.localScale = tenThousandBaseScale * pop;
        tenThousand.transform.position = tenThousandFinalPosition;
        AnchorAtBottomCenter(tenThousand, StructureBottom);

        if (time > 9.4f && time < 12.2f)
        {
            float bob = Mathf.Abs(Mathf.Sin((time - 9.4f) * 2.4f)) * 0.12f;
            tenThousand.transform.position += Vector3.up * bob;
            float sway = Mathf.Sin((time - 9.4f) * 2.4f) * 1.8f;
            tenThousand.transform.rotation = Quaternion.Euler(0f, 0f, sway);
        }
        else
        {
            tenThousand.transform.rotation = Quaternion.identity;
        }

        if (time >= 12.2f)
        {
            float jump = Mathf.Clamp01((time - 12.2f) / (GiantLandingTime - 12.2f));
            Vector3 landingBottom = Vector3.Lerp(
                StructureBottom,
                ButtonBottom,
                Mathf.SmoothStep(0f, 1f, jump));
            float height = Mathf.Sin(jump * Mathf.PI) * 1.25f;
            if (jump > 0.84f)
            {
                float landing = Mathf.InverseLerp(0.84f, 1f, jump);
                tenThousand.transform.localScale = Vector3.Scale(
                    tenThousandBaseScale,
                    Vector3.Lerp(Vector3.one, new Vector3(1.1f, 0.84f, 1.08f), landing));
            }
            AnchorAtBottomCenter(tenThousand, landingBottom + Vector3.up * height);
        }

        if (time >= GiantLandingTime)
        {
            float vanish = Mathf.Clamp01((time - GiantLandingTime) / (DivideTime - GiantLandingTime));
            float scale = 1f - Mathf.SmoothStep(0f, 1f, vanish);
            tenThousand.transform.localScale = tenThousandBaseScale * scale;
            AnchorAtBottomCenter(tenThousand, ButtonBottom);
        }
    }

    private void AnimateSummonAudio(float time)
    {
        if (summonSource == null || !summonSource.isPlaying)
        {
            return;
        }

        const float fadeDuration = 0.55f;
        float fade = Mathf.InverseLerp(TransformTime, TransformTime - fadeDuration, time);
        summonSource.volume = 0.58f * fade;
    }

    private void TriggerTimelineEvents(float time)
    {
        if (!FirstPressTriggered && time >= FirstPressTime)
        {
            FirstPressTriggered = true;
            Play(pressClip, 0.8f);
            if (summonClip != null)
            {
                summonSource.clip = summonClip;
                summonSource.volume = 0.58f;
                summonSource.Play();
            }
            CreateSparkBurst(PositionForBottom(thousands[0].Root, ButtonBottom), 18, FirstPressTime, 1.2f);
            CreateFlash(new Vector3(1.2f, 1.05f, -0.4f), FirstPressTime, 0.46f, 3.1f);
            StartShake(0.35f, 0.18f);
        }

        if (!TransformationTriggered && time >= TransformTime)
        {
            TransformationTriggered = true;
            if (summonSource.isPlaying)
            {
                summonSource.Stop();
            }
            summonSource.volume = 0.58f;
            SummonStoppedAtTransformation = !summonSource.isPlaying;
            Play(transformClip, 1f);
            CreateSparkBurst(new Vector3(0f, 5.8f, -0.3f), 26, TransformTime, 1.8f);
            CreateFlash(new Vector3(0f, 5.5f, -0.45f), TransformTime, 0.62f, 8.6f);
            StartShake(0.62f, 0.28f);
        }

        if (!divideLabelTriggered && time >= DivideLabelTime)
        {
            divideLabelTriggered = true;
            divideLabelShown = true;
            SetButtonLabel("÷10");
            Play(divideClip, 0.72f);
            CreateSparkBurst(new Vector3(1.2f, 0.82f, -0.5f), 12, DivideLabelTime, 0.75f);
        }

        if (!DivisionTriggered && time >= GiantLandingTime)
        {
            DivisionTriggered = true;
            Play(pressClip, 1f);
            StartShake(0.52f, 0.34f);
            CreateFlash(new Vector3(0f, 4.8f, -0.5f), GiantLandingTime, 0.72f, 9.5f);
            CreateSparkBurst(new Vector3(0f, 0.8f, -0.35f), 30, GiantLandingTime, 2.2f);
        }

        if (time >= DivideTime && time < SequenceDuration)
        {
            ThousandActor hero = thousands[0];
            hero.Root.SetActive(true);
            VisibleThousandCount = 1;
            float reveal = Mathf.Clamp01((time - DivideTime) / 0.45f);
            hero.Root.transform.position = PositionForBottom(hero.Root, ButtonBottom);
            hero.Root.transform.rotation = Quaternion.identity;
            hero.Root.transform.localScale = hero.BaseScale * EaseOutBack(reveal);

            if (time >= ReturnStartTime)
            {
                float back = Mathf.Clamp01((time - ReturnStartTime) / (ReturnCompleteTime - ReturnStartTime));
                Vector3 openingPosition = PositionForBottom(hero.Root, OpeningBottom);
                Vector3 buttonPosition = PositionForBottom(hero.Root, ButtonBottom);
                hero.Root.transform.position = Vector3.Lerp(
                    buttonPosition,
                    openingPosition,
                    Mathf.SmoothStep(0f, 1f, back));
                hero.Root.transform.position += Vector3.up * Mathf.Abs(Mathf.Sin(back * Mathf.PI * 3f)) * 0.34f * (1f - back);
                hero.Root.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(back * Mathf.PI * 3f) * 5f);

                if (back >= 0.58f && divideLabelShown)
                {
                    divideLabelShown = false;
                    SetButtonLabel("×10");
                }
            }
        }
    }

    private void AnimateCamera(float time)
    {
        Vector3 closePosition = new Vector3(0f, 4.35f, -11.5f);
        Vector3 widePosition = new Vector3(0f, 6.7f, -20.5f);
        Vector3 targetPosition;
        Vector3 lookTarget;

        if (time < SummonStartTime)
        {
            targetPosition = closePosition;
            lookTarget = new Vector3(0f, 2f, 0f);
        }
        else if (time < TransformTime)
        {
            float pull = Mathf.InverseLerp(SummonStartTime, AssemblyCompleteTime, time);
            targetPosition = Vector3.Lerp(closePosition, widePosition, Mathf.SmoothStep(0f, 1f, pull));
            lookTarget = Vector3.Lerp(new Vector3(0f, 2f, 0f), new Vector3(0f, 5f, 0f), pull);
        }
        else if (time < ReturnStartTime)
        {
            targetPosition = widePosition;
            lookTarget = new Vector3(0f, 5f, 0f);
        }
        else
        {
            float close = Mathf.InverseLerp(ReturnStartTime, ReturnCompleteTime, time);
            targetPosition = Vector3.Lerp(widePosition, closePosition, Mathf.SmoothStep(0f, 1f, close));
            lookTarget = Vector3.Lerp(new Vector3(0f, 5f, 0f), new Vector3(0f, 2f, 0f), close);
        }

        if (Time.time < shakeUntil)
        {
            float remaining = Mathf.Clamp01((shakeUntil - Time.time) / 0.7f);
            targetPosition += new Vector3(
                Mathf.Sin(Time.time * 71f),
                Mathf.Cos(Time.time * 83f),
                0f) * shakeStrength * remaining;
        }

        shotCamera.transform.SetPositionAndRotation(targetPosition, Quaternion.LookRotation(lookTarget - targetPosition));
    }

    private void CreateSparkBurst(Vector3 origin, int count, float startTime, float speed)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "S88 Multiply Spark";
            spark.transform.position = origin;
            spark.transform.localScale = Vector3.one * 0.12f;
            spark.GetComponent<Renderer>().sharedMaterial = sparkMaterial;
            Destroy(spark.GetComponent<Collider>());

            float angle = i * Mathf.PI * 2f / count + (i % 3) * 0.12f;
            float lift = 0.45f + (i % 5) * 0.12f;
            sparks.Add(new Spark
            {
                Transform = spark.transform,
                Origin = origin,
                Velocity = new Vector3(Mathf.Cos(angle) * speed, lift * speed, Mathf.Sin(angle) * 0.18f),
                StartTime = startTime,
                Lifetime = 0.58f + (i % 4) * 0.09f,
                Size = 0.09f + (i % 3) * 0.035f
            });
        }
    }

    private void AnimateSparks(float time)
    {
        foreach (Spark spark in sparks)
        {
            float age = time - spark.StartTime;
            if (age < 0f || age > spark.Lifetime)
            {
                spark.Transform.gameObject.SetActive(false);
                continue;
            }

            spark.Transform.gameObject.SetActive(true);
            spark.Transform.position = spark.Origin + spark.Velocity * age + Vector3.down * (2.2f * age * age);
            float size = spark.Size * Mathf.Sin(age / spark.Lifetime * Mathf.PI);
            spark.Transform.localScale = Vector3.one * size;
        }
    }

    private void CreateFlash(Vector3 position, float startTime, float duration, float maximumScale)
    {
        GameObject flashObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flashObject.name = "S88 Transformation Flash";
        flashObject.transform.position = position;
        flashObject.transform.rotation = Quaternion.identity;
        Destroy(flashObject.GetComponent<Collider>());
        Renderer renderer = flashObject.GetComponent<Renderer>();
        Material runtimeMaterial = new Material(flashMaterial);
        renderer.sharedMaterial = runtimeMaterial;
        flashes.Add(new Flash
        {
            Transform = flashObject.transform,
            Renderer = renderer,
            RuntimeMaterial = runtimeMaterial,
            StartTime = startTime,
            Duration = duration,
            MaximumScale = maximumScale
        });
    }

    private void AnimateFlashes(float time)
    {
        foreach (Flash flash in flashes)
        {
            float age = time - flash.StartTime;
            if (age < 0f || age > flash.Duration)
            {
                flash.Transform.gameObject.SetActive(false);
                continue;
            }

            flash.Transform.gameObject.SetActive(true);
            float t = age / flash.Duration;
            flash.Transform.localScale = Vector3.one * Mathf.Lerp(0.2f, flash.MaximumScale, Mathf.SmoothStep(0f, 1f, t));
            Color color = GetMaterialColor(flash.RuntimeMaterial);
            color.a = (1f - t) * 0.72f;
            SetMaterialColor(flash.RuntimeMaterial, color);
            flash.Renderer.sharedMaterial = flash.RuntimeMaterial;
        }
    }

    private void ApplyOpeningPose()
    {
        for (int i = 0; i < thousands.Count; i++)
        {
            ThousandActor actor = thousands[i];
            actor.Root.SetActive(i == 0);
            actor.Root.transform.localScale = actor.BaseScale;
            actor.Root.transform.rotation = Quaternion.identity;
        }
        PlaceAtBottomCenter(thousands[0].Root, OpeningBottom);
        tenThousand.SetActive(false);
        buttonTop.localPosition = buttonTopStartPosition;
        buttonTop.localScale = buttonTopStartScale;
        SetButtonLabel("×10");
        VisibleThousandCount = 1;
        Vector3 cameraPosition = new Vector3(0f, 4.35f, -11.5f);
        Vector3 lookTarget = new Vector3(0f, 2f, 0f);
        shotCamera.transform.SetPositionAndRotation(cameraPosition, Quaternion.LookRotation(lookTarget - cameraPosition));
        UpdateButtonCaption();
    }

    private void ConfigurePhysicalButton()
    {
        Transform buttonRoot = buttonTop.parent;
        if (buttonRoot != null)
        {
            Vector3 position = buttonRoot.position;
            position.z = ButtonBottom.z;
            buttonRoot.position = position;
        }

        GameObject stage = GameObject.Find("Numberblocks Stage");
        if (stage != null)
        {
            stage.transform.position = new Vector3(0f, 0.25f, 0f);
            stage.transform.localScale = new Vector3(8.8f, 0.5f, 7f);
        }

        Transform oldBadge = buttonTop.Find("Dark Symbol Badge");
        if (oldBadge != null)
        {
            oldBadge.gameObject.SetActive(false);
        }

        ConfigureTopLabel(multiplyLabel);
        ConfigureTopLabel(divideLabel);
        multiplyLabel.gameObject.SetActive(false);
        divideLabel.gameObject.SetActive(false);
    }

    private static void ConfigureTopLabel(Transform label)
    {
        label.localPosition = new Vector3(0f, 0.34f, 0f);
        label.localRotation = Quaternion.Euler(90f, 0f, 0f);
        label.localScale = Vector3.one;
    }

    private void CreateButtonCaption()
    {
        GameObject canvasObject = new GameObject("Button Mode Caption Canvas");
        buttonCaptionCanvas = canvasObject.AddComponent<Canvas>();
        buttonCaptionCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        buttonCaptionCanvas.worldCamera = shotCamera;
        buttonCaptionCanvas.planeDistance = 0.45f;
        buttonCaptionCanvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(540f, 960f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panelObject = new GameObject("Button Mode Caption — Below Physical Button");
        panelObject.transform.SetParent(canvasObject.transform, false);
        buttonCaptionPanel = panelObject.AddComponent<RectTransform>();
        buttonCaptionPanel.anchorMin = new Vector2(0.5f, 0.5f);
        buttonCaptionPanel.anchorMax = new Vector2(0.5f, 0.5f);
        buttonCaptionPanel.pivot = new Vector2(0.5f, 0.5f);
        buttonCaptionPanel.sizeDelta = new Vector2(300f, 68f);
        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.12f, 0.2f, 0.92f);
        Outline panelOutline = panelObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(1f, 1f, 1f, 0.28f);
        panelOutline.effectDistance = new Vector2(2f, -2f);

        GameObject textObject = new GameObject("Button Mode Text");
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);
        buttonCaptionText = textObject.AddComponent<TextMeshProUGUI>();
        buttonCaptionText.text = "BUTTON: ×10";
        buttonCaptionText.fontSize = 38f;
        buttonCaptionText.fontStyle = FontStyles.Bold;
        buttonCaptionText.alignment = TextAlignmentOptions.Center;
        buttonCaptionText.color = Color.white;
        buttonCaptionText.textWrappingMode = TextWrappingModes.NoWrap;
        buttonCaptionText.raycastTarget = false;
    }

    private void UpdateButtonCaption()
    {
        if (buttonCaptionCanvas == null || buttonCaptionPanel == null || shotCamera == null)
        {
            return;
        }

        RectTransform canvasRect = buttonCaptionCanvas.transform as RectTransform;
        Vector3 screenPoint = shotCamera.WorldToScreenPoint(buttonTop.position);
        if (screenPoint.z <= 0f || canvasRect == null)
        {
            buttonCaptionPanel.gameObject.SetActive(false);
            return;
        }

        buttonCaptionPanel.gameObject.SetActive(true);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                shotCamera,
                out Vector2 localPoint))
        {
            return;
        }

        localPoint.y -= 72f;
        float halfWidth = buttonCaptionPanel.sizeDelta.x * 0.5f;
        float halfHeight = buttonCaptionPanel.sizeDelta.y * 0.5f;
        Rect canvasBounds = canvasRect.rect;
        localPoint.x = Mathf.Clamp(
            localPoint.x,
            canvasBounds.xMin + halfWidth + 12f,
            canvasBounds.xMax - halfWidth - 12f);
        localPoint.y = Mathf.Clamp(
            localPoint.y,
            canvasBounds.yMin + halfHeight + 12f,
            canvasBounds.yMax - halfHeight - 12f);
        buttonCaptionPanel.anchoredPosition = localPoint;
    }

    private void SetButtonLabel(string value)
    {
        multiplyLabel.gameObject.SetActive(false);
        divideLabel.gameObject.SetActive(false);
        if (buttonCaptionText != null)
        {
            buttonCaptionText.text = value == "÷10" ? "BUTTON: ÷10" : "BUTTON: ×10";
        }
    }

    private void StartShake(float duration, float strength)
    {
        shakeUntil = Time.time + duration;
        shakeStrength = Mathf.Max(shakeStrength, strength);
    }

    private void Play(AudioClip clip, float volume)
    {
        if (clip != null) effectsSource.PlayOneShot(clip, volume);
    }

    private static float EaseOutBack(float value)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float x = value - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }

    private static GameObject InstantiatePrepared(GameObject prefab, string objectName)
    {
        GameObject instance = Instantiate(prefab);
        instance.name = objectName;
        foreach (Rigidbody body in instance.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.useGravity = false;
        }
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
        foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour != null) behaviour.enabled = false;
        }
        return instance;
    }

    private static Bounds BoundsOf(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(target.transform.position, Vector3.one);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static void PlaceAtBottomCenter(GameObject target, Vector3 bottomCenter)
    {
        Bounds bounds = BoundsOf(target);
        Vector3 currentBottom = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        target.transform.position += bottomCenter - currentBottom;
    }

    private static Vector3 PositionForBottom(GameObject target, Vector3 bottomCenter)
    {
        Vector3 original = target.transform.position;
        Bounds bounds = BoundsOf(target);
        Vector3 currentBottom = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        return original + bottomCenter - currentBottom;
    }

    private static void AnchorAtBottomCenter(GameObject target, Vector3 bottomCenter)
    {
        PlaceAtBottomCenter(target, bottomCenter);
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        return material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }
}
