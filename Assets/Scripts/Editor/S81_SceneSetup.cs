using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Builds and validates Scene 81, the One-to-Million decimal forge.</summary>
public static class S81_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_81.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene81";
    private const string MaterialRoot = AssetRoot + "/Materials";

    private static readonly string[] PowerPrefabPaths =
    {
        "Assets/Prefabs/Blocks/1.prefab",
        "Assets/Prefabs/Blocks/10.prefab",
        "Assets/Prefabs/Blocks/100.prefab",
        "Assets/Prefabs/Blocks/1000.prefab",
        "Assets/Prefabs/Blocks/prefabTenThousand.prefab",
        "Assets/Prefabs/Blocks/OneHundredThousand.prefab",
        "Assets/Prefabs/Blocks/OneMillion.prefab"
    };

    private static readonly float[] CaptureTimes =
    {
        1.2f,
        5.35f,
        7.15f,
        11.25f,
        17.3f,
        24.45f,
        30.25f,
        38.1f
    };

    private static bool[] capturedFrames;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static double playModeStartTime;

    [MenuItem("Tools/Scene_81/Quick Setup Decimal Forge")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material floorMaterial = CreateEmissionMaterial(
            MaterialRoot + "/S81_Floor.mat",
            new Color(0.018f, 0.026f, 0.07f),
            new Color(0.015f, 0.035f, 0.12f));
        Material pedestalMaterial = CreateEmissionMaterial(
            MaterialRoot + "/S81_Pedestal.mat",
            new Color(0.035f, 0.065f, 0.13f),
            new Color(0.02f, 0.13f, 0.24f));
        Material cyanMaterial = CreateEmissionMaterial(
            MaterialRoot + "/S81_Cyan.mat",
            new Color(0.08f, 0.68f, 0.82f),
            new Color(0.08f, 1.4f, 2.1f));
        Material magentaMaterial = CreateEmissionMaterial(
            MaterialRoot + "/S81_Magenta.mat",
            new Color(0.72f, 0.12f, 0.73f),
            new Color(1.65f, 0.12f, 1.9f));
        Material goldMaterial = CreateEmissionMaterial(
            MaterialRoot + "/S81_Gold.mat",
            new Color(1f, 0.55f, 0.08f),
            new Color(2.6f, 0.9f, 0.08f));
        Material starMaterial = CreateEmissionMaterial(
            MaterialRoot + "/S81_Stars.mat",
            new Color(0.58f, 0.75f, 1f),
            new Color(0.75f, 1.1f, 2.2f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateCamera();
        Light energyLight = CreateLighting();
        CreateEnvironment(floorMaterial, pedestalMaterial, cyanMaterial, magentaMaterial, starMaterial);
        Transform portalRig = CreatePortalRig(cyanMaterial, magentaMaterial, goldMaterial);
        Transform mergePulseRig = CreateMergePulse(goldMaterial, cyanMaterial);

        GameObject managerObject = new GameObject("GameManager — Power of Ten Timeline");
        S81_Main director = managerObject.AddComponent<S81_Main>();
        director.powerPrefabs = new GameObject[S81_Main.PowerCount];
        for (int i = 0; i < PowerPrefabPaths.Length; i++)
        {
            director.powerPrefabs[i] = LoadAsset<GameObject>(PowerPrefabPaths[i]);
        }

        director.portalRig = portalRig;
        director.mergePulseRig = mergePulseRig;
        director.energyLight = energyLight;
        director.sparkMaterial = goldMaterial;
        CreateInterface(director, camera);
        AssignAudio(director);
        EditorUtility.SetDirty(director);
        EditorUtility.SetDirty(camera);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S81] Unity could not save the scene: {ScenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "[S81] Scene_81 created — six exact ×10 assemblies, cinematic forge, " +
            "42-second timeline, interface, effects, and existing project audio.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_81 — Power of Ten",
                "Готово. Выберите Game View 16:9 и нажмите Play. Ролик длится 42 секунды.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_81/Validate Decimal Forge")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S81_VALIDATE] Scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S81_Main director = UnityEngine.Object.FindAnyObjectByType<S81_Main>();
        if (director == null)
        {
            throw new InvalidOperationException("[S81_VALIDATE] Scene director is missing.");
        }

        if (director.powerPrefabs == null || director.powerPrefabs.Length != S81_Main.PowerCount)
        {
            throw new InvalidOperationException("[S81_VALIDATE] Seven power prefabs are required.");
        }

        for (int i = 0; i < PowerPrefabPaths.Length; i++)
        {
            if (AssetDatabase.GetAssetPath(director.powerPrefabs[i]) != PowerPrefabPaths[i])
            {
                throw new InvalidOperationException(
                    $"[S81_VALIDATE] Power prefab {i} is not {PowerPrefabPaths[i]}.");
            }

            Scale scale = director.powerPrefabs[i].GetComponent<Scale>();
            if (scale == null || scale.number != S81_Main.GetPowerValue(i))
            {
                throw new InvalidOperationException(
                    $"[S81_VALIDATE] {PowerPrefabPaths[i]} does not represent " +
                    $"{S81_Main.GetPowerValue(i):N0}.");
            }

            if (scale.scale != S81_Main.GetLogicalDimensions(i))
            {
                throw new InvalidOperationException(
                    $"[S81_VALIDATE] Unexpected dimensions on {PowerPrefabPaths[i]}: {scale.scale}.");
            }
        }

        for (int i = 0; i < S81_Main.StageCount; i++)
        {
            Vector3Int layout = S81_Main.GetStageLayout(i);
            if (layout.x * layout.y * layout.z != S81_Main.PiecesPerStage)
            {
                throw new InvalidOperationException($"[S81_VALIDATE] Stage {i + 1} is not an exact ×10 layout.");
            }

            Vector3 layoutSize = new Vector3(layout.x, layout.y, layout.z);
            Vector3 expected = Vector3.Scale(S81_Main.GetLogicalDimensions(i), layoutSize);
            if (expected != S81_Main.GetLogicalDimensions(i + 1))
            {
                throw new InvalidOperationException(
                    $"[S81_VALIDATE] Stage {i + 1} pieces do not fill the next Numberblock.");
            }
        }

        Camera camera = Camera.main;
        if (camera == null || camera.name != "Main Camera — Cinematic 16x9" ||
            Mathf.Abs(camera.fieldOfView - 38f) > 0.01f)
        {
            throw new InvalidOperationException("[S81_VALIDATE] The cinematic 16:9 camera is misconfigured.");
        }

        if (director.portalRig == null || director.portalRig.childCount != 3 ||
            director.mergePulseRig == null || director.mergePulseRig.childCount != 2 ||
            director.energyLight == null || director.sparkMaterial == null)
        {
            throw new InvalidOperationException("[S81_VALIDATE] Decimal forge effects are incomplete.");
        }

        if (director.titleText == null || director.equationText == null ||
            director.geometryText == null || director.progressText == null ||
            director.fadeCanvasGroup == null)
        {
            throw new InvalidOperationException("[S81_VALIDATE] Interface references are incomplete.");
        }

        if (director.musicClip == null || director.flightClip == null ||
            director.mergeClip == null || director.mergeAccentClip == null ||
            director.numberVoiceClips == null ||
            director.numberVoiceClips.Length != S81_Main.PowerCount ||
            director.numberVoiceClips[0] == null || director.numberVoiceClips[1] == null ||
            director.numberVoiceClips[2] == null || director.numberVoiceClips[3] == null ||
            director.numberVoiceClips[6] == null)
        {
            throw new InvalidOperationException("[S81_VALIDATE] Existing project audio is incomplete.");
        }

        if (!(S81_Main.PreparationDuration < S81_Main.MergeTime &&
              S81_Main.MergeTime < S81_Main.RevealTime &&
              S81_Main.RevealTime < S81_Main.StageDuration &&
              S81_Main.IntroDuration + S81_Main.StageCount * S81_Main.StageDuration <
              S81_Main.SequenceDuration))
        {
            throw new InvalidOperationException("[S81_VALIDATE] Timeline beats overlap or are out of order.");
        }

        Debug.Log(
            "[S81_VALIDATE] PASS — 16:9, exact 10-piece layouts, seven original " +
            "Numberblocks, six ordered merges, complete UI/effects/audio, and 1,000,000 finale.");
    }

    /// <summary>Runs the full sequence at 4× speed and captures representative story beats.</summary>
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S81_PLAY] This entry point is batch-mode only.");
        }

        ValidateScene();
        capturedFrames = new bool[CaptureTimes.Length];
        playModeStarted = false;
        playModePassed = false;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorApplication.update -= PlayModeValidationTick;
        EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        EditorApplication.update += PlayModeValidationTick;
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        EditorApplication.isPlaying = true;
    }

    private static void PlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            playModeStartTime = EditorApplication.timeSinceStartup;
            playModeStarted = true;
            Time.timeScale = 4f;
            Debug.Log("[S81_PLAY] Entered Play Mode at 4× speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;

            if (playModePassed)
            {
                Debug.Log(
                    $"[S81_PLAY] PASS — complete timeline and all {CaptureTimes.Length} keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S81_PLAY] FAIL — the sequence did not reach the Million finale.");
                EditorApplication.Exit(1);
            }
        }
    }

    private static void PlayModeValidationTick()
    {
        if (!playModeStarted || !EditorApplication.isPlaying)
        {
            return;
        }

        S81_Main director = UnityEngine.Object.FindAnyObjectByType<S81_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene81-{i + 1:00}.png");
                }
            }

            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() &&
                    director.CompletedMerges == S81_Main.StageCount &&
                    director.ActiveStage == S81_Main.StageCount - 1 &&
                    director.ActivePieceCount == S81_Main.PiecesPerStage &&
                    director.MillionRevealed;
                EditorApplication.isPlaying = false;
                return;
            }
        }

        if (EditorApplication.timeSinceStartup - playModeStartTime > 18d)
        {
            Debug.LogError("[S81_PLAY] Timed out after 18 real seconds.");
            playModePassed = false;
            EditorApplication.isPlaying = false;
        }
    }

    private static bool AllFramesCaptured()
    {
        foreach (bool captured in capturedFrames)
        {
            if (!captured)
            {
                return false;
            }
        }
        return true;
    }

    private static void CaptureRuntimeFrame(string path)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            throw new InvalidOperationException("[S81_PLAY] Main Camera disappeared.");
        }

        const int width = 960;
        const int height = 540;
        RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;

        try
        {
            camera.targetTexture = renderTexture;
            camera.aspect = (float)width / height;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = renderTexture;
            frame.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            frame.Apply();
            File.WriteAllBytes(path, frame.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(frame);
        }

        Debug.Log($"[S81_PLAY] Captured {path}");
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Cinematic 16x9", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.006f, 0.01f, 0.035f);
        camera.fieldOfView = 38f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 120f;
        camera.allowHDR = true;
        camera.allowMSAA = true;
        camera.aspect = 16f / 9f;
        camera.transform.position = new Vector3(2.55f, 3.8f, -14.9f);
        camera.transform.LookAt(new Vector3(0f, 2.12f, 0.3f));
        return camera;
    }

    private static Light CreateLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.34f, 0.38f, 0.52f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.006f, 0.012f, 0.04f);
        RenderSettings.fogDensity = 0.008f;

        GameObject keyObject = new GameObject("Key Light");
        Light key = keyObject.AddComponent<Light>();
        key.type = LightType.Directional;
        key.color = new Color(0.78f, 0.88f, 1f);
        key.intensity = 1.35f;
        key.shadows = LightShadows.Soft;
        keyObject.transform.rotation = Quaternion.Euler(43f, -31f, 0f);

        GameObject energyObject = new GameObject("Decimal Energy Light");
        Light energy = energyObject.AddComponent<Light>();
        energy.type = LightType.Point;
        energy.color = new Color(0.18f, 0.82f, 1f);
        energy.intensity = 1.55f;
        energy.range = 11f;
        energy.shadows = LightShadows.None;
        energyObject.transform.position = new Vector3(0f, 3.1f, -1.2f);
        return energy;
    }

    private static void CreateEnvironment(
        Material floorMaterial,
        Material pedestalMaterial,
        Material cyanMaterial,
        Material magentaMaterial,
        Material starMaterial)
    {
        GameObject environment = new GameObject("Decimal Forge Environment");

        CreatePrimitive(
            PrimitiveType.Cube,
            "Infinite Dark Floor",
            environment.transform,
            new Vector3(0f, -0.13f, 2f),
            new Vector3(28f, 0.24f, 24f),
            Quaternion.identity,
            floorMaterial);

        GameObject pedestal = CreatePrimitive(
            PrimitiveType.Cylinder,
            "Assembly Pedestal",
            environment.transform,
            new Vector3(0f, -0.035f, 0f),
            new Vector3(4.2f, 0.08f, 4.2f),
            Quaternion.identity,
            pedestalMaterial);
        UnityEngine.Object.DestroyImmediate(pedestal.GetComponent<Collider>());

        GameObject grid = new GameObject("Perspective Grid");
        grid.transform.SetParent(environment.transform, false);
        for (int i = -10; i <= 10; i++)
        {
            Material material = i % 5 == 0 ? magentaMaterial : cyanMaterial;
            GameObject lineX = CreatePrimitive(
                PrimitiveType.Cube,
                $"Grid X {i:+00;-00;00}",
                grid.transform,
                new Vector3(i, 0.008f, 2f),
                new Vector3(i % 5 == 0 ? 0.026f : 0.012f, 0.012f, 20f),
                Quaternion.identity,
                material);
            GameObject lineZ = CreatePrimitive(
                PrimitiveType.Cube,
                $"Grid Z {i:+00;-00;00}",
                grid.transform,
                new Vector3(0f, 0.009f, i + 3f),
                new Vector3(22f, 0.012f, i % 5 == 0 ? 0.026f : 0.012f),
                Quaternion.identity,
                material);
            UnityEngine.Object.DestroyImmediate(lineX.GetComponent<Collider>());
            UnityEngine.Object.DestroyImmediate(lineZ.GetComponent<Collider>());
        }

        GameObject starfield = new GameObject("Geometric Starfield");
        starfield.transform.SetParent(environment.transform, false);
        System.Random random = new System.Random(810081);
        for (int i = 0; i < 52; i++)
        {
            float x = Mathf.Lerp(-13f, 13f, (float)random.NextDouble());
            float y = Mathf.Lerp(0.9f, 10.5f, (float)random.NextDouble());
            float z = Mathf.Lerp(5.5f, 12f, (float)random.NextDouble());
            float size = Mathf.Lerp(0.025f, 0.085f, (float)random.NextDouble());
            GameObject star = CreatePrimitive(
                i % 4 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere,
                $"Star {i + 1:00}",
                starfield.transform,
                new Vector3(x, y, z),
                Vector3.one * size,
                Quaternion.Euler(0f, 0f, i * 17f),
                i % 7 == 0 ? magentaMaterial : starMaterial);
            UnityEngine.Object.DestroyImmediate(star.GetComponent<Collider>());
        }
    }

    private static Transform CreatePortalRig(Material cyan, Material magenta, Material gold)
    {
        GameObject rig = new GameObject("Power-of-Ten Portal — Three Rings");
        rig.transform.position = new Vector3(0f, 2.25f, 4.15f);
        CreateSegmentRing(rig.transform, "Outer Cyan Ring", 5.15f, 56, 0.055f, cyan, 0f);
        CreateSegmentRing(rig.transform, "Middle Magenta Ring", 4.4f, 48, 0.05f, magenta, 0.22f);
        CreateSegmentRing(rig.transform, "Inner Gold Ring", 3.72f, 44, 0.045f, gold, -0.18f);
        return rig.transform;
    }

    private static Transform CreateMergePulse(Material gold, Material cyan)
    {
        GameObject rig = new GameObject("Merge Pulse — Two Rings");
        rig.transform.position = new Vector3(0f, 2.2f, 1.7f);
        CreateSegmentRing(rig.transform, "Gold Merge Wave", 2.25f, 40, 0.075f, gold, 0f);
        CreateSegmentRing(rig.transform, "Cyan Merge Wave", 2.48f, 40, 0.045f, cyan, 0.08f);
        rig.SetActive(false);
        return rig.transform;
    }

    private static void CreateSegmentRing(
        Transform parent,
        string name,
        float radius,
        int segmentCount,
        float thickness,
        Material material,
        float depthOffset)
    {
        GameObject ring = new GameObject(name);
        ring.transform.SetParent(parent, false);
        ring.transform.localPosition = new Vector3(0f, 0f, depthOffset);
        float segmentLength = 2f * Mathf.PI * radius / segmentCount * 0.74f;

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = i * Mathf.PI * 2f / segmentCount;
            GameObject segment = CreatePrimitive(
                PrimitiveType.Cube,
                $"Segment {i + 1:00}",
                ring.transform,
                new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f),
                new Vector3(segmentLength, thickness, thickness),
                Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 90f),
                material);
            UnityEngine.Object.DestroyImmediate(segment.GetComponent<Collider>());
        }
    }

    private static GameObject CreatePrimitive(
        PrimitiveType type,
        string name,
        Transform parent,
        Vector3 position,
        Vector3 scale,
        Quaternion rotation,
        Material material)
    {
        GameObject primitive = GameObject.CreatePrimitive(type);
        primitive.name = name;
        primitive.transform.SetParent(parent, false);
        primitive.transform.localPosition = position;
        primitive.transform.localRotation = rotation;
        primitive.transform.localScale = scale;
        primitive.GetComponent<Renderer>().sharedMaterial = material;
        return primitive;
    }

    private static void CreateInterface(S81_Main director, Camera camera)
    {
        GameObject canvasObject = new GameObject(
            "S81 Cinematic Interface",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 0.2f;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        director.fadeCanvasGroup = canvasObject.GetComponent<CanvasGroup>();
        director.titleText = CreateText(
            canvasObject.transform,
            "Chapter Title",
            new Vector2(0.1f, 0.925f),
            new Vector2(0.9f, 0.985f),
            28f,
            new Color(0.38f, 0.94f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center);
        director.titleText.characterSpacing = 8f;

        director.equationText = CreateText(
            canvasObject.transform,
            "Power Equation",
            new Vector2(0.06f, 0.805f),
            new Vector2(0.94f, 0.925f),
            56f,
            new Color(1f, 0.84f, 0.34f),
            FontStyles.Bold,
            TextAlignmentOptions.Center);
        director.equationText.outlineColor = new Color(0.02f, 0.025f, 0.12f, 0.95f);
        director.equationText.outlineWidth = 0.26f;

        director.geometryText = CreateText(
            canvasObject.transform,
            "Assembly Geometry",
            new Vector2(0.12f, 0.085f),
            new Vector2(0.88f, 0.145f),
            27f,
            new Color(0.88f, 0.94f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center);
        director.geometryText.characterSpacing = 4f;

        director.progressText = CreateText(
            canvasObject.transform,
            "Power Progress",
            new Vector2(0.04f, 0.025f),
            new Vector2(0.96f, 0.08f),
            23f,
            Color.white,
            FontStyles.Normal,
            TextAlignmentOptions.Center);
        director.progressText.richText = true;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize,
        Color color,
        FontStyles style,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.text = string.Empty;
        SetRect(text.rectTransform, anchorMin, anchorMax);
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void AssignAudio(S81_Main director)
    {
        director.musicClip = LoadAsset<AudioClip>("Assets/Sound/Future Glider - Brian Bolger.mp3");
        director.flightClip = LoadAsset<AudioClip>("Assets/portal.mp3");
        director.mergeClip = LoadAsset<AudioClip>("Assets/Sound/collCube.wav");
        director.mergeAccentClip = LoadAsset<AudioClip>(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav");
        director.numberVoiceClips = new AudioClip[S81_Main.PowerCount];
        director.numberVoiceClips[0] = LoadAsset<AudioClip>("Assets/Resources/sound/1.wav");
        director.numberVoiceClips[1] = LoadAsset<AudioClip>("Assets/Resources/sound/10.wav");
        director.numberVoiceClips[2] = LoadAsset<AudioClip>("Assets/Sound/100.wav");
        director.numberVoiceClips[3] = LoadAsset<AudioClip>("Assets/Sound/1000.wav");
        director.numberVoiceClips[6] = LoadAsset<AudioClip>("Assets/Sound/1000000.wav");
    }

    private static Material CreateEmissionMaterial(string path, Color baseColor, Color emissionColor)
    {
        bool usesScriptableRenderPipeline = GraphicsSettings.currentRenderPipeline != null;
        Shader shader = usesScriptableRenderPipeline
            ? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")
            : Shader.Find("Standard");
        if (shader == null)
        {
            throw new InvalidOperationException("[S81] No supported Lit shader is available.");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        material.name = Path.GetFileNameWithoutExtension(path);
        material.color = baseColor;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", baseColor);
        if (material.HasProperty("_Color")) material.SetColor("_Color", baseColor);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor);
        }
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.72f);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.18f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene81");
        EnsureFolder(AssetRoot, "Materials");
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException($"[S81] Required asset is missing: {path}", path);
        }
        return asset;
    }

}
