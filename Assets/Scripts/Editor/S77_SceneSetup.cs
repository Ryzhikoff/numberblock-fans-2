using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the complete 8:30 Scene_77 obstacle-course episode.</summary>
public static class S77_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_77.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene77";
    private const string TextureRoot = AssetRoot + "/FaceTextures";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string PrefabRoot = AssetRoot + "/Prefabs";

    private const string OneSourcePath = "Assets/Prefabs/Blocks/1.prefab";
    private const string TenSourcePath = "Assets/Prefabs/Blocks/10.prefab";
    private const string HundredSourcePath = "Assets/Prefabs/Blocks/100.prefab";
    private const string OriginalFaceMaterialPath = "Assets/Materials/Materials/11.mat";
    private const string CleanBackMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/newMillion 1.mat";

    private const string SurprisedTexturePath = TextureRoot + "/Hundred_Surprised.png";
    private const string DeterminedTexturePath = TextureRoot + "/Hundred_Determined.png";
    private const string SadTexturePath = TextureRoot + "/Hundred_Sad.png";
    private const string SurprisedMaterialPath =
        MaterialRoot + "/Hundred_Surprised_Transparent.mat";
    private const string DeterminedMaterialPath =
        MaterialRoot + "/Hundred_Determined_Transparent.mat";
    private const string SadMaterialPath =
        MaterialRoot + "/Hundred_Sad_Transparent.mat";
    private const string HundredVariantPath = PrefabRoot + "/Hundred_DoorRunner.prefab";

    private const string FrontBodyName = "S77 Front Body";
    private const string FaceOverlayName = "S77 Face Overlay";

    private static readonly float[] CaptureTimes =
    {
        12f, 37f, 58f, 79f, 97f, 104f, 114f, 129f, 141f, 157f,
        163f, 175f, 186f, 189f, 217f, 227f, 240f, 268f, 282f, 305f,
        327f, 342f, 370f, 402f, 412f, 428f, 443f, 452f, 480f, 500f
    };

    private static double playModeStartTime;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static bool[] capturedFrames;

    [MenuItem("Tools/Scene_77/Quick Setup Long Episode")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        ConfigureFaceTexture(SurprisedTexturePath);
        ConfigureFaceTexture(DeterminedTexturePath);
        ConfigureFaceTexture(SadTexturePath);

        Material surprisedMaterial = CreateTransparentFaceMaterial(
            SurprisedMaterialPath,
            LoadAsset<Texture2D>(SurprisedTexturePath));
        Material determinedMaterial = CreateTransparentFaceMaterial(
            DeterminedMaterialPath,
            LoadAsset<Texture2D>(DeterminedTexturePath));
        Material sadMaterial = CreateTransparentFaceMaterial(
            SadMaterialPath,
            LoadAsset<Texture2D>(SadTexturePath));
        CreateHundredVariant(surprisedMaterial);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        CreateEnvironment();
        CreateEpisodeText(
            out TextMesh title,
            out TextMesh caption,
            out TextMesh[] titleOutline,
            out TextMesh[] captionOutline);
        GameObject[] obstacles = CreateObstacles(
            out Transform spinner,
            out Transform seesaw,
            out Transform bridge);

        GameObject gameManager = new GameObject("GameManager — 8m30s Timeline");
        S77_Main director = gameManager.AddComponent<S77_Main>();
        director.onePrefab = LoadAsset<GameObject>(OneSourcePath);
        director.tenPrefab = LoadAsset<GameObject>(TenSourcePath);
        director.hundredPrefab = LoadAsset<GameObject>(HundredVariantPath);
        director.cleanBackMaterial = LoadAsset<Material>(CleanBackMaterialPath);
        director.surprisedFaceMaterial = surprisedMaterial;
        director.determinedFaceMaterial = determinedMaterial;
        director.sadFaceMaterial = sadMaterial;
        director.obstacleRoots = obstacles;
        director.spinnerBars = spinner;
        director.seesawDeck = seesaw;
        director.liftingBridge = bridge;
        director.stageTitle = title;
        director.caption = caption;
        director.stageTitleOutline = titleOutline;
        director.captionOutline = captionOutline;

        director.musicClip = LoadOptional<AudioClip>(
            "Assets/Sound/Jungle Trip - Quincas Moreira.mp3");
        director.impactClip = LoadOptional<AudioClip>("Assets/Sound/collCube.wav");
        director.splitClip = LoadOptional<AudioClip>("Assets/Sound/destroy_blocks.mp3");
        director.successClip = LoadOptional<AudioClip>(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav");

        EditorUtility.SetDirty(director);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S77] Unity could not save the scene: {ScenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "[S77] Scene_77 rebuilt as a horizontal 8:30 episode with seven obstacles, " +
            "original One/Ten/Hundred assets, and three transparent Hundred expressions.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_77 — Long obstacle course",
                "Готово: та же Scene_77 теперь рассчитана на 8:30. " +
                "Выберите Game View 16:9 и нажмите Play.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_77/Validate Long Episode")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S77_VALIDATE] Scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S77_Main director = UnityEngine.Object.FindAnyObjectByType<S77_Main>();
        if (director == null)
        {
            throw new InvalidOperationException("[S77_VALIDATE] Scene has no S77_Main director.");
        }

        if (AssetDatabase.GetAssetPath(director.onePrefab) != OneSourcePath ||
            AssetDatabase.GetAssetPath(director.tenPrefab) != TenSourcePath ||
            AssetDatabase.GetAssetPath(director.hundredPrefab) != HundredVariantPath)
        {
            throw new InvalidOperationException(
                "[S77_VALIDATE] One, Ten, or the scene-local Hundred uses the wrong prefab.");
        }

        if (director.cleanBackMaterial != LoadAsset<Material>(CleanBackMaterialPath))
        {
            throw new InvalidOperationException(
                "[S77_VALIDATE] Hundred does not preserve its clean original back texture.");
        }

        if (director.obstacleRoots == null || director.obstacleRoots.Length != 7)
        {
            throw new InvalidOperationException("[S77_VALIDATE] Exactly seven obstacle roots are required.");
        }

        for (int i = 0; i < director.obstacleRoots.Length; i++)
        {
            if (director.obstacleRoots[i] == null)
            {
                throw new InvalidOperationException($"[S77_VALIDATE] Obstacle {i + 1} is missing.");
            }
        }

        if (director.stageTitle == null || director.caption == null ||
            director.stageTitleOutline == null || director.stageTitleOutline.Length != 8 ||
            director.captionOutline == null || director.captionOutline.Length != 8 ||
            director.spinnerBars == null || director.seesawDeck == null ||
            director.liftingBridge == null)
        {
            throw new InvalidOperationException(
                "[S77_VALIDATE] Text or animated obstacle references are incomplete.");
        }

        ValidateFaceTexture(SurprisedTexturePath);
        ValidateFaceTexture(DeterminedTexturePath);
        ValidateFaceTexture(SadTexturePath);
        ValidateTransparentMaterial(director.surprisedFaceMaterial, SurprisedTexturePath);
        ValidateTransparentMaterial(director.determinedFaceMaterial, DeterminedTexturePath);
        ValidateTransparentMaterial(director.sadFaceMaterial, SadTexturePath);
        ValidateHundredVariant();

        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic ||
            Mathf.Abs(camera.orthographicSize - 5.55f) > 0.01f)
        {
            throw new InvalidOperationException(
                "[S77_VALIDATE] The horizontal orthographic camera is missing or misconfigured.");
        }

        if (Mathf.Abs(S77_Main.EpisodeDuration - 510f) > 0.001f)
        {
            throw new InvalidOperationException("[S77_VALIDATE] Episode duration must be exactly 510 seconds.");
        }

        Debug.Log(
            "[S77_VALIDATE] PASS — 510.000s (8:30), 16:9 camera, seven obstacles, original " +
            "One and 2x5 Ten prefabs, scene-local Hundred, clean back texture, and three RGBA faces.");
    }

    /// <summary>Accelerates the full 8:30 episode to about 26 seconds and captures every chapter.</summary>
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S77_PLAY] This validation entry point is batch-mode only.");
        }

        ValidateScene();
        playModeStarted = false;
        playModePassed = false;
        capturedFrames = new bool[CaptureTimes.Length];
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
            Time.timeScale = 20f;
            Debug.Log("[S77_PLAY] Entered Play Mode at 20x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            if (playModePassed)
            {
                Debug.Log(
                    $"[S77_PLAY] PASS — the complete 510-second episode and all " +
                    $"{CaptureTimes.Length} diagnostic keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S77_PLAY] FAIL — the accelerated episode did not reach its ending.");
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

        S77_Main director = UnityEngine.Object.FindAnyObjectByType<S77_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.EpisodeTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene77-long-{i + 1:00}.png");
                }
            }

            if (director.EpisodeComplete)
            {
                playModePassed = AllFramesCaptured();
                EditorApplication.isPlaying = false;
                return;
            }
        }

        double elapsed = EditorApplication.timeSinceStartup - playModeStartTime;
        if (elapsed > 70d)
        {
            Debug.LogError("[S77_PLAY] Timed out after 70 real seconds.");
            playModePassed = false;
            EditorApplication.isPlaying = false;
        }
    }

    private static bool AllFramesCaptured()
    {
        for (int i = 0; i < capturedFrames.Length; i++)
        {
            if (!capturedFrames[i])
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
            throw new InvalidOperationException("[S77_PLAY] Main Camera disappeared during Play Mode.");
        }

        const int width = 1280;
        const int height = 720;
        RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;

        try
        {
            camera.targetTexture = renderTexture;
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

        Debug.Log($"[S77_PLAY] Captured {path}");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — 16x9");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.orthographic = true;
        camera.orthographicSize = 5.55f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.55f, 0.83f, 0.96f);
        camera.aspect = 16f / 9f;
        camera.transform.position = new Vector3(0f, 0.25f, -20f);
        camera.transform.rotation = Quaternion.identity;
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        RenderSettings.ambientLight = new Color(0.64f, 0.64f, 0.64f);
    }

    private static void CreateEnvironment()
    {
        Material sky = CreateOpaqueMaterial(
            MaterialRoot + "/S77_Backdrop.mat",
            new Color(0.57f, 0.84f, 0.96f));
        Material ground = CreateOpaqueMaterial(
            MaterialRoot + "/S77_Ground.mat",
            new Color(0.37f, 0.72f, 0.34f));
        Material stripe = CreateOpaqueMaterial(
            MaterialRoot + "/S77_TrackStripe.mat",
            new Color(0.95f, 0.9f, 0.56f));

        CreateCube(
            "S77 Sky Backdrop",
            new Vector3(0f, 0.35f, 3.5f),
            new Vector3(22f, 11.5f, 0.2f),
            sky,
            null);
        CreateCube(
            "S77 Ground",
            new Vector3(0f, -4.05f, 1.8f),
            new Vector3(22f, 2f, 4f),
            ground,
            null);
        CreateCube(
            "S77 Course Start Line",
            new Vector3(-7.75f, -2.98f, 0.5f),
            new Vector3(0.14f, 0.1f, 1.8f),
            stripe,
            null);
        CreateCube(
            "S77 Course Finish Line",
            new Vector3(7.75f, -2.98f, 0.5f),
            new Vector3(0.14f, 0.1f, 1.8f),
            stripe,
            null);
    }

    private static void CreateEpisodeText(
        out TextMesh title,
        out TextMesh caption,
        out TextMesh[] titleOutline,
        out TextMesh[] captionOutline)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            throw new InvalidOperationException("[S77] Unity's built-in LegacyRuntime font is unavailable.");
        }

        title = CreateTextMesh(
            "S77 Stage Title",
            font,
            new Vector3(0f, 4.72f, -2.15f),
            62,
            0.085f,
            Color.white);
        titleOutline = CreateTextOutline(
            "S77 Stage Title Outline",
            font,
            new Vector3(0f, 4.72f, -2.05f),
            62,
            0.085f,
            0.035f);
        caption = CreateTextMesh(
            "S77 Stage Caption",
            font,
            new Vector3(0f, 4.05f, -2.15f),
            46,
            0.075f,
            new Color(1f, 0.96f, 0.48f));
        captionOutline = CreateTextOutline(
            "S77 Stage Caption Outline",
            font,
            new Vector3(0f, 4.05f, -2.05f),
            46,
            0.075f,
            0.03f);
    }

    private static TextMesh[] CreateTextOutline(
        string name,
        Font font,
        Vector3 position,
        int fontSize,
        float characterSize,
        float thickness)
    {
        Vector2[] directions =
        {
            new Vector2(-1f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, -1f), new Vector2(0f, 1f),
            new Vector2(-0.72f, -0.72f), new Vector2(-0.72f, 0.72f),
            new Vector2(0.72f, -0.72f), new Vector2(0.72f, 0.72f)
        };
        TextMesh[] outline = new TextMesh[directions.Length];
        Color outlineColor = new Color(0.055f, 0.09f, 0.16f);
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 offset = new Vector3(
                directions[i].x * thickness,
                directions[i].y * thickness,
                0f);
            outline[i] = CreateTextMesh(
                $"{name} {i + 1}",
                font,
                position + offset,
                fontSize,
                characterSize,
                outlineColor);
        }
        return outline;
    }

    private static TextMesh CreateTextMesh(
        string name,
        Font font,
        Vector3 position,
        int fontSize,
        float characterSize,
        Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.position = position;
        TextMesh text = textObject.AddComponent<TextMesh>();
        text.font = font;
        text.fontSize = fontSize;
        text.characterSize = characterSize;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;
        text.richText = false;
        textObject.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        return text;
    }

    private static GameObject[] CreateObstacles(
        out Transform spinner,
        out Transform seesaw,
        out Transform bridge)
    {
        Material blue = CreateOpaqueMaterial(
            MaterialRoot + "/S77_ObstacleBlue.mat", new Color(0.18f, 0.54f, 0.82f));
        Material yellow = CreateOpaqueMaterial(
            MaterialRoot + "/S77_ObstacleYellow.mat", new Color(0.96f, 0.74f, 0.16f));
        Material orange = CreateOpaqueMaterial(
            MaterialRoot + "/S77_ObstacleOrange.mat", new Color(0.94f, 0.38f, 0.16f));
        Material purple = CreateOpaqueMaterial(
            MaterialRoot + "/S77_ObstaclePurple.mat", new Color(0.55f, 0.27f, 0.76f));
        Material red = CreateOpaqueMaterial(
            MaterialRoot + "/S77_ObstacleRed.mat", new Color(0.84f, 0.16f, 0.2f));
        Material green = CreateOpaqueMaterial(
            MaterialRoot + "/S77_ObstacleGreen.mat", new Color(0.14f, 0.68f, 0.46f));
        Material water = CreateOpaqueMaterial(
            MaterialRoot + "/S77_Water.mat", new Color(0.12f, 0.58f, 0.88f));
        Material stone = CreateOpaqueMaterial(
            MaterialRoot + "/S77_Stone.mat", new Color(0.62f, 0.67f, 0.7f));
        Material dark = CreateOpaqueMaterial(
            MaterialRoot + "/S77_TunnelDark.mat", new Color(0.07f, 0.09f, 0.16f));

        GameObject[] roots = new GameObject[7];
        roots[0] = CreateLowBeam(blue, yellow);
        roots[1] = CreateHoop(orange, yellow);
        roots[2] = CreateZigzag(purple, yellow);
        roots[3] = CreateSteppingStones(water, stone);
        roots[4] = CreateSpinner(red, yellow, out spinner);
        roots[5] = CreateBalanceBridge(blue, green, yellow, out seesaw, out bridge);
        roots[6] = CreateShrinkingTunnel(purple, orange, red, dark);

        for (int i = 0; i < roots.Length; i++)
        {
            roots[i].SetActive(false);
        }
        return roots;
    }

    private static GameObject CreateLowBeam(Material frame, Material warning)
    {
        GameObject root = new GameObject("Obstacle 1 — Low Beam");
        CreateCube("Low Beam Left Support", new Vector3(0.6f, -2.25f, -0.55f),
            new Vector3(0.42f, 1.6f, 0.45f), frame, root.transform);
        CreateCube("Low Beam Right Support", new Vector3(6.4f, -2.25f, -0.55f),
            new Vector3(0.42f, 1.6f, 0.45f), frame, root.transform);
        CreateCube("Low Beam", new Vector3(3.5f, -1.48f, -0.65f),
            new Vector3(6.2f, 0.36f, 0.5f), warning, root.transform);
        for (int i = 0; i < 8; i++)
        {
            CreateCube($"Beam Stripe {i + 1}", new Vector3(0.9f + i * 0.74f, -1.48f, -0.75f),
                new Vector3(0.24f, 0.38f, 0.08f), frame, root.transform);
        }
        return root;
    }

    private static GameObject CreateHoop(Material hoop, Material stand)
    {
        GameObject root = new GameObject("Obstacle 2 — Jumping Hoop");
        Vector3 center = new Vector3(3.05f, -0.25f, -0.65f);
        const int segmentCount = 24;
        const float radius = 2.15f;
        for (int i = 0; i < segmentCount; i++)
        {
            float angle = i * Mathf.PI * 2f / segmentCount;
            Vector3 position = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
            GameObject segment = CreateCube(
                $"Hoop Segment {i + 1:00}", position, new Vector3(0.72f, 0.24f, 0.35f), hoop, root.transform);
            segment.transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg + 90f);
        }
        CreateCube("Hoop Stand Left", new Vector3(0.9f, -2.6f, -0.55f),
            new Vector3(0.3f, 0.9f, 0.45f), stand, root.transform);
        CreateCube("Hoop Stand Right", new Vector3(5.2f, -2.6f, -0.55f),
            new Vector3(0.3f, 0.9f, 0.45f), stand, root.transform);
        return root;
    }

    private static GameObject CreateZigzag(Material post, Material marker)
    {
        GameObject root = new GameObject("Obstacle 3 — Zigzag Gates");
        float[] xPositions = { 0.4f, 2.1f, 3.8f, 5.5f, 7.2f };
        for (int i = 0; i < xPositions.Length; i++)
        {
            float y = i % 2 == 0 ? -0.85f : -1.85f;
            CreateCube($"Zigzag Post {i + 1}", new Vector3(xPositions[i], y, -0.55f),
                new Vector3(0.38f, i % 2 == 0 ? 4.4f : 2.4f, 0.42f), post, root.transform);
            CreateCube($"Zigzag Marker {i + 1}", new Vector3(xPositions[i], y + 1.5f, -0.72f),
                new Vector3(0.62f, 0.28f, 0.18f), marker, root.transform);
        }
        return root;
    }

    private static GameObject CreateSteppingStones(Material water, Material stone)
    {
        GameObject root = new GameObject("Obstacle 4 — Stepping Stones");
        CreateCube("Water", new Vector3(3.4f, -2.73f, 0.35f),
            new Vector3(8.2f, 0.55f, 1.5f), water, root.transform);
        float[] xPositions = { 0.1f, 1.75f, 3.4f, 5.05f, 6.7f };
        for (int i = 0; i < xPositions.Length; i++)
        {
            CreateCube($"Stepping Stone {i + 1}", new Vector3(xPositions[i], -2.52f, -0.58f),
                new Vector3(1.05f, 0.38f, 0.85f), stone, root.transform);
        }
        return root;
    }

    private static GameObject CreateSpinner(Material bar, Material hub, out Transform spinner)
    {
        GameObject root = new GameObject("Obstacle 5 — Spinning Bars");
        GameObject spinnerObject = new GameObject("Spinner Pivot");
        spinnerObject.transform.SetParent(root.transform, false);
        spinnerObject.transform.position = new Vector3(3.1f, -0.45f, -0.62f);
        spinner = spinnerObject.transform;

        CreateCube("Spinner Long Bar", spinner.position, new Vector3(6.6f, 0.32f, 0.42f),
            bar, spinner);
        CreateCube("Spinner Short Bar", spinner.position, new Vector3(0.32f, 5.2f, 0.42f),
            bar, spinner);
        GameObject hubObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hubObject.name = "Spinner Hub";
        hubObject.transform.SetParent(spinner, true);
        hubObject.transform.position = spinner.position + Vector3.back * 0.25f;
        hubObject.transform.localScale = Vector3.one * 0.72f;
        hubObject.GetComponent<Renderer>().sharedMaterial = hub;
        UnityEngine.Object.DestroyImmediate(hubObject.GetComponent<Collider>());
        return root;
    }

    private static GameObject CreateBalanceBridge(
        Material structure,
        Material button,
        Material deck,
        out Transform seesaw,
        out Transform bridge)
    {
        GameObject root = new GameObject("Obstacle 6 — Balance Bridge");
        CreateCube("Pressure Button One", new Vector3(0f, -2.88f, -0.65f),
            new Vector3(1.15f, 0.22f, 0.85f), button, root.transform);
        CreateCube("Pressure Button Two", new Vector3(1.5f, -2.88f, -0.65f),
            new Vector3(1.15f, 0.22f, 0.85f), button, root.transform);

        GameObject seesawObject = new GameObject("Balance Deck Pivot");
        seesawObject.transform.SetParent(root.transform, false);
        seesawObject.transform.position = new Vector3(0.75f, -2.62f, -0.58f);
        seesawObject.transform.rotation = Quaternion.Euler(0f, 0f, -18f);
        seesaw = seesawObject.transform;
        CreateCube("Balance Deck", seesaw.position, new Vector3(3f, 0.28f, 0.9f),
            deck, seesaw);

        GameObject bridgeObject = new GameObject("Lifting Bridge");
        bridgeObject.transform.SetParent(root.transform, false);
        bridgeObject.transform.localPosition = new Vector3(4.75f, -4f, -0.58f);
        bridge = bridgeObject.transform;
        CreateCube("Lifting Bridge Deck", bridge.position, new Vector3(5.1f, 0.34f, 0.9f),
            structure, bridge);
        CreateCube("Bridge Left Tower", new Vector3(2.25f, -1.55f, -0.55f),
            new Vector3(0.4f, 3f, 0.5f), structure, root.transform);
        CreateCube("Bridge Right Tower", new Vector3(7.25f, -1.55f, -0.55f),
            new Vector3(0.4f, 3f, 0.5f), structure, root.transform);
        return root;
    }

    private static GameObject CreateShrinkingTunnel(
        Material large,
        Material medium,
        Material small,
        Material interior)
    {
        GameObject root = new GameObject("Obstacle 7 — Shrinking Tunnel");
        CreateDoorFrame(root.transform, 1.45f, 5.2f, 3.35f, large, interior, "Large Portal");
        CreateDoorFrame(root.transform, 4.4f, 3.7f, 2.35f, medium, interior, "Medium Portal");
        CreateDoorFrame(root.transform, 7.1f, 1.65f, 1.35f, small, interior, "Small Portal");
        return root;
    }

    private static void CreateDoorFrame(
        Transform parent,
        float x,
        float height,
        float width,
        Material frame,
        Material interior,
        string label)
    {
        float centerY = -3.05f + height * 0.5f;
        CreateCube(label + " Interior", new Vector3(x, centerY, 1.12f),
            new Vector3(width, height, 0.16f), interior, parent);
        CreateCube(label + " Left", new Vector3(x - width * 0.5f - 0.22f, centerY, -0.65f),
            new Vector3(0.44f, height + 0.45f, 0.42f), frame, parent);
        CreateCube(label + " Right", new Vector3(x + width * 0.5f + 0.22f, centerY, -0.65f),
            new Vector3(0.44f, height + 0.45f, 0.42f), frame, parent);
        CreateCube(label + " Lintel", new Vector3(x, -3.05f + height + 0.22f, -0.65f),
            new Vector3(width + 0.88f, 0.44f, 0.42f), frame, parent);
    }

    private static GameObject CreateCube(
        string name,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, true);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(cube.GetComponent<Collider>());
        return cube;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets/Prefabs/Blocks", "Scene77");
        EnsureFolder(AssetRoot, "FaceTextures");
        EnsureFolder(AssetRoot, "Materials");
        EnsureFolder(AssetRoot, "Prefabs");
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void ConfigureFaceTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("[S77] Face texture was not imported.", path);
        }

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static Material CreateTransparentFaceMaterial(string path, Texture2D texture)
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        shader ??= Shader.Find("Sprites/Default");
        if (shader == null)
        {
            throw new InvalidOperationException("[S77] No transparent shader is available.");
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
        material.mainTexture = texture;
        SetTextureIfPresent(material, "_BaseMap", texture);
        SetTextureIfPresent(material, "_MainTex", texture);
        SetColorIfPresent(material, "_BaseColor", Color.white);
        SetColorIfPresent(material, "_Color", Color.white);
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        }
        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        }
        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateHundredVariant(Material defaultOverlayMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(HundredSourcePath);
        if (root == null)
        {
            throw new FileNotFoundException("[S77] Original Hundred prefab is missing.", HundredSourcePath);
        }

        try
        {
            Material originalFaceMaterial = LoadAsset<Material>(OriginalFaceMaterialPath);
            Renderer frontRenderer = FindRendererUsingMaterial(root, originalFaceMaterial);
            if (frontRenderer == null)
            {
                throw new InvalidOperationException("[S77] Could not find Hundred's original front face.");
            }

            Bounds originalBounds = BoundsOf(root);
            Bounds faceBounds = frontRenderer.bounds;
            frontRenderer.gameObject.name = FrontBodyName;

            GameObject overlay = UnityEngine.Object.Instantiate(
                frontRenderer.gameObject,
                frontRenderer.transform.parent,
                false);
            overlay.name = FaceOverlayName;
            Renderer overlayRenderer = overlay.GetComponent<Renderer>();
            overlayRenderer.sharedMaterial = defaultOverlayMaterial;
            overlayRenderer.shadowCastingMode = ShadowCastingMode.Off;
            overlayRenderer.receiveShadows = false;
            overlayRenderer.sortingOrder = 20;

            Collider[] overlayColliders = overlay.GetComponents<Collider>();
            for (int i = 0; i < overlayColliders.Length; i++)
            {
                UnityEngine.Object.DestroyImmediate(overlayColliders[i]);
            }

            Vector3 outward = faceBounds.center - originalBounds.center;
            if (outward.sqrMagnitude < 0.000001f)
            {
                outward = -overlay.transform.forward;
            }
            overlay.transform.position += outward.normalized * 0.006f;
            overlay.SetActive(false);

            root.name = "Hundred_DoorRunner";
            PrefabUtility.SaveAsPrefabAsset(root, HundredVariantPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Material CreateOpaqueMaterial(string path, Color color)
    {
        Material template = GraphicsSettings.currentRenderPipeline != null
            ? GraphicsSettings.currentRenderPipeline.defaultMaterial
            : null;
        Shader fallback = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Lit")
            : Shader.Find("Standard");
        fallback ??= Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (template == null && fallback == null)
        {
            throw new InvalidOperationException("[S77] No opaque shader is available.");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = template != null ? new Material(template) : new Material(fallback);
            AssetDatabase.CreateAsset(material, path);
        }
        else if (template != null)
        {
            material.shader = template.shader;
            material.CopyPropertiesFromMaterial(template);
        }
        else
        {
            material.shader = fallback;
        }

        material.color = color;
        SetColorIfPresent(material, "_BaseColor", color);
        SetColorIfPresent(material, "_Color", color);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ValidateFaceTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || importer.alphaSource != TextureImporterAlphaSource.FromInput ||
            !importer.alphaIsTransparency)
        {
            throw new InvalidOperationException($"[S77_VALIDATE] Texture is not imported with alpha: {path}");
        }
    }

    private static void ValidateTransparentMaterial(Material material, string expectedTexturePath)
    {
        if (material == null || material.renderQueue < (int)RenderQueue.Transparent ||
            material.GetTag("RenderType", false) != "Transparent")
        {
            throw new InvalidOperationException("[S77_VALIDATE] A face material is not transparent.");
        }

        if (AssetDatabase.GetAssetPath(material.mainTexture) != expectedTexturePath)
        {
            throw new InvalidOperationException("[S77_VALIDATE] A face material uses the wrong texture.");
        }
    }

    private static void ValidateHundredVariant()
    {
        GameObject source = PrefabUtility.LoadPrefabContents(HundredSourcePath);
        GameObject variant = PrefabUtility.LoadPrefabContents(HundredVariantPath);
        try
        {
            Renderer sourceFront = FindRendererUsingMaterial(source, LoadAsset<Material>(OriginalFaceMaterialPath));
            Renderer variantFront = FindRenderer(variant, FrontBodyName);
            Renderer overlay = FindRenderer(variant, FaceOverlayName);
            if (sourceFront == null || variantFront == null || overlay == null)
            {
                throw new InvalidOperationException("[S77_VALIDATE] Hundred face renderers are incomplete.");
            }

            Vector3 sourceCenter = source.transform.InverseTransformPoint(sourceFront.bounds.center);
            Vector3 frontCenter = variant.transform.InverseTransformPoint(variantFront.bounds.center);
            Vector3 overlayCenter = variant.transform.InverseTransformPoint(overlay.bounds.center);
            if (Mathf.Abs(sourceCenter.x - frontCenter.x) > 0.001f ||
                Mathf.Abs(sourceCenter.y - frontCenter.y) > 0.001f ||
                Mathf.Abs(sourceCenter.x - overlayCenter.x) > 0.001f ||
                Mathf.Abs(sourceCenter.y - overlayCenter.y) > 0.001f ||
                sourceFront.transform.localRotation != overlay.transform.localRotation ||
                sourceFront.transform.localScale != overlay.transform.localScale)
            {
                throw new InvalidOperationException(
                    "[S77_VALIDATE] Face size, horizontal position, or vertical position changed.");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(source);
            PrefabUtility.UnloadPrefabContents(variant);
        }
    }

    private static Renderer FindRenderer(GameObject root, string objectName)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject.name == objectName)
            {
                return renderers[i];
            }
        }
        return null;
    }

    private static Renderer FindRendererUsingMaterial(GameObject root, Material material)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j] == material)
                {
                    return renderers[i];
                }
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

    private static void SetTextureIfPresent(Material material, string property, Texture texture)
    {
        if (material.HasProperty(property))
        {
            material.SetTexture(property, texture);
        }
    }

    private static void SetColorIfPresent(Material material, string property, Color color)
    {
        if (material.HasProperty(property))
        {
            material.SetColor(property, color);
        }
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException($"[S77] Required asset is missing: {path}", path);
        }
        return asset;
    }

    private static T LoadOptional<T>(string path) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
