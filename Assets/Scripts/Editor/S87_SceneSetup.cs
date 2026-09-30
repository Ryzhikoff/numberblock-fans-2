using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the vertical Scene 87 stair-fall short.</summary>
public static class S87_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_87.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene87";
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

    private const string FiftyTexturePath = TextureRoot + "/Fifty_Startled.png";
    private const string HundredTexturePath = TextureRoot + "/Hundred_Startled.png";
    private const string OneFiftyTexturePath = TextureRoot + "/OneFifty_Delighted.png";
    private const string FiftyMaterialPath = MaterialRoot + "/Fifty_Startled_Transparent.mat";
    private const string HundredMaterialPath = MaterialRoot + "/Hundred_Startled_Transparent.mat";
    private const string OneFiftyMaterialPath = MaterialRoot + "/OneFifty_Delighted_Transparent.mat";
    private const string FiftyPrefabPath = PrefabRoot + "/Fifty_Stairs.prefab";
    private const string HundredPrefabPath = PrefabRoot + "/Hundred_Stairs.prefab";
    private const string OneFiftyPrefabPath = PrefabRoot + "/OneFifty_Stairs.prefab";

    private static readonly string[] SharedSourcePaths =
    {
        OneSourcePath,
        FiftySourcePath,
        HundredSourcePath,
        FiftyOriginalFaceMaterialPath,
        HundredOriginalFaceMaterialPath,
        HundredCleanFrontMaterialPath
    };

    private static readonly float[] CaptureTimes =
    {
        0.3f,
        2.3f,
        4.7f,
        5.7f,
        7.1f,
        8.2f,
        10.5f,
        12.9f,
        14.7f,
        17.7f
    };

    private static bool[] capturedFrames;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static double playModeStartTime;

    [MenuItem("Tools/Scene_87/Create 50 + 100 Stair Fall Short")]
    public static void QuickSetup()
    {
        Dictionary<string, string> sourceHashes = HashSharedSources();
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        ConfigureFaceTexture(FiftyTexturePath);
        ConfigureFaceTexture(HundredTexturePath);
        ConfigureFaceTexture(OneFiftyTexturePath);
        Material fiftyFace = CreateTransparentMaterial(
            FiftyMaterialPath,
            LoadAsset<Texture2D>(FiftyTexturePath));
        Material hundredFace = CreateTransparentMaterial(
            HundredMaterialPath,
            LoadAsset<Texture2D>(HundredTexturePath));
        Material oneFiftyFace = CreateTransparentMaterial(
            OneFiftyMaterialPath,
            LoadAsset<Texture2D>(OneFiftyTexturePath));

        CreateFiftyVariant(fiftyFace);
        CreateHundredVariant(hundredFace);
        CreateOneFiftyVariant(oneFiftyFace);

        Material groundMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S87_Ground.mat",
            new Color(0.2f, 0.58f, 0.28f));
        Material stepBlue = CreateOpaqueMaterial(
            MaterialRoot + "/S87_StepBlue.mat",
            new Color(0.16f, 0.45f, 0.82f));
        Material stepGold = CreateOpaqueMaterial(
            MaterialRoot + "/S87_StepGold.mat",
            new Color(1f, 0.61f, 0.14f));
        Material stepPurple = CreateOpaqueMaterial(
            MaterialRoot + "/S87_StepPurple.mat",
            new Color(0.5f, 0.28f, 0.76f));
        Material hillMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S87_Hills.mat",
            new Color(0.14f, 0.45f, 0.26f));
        Material cloudMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S87_Clouds.mat",
            new Color(0.98f, 0.99f, 1f));
        Material dustMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S87_Dust.mat",
            new Color(1f, 0.78f, 0.25f));
        Material flashMaterial = CreateTransparentColorMaterial(
            MaterialRoot + "/S87_Flash.mat",
            new Color(1f, 0.93f, 0.35f, 0.82f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        EnvironmentResult environment = CreateEnvironment(
            groundMaterial,
            stepBlue,
            stepGold,
            stepPurple,
            hillMaterial,
            cloudMaterial);

        GameObject burstObject = new GameObject("Exactly 150 Ones — Scatter and Reassembly");
        S87_UnitBurst burst = burstObject.AddComponent<S87_UnitBurst>();
        burst.onePrefab = LoadAsset<GameObject>(OneSourcePath);
        burst.landingCollider = environment.GroundCollider;

        GameObject gameManager = new GameObject("GameManager — 50 + 100 Stair Fall Timeline");
        S87_Main director = gameManager.AddComponent<S87_Main>();
        director.onePrefab = LoadAsset<GameObject>(OneSourcePath);
        director.fiftyPrefab = LoadAsset<GameObject>(FiftyPrefabPath);
        director.hundredPrefab = LoadAsset<GameObject>(HundredPrefabPath);
        director.oneFiftyPrefab = LoadAsset<GameObject>(OneFiftyPrefabPath);
        director.characterScale = 0.45f;
        director.brokenStep = environment.BrokenStep;
        director.finalFaceMaterial = oneFiftyFace;
        director.dustMaterial = dustMaterial;
        director.flashMaterial = flashMaterial;
        director.unitBurst = burst;
        director.hopClip = LoadOptional<AudioClip>("Assets/Sound/zvuk-priblijeniya.mp3");
        director.crackClip = LoadOptional<AudioClip>("Assets/Sound/collCube.wav");
        director.burstClip = LoadOptional<AudioClip>("Assets/Sound/destroy_blocks.mp3");
        director.successClip = LoadOptional<AudioClip>(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav");
        EditorUtility.SetDirty(burst);
        EditorUtility.SetDirty(director);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException("[S87] Unity could not save the scene: " + ScenePath);
        }
        AssetDatabase.SaveAssets();
        ValidateSharedSourceHashes(sourceHashes);
        Debug.Log(
            "[S87] Scene_87 created: vertical 9:16, four stair hops, joint trip, exact " +
            "150-One burst, 15 x 10 reassembly, no dialogue, and 18.2-second duration.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_87 — 50 + 100 stair fall",
                "Готово. Выберите Game View 9:16 и нажмите Play. Длительность — 18,2 секунды.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_87/Validate Stair Fall Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S87_VALIDATE] Scene is missing.", ScenePath);
        }
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S87_Main director = UnityEngine.Object.FindAnyObjectByType<S87_Main>();
        S87_UnitBurst burst = UnityEngine.Object.FindAnyObjectByType<S87_UnitBurst>();
        if (director == null || burst == null)
        {
            throw new InvalidOperationException("[S87_VALIDATE] Director or unit burst is missing.");
        }
        if (S87_Main.SequenceDuration > 20f ||
            !(S87_Main.HopStartTime < S87_Main.WarningTime &&
              S87_Main.WarningTime < S87_Main.TripTime &&
              S87_Main.TripTime < S87_Main.BurstTime &&
              S87_Main.BurstTime < S87_Main.ResultRevealTime &&
              S87_Main.ResultRevealTime < S87_Main.FinalLandingTime &&
              S87_Main.FinalLandingTime < S87_Main.SequenceDuration))
        {
            throw new InvalidOperationException("[S87_VALIDATE] Timeline exceeds 20 seconds or is unordered.");
        }
        bool hasSplitStep = director.brokenStep != null &&
            director.brokenStep.Find("Breaking Half Left") != null &&
            director.brokenStep.Find("Breaking Half Right") != null;
        bool hasLegacyStep = director.brokenStep != null &&
            director.brokenStep.GetComponent<Renderer>() != null;
        if (AssetDatabase.GetAssetPath(director.onePrefab) != OneSourcePath ||
            AssetDatabase.GetAssetPath(director.fiftyPrefab) != FiftyPrefabPath ||
            AssetDatabase.GetAssetPath(director.hundredPrefab) != HundredPrefabPath ||
            AssetDatabase.GetAssetPath(director.oneFiftyPrefab) != OneFiftyPrefabPath ||
            director.unitBurst != burst || burst.onePrefab != director.onePrefab ||
            burst.landingCollider == null || (!hasSplitStep && !hasLegacyStep))
        {
            throw new InvalidOperationException("[S87_VALIDATE] Scene references are incomplete.");
        }
        if (S87_UnitBurst.FiftyUnitCount != 50 ||
            S87_UnitBurst.HundredUnitCount != 100 ||
            S87_UnitBurst.TotalUnitCount != 150 ||
            S87_UnitBurst.ResultColumns != 15 ||
            S87_UnitBurst.RowCount != 10)
        {
            throw new InvalidOperationException("[S87_VALIDATE] Exact 50 + 100 = 150 geometry changed.");
        }
        Camera camera = Camera.main;
        if (camera == null || camera.orthographic || camera.name != "Main Camera — Vertical 9x16" ||
            Mathf.Abs(camera.fieldOfView - 43f) > 0.01f)
        {
            throw new InvalidOperationException("[S87_VALIDATE] Vertical camera is misconfigured.");
        }
        if (UnityEngine.Object.FindAnyObjectByType<TextMesh>() != null)
        {
            throw new InvalidOperationException("[S87_VALIDATE] This wordless short must not contain text.");
        }

        ValidateFaceTexture(FiftyTexturePath, 1101, 1056);
        ValidateFaceTexture(HundredTexturePath, 956, 956);
        ValidateFaceTexture(OneFiftyTexturePath, 1536, 1024);
        ValidateTransparentMaterial(FiftyMaterialPath, FiftyTexturePath);
        ValidateTransparentMaterial(HundredMaterialPath, HundredTexturePath);
        ValidateTransparentMaterial(OneFiftyMaterialPath, OneFiftyTexturePath);
        ValidateVariant(
            FiftyPrefabPath,
            FiftySourcePath,
            S87_Main.FiftyOriginalFaceName,
            S87_Main.FiftyStartledFaceName,
            FiftyMaterialPath);
        ValidateVariant(
            HundredPrefabPath,
            HundredSourcePath,
            S87_Main.HundredOriginalFaceName,
            S87_Main.HundredStartledFaceName,
            HundredMaterialPath);
        ValidateOneFiftyPrefab();
        Debug.Log(
            "[S87_VALIDATE] PASS — wordless 18.2s stair short, exact 150 units, valid scene-local " +
            "prefabs, face overlays and transparent PNGs.");
    }

    [MenuItem("Tools/Scene_87/Run Complete Play Mode Validation")]
    public static void RunPlayModeValidation()
    {
        ValidateScene();
        capturedFrames = new bool[CaptureTimes.Length];
        playModeStarted = false;
        playModePassed = false;
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        EditorApplication.update += PlayModeValidationTick;
        EditorApplication.isPlaying = true;
    }

    private static void PlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            playModeStartTime = EditorApplication.timeSinceStartup;
            playModeStarted = true;
            Time.timeScale = 2f;
            Debug.Log("[S87_PLAY] Entered Play Mode at 2x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            if (playModePassed)
            {
                Debug.Log(
                    $"[S87_PLAY] PASS — full short and all {CaptureTimes.Length} keyframes ran cleanly.");
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S87_PLAY] FAIL — the complete short did not reach its verified ending.");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }
    }

    private static void PlayModeValidationTick()
    {
        if (!playModeStarted || !EditorApplication.isPlaying)
        {
            return;
        }
        S87_Main director = UnityEngine.Object.FindAnyObjectByType<S87_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene87-{i + 1:00}.png");
                }
            }
            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() && director.CompletedHops == 4 &&
                    director.TripTriggered && director.BurstTriggered && director.ResultRevealed &&
                    director.FinalLandingTriggered && director.unitBurst.Started &&
                    director.unitBurst.FormationComplete;
                EditorApplication.isPlaying = false;
                return;
            }
        }
        if (EditorApplication.timeSinceStartup - playModeStartTime > 25d)
        {
            Debug.LogError("[S87_PLAY] Timed out after 25 real seconds.");
            playModePassed = false;
            EditorApplication.isPlaying = false;
        }
    }

    private static bool AllFramesCaptured()
    {
        foreach (bool captured in capturedFrames)
        {
            if (!captured) return false;
        }
        return true;
    }

    private static void CaptureRuntimeFrame(string path)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            throw new InvalidOperationException("[S87_PLAY] Main Camera disappeared.");
        }
        const int width = 540;
        const int height = 960;
        RenderTexture render = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture previousTarget = camera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        try
        {
            camera.targetTexture = render;
            camera.aspect = (float)width / height;
            camera.Render();
            RenderTexture.active = render;
            frame.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            frame.Apply();
            File.WriteAllBytes(path, frame.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            render.Release();
            UnityEngine.Object.DestroyImmediate(render);
            UnityEngine.Object.DestroyImmediate(frame);
        }
        Debug.Log("[S87_PLAY] Captured " + path);
    }

    private sealed class EnvironmentResult
    {
        public BoxCollider GroundCollider;
        public Transform BrokenStep;
    }

    private static EnvironmentResult CreateEnvironment(
        Material groundMaterial,
        Material stepBlue,
        Material stepGold,
        Material stepPurple,
        Material hillMaterial,
        Material cloudMaterial)
    {
        GameObject environment = new GameObject("S87 Giant Rainbow Staircase");
        GameObject ground = CreateCube(
            "Landing Ground",
            new Vector3(0f, -0.45f, -4.4f),
            new Vector3(18f, 0.9f, 11f),
            groundMaterial,
            environment.transform,
            true);
        BoxCollider groundCollider = ground.GetComponent<BoxCollider>();

        Material[] stepMaterials = { stepBlue, stepGold, stepPurple, stepBlue, stepGold };
        float[] topHeights = { 7.15f, 5.95f, 4.75f, 3.55f, 2.35f };
        float[] zPositions = { 7.45f, 5.05f, 2.65f, 0.25f, -2.15f };
        Transform brokenStep = null;
        for (int i = 0; i < topHeights.Length; i++)
        {
            if (i == topHeights.Length - 1)
            {
                GameObject stepRoot = new GameObject("Breakable Final Step");
                stepRoot.transform.SetParent(environment.transform, false);
                stepRoot.transform.position = new Vector3(0f, topHeights[i] - 0.55f, zPositions[i]);
                CreateCube(
                    "Breaking Half Left",
                    stepRoot.transform.position + Vector3.left * 2.325f,
                    new Vector3(4.55f, 1.1f, 2.45f),
                    stepMaterials[i],
                    stepRoot.transform,
                    false);
                CreateCube(
                    "Breaking Half Right",
                    stepRoot.transform.position + Vector3.right * 2.325f,
                    new Vector3(4.55f, 1.1f, 2.45f),
                    stepMaterials[i],
                    stepRoot.transform,
                    false);
                brokenStep = stepRoot.transform;
            }
            else
            {
                CreateCube(
                    $"Stair Step {i + 1:00}",
                    new Vector3(0f, topHeights[i] - 0.55f, zPositions[i]),
                    new Vector3(9.2f, 1.1f, 2.45f),
                    stepMaterials[i],
                    environment.transform,
                    false);
            }
        }

        CreateSphere(
            "Left Hill",
            new Vector3(-7.3f, 2.2f, 14f),
            new Vector3(9f, 5f, 2f),
            hillMaterial,
            environment.transform);
        CreateSphere(
            "Right Hill",
            new Vector3(7.6f, 1.6f, 15f),
            new Vector3(10f, 4.5f, 2f),
            hillMaterial,
            environment.transform);
        GameObject clouds = new GameObject("Soft Clouds");
        clouds.transform.SetParent(environment.transform, false);
        CreateCloud(new Vector3(-3.2f, 10.6f, 16f), 0.72f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(3.5f, 9.2f, 15f), 0.55f, cloudMaterial, clouds.transform);

        return new EnvironmentResult
        {
            GroundCollider = groundCollider,
            BrokenStep = brokenStep
        };
    }

    private static void CreateFiftyVariant(Material startledMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(FiftySourcePath);
        try
        {
            Material originalMaterial = LoadAsset<Material>(FiftyOriginalFaceMaterialPath);
            Renderer originalFace = FindRendererUsingMaterial(root, originalMaterial);
            if (originalFace == null)
            {
                throw new InvalidOperationException("[S87] Original Fifty face plane was not found.");
            }
            Vector3 outward = -originalFace.transform.forward.normalized;
            originalFace.gameObject.name = S87_Main.FiftyOriginalFaceName;
            ConfigureOverlay(originalFace.gameObject, originalMaterial);
            GameObject startled = UnityEngine.Object.Instantiate(
                originalFace.gameObject,
                originalFace.transform.parent,
                false);
            startled.name = S87_Main.FiftyStartledFaceName;
            ConfigureOverlay(startled, startledMaterial);
            startled.transform.position += outward * 0.004f;
            startled.SetActive(false);
            root.name = "Fifty_Stairs";
            PrefabUtility.SaveAsPrefabAsset(root, FiftyPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void CreateHundredVariant(Material startledMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(HundredSourcePath);
        try
        {
            Material originalMaterial = LoadAsset<Material>(HundredOriginalFaceMaterialPath);
            Material cleanMaterial = LoadAsset<Material>(HundredCleanFrontMaterialPath);
            Renderer front = FindRendererUsingMaterial(root, originalMaterial);
            if (front == null)
            {
                throw new InvalidOperationException("[S87] Original Hundred front plane was not found.");
            }
            Vector3 outward = -front.transform.forward.normalized;
            GameObject originalFace = UnityEngine.Object.Instantiate(
                front.gameObject,
                front.transform.parent,
                false);
            originalFace.name = S87_Main.HundredOriginalFaceName;
            ConfigureOverlay(originalFace, originalMaterial);
            originalFace.transform.position += outward * 0.004f;
            GameObject startled = UnityEngine.Object.Instantiate(
                originalFace,
                originalFace.transform.parent,
                false);
            startled.name = S87_Main.HundredStartledFaceName;
            ConfigureOverlay(startled, startledMaterial);
            startled.transform.position += outward * 0.003f;
            startled.SetActive(false);
            front.gameObject.name = "S87 100 Clean Front Body";
            ReplaceMaterial(front, originalMaterial, cleanMaterial);
            root.name = "Hundred_Stairs";
            PrefabUtility.SaveAsPrefabAsset(root, HundredPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void CreateOneFiftyVariant(Material faceMaterial)
    {
        GameObject root = new GameObject("OneFifty_Stairs");
        try
        {
            GameObject fiftyPart = PrefabUtility.InstantiatePrefab(
                LoadAsset<GameObject>(FiftyPrefabPath)) as GameObject;
            GameObject hundredPart = PrefabUtility.InstantiatePrefab(
                LoadAsset<GameObject>(HundredPrefabPath)) as GameObject;
            if (fiftyPart == null || hundredPart == null)
            {
                throw new InvalidOperationException("[S87] Could not instantiate the 50/100 scene variants.");
            }
            fiftyPart.name = "Original Numberblock 50 Part";
            hundredPart.name = "Original Numberblock 100 Part";
            fiftyPart.transform.SetParent(root.transform, false);
            hundredPart.transform.SetParent(root.transform, false);
            DisableFaceLayers(fiftyPart);
            DisableFaceLayers(hundredPart);
            fiftyPart.transform.localScale = Vector3.one * 0.45f;
            hundredPart.transform.localScale = Vector3.one * 0.45f;

            Bounds fiftyBounds = S87_Main.BoundsOf(fiftyPart);
            Bounds hundredBounds = S87_Main.BoundsOf(hundredPart);
            PlaceBottomCenter(fiftyPart, new Vector3(-hundredBounds.size.x * 0.5f, 0f, 0f));
            PlaceBottomCenter(hundredPart, new Vector3(fiftyBounds.size.x * 0.5f, 0f, 0f));
            Bounds combined = Encapsulate(S87_Main.BoundsOf(fiftyPart), S87_Main.BoundsOf(hundredPart));

            GameObject face = GameObject.CreatePrimitive(PrimitiveType.Quad);
            face.name = "S87 150 Delighted Face";
            face.transform.SetParent(root.transform, true);
            face.transform.position = new Vector3(
                combined.center.x,
                combined.center.y,
                combined.min.z - 0.018f);
            face.transform.localScale = new Vector3(combined.size.x, combined.size.y, 1f);
            ConfigureOverlay(face, faceMaterial);

            PrefabUtility.SaveAsPrefabAsset(root, OneFiftyPrefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void DisableFaceLayers(GameObject target)
    {
        foreach (Transform child in target.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.StartsWith("S87 ") && child.name.EndsWith(" Face"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private static void ConfigureOverlay(GameObject overlay, Material material)
    {
        Renderer renderer = overlay.GetComponent<Renderer>();
        if (renderer == null)
        {
            throw new InvalidOperationException("[S87] Face overlay has no Renderer.");
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
            throw new InvalidOperationException("[S87] Original face material was not assigned.");
        }
        renderer.sharedMaterials = materials;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Vertical 9x16");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.orthographic = false;
        camera.fieldOfView = 43f;
        camera.aspect = 9f / 16f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 160f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.35f, 0.76f, 0.96f);
        Vector3 target = new Vector3(0f, 5.4f, 3.9f);
        camera.transform.position = target + new Vector3(0f, 3.25f, -22.5f);
        camera.transform.LookAt(target);
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
        RenderSettings.ambientLight = new Color(0.64f, 0.64f, 0.64f);
    }

    private static GameObject CreateCube(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent,
        bool keepCollider)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;
        cube.transform.SetParent(parent, true);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        if (!keepCollider) RemoveColliders(cube);
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

    private static void CreateCloud(Vector3 center, float scale, Material material, Transform parent)
    {
        CreateSphere("Cloud Puff", center + Vector3.left * 0.72f * scale,
            new Vector3(1.45f, 0.72f, 0.35f) * scale, material, parent);
        CreateSphere("Cloud Puff", center + Vector3.up * 0.22f * scale,
            new Vector3(1.55f, 0.92f, 0.38f) * scale, material, parent);
        CreateSphere("Cloud Puff", center + Vector3.right * 0.78f * scale,
            new Vector3(1.38f, 0.68f, 0.34f) * scale, material, parent);
    }

    private static Material CreateTransparentMaterial(string path, Texture2D texture)
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        shader ??= Shader.Find("Sprites/Default");
        if (shader == null) throw new InvalidOperationException("[S87] No transparent shader is available.");
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

    private static Material CreateTransparentColorMaterial(string path, Color color)
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        shader ??= Shader.Find("Sprites/Default");
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
        SetMaterialColor(material, color);
        ConfigureTransparentBlend(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateOpaqueMaterial(string path, Color color)
    {
        Material template = GraphicsSettings.currentRenderPipeline != null
            ? GraphicsSettings.currentRenderPipeline.defaultMaterial
            : null;
        Shader shader = template == null ? Shader.Find("Standard") : template.shader;
        shader ??= Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null) throw new InvalidOperationException("[S87] No opaque shader is available.");
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
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        if (material.HasProperty("_DstBlend"))
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        material.color = color;
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0f);
    }

    private static void SetTextureIfPresent(Material material, string property, Texture texture)
    {
        if (material.HasProperty(property)) material.SetTexture(property, texture);
    }

    private static void ConfigureFaceTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new FileNotFoundException("[S87] Face texture was not imported.", path);
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

    private static void ValidateFaceTexture(string path, int width, int height)
    {
        byte[] bytes = File.ReadAllBytes(path);
        if (bytes.Length < 26 || bytes[25] != 6)
            throw new InvalidOperationException("[S87_FACE] Expected RGBA PNG: " + path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!texture.LoadImage(bytes, false) || texture.width != width || texture.height != height)
                throw new InvalidOperationException("[S87_FACE] Wrong PNG size: " + path);
            Color32[] pixels = texture.GetPixels32();
            if (pixels[0].a != 0 || pixels[width - 1].a != 0 ||
                pixels[(height - 1) * width].a != 0 || pixels[pixels.Length - 1].a != 0)
                throw new InvalidOperationException("[S87_FACE] Transparent corners are missing: " + path);
            int transparent = 0;
            int visible = 0;
            foreach (Color32 pixel in pixels)
            {
                if (pixel.a == 0) transparent++;
                if (pixel.a > 220) visible++;
            }
            if (transparent < pixels.Length / 5 || visible < pixels.Length / 100)
                throw new InvalidOperationException("[S87_FACE] Invalid visible/transparent balance: " + path);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || !importer.alphaIsTransparency || importer.mipmapEnabled ||
            importer.npotScale != TextureImporterNPOTScale.None || importer.wrapMode != TextureWrapMode.Clamp ||
            importer.filterMode != FilterMode.Bilinear ||
            importer.textureCompression != TextureImporterCompression.Uncompressed)
            throw new InvalidOperationException("[S87_FACE] Import settings changed: " + path);
    }

    private static void ValidateTransparentMaterial(string materialPath, string texturePath)
    {
        Material material = LoadAsset<Material>(materialPath);
        if (material.mainTexture != LoadAsset<Texture2D>(texturePath) ||
            material.renderQueue < (int)RenderQueue.Transparent ||
            (material.HasProperty("_ZWrite") && material.GetFloat("_ZWrite") != 0f))
            throw new InvalidOperationException("[S87_FACE] Invalid transparent material: " + materialPath);
    }

    private static void ValidateVariant(
        string prefabPath,
        string sourcePath,
        string originalFaceName,
        string newFaceName,
        string materialPath)
    {
        GameObject variant = PrefabUtility.LoadPrefabContents(prefabPath);
        GameObject source = PrefabUtility.LoadPrefabContents(sourcePath);
        try
        {
            Transform originalFace = FindChild(variant.transform, originalFaceName);
            Transform newFace = FindChild(variant.transform, newFaceName);
            if (originalFace == null || newFace == null)
                throw new InvalidOperationException("[S87_FACE] Missing face layers: " + prefabPath);
            Renderer renderer = newFace.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterial != LoadAsset<Material>(materialPath) ||
                renderer.shadowCastingMode != ShadowCastingMode.Off || renderer.receiveShadows)
                throw new InvalidOperationException("[S87_FACE] Invalid overlay renderer: " + prefabPath);
            Vector3 tangentDelta = newFace.position - originalFace.position;
            tangentDelta -= Vector3.Project(tangentDelta, originalFace.forward);
            if (tangentDelta.magnitude > 0.003f ||
                Quaternion.Angle(newFace.rotation, originalFace.rotation) > 0.01f ||
                (newFace.lossyScale - originalFace.lossyScale).magnitude > 0.01f)
                throw new InvalidOperationException("[S87_FACE] Overlay moved off the source plane: " + prefabPath);
            Vector3 sourceSize = S87_Main.BoundsOf(source).size;
            Vector3 variantSize = S87_Main.BoundsOf(variant).size;
            if ((sourceSize - variantSize).magnitude > 0.1f)
                throw new InvalidOperationException("[S87_FACE] Variant changed source geometry: " + prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(variant);
            PrefabUtility.UnloadPrefabContents(source);
        }
    }

    private static void ValidateOneFiftyPrefab()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(OneFiftyPrefabPath);
        try
        {
            Transform face = FindChild(root.transform, "S87 150 Delighted Face");
            if (face == null) throw new InvalidOperationException("[S87_FACE] 150 face is missing.");
            Renderer renderer = face.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterial != LoadAsset<Material>(OneFiftyMaterialPath) ||
                renderer.shadowCastingMode != ShadowCastingMode.Off || renderer.receiveShadows)
                throw new InvalidOperationException("[S87_FACE] 150 overlay is invalid.");
            Bounds bounds = S87_Main.BoundsOf(root);
            if (Mathf.Abs(bounds.size.x / bounds.size.y - 1.5f) > 0.08f)
                throw new InvalidOperationException("[S87_FACE] 150 body is not a 15 x 10 composition.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Renderer FindRendererUsingMaterial(GameObject root, Material material)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        foreach (Material candidate in renderer.sharedMaterials)
        {
            if (candidate == material) return renderer;
        }
        return null;
    }

    private static Transform FindChild(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name) return child;
        }
        return null;
    }

    private static void PlaceBottomCenter(GameObject target, Vector3 desired)
    {
        Bounds bounds = S87_Main.BoundsOf(target);
        Vector3 current = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        target.transform.position += desired - current;
    }

    private static Bounds Encapsulate(Bounds a, Bounds b)
    {
        a.Encapsulate(b.min);
        a.Encapsulate(b.max);
        return a;
    }

    private static void RemoveColliders(GameObject target)
    {
        foreach (Collider collider in target.GetComponents<Collider>())
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene87");
        EnsureFolder(AssetRoot, "FaceTextures");
        EnsureFolder(AssetRoot, "Materials");
        EnsureFolder(AssetRoot, "Prefabs");
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new FileNotFoundException("[S87] Asset is missing.", path);
        return asset;
    }

    private static T LoadOptional<T>(string path) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private static Dictionary<string, string> HashSharedSources()
    {
        Dictionary<string, string> hashes = new Dictionary<string, string>();
        foreach (string path in SharedSourcePaths) hashes[path] = Hash(path);
        return hashes;
    }

    private static void ValidateSharedSourceHashes(Dictionary<string, string> before)
    {
        foreach (KeyValuePair<string, string> item in before)
        {
            if (Hash(item.Key) != item.Value)
                throw new InvalidOperationException("[S87] Shared source changed: " + item.Key);
        }
    }

    private static string Hash(string assetPath)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = File.ReadAllBytes(assetPath);
        return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
    }
}
