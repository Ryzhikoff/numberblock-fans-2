using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene 83: a paced aspect-responsive video where 10 repeatedly joins the current tens,
/// then 100 repeatedly joins the hundreds, until the original blocks reach 1000.
/// </summary>
public class S83_Main : MonoBehaviour
{
    public const int RoundCount = 18;
    public const int ExpectedPhysicalImpacts = RoundCount * 2;
    public const int ExpectedBurstCount = RoundCount;
    public const string OriginalFaceName = "S83 Original Face";
    public const string DeterminedFaceName = "S83 Determined Face";
    public const string SurprisedFaceName = "S83 Surprised Face";
    public const string VictoryFaceName = "S83 Victory Face";

    private static readonly int[] ProgressionValues =
    {
        10, 20, 30, 40, 50, 60, 70, 80, 90, 100,
        200, 300, 400, 500, 600, 700, 800, 900, 1000
    };

    [Header("Original blocks and scene-local face variants")]
    public GameObject onePrefab;
    public GameObject[] progressionPrefabs = new GameObject[19];

    [Header("Staging")]
    public S83_UnitBurst unitBurst;
    public TextMesh titleText;
    public TextMesh equationText;
    public TextMesh stageText;
    public Transform progressFillRoot;
    public Material flashMaterial;
    public Material dustMaterial;
    public Material crackMaterial;

    [Header("Existing project audio")]
    public AudioClip whooshClip;
    public AudioClip impactClip;
    public AudioClip impactAccentClip;
    public AudioClip explosionClip;
    public AudioClip successClip;

    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public int CollisionCount { get; private set; }
    public int CompletedRoundCount { get; private set; }
    public int BurstCount { get; private set; }
    public int LastResultValue { get; private set; }
    public int CustomExpressionShows { get; private set; }
    public bool FinalGroundCrack { get; private set; }

    private Camera shotCamera;
    private Vector3 cameraBasePosition;
    private Quaternion cameraBaseRotation;
    private float cameraBaseFov;
    private float sequenceStartTime;
    private float shakeUntil;
    private float shakeStrength;
    private int currentRound;
    private AudioSource whooshSource;
    private AudioSource effectsSource;
    private Actor finalActor;
    private readonly List<GameObject> crackPieces = new List<GameObject>(10);

    private sealed class Actor
    {
        public Transform Root;
        public GameObject Visual;
        public Renderer[] Renderers;
        public Vector3Int Grid;
        public int Value;
        public Transform OriginalFace;
        public Transform DeterminedFace;
        public Transform SurprisedFace;
        public Transform VictoryFace;
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
            Debug.LogError("[S83] Scene needs a camera tagged MainCamera.", this);
            return;
        }
        // Re-enable Unity's automatic aspect handling. A forced 9:16 projection
        // stretches the picture when Game View or Recorder switches to 16:9.
        shotCamera.ResetAspect();
        cameraBasePosition = shotCamera.transform.position;
        cameraBaseRotation = shotCamera.transform.rotation;
        cameraBaseFov = shotCamera.fieldOfView;
        ConfigureAudio();
        unitBurst.Prepare();
        if (!unitBurst.IsPrepared)
        {
            return;
        }

