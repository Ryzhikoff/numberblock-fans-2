using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Directs Scene_76, a campfire story that grows from one to one billion.</summary>
public class S76_Main : MonoBehaviour
{
    [Header("Scene-local Numberblock prefabs")]
    public GameObject onePrefab;
    public GameObject tenPrefab;
    public GameObject hundredPrefab;
    public GameObject thousandPrefab;
    public GameObject tenThousandPrefab;
    public GameObject hundredThousandPrefab;
    public GameObject millionPrefab;
    public GameObject tenMillionPrefab;
    public GameObject hundredMillionPrefab;
    public GameObject billionPrefab;

    [Header("Dialogue (001.wav through 023.wav)")]
    public AudioClip[] dialogueClips;

    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI mathText;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI creditText;
    public CanvasGroup fadeCanvasGroup;

    [Header("Timing")]
    public float openingFadeDuration = 1.5f;
    public float cameraMoveDuration = 0.45f;
    public float dialoguePadding = 0.28f;
    public float interSceneFadeDuration = 0.75f;
    public float titleHoldDuration = 2.2f;

    [Header("Framing")]
    public float campCharacterHeight = 4.4f;
    public float closeUpPadding = 1.45f;
    public float groupPadding = 1.32f;

    private static readonly string[] SpeakerNames =
    {
        "ONE HUNDRED THOUSAND", "TEN THOUSAND", "ONE THOUSAND",
        "ONE HUNDRED THOUSAND", "ONE THOUSAND", "TEN THOUSAND",
        "ONE THOUSAND", "ONE HUNDRED THOUSAND", "ONE THOUSAND",
        "ONE HUNDRED THOUSAND", "ONE THOUSAND", "ONE HUNDRED THOUSAND",
        "ONE THOUSAND", "ONE MILLION", "ONE THOUSAND", "ONE MILLION",
        "ONE THOUSAND", "ONE THOUSAND", "TEN MILLION",
        "ONE HUNDRED MILLION", "ONE BILLION", "ONE THOUSAND", "ONE THOUSAND"
    };

    private static readonly string[] DialogueText =
    {
        "What a beautiful night. Hello, One Thousand. Hello, Ten Thousand!",
        "Hello, One Hundred Thousand!",
        "Look at all those stars! There must be one hundred thousand.",
        "Many more. Big numbers are perfect for big thoughts.",
        "If I join you, we make one hundred and one thousand!",
        "And I can add ten thousand at a time.",
        "One hundred and ten thousand... one hundred and twenty thousand... amazing!",
        "Hundred thousands can grow too: one hundred thousand, two hundred thousand, three hundred thousand.",
        "Then what comes after nine hundred and ninety-nine thousand, nine hundred and ninety-nine?",
        "Start with One. Ten Ones make Ten.",
        "Ten Tens make One Hundred!",
        "Ten Hundreds make One Thousand.",
        "A thousand Thousands make...",
        "One Million!",
        "You look like a giant me!",
        "That is because I am one million Ones.",
        "Good night, One Million... if my big thoughts let me sleep.",
        "One dream. Ten Ones, ten Tens, ten Hundreds... bigger and bigger!",
        "Ten Million!",
        "One Hundred Million!",
        "One Billion. Hello, little One Thousand.",
        "Numbers can always grow. They go on forever and ever!",
        "Good night, Numberland."
    };

    private readonly List<GameObject> temporaryActors = new List<GameObject>();

    private Camera shotCamera;
    private AudioSource dialogueSource;
    private GameObject thousand;
    private GameObject tenThousand;
    private GameObject hundredThousand;
    private GameObject stageActor;
    private GameObject stagePartner;
    private Transform campfireFlame;
    private Vector3 campfireFlameScale;
    private bool batchErrorDetected;
    private bool batchSequenceComplete;

