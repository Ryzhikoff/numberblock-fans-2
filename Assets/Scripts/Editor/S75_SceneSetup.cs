using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Builds the scene-local prefabs and the complete ten-second Scene_75.</summary>
public static class S75_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_75.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene75";
    private const string TextureRoot = AssetRoot + "/Textures";
    private const string MaterialRoot = AssetRoot + "/Materials";

    private const string HundredSourcePath = "Assets/Prefabs/Blocks/100.prefab";
    private const string FiveThousandSourcePath = "Assets/Prefabs/Blocks/5000.prefab";
    private const string OneSourcePath = "Assets/Prefabs/Blocks/1.prefab";

    private const string HundredSmilingPath = AssetRoot + "/Hundred_Smiling.prefab";
    private const string HundredSurprisedPath = AssetRoot + "/Hundred_Surprised.prefab";
    private const string HundredScaredPath = AssetRoot + "/Hundred_Scared.prefab";
    private const string FiveThousandLookingPath = AssetRoot + "/FiveThousand_LookingAtHundred.prefab";
    private const string OneRunnerPath = AssetRoot + "/One_Runner.prefab";

    private const string HundredSurprisedTexturePath = TextureRoot + "/Hundred_Surprised.png";
    private const string HundredScaredTexturePath = TextureRoot + "/Hundred_Scared.png";
    private const string FiveThousandLookingTexturePath = TextureRoot + "/FiveThousand_LookingAtHundred.png";

    [MenuItem("Tools/Scene_75/Quick Setup Scene")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        ConfigureFaceTexture(HundredSurprisedTexturePath);
        ConfigureFaceTexture(HundredScaredTexturePath);
        ConfigureFaceTexture(FiveThousandLookingTexturePath);

        CreatePrefabCopy(HundredSourcePath, HundredSmilingPath, "Hundred_Smiling");
        CreateFaceVariant(
            HundredSourcePath,
            HundredSurprisedPath,
            HundredSurprisedTexturePath,
            MaterialRoot + "/Hundred_Surprised.mat",
            "Hundred_Surprised");
        CreateFaceVariant(
            HundredSourcePath,
            HundredScaredPath,
            HundredScaredTexturePath,
            MaterialRoot + "/Hundred_Scared.mat",
            "Hundred_Scared");
        CreateFaceVariant(
            FiveThousandSourcePath,
            FiveThousandLookingPath,
            FiveThousandLookingTexturePath,
            MaterialRoot + "/FiveThousand_LookingAtHundred.mat",
            "FiveThousand_LookingAtHundred");
        CreatePrefabCopy(OneSourcePath, OneRunnerPath, "One_Runner");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLight();
        CreateGround();

        GameObject gameManager = new GameObject("GameManager");
        S75_Main director = gameManager.AddComponent<S75_Main>();
        director.hundredSmilingPrefab = LoadPrefab(HundredSmilingPath);
        director.hundredSurprisedPrefab = LoadPrefab(HundredSurprisedPath);
        director.hundredScaredPrefab = LoadPrefab(HundredScaredPath);
        director.fiveThousandLookingAtHundredPrefab = LoadPrefab(FiveThousandLookingPath);
        director.onePrefab = LoadPrefab(OneRunnerPath);

        director.shadowClip = LoadClip("Assets/Sound/Thunder Crack.mp3");
        director.revealClip = LoadClip("Assets/Resources/sound/tops/top_5000.mp3");
        director.scaredClip = LoadClip("Assets/Sound/fail_down.wav");
        director.splitClip = LoadClip("Assets/Sound/destroy_blocks.mp3");

        EditorUtility.SetDirty(director);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S75] Unity could not save the scene: {ScenePath}");
        }
        AssetDatabase.SaveAssets();

        Debug.Log("[S75] Scene_75 and five scene-local block prefabs were created successfully.");
        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_75 — Hundred sees Five Thousand",
                "Готово. Открой Game View в формате 9:16 и нажми Play — ролик длится ровно 10 секунд.",
                "OK");
        }
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets/Prefabs/Blocks", "Scene75");
        EnsureFolder(AssetRoot, "Textures");
        EnsureFolder(AssetRoot, "Materials");
    }

    private static void EnsureFolder(string parent, string name)
    {
        string fullPath = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void ConfigureFaceTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"[S75] Face texture was not imported: {path}");
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static void CreatePrefabCopy(string sourcePath, string destinationPath, string rootName)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(sourcePath);
        if (root == null)
        {
            Debug.LogError($"[S75] Source prefab was not found: {sourcePath}");
            return;
        }

        try
        {
            root.name = rootName;
            PrefabUtility.SaveAsPrefabAsset(root, destinationPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void CreateFaceVariant(
        string sourcePath,
        string destinationPath,
        string texturePath,
        string materialPath,
        string rootName)
    {
        Texture2D faceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (faceTexture == null)
        {
            Debug.LogError($"[S75] Face texture was not found: {texturePath}");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(sourcePath);
        if (root == null)
        {
            Debug.LogError($"[S75] Source prefab was not found: {sourcePath}");
            return;
        }

        try
        {
            Transform front = root.transform.Find("Quad");
            Renderer frontRenderer = front != null ? front.GetComponent<Renderer>() : null;
            if (frontRenderer == null || frontRenderer.sharedMaterial == null)
            {
                Debug.LogError($"[S75] Front-face renderer 'Quad' was not found in {sourcePath}");
                return;
            }

            Material faceMaterial = CreateOrUpdateFaceMaterial(
                frontRenderer.sharedMaterial,
                faceTexture,
                materialPath);
            frontRenderer.sharedMaterial = faceMaterial;
            root.name = rootName;
            PrefabUtility.SaveAsPrefabAsset(root, destinationPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Material CreateOrUpdateFaceMaterial(
        Material source,
        Texture2D texture,
        string materialPath)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(source);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else
        {
            material.shader = source.shader;
            material.CopyPropertiesFromMaterial(source);
        }

        material.name = Path.GetFileNameWithoutExtension(materialPath);
        material.mainTexture = texture;
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }
        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", texture);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.fieldOfView = 45f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 500f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.56f, 0.84f, 0.96f);
        camera.transform.position = new Vector3(0f, 5f, -20f);
    }

    private static void CreateLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.55f);
    }

    private static void CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "S75 Ground";
        ground.transform.position = new Vector3(5f, -0.02f, 5f);
        ground.transform.localScale = new Vector3(12f, 1f, 12f);
        ground.GetComponent<Renderer>().sharedMaterial = GroundMaterial();
    }

    private static Material GroundMaterial()
    {
        const string path = MaterialRoot + "/S75_Ground.mat";
        Material template = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
            ? UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.defaultMaterial
            : null;
        Shader fallback = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
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

        Color grass = new Color(0.36f, 0.73f, 0.34f);
        material.color = grass;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", grass);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject LoadPrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[S75] Prefab was not created: {path}");
        }
        return prefab;
    }

    private static AudioClip LoadClip(string path)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null && File.Exists(path))
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        return clip;
    }
}
