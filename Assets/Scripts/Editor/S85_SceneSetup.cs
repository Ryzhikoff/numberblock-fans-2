using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Creates Scene 85 and its private face/material/prefab assets; validates the whole film.</summary>
public static class S85_SceneSetup
{
    private const string Root = "Assets/Prefabs/Blocks/Scene85";
    private const string ScenePath = "Assets/Scenes/Scene_85.unity";
    private const string FaceManifestPath = "/private/tmp/scene85-faces.json";
    private const string BodyMaterials = "Assets/Prefabs/Blocks/materials/Materials/";

    [Serializable]
    private sealed class FaceInput
    {
        public string Name;
        public string Source;
        public int Width;
        public int Height;
    }

    [Serializable]
    private sealed class FaceManifest
    {
        public FaceInput[] Faces;
    }

    private sealed class Source
    {
        public int Number;
        public string Prefab;
        public string FaceMaterial;
        public string CleanMaterial;
        public string Smug;
        public string Scared;
        public Vector3Int Grid;

        public Source(int number, string prefab, string face, string clean, string smug, string scared, Vector3Int grid)
        {
            Number = number;
            Prefab = "Assets/Prefabs/Blocks/" + prefab + ".prefab";
            FaceMaterial = face;
            CleanMaterial = clean;
            Smug = smug;
            Scared = scared;
            Grid = grid;
        }
    }

    private static readonly Source[] Sources =
    {
        new Source(10, "10", BodyMaterials + "tenFace.mat", BodyMaterials + "tenBack.mat",
            "Ten_Smug", "Ten_Scared", new Vector3Int(2, 5, 1)),
        new Source(50, "50", BodyMaterials + "50-face.mat", null,
            "Fifty_Smug", "Fifty_Scared", new Vector3Int(5, 10, 1)),
        new Source(100, "100", "Assets/Materials/Materials/11.mat", BodyMaterials + "newMillion 1.mat",
            "Hundred_Smug", "Hundred_Scared", new Vector3Int(10, 10, 1)),
        new Source(500, "500", BodyMaterials + "500-face.mat", BodyMaterials + "500-back.mat",
            "FiveHundred_Smug", "FiveHundred_Scared", new Vector3Int(10, 50, 1)),
        new Source(1000, "1000", "Assets/Materials/Materials/oneThousandFace.mat",
            "Assets/Materials/Materials/OneThousandTexture 1.mat", "Thousand_Smug", "Thousand_Scared", new Vector3Int(10, 10, 10)),
        new Source(5000, "5000", BodyMaterials + "500-face.mat", BodyMaterials + "500-back.mat",
            "FiveHundred_Smug", "FiveHundred_Scared", new Vector3Int(10, 50, 10)),
        new Source(10000, "prefabTenThousand", BodyMaterials + "10000-face.mat", BodyMaterials + "10000-back.mat",
            "TenThousand_Smug", "TenThousand_Scared", new Vector3Int(20, 50, 10)),
        new Source(50000, "50_000", BodyMaterials + "50-face.mat", null,
            "Fifty_Smug", "Fifty_Scared", new Vector3Int(50, 100, 10)),
        new Source(100000, "OneHundredThousand", BodyMaterials + "1k-face.mat", BodyMaterials + "1k-back.mat",
            "HundredThousand_Smug", "HundredThousand_Scared", new Vector3Int(100, 100, 10)),
        new Source(500000, "OneHundredThousand", BodyMaterials + "500-face.mat", null,
            "FiveHundred_Smug", "FiveHundred_Scared", new Vector3Int(100, 500, 10)),
        new Source(1000000, "OneMillion", "Assets/Materials/Materials/oneMillionFaceTexture.mat",
            "Assets/Materials/Materials/oneMillionTexture.mat", "Million_Smug", "Million_Smug", new Vector3Int(100, 100, 100))
    };

    private static Dictionary<string, string> sourceHashes;
    private static readonly HashSet<string> captures = new HashSet<string>();
    private static bool playStarted;
    private static bool playPassed;
    private static bool playError;
    private static double playStart;
    private static bool originalPlayOptionsEnabled;
    private static EnterPlayModeOptions originalPlayOptions;