    private IEnumerator Start()
    {
        if (Application.isBatchMode)
        {
            Time.timeScale = 12f;
            StartCoroutine(BatchWatchdog());
        }

        ResolveEditorReferences();
        if (!ValidateReferences())
        {
            yield break;
        }

        shotCamera = Camera.main;
        if (shotCamera == null)
        {
            Debug.LogError("Scene 76 needs a camera tagged MainCamera.", this);
            yield break;
        }

        dialogueSource = gameObject.AddComponent<AudioSource>();
        dialogueSource.playOnAwake = false;
        dialogueSource.spatialBlend = 0f;

        GameObject flame = GameObject.Find("Campfire Flame");
        if (flame != null)
        {
            campfireFlame = flame.transform;
            campfireFlameScale = campfireFlame.localScale;
        }

        PrepareUi();
        SpawnCampCharacters();
        FrameNow(CombinedBounds(thousand, tenThousand, hundredThousand), groupPadding);

        yield return FadeTo(0f, openingFadeDuration);
        yield return ShowTitle("ONE MILLION\nAND ONE THOUSAND", titleHoldDuration);

        yield return Speak(0, hundredThousand);
        yield return Speak(1, tenThousand);
        mathText.text = "★  ★  ★  ···  ?";
        yield return Speak(2, thousand);
        yield return Speak(3, hundredThousand);

        mathText.text = "100 000 + 1 000 = 101 000";
        yield return Speak(4, thousand);
        mathText.text = "100 000 + 10 000 = 110 000";
        yield return Speak(5, tenThousand);
        mathText.text = "110 000 + 10 000 = 120 000";
        yield return Speak(6, thousand);
        mathText.text = "100 000 → 200 000 → 300 000 → …";
        yield return Speak(7, hundredThousand);
        mathText.text = "999 999 + 1 = ?";
        yield return Speak(8, thousand);

        yield return FadeTo(1f, interSceneFadeDuration);
        SetCampActive(false);
        shotCamera.backgroundColor = new Color(0.025f, 0.03f, 0.12f);
        yield return ShowTitle("A BIG-NUMBER LESSON", 1.35f);
        yield return FadeTo(0f, interSceneFadeDuration);

        stageActor = ShowStageNumber(onePrefab, "One (Lesson)", 2.7f);
        mathText.text = "10 × 1 = 10";
        yield return Speak(9, stageActor);

        ReplaceStageActor(tenPrefab, "Ten (Lesson)", 3.2f);
        mathText.text = "10 × 10 = 100";
        yield return Speak(10, stageActor);

        ReplaceStageActor(hundredPrefab, "One Hundred (Lesson)", 3.7f);
        mathText.text = "10 × 100 = 1 000";
        yield return Speak(11, stageActor);

        ReplaceStageActor(thousandPrefab, "One Thousand (Lesson)", 4.25f);
        mathText.text = "1 000 × 1 000 = ?";
        yield return Speak(12, stageActor);

        ReplaceStageActor(millionPrefab, "One Million", 6.4f);
        mathText.text = "1 000 × 1 000 = 1 000 000";
        yield return Speak(13, stageActor);

        stagePartner = ShowStageNumber(thousandPrefab, "One Thousand (Meeting)", 3.25f, -3.8f);
        stageActor.transform.position = GroundAt(stageActor, new Vector3(2.3f, 0f, 2.5f));
        FrameNow(CombinedBounds(stagePartner, stageActor), groupPadding);
        yield return Speak(14, stagePartner);
        yield return Speak(15, stageActor);
        yield return Speak(16, stagePartner);

        yield return FadeTo(1f, interSceneFadeDuration);
        ClearTemporaryActors();
        mathText.text = string.Empty;
        yield return ShowTitle("ONE THOUSAND'S DREAM", 1.45f);
        yield return FadeTo(0f, interSceneFadeDuration);

        stageActor = ShowStageNumber(thousandPrefab, "One Thousand (Dreaming)", 4.1f);
        mathText.text = "1 → 10 → 100 → 1 000 → 1 000 000";
        yield return Speak(17, stageActor);

        ReplaceStageActor(tenMillionPrefab, "Ten Million", 5.2f);
        mathText.text = "10 × 1 000 000 = 10 000 000";
        yield return Speak(18, stageActor);

        ReplaceStageActor(hundredMillionPrefab, "One Hundred Million", 6.2f);
        mathText.text = "10 × 10 000 000 = 100 000 000";
        yield return Speak(19, stageActor);

        ReplaceStageActor(billionPrefab, "One Billion", 8f);
        mathText.text = "10 × 100 000 000 = 1 000 000 000";
        yield return Speak(20, stageActor);

        stagePartner = ShowStageNumber(thousandPrefab, "One Thousand (Dream)", 2.45f, -4.8f);
        stageActor.transform.position = GroundAt(stageActor, new Vector3(2.1f, 0f, 3f));
        FrameNow(CombinedBounds(stagePartner, stageActor), groupPadding);
        yield return Speak(21, stagePartner);

        yield return FadeTo(1f, interSceneFadeDuration);
        ClearTemporaryActors();
        SetCampActive(true);
        mathText.text = string.Empty;
        shotCamera.backgroundColor = new Color(0.035f, 0.045f, 0.13f);
        FrameNow(CombinedBounds(thousand, tenThousand, hundredThousand), groupPadding);
        yield return FadeTo(0f, interSceneFadeDuration);
        yield return Speak(22, thousand);

        speakerText.text = string.Empty;
        subtitleText.text = string.Empty;
        mathText.text = "NUMBERS GO ON FOREVER ∞";
        creditText.text = "Inspired by ZaxanerH's fan script\nOne Million and One Thousand";
        creditText.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);
        yield return FadeTo(1f, 1.6f);

