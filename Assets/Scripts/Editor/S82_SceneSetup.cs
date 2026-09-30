using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the vertical Scene 82 collision-and-reassembly short.</summary>
public static class S82_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_82.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene82";
    private const string TextureRoot = AssetRoot + "/FaceTextures";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string PrefabRoot = AssetRoot + "/Prefabs";

    private const string OneSourcePath = "Assets/Prefabs/Blocks/1.prefab";
    private const string FiftySourcePath = "Assets/Prefabs/Blocks/50.prefab";
    private const string HundredSourcePath = "Assets/Prefabs/Blocks/100.prefab";
    private const string FiftyOriginalFaceMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/50-face.mat";
    private const string HundredOriginalFaceMaterialPath = "Assets/Materials/Materials/11.mat";
    private const string HundredCleanFrontMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/newMillion 1.mat";

    private const string FiftyDeterminedTexturePath = TextureRoot + "/Fifty_Determined.png";
    private const string HundredDeterminedTexturePath = TextureRoot + "/Hundred_Determined.png";
    private const string HundredShockedTexturePath = TextureRoot + "/Hundred_Shocked.png";
    private const string OneFiftyHappyTexturePath = TextureRoot + "/OneFifty_Happy.png";

    private const string FiftyDeterminedMaterialPath =
        MaterialRoot + "/Fifty_Determined_Transparent.mat";
    private const string HundredDeterminedMaterialPath =
        MaterialRoot + "/Hundred_Determined_Transparent.mat";
    private const string HundredShockedMaterialPath =
        MaterialRoot + "/Hundred_Shocked_Transparent.mat";
    private const string OneFiftyHappyMaterialPath =
        MaterialRoot + "/OneFifty_Happy_Transparent.mat";

    private const string FiftyPrefabPath = PrefabRoot + "/Fifty_Collision.prefab";
    private const string HundredPrefabPath = PrefabRoot + "/Hundred_Collision.prefab";

    private const string WhooshPath = "Assets/Sound/zvuk-priblijeniya.mp3";
    private const string ImpactPath = "Assets/Sound/collCube.wav";
    private const string ImpactAccentPath = "Assets/Sound/boom_metal.wav";
    private const string ExplosionPath = "Assets/Sound/destroy_blocks.mp3";
    private const string SuccessPath =
        "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav";

    private static readonly float[] CaptureTimes =
    {
        0.7f,
        2.85f,
        4.55f,
        5.45f,
        7.25f,
        9.85f,
        10.8f,
        12.45f,
        15.25f
    };

    private static bool[] capturedFrames;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static double playModeStartTime;

    [MenuItem("Tools/Scene_82/Quick Setup 50 + 100 Collision Short")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        ConfigureFaceTexture(FiftyDeterminedTexturePath);
        ConfigureFaceTexture(HundredDeterminedTexturePath);
        ConfigureFaceTexture(HundredShockedTexturePath);
        ConfigureFaceTexture(OneFiftyHappyTexturePath);

        Material fiftyDetermined = CreateTransparentMaterial(
            FiftyDeterminedMaterialPath,
            LoadAsset<Texture2D>(FiftyDeterminedTexturePath));
        Material hundredDetermined = CreateTransparentMaterial(
            HundredDeterminedMaterialPath,
            LoadAsset<Texture2D>(HundredDeterminedTexturePath));
        Material hundredShocked = CreateTransparentMaterial(
            HundredShockedMaterialPath,
            LoadAsset<Texture2D>(HundredShockedTexturePath));
        Material oneFiftyHappy = CreateTransparentMaterial(
            OneFiftyHappyMaterialPath,
            LoadAsset<Texture2D>(OneFiftyHappyTexturePath));

        CreateFiftyVariant(fiftyDetermined);
        CreateHundredVariant(hundredDetermined, hundredShocked);

        Material groundMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S82_Ground.mat",
            new Color(0.25f, 0.67f, 0.32f));
        Material arenaMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S82_Arena.mat",
            new Color(0.12f, 0.36f, 0.62f));
        Material hillMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S82_Hills.mat",
            new Color(0.2f, 0.53f, 0.28f));
        Material cloudMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S82_Clouds.mat",
            new Color(0.98f, 0.99f, 1f));
        Material dustMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S82_Dust.mat",
            new Color(1f, 0.73f, 0.2f));
        Material crackMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S82_Crack.mat",
            new Color(0.09f, 0.075f, 0.055f));
        Material flashMaterial = CreateFlashMaterial(MaterialRoot + "/S82_Flash.mat");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        BoxCollider groundCollider = CreateEnvironment(
            groundMaterial,
            arenaMaterial,
            hillMaterial,
            cloudMaterial);
        TextMesh equation = CreateEquationText();

        GameObject burstObject = new GameObject("150 Original Ones — Scatter and Reassembly");
        S82_UnitBurst burst = burstObject.AddComponent<S82_UnitBurst>();
        burst.onePrefab = LoadAsset<GameObject>(OneSourcePath);
        burst.groundCollider = groundCollider;

        GameObject gameManager = new GameObject("GameManager — 50 + 100 Collision Timeline");
        S82_Main director = gameManager.AddComponent<S82_Main>();
        director.onePrefab = LoadAsset<GameObject>(OneSourcePath);
        director.fiftyPrefab = LoadAsset<GameObject>(FiftyPrefabPath);
        director.hundredPrefab = LoadAsset<GameObject>(HundredPrefabPath);
        director.characterScale = 0.52f;
        director.finalFaceMaterial = oneFiftyHappy;
        director.dustMaterial = dustMaterial;
        director.flashMaterial = flashMaterial;
        director.crackMaterial = crackMaterial;
        director.unitBurst = burst;
        director.equationText = equation;
        director.whooshClip = LoadOptional<AudioClip>(WhooshPath);
        director.impactClip = LoadOptional<AudioClip>(ImpactPath);
        director.impactAccentClip = LoadOptional<AudioClip>(ImpactAccentPath);
        director.explosionClip = LoadOptional<AudioClip>(ExplosionPath);
        director.successClip = LoadOptional<AudioClip>(SuccessPath);
        EditorUtility.SetDirty(burst);
        EditorUtility.SetDirty(director);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S82] Unity could not save the scene: {ScenePath}");
        }
        AssetDatabase.SaveAssets();

        Debug.Log(
            "[S82] Scene_82 created: vertical 9:16, two impacts, exact 50 + 100 unit " +
            "burst, 15 x 10 reassembly, original source blocks, and transparent custom faces.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_82 — 50 + 100 collision",
                "Готово. Выберите Game View 9:16 и нажмите Play. Длительность — 15,5 секунды.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_82/Validate Collision Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S82_VALIDATE] Scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S82_Main director = UnityEngine.Object.FindAnyObjectByType<S82_Main>();
        S82_UnitBurst burst = UnityEngine.Object.FindAnyObjectByType<S82_UnitBurst>();
        if (director == null || burst == null)
        {
            throw new InvalidOperationException("[S82_VALIDATE] Director or unit burst is missing.");
        }

        if (AssetDatabase.GetAssetPath(director.onePrefab) != OneSourcePath ||
            AssetDatabase.GetAssetPath(director.fiftyPrefab) != FiftyPrefabPath ||
            AssetDatabase.GetAssetPath(director.hundredPrefab) != HundredPrefabPath ||
            director.unitBurst != burst || burst.onePrefab != director.onePrefab)
        {
            throw new InvalidOperationException("[S82_VALIDATE] Numberblock references are incomplete.");
        }

        if (!(burst.groundCollider is BoxCollider) || director.finalFaceMaterial == null ||
            director.dustMaterial == null || director.flashMaterial == null ||
            director.crackMaterial == null || director.equationText == null)
        {
            throw new InvalidOperationException("[S82_VALIDATE] Scene effects or staging are incomplete.");
        }

        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic ||
            Mathf.Abs(camera.orthographicSize - 8.5f) > 0.01f ||
            camera.name != "Main Camera — Vertical 9x16")
        {
            throw new InvalidOperationException("[S82_VALIDATE] Vertical camera is misconfigured.");
        }

        if (S82_UnitBurst.FiftyUnitCount != 50 ||
            S82_UnitBurst.HundredUnitCount != 100 ||
            S82_UnitBurst.TotalUnitCount != 150 ||
            S82_UnitBurst.ResultColumns != 15 ||
            S82_UnitBurst.RowCount != 10)
        {
            throw new InvalidOperationException("[S82_VALIDATE] Exact 50 + 100 = 150 geometry changed.");
        }

        if (!(S82_Main.DeterminedTime < S82_Main.FirstImpactTime &&
              S82_Main.FirstImpactTime < S82_Main.FirstRecoveryTime &&
              S82_Main.FirstRecoveryTime < S82_Main.FinalChargeTime &&
              S82_Main.FinalChargeTime < S82_Main.FinalImpactTime &&
              S82_Main.FinalImpactTime < S82_Main.ResultRevealTime &&
              S82_Main.ResultRevealTime < S82_Main.FinalJumpTime &&
              S82_Main.FinalJumpTime < S82_Main.FinalLandingTime &&
              S82_Main.FinalLandingTime < S82_Main.SequenceDuration))
        {
            throw new InvalidOperationException("[S82_VALIDATE] Timeline beats overlap or are unordered.");
        }

        ValidateFaceTexture(FiftyDeterminedTexturePath, 1101, 1056);
        ValidateFaceTexture(HundredDeterminedTexturePath, 956, 956);
        ValidateFaceTexture(HundredShockedTexturePath, 956, 956);
        ValidateFaceTexture(OneFiftyHappyTexturePath, 1536, 1024);
        ValidateTransparentMaterial(director.finalFaceMaterial, OneFiftyHappyTexturePath);
        ValidateFiftyVariant();
        ValidateHundredVariant();

        Debug.Log(
            "[S82_VALIDATE] PASS — 9:16, 15.5s timeline, original 50/100/One sources, " +
            "four RGBA face assets, square Hundred eye, and exact 15 x 10 result.");
    }

    /// <summary>Runs the complete short at 2x and captures every important visual beat.</summary>
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S82_PLAY] This entry point is batch-mode only.");
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
            Time.timeScale = 2f;
            Debug.Log("[S82_PLAY] Entered Play Mode at 2x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            if (playModePassed)
            {
                Debug.Log(
                    $"[S82_PLAY] PASS — complete short and all {CaptureTimes.Length} keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S82_PLAY] FAIL — the complete short did not reach its verified ending.");
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

        S82_Main director = UnityEngine.Object.FindAnyObjectByType<S82_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene82-{i + 1:00}.png");
                }
            }

            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() &&
                    director.CollisionCount == 2 &&
                    director.BurstTriggered &&
                    director.ResultRevealed &&
                    director.GroundCracked &&
                    director.unitBurst.Started &&
                    director.unitBurst.FormationComplete;
                EditorApplication.isPlaying = false;
                return;
            }
        }

        if (EditorApplication.timeSinceStartup - playModeStartTime > 20d)
        {
            Debug.LogError("[S82_PLAY] Timed out after 20 real seconds.");
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
            throw new InvalidOperationException("[S82_PLAY] Main Camera disappeared.");
        }

        const int width = 540;
        const int height = 960;
        RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        try
        {
            camera.targetTexture = renderTexture;
            camera.aspect = (float)width / height;
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
        Debug.Log($"[S82_PLAY] Captured {path}");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene82");
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
            throw new FileNotFoundException("[S82] Face texture was not imported.", path);
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

    private static Material CreateTransparentMaterial(string path, Texture2D texture)
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        shader ??= Shader.Find("Sprites/Default");
        if (shader == null)
        {
            throw new InvalidOperationException("[S82] No transparent shader is available.");
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
        SetMaterialColor(material, Color.white);
        ConfigureTransparentBlend(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateFiftyVariant(Material determinedMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(FiftySourcePath);
        if (root == null)
        {
            throw new FileNotFoundException("[S82] Original Fifty prefab is missing.", FiftySourcePath);
        }

        try
        {
            Material originalMaterial = LoadAsset<Material>(FiftyOriginalFaceMaterialPath);
            Renderer originalFace = FindRendererUsingMaterial(root, originalMaterial);
            if (originalFace == null)
            {
                throw new InvalidOperationException("[S82] Original Fifty face renderer was not found.");
            }

            Vector3 outward = -originalFace.transform.forward.normalized;

            originalFace.gameObject.name = S82_Main.FiftyOriginalFaceName;
            ConfigureOverlay(originalFace.gameObject, originalMaterial);

            GameObject determined = UnityEngine.Object.Instantiate(
                originalFace.gameObject,
                originalFace.transform.parent,
                false);
            determined.name = S82_Main.FiftyDeterminedFaceName;
            ConfigureOverlay(determined, determinedMaterial);
            determined.transform.position += outward * 0.004f;
            determined.SetActive(false);

            root.name = "Fifty_Collision";
            PrefabUtility.SaveAsPrefabAsset(root, FiftyPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void CreateHundredVariant(Material determinedMaterial, Material shockedMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(HundredSourcePath);
        if (root == null)
        {
            throw new FileNotFoundException("[S82] Original Hundred prefab is missing.", HundredSourcePath);
        }

        try
        {
            Material originalMaterial = LoadAsset<Material>(HundredOriginalFaceMaterialPath);
            Material cleanMaterial = LoadAsset<Material>(HundredCleanFrontMaterialPath);
            Renderer front = FindRendererUsingMaterial(root, originalMaterial);
            if (front == null)
            {
                throw new InvalidOperationException("[S82] Original Hundred front renderer was not found.");
            }

            Vector3 outward = -front.transform.forward.normalized;

            GameObject originalFace = UnityEngine.Object.Instantiate(
                front.gameObject,
                front.transform.parent,
                false);
            originalFace.name = S82_Main.HundredOriginalFaceName;
            ConfigureOverlay(originalFace, originalMaterial);
            originalFace.transform.position += outward * 0.004f;

            GameObject determined = UnityEngine.Object.Instantiate(
                originalFace,
                originalFace.transform.parent,
                false);
            determined.name = S82_Main.HundredDeterminedFaceName;
            ConfigureOverlay(determined, determinedMaterial);
            determined.transform.position += outward * 0.003f;
            determined.SetActive(false);

            GameObject shocked = UnityEngine.Object.Instantiate(
                originalFace,
                originalFace.transform.parent,
                false);
            shocked.name = S82_Main.HundredShockedFaceName;
            ConfigureOverlay(shocked, shockedMaterial);
            shocked.transform.position += outward * 0.006f;
            shocked.SetActive(false);

            front.gameObject.name = "S82 100 Clean Front Body";
            ReplaceMaterial(front, originalMaterial, cleanMaterial);
            root.name = "Hundred_Collision";
            PrefabUtility.SaveAsPrefabAsset(root, HundredPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureOverlay(GameObject overlay, Material material)
    {
        Renderer renderer = overlay.GetComponent<Renderer>();
        if (renderer == null)
        {
            throw new InvalidOperationException("[S82] Face overlay has no Renderer.");
        }
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = 25;
        RemoveColliders(overlay);
    }

    private static void ReplaceMaterial(Renderer renderer, Material from, Material to)
    {
        Material[] materials = renderer.sharedMaterials;
        bool replaced = false;
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == from)
            {
                materials[i] = to;
                replaced = true;
            }
        }
        if (!replaced)
        {
            throw new InvalidOperationException("[S82] Original face material was not assigned.");
        }
        renderer.sharedMaterials = materials;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Vertical 9x16");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.orthographic = true;
        camera.orthographicSize = 8.5f;
        camera.aspect = 9f / 16f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 120f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.35f, 0.74f, 0.95f);
        camera.transform.position = new Vector3(0f, 4f, -20f);
        camera.transform.LookAt(new Vector3(0f, 3.4f, 0.3f));
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.28f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
        RenderSettings.ambientLight = new Color(0.62f, 0.62f, 0.62f);
    }

    private static BoxCollider CreateEnvironment(
        Material groundMaterial,
        Material arenaMaterial,
        Material hillMaterial,
        Material cloudMaterial)
    {
        GameObject environment = new GameObject("S82 Giant Collision Arena");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Grass Ground";
        ground.transform.SetParent(environment.transform, true);
        ground.transform.position = new Vector3(0f, -0.06f, 7f);
        ground.transform.localScale = new Vector3(18f, 1f, 18f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        RemoveColliders(ground);
        BoxCollider groundCollider = ground.AddComponent<BoxCollider>();
        groundCollider.size = new Vector3(10f, 0.12f, 10f);
        groundCollider.center = new Vector3(0f, -0.06f, 0f);

        CreateCube(
            "Arena Back Wall",
            new Vector3(0f, 3.4f, 12f),
            new Vector3(28f, 7.2f, 0.45f),
            arenaMaterial,
            environment.transform);
        CreateSphere(
            "Left Hill",
            new Vector3(-7.8f, -1.25f, 9.8f),
            new Vector3(11f, 5.2f, 2.2f),
            hillMaterial,
            environment.transform);
        CreateSphere(
            "Right Hill",
            new Vector3(8.1f, -1.45f, 10.1f),
            new Vector3(12f, 5.5f, 2.3f),
            hillMaterial,
            environment.transform);

        GameObject clouds = new GameObject("Soft Clouds");
        clouds.transform.SetParent(environment.transform, false);
        CreateCloud(new Vector3(-3.4f, 8.8f, 10.7f), 0.72f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(3.1f, 11.1f, 10.9f), 0.58f, cloudMaterial, clouds.transform);

        for (int i = 0; i < 9; i++)
        {
            float x = -6f + i * 1.5f;
            CreateCube(
                $"Arena Floor Marker {i + 1:00}",
                new Vector3(x, 0.012f, 1.8f),
                new Vector3(0.055f, 0.018f, 3.8f),
                arenaMaterial,
                environment.transform);
        }
        return groundCollider;
    }

    private static void CreateCloud(Vector3 center, float scale, Material material, Transform parent)
    {
        CreateSphere(
            "Cloud Puff",
            center + Vector3.left * 0.72f * scale,
            new Vector3(1.45f, 0.72f, 0.35f) * scale,
            material,
            parent);
        CreateSphere(
            "Cloud Puff",
            center + Vector3.up * 0.22f * scale,
            new Vector3(1.55f, 0.92f, 0.38f) * scale,
            material,
            parent);
        CreateSphere(
            "Cloud Puff",
            center + Vector3.right * 0.78f * scale,
            new Vector3(1.38f, 0.68f, 0.34f) * scale,
            material,
            parent);
    }

    private static TextMesh CreateEquationText()
    {
        GameObject textObject = new GameObject("Equation — 50 + 100");
        textObject.transform.position = new Vector3(0f, 7.25f, -0.5f);
        TextMesh text = textObject.AddComponent<TextMesh>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.font = font;
        text.fontSize = 92;
        text.characterSize = 0.095f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;
        text.text = "50 + 100 = ?";
        text.richText = false;
        if (font != null)
        {
            textObject.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }
        return text;
    }

    private static GameObject CreateCube(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;
        cube.transform.SetParent(parent, true);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        RemoveColliders(cube);
        return cube;
    }

    private static GameObject CreateSphere(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = objectName;
        sphere.transform.SetParent(parent, true);
        sphere.transform.position = position;
        sphere.transform.localScale = scale;
        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        RemoveColliders(sphere);
        return sphere;
    }

    private static Material CreateOpaqueMaterial(string path, Color color)
    {
        Material template = GraphicsSettings.currentRenderPipeline != null
            ? GraphicsSettings.currentRenderPipeline.defaultMaterial
            : null;
        Shader shader = template == null ? Shader.Find("Standard") : template.shader;
        shader ??= Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null)
        {
            throw new InvalidOperationException("[S82] No opaque shader is available.");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = template != null ? new Material(template) : new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else if (template != null)
        {
            material.shader = template.shader;
            material.CopyPropertiesFromMaterial(template);
        }
        else
        {
            material.shader = shader;
        }
        SetMaterialColor(material, color);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateFlashMaterial(string path)
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        shader ??= Shader.Find("Sprites/Default");
        if (shader == null)
        {
            throw new InvalidOperationException("[S82] No flash shader is available.");
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
        SetMaterialColor(material, new Color(1f, 0.84f, 0.12f, 0.78f));
        ConfigureTransparentBlend(material);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(1f, 0.38f, 0.02f) * 1.8f);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ConfigureTransparentBlend(Material material)
    {
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
    }

    private static void ValidateFaceTexture(string path, int width, int height)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        Texture2D texture = LoadAsset<Texture2D>(path);
        if (importer == null || texture == null ||
            importer.alphaSource != TextureImporterAlphaSource.FromInput ||
            !importer.alphaIsTransparency || texture.width != width || texture.height != height)
        {
            throw new InvalidOperationException(
                $"[S82_VALIDATE] Face texture import or dimensions are wrong: {path}");
        }

        Texture2D copy = ReadTexture(texture, texture.width, texture.height);
        try
        {
            Color32[] pixels = copy.GetPixels32();
            int upperLeft = (copy.height - 1) * copy.width;
            int upperRight = upperLeft + copy.width - 1;
            if (pixels[0].a > 8 || pixels[copy.width - 1].a > 8 ||
                pixels[upperLeft].a > 8 || pixels[upperRight].a > 8)
            {
                throw new InvalidOperationException(
                    $"[S82_VALIDATE] Face PNG corners are not transparent: {path}");
            }

            int visible = 0;
            int transparent = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 16) visible++;
                if (pixels[i].a <= 8) transparent++;
            }
            if (visible < pixels.Length / 20 || transparent < pixels.Length / 5)
            {
                throw new InvalidOperationException(
                    $"[S82_VALIDATE] Face PNG lacks visible art or genuine alpha: {path}");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(copy);
        }
    }

    private static void ValidateTransparentMaterial(Material material, string expectedTexturePath)
    {
        if (material.renderQueue < (int)RenderQueue.Transparent ||
            material.GetTag("RenderType", false) != "Transparent" ||
            AssetDatabase.GetAssetPath(material.mainTexture) != expectedTexturePath)
        {
            throw new InvalidOperationException("[S82_VALIDATE] Transparent face material is invalid.");
        }
    }

    private static void ValidateFiftyVariant()
    {
        GameObject source = PrefabUtility.LoadPrefabContents(FiftySourcePath);
        GameObject variant = PrefabUtility.LoadPrefabContents(FiftyPrefabPath);
        try
        {
            Renderer sourceFace = FindRendererUsingMaterial(
                source,
                LoadAsset<Material>(FiftyOriginalFaceMaterialPath));
            Renderer original = FindRenderer(variant, S82_Main.FiftyOriginalFaceName);
            Renderer determined = FindRenderer(variant, S82_Main.FiftyDeterminedFaceName);
            if (sourceFace == null || original == null || determined == null)
            {
                throw new InvalidOperationException("[S82_VALIDATE] Fifty face layers are incomplete.");
            }
            ValidateOverlayTransform(sourceFace.transform, original.transform, "Fifty original");
            ValidateOverlayTransform(sourceFace.transform, determined.transform, "Fifty determined");
            ValidateTransparentMaterial(determined.sharedMaterial, FiftyDeterminedTexturePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(source);
            PrefabUtility.UnloadPrefabContents(variant);
        }
    }

    private static void ValidateHundredVariant()
    {
        GameObject source = PrefabUtility.LoadPrefabContents(HundredSourcePath);
        GameObject variant = PrefabUtility.LoadPrefabContents(HundredPrefabPath);
        try
        {
            Renderer sourceFace = FindRendererUsingMaterial(
                source,
                LoadAsset<Material>(HundredOriginalFaceMaterialPath));
            Renderer clean = FindRenderer(variant, "S82 100 Clean Front Body");
            Renderer original = FindRenderer(variant, S82_Main.HundredOriginalFaceName);
            Renderer determined = FindRenderer(variant, S82_Main.HundredDeterminedFaceName);
            Renderer shocked = FindRenderer(variant, S82_Main.HundredShockedFaceName);
            if (sourceFace == null || clean == null || original == null ||
                determined == null || shocked == null)
            {
                throw new InvalidOperationException("[S82_VALIDATE] Hundred face layers are incomplete.");
            }
            ValidateOverlayTransform(sourceFace.transform, original.transform, "Hundred original");
            ValidateOverlayTransform(sourceFace.transform, determined.transform, "Hundred determined");
            ValidateOverlayTransform(sourceFace.transform, shocked.transform, "Hundred shocked");
            if (clean.sharedMaterial != LoadAsset<Material>(HundredCleanFrontMaterialPath))
            {
                throw new InvalidOperationException("[S82_VALIDATE] Hundred clean front is incorrect.");
            }
            ValidateTransparentMaterial(determined.sharedMaterial, HundredDeterminedTexturePath);
            ValidateTransparentMaterial(shocked.sharedMaterial, HundredShockedTexturePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(source);
            PrefabUtility.UnloadPrefabContents(variant);
        }
    }

    private static void ValidateOverlayTransform(Transform source, Transform overlay, string label)
    {
        if (Mathf.Abs(source.localPosition.x - overlay.localPosition.x) > 0.001f ||
            Mathf.Abs(source.localPosition.y - overlay.localPosition.y) > 0.001f ||
            source.localRotation != overlay.localRotation ||
            source.localScale != overlay.localScale)
        {
            throw new InvalidOperationException(
                $"[S82_VALIDATE] {label} moved or changed size relative to its original face.");
        }
    }

    private static Texture2D ReadTexture(Texture source, int width, int height)
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(
            width,
            height,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default);
        RenderTexture previous = RenderTexture.active;
        try
        {
            Graphics.Blit(source, renderTexture);
            RenderTexture.active = renderTexture;
            Texture2D copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            copy.Apply(false, false);
            return copy;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    private static Renderer FindRendererUsingMaterial(GameObject root, Material material)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material candidate in renderer.sharedMaterials)
            {
                if (candidate == material)
                {
                    return renderer;
                }
            }
        }
        return null;
    }

    private static Renderer FindRenderer(GameObject root, string objectName)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.name == objectName)
            {
                return renderer;
            }
        }
        return null;
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

    private static void RemoveColliders(GameObject target)
    {
        foreach (Collider collider in target.GetComponents<Collider>())
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static void SetTextureIfPresent(Material material, string property, Texture texture)
    {
        if (material.HasProperty(property))
        {
            material.SetTexture(property, texture);
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
        material.color = color;
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException($"[S82] Required asset is missing: {path}", path);
        }
        return asset;
    }

    private static T LoadOptional<T>(string path) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
