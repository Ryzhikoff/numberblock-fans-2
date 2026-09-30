using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the long perspective Scene 83 short.</summary>
public static class S83_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_83.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene83";
    private const string TextureRoot = AssetRoot + "/FaceTextures";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string PrefabRoot = AssetRoot + "/Prefabs";

    private const string OneSourcePath = "Assets/Prefabs/Blocks/1.prefab";
    private const string TenSourcePath = "Assets/Prefabs/Blocks/10.prefab";
    private const string FiftySourcePath = "Assets/Prefabs/Blocks/50.prefab";
    private const string HundredSourcePath = "Assets/Prefabs/Blocks/100.prefab";
    private const string FiveHundredSourcePath = "Assets/Prefabs/Blocks/500.prefab";
    private const string ThousandSourcePath = "Assets/Prefabs/Blocks/1000.prefab";

    private const string TenFaceMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/tenFace.mat";
    private const string TenCleanMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/tenBack.mat";
    private const string FiftyFaceMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/50-face.mat";
    private const string HundredFaceMaterialPath = "Assets/Materials/Materials/11.mat";
    private const string HundredCleanMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/newMillion 1.mat";
    private const string FiveHundredFaceMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/500-face.mat";
    private const string FiveHundredCleanMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/500-back.mat";
    private const string ThousandFaceMaterialPath =
        "Assets/Materials/Materials/oneThousandFace.mat";
    private const string ThousandCleanMaterialPath =
        "Assets/Materials/Materials/OneThousandTexture 1.mat";

    private const string TenDeterminedTexturePath = TextureRoot + "/Ten_Determined.png";
    private const string TenSurprisedTexturePath = TextureRoot + "/Ten_Surprised.png";
    private const string FiftySurprisedTexturePath = TextureRoot + "/Fifty_Surprised.png";
    private const string HundredSurprisedTexturePath = TextureRoot + "/Hundred_Surprised.png";
    private const string FiveHundredShockedTexturePath = TextureRoot + "/FiveHundred_Shocked.png";
    private const string ThousandVictoryTexturePath = TextureRoot + "/Thousand_Victory.png";

    private const string TenPrefabPath = PrefabRoot + "/Ten_Expressions.prefab";
    private const string FiftyPrefabPath = PrefabRoot + "/Fifty_Expressions.prefab";
    private const string HundredPrefabPath = PrefabRoot + "/Hundred_Expressions.prefab";
    private const string FiveHundredPrefabPath = PrefabRoot + "/FiveHundred_Expressions.prefab";
    private const string ThousandPrefabPath = PrefabRoot + "/Thousand_Expressions.prefab";

    private static readonly string[] ProgressionPrefabPaths =
    {
        TenPrefabPath,
        "Assets/Prefabs/Blocks/20.prefab",
        "Assets/Prefabs/Blocks/30.prefab",
        "Assets/Prefabs/Blocks/40.prefab",
        FiftyPrefabPath,
        "Assets/Prefabs/Blocks/60.prefab",
        "Assets/Prefabs/Blocks/70.prefab",
        "Assets/Prefabs/Blocks/80.prefab",
        "Assets/Prefabs/Blocks/90.prefab",
        HundredPrefabPath,
        "Assets/Prefabs/Blocks/200.prefab",
        "Assets/Prefabs/Blocks/300.prefab",
        "Assets/Prefabs/Blocks/400.prefab",
        FiveHundredPrefabPath,
        "Assets/Prefabs/Blocks/600.prefab",
        "Assets/Prefabs/Blocks/700.prefab",
        "Assets/Prefabs/Blocks/800.prefab",
        "Assets/Prefabs/Blocks/900.prefab",
        ThousandPrefabPath
    };

    private static readonly float[] CaptureTimes =
    {
        1.2f, 12f, 24f, 36f, 48f, 60f, 72f, 84f, 96f, 108f, 120f, 132f, 144f, 154f, 161.2f
    };

    private static bool[] capturedFrames;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static double playModeStartTime;

    private sealed class ExpressionSpec
    {
        public string ObjectName;
        public Material Material;
    }

    [MenuItem("Tools/Scene_83/Quick Setup 10 To 1000 Collision Journey")]
    public static void QuickSetup()
    {
        Scene openScene = SceneManager.GetActiveScene();
        if (!Application.isBatchMode && openScene.IsValid() && openScene.isDirty &&
            !string.IsNullOrEmpty(openScene.path))
        {
            if (!EditorSceneManager.SaveScene(openScene))
            {
                throw new IOException($"[S83] Could not preserve the open scene: {openScene.path}");
            }
            Debug.Log($"[S83] Preserved open scene changes before setup: {openScene.path}");
        }
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        ConfigureFaceTexture(TenDeterminedTexturePath);
        ConfigureFaceTexture(TenSurprisedTexturePath);
        ConfigureFaceTexture(FiftySurprisedTexturePath);
        ConfigureFaceTexture(HundredSurprisedTexturePath);
        ConfigureFaceTexture(FiveHundredShockedTexturePath);
        ConfigureFaceTexture(ThousandVictoryTexturePath);

        Material tenDetermined = CreateTransparentFaceMaterial(
            MaterialRoot + "/Ten_Determined_Transparent.mat",
            LoadAsset<Texture2D>(TenDeterminedTexturePath));
        Material tenSurprised = CreateTransparentFaceMaterial(
            MaterialRoot + "/Ten_Surprised_Transparent.mat",
            LoadAsset<Texture2D>(TenSurprisedTexturePath));
        Material fiftySurprised = CreateTransparentFaceMaterial(
            MaterialRoot + "/Fifty_Surprised_Transparent.mat",
            LoadAsset<Texture2D>(FiftySurprisedTexturePath));
        Material hundredSurprised = CreateTransparentFaceMaterial(
            MaterialRoot + "/Hundred_Surprised_Transparent.mat",
            LoadAsset<Texture2D>(HundredSurprisedTexturePath));
        Material fiveHundredShocked = CreateTransparentFaceMaterial(
            MaterialRoot + "/FiveHundred_Shocked_Transparent.mat",
            LoadAsset<Texture2D>(FiveHundredShockedTexturePath));
        Material thousandVictory = CreateTransparentFaceMaterial(
            MaterialRoot + "/Thousand_Victory_Transparent.mat",
            LoadAsset<Texture2D>(ThousandVictoryTexturePath));

        CreateExpressionVariant(
            TenSourcePath,
            TenPrefabPath,
            TenFaceMaterialPath,
            TenCleanMaterialPath,
            new ExpressionSpec
            {
                ObjectName = S83_Main.DeterminedFaceName,
                Material = tenDetermined
            },
            new ExpressionSpec
            {
                ObjectName = S83_Main.SurprisedFaceName,
                Material = tenSurprised
            });
        CreateExpressionVariant(
            FiftySourcePath,
            FiftyPrefabPath,
            FiftyFaceMaterialPath,
            null,
            new ExpressionSpec
            {
                ObjectName = S83_Main.SurprisedFaceName,
                Material = fiftySurprised
            });
        CreateExpressionVariant(
            HundredSourcePath,
            HundredPrefabPath,
            HundredFaceMaterialPath,
            HundredCleanMaterialPath,
            new ExpressionSpec
            {
                ObjectName = S83_Main.SurprisedFaceName,
                Material = hundredSurprised
            });
        CreateExpressionVariant(
            FiveHundredSourcePath,
            FiveHundredPrefabPath,
            FiveHundredFaceMaterialPath,
            FiveHundredCleanMaterialPath,
            new ExpressionSpec
            {
                ObjectName = S83_Main.SurprisedFaceName,
                Material = fiveHundredShocked
            });
        CreateExpressionVariant(
            ThousandSourcePath,
            ThousandPrefabPath,
            ThousandFaceMaterialPath,
            ThousandCleanMaterialPath,
            new ExpressionSpec
            {
                ObjectName = S83_Main.VictoryFaceName,
                Material = thousandVictory
            });

        Material grass = CreateOpaqueMaterial(
            MaterialRoot + "/S83_Grass.mat", new Color(0.22f, 0.63f, 0.25f));
        Material dirt = CreateOpaqueMaterial(
            MaterialRoot + "/S83_Dirt.mat", new Color(0.38f, 0.2f, 0.095f));
        Material stone = CreateOpaqueMaterial(
            MaterialRoot + "/S83_Stone.mat", new Color(0.25f, 0.29f, 0.33f));
        Material darkStone = CreateOpaqueMaterial(
            MaterialRoot + "/S83_DarkStone.mat", new Color(0.085f, 0.1f, 0.13f));
        Material rail = CreateOpaqueMaterial(
            MaterialRoot + "/S83_Rail.mat", new Color(0.63f, 0.66f, 0.68f));
        Material timber = CreateOpaqueMaterial(
            MaterialRoot + "/S83_Timber.mat", new Color(0.37f, 0.18f, 0.07f));
        Material torch = CreateEmissiveMaterial(
            MaterialRoot + "/S83_Torch.mat", new Color(1f, 0.48f, 0.08f), 1.5f);
        Material flash = CreateTransparentColorMaterial(
            MaterialRoot + "/S83_Flash.mat", new Color(1f, 0.82f, 0.22f, 0.72f));
        Material dust = CreateOpaqueMaterial(
            MaterialRoot + "/S83_Dust.mat", new Color(0.72f, 0.48f, 0.19f));
        Material crack = CreateOpaqueMaterial(
            MaterialRoot + "/S83_Crack.mat", new Color(0.025f, 0.022f, 0.02f));
        Material progressOff = CreateOpaqueMaterial(
            MaterialRoot + "/S83_ProgressOff.mat", new Color(0.12f, 0.15f, 0.18f));
        Material progressOn = CreateEmissiveMaterial(
            MaterialRoot + "/S83_ProgressOn.mat", new Color(0.2f, 0.9f, 0.96f), 1.25f);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateCamera();
        CreateLighting();
        CreateMinecraftEnvironment(grass, dirt, stone, darkStone, rail, timber, torch);
        TextMesh title = CreateText("Title — Hook", new Vector3(0f, 9.25f, 0f), 74, 0.086f);
        TextMesh equation = CreateText("Equation — Current Collision", new Vector3(0f, 8.15f, 0f), 96, 0.09f);
        TextMesh stage = CreateText("Stage — Progress", new Vector3(0f, 7.22f, 0f), 56, 0.073f);
        Transform progressFill = CreateProgressTrack(progressOff, progressOn);
        AddBackgroundMusic(camera.gameObject);

        GameObject burstObject = new GameObject("Exact Original Ones — Milestone Bursts");
        S83_UnitBurst burst = burstObject.AddComponent<S83_UnitBurst>();
        burst.onePrefab = LoadAsset<GameObject>(OneSourcePath);
        burst.floorY = 0.12f;

        GameObject manager = new GameObject("GameManager — 10 To 1000 Timeline");
        S83_Main director = manager.AddComponent<S83_Main>();
        director.onePrefab = LoadAsset<GameObject>(OneSourcePath);
        director.progressionPrefabs = new GameObject[ProgressionPrefabPaths.Length];
        for (int i = 0; i < ProgressionPrefabPaths.Length; i++)
        {
            director.progressionPrefabs[i] = LoadAsset<GameObject>(ProgressionPrefabPaths[i]);
        }
        director.unitBurst = burst;
        director.titleText = title;
        director.equationText = equation;
        director.stageText = stage;
        director.progressFillRoot = progressFill;
        director.flashMaterial = flash;
        director.dustMaterial = dust;
        director.crackMaterial = crack;
        director.whooshClip = LoadOptional<AudioClip>("Assets/Sound/zvuk-priblijeniya.mp3");
        director.impactClip = LoadOptional<AudioClip>("Assets/Sound/collCube.wav");
        director.impactAccentClip = LoadOptional<AudioClip>("Assets/Sound/boom_metal.wav");
        director.explosionClip = LoadOptional<AudioClip>("Assets/Sound/destroy_blocks.mp3");
        director.successClip = LoadOptional<AudioClip>(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav");
        EditorUtility.SetDirty(burst);
        EditorUtility.SetDirty(director);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S83] Unity could not save the scene: {ScenePath}");
        }
        AssetDatabase.SaveAssets();
        Debug.Log(
            "[S83] Scene_83 created — perspective Minecraft arena, 18 additions, " +
            "36 impacts, 18 exact bursts, reduced lighting, and six RGBA face textures.");
    }

    [MenuItem("Tools/Scene_83/Validate 10 To 1000 Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S83_VALIDATE] Scene is missing.", ScenePath);
        }
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S83_Main director = UnityEngine.Object.FindAnyObjectByType<S83_Main>();
        S83_UnitBurst burst = UnityEngine.Object.FindAnyObjectByType<S83_UnitBurst>();
        if (director == null || burst == null || director.unitBurst != burst)
        {
            throw new InvalidOperationException("[S83_VALIDATE] Director or exact unit burst is missing.");
        }
        if (director.progressionPrefabs == null ||
            director.progressionPrefabs.Length != ProgressionPrefabPaths.Length)
        {
            throw new InvalidOperationException("[S83_VALIDATE] Progression prefab list is incomplete.");
        }
        for (int i = 0; i < ProgressionPrefabPaths.Length; i++)
        {
            if (AssetDatabase.GetAssetPath(director.progressionPrefabs[i]) != ProgressionPrefabPaths[i])
            {
                throw new InvalidOperationException($"[S83_VALIDATE] Wrong prefab at progression index {i}.");
            }
        }
        Camera camera = Camera.main;
        if (camera == null || camera.orthographic || Mathf.Abs(camera.fieldOfView - 42f) > 0.01f ||
            camera.name != "Main Camera — Perspective Adaptive")
        {
            throw new InvalidOperationException("[S83_VALIDATE] Perspective camera is misconfigured.");
        }
        if (GameObject.Find("S83 Minecraft Foreground") == null ||
            director.progressFillRoot == null || director.progressFillRoot.childCount != S83_Main.RoundCount)
        {
            throw new InvalidOperationException("[S83_VALIDATE] Minecraft staging or progress track is incomplete.");
        }

        ValidateFaceTexture(TenDeterminedTexturePath, 154, 379);
        ValidateFaceTexture(TenSurprisedTexturePath, 154, 379);
        ValidateFaceTexture(FiftySurprisedTexturePath, 1101, 1056);
        ValidateFaceTexture(HundredSurprisedTexturePath, 956, 956);
        ValidateFaceTexture(FiveHundredShockedTexturePath, 521, 521);
        ValidateFaceTexture(ThousandVictoryTexturePath, 380, 380);
        ValidateVariant(TenPrefabPath, true, true, false);
        ValidateVariant(FiftyPrefabPath, false, true, false);
        ValidateVariant(HundredPrefabPath, false, true, false);
        ValidateVariant(FiveHundredPrefabPath, false, true, false);
        ValidateVariant(ThousandPrefabPath, false, false, true);

        Light keyLight = GameObject.Find("Moonlit Key Light")?.GetComponent<Light>();
        if (keyLight == null || keyLight.intensity > 0.76f)
        {
            throw new InvalidOperationException("[S83_VALIDATE] Main light is too bright.");
        }
        if (S83_Main.RoundCount != 18 || S83_Main.ExpectedPhysicalImpacts != 36 ||
            S83_Main.ExpectedBurstCount != 18 ||
            S83_UnitBurst.MaximumUnitCount != 1000)
        {
            throw new InvalidOperationException("[S83_VALIDATE] Timeline counts changed.");
        }
        Debug.Log(
            "[S83_VALIDATE] PASS — adaptive perspective camera, Minecraft foreground, 18 rounds, " +
            "two impacts and one exact burst per round, reduced lighting, source prefabs " +
            "preserved, six transparent face overlays.");
    }

    /// <summary>Runs the full extended scene at 4x and captures key frames.</summary>
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S83_PLAY] This entry point is batch-mode only.");
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
            Debug.Log("[S83_PLAY] Entered Play Mode at 4x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            if (playModePassed)
            {
                Debug.Log($"[S83_PLAY] PASS — complete short and {CaptureTimes.Length} keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S83_PLAY] FAIL — complete short did not reach its verified ending.");
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
        S83_Main director = UnityEngine.Object.FindAnyObjectByType<S83_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene83-{i + 1:00}.png");
                }
            }
            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() &&
                    director.CompletedRoundCount == S83_Main.RoundCount &&
                    director.CollisionCount == S83_Main.ExpectedPhysicalImpacts &&
                    director.BurstCount == S83_Main.ExpectedBurstCount &&
                    director.LastResultValue == 1000 && director.CustomExpressionShows >= 20 &&
                    director.FinalGroundCrack && director.unitBurst.LastCompletedCount == 1000;
                EditorApplication.isPlaying = false;
                return;
            }
        }
        if (EditorApplication.timeSinceStartup - playModeStartTime > 52d)
        {
            Debug.LogError("[S83_PLAY] Timed out after 52 real seconds.");
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
        if (camera == null) throw new InvalidOperationException("[S83_PLAY] Main Camera disappeared.");
        const int width = 960;
        const int height = 540;
        RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D frame = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        float previousAspect = camera.aspect;
        try
        {
            camera.targetTexture = target;
            camera.aspect = (float)width / height;
            camera.Render();
            RenderTexture.active = target;
            frame.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            frame.Apply();
            File.WriteAllBytes(path, frame.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            if (previousTarget == null)
            {
                camera.ResetAspect();
            }
            else
            {
                camera.aspect = previousAspect;
            }
            RenderTexture.active = previousActive;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(frame);
        }
        Debug.Log($"[S83_PLAY] Captured {path}");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene83");
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
            throw new FileNotFoundException("[S83] Face texture was not imported.", path);
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
        Material material = CreateOrLoadMaterial(path, true);
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
        Material material = CreateOrLoadMaterial(path, true);
        material.mainTexture = null;
        SetTextureIfPresent(material, "_BaseMap", null);
        SetTextureIfPresent(material, "_MainTex", null);
        SetMaterialColor(material, color);
        ConfigureTransparentBlend(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateOpaqueMaterial(string path, Color color)
    {
        Material material = CreateOrLoadMaterial(path, false);
        SetMaterialColor(material, color);
        material.renderQueue = -1;
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 0f);
        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 1f);
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateEmissiveMaterial(string path, Color color, float intensity)
    {
        Material material = CreateOpaqueMaterial(path, color);
        Color emission = color * intensity;
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
            material.EnableKeyword("_EMISSION");
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateOrLoadMaterial(string path, bool transparent)
    {
        Shader shader = transparent && GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/Lit")
                : transparent ? Shader.Find("Unlit/Transparent") : Shader.Find("Standard");
        shader ??= Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
        if (shader == null)
        {
            throw new InvalidOperationException("[S83] No compatible material shader is available.");
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
        return material;
    }

    private static void ConfigureTransparentBlend(Material material)
    {
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
        if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.renderQueue = (int)RenderQueue.Transparent;
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private static void SetTextureIfPresent(Material material, string property, Texture texture)
    {
        if (material.HasProperty(property)) material.SetTexture(property, texture);
    }

    private static void CreateExpressionVariant(
        string sourcePath,
        string destinationPath,
        string originalMaterialPath,
        string cleanMaterialPath,
        params ExpressionSpec[] expressions)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(sourcePath);
        if (root == null)
        {
            throw new FileNotFoundException("[S83] Source Numberblock prefab is missing.", sourcePath);
        }
        try
        {
            Material originalMaterial = LoadAsset<Material>(originalMaterialPath);
            Renderer front = FindRendererUsingMaterial(root, originalMaterial);
            if (front == null)
            {
                throw new InvalidOperationException($"[S83] Face renderer not found in {sourcePath}.");
            }
            Vector3 outward = -front.transform.forward.normalized;
            GameObject originalFace;
            if (string.IsNullOrEmpty(cleanMaterialPath))
            {
                originalFace = front.gameObject;
                originalFace.name = S83_Main.OriginalFaceName;
                ConfigureOverlay(originalFace, originalMaterial);
            }
            else
            {
                Material cleanMaterial = LoadAsset<Material>(cleanMaterialPath);
                originalFace = UnityEngine.Object.Instantiate(
                    front.gameObject, front.transform.parent, false);
                originalFace.name = S83_Main.OriginalFaceName;
                ConfigureOverlay(originalFace, originalMaterial);
                originalFace.transform.position += outward * 0.004f;
                ReplaceMaterial(front, originalMaterial, cleanMaterial);
                front.gameObject.name = "S83 Clean Front Body";
            }

            for (int i = 0; i < expressions.Length; i++)
            {
                GameObject overlay = UnityEngine.Object.Instantiate(
                    originalFace, originalFace.transform.parent, false);
                overlay.name = expressions[i].ObjectName;
                ConfigureOverlay(overlay, expressions[i].Material);
                overlay.transform.position += outward * (0.004f + i * 0.003f);
                overlay.SetActive(false);
            }
            root.name = Path.GetFileNameWithoutExtension(destinationPath);
            PrefabUtility.SaveAsPrefabAsset(root, destinationPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Renderer FindRendererUsingMaterial(GameObject root, Material material)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material candidate in renderer.sharedMaterials)
            {
                if (candidate == material) return renderer;
            }
        }
        return null;
    }

    private static void ConfigureOverlay(GameObject overlay, Material material)
    {
        Renderer renderer = overlay.GetComponent<Renderer>();
        if (renderer == null) throw new InvalidOperationException("[S83] Face overlay has no Renderer.");
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = 30;
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
        if (!replaced) throw new InvalidOperationException("[S83] Source face material was not assigned.");
        renderer.sharedMaterials = materials;
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Perspective Adaptive");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.orthographic = false;
        camera.fieldOfView = 42f;
        camera.ResetAspect();
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 150f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.16f, 0.25f);
        camera.transform.position = new Vector3(0f, 5.15f, -22f);
        camera.transform.LookAt(new Vector3(0f, 3.75f, 0f));
        return camera;
    }

    private static void CreateLighting()
    {
        GameObject keyObject = new GameObject("Moonlit Key Light");
        Light key = keyObject.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 0.72f;
        key.shadows = LightShadows.Soft;
        key.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        GameObject rimObject = new GameObject("Warm Torch Rim");
        Light rim = rimObject.AddComponent<Light>();
        rim.type = LightType.Point;
        rim.color = new Color(1f, 0.48f, 0.18f);
        rim.intensity = 1.6f;
        rim.range = 13f;
        rim.transform.position = new Vector3(-4f, 4f, -3f);
        RenderSettings.ambientLight = new Color(0.28f, 0.31f, 0.36f);
    }

    private static void CreateMinecraftEnvironment(
        Material grass,
        Material dirt,
        Material stone,
        Material darkStone,
        Material rail,
        Material timber,
        Material torch)
    {
        GameObject root = new GameObject("S83 Minecraft Foreground");
        CreateCube("Grass Arena", new Vector3(0f, -0.38f, 4f), new Vector3(18f, 0.7f, 30f), grass, root.transform);
        CreateCube("Dirt Underlayer", new Vector3(0f, -1.05f, 4f), new Vector3(18f, 0.7f, 30f), dirt, root.transform);

        for (int z = -8; z <= 10; z += 2)
        {
            CreateCube("Left Foreground Stone", new Vector3(-4.45f, 0.35f, z), new Vector3(1.35f, 1.35f, 1.85f), stone, root.transform);
            CreateCube("Right Foreground Stone", new Vector3(4.45f, 0.35f, z), new Vector3(1.35f, 1.35f, 1.85f), stone, root.transform);
            if (z % 4 == 0)
            {
                CreateCube("Left Grass Cap", new Vector3(-4.45f, 1.08f, z), new Vector3(1.35f, 0.14f, 1.85f), grass, root.transform);
                CreateCube("Right Grass Cap", new Vector3(4.45f, 1.08f, z), new Vector3(1.35f, 0.14f, 1.85f), grass, root.transform);
            }
        }

        for (int z = -9; z <= 11; z++)
        {
            CreateCube("Left Rail", new Vector3(-0.58f, 0.08f, z), new Vector3(0.09f, 0.07f, 0.92f), rail, root.transform);
            CreateCube("Right Rail", new Vector3(0.58f, 0.08f, z), new Vector3(0.09f, 0.07f, 0.92f), rail, root.transform);
            if (z % 2 == 0)
            {
                CreateCube("Rail Tie", new Vector3(0f, 0.035f, z), new Vector3(1.55f, 0.08f, 0.18f), timber, root.transform);
            }
        }

        CreateCube("Back Tunnel Left", new Vector3(-3.5f, 3.1f, 10.5f), new Vector3(2.2f, 6.2f, 1.6f), darkStone, root.transform);
        CreateCube("Back Tunnel Right", new Vector3(3.5f, 3.1f, 10.5f), new Vector3(2.2f, 6.2f, 1.6f), darkStone, root.transform);
        CreateCube("Back Tunnel Beam", new Vector3(0f, 6.45f, 10.5f), new Vector3(9.2f, 1.1f, 1.6f), darkStone, root.transform);
        CreateCube("Distant 1000 Silhouette", new Vector3(0f, 2.2f, 10.2f), new Vector3(4.3f, 4.4f, 0.55f), stone, root.transform);

        CreateTorch(new Vector3(-3.65f, 3.1f, 2.2f), timber, torch, root.transform);
        CreateTorch(new Vector3(3.65f, 3.1f, 2.2f), timber, torch, root.transform);
        CreateTorch(new Vector3(-3.65f, 3.1f, 7.5f), timber, torch, root.transform);
        CreateTorch(new Vector3(3.65f, 3.1f, 7.5f), timber, torch, root.transform);
    }

    private static void CreateTorch(Vector3 position, Material timber, Material torch, Transform parent)
    {
        CreateCube("Torch Handle", position, new Vector3(0.13f, 0.72f, 0.13f), timber, parent);
        CreateCube("Pixel Flame", position + Vector3.up * 0.48f, new Vector3(0.34f, 0.42f, 0.34f), torch, parent);
    }

    private static TextMesh CreateText(string objectName, Vector3 position, int fontSize, float characterSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.position = position;
        TextMesh text = textObject.AddComponent<TextMesh>();
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.font = font;
        text.fontSize = fontSize;
        text.characterSize = characterSize;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;
        text.richText = false;
        if (font != null) textObject.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        return text;
    }

    private static Transform CreateProgressTrack(Material offMaterial, Material onMaterial)
    {
        GameObject track = new GameObject("S83 Block Progress Track");
        GameObject fillRoot = new GameObject("S83 Progress Fill");
        fillRoot.transform.SetParent(track.transform, false);
        const float spacing = 0.39f;
        float startX = -spacing * (S83_Main.RoundCount - 1) * 0.5f;
        for (int i = 0; i < S83_Main.RoundCount; i++)
        {
            Vector3 position = new Vector3(startX + i * spacing, 6.55f, -0.28f);
            CreateCube($"Progress Back {i + 1:00}", position, new Vector3(0.3f, 0.16f, 0.07f), offMaterial, track.transform);
            GameObject fill = CreateCube($"Progress Fill {i + 1:00}", position + Vector3.back * 0.04f, new Vector3(0.25f, 0.11f, 0.04f), onMaterial, fillRoot.transform);
            fill.SetActive(false);
        }
        return fillRoot.transform;
    }

    private static void AddBackgroundMusic(GameObject cameraObject)
    {
        AudioClip music = LoadOptional<AudioClip>("Assets/Sound/Jungle Trip - Quincas Moreira.mp3");
        if (music == null) return;
        AudioSource source = cameraObject.AddComponent<AudioSource>();
        source.clip = music;
        source.playOnAwake = true;
        source.loop = true;
        source.volume = 0.3f;
        source.spatialBlend = 0f;
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

    private static void RemoveColliders(GameObject root)
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static void ValidateFaceTexture(string path, int expectedWidth, int expectedHeight)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || importer.alphaSource != TextureImporterAlphaSource.FromInput ||
            !importer.alphaIsTransparency || importer.mipmapEnabled ||
            importer.npotScale != TextureImporterNPOTScale.None ||
            importer.wrapMode != TextureWrapMode.Clamp ||
            importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            throw new InvalidOperationException($"[S83_VALIDATE] Import settings are invalid: {path}");
        }
        Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!decoded.LoadImage(File.ReadAllBytes(path), false) ||
                decoded.width != expectedWidth || decoded.height != expectedHeight)
            {
                throw new InvalidOperationException($"[S83_VALIDATE] Wrong PNG dimensions: {path}");
            }
            Color[] corners =
            {
                decoded.GetPixel(0, 0),
                decoded.GetPixel(decoded.width - 1, 0),
                decoded.GetPixel(0, decoded.height - 1),
                decoded.GetPixel(decoded.width - 1, decoded.height - 1)
            };
            foreach (Color corner in corners)
            {
                if (corner.a > 0.01f)
                {
                    throw new InvalidOperationException($"[S83_VALIDATE] PNG corner is not transparent: {path}");
                }
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(decoded);
        }
    }

    private static void ValidateVariant(
        string path,
        bool needsDetermined,
        bool needsSurprised,
        bool needsVictory)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root == null) throw new FileNotFoundException("[S83_VALIDATE] Variant is missing.", path);
        try
        {
            Transform original = FindChild(root.transform, S83_Main.OriginalFaceName);
            Transform determined = FindChild(root.transform, S83_Main.DeterminedFaceName);
            Transform surprised = FindChild(root.transform, S83_Main.SurprisedFaceName);
            Transform victory = FindChild(root.transform, S83_Main.VictoryFaceName);
            if (original == null || (needsDetermined && determined == null) ||
                (needsSurprised && surprised == null) || (needsVictory && victory == null))
            {
                throw new InvalidOperationException($"[S83_VALIDATE] Expression overlays are incomplete: {path}");
            }
            if (determined != null) ValidateOverlayTransform(original, determined, path + " determined");
            if (surprised != null) ValidateOverlayTransform(original, surprised, path + " surprised");
            if (victory != null) ValidateOverlayTransform(original, victory, path + " victory");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ValidateOverlayTransform(Transform source, Transform overlay, string label)
    {
        if (Quaternion.Angle(source.localRotation, overlay.localRotation) > 0.01f ||
            Vector3.Distance(source.localScale, overlay.localScale) > 0.0001f ||
            Vector3.Distance(source.localPosition, overlay.localPosition) > 0.025f)
        {
            throw new InvalidOperationException($"[S83_VALIDATE] Overlay transform drifted: {label}");
        }
    }

    private static Transform FindChild(Transform root, string exactName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == exactName) return child;
        }
        return null;
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new FileNotFoundException($"[S83] Required asset is missing: {path}", path);
        return asset;
    }

    private static T LoadOptional<T>(string path) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
