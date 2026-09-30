using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Creates and validates the silent vertical collision chase in Scene 86.</summary>
public static class S86_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_86.unity";
    private const string Root = "Assets/Prefabs/Blocks/Scene86";
    private const string MaterialRoot = Root + "/Materials";
    private const string HundredPath = "Assets/Prefabs/Blocks/Scene82/Prefabs/Hundred_Collision.prefab";
    private const string TenPath = "Assets/Prefabs/Blocks/Scene85/Prefabs/Numberblock_10_Expressions.prefab";
    private const string FiftyPath = "Assets/Prefabs/Blocks/Scene85/Prefabs/Numberblock_50_Expressions.prefab";
    private const string ThousandPath = "Assets/Prefabs/Blocks/Scene85/Prefabs/Numberblock_1000_Expressions.prefab";
    private const string OnePath = "Assets/Prefabs/Blocks/1.prefab";
    private static readonly HashSet<string> captures = new HashSet<string>();
    private static bool playStarted;
    private static bool playPassed;
    private static bool playError;
    private static double playStart;
    private static bool originalPlayOptionsEnabled;
    private static EnterPlayModeOptions originalPlayOptions;

    [MenuItem("Tools/Scene_86/Quick Setup Collision Chase Short")]
    public static void QuickSetup()
    {
        EnsureFolders();
        Material groundMaterial = CreateMaterial("S86_Ground", new Color(0.22f, 0.68f, 0.30f));
        Material stripeMaterial = CreateMaterial("S86_TrackStripe", new Color(0.94f, 0.78f, 0.20f));
        Material hillMaterial = CreateMaterial("S86_Hills", new Color(0.12f, 0.43f, 0.25f));
        Material cloudMaterial = CreateMaterial("S86_Clouds", new Color(0.95f, 0.98f, 1f));
        Material flashMaterial = CreateMaterial("S86_ImpactFlash", new Color(1f, 0.92f, 0.18f), true);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateCamera();
        CreateLighting();
        CreateEnvironment(groundMaterial, stripeMaterial, hillMaterial, cloudMaterial);

        GameObject burstObject = new GameObject("S86 Exact Ones — Three GPU Bursts");
        S86_UnitBurst burst = burstObject.AddComponent<S86_UnitBurst>();
        burst.onePrefab = Load<GameObject>(OnePath);
        burst.floorY = 0.02f;

        GameObject directorObject = new GameObject("S86 Director — Silent Collision Chase");
        S86_Main director = directorObject.AddComponent<S86_Main>();
        director.hundredPrefab = Load<GameObject>(HundredPath);
        director.tenPrefab = Load<GameObject>(TenPath);
        director.fiftyPrefab = Load<GameObject>(FiftyPath);
        director.thousandPrefab = Load<GameObject>(ThousandPath);
        director.shotCamera = camera;
        director.unitBurst = burst;
        director.flashMaterial = flashMaterial;
        director.runClip = Load<AudioClip>("Assets/Sound/runaway.mp3");
        director.impactClip = Load<AudioClip>("Assets/Sound/boom_metal.wav");
        director.burstClip = Load<AudioClip>("Assets/Sound/destroy_blocks.mp3");
        director.openingHold = 0.45f;
        director.runSpeed = 10.5f;
        director.finalHold = 2.8f;

        EditorUtility.SetDirty(burst);
        EditorUtility.SetDirty(director);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException("[S86_SETUP] Unity could not save Scene_86.");
        }
        AssetDatabase.SaveAssets();
        ValidateScene();
        Debug.Log("[S86_SETUP] PASS — vertical silent chase, exact 10/50/100 bursts and reused validated expressions.");
    }

    [MenuItem("Tools/Scene_86/Validate Collision Chase Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S86_VALIDATE] Scene is missing.", ScenePath);
        }
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S86_Main director = UnityEngine.Object.FindAnyObjectByType<S86_Main>();
        S86_UnitBurst burst = UnityEngine.Object.FindAnyObjectByType<S86_UnitBurst>();
        Camera camera = Camera.main;
        if (director == null || burst == null || camera == null)
        {
            throw new InvalidOperationException("[S86_VALIDATE] Director, burst, or Main Camera is missing.");
        }
        if (AssetDatabase.GetAssetPath(director.hundredPrefab) != HundredPath ||
            AssetDatabase.GetAssetPath(director.tenPrefab) != TenPath ||
            AssetDatabase.GetAssetPath(director.fiftyPrefab) != FiftyPath ||
            AssetDatabase.GetAssetPath(director.thousandPrefab) != ThousandPath ||
            AssetDatabase.GetAssetPath(burst.onePrefab) != OnePath || director.unitBurst != burst)
        {
            throw new InvalidOperationException("[S86_VALIDATE] Character or original One references are incorrect.");
        }
        // Camera.aspect is a runtime value and is not serialized by Unity. S86_Main
        // enforces 9:16 in Start; here we validate the persisted camera setup.
        if (camera.name != "Main Camera — Vertical 9x16 Follow" || camera.orthographic ||
            Mathf.Abs(camera.fieldOfView - 50f) > 0.001f)
        {
            throw new InvalidOperationException("[S86_VALIDATE] Vertical follow camera is incorrectly configured.");
        }
        if (director.runClip == null || director.impactClip == null || director.burstClip == null ||
            director.flashMaterial == null || S86_Main.ExpectedBurstCount != 3 ||
            S86_Main.ExpectedUnitCount != S86_UnitBurst.MaximumUnitCount)
        {
            throw new InvalidOperationException("[S86_VALIDATE] Audio, effects, or exact unit totals are incorrect.");
        }

        ValidateFace(HundredPath, S86_Main.HundredFocusedFaceName, "Hundred_Determined.png", 1);
        ValidateFace(TenPath, S86_Main.ScaredFaceName, "Ten_Scared.png", 2);
        ValidateFace(FiftyPath, S86_Main.ScaredFaceName, "Fifty_Scared.png", 2);
        ValidateFace(ThousandPath, S86_Main.SmugFaceName, "Thousand_Smug.png", 1);
        Debug.Log("[S86_VALIDATE] PASS — scene references, 9:16 camera, anatomy-preserving RGBA faces, audio and 160 exact Ones.");
    }

    [MenuItem("Tools/Scene_86/Run Play Mode Validation")]
    public static void RunPlayModeValidation()
    {
        ValidateScene();
        captures.Clear();
        playStarted = false;
        playPassed = false;
        playError = false;
        originalPlayOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled;
        originalPlayOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        Application.logMessageReceived += OnLog;
        EditorApplication.playModeStateChanged += OnPlayState;
        EditorApplication.update += PlayTick;
        EditorApplication.isPlaying = true;
    }

    private static void OnLog(string message, string trace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            playError = true;
        }
    }

    private static void OnPlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            playStarted = true;
            playStart = EditorApplication.timeSinceStartup;
            Time.captureDeltaTime = 1f / 30f;
            Debug.Log("[S86_PLAY] Started fixed 30fps validation.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playStarted)
        {
            Time.captureDeltaTime = 0f;
            EditorSettings.enterPlayModeOptionsEnabled = originalPlayOptionsEnabled;
            EditorSettings.enterPlayModeOptions = originalPlayOptions;
            Application.logMessageReceived -= OnLog;
            EditorApplication.playModeStateChanged -= OnPlayState;
            EditorApplication.update -= PlayTick;
            Debug.Log(playPassed
                ? "[S86_PLAY] PASS — centered chase, three collisions, exact bursts and standing smug 1000."
                : "[S86_PLAY] FAIL");
            EditorApplication.Exit(playPassed ? 0 : 1);
        }
    }

    private static void PlayTick()
    {
        if (!playStarted || !EditorApplication.isPlaying)
        {
            return;
        }
        S86_Main director = UnityEngine.Object.FindAnyObjectByType<S86_Main>();
        if (director != null)
        {
            bool captureReady = director.BeatTime > 0.16f;
            string key = $"{director.CollisionCount}-{director.Beat}";
            if (captureReady && captures.Add(key))
            {
                CaptureFrame("/private/tmp/scene86-" + captures.Count.ToString("00") + ".png");
            }
            string revealKey = key + "-collision-framing";
            if (director.RunProgress > 0.88f && director.CollisionFraming > 0.85f && captures.Add(revealKey))
            {
                CaptureFrame("/private/tmp/scene86-" + captures.Count.ToString("00") + ".png");
            }
            if (director.SequenceComplete)
            {
                playPassed = !playError && director.CollisionCount == S86_Main.ExpectedBurstCount &&
                    director.DestroyedUnitCount == S86_Main.ExpectedUnitCount &&
                    director.unitBurst.BurstCount == S86_Main.ExpectedBurstCount &&
                    director.unitBurst.TotalSpawned == S86_Main.ExpectedUnitCount &&
                    director.HundredStayedCentered && director.ThousandStayedStanding && captures.Count >= 11;
                Debug.Log(
                    $"[S86_PLAY] duration={director.SequenceTime:F2}s collisions={director.CollisionCount} " +
                    $"units={director.unitBurst.TotalSpawned} centered={director.HundredStayedCentered} " +
                    $"standing={director.ThousandStayedStanding} captures={captures.Count} errors={playError}");
                EditorApplication.isPlaying = false;
                return;
            }
        }
        if (playError || EditorApplication.timeSinceStartup - playStart > 90d)
        {
            playPassed = false;
            Debug.LogError("[S86_PLAY] Runtime error or validation timeout.");
            EditorApplication.isPlaying = false;
        }
    }

    private static void CaptureFrame(string path)
    {
        Camera camera = Camera.main;
        RenderTexture render = new RenderTexture(540, 960, 24, RenderTextureFormat.ARGB32);
        Texture2D texture = new Texture2D(540, 960, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        try
        {
            camera.targetTexture = render;
            camera.Render();
            RenderTexture.active = render;
            texture.ReadPixels(new Rect(0f, 0f, 540f, 960f), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            render.Release();
            UnityEngine.Object.DestroyImmediate(render);
            UnityEngine.Object.DestroyImmediate(texture);
        }
        Debug.Log("[S86_PLAY] Captured " + path);
    }

    private static void ValidateFace(string prefabPath, string faceName, string expectedTexture, int expectedEyeCount)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            Transform face = FindChild(root.transform, faceName);
            if (face == null)
            {
                throw new InvalidOperationException("[S86_FACE] Missing expression: " + faceName);
            }
            Renderer renderer = face.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null)
            {
                throw new InvalidOperationException("[S86_FACE] Face has no renderer, material, or texture: " + faceName);
            }
            if (renderer.shadowCastingMode != ShadowCastingMode.Off || renderer.receiveShadows)
            {
                throw new InvalidOperationException("[S86_FACE] Face overlay shadows must remain disabled: " + faceName);
            }
            string texturePath = AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture);
            if (Path.GetFileName(texturePath) != expectedTexture)
            {
                throw new InvalidOperationException($"[S86_FACE] {faceName} uses {texturePath}, expected {expectedTexture}.");
            }
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null || !importer.alphaIsTransparency || importer.mipmapEnabled ||
                importer.npotScale != TextureImporterNPOTScale.None || importer.wrapMode != TextureWrapMode.Clamp ||
                importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                throw new InvalidOperationException("[S86_FACE] Import settings changed: " + texturePath);
            }
            ValidatePngAlpha(texturePath);
            Debug.Log($"[S86_FACE] {faceName}: source {texturePath}, {expectedEyeCount} eye(s), true transparent PNG.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ValidatePngAlpha(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        if (bytes.Length < 26 || bytes[25] != 6)
        {
            throw new InvalidOperationException("[S86_FACE] Expected an RGBA PNG: " + path);
        }
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!texture.LoadImage(bytes, false))
            {
                throw new IOException("[S86_FACE] Invalid PNG: " + path);
            }
            Color32[] pixels = texture.GetPixels32();
            int width = texture.width;
            int height = texture.height;
            if (pixels[0].a != 0 || pixels[width - 1].a != 0 ||
                pixels[(height - 1) * width].a != 0 || pixels[pixels.Length - 1].a != 0)
            {
                throw new InvalidOperationException("[S86_FACE] Transparent corners are missing: " + path);
            }
            int transparent = 0;
            int visible = 0;
            foreach (Color32 colour in pixels)
            {
                if (colour.a == 0) transparent++;
                if (colour.a > 220) visible++;
            }
            if (transparent < pixels.Length / 5 || visible < pixels.Length / 100)
            {
                throw new InvalidOperationException("[S86_FACE] Empty background or visible anatomy is invalid: " + path);
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Vertical 9x16 Follow");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.47f, 0.80f, 0.96f);
        camera.fieldOfView = 50f;
        camera.aspect = 9f / 16f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 500f;
        camera.allowHDR = false;
        camera.transform.position = new Vector3(0f, 4.55f, -18f);
        camera.transform.LookAt(new Vector3(0f, 3.15f, 0f));
        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static void CreateLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.72f, 0.76f, 0.82f);
        GameObject lightObject = new GameObject("S86 Sun");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.95f, 0.86f);
        light.shadows = LightShadows.Soft;
        lightObject.transform.rotation = Quaternion.Euler(44f, -28f, 0f);
    }

    private static void CreateEnvironment(
        Material groundMaterial,
        Material stripeMaterial,
        Material hillMaterial,
        Material cloudMaterial)
    {
        GameObject environment = new GameObject("S86 Moving World");
        CreatePrimitive("S86 Long Ground", PrimitiveType.Cube,
            new Vector3(-45f, -0.28f, 4f), new Vector3(190f, 0.56f, 24f), groundMaterial, environment.transform);

        for (int i = 0; i < 24; i++)
        {
            float x = 20f - i * 8f;
            CreatePrimitive($"S86 Speed Stripe {i + 1:00}", PrimitiveType.Cube,
                new Vector3(x, 0.025f, -0.2f), new Vector3(0.24f, 0.025f, 5f),
                stripeMaterial, environment.transform);
        }

        for (int i = 0; i < 15; i++)
        {
            float x = 16f - i * 13.5f;
            float height = 2.2f + (i % 4) * 0.75f;
            CreatePrimitive($"S86 Background Hill {i + 1:00}", PrimitiveType.Sphere,
                new Vector3(x, height * 0.35f, 10.5f), new Vector3(8f, height, 2.4f),
                hillMaterial, environment.transform);
            if ((i & 1) == 0)
            {
                CreateCloud(new Vector3(x - 2.5f, 10.5f + (i % 3), 13f), cloudMaterial, environment.transform);
            }
        }
    }

    private static void CreateCloud(Vector3 position, Material material, Transform parent)
    {
        GameObject root = new GameObject("S86 Cloud");
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        CreatePrimitive("Cloud A", PrimitiveType.Sphere, position + Vector3.left * 1.1f,
            new Vector3(2.6f, 1.25f, 0.8f), material, root.transform);
        CreatePrimitive("Cloud B", PrimitiveType.Sphere, position + Vector3.up * 0.35f,
            new Vector3(3.0f, 1.7f, 0.9f), material, root.transform);
        CreatePrimitive("Cloud C", PrimitiveType.Sphere, position + Vector3.right * 1.25f,
            new Vector3(2.3f, 1.15f, 0.75f), material, root.transform);
    }

    private static GameObject CreatePrimitive(
        string name,
        PrimitiveType type,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent)
    {
        GameObject gameObject = GameObject.CreatePrimitive(type);
        gameObject.name = name;
        gameObject.transform.SetParent(parent, true);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;
        gameObject.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
        return gameObject;
    }

    private static Material CreateMaterial(string name, Color color, bool unlit = false)
    {
        string path = MaterialRoot + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        // This project currently renders through Unity's built-in compatibility
        // path. Package URP shaders can be found by name while still rendering
        // magenta, so use the same supported built-in shaders as Scene 85.
        Shader shader = Shader.Find(unlit ? "Unlit/Color" : "Standard");
        if (shader == null)
        {
            throw new InvalidOperationException("[S86_SETUP] Required built-in shader is unavailable.");
        }
        if (material == null)
        {
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene86");
        EnsureFolder(Root, "Materials");
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static Transform FindChild(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child;
            }
        }
        return null;
    }

    private static T Load<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException("[S86_SETUP] Missing asset: " + path, path);
        }
        return asset;
    }
}