        if (Application.isBatchMode)
        {
            batchSequenceComplete = true;
            int exitCode = batchErrorDetected ? 2 : 0;
            if (batchErrorDetected)
            {
                Debug.LogError("[S76_PLAYTEST] FAIL — the full accelerated sequence completed with logged errors.");
            }
            else
            {
                Debug.Log("[S76_PLAYTEST] PASS — the full accelerated sequence reached the final fade.");
            }
            ExitBatchMode(exitCode);
        }
    }

    private void OnEnable()
    {
        if (Application.isBatchMode)
        {
            Application.logMessageReceived += HandleBatchLog;
        }
    }

    private void OnDisable()
    {
        if (Application.isBatchMode)
        {
            Application.logMessageReceived -= HandleBatchLog;
        }
    }

    private void Update()
    {
        if (campfireFlame == null)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * 8.7f) * 0.08f + Mathf.Sin(Time.time * 13.1f) * 0.035f;
        campfireFlame.localScale = new Vector3(
            campfireFlameScale.x * (2f - pulse),
            campfireFlameScale.y * pulse,
            campfireFlameScale.z * (2f - pulse));
    }

    private IEnumerator BatchWatchdog()
    {
        yield return new WaitForSecondsRealtime(30f);
        if (!batchSequenceComplete)
        {
            Debug.LogError("[S76_PLAYTEST] FAIL — accelerated playback did not reach the final fade in 30 seconds.");
            ExitBatchMode(3);
        }
    }

    private void HandleBatchLog(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            batchErrorDetected = true;
        }
    }

    private static void ExitBatchMode(int exitCode)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.Exit(exitCode);
#else
        Application.Quit(exitCode);