        SequenceComplete = false;
        CollisionCount = 0;
        CompletedRoundCount = 0;
        BurstCount = 0;
        LastResultValue = 10;
        CustomExpressionShows = 0;
        FinalGroundCrack = false;
        sequenceStartTime = Time.time;
        StartCoroutine(RunSequence());
    }

    private void Update()
    {
        if (!SequenceComplete)
        {
            SequenceTime = Mathf.Max(0f, Time.time - sequenceStartTime);
        }
        AnimateCamera();
    }

    private IEnumerator RunSequence()
    {
        yield return RunHook();

        for (int round = 0; round < RoundCount; round++)
        {
            currentRound = round;
            int currentValue = ProgressionValues[round];
            int addend = currentValue < 100 ? 10 : 100;
            int result = ProgressionValues[round + 1];
            yield return RunRound(round, currentValue, addend, result, IsMilestone(result));

            if (result == 100)
            {
                stageText.text = "TENS COMPLETE!\nNOW ADD HUNDREDS";
                yield return Wait(1.35f);
                stageText.text = string.Empty;
            }
        }

        titleText.text = "NUMBERBLOCKS 1000!";
        equationText.text = "100 + 900 = 1000";
        stageText.text = "TEN BY TEN BY TEN!";
        if (successClip != null)
        {
            effectsSource.PlayOneShot(successClip, 0.9f);
        }
        if (finalActor != null)
        {
            yield return Hop(finalActor.Root, 0.72f, 0.9f);
        }
        yield return Wait(2.8f);
        SequenceTime = Time.time - sequenceStartTime;
        SequenceComplete = true;
        Debug.Log(
            $"[S83] COMPLETE — {CompletedRoundCount} additions, {CollisionCount} physical impacts, " +
            $"{BurstCount} exact bursts, final result {LastResultValue} in {SequenceTime:0.0}s.",
            this);
    }

    private IEnumerator RunHook()
    {
        titleText.text = "CAN 10 REACH 1000?";
        equationText.text = "10  →  ?  →  1000";
        stageText.text = "COLLIDE • SCATTER • COMBINE";
        Actor ten = SpawnActor(10, 0, "Hook Numberblock 10", DisplayHeight(10));
        ten.Root.position = new Vector3(0f, 0f, 0f);
        ten.Root.localScale = Vector3.one * 0.01f;
        yield return TweenScale(ten.Root, Vector3.one, 0.68f, true);
        yield return Wait(0.72f);
        if (SetExpression(ten, DeterminedFaceName))
        {
            CustomExpressionShows++;
        }
        yield return Hop(ten.Root, 0.48f, 0.9f);
        yield return Wait(0.55f);
        Destroy(ten.Root.gameObject);
        titleText.text = "FROM 10 TO 1000";
        stageText.text = string.Empty;
    }

    private IEnumerator RunRound(
        int roundIndex,
        int currentValue,
        int addend,
        int resultValue,
        bool milestone)
    {
        Actor left = SpawnActor(
            currentValue,
            IndexForValue(currentValue),
            $"Round {roundIndex + 1:00} — {currentValue}",
            DisplayHeight(currentValue));
        Actor right = SpawnActor(
            addend,
            IndexForValue(addend),
            $"Round {roundIndex + 1:00} — Plus {addend}",
            addend == 10 ? 3.05f : 3.55f);
        Actor result = SpawnActor(
            resultValue,
            IndexForValue(resultValue),
            $"Round {roundIndex + 1:00} — Result {resultValue}",
            DisplayHeight(resultValue));

        SetVisible(result, false);
        Bounds leftBounds = BoundsOf(left);
        Bounds rightBounds = BoundsOf(right);
        float leftReadyX = -leftBounds.extents.x - 1.05f;
        float rightReadyX = rightBounds.extents.x + 1.05f;
        float leftContactX = -leftBounds.extents.x - 0.04f;
        float rightContactX = rightBounds.extents.x + 0.04f;
        Vector3 leftReady = new Vector3(leftReadyX, 0f, 0f);
        Vector3 rightReady = new Vector3(rightReadyX, 0f, 0f);

        left.Root.position = leftReady + Vector3.left * 4.6f;
        right.Root.position = rightReady + Vector3.right * 4.6f;
        equationText.text = $"{currentValue} + {addend} = ?";
        stageText.text = roundIndex < 9 ? $"TENS  {roundIndex + 1}/9" : $"HUNDREDS  {roundIndex - 8}/9";

        if (whooshClip != null)
        {
            whooshSource.PlayOneShot(whooshClip, 0.34f);
        }
        yield return TweenPair(left.Root, leftReady, right.Root, rightReady, 0.64f, true);
        yield return Wait(0.42f);

        if (SetExpression(right, DeterminedFaceName))
        {
            CustomExpressionShows++;
        }
        if (currentValue == 10 && SetExpression(left, DeterminedFaceName))
        {
            CustomExpressionShows++;
        }
        yield return ChargeWiggle(left.Root, right.Root, 0.28f);

        PlayWhoosh(1.04f);
        yield return TweenPair(
            left.Root,
            new Vector3(leftContactX, 0f, 0f),
            right.Root,
            new Vector3(rightContactX, 0f, 0f),
            0.72f,
            false);

        whooshSource.Stop();
        CollisionCount++;
        TriggerImpact(0.17f, false);
        if (SetExpression(left, SurprisedFaceName)) CustomExpressionShows++;
        if (SetExpression(right, SurprisedFaceName)) CustomExpressionShows++;
        yield return SquashPair(left.Root, right.Root, 0.17f);

        Vector3 leftRecovery = leftReady + Vector3.left * 0.5f;
        Vector3 rightRecovery = rightReady + Vector3.right * 0.5f;
        stageText.text = "AGAIN — HARDER!";
        yield return TweenPair(
            left.Root,
            leftRecovery,
            right.Root,
            rightRecovery,
            0.62f,
            true);
        SetExpression(left, OriginalFaceName);
        SetExpression(right, OriginalFaceName);
        yield return Wait(0.34f);
        if (SetExpression(right, DeterminedFaceName)) CustomExpressionShows++;
        if (currentValue == 10 && SetExpression(left, DeterminedFaceName)) CustomExpressionShows++;
        yield return ChargeWiggle(left.Root, right.Root, 0.28f);

        PlayWhoosh(0.86f);
        yield return TweenPair(
            left.Root,
            new Vector3(leftContactX, 0f, 0f),
            right.Root,
            new Vector3(rightContactX, 0f, 0f),
            0.86f,
            false);

        whooshSource.Stop();
        CollisionCount++;
        CompletedRoundCount++;
        LastResultValue = resultValue;
        MarkProgress(roundIndex);
        TriggerImpact(milestone ? 0.36f : 0.24f, milestone);
        if (SetExpression(left, SurprisedFaceName)) CustomExpressionShows++;
        if (SetExpression(right, SurprisedFaceName)) CustomExpressionShows++;
        yield return SquashPair(left.Root, right.Root, 0.19f);

        leftBounds = BoundsOf(left);
        rightBounds = BoundsOf(right);
        result.Root.position = Vector3.zero;
        Bounds resultBounds = BoundsOf(result);
        SetVisible(left, false);
        SetVisible(right, false);

        BurstCount++;
        stageText.text = $"{resultValue} ORIGINAL ONES!";
        if (explosionClip != null)
        {
            effectsSource.PlayOneShot(explosionClip, resultValue == 1000 ? 0.78f : 0.38f);
        }
        unitBurst.Begin(
            leftBounds,
            left.Grid,
            currentValue,
            rightBounds,
            right.Grid,
            addend,
            resultBounds,
            result.Grid);

        float deadline = Time.time + S83_UnitBurst.ScatterDuration +
            S83_UnitBurst.FormationDuration + 0.18f;
        while (!unitBurst.FormationComplete && Time.time < deadline)
        {
            yield return null;
        }
        unitBurst.Hide();

        SetVisible(result, true);
        if (resultValue == 1000)
        {
            if (SetExpression(result, VictoryFaceName))
            {
                CustomExpressionShows++;
            }
        }
        else if (milestone && SetExpression(result, SurprisedFaceName))
        {
            CustomExpressionShows++;
        }

        result.Root.localScale = Vector3.one * 0.08f;
        equationText.text = $"{currentValue} + {addend} = {resultValue}!";
        stageText.text = milestone ? $"MILESTONE {resultValue}!" : $"NEW TOTAL: {resultValue}";
        yield return TweenScale(result.Root, Vector3.one, 0.58f, true);
        yield return Hop(result.Root, milestone ? 0.56f : 0.38f, milestone ? 0.72f : 0.42f);
        if (resultValue == 1000)
        {
            BuildFinalCrack();
            TriggerImpact(0.42f, true);
        }
        yield return Wait(milestone ? 0.68f : 0.46f);

        Destroy(left.Root.gameObject);
        Destroy(right.Root.gameObject);
        if (resultValue == 1000)
        {
            finalActor = result;
        }
        else
        {
            Destroy(result.Root.gameObject);
        }
        yield return Wait(0.22f);
    }

    private Actor SpawnActor(int value, int prefabIndex, string objectName, float displayHeight)
    {
        GameObject prefab = progressionPrefabs[prefabIndex];
        GameObject rootObject = new GameObject(objectName);
        rootObject.transform.SetParent(transform, false);
        GameObject visual = Instantiate(prefab, rootObject.transform);
        visual.name = objectName + " Visual";
        PrepareForAnimation(visual);

        Bounds rawBounds = BoundsOf(visual.GetComponentsInChildren<Renderer>(true));
        float scale = displayHeight / Mathf.Max(0.01f, rawBounds.size.y);
        visual.transform.localScale = Vector3.one * scale;
        Bounds scaledBounds = BoundsOf(visual.GetComponentsInChildren<Renderer>(true));
        visual.transform.position -= new Vector3(
            scaledBounds.center.x,
            scaledBounds.min.y,
            scaledBounds.center.z);

        Scale scaleData = visual.GetComponentInChildren<Scale>(true);
        Vector3Int grid = scaleData != null
            ? new Vector3Int(
                Mathf.Max(1, Mathf.RoundToInt(scaleData.scale.x)),
                Mathf.Max(1, Mathf.RoundToInt(scaleData.scale.y)),
                Mathf.Max(1, Mathf.RoundToInt(scaleData.scale.z)))
            : GridForValue(value);

        Actor actor = new Actor
        {
            Root = rootObject.transform,
            Visual = visual,
            Renderers = visual.GetComponentsInChildren<Renderer>(true),
            Grid = grid,
            Value = value,
            OriginalFace = FindChildByName(visual.transform, OriginalFaceName),
            DeterminedFace = FindChildByName(visual.transform, DeterminedFaceName),
            SurprisedFace = FindChildByName(visual.transform, SurprisedFaceName),
            VictoryFace = FindChildByName(visual.transform, VictoryFaceName)
        };
        SetExpression(actor, OriginalFaceName);
        return actor;
    }

    private static void PrepareForAnimation(GameObject visual)
    {
        foreach (Animator animator in visual.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
        }
        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.useGravity = false;
        }
        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
        foreach (AudioSource audioSource in visual.GetComponentsInChildren<AudioSource>(true))
        {
            audioSource.enabled = false;
        }
    }

    private bool SetExpression(Actor actor, string expressionName)
    {
        if (actor == null)
        {
            return false;
        }
        Transform selected = expressionName == OriginalFaceName ? actor.OriginalFace :
            expressionName == DeterminedFaceName ? actor.DeterminedFace :
            expressionName == SurprisedFaceName ? actor.SurprisedFace :
            expressionName == VictoryFaceName ? actor.VictoryFace : null;
        if (selected == null)
        {
            return false;
        }

        if (actor.OriginalFace != null) actor.OriginalFace.gameObject.SetActive(false);
        if (actor.DeterminedFace != null) actor.DeterminedFace.gameObject.SetActive(false);
        if (actor.SurprisedFace != null) actor.SurprisedFace.gameObject.SetActive(false);
        if (actor.VictoryFace != null) actor.VictoryFace.gameObject.SetActive(false);
        selected.gameObject.SetActive(true);
        return true;
    }

    private static Transform FindChildByName(Transform root, string exactName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == exactName)
            {
                return child;
            }
        }
        return null;
    }

    private static void SetVisible(Actor actor, bool visible)
    {
        foreach (Renderer renderer in actor.Renderers)
        {
            renderer.enabled = visible;
        }
    }

    private static Bounds BoundsOf(Actor actor)
    {
        return BoundsOf(actor.Renderers);
    }

    private static Bounds BoundsOf(Renderer[] renderers)
    {
        bool found = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        foreach (Renderer renderer in renderers)
        {
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

    private void TriggerImpact(float strength, bool milestone)
    {
        shakeStrength = strength;
        shakeUntil = Time.time + (milestone ? 0.34f : 0.2f);
        if (impactClip != null)
        {
            effectsSource.PlayOneShot(impactClip, milestone ? 0.85f : 0.52f);
        }
        if (milestone && impactAccentClip != null)
        {
            effectsSource.PlayOneShot(impactAccentClip, 0.55f);
        }
        StartCoroutine(FlashPulse(milestone ? 3.5f : 2.1f, milestone ? 0.38f : 0.25f));
        StartCoroutine(DustBurst(milestone ? 12 : 6, milestone ? 1.25f : 0.72f));
    }

    private IEnumerator FlashPulse(float maximumSize, float duration)
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "S83 Impact Flash";
        flash.transform.position = new Vector3(0f, 2.1f, -0.35f);
        flash.GetComponent<Renderer>().sharedMaterial = flashMaterial;
        Collider collider = flash.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        float start = Time.time;
        while (Time.time - start < duration)
        {
            float t = (Time.time - start) / duration;
            float pulse = Mathf.Sin(t * Mathf.PI);
            flash.transform.localScale = Vector3.one * maximumSize * pulse;
            yield return null;
        }
        Destroy(flash);
    }

    private IEnumerator DustBurst(int count, float spread)
    {
        GameObject[] pieces = new GameObject[count];
        Vector3[] velocities = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            pieces[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pieces[i].name = "S83 Pixel Dust";
            pieces[i].transform.position = new Vector3(0f, 0.18f, -0.2f);
            pieces[i].transform.localScale = Vector3.one * Mathf.Lerp(0.08f, 0.2f, Hash01(i + CollisionCount * 31));
            pieces[i].GetComponent<Renderer>().sharedMaterial = dustMaterial;
            Collider collider = pieces[i].GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            float side = i % 2 == 0 ? -1f : 1f;
            velocities[i] = new Vector3(
                side * Mathf.Lerp(0.8f, 2.4f, Hash01(i * 7 + 3)) * spread,
                Mathf.Lerp(1.8f, 4.4f, Hash01(i * 11 + 5)),
                Mathf.Lerp(-0.5f, 0.5f, Hash01(i * 13 + 7)));
        }

        float start = Time.time;
        while (Time.time - start < 0.72f)
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < count; i++)
            {
                velocities[i] += Physics.gravity * 0.55f * dt;
                pieces[i].transform.position += velocities[i] * dt;
                pieces[i].transform.Rotate(120f * dt, 170f * dt, 80f * dt);
            }
            yield return null;
        }
        foreach (GameObject piece in pieces)
        {
            Destroy(piece);
        }
    }

    private void BuildFinalCrack()
    {
        if (FinalGroundCrack)
        {
            return;
        }
        FinalGroundCrack = true;
        for (int i = 0; i < 8; i++)
        {
            GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crack.name = $"S83 Final Crack {i + 1:00}";
            crack.transform.position = new Vector3(0f, 0.025f, i * 0.08f - 0.28f);
            crack.transform.rotation = Quaternion.Euler(0f, i * 43f + 12f, 0f);
            crack.transform.localScale = new Vector3(0.055f, 0.025f, 0.55f + i * 0.08f);
            crack.GetComponent<Renderer>().sharedMaterial = crackMaterial;
            Collider collider = crack.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            crackPieces.Add(crack);
        }
    }

    private void MarkProgress(int roundIndex)
    {
        if (progressFillRoot == null || roundIndex < 0 || roundIndex >= progressFillRoot.childCount)
        {
            return;
        }
        progressFillRoot.GetChild(roundIndex).gameObject.SetActive(true);
    }

    private void AnimateCamera()
    {
        if (shotCamera == null)
        {
            return;
        }
        float progress = Mathf.Clamp01((currentRound + 1f) / RoundCount);
        Vector3 dolly = new Vector3(0f, Mathf.Lerp(0f, 0.65f, progress), Mathf.Lerp(0f, -1.8f, progress));
        Vector3 shake = Vector3.zero;
        if (Time.time < shakeUntil)
        {
            float remaining = Mathf.Clamp01((shakeUntil - Time.time) / 0.35f);
            shake = new Vector3(
                Mathf.Sin(Time.time * 83f),
                Mathf.Cos(Time.time * 71f),
                0f) * shakeStrength * remaining;
        }
        shotCamera.transform.position = cameraBasePosition + dolly + shake;
        shotCamera.transform.rotation = cameraBaseRotation;
        shotCamera.fieldOfView = Mathf.Lerp(cameraBaseFov, cameraBaseFov - 3.5f, progress);
    }

    private void ConfigureAudio()
    {
        whooshSource = gameObject.AddComponent<AudioSource>();
        whooshSource.playOnAwake = false;
        whooshSource.spatialBlend = 0f;
        whooshSource.volume = 0.3f;
        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        effectsSource.volume = 0.88f;
    }

    private void PlayWhoosh(float pitch)
    {
        if (whooshClip == null)
        {
            return;
        }
        whooshSource.Stop();
        whooshSource.clip = whooshClip;
        whooshSource.pitch = pitch;
        whooshSource.volume = 0.3f;
        whooshSource.Play();
    }

    private bool ValidateReferences()
    {
        if (onePrefab == null || progressionPrefabs == null ||
            progressionPrefabs.Length != ProgressionValues.Length || unitBurst == null ||
            titleText == null || equationText == null || stageText == null ||
            flashMaterial == null || dustMaterial == null || crackMaterial == null)
        {
            Debug.LogError("[S83] Scene references are incomplete.", this);
            return false;
        }
        for (int i = 0; i < progressionPrefabs.Length; i++)
        {
            if (progressionPrefabs[i] == null)
            {
                Debug.LogError($"[S83] Missing progression prefab at index {i}.", this);
                return false;
            }
        }
        return true;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorAsset("Assets/Prefabs/Blocks/1.prefab", onePrefab);
        string[] paths =
        {
            "Assets/Prefabs/Blocks/Scene83/Prefabs/Ten_Expressions.prefab",
            "Assets/Prefabs/Blocks/20.prefab",
            "Assets/Prefabs/Blocks/30.prefab",
            "Assets/Prefabs/Blocks/40.prefab",
            "Assets/Prefabs/Blocks/Scene83/Prefabs/Fifty_Expressions.prefab",
            "Assets/Prefabs/Blocks/60.prefab",
            "Assets/Prefabs/Blocks/70.prefab",
            "Assets/Prefabs/Blocks/80.prefab",
            "Assets/Prefabs/Blocks/90.prefab",
            "Assets/Prefabs/Blocks/Scene83/Prefabs/Hundred_Expressions.prefab",
            "Assets/Prefabs/Blocks/200.prefab",
            "Assets/Prefabs/Blocks/300.prefab",
            "Assets/Prefabs/Blocks/400.prefab",
            "Assets/Prefabs/Blocks/Scene83/Prefabs/FiveHundred_Expressions.prefab",
            "Assets/Prefabs/Blocks/600.prefab",
            "Assets/Prefabs/Blocks/700.prefab",
            "Assets/Prefabs/Blocks/800.prefab",
            "Assets/Prefabs/Blocks/900.prefab",
            "Assets/Prefabs/Blocks/Scene83/Prefabs/Thousand_Expressions.prefab"
        };
        if (progressionPrefabs == null || progressionPrefabs.Length != paths.Length)
        {
            progressionPrefabs = new GameObject[paths.Length];
        }
        for (int i = 0; i < paths.Length; i++)
        {
            progressionPrefabs[i] = LoadEditorAsset(paths[i], progressionPrefabs[i]);
        }
#endif
    }

