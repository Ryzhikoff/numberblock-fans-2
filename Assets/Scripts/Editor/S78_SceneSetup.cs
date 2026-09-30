using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the vertical ten-second Scene_78 short.</summary>
public static class S78_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_78.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene78";
    private const string TextureRoot = AssetRoot + "/FaceTextures";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string PrefabPath = AssetRoot + "/Thousand_Smirk.prefab";

    private const string OneSourcePath = "Assets/Prefabs/Blocks/1.prefab";
    private const string ThousandSourcePath = "Assets/Prefabs/Blocks/1000.prefab";
    private const string OriginalFaceMaterialPath =
        "Assets/Materials/Materials/oneThousandFace.mat";
    private const string CleanBodyMaterialPath =
        "Assets/Materials/Materials/OneThousandTexture 1.mat";
    private const string SmirkSourcePath = TextureRoot + "/Thousand_Smirk_Source.png";
    private const string SmirkTexturePath = TextureRoot + "/Thousand_Smirk.png";
    private const string SmirkMaterialPath = MaterialRoot + "/Thousand_Smirk_Transparent.mat";

    private const string HappyWalkAudioPath =
        "Assets/Sound/March of the Hares - Nathan Moore (1).mp3";
    private const string ImpactAudioPath =
        "Assets/Sound/topSound/jg-032316-sfx-distant-meteor-crash-impact-2.mp3";
    private const string ImpactAccentAudioPath = "Assets/Sound/collCube.wav";

    private static readonly float[] CaptureTimes = { 1.4f, 3.45f, 4.03f, 4.55f, 6.0f, 9.0f };
    private static bool[] capturedFrames;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static double playModeStartTime;

    private struct PixelBounds
    {
        public int MinX;
        public int MinY;
        public int MaxX;
        public int MaxY;

        public int Width => MaxX - MinX + 1;
        public int Height => MaxY - MinY + 1;
        public bool IsValid => MaxX >= MinX && MaxY >= MinY;
    }

    [MenuItem("Tools/Scene_78/Quick Setup Short")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        ConfigureFaceTexture(SmirkSourcePath);
        NormalizeSmirkToOriginalFace();
        ConfigureFaceTexture(SmirkTexturePath);

        Material smirkMaterial = CreateTransparentFaceMaterial(
            SmirkMaterialPath,
            LoadAsset<Texture2D>(SmirkTexturePath));
        CreateThousandVariant(smirkMaterial);

        Material groundMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S78_Ground.mat",
            new Color(0.32f, 0.72f, 0.3f));
        Material hillMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S78_Hills.mat",
            new Color(0.22f, 0.61f, 0.29f));
        Material cloudMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S78_Clouds.mat",
            new Color(0.96f, 0.985f, 1f));
        Material dustMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S78_Dust.mat",
            new Color(0.93f, 0.78f, 0.48f));
        Material shadowMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S78_Shadow.mat",
            new Color(0.1f, 0.31f, 0.1f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        CreateEnvironment(groundMaterial, hillMaterial, cloudMaterial);
        GameObject shadow = CreateImpactShadow(shadowMaterial);

        GameObject gameManager = new GameObject("GameManager — 10 Second Timeline");
        S78_Main director = gameManager.AddComponent<S78_Main>();
        director.onePrefab = LoadAsset<GameObject>(OneSourcePath);
        director.thousandPrefab = LoadAsset<GameObject>(PrefabPath);
        director.oneCount = 7;
        director.thousandScale = 0.55f;
        director.dropHeight = 14f;
        director.impactPoint = new Vector3(0.25f, 0f, 0.55f);
        director.impactShadow = shadow;
        director.dustMaterial = dustMaterial;
        director.happyWalkClip = LoadAsset<AudioClip>(HappyWalkAudioPath);
        director.impactClip = LoadAsset<AudioClip>(ImpactAudioPath);
        director.impactAccentClip = LoadAsset<AudioClip>(ImpactAccentAudioPath);
        EditorUtility.SetDirty(director);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S78] Unity could not save the scene: {ScenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "[S78] Scene_78 created: seven walking Ones, a falling Thousand, impact scatter, " +
            "and an exact-position transparent smirk overlay.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_78 — Thousand lands",
                "Готово. Выберите Game View 9:16 и нажмите Play — шорт длится ровно 10 секунд.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_78/Validate Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S78_VALIDATE] Scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S78_Main director = UnityEngine.Object.FindAnyObjectByType<S78_Main>();
        if (director == null)
        {
            throw new InvalidOperationException("[S78_VALIDATE] Scene has no S78_Main director.");
        }

        if (AssetDatabase.GetAssetPath(director.onePrefab) != OneSourcePath ||
            AssetDatabase.GetAssetPath(director.thousandPrefab) != PrefabPath)
        {
            throw new InvalidOperationException(
                "[S78_VALIDATE] Scene does not use the original One or scene-local Thousand prefab.");
        }

        if (director.oneCount != 7 || director.impactShadow == null || director.dustMaterial == null)
        {
            throw new InvalidOperationException("[S78_VALIDATE] Cast or impact references are incomplete.");
        }

        if (director.impactClip == null || director.impactAccentClip == null)
        {
            throw new InvalidOperationException("[S78_VALIDATE] The landing needs both crash sound layers.");
        }

        Camera camera = Camera.main;
        if (camera == null || camera.name != "Main Camera — Vertical 9x16" ||
            Mathf.Abs(camera.fieldOfView - 46f) > 0.01f)
        {
            throw new InvalidOperationException("[S78_VALIDATE] Vertical 9:16 camera is misconfigured.");
        }

        if (Mathf.Abs(S78_Main.SequenceDuration - 10f) > 0.0001f ||
            S78_Main.ImpactTime >= S78_Main.SmirkTime)
        {
            throw new InvalidOperationException("[S78_VALIDATE] Timeline must be exactly ten seconds.");
        }

        ValidateFaceTexture(SmirkTexturePath);
        ValidateTransparentFacePixels();
        ValidateThousandVariant();

        Debug.Log(
            "[S78_VALIDATE] PASS — 10.000s, vertical 9:16 framing, seven Ones, original 1000 " +
            "geometry, layered crash audio, and exact-position RGBA smirk face.");
    }

    /// <summary>Runs the complete short at 2x and captures all important beats.</summary>
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S78_PLAY] This entry point is batch-mode only.");
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
            Debug.Log("[S78_PLAY] Entered Play Mode at 2x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;

            if (playModePassed)
            {
                Debug.Log(
                    $"[S78_PLAY] PASS — complete short and all {CaptureTimes.Length} keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S78_PLAY] FAIL — the complete short did not reach its ending.");
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

        S78_Main director = UnityEngine.Object.FindAnyObjectByType<S78_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene78-{i + 1:00}.png");
                }
            }

            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() && director.ImpactTriggered && director.SmirkVisible;
                EditorApplication.isPlaying = false;
                return;
            }
        }

        if (EditorApplication.timeSinceStartup - playModeStartTime > 15d)
        {
            Debug.LogError("[S78_PLAY] Timed out after 15 real seconds.");
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
            throw new InvalidOperationException("[S78_PLAY] Main Camera disappeared.");
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

        Debug.Log($"[S78_PLAY] Captured {path}");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets/Prefabs/Blocks", "Scene78");
        EnsureFolder(AssetRoot, "FaceTextures");
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

    private static void ConfigureFaceTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("[S78] Face texture was not imported.", path);
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

    private static void NormalizeSmirkToOriginalFace()
    {
        Texture2D source = LoadAsset<Texture2D>(SmirkSourcePath);
        Texture2D originalFace = LoadAsset<Material>(OriginalFaceMaterialPath).mainTexture as Texture2D;
        Texture2D cleanBody = LoadAsset<Material>(CleanBodyMaterialPath).mainTexture as Texture2D;
        if (originalFace == null || cleanBody == null)
        {
            throw new InvalidOperationException("[S78] Original Thousand face/body textures are missing.");
        }

        Texture2D sourceCopy = ReadTexture(source, source.width, source.height);
        Texture2D faceCopy = ReadTexture(originalFace, originalFace.width, originalFace.height);
        Texture2D bodyCopy = ReadTexture(cleanBody, originalFace.width, originalFace.height);

        try
        {
            PixelBounds sourceBounds = FindAlphaBounds(
                sourceCopy.GetPixels32(),
                sourceCopy.width,
                sourceCopy.height,
                8);
            PixelBounds targetBounds = FindDifferenceBounds(
                faceCopy.GetPixels32(),
                bodyCopy.GetPixels32(),
                faceCopy.width,
                faceCopy.height,
                0.2f);

            ValidateDetectedBounds(sourceBounds, sourceCopy.width, sourceCopy.height, "generated smirk");
            ValidateDetectedBounds(targetBounds, faceCopy.width, faceCopy.height, "original Thousand face");

            int width = sourceCopy.width;
            int height = sourceCopy.height;
            PixelBounds destination = ScaleBounds(targetBounds, faceCopy.width, faceCopy.height, width, height);
            Color[] result = new Color[width * height];

            for (int y = destination.MinY; y <= destination.MaxY; y++)
            {
                float v = destination.Height <= 1
                    ? 0.5f
                    : (y - destination.MinY) / (float)(destination.Height - 1);
                float sourceY = sourceBounds.MinY + v * (sourceBounds.Height - 1);
                float textureV = (sourceY + 0.5f) / sourceCopy.height;

                for (int x = destination.MinX; x <= destination.MaxX; x++)
                {
                    float u = destination.Width <= 1
                        ? 0.5f
                        : (x - destination.MinX) / (float)(destination.Width - 1);
                    float sourceX = sourceBounds.MinX + u * (sourceBounds.Width - 1);
                    float textureU = (sourceX + 0.5f) / sourceCopy.width;
                    result[y * width + x] = sourceCopy.GetPixelBilinear(textureU, textureV);
                }
            }

            Texture2D normalized = new Texture2D(width, height, TextureFormat.RGBA32, false);
            normalized.SetPixels(result);
            normalized.Apply(false, false);
            File.WriteAllBytes(Path.GetFullPath(SmirkTexturePath), normalized.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(normalized);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(sourceCopy);
            UnityEngine.Object.DestroyImmediate(faceCopy);
            UnityEngine.Object.DestroyImmediate(bodyCopy);
        }

        AssetDatabase.ImportAsset(SmirkTexturePath, ImportAssetOptions.ForceSynchronousImport);
    }

    private static PixelBounds FindAlphaBounds(
        Color32[] pixels,
        int width,
        int height,
        byte threshold)
    {
        PixelBounds bounds = InvalidBounds(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].a > threshold)
                {
                    Include(ref bounds, x, y);
                }
            }
        }
        return bounds;
    }

    private static PixelBounds FindDifferenceBounds(
        Color32[] facePixels,
        Color32[] bodyPixels,
        int width,
        int height,
        float threshold)
    {
        PixelBounds bounds = InvalidBounds(width, height);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color32 face = facePixels[index];
                Color32 body = bodyPixels[index];
                float difference = Mathf.Max(
                    Mathf.Abs(face.r - body.r),
                    Mathf.Max(Mathf.Abs(face.g - body.g), Mathf.Abs(face.b - body.b))) / 255f;
                if (difference > threshold && face.a > 8)
                {
                    Include(ref bounds, x, y);
                }
            }
        }
        return bounds;
    }

    private static PixelBounds InvalidBounds(int width, int height)
    {
        return new PixelBounds
        {
            MinX = width,
            MinY = height,
            MaxX = -1,
            MaxY = -1
        };
    }

    private static void Include(ref PixelBounds bounds, int x, int y)
    {
        bounds.MinX = Mathf.Min(bounds.MinX, x);
        bounds.MinY = Mathf.Min(bounds.MinY, y);
        bounds.MaxX = Mathf.Max(bounds.MaxX, x);
        bounds.MaxY = Mathf.Max(bounds.MaxY, y);
    }

    private static PixelBounds ScaleBounds(
        PixelBounds source,
        int sourceWidth,
        int sourceHeight,
        int destinationWidth,
        int destinationHeight)
    {
        return new PixelBounds
        {
            MinX = Mathf.Clamp(Mathf.RoundToInt(source.MinX / (float)sourceWidth * destinationWidth), 0, destinationWidth - 1),
            MinY = Mathf.Clamp(Mathf.RoundToInt(source.MinY / (float)sourceHeight * destinationHeight), 0, destinationHeight - 1),
            MaxX = Mathf.Clamp(Mathf.RoundToInt((source.MaxX + 1f) / sourceWidth * destinationWidth) - 1, 0, destinationWidth - 1),
            MaxY = Mathf.Clamp(Mathf.RoundToInt((source.MaxY + 1f) / sourceHeight * destinationHeight) - 1, 0, destinationHeight - 1)
        };
    }

    private static void ValidateDetectedBounds(
        PixelBounds bounds,
        int width,
        int height,
        string label)
    {
        if (!bounds.IsValid || bounds.Width < width * 0.12f || bounds.Height < height * 0.12f ||
            bounds.Width > width * 0.82f || bounds.Height > height * 0.9f)
        {
            throw new InvalidOperationException(
                $"[S78] Could not isolate {label}; detected bounds were " +
                $"{bounds.Width}x{bounds.Height} on {width}x{height}.");
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

    private static Material CreateTransparentFaceMaterial(string path, Texture2D texture)
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        shader ??= Shader.Find("Sprites/Default");
        if (shader == null)
        {
            throw new InvalidOperationException("[S78] No transparent shader is available.");
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

    private static void CreateThousandVariant(Material smirkMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(ThousandSourcePath);
        if (root == null)
        {
            throw new FileNotFoundException("[S78] Original 1000 prefab is missing.", ThousandSourcePath);
        }

        try
        {
            Material originalFaceMaterial = LoadAsset<Material>(OriginalFaceMaterialPath);
            Material cleanBodyMaterial = LoadAsset<Material>(CleanBodyMaterialPath);
            Renderer front = FindRendererUsingMaterial(root, originalFaceMaterial);
            if (front == null)
            {
                throw new InvalidOperationException("[S78] Original 1000 front face was not found.");
            }

            Bounds rootBounds = BoundsOf(root);
            Vector3 outward = front.bounds.center - rootBounds.center;
            if (outward.sqrMagnitude < 0.000001f)
            {
                outward = -front.transform.forward;
            }
            outward.Normalize();

            GameObject originalOverlay = UnityEngine.Object.Instantiate(
                front.gameObject,
                front.transform.parent,
                false);
            originalOverlay.name = S78_Main.OriginalFaceName;
            Renderer originalOverlayRenderer = originalOverlay.GetComponent<Renderer>();
            originalOverlayRenderer.sharedMaterial = originalFaceMaterial;
            ConfigureOverlayRenderer(originalOverlayRenderer);
            RemoveColliders(originalOverlay);
            originalOverlay.transform.position += outward * 0.006f;

            GameObject smirkOverlay = UnityEngine.Object.Instantiate(
                originalOverlay,
                originalOverlay.transform.parent,
                false);
            smirkOverlay.name = S78_Main.SmirkFaceName;
            Renderer smirkRenderer = smirkOverlay.GetComponent<Renderer>();
            smirkRenderer.sharedMaterial = smirkMaterial;
            ConfigureOverlayRenderer(smirkRenderer);
            RemoveColliders(smirkOverlay);
            smirkOverlay.transform.position += outward * 0.002f;
            smirkOverlay.SetActive(false);

            front.gameObject.name = "S78 Clean Front Body";
            ReplaceMaterial(front, originalFaceMaterial, cleanBodyMaterial);
            root.name = "Thousand_Smirk";
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureOverlayRenderer(Renderer renderer)
    {
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = 20;
    }

    private static void RemoveColliders(GameObject target)
    {
        foreach (Collider collider in target.GetComponents<Collider>())
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
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
            throw new InvalidOperationException("[S78] Original face material was not assigned to the front.");
        }
        renderer.sharedMaterials = materials;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Vertical 9x16");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.fieldOfView = 46f;
        camera.aspect = 9f / 16f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.49f, 0.82f, 0.97f);
        camera.transform.position = new Vector3(0f, 4.4f, -18f);
        camera.transform.LookAt(new Vector3(0f, 3.2f, 0.45f));
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(46f, -34f, 0f);
        RenderSettings.ambientLight = new Color(0.58f, 0.58f, 0.58f);
    }

    private static void CreateEnvironment(
        Material groundMaterial,
        Material hillMaterial,
        Material cloudMaterial)
    {
        GameObject environment = new GameObject("S78 Cheerful Park");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Grass Ground";
        ground.transform.SetParent(environment.transform, true);
        ground.transform.position = new Vector3(0f, -0.03f, 4f);
        ground.transform.localScale = new Vector3(6f, 1f, 7f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        RemoveColliders(ground);

        CreateBackdropSphere(
            "Left Hill",
            new Vector3(-7.2f, -1.25f, 11f),
            new Vector3(10f, 4.2f, 2f),
            hillMaterial,
            environment.transform);
        CreateBackdropSphere(
            "Right Hill",
            new Vector3(6.7f, -1.5f, 12f),
            new Vector3(11f, 4.7f, 2f),
            hillMaterial,
            environment.transform);

        GameObject clouds = new GameObject("Soft Clouds");
        clouds.transform.SetParent(environment.transform, false);
        CreateCloud(new Vector3(-2.7f, 8.5f, 9f), 0.8f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(2.9f, 10.2f, 10f), 0.58f, cloudMaterial, clouds.transform);
    }

    private static void CreateCloud(
        Vector3 center,
        float scale,
        Material material,
        Transform parent)
    {
        CreateBackdropSphere("Cloud Puff", center + Vector3.left * 0.72f * scale,
            new Vector3(1.45f, 0.72f, 0.35f) * scale, material, parent);
        CreateBackdropSphere("Cloud Puff", center + Vector3.up * 0.22f * scale,
            new Vector3(1.55f, 0.92f, 0.38f) * scale, material, parent);
        CreateBackdropSphere("Cloud Puff", center + Vector3.right * 0.78f * scale,
            new Vector3(1.38f, 0.68f, 0.34f) * scale, material, parent);
    }

    private static GameObject CreateBackdropSphere(
        string name,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
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

    private static GameObject CreateImpactShadow(Material material)
    {
        GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shadow.name = "Growing Landing Shadow";
        shadow.transform.position = new Vector3(0.25f, 0.015f, 0.55f);
        shadow.transform.localScale = Vector3.zero;
        shadow.GetComponent<Renderer>().sharedMaterial = material;
        RemoveColliders(shadow);
        shadow.SetActive(false);
        return shadow;
    }

    private static Material CreateOpaqueMaterial(string path, Color color)
    {
        // The original Numberblock materials in this project use Standard successfully.
        // Using a render-pipeline default here can select an incompatible shader when
        // multiple sample pipelines are installed, which renders bright magenta.
        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            throw new InvalidOperationException("[S78] No opaque shader is available.");
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
            throw new InvalidOperationException($"[S78_VALIDATE] Texture is not imported as RGBA: {path}");
        }
    }

    private static void ValidateTransparentFacePixels()
    {
        Texture2D texture = LoadAsset<Texture2D>(SmirkTexturePath);
        Texture2D copy = ReadTexture(texture, texture.width, texture.height);
        try
        {
            Color32[] pixels = copy.GetPixels32();
            int last = pixels.Length - 1;
            int upperLeft = (copy.height - 1) * copy.width;
            int upperRight = upperLeft + copy.width - 1;
            if (pixels[0].a > 8 || pixels[copy.width - 1].a > 8 ||
                pixels[upperLeft].a > 8 || pixels[upperRight].a > 8 || pixels[last].a > 8)
            {
                throw new InvalidOperationException(
                    "[S78_VALIDATE] Smirk texture corners are not genuinely transparent.");
            }

            PixelBounds finalBounds = FindAlphaBounds(pixels, copy.width, copy.height, 8);
            ValidateDetectedBounds(finalBounds, copy.width, copy.height, "normalized smirk");

            Material faceMaterial = LoadAsset<Material>(OriginalFaceMaterialPath);
            Material bodyMaterial = LoadAsset<Material>(CleanBodyMaterialPath);
            Texture2D face = faceMaterial.mainTexture as Texture2D;
            Texture2D body = bodyMaterial.mainTexture as Texture2D;
            Texture2D faceCopy = ReadTexture(face, face.width, face.height);
            Texture2D bodyCopy = ReadTexture(body, face.width, face.height);
            try
            {
                PixelBounds originalBounds = FindDifferenceBounds(
                    faceCopy.GetPixels32(),
                    bodyCopy.GetPixels32(),
                    faceCopy.width,
                    faceCopy.height,
                    0.2f);
                PixelBounds expected = ScaleBounds(
                    originalBounds,
                    faceCopy.width,
                    faceCopy.height,
                    copy.width,
                    copy.height);

                if (Mathf.Abs(finalBounds.MinX - expected.MinX) > 2 ||
                    Mathf.Abs(finalBounds.MinY - expected.MinY) > 2 ||
                    Mathf.Abs(finalBounds.MaxX - expected.MaxX) > 2 ||
                    Mathf.Abs(finalBounds.MaxY - expected.MaxY) > 2)
                {
                    throw new InvalidOperationException(
                        "[S78_VALIDATE] Smirk size or texture-space position differs from the original face.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(faceCopy);
                UnityEngine.Object.DestroyImmediate(bodyCopy);
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(copy);
        }
    }

    private static void ValidateThousandVariant()
    {
        GameObject source = PrefabUtility.LoadPrefabContents(ThousandSourcePath);
        GameObject variant = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Renderer sourceFace = FindRendererUsingMaterial(
                source,
                LoadAsset<Material>(OriginalFaceMaterialPath));
            Renderer cleanFront = FindRenderer(variant, "S78 Clean Front Body");
            Renderer originalOverlay = FindRenderer(variant, S78_Main.OriginalFaceName);
            Renderer smirkOverlay = FindRenderer(variant, S78_Main.SmirkFaceName);
            if (sourceFace == null || cleanFront == null || originalOverlay == null || smirkOverlay == null)
            {
                throw new InvalidOperationException("[S78_VALIDATE] Thousand face layers are incomplete.");
            }

            Vector3 sourceCenter = source.transform.InverseTransformPoint(sourceFace.bounds.center);
            Vector3 smirkCenter = variant.transform.InverseTransformPoint(smirkOverlay.bounds.center);
            if (Mathf.Abs(sourceCenter.x - smirkCenter.x) > 0.001f ||
                Mathf.Abs(sourceCenter.y - smirkCenter.y) > 0.001f ||
                sourceFace.transform.localRotation != smirkOverlay.transform.localRotation ||
                sourceFace.transform.localScale != smirkOverlay.transform.localScale)
            {
                throw new InvalidOperationException(
                    "[S78_VALIDATE] Smirk quad moved or changed size relative to the original face.");
            }

            if (cleanFront.sharedMaterial != LoadAsset<Material>(CleanBodyMaterialPath) ||
                smirkOverlay.sharedMaterial != LoadAsset<Material>(SmirkMaterialPath) ||
                smirkOverlay.sharedMaterial.renderQueue < (int)RenderQueue.Transparent)
            {
                throw new InvalidOperationException(
                    "[S78_VALIDATE] Clean grid or transparent smirk material is assigned incorrectly.");
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
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.name == objectName)
            {
                return renderer;
            }
        }
        return null;
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

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException($"[S78] Required asset is missing: {path}", path);
        }
        return asset;
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
}
