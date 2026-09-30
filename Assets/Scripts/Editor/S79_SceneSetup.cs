using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the horizontal Scene 79 accumulation video.</summary>
public static class S79_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_79.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene79";
    private const string MaterialRoot = AssetRoot + "/Materials";

    private const string OnePath = "Assets/Prefabs/Blocks/1.prefab";
    private const string ThousandPath = "Assets/Prefabs/Blocks/Scene78/Thousand_Smirk.prefab";
    private const string TenThousandPath = "Assets/Prefabs/Blocks/prefabTenThousand.prefab";
    private const string ImpactPath =
        "Assets/Sound/topSound/jg-032316-sfx-distant-meteor-crash-impact-2.mp3";
    private const string ImpactAccentPath = "Assets/Sound/collCube.wav";
    private const string TransformPath = "Assets/Sound/boom_metal.wav";
    private const string BurstPath = "Assets/Sound/Big Explosion Cut Off.mp3";

    private static readonly float[] CaptureTimes =
    {
        0.4f,
        1.8f,
        10.4f,
        21.2f,
        23.5f,
        26.4f,
        33.7f
    };

    private static bool[] capturedFrames;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static double playModeStartTime;

    [MenuItem("Tools/Scene_79/Quick Setup Short")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material groundMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S79_Ground.mat",
            new Color(0.31f, 0.71f, 0.29f));
        Material hillMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S79_Hills.mat",
            new Color(0.19f, 0.57f, 0.27f));
        Material cloudMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S79_Clouds.mat",
            new Color(0.97f, 0.99f, 1f));
        Material dustMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S79_Dust.mat",
            new Color(0.94f, 0.76f, 0.43f));
        Material flashMaterial = CreateTransparentFlashMaterial(
            MaterialRoot + "/S79_Flash.mat");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        BoxCollider groundCollider = CreateEnvironment(groundMaterial, hillMaterial, cloudMaterial);

        GameObject burstObject = new GameObject("10,000 Real Ones — GPU Burst");
        S79_UnitBurst burst = burstObject.AddComponent<S79_UnitBurst>();
        burst.onePrefab = LoadAsset<GameObject>(OnePath);
        burst.groundCollider = groundCollider;

        GameObject gameManager = new GameObject("GameManager — 34 Second Timeline");
        S79_Main director = gameManager.AddComponent<S79_Main>();
        director.onePrefab = LoadAsset<GameObject>(OnePath);
        director.thousandPrefab = LoadAsset<GameObject>(ThousandPath);
        director.tenThousandPrefab = LoadAsset<GameObject>(TenThousandPath);
        director.blockScale = 0.34f;
        director.dropHeight = 11f;
        director.structureDepth = 0.6f;
        director.dustMaterial = dustMaterial;
        director.flashMaterial = flashMaterial;
        director.unitBurst = burst;
        director.impactClip = LoadAsset<AudioClip>(ImpactPath);
        director.impactAccentClip = LoadAsset<AudioClip>(ImpactAccentPath);
        director.transformClip = LoadAsset<AudioClip>(TransformPath);
        director.burstClip = LoadAsset<AudioClip>(BurstPath);
        EditorUtility.SetDirty(burst);
        EditorUtility.SetDirty(director);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S79] Unity could not save the scene: {ScenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "[S79] Scene_79 created: ten squash landings, two columns of five, a 10,000 " +
            "transformation, and an exact 20 x 50 x 10 burst of real One geometry.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_79 — Ten Thousand",
                "Готово. Выберите Game View 16:9 и нажмите Play. Ролик длится 34 секунды.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_79/Validate Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S79_VALIDATE] Scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S79_Main director = UnityEngine.Object.FindAnyObjectByType<S79_Main>();
        S79_UnitBurst burst = UnityEngine.Object.FindAnyObjectByType<S79_UnitBurst>();
        if (director == null || burst == null)
        {
            throw new InvalidOperationException("[S79_VALIDATE] Scene director or unit burst is missing.");
        }

        if (AssetDatabase.GetAssetPath(director.onePrefab) != OnePath ||
            AssetDatabase.GetAssetPath(director.thousandPrefab) != ThousandPath ||
            AssetDatabase.GetAssetPath(director.tenThousandPrefab) != TenThousandPath)
        {
            throw new InvalidOperationException(
                "[S79_VALIDATE] Scene does not use the requested original Numberblock prefabs.");
        }

        if (director.unitBurst != burst || burst.onePrefab != director.onePrefab ||
            !(burst.groundCollider is BoxCollider) ||
            director.dustMaterial == null || director.flashMaterial == null)
        {
            throw new InvalidOperationException("[S79_VALIDATE] Effects references are incomplete.");
        }

        if (director.impactClip == null || director.impactAccentClip == null ||
            director.transformClip == null || director.burstClip == null)
        {
            throw new InvalidOperationException("[S79_VALIDATE] Landing or transformation audio is incomplete.");
        }

        Camera camera = Camera.main;
        if (camera == null || camera.name != "Main Camera — Horizontal 16x9" ||
            Mathf.Abs(camera.fieldOfView - 46f) > 0.01f ||
            Mathf.Abs(camera.aspect - 16f / 9f) > 0.01f)
        {
            throw new InvalidOperationException("[S79_VALIDATE] Horizontal 16:9 camera is misconfigured.");
        }

        if (S79_Main.ThousandCount != 10 || S79_Main.BlocksPerColumn != 5 ||
            S79_UnitBurst.TotalUnitCount != 10000 ||
            S79_UnitBurst.GridX != 20 || S79_UnitBurst.GridY != 50 || S79_UnitBurst.GridZ != 10)
        {
            throw new InvalidOperationException(
                "[S79_VALIDATE] The 2 x 5 Thousand layout or 20 x 50 x 10 unit grid changed.");
        }

        if (S79_Main.MergeStartTime <=
                S79_Main.FirstImpactTime + (S79_Main.ThousandCount - 1) * S79_Main.ImpactInterval ||
            S79_Main.BurstTime <= S79_Main.MergeStartTime + S79_Main.MergeDuration ||
            S79_Main.SequenceDuration <= S79_Main.BurstTime)
        {
            throw new InvalidOperationException("[S79_VALIDATE] Timeline beats overlap or are out of order.");
        }

        Debug.Log(
            "[S79_VALIDATE] PASS — horizontal 16:9, ground BoxCollider, ten Scene 78 " +
            "Thousand variants, Ten Thousand, and exactly 10,000 grounded instanced Ones.");
    }

    /// <summary>Runs the complete short at 3x speed and captures every important beat.</summary>
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S79_PLAY] This entry point is batch-mode only.");
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
            Time.timeScale = 3f;
            Debug.Log("[S79_PLAY] Entered Play Mode at 3x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;

            if (playModePassed)
            {
                Debug.Log(
                    $"[S79_PLAY] PASS — complete short and all {CaptureTimes.Length} keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S79_PLAY] FAIL — the complete short did not reach its ending.");
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

        S79_Main director = UnityEngine.Object.FindAnyObjectByType<S79_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene79-{i + 1:00}.png");
                }
            }

            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() &&
                    director.ImpactCount == S79_Main.ThousandCount &&
                    director.TransformationTriggered &&
                    director.BurstTriggered &&
                    director.unitBurst.Active &&
                    director.unitBurst.RenderedUnitCount == S79_UnitBurst.TotalUnitCount &&
                    director.unitBurst.SettledUnitCount == S79_UnitBurst.TotalUnitCount;
                EditorApplication.isPlaying = false;
                return;
            }
        }

        if (EditorApplication.timeSinceStartup - playModeStartTime > 28d)
        {
            Debug.LogError("[S79_PLAY] Timed out after 28 real seconds.");
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
            throw new InvalidOperationException("[S79_PLAY] Main Camera disappeared.");
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

        Debug.Log($"[S79_PLAY] Captured {path}");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene79");
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

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Horizontal 16x9");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.fieldOfView = 46f;
        camera.aspect = 16f / 9f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 180f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.48f, 0.81f, 0.97f);
        camera.transform.position = new Vector3(-1.7f, 2.2f, -10.9f);
        camera.transform.LookAt(new Vector3(-1.7f, 1.7f, 0.6f));
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(44f, -32f, 0f);
        RenderSettings.ambientLight = new Color(0.58f, 0.58f, 0.58f);
    }

    private static BoxCollider CreateEnvironment(
        Material groundMaterial,
        Material hillMaterial,
        Material cloudMaterial)
    {
        GameObject environment = new GameObject("S79 Cheerful Open Field");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Grass Ground";
        ground.transform.SetParent(environment.transform, true);
        ground.transform.position = new Vector3(0f, 0f, 7f);
        ground.transform.localScale = new Vector3(24f, 1f, 20f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        RemoveColliders(ground);
        BoxCollider groundCollider = ground.AddComponent<BoxCollider>();
        groundCollider.size = new Vector3(10f, 0.12f, 10f);
        groundCollider.center = new Vector3(0f, -0.06f, 0f);

        CreateBackdropSphere(
            "Left Hill",
            new Vector3(-9f, -1.6f, 13f),
            new Vector3(13f, 5.4f, 2.2f),
            hillMaterial,
            environment.transform);
        CreateBackdropSphere(
            "Right Hill",
            new Vector3(8.2f, -1.8f, 14f),
            new Vector3(14f, 5.8f, 2.3f),
            hillMaterial,
            environment.transform);

        GameObject clouds = new GameObject("High Soft Clouds");
        clouds.transform.SetParent(environment.transform, false);
        CreateCloud(new Vector3(-4.7f, 8.8f, 11f), 0.9f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(4.5f, 14.8f, 12f), 0.72f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(-3f, 20.5f, 13f), 0.6f, cloudMaterial, clouds.transform);
        return groundCollider;
    }

    private static void CreateCloud(
        Vector3 center,
        float scale,
        Material material,
        Transform parent)
    {
        CreateBackdropSphere(
            "Cloud Puff",
            center + Vector3.left * 0.72f * scale,
            new Vector3(1.45f, 0.72f, 0.35f) * scale,
            material,
            parent);
        CreateBackdropSphere(
            "Cloud Puff",
            center + Vector3.up * 0.22f * scale,
            new Vector3(1.55f, 0.92f, 0.38f) * scale,
            material,
            parent);
        CreateBackdropSphere(
            "Cloud Puff",
            center + Vector3.right * 0.78f * scale,
            new Vector3(1.38f, 0.68f, 0.34f) * scale,
            material,
            parent);
    }

    private static GameObject CreateBackdropSphere(
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
        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            throw new InvalidOperationException("[S79] No opaque shader is available.");
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

        SetMaterialColor(material, color);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateTransparentFlashMaterial(string path)
    {
        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            throw new InvalidOperationException("[S79] No transparent shader is available.");
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

        material.SetFloat("_Mode", 3f);
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)RenderQueue.Transparent;
        SetMaterialColor(material, new Color(1f, 0.86f, 0.28f, 0.35f));
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(1f, 0.55f, 0.05f) * 1.8f);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void RemoveColliders(GameObject target)
    {
        foreach (Collider collider in target.GetComponents<Collider>())
        {
            UnityEngine.Object.DestroyImmediate(collider);
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
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException($"[S79] Required asset is missing: {path}", path);
        }
        return asset;
    }
}
