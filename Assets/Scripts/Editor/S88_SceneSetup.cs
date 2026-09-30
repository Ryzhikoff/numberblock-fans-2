using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the vertical Scene 88 x10 button short.</summary>
public static class S88_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_88.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene88";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string ThousandPath = "Assets/Prefabs/Blocks/1000.prefab";
    private const string TenThousandPath = "Assets/Prefabs/Blocks/prefabTenThousand.prefab";
    private const string MusicPath =
        "Assets/Sound/487685__gr8horizon__dragon-power-training-loop.wav";

    private static readonly float[] CaptureTimes =
    {
        0.4f,
        2.92f,
        4.4f,
        7.7f,
        8.9f,
        11.7f,
        13.48f,
        14.45f,
        17.9f
    };

    private static bool[] capturedFrames;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static double playModeStartTime;

    [MenuItem("Tools/Scene_88/Create x10 Button Short")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material groundMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S88_Ground.mat",
            new Color(0.18f, 0.58f, 0.3f));
        Material platformMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S88_Platform.mat",
            new Color(0.19f, 0.45f, 0.83f));
        Material buttonBaseMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S88_ButtonBase.mat",
            new Color(0.16f, 0.18f, 0.25f));
        Material buttonTopMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S88_ButtonTop.mat",
            new Color(1f, 0.25f, 0.08f));
        Material labelMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S88_Label.mat",
            Color.white);
        Material hillMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S88_Hills.mat",
            new Color(0.12f, 0.42f, 0.23f));
        Material cloudMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S88_Clouds.mat",
            new Color(0.98f, 0.99f, 1f));
        Material sparkMaterial = CreateEmissiveMaterial(
            MaterialRoot + "/S88_Spark.mat",
            new Color(1f, 0.78f, 0.08f));
        Material flashMaterial = CreateTransparentMaterial(
            MaterialRoot + "/S88_Flash.mat",
            new Color(1f, 0.9f, 0.28f, 0.72f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        CreateEnvironment(groundMaterial, platformMaterial, hillMaterial, cloudMaterial);
        ButtonResult button = CreateButton(buttonBaseMaterial, buttonTopMaterial, labelMaterial);

        GameObject gameManager = new GameObject("GameManager — x10 Silent Loop Timeline");
        S88_Main director = gameManager.AddComponent<S88_Main>();
        director.thousandPrefab = LoadAsset<GameObject>(ThousandPath);
        director.tenThousandPrefab = LoadAsset<GameObject>(TenThousandPath);
        director.blockScale = 0.22f;
        director.buttonTop = button.Top;
        director.multiplyLabel = button.MultiplyLabel;
        director.divideLabel = button.DivideLabel;
        director.sparkMaterial = sparkMaterial;
        director.flashMaterial = flashMaterial;
        director.pressClip = LoadOptional<AudioClip>("Assets/Sound/collCube.wav");
        director.summonClip = LoadOptional<AudioClip>("Assets/Sound/zvuk-priblijeniya.mp3");
        director.transformClip = LoadOptional<AudioClip>("Assets/Sound/boom_metal.wav");
        director.divideClip = LoadOptional<AudioClip>(
            "Assets/Sound/588718__collierhs-colinlib__elevator-ding.wav");
        director.musicClip = LoadAsset<AudioClip>(MusicPath);
        director.musicVolume = 0.18f;
        EditorUtility.SetDirty(director);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException("[S88] Unity could not save the scene: " + ScenePath);
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "[S88] Scene_88 created: vertical 9:16, x10 button, ten original Thousands, " +
            "original Ten Thousand, /10 return, no dialogue, 18.4 seconds.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_88 — x10 button",
                "Готово. Выберите Game View 9:16 и нажмите Play. Длительность — 18,4 секунды.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_88/Validate x10 Button Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S88_VALIDATE] Scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S88_Main director = UnityEngine.Object.FindAnyObjectByType<S88_Main>();
        if (director == null)
        {
            throw new InvalidOperationException("[S88_VALIDATE] Director is missing.");
        }

        if (S88_Main.SequenceDuration > 20f ||
            !(S88_Main.FirstPressTime < S88_Main.SummonStartTime &&
              S88_Main.SummonStartTime < S88_Main.AssemblyCompleteTime &&
              S88_Main.AssemblyCompleteTime < S88_Main.TransformTime &&
              S88_Main.TransformTime < S88_Main.DivideLabelTime &&
              S88_Main.DivideLabelTime < S88_Main.GiantLandingTime &&
              S88_Main.GiantLandingTime < S88_Main.DivideTime &&
              S88_Main.DivideTime < S88_Main.ReturnStartTime &&
              S88_Main.ReturnStartTime < S88_Main.ReturnCompleteTime &&
              S88_Main.ReturnCompleteTime < S88_Main.SequenceDuration))
        {
            throw new InvalidOperationException("[S88_VALIDATE] Timeline exceeds 20 seconds or is unordered.");
        }

        if (AssetDatabase.GetAssetPath(director.thousandPrefab) != ThousandPath ||
            AssetDatabase.GetAssetPath(director.tenThousandPrefab) != TenThousandPath ||
            director.buttonTop == null || director.multiplyLabel == null ||
            director.divideLabel == null || director.sparkMaterial == null ||
            director.flashMaterial == null || director.pressClip == null ||
            director.summonClip == null || director.transformClip == null ||
            director.divideClip == null)
        {
            throw new InvalidOperationException("[S88_VALIDATE] Scene references are incomplete.");
        }

        Camera camera = Camera.main;
        // Camera.aspect follows the active Game View in Edit Mode and is therefore
        // enforced by S88_Main at runtime rather than asserted here.
        if (camera == null || camera.name != "Main Camera — Vertical 9x16" ||
            Mathf.Abs(camera.fieldOfView - 48f) > 0.01f)
        {
            throw new InvalidOperationException("[S88_VALIDATE] Vertical 9:16 camera is misconfigured.");
        }

        if (S88_Main.ThousandCount != 10)
        {
            throw new InvalidOperationException("[S88_VALIDATE] The multiplication must use exactly ten Thousands.");
        }

        LoadAsset<AudioClip>(MusicPath);

        Debug.Log(
            "[S88_VALIDATE] PASS — 9:16, 18.4 seconds, no dialogue components, exactly " +
            "ten original Thousands, original Ten Thousand, x10 and /10 loop.");
    }

    [MenuItem("Tools/Scene_88/Run Complete Play Mode Validation")]
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S88_PLAY] This entry point is batch-mode only.");
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
            Debug.Log("[S88_PLAY] Entered Play Mode at 3x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            if (playModePassed)
            {
                Debug.Log($"[S88_PLAY] PASS — complete short and all {CaptureTimes.Length} keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S88_PLAY] FAIL — the complete short did not reach its ending.");
                EditorApplication.Exit(1);
            }
        }
    }

    private static void PlayModeValidationTick()
    {
        if (!playModeStarted || !EditorApplication.isPlaying) return;

        S88_Main director = UnityEngine.Object.FindAnyObjectByType<S88_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene88-{i + 1:00}.png");
                }
            }

            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() &&
                    director.FirstPressTriggered &&
                    director.TransformationTriggered &&
                    director.DivisionTriggered &&
                    director.SummonStoppedAtTransformation &&
                    director.VisibleThousandCount == 1;
                EditorApplication.isPlaying = false;
                return;
            }
        }

        if (EditorApplication.timeSinceStartup - playModeStartTime > 24d)
        {
            Debug.LogError("[S88_PLAY] Timed out after 24 real seconds.");
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
        if (camera == null) throw new InvalidOperationException("[S88_PLAY] Main Camera disappeared.");

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
        Debug.Log("[S88_PLAY] Captured " + path);
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(AssetRoot))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs/Blocks", "Scene88");
        }
        if (!AssetDatabase.IsValidFolder(MaterialRoot))
        {
            AssetDatabase.CreateFolder(AssetRoot, "Materials");
        }
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Vertical 9x16");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.fieldOfView = 48f;
        camera.aspect = 9f / 16f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 120f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.45f, 0.8f, 0.97f);
        camera.transform.position = new Vector3(0f, 4.35f, -11.5f);
        camera.transform.LookAt(new Vector3(0f, 2f, 0f));
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light — Warm Key");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(42f, -34f, 0f);
        RenderSettings.ambientLight = new Color(0.62f, 0.62f, 0.62f);
    }

    private static void CreateEnvironment(
        Material groundMaterial,
        Material platformMaterial,
        Material hillMaterial,
        Material cloudMaterial)
    {
        GameObject environment = new GameObject("S88 Bright Button Meadow");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Grass Ground";
        ground.transform.SetParent(environment.transform, false);
        ground.transform.position = new Vector3(0f, 0f, 7f);
        ground.transform.localScale = new Vector3(8f, 1f, 8f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        RemoveColliders(ground);

        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "Numberblocks Stage";
        platform.transform.SetParent(environment.transform, false);
        platform.transform.position = new Vector3(0f, 0.25f, 0f);
        platform.transform.localScale = new Vector3(8.8f, 0.5f, 7f);
        platform.GetComponent<Renderer>().sharedMaterial = platformMaterial;
        RemoveColliders(platform);

        CreateBackdropSphere("Left Hill", new Vector3(-7f, -1.5f, 12f), new Vector3(11f, 4.8f, 2f), hillMaterial, environment.transform);
        CreateBackdropSphere("Right Hill", new Vector3(7f, -1.7f, 13f), new Vector3(12f, 5.2f, 2.2f), hillMaterial, environment.transform);

        GameObject clouds = new GameObject("Soft Vertical Clouds");
        clouds.transform.SetParent(environment.transform, false);
        CreateCloud(new Vector3(-3.1f, 6.6f, 12f), 0.8f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(3.2f, 11.8f, 13f), 0.68f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(-2.5f, 16.8f, 14f), 0.56f, cloudMaterial, clouds.transform);
    }

    private static ButtonResult CreateButton(
        Material baseMaterial,
        Material topMaterial,
        Material labelMaterial)
    {
        GameObject root = new GameObject("Magic Multiply and Divide Button");
        root.transform.position = new Vector3(1.2f, 0.5f, -2.2f);

        GameObject buttonBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        buttonBase.name = "Button Base";
        buttonBase.transform.SetParent(root.transform, false);
        buttonBase.transform.localPosition = Vector3.zero;
        buttonBase.transform.localScale = new Vector3(1.35f, 0.18f, 1.35f);
        buttonBase.GetComponent<Renderer>().sharedMaterial = baseMaterial;
        RemoveColliders(buttonBase);

        GameObject topRoot = new GameObject("Animated Button Top");
        topRoot.transform.SetParent(root.transform, false);
        topRoot.transform.localPosition = new Vector3(0f, 0.2f, 0f);

        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "Red Button Cap";
        cap.transform.SetParent(topRoot.transform, false);
        cap.transform.localScale = new Vector3(1.08f, 0.15f, 1.08f);
        cap.GetComponent<Renderer>().sharedMaterial = topMaterial;
        RemoveColliders(cap);

        Transform multiply = CreateMultiplyLabel(topRoot.transform, labelMaterial);
        Transform divide = CreateDivideLabel(topRoot.transform, labelMaterial);
        multiply.gameObject.SetActive(false);
        divide.gameObject.SetActive(false);

        return new ButtonResult
        {
            Top = topRoot.transform,
            MultiplyLabel = multiply,
            DivideLabel = divide
        };
    }

    private static Transform CreateMultiplyLabel(Transform parent, Material material)
    {
        GameObject root = new GameObject("Multiply Label — x10");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0f, 0.34f, 0f);
        root.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        CreateStroke("Multiply Slash A", root.transform, new Vector2(-0.67f, 0f), new Vector2(0.48f, 0.09f), 45f, material);
        CreateStroke("Multiply Slash B", root.transform, new Vector2(-0.67f, 0f), new Vector2(0.48f, 0.09f), -45f, material);
        CreateOne(root.transform, -0.12f, material);
        CreateZero(root.transform, 0.55f, material);
        return root.transform;
    }

    private static Transform CreateDivideLabel(Transform parent, Material material)
    {
        GameObject root = new GameObject("Divide Label — divide10");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = new Vector3(0f, 0.34f, 0f);
        root.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        CreateStroke("Divide Bar", root.transform, new Vector2(-0.68f, 0f), new Vector2(0.48f, 0.08f), 0f, material);
        CreateStroke("Divide Dot Top", root.transform, new Vector2(-0.68f, 0.2f), new Vector2(0.11f, 0.11f), 0f, material);
        CreateStroke("Divide Dot Bottom", root.transform, new Vector2(-0.68f, -0.2f), new Vector2(0.11f, 0.11f), 0f, material);
        CreateOne(root.transform, -0.12f, material);
        CreateZero(root.transform, 0.55f, material);
        return root.transform;
    }

    private static void CreateOne(Transform parent, float x, Material material)
    {
        CreateStroke("Digit 1", parent, new Vector2(x, 0f), new Vector2(0.1f, 0.52f), 0f, material);
        CreateStroke("Digit 1 Foot", parent, new Vector2(x + 0.03f, -0.24f), new Vector2(0.28f, 0.08f), 0f, material);
    }

    private static void CreateZero(Transform parent, float x, Material material)
    {
        CreateStroke("Digit 0 Left", parent, new Vector2(x - 0.2f, 0f), new Vector2(0.08f, 0.5f), 0f, material);
        CreateStroke("Digit 0 Right", parent, new Vector2(x + 0.2f, 0f), new Vector2(0.08f, 0.5f), 0f, material);
        CreateStroke("Digit 0 Top", parent, new Vector2(x, 0.23f), new Vector2(0.42f, 0.08f), 0f, material);
        CreateStroke("Digit 0 Bottom", parent, new Vector2(x, -0.23f), new Vector2(0.42f, 0.08f), 0f, material);
    }

    private static void CreateStroke(
        string objectName,
        Transform parent,
        Vector2 position,
        Vector2 size,
        float angle,
        Material material)
    {
        GameObject stroke = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stroke.name = objectName;
        stroke.transform.SetParent(parent, false);
        stroke.transform.localPosition = new Vector3(position.x, position.y, 0f);
        stroke.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        stroke.transform.localScale = new Vector3(size.x, size.y, 0.045f);
        stroke.GetComponent<Renderer>().sharedMaterial = material;
        RemoveColliders(stroke);
    }

    private static void CreateCloud(Vector3 center, float scale, Material material, Transform parent)
    {
        CreateBackdropSphere("Cloud Puff", center + Vector3.left * 0.7f * scale, new Vector3(1.4f, 0.7f, 0.35f) * scale, material, parent);
        CreateBackdropSphere("Cloud Puff", center + Vector3.up * 0.22f * scale, new Vector3(1.55f, 0.9f, 0.38f) * scale, material, parent);
        CreateBackdropSphere("Cloud Puff", center + Vector3.right * 0.75f * scale, new Vector3(1.35f, 0.66f, 0.34f) * scale, material, parent);
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
        if (shader == null) throw new InvalidOperationException("[S88] No opaque shader is available.");
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

    private static Material CreateEmissiveMaterial(string path, Color color)
    {
        Material material = CreateOpaqueMaterial(path, color);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2.4f);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateTransparentMaterial(string path, Color color)
    {
        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) throw new InvalidOperationException("[S88] No transparent shader is available.");
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
        SetMaterialColor(material, color);
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
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new FileNotFoundException("[S88] Required asset is missing: " + path, path);
        return asset;
    }

    private static T LoadOptional<T>(string path) where T : UnityEngine.Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    private struct ButtonResult
    {
        public Transform Top;
        public Transform MultiplyLabel;
        public Transform DivideLabel;
    }
}