    [MenuItem("Tools/Scene_85/Create Landscape Stomp Chain")]
    public static void QuickSetup()
    {
        sourceHashes = HashSources();
        EnsureFolders();
        NormalizeFaces();
        S85_Main.Character[] characters = new S85_Main.Character[Sources.Length];
        for (int i = 0; i < Sources.Length; i++)
        {
            Source source = Sources[i];
            string path = PrefabPath(source.Number);
            if (source.Number == 500000) CreateFiveHundredThousand(source, path);
            else CreateVariant(source, path);
            characters[i] = new S85_Main.Character
            {
                Number = source.Number,
                Prefab = Load<GameObject>(path),
                UnitGrid = source.Grid
            };
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateCamera();
        CreateEnvironment();
        GameObject burstObject = new GameObject("S85 Original Ones — Bounded GPU Crowd");
        S85_UnitBurst burst = burstObject.AddComponent<S85_UnitBurst>();
        burst.OnePrefab = Load<GameObject>("Assets/Prefabs/Blocks/1.prefab");
        GameObject manager = new GameObject("S85 Director — 10 to 1 Million, Wait for Every One");
        S85_Main director = manager.AddComponent<S85_Main>();
        director.Characters = characters;
        director.ShotCamera = camera;
        director.UnitBurst = burst;
        ConfigureAudio(director);
        EditorUtility.SetDirty(director);
        EditorUtility.SetDirty(burst);
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save Scene 85.");
        AssetDatabase.SaveAssets();
        S85_VideoRecorder.CreatePreset();
        ValidateSourceHashes();
        ValidateScene();
        Debug.Log("[S85_SETUP] PASS — landscape scene, eleven real-sized characters, private expressions and bounded crowd.");
    }

    [MenuItem("Tools/Scene_85/Update Timing and Audio")]
    public static void UpdateTimingAndAudio()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S85_Main director = UnityEngine.Object.FindAnyObjectByType<S85_Main>();
        if (director == null) throw new InvalidOperationException("Scene 85 director is missing.");
        ConfigureAudio(director);
        EditorUtility.SetDirty(director);
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Cannot save Scene 85.");
        AssetDatabase.SaveAssets();
        ValidateScene();
        Debug.Log("[S85_SETUP] Timing/audio updated — immediate jump, adjusted effects and looping music at 0.2.");
    }

    private static void ConfigureAudio(S85_Main director)
    {
        director.ApproachClip = Load<AudioClip>("Assets/Sound/zvuk-priblijeniya.mp3");
        director.StompClip = Load<AudioClip>("Assets/Sound/boom_metal.wav");
        director.BurstClip = Load<AudioClip>("Assets/Sound/destroy_blocks.mp3");
        director.RunClip = Load<AudioClip>("Assets/Sound/runaway.mp3");
        director.MusicClip = Load<AudioClip>("Assets/Sound/Survival Mode - Blue Deer Studio.mp3");
        director.ApproachVolume = 0.25f;
        director.StompVolume = 0.95f;
        director.BurstVolume = 0.68f;
        director.RunVolume = 0.5f;
        director.MusicVolume = 0.2f;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene85");
        EnsureFolder(Root, "FaceTextures");
        EnsureFolder(Root, "BodyTextures");
        EnsureFolder(Root, "Materials");
        EnsureFolder(Root, "Prefabs");
    }