#if UNITY_EDITOR
    private static T LoadEditorAsset<T>(string path, T current) where T : Object
    {
        T expected = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        return expected != null ? expected : current;
    }
#endif

    private static int IndexForValue(int value)
    {
        for (int i = 0; i < ProgressionValues.Length; i++)
        {
            if (ProgressionValues[i] == value)
            {
                return i;
            }
        }
        return 0;
    }

    private static bool IsMilestone(int value)
    {
        return value == 50 || value == 100 || value == 500 || value == 1000;
    }

    private static float DisplayHeight(int value)
    {
        if (value <= 100)
        {
            return Mathf.Lerp(3.2f, 4.75f, (value - 10f) / 90f);
        }
        return Mathf.Lerp(4.8f, 6.25f, (value - 100f) / 900f);
    }

    private static Vector3Int GridForValue(int value)
    {
        if (value == 10) return new Vector3Int(2, 5, 1);
        if (value <= 100) return new Vector3Int(Mathf.Max(1, value / 10), 10, 1);
        if (value < 1000) return new Vector3Int(10, value / 10, 1);
        return new Vector3Int(10, 10, 10);
    }

    private static IEnumerator Wait(float duration)
    {
        float end = Time.time + duration;
        while (Time.time < end)
        {
            yield return null;
        }
    }

    private static IEnumerator TweenPair(
        Transform left,
        Vector3 leftTarget,
        Transform right,
        Vector3 rightTarget,
        float duration,
        bool overshoot)
    {
        Vector3 leftStart = left.position;
        Vector3 rightStart = right.position;
        float start = Time.time;
        while (Time.time - start < duration)
        {
            float t = Mathf.Clamp01((Time.time - start) / duration);
            float eased = overshoot ? EaseOutBack(t) : EaseInCubic(t);
            left.position = Vector3.LerpUnclamped(leftStart, leftTarget, eased);
            right.position = Vector3.LerpUnclamped(rightStart, rightTarget, eased);
            yield return null;
        }
        left.position = leftTarget;
        right.position = rightTarget;
    }

    private static IEnumerator TweenScale(Transform target, Vector3 endScale, float duration, bool overshoot)
    {
        Vector3 startScale = target.localScale;
        float start = Time.time;
        while (Time.time - start < duration)
        {
            float t = Mathf.Clamp01((Time.time - start) / duration);
            float eased = overshoot ? EaseOutBack(t) : EaseInOutCubic(t);
            target.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);
            yield return null;
        }
        target.localScale = endScale;
    }

    private static IEnumerator ChargeWiggle(Transform left, Transform right, float duration)
    {
        float start = Time.time;
        while (Time.time - start < duration)
        {
            float t = (Time.time - start) / duration;
            float tilt = Mathf.Sin(t * Mathf.PI * 4f) * 3f * (1f - t);
            left.localRotation = Quaternion.Euler(0f, 0f, -tilt);
            right.localRotation = Quaternion.Euler(0f, 0f, tilt);
            yield return null;
        }
        left.localRotation = Quaternion.identity;
        right.localRotation = Quaternion.identity;
    }

    private static IEnumerator SquashPair(Transform left, Transform right, float duration)
    {
        float start = Time.time;
        while (Time.time - start < duration)
        {
            float t = Mathf.Clamp01((Time.time - start) / duration);
            float squash = Mathf.Sin(t * Mathf.PI);
            left.localScale = new Vector3(1f + squash * 0.22f, 1f - squash * 0.28f, 1f);
            right.localScale = new Vector3(1f + squash * 0.22f, 1f - squash * 0.28f, 1f);
            yield return null;
        }
        left.localScale = Vector3.one;
        right.localScale = Vector3.one;
    }

    private static IEnumerator Hop(Transform target, float duration, float height)
    {
        Vector3 basePosition = target.position;
        float start = Time.time;
        while (Time.time - start < duration)
        {
            float t = Mathf.Clamp01((Time.time - start) / duration);
            target.position = basePosition + Vector3.up * Mathf.Sin(t * Mathf.PI) * height;
            target.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 2f) * 2.5f);
            yield return null;
        }
        target.position = basePosition;
        target.localRotation = Quaternion.identity;
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
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float x = Mathf.Clamp01(value) - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
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