#endif
    }

    private bool ValidateReferences()
    {
        GameObject[] requiredPrefabs =
        {
            onePrefab, tenPrefab, hundredPrefab, thousandPrefab, tenThousandPrefab,
            hundredThousandPrefab, millionPrefab, tenMillionPrefab, hundredMillionPrefab, billionPrefab
        };

        for (int i = 0; i < requiredPrefabs.Length; i++)
        {
            if (requiredPrefabs[i] == null)
            {
                Debug.LogError($"Scene 76 is missing Numberblock prefab at index {i}.", this);
                return false;
            }
        }

        if (dialogueClips == null || dialogueClips.Length != DialogueText.Length)
        {
            Debug.LogWarning("Scene 76 expects 23 dialogue clips; subtitles will still play for missing clips.", this);
        }

        return true;
    }

    private void PrepareUi()
    {
        titleText.text = string.Empty;
        mathText.text = string.Empty;
        speakerText.text = string.Empty;
        subtitleText.text = string.Empty;
        creditText.text = string.Empty;
        creditText.gameObject.SetActive(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }
    }

    private void SpawnCampCharacters()
    {
        thousand = SpawnNormalized(thousandPrefab, new Vector3(-4.4f, 0f, 2.8f), campCharacterHeight, "One Thousand");
        tenThousand = SpawnNormalized(tenThousandPrefab, new Vector3(0f, 0f, 3.35f), campCharacterHeight * 1.08f, "Ten Thousand");
        hundredThousand = SpawnNormalized(hundredThousandPrefab, new Vector3(4.5f, 0f, 2.8f), campCharacterHeight * 1.2f, "One Hundred Thousand");
    }

    private IEnumerator Speak(int cueIndex, GameObject speaker)
    {
        if (speaker != null && speaker.activeInHierarchy)
        {
            yield return MoveCameraTo(BoundsOf(speaker), closeUpPadding, cameraMoveDuration);
        }

        speakerText.text = SpeakerNames[cueIndex];
        subtitleText.text = DialogueText[cueIndex];

        AudioClip clip = dialogueClips != null && cueIndex < dialogueClips.Length
            ? dialogueClips[cueIndex]
            : null;
        if (clip != null)
        {
            dialogueSource.clip = clip;
            dialogueSource.Play();
        }

        float duration = clip != null
            ? clip.length + dialoguePadding
            : Mathf.Max(1.8f, DialogueText[cueIndex].Length * 0.055f);
        yield return AnimateSpeaker(speaker, duration);

        if (dialogueSource.isPlaying)
        {
            dialogueSource.Stop();
        }
    }

    private IEnumerator AnimateSpeaker(GameObject speaker, float duration)
    {
        if (speaker == null)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        Vector3 basePosition = speaker.transform.position;
        Transform face = FindFaceOverlay(speaker);
        Vector3 faceScale = face != null ? face.localScale : Vector3.one;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float talk = Mathf.Abs(Mathf.Sin(elapsed * 8.5f));
            speaker.transform.position = basePosition + Vector3.up * talk * 0.045f;
            speaker.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(elapsed * 4.25f) * 0.8f);
            if (face != null)
            {
                face.localScale = Vector3.Scale(faceScale, new Vector3(1f + talk * 0.018f, 1f - talk * 0.035f, 1f));
            }
            yield return null;
        }

        speaker.transform.position = basePosition;
        speaker.transform.localRotation = Quaternion.identity;
        if (face != null)
        {
            face.localScale = faceScale;
        }
    }

    private GameObject ShowStageNumber(GameObject prefab, string objectName, float targetHeight, float x = 0f)
    {
        GameObject instance = SpawnNormalized(prefab, new Vector3(x, 0f, 2.5f), targetHeight, objectName);
        temporaryActors.Add(instance);
        FrameNow(BoundsOf(instance), closeUpPadding);
        return instance;
    }

    private void ReplaceStageActor(GameObject prefab, string objectName, float targetHeight)
    {
        if (stageActor != null)
        {
            temporaryActors.Remove(stageActor);
            Destroy(stageActor);
        }

        stageActor = ShowStageNumber(prefab, objectName, targetHeight);
    }

    private void SetCampActive(bool active)
    {
        thousand.SetActive(active);
        tenThousand.SetActive(active);
        hundredThousand.SetActive(active);
    }

    private void ClearTemporaryActors()
    {
        foreach (GameObject actor in temporaryActors)
        {
            if (actor != null)
            {
                Destroy(actor);
            }
        }

        temporaryActors.Clear();
        stageActor = null;
        stagePartner = null;
    }

    private GameObject SpawnNormalized(GameObject prefab, Vector3 groundPosition, float targetHeight, string objectName)
    {
        GameObject instance = Instantiate(prefab, groundPosition, Quaternion.identity, transform);
        instance.name = objectName;
        PrepareForAnimation(instance);

        Bounds initialBounds = BoundsOf(instance);
        float scale = targetHeight / Mathf.Max(0.001f, initialBounds.size.y);
        instance.transform.localScale *= scale;
        instance.transform.position = GroundAt(instance, groundPosition);
        return instance;
    }

    private static Vector3 GroundAt(GameObject target, Vector3 groundPosition)
    {
        target.transform.position = groundPosition;
        Bounds bounds = BoundsOf(target);
        target.transform.position += Vector3.up * (groundPosition.y - bounds.min.y);
        return target.transform.position;
    }

    private static void PrepareForAnimation(GameObject target)
    {
        foreach (Rigidbody body in target.GetComponentsInChildren<Rigidbody>())
        {
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        foreach (Collider collider in target.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
    }

    private IEnumerator ShowTitle(string value, float holdDuration)
    {
        titleText.text = value;
        titleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(holdDuration);
        titleText.gameObject.SetActive(false);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.01f;
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration)));
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    private IEnumerator MoveCameraTo(Bounds bounds, float padding, float duration)
    {
        Vector3 startPosition = shotCamera.transform.position;
        Quaternion startRotation = shotCamera.transform.rotation;
        Vector3 targetPosition = Frame(bounds, padding);
        Quaternion targetRotation = Quaternion.LookRotation(bounds.center - targetPosition);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration)));
            shotCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            shotCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        shotCamera.transform.position = targetPosition;
        shotCamera.transform.rotation = targetRotation;
    }

    private Vector3 Frame(Bounds bounds, float padding)
    {
        float verticalHalf = bounds.extents.y * padding;
        float horizontalHalf = bounds.extents.x * padding / Mathf.Max(0.01f, shotCamera.aspect);
        float requiredHalf = Mathf.Max(verticalHalf, horizontalHalf);
        float distance = requiredHalf / Mathf.Tan(shotCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        return bounds.center + Vector3.back * (distance + bounds.extents.z + 0.8f);
    }

    private void FrameNow(Bounds bounds, float padding)
    {
        shotCamera.transform.position = Frame(bounds, padding);
        shotCamera.transform.LookAt(bounds.center);
    }

    private static Bounds CombinedBounds(params GameObject[] targets)
    {
        bool initialized = false;
        Bounds combined = new Bounds(Vector3.zero, Vector3.one);

        foreach (GameObject target in targets)
        {
            if (target == null || !target.activeInHierarchy)
            {
                continue;
            }

            Bounds targetBounds = BoundsOf(target);
            if (!initialized)
            {
                combined = targetBounds;
                initialized = true;
            }
            else
            {
                combined.Encapsulate(targetBounds);
            }
        }

        return combined;
    }

    private static Bounds BoundsOf(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
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

    private static Transform FindFaceOverlay(GameObject target)
    {
        foreach (Transform child in target.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "S76 Original Face Overlay")
            {
                return child;
            }
        }
        return null;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/One_Dream.prefab", onePrefab);
        tenPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/Ten_Dream.prefab", tenPrefab);
        hundredPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/Hundred_Dream.prefab", hundredPrefab);
        thousandPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/Thousand_Curious.prefab", thousandPrefab);
        tenThousandPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/TenThousand_Friendly.prefab", tenThousandPrefab);
        hundredThousandPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/HundredThousand_Wise.prefab", hundredThousandPrefab);
        millionPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/Million_Dream.prefab", millionPrefab);
        tenMillionPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/TenMillion_Dream.prefab", tenMillionPrefab);
        hundredMillionPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/HundredMillion_Dream.prefab", hundredMillionPrefab);
        billionPrefab = LoadEditorPrefab("Assets/Prefabs/Blocks/Scene76/Billion_Dream.prefab", billionPrefab);
#endif
    }

#if UNITY_EDITOR
    private static GameObject LoadEditorPrefab(string path, GameObject current)
    {
        GameObject expected = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return expected != null ? expected : current;
    }
#endif
}