    private static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }

    private static void NormalizeFaces()
    {
        if (!File.Exists(FaceManifestPath))
        {
            if (File.Exists(FacePath("Million_Smug"))) return;
            throw new FileNotFoundException("Prepared image inputs are missing.", FaceManifestPath);
        }
        FaceManifest manifest = JsonUtility.FromJson<FaceManifest>(File.ReadAllText(FaceManifestPath));
        foreach (FaceInput input in manifest.Faces)
        {
            string preparedPath = FacePath(input.Name);
            if (File.Exists(preparedPath) && File.GetLastWriteTimeUtc(preparedPath) > File.GetLastWriteTimeUtc(input.Source))
            {
                ConfigureTexture(preparedPath, true);
                continue;
            }
            Texture2D original = ReadPng(input.Source);
            try
            {
                ValidateAlpha(original, input.Source);
                // Only mechanical resampling to the original source canvas, no creative repainting.
                Texture2D normalized = new Texture2D(input.Width, input.Height, TextureFormat.RGBA32, false);
                try
                {
                    Color[] pixels = new Color[input.Width * input.Height];
                    for (int y = 0; y < input.Height; y++)
                    for (int x = 0; x < input.Width; x++)
                    {
                        Color colour = original.GetPixelBilinear((x + 0.5f) / input.Width, (y + 0.5f) / input.Height);
                        if (colour.a < 0.015f) colour = Color.clear;
                        pixels[y * input.Width + x] = colour;
                    }
                    normalized.SetPixels(pixels);
                    normalized.Apply();
                    ValidateAlpha(normalized, input.Name);
                    string path = FacePath(input.Name);
                    File.WriteAllBytes(path, normalized.EncodeToPNG());
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    ConfigureTexture(path, true);
                    Debug.Log($"[S85_FACE] {input.Name}: RGBA, {input.Width}x{input.Height}, zero-alpha corners/background.");
                }
                finally { UnityEngine.Object.DestroyImmediate(normalized); }
            }
            finally { UnityEngine.Object.DestroyImmediate(original); }
        }
    }

    private static Texture2D ReadPng(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        if (bytes.Length < 26 || bytes[25] != 6)
            throw new InvalidOperationException($"[S85_FACE] Expected genuine RGBA PNG, not a drawn background: {path}");
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes, false)) throw new IOException("Invalid image: " + path);
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    private static void ValidateAlpha(Texture2D texture, string name)
    {
        Color32[] pixels = texture.GetPixels32();
        int width = texture.width;
        int height = texture.height;
        if (pixels[0].a != 0 || pixels[width - 1].a != 0 ||
            pixels[(height - 1) * width].a != 0 || pixels[pixels.Length - 1].a != 0)
            throw new InvalidOperationException("[S85_FACE] Nontransparent corners: " + name);
        int transparent = 0;
        int opaque = 0;
        foreach (Color32 colour in pixels)
        {
            if (colour.a == 0) transparent++;
            if (colour.a > 220) opaque++;
        }
        if (transparent < pixels.Length / 5 || opaque < pixels.Length / 100)
            throw new InvalidOperationException("[S85_FACE] Missing visible features or true empty background: " + name);
    }

    private static void ConfigureTexture(string path, bool face)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = face;
        importer.mipmapEnabled = false;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 8192;
        importer.SaveAndReimport();
    }

    private static void CreateVariant(Source source, string path)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(source.Prefab);
        try
        {
            Material original = Load<Material>(source.FaceMaterial);
            Renderer front = FindRenderer(root, original);
            if (front == null) throw new InvalidOperationException($"No original face plane for {source.Number}.");
            Material clean = source.CleanMaterial == null ? null : Load<Material>(source.CleanMaterial);
            if (clean != null)
            {
                // Happy uses the untouched original baked front. Smug/scared use clean body + transparent face.
                GameObject happy = ClonePlane(front, S85_Main.HappyFaceName, original, 0.003f, false);
                happy.SetActive(true);
                Material[] materials = front.sharedMaterials;
                for (int i = 0; i < materials.Length; i++) if (materials[i] == original) materials[i] = clean;
                front.sharedMaterials = materials;
                front.name = "S85 Clean Front Body";
            }
            else
            {
                front.name = S85_Main.HappyFaceName;
                front.shadowCastingMode = ShadowCastingMode.Off;
                front.receiveShadows = false;
            }
            ClonePlane(front, S85_Main.SmugFaceName, FaceMaterial(source.Smug), 0.008f, true).SetActive(false);
            ClonePlane(front, S85_Main.ScaredFaceName, FaceMaterial(source.Scared), 0.011f, true).SetActive(false);
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++) materials[i] = MatteCopy(materials[i]);
                renderer.sharedMaterials = materials;
            }
            root.name = "Numberblock_" + source.Number + "_Expressions";
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static GameObject ClonePlane(Renderer front, string name, Material material, float offset, bool transparent)
    {
        GameObject overlay = UnityEngine.Object.Instantiate(front.gameObject, front.transform.parent, false);
        overlay.name = name;
        overlay.transform.position += -front.transform.forward.normalized * offset;
        Renderer renderer = overlay.GetComponent<Renderer>();
        renderer.enabled = true;
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = transparent ? 20 : 0;
        foreach (Collider collider in overlay.GetComponents<Collider>()) UnityEngine.Object.DestroyImmediate(collider);
        return overlay;
    }

    private static void CreateFiveHundredThousand(Source source, string path)
    {
        Material wide = CreateGridBody("FiveHundredThousand_Wide", 100, 100);
        Material narrow = CreateGridBody("FiveHundredThousand_Narrow", 10, 100);
        GameObject root = new GameObject("Numberblock_500000_Expressions");
        try
        {
            Renderer topFront = null;
            Material originalFace = Load<Material>(BodyMaterials + "1k-face.mat");
            for (int i = 0; i < 5; i++)
            {
                GameObject section = UnityEngine.Object.Instantiate(Load<GameObject>(source.Prefab), root.transform);
                section.name = "100000 Unit Section " + (i + 1);
                section.transform.localPosition = Vector3.zero;
                Bounds bounds = S85_Main.BoundsOf(section);
                section.transform.position += new Vector3(50f - bounds.center.x, i * 100f - bounds.min.y, 5f - bounds.center.z);
                foreach (Renderer renderer in section.GetComponentsInChildren<Renderer>(true))
                {
                    if (i == 4 && Array.IndexOf(renderer.sharedMaterials, originalFace) >= 0) topFront = renderer;
                    Vector3 scale = renderer.transform.localScale;
                    renderer.sharedMaterial = Mathf.Min(Mathf.Abs(scale.x), Mathf.Abs(scale.y)) < 50f ? narrow : wide;
                }
            }
            if (topFront == null) throw new InvalidOperationException("500000 upper source plane not found.");
            ClonePlane(topFront, S85_Main.HappyFaceName, FaceMaterial("FiveHundred_Happy"), 0.008f, true);
            ClonePlane(topFront, S85_Main.SmugFaceName, FaceMaterial(source.Smug), 0.011f, true).SetActive(false);
            ClonePlane(topFront, S85_Main.ScaredFaceName, FaceMaterial(source.Scared), 0.014f, true).SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static Material CreateGridBody(string name, int cellsX, int cellsY)
    {
        int width = cellsX * 10;
        int height = cellsY * 10;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        try
        {
            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool line = x % 10 == 0 || y % 10 == 0;
                bool light = ((x / 10 + y / 10) & 1) == 0;
                pixels[y * width + x] = line ? new Color32(10, 174, 157, 255) :
                    light ? new Color32(90, 255, 224, 255) : new Color32(22, 232, 201, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            string texturePath = Root + "/BodyTextures/" + name + ".png";
            File.WriteAllBytes(texturePath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureTexture(texturePath, false);
            return MaterialAsset(name, Color.white, Load<Texture2D>(texturePath), false);
        }
        finally { UnityEngine.Object.DestroyImmediate(texture); }
    }

    private static Material FaceMaterial(string name) => MaterialAsset(name + "_Transparent", Color.white, Load<Texture2D>(FacePath(name)), true);

    private static Material MatteCopy(Material original)
    {
        if (original == null || AssetDatabase.GetAssetPath(original).StartsWith(Root + "/", StringComparison.Ordinal)) return original;
        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(original));
        string path = Root + "/Materials/Matte_" + guid + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(original);
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = original.shader;
        material.CopyPropertiesFromMaterial(original);
        material.name = "S85 Matte " + original.name;
        ConfigureMatte(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ConfigureMatte(Material material)
    {
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0f);
        if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0f);
        if (material.HasProperty("_GlossMapScale")) material.SetFloat("_GlossMapScale", 0f);
        if (material.HasProperty("_SpecularHighlights"))
        {
            material.SetFloat("_SpecularHighlights", 0f);
            material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        }
        if (material.HasProperty("_GlossyReflections"))
        {
            material.SetFloat("_GlossyReflections", 0f);
            material.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
        }
    }

    private static Material MaterialAsset(string name, Color colour, Texture2D texture, bool transparent)
    {
        bool srp = GraphicsSettings.currentRenderPipeline != null;
        Shader shader = Shader.Find(srp ? (transparent || texture != null ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit") :
            (transparent ? "Unlit/Transparent" : "Standard"));
        if (shader == null || !shader.isSupported) throw new InvalidOperationException("Required scene shader is unsupported.");
        string path = Root + "/Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
        material.shader = shader;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", colour);
        if (material.HasProperty("_Color")) material.SetColor("_Color", colour);
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        ConfigureMatte(material);
        material.SetFloat("_Surface", transparent ? 1f : 0f);
        material.SetFloat("_ZWrite", transparent ? 0f : 1f);
        material.SetFloat("_Cull", 0f);
        material.SetFloat("_SrcBlend", transparent ? (float)BlendMode.SrcAlpha : (float)BlendMode.One);
        material.SetFloat("_DstBlend", transparent ? (float)BlendMode.OneMinusSrcAlpha : (float)BlendMode.Zero);
        material.SetOverrideTag("RenderType", transparent ? "Transparent" : "Opaque");
        material.renderQueue = transparent ? (int)RenderQueue.Transparent : (int)RenderQueue.Geometry;
        if (transparent) material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        else material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.SetShaderPassEnabled("ShadowCaster", !transparent);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Landscape 16x9, Real Scale");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.orthographic = false;
        camera.fieldOfView = 36f;
        camera.aspect = 16f / 9f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 10000f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.46f, 0.8f, 0.98f);
        camera.transform.SetPositionAndRotation(new Vector3(3.5f, 5.5f, -17f), Quaternion.Euler(9f, -12f, 0f));
        return camera;
    }

    private static void CreateEnvironment()
    {
        RenderSettings.fog = false;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.6f);
        GameObject lightObject = new GameObject("S85 Soft Daylight");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.7f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "S85 Continuous Ground — Original World Units";
        ground.transform.position = new Vector3(0f, -0.5f, 0f);
        ground.transform.localScale = new Vector3(12000f, 1f, 12000f);
        ground.GetComponent<Renderer>().sharedMaterial = MaterialAsset("S85_Ground", new Color(0.38f, 0.82f, 0.43f), null, false);
        // Landmark trees retain the same world size through every camera pullback.
        Material trunk = MaterialAsset("S85_Trunk", new Color(0.34f, 0.2f, 0.12f), null, false);
        Material leaves = MaterialAsset("S85_Leaves", new Color(0.12f, 0.5f, 0.27f), null, false);
        for (int i = 0; i < 18; i++)
        {
            float x = -240f + i * 29f;
            float z = 150f + (i % 3) * 35f;
            CreatePrimitive("S85 Tree Trunk", PrimitiveType.Cylinder, new Vector3(x, 2f, z), new Vector3(0.65f, 2f, 0.65f), trunk);
            CreatePrimitive("S85 Tree Crown", PrimitiveType.Sphere, new Vector3(x, 5f, z), new Vector3(4.5f, 5f, 4.5f), leaves);
        }
    }

    private static void CreatePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
    }

    [MenuItem("Tools/Scene_85/Validate Landscape Stomp Chain")]
    public static void ValidateScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S85_Main director = UnityEngine.Object.FindAnyObjectByType<S85_Main>();
        if (director == null || director.Characters.Length != Sources.Length || director.ShotCamera == null ||
            director.UnitBurst == null || director.UnitBurst.OnePrefab == null ||
            director.ApproachClip == null || director.StompClip == null || director.BurstClip == null || director.RunClip == null || director.MusicClip == null)
            throw new InvalidOperationException("[S85_VALIDATE] Missing scene references.");
        if (director.ShotCamera.orthographic || Mathf.Abs(director.ShotCamera.fieldOfView - 36f) > 0.001f ||
            director.ShotCamera.name != "Main Camera — Landscape 16x9, Real Scale")
            throw new InvalidOperationException("[S85_VALIDATE] Incorrect landscape camera.");
        for (int i = 0; i < Sources.Length; i++)
        {
            Source source = Sources[i];
            S85_Main.Character character = director.Characters[i];
            if (character.Number != source.Number || character.UnitGrid != source.Grid ||
                AssetDatabase.GetAssetPath(character.Prefab) != PrefabPath(source.Number) ||
                (long)source.Grid.x * source.Grid.y * source.Grid.z != source.Number)
                throw new InvalidOperationException("[S85_VALIDATE] Incorrect chain or unit volume.");
            GameObject variant = PrefabUtility.LoadPrefabContents(PrefabPath(source.Number));
            GameObject original = PrefabUtility.LoadPrefabContents(source.Prefab);
            try
            {
                if (variant.transform.localScale != Vector3.one || variant.transform.localScale != original.transform.localScale)
                    throw new InvalidOperationException($"[S85_VALIDATE] Root scale changed: {source.Number}");
                Bounds originalBounds = S85_Main.BoundsOf(original);
                Bounds variantBounds = S85_Main.BoundsOf(variant);
                Vector3 expected = source.Number == 500000 ? new Vector3(100f, 500f, 10f) : originalBounds.size;
                if ((variantBounds.size - expected).magnitude > 0.06f)
                    throw new InvalidOperationException($"[S85_VALIDATE] Original geometry changed: {source.Number}: {variantBounds.size}, expected {expected}");
                foreach (string faceName in new[] { S85_Main.HappyFaceName, S85_Main.SmugFaceName, S85_Main.ScaredFaceName })
                {
                    Transform face = FindChild(variant.transform, faceName);
                    if (face == null) throw new InvalidOperationException("[S85_VALIDATE] Missing expression: " + source.Number);
                    Renderer renderer = face.GetComponent<Renderer>();
                    if (renderer == null || renderer.sharedMaterial == null) throw new InvalidOperationException("[S85_VALIDATE] Missing face material.");
                    if (faceName == S85_Main.HappyFaceName) continue;
                    Material material = renderer.sharedMaterial;
                    bool builtinTransparent = material.shader.name == "Unlit/Transparent";
                    if (!material.shader.isSupported || material.renderQueue != (int)RenderQueue.Transparent ||
                        (!builtinTransparent && (material.GetFloat("_ZWrite") != 0f ||
                        material.GetFloat("_SrcBlend") != (float)BlendMode.SrcAlpha || material.GetFloat("_DstBlend") != (float)BlendMode.OneMinusSrcAlpha)) ||
                        renderer.shadowCastingMode != ShadowCastingMode.Off || renderer.receiveShadows)
                        throw new InvalidOperationException("[S85_VALIDATE] Incorrect transparent overlay.");
                    string texturePath = AssetDatabase.GetAssetPath(material.mainTexture);
                    Texture2D png = ReadPng(texturePath);
                    try { ValidateAlpha(png, texturePath); }
                    finally { UnityEngine.Object.DestroyImmediate(png); }
                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                    if (!importer.alphaIsTransparency || importer.mipmapEnabled || importer.npotScale != TextureImporterNPOTScale.None ||
                        importer.wrapMode != TextureWrapMode.Clamp || importer.textureCompression != TextureImporterCompression.Uncompressed)
                        throw new InvalidOperationException("[S85_VALIDATE] Incorrect face importer.");
                    Renderer originalFace = source.Number == 500000 ? null : FindRenderer(original, Load<Material>(source.FaceMaterial));
                    if (originalFace != null)
                    {
                        Vector3 delta = face.position - originalFace.transform.position;
                        Vector3 tangent = delta - Vector3.Project(delta, originalFace.transform.forward);
                        if (tangent.magnitude > 0.003f || Quaternion.Angle(face.rotation, originalFace.transform.rotation) > 0.01f ||
                            (face.lossyScale - originalFace.transform.lossyScale).magnitude > 0.01f)
                            throw new InvalidOperationException("[S85_VALIDATE] Face moved relative to original plane: " + source.Number);
                        Texture2D originalTexture = (Texture2D)Load<Material>(source.FaceMaterial).mainTexture;
                        byte[] originalBytes = File.ReadAllBytes(AssetDatabase.GetAssetPath(originalTexture));
                        Texture2D newTexture = (Texture2D)material.mainTexture;
                        int originalWidth = PngDimension(originalBytes, 16);
                        int originalHeight = PngDimension(originalBytes, 20);
                        if (newTexture.width != originalWidth || newTexture.height != originalHeight)
                            throw new InvalidOperationException("[S85_VALIDATE] Wrong original face canvas: " + source.Number);
                    }
                }
                Debug.Log($"[S85_VALIDATE] {source.Number}: unchanged size {expected}, original root scale, valid expressions.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(variant);
                PrefabUtility.UnloadPrefabContents(original);
            }
        }
        ValidateSourceHashes();
        Debug.Log("[S85_VALIDATE] PASS — chain, original sizes/planes, true RGBA, transparent blend/importers, audio and crowd references.");
    }

    public static void DescribeSources()
    {
        foreach (Source source in Sources)
        {
            GameObject original = PrefabUtility.LoadPrefabContents(source.Prefab);
            try
            {
                Debug.Log($"[S85_SOURCE] {source.Number} root scale={original.transform.localScale} bounds={S85_Main.BoundsOf(original).size}");
                foreach (Renderer renderer in original.GetComponentsInChildren<Renderer>(true))
                    Debug.Log($"[S85_SOURCE] {renderer.name}, local scale={renderer.transform.localScale}, material={renderer.sharedMaterial?.name}");
            }
            finally { PrefabUtility.UnloadPrefabContents(original); }
        }
    }

    public static void ReviewFaces()
    {
        string[] files = Directory.GetFiles(Root + "/FaceTextures", "*.png");
        Array.Sort(files);
        const int cell = 320;
        const int columns = 4;
        int rows = (files.Length + columns - 1) / columns;
        Texture2D sheet = new Texture2D(cell * columns, cell * rows, TextureFormat.RGBA32, false);
        try
        {
            Color[] pixels = new Color[sheet.width * sheet.height];
            for (int i = 0; i < files.Length; i++)
            {
                Texture2D face = ReadPng(files[i]);
                try
                {
                    ValidateAlpha(face, files[i]);
                    int width = Mathf.RoundToInt(Mathf.Min(cell - 20, (cell - 20) * ((float)face.width / face.height)));
                    int height = Mathf.RoundToInt(width * (float)face.height / face.width);
                    int left = i % columns * cell + (cell - width) / 2;
                    int bottom = (rows - 1 - i / columns) * cell + (cell - height) / 2;
                    for (int y = 0; y < cell; y++)
                    for (int x = 0; x < cell; x++)
                    {
                        int sx = i % columns * cell + x;
                        int sy = (rows - 1 - i / columns) * cell + y;
                        pixels[sy * sheet.width + sx] = i % 2 == 0 ? new Color(0.95f, 0.1f, 0.8f) : new Color(0.12f, 0.9f, 0.5f);
                    }
                    for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        Color colour = face.GetPixelBilinear((x + 0.5f) / width, (y + 0.5f) / height);
                        int index = (bottom + y) * sheet.width + left + x;
                        pixels[index] = Color.Lerp(pixels[index], new Color(colour.r, colour.g, colour.b, 1f), colour.a);
                    }
                    Debug.Log($"[S85_REVIEW] Cell {i + 1}: {Path.GetFileName(files[i])}");
                }
                finally { UnityEngine.Object.DestroyImmediate(face); }
            }
            sheet.SetPixels(pixels);
            sheet.Apply();
            File.WriteAllBytes("/private/tmp/scene85-face-review.png", sheet.EncodeToPNG());
        }
        finally { UnityEngine.Object.DestroyImmediate(sheet); }
    }

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
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) playError = true;
    }

    private static void OnPlayState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            playStarted = true;
            playStart = EditorApplication.timeSinceStartup;
            // Fixed simulated steps keep jumps, running and last-unit gates observable during validation.
            Time.captureDeltaTime = 1f / 20f;
            Debug.Log("[S85_PLAY] Started full-film validation at fixed 20fps simulation.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playStarted)
        {
            Time.captureDeltaTime = 0f;
            EditorSettings.enterPlayModeOptionsEnabled = originalPlayOptionsEnabled;
            EditorSettings.enterPlayModeOptions = originalPlayOptions;
            Application.logMessageReceived -= OnLog;
            EditorApplication.playModeStateChanged -= OnPlayState;
            EditorApplication.update -= PlayTick;
            Debug.Log(playPassed ? "[S85_PLAY] PASS — all ten stomps, complete escapes, contacts, original sizes and finale." : "[S85_PLAY] FAIL");
            EditorApplication.Exit(playPassed ? 0 : 1);
        }
    }

    private static void PlayTick()
    {
        if (!playStarted || !EditorApplication.isPlaying) return;
        S85_Main director = UnityEngine.Object.FindAnyObjectByType<S85_Main>();
        if (director != null)
        {
            bool ready = (director.Beat == "Happy" && director.BeatTime > 1f) ||
                (director.Beat == "Approach" && director.BeatTime > director.ApproachDuration * 0.85f) ||
                (director.Beat == "Jump" && director.BeatTime > director.JumpDuration * 0.48f) ||
                (director.Beat == "Crush" && director.BeatTime > director.CrushDuration * 0.75f) ||
                (director.Beat == "Burst" && director.BeatTime > 0.25f) ||
                (director.Beat == "Escape" && director.BeatTime > 0.8f) ||
                (director.Beat == "WinnerRise" && director.BeatTime > director.WinnerRiseDuration * 0.5f) ||
                (director.Beat == "Winner" && director.BeatTime > director.WinnerHold * 0.75f) ||
                (director.Beat == "Final" && director.BeatTime > 2f);
            string key = $"{director.RoundIndex:00}-{director.Beat}";
            if (ready && captures.Add(key)) CaptureFrame("/private/tmp/scene85-" + key + ".png");
            if (director.SequenceComplete)
            {
                playPassed = !playError && director.CompletedRounds == 10 && director.EscapedNumbers.Count == 10 &&
                    director.CurrentNumber == 1000000 && director.ContactIsValid && director.ScalesAreOriginal &&
                    director.UnitBurst.ActiveUnitCount == 0 && captures.Count >= 61;
                Debug.Log($"[S85_PLAY] duration={director.SequenceTime:F2}s captures={captures.Count} completed={director.CompletedRounds} errors={playError}");
                EditorApplication.isPlaying = false;
                return;
            }
        }
        if (EditorApplication.timeSinceStartup - playStart > 1200d || playError)
        {
            playPassed = false;
            Debug.LogError("[S85_PLAY] Runtime error or film validation timeout.");
            EditorApplication.isPlaying = false;
        }
    }

    private static void CaptureFrame(string path)
    {
        Camera camera = Camera.main;
        RenderTexture render = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
        Texture2D texture = new Texture2D(1280, 720, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;
        try
        {
            camera.targetTexture = render;
            camera.Render();
            RenderTexture.active = render;
            texture.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
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
        Debug.Log("[S85_PLAY] Captured " + path);
    }

    private static Dictionary<string, string> HashSources()
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach (Source source in Sources)
        foreach (string path in new[] { source.Prefab, source.FaceMaterial, source.CleanMaterial })
            if (path != null) result[path] = Hash(path);
        result["Assets/Prefabs/Blocks/1.prefab"] = Hash("Assets/Prefabs/Blocks/1.prefab");
        return result;
    }

    private static string Hash(string path)
    {
        using (SHA256 algorithm = SHA256.Create()) return Convert.ToBase64String(algorithm.ComputeHash(File.ReadAllBytes(path)));
    }

    private static void ValidateSourceHashes()
    {
        if (sourceHashes == null) return;
        foreach (KeyValuePair<string, string> item in sourceHashes)
            if (Hash(item.Key) != item.Value) throw new InvalidOperationException("[S85_VALIDATE] Shared source changed: " + item.Key);
    }

    private static Renderer FindRenderer(GameObject root, Material material)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            if (Array.IndexOf(renderer.sharedMaterials, material) >= 0) return renderer;
        return null;
    }

    private static Transform FindChild(Transform root, string name)
    {
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true)) if (transform.name == name) return transform;
        return null;
    }

    private static string FacePath(string name) => Root + "/FaceTextures/" + name + ".png";
    private static string PrefabPath(int number) => Root + "/Prefabs/Numberblock_" + number + "_Expressions.prefab";
    private static int PngDimension(byte[] bytes, int index) =>
        (bytes[index] << 24) | (bytes[index + 1] << 16) | (bytes[index + 2] << 8) | bytes[index + 3];

    private static T Load<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new FileNotFoundException("[S85] Missing Unity asset: " + path, path);
        return asset;
    }
}
