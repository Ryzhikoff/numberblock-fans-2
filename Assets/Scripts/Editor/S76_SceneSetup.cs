using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Creates all scene-local characters and the complete Scene_76.</summary>
public static class S76_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_76.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene76";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string GeneratedFaceRoot = AssetRoot + "/GeneratedFaces";
    private const string AudioRoot = "Assets/Sound/Scene76";
    private const string FaceOverlayName = "S76 Original Face Overlay";

    private const string OnePath = AssetRoot + "/One_Dream.prefab";
    private const string TenPath = AssetRoot + "/Ten_Dream.prefab";
    private const string HundredPath = AssetRoot + "/Hundred_Dream.prefab";
    private const string ThousandPath = AssetRoot + "/Thousand_Curious.prefab";
    private const string TenThousandPath = AssetRoot + "/TenThousand_Friendly.prefab";
    private const string HundredThousandPath = AssetRoot + "/HundredThousand_Wise.prefab";
    private const string MillionPath = AssetRoot + "/Million_Dream.prefab";
    private const string TenMillionPath = AssetRoot + "/TenMillion_Dream.prefab";
    private const string HundredMillionPath = AssetRoot + "/HundredMillion_Dream.prefab";
    private const string BillionPath = AssetRoot + "/Billion_Dream.prefab";

    private sealed class CharacterVisualSpec
    {
        public readonly string SourcePath;
        public readonly string DestinationPath;
        public readonly string RootName;
        public readonly string FaceMaterialPath;
        public readonly string BodyMaterialPath;
        public readonly string FaceAssetName;

        public CharacterVisualSpec(
            string sourcePath,
            string destinationPath,
            string rootName,
            string faceMaterialPath,
            string bodyMaterialPath,
            string faceAssetName)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            RootName = rootName;
            FaceMaterialPath = faceMaterialPath;
            BodyMaterialPath = bodyMaterialPath;
            FaceAssetName = faceAssetName;
        }
    }

    private static readonly CharacterVisualSpec[] CharacterVisuals =
    {
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/1.prefab", OnePath, "One_Dream",
            "Assets/Materials/Materials/oneFaceFrame.mat",
            "Assets/Materials/Materials/oneAllPartFrame.mat", "One"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/10.prefab", TenPath, "Ten_Dream",
            "Assets/Prefabs/Blocks/materials/Materials/tenFace.mat",
            "Assets/Prefabs/Blocks/materials/Materials/tenBack.mat", "Ten"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/100.prefab", HundredPath, "Hundred_Dream",
            "Assets/Materials/Materials/11.mat",
            "Assets/Prefabs/Blocks/materials/Materials/newMillion 1.mat", "Hundred"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/1000.prefab", ThousandPath, "Thousand_Curious",
            "Assets/Materials/Materials/oneThousandFace.mat",
            "Assets/Materials/Materials/OneThousandTexture 1.mat", "Thousand"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/prefabTenThousand.prefab", TenThousandPath, "TenThousand_Friendly",
            "Assets/Prefabs/Blocks/materials/Materials/10000-face.mat",
            "Assets/Prefabs/Blocks/materials/Materials/10000-back.mat", "TenThousand"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/OneHundredThousand.prefab", HundredThousandPath, "HundredThousand_Wise",
            "Assets/Prefabs/Blocks/materials/Materials/1k-face.mat",
            "Assets/Prefabs/Blocks/materials/Materials/1k-back.mat", "HundredThousand"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/OneMillion.prefab", MillionPath, "Million_Dream",
            "Assets/Materials/Materials/oneMillionFaceTexture.mat",
            "Assets/Materials/Materials/oneMillionTexture.mat", "Million"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/prefabTenMillion.prefab", TenMillionPath, "TenMillion_Dream",
            "Assets/Prefabs/Blocks/materials/Materials/10million face_.mat",
            null, "TenMillion"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/OneHundredMillion.prefab", HundredMillionPath, "HundredMillion_Dream",
            "Assets/Materials/Materials/100million face.mat",
            null, "HundredMillion"),
        new CharacterVisualSpec(
            "Assets/Prefabs/Blocks/prefabBillion.prefab", BillionPath, "Billion_Dream",
            "Assets/Materials/Materials/billionface.mat",
            "Assets/Materials/Materials/billion.mat", "Billion")
    };

    [MenuItem("Tools/Scene_76/Quick Setup Scene")]
    public static void QuickSetup()
    {
        EnsureFolders();
        RemoveObsoleteGeneratedFaces();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        foreach (CharacterVisualSpec visual in CharacterVisuals)
        {
            CreateCharacterVariant(visual);
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        CreateEnvironment();

        GameObject gameManager = new GameObject("GameManager");
        S76_Main director = gameManager.AddComponent<S76_Main>();
        AssignPrefabs(director);
        AssignDialogue(director);
        CreateUi(director);

        EditorUtility.SetDirty(director);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S76] Unity could not save the scene: {ScenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[S76] Scene_76, ten scene-local Numberblock prefabs with their original faces, and 23 voiced lines are ready.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_76 — One Million and One Thousand",
                "Готово. Откройте Scene_76, выберите Game View 16:9 и нажмите Play.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_76/Validate Scene")]
    public static void ValidateScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S76_Main director = UnityEngine.Object.FindAnyObjectByType<S76_Main>();
        if (director == null)
        {
            throw new InvalidOperationException("[S76_VALIDATE] Scene_76 has no S76_Main director.");
        }

        foreach (CharacterVisualSpec visual in CharacterVisuals)
        {
            ValidateVisualCopy(LoadPrefab(visual.SourcePath), LoadPrefab(visual.DestinationPath), visual);
        }

        if (director.dialogueClips == null || director.dialogueClips.Length != 23)
        {
            throw new InvalidOperationException("[S76_VALIDATE] Scene must reference exactly 23 dialogue clips.");
        }

        float dialogueDuration = 0f;
        for (int i = 0; i < director.dialogueClips.Length; i++)
        {
            AudioClip clip = director.dialogueClips[i];
            if (clip == null || clip.length < 0.25f)
            {
                throw new InvalidOperationException($"[S76_VALIDATE] Dialogue clip {i + 1:000} is missing or empty.");
            }
            dialogueDuration += clip.length;
        }

        if (Camera.main == null || GameObject.Find("Campfire") == null || GameObject.Find("Moon") == null)
        {
            throw new InvalidOperationException("[S76_VALIDATE] Camera or campsite environment is incomplete.");
        }

        if (director.titleText == null || director.mathText == null || director.speakerText == null ||
            director.subtitleText == null || director.creditText == null || director.fadeCanvasGroup == null)
        {
            throw new InvalidOperationException("[S76_VALIDATE] One or more UI references are missing.");
        }

        Debug.Log($"[S76_VALIDATE] PASS — 10 prefabs preserve their original faces/materials, 23 clips ({dialogueDuration:F1}s), camera, campsite, and UI are valid.");
    }

    [MenuItem("Tools/Scene_76/Capture Preview")]
    public static void CapturePreview()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Camera camera = Camera.main;
        if (camera == null)
        {
            throw new InvalidOperationException("[S76_PREVIEW] Main Camera is missing.");
        }

        GameObject thousand = SpawnPreview(ThousandPath, new Vector3(-4.4f, 0f, 2.8f), 4.4f);
        GameObject tenThousand = SpawnPreview(TenThousandPath, new Vector3(0f, 0f, 3.35f), 4.75f);
        GameObject hundredThousand = SpawnPreview(HundredThousandPath, new Vector3(4.5f, 0f, 2.8f), 5.28f);
        Bounds group = CombinedBounds(thousand, tenThousand, hundredThousand);

        const int width = 1920;
        const int height = 1080;
        camera.aspect = (float)width / height;
        float verticalHalf = group.extents.y * 1.32f;
        float horizontalHalf = group.extents.x * 1.32f / camera.aspect;
        float requiredHalf = Mathf.Max(verticalHalf, horizontalHalf);
        float distance = requiredHalf / Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        camera.transform.position = group.center + Vector3.back * (distance + group.extents.z + 0.8f);
        camera.transform.LookAt(group.center);

        RenderTexture renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        Texture2D preview = new Texture2D(width, height, TextureFormat.RGBA32, false);
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture previousTarget = camera.targetTexture;

        try
        {
            camera.targetTexture = renderTexture;
            camera.Render();
            RenderTexture.active = renderTexture;
            preview.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            preview.Apply();
            File.WriteAllBytes("/private/tmp/scene76-preview.png", preview.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(preview);
            UnityEngine.Object.DestroyImmediate(thousand);
            UnityEngine.Object.DestroyImmediate(tenThousand);
            UnityEngine.Object.DestroyImmediate(hundredThousand);
        }

        Debug.Log("[S76_PREVIEW] Saved /private/tmp/scene76-preview.png");
    }

    public static void PlaySceneInBatch()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Scenes");
        EnsureFolder("Assets/Prefabs/Blocks", "Scene76");
        EnsureFolder(AssetRoot, "Materials");
        EnsureFolder(AssetRoot, "GeneratedFaces");
        EnsureFolder("Assets", "Sound");
        EnsureFolder("Assets/Sound", "Scene76");
    }

    private static void EnsureFolder(string parent, string name)
    {
        string path = parent + "/" + name;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void RemoveObsoleteGeneratedFaces()
    {
        // These assets belonged to the interrupted arbitrary-face implementation.
        // They are intentionally removed so a rebuilt scene cannot regress to them.
        AssetDatabase.DeleteAsset(AssetRoot + "/Textures");
        AssetDatabase.DeleteAsset(MaterialRoot + "/FacePlate.mat");
    }

    private static void CreateCharacterVariant(CharacterVisualSpec visual)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(visual.SourcePath);
        if (root == null)
        {
            throw new FileNotFoundException($"[S76] Source Numberblock prefab is missing: {visual.SourcePath}");
        }

        try
        {
            Material faceMaterial = LoadMaterial(visual.FaceMaterialPath);
            Material bodyMaterial = string.IsNullOrEmpty(visual.BodyMaterialPath)
                ? null
                : LoadMaterial(visual.BodyMaterialPath);
            Renderer sourceFaceRenderer = FindRendererUsingMaterial(root, faceMaterial);
            if (sourceFaceRenderer == null)
            {
                throw new InvalidOperationException(
                    $"[S76] Could not find original face material {visual.FaceMaterialPath} in {visual.SourcePath}.");
            }

            Texture2D originalFaceTexture = faceMaterial.mainTexture as Texture2D;
            if (originalFaceTexture == null)
            {
                throw new InvalidOperationException($"[S76] Original face has no Texture2D: {visual.FaceMaterialPath}");
            }

            Texture2D transparentFaceTexture = originalFaceTexture;
            Renderer overlayRenderer;

            if (bodyMaterial != null)
            {
                Texture2D bodyTexture = bodyMaterial.mainTexture as Texture2D;
                if (bodyTexture == null)
                {
                    throw new InvalidOperationException($"[S76] Body material has no Texture2D: {visual.BodyMaterialPath}");
                }

                string generatedTexturePath = $"{GeneratedFaceRoot}/{visual.FaceAssetName}_OriginalFace.png";
                transparentFaceTexture = CreateTransparentDifferenceTexture(
                    originalFaceTexture,
                    bodyTexture,
                    generatedTexturePath);

                Bounds originalBounds = BoundsOf(root);
                GameObject overlay = UnityEngine.Object.Instantiate(
                    sourceFaceRenderer.gameObject,
                    sourceFaceRenderer.transform.parent,
                    false);
                overlay.name = FaceOverlayName;
                overlayRenderer = overlay.GetComponent<Renderer>();

                ReplaceMaterial(sourceFaceRenderer, faceMaterial, bodyMaterial);
                OffsetOverlayFromBody(overlay.transform, sourceFaceRenderer.bounds, originalBounds);

                foreach (Collider collider in overlay.GetComponents<Collider>())
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }
            else
            {
                // Ten Million and One Hundred Million already use a dedicated transparent face quad.
                sourceFaceRenderer.gameObject.name = FaceOverlayName;
                overlayRenderer = sourceFaceRenderer;
            }

            Material transparentMaterial = CreateTransparentFaceMaterial(
                $"{MaterialRoot}/{visual.FaceAssetName}_OriginalFaceOverlay.mat",
                transparentFaceTexture);
            overlayRenderer.sharedMaterial = transparentMaterial;
            overlayRenderer.shadowCastingMode = ShadowCastingMode.Off;
            overlayRenderer.receiveShadows = false;
            overlayRenderer.sortingOrder = 10;

            root.name = visual.RootName;
            PrefabUtility.SaveAsPrefabAsset(root, visual.DestinationPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ValidateVisualCopy(
        GameObject source,
        GameObject copy,
        CharacterVisualSpec visual)
    {
        Material originalFaceMaterial = LoadMaterial(visual.FaceMaterialPath);
        Renderer sourceFaceRenderer = FindRendererUsingMaterial(source, originalFaceMaterial);
        if (sourceFaceRenderer == null)
        {
            throw new InvalidOperationException($"[S76_VALIDATE] Original face is missing in {visual.SourcePath}.");
        }

        Renderer[] sourceRenderers = source.GetComponentsInChildren<Renderer>();
        Renderer[] copyRenderers = copy.GetComponentsInChildren<Renderer>();
        int expectedRendererCount = sourceRenderers.Length + (string.IsNullOrEmpty(visual.BodyMaterialPath) ? 0 : 1);
        if (copyRenderers.Length != expectedRendererCount)
        {
            throw new InvalidOperationException($"[S76_VALIDATE] Unexpected renderer count in {visual.DestinationPath}.");
        }

        Renderer overlayRenderer = null;
        foreach (Renderer renderer in copyRenderers)
        {
            if (renderer.name == FaceOverlayName)
            {
                if (overlayRenderer != null)
                {
                    throw new InvalidOperationException($"[S76_VALIDATE] Multiple face overlays in {visual.DestinationPath}.");
                }
                overlayRenderer = renderer;
            }
        }

        if (overlayRenderer == null || !IsTransparentMaterial(overlayRenderer.sharedMaterial))
        {
            throw new InvalidOperationException($"[S76_VALIDATE] Transparent face overlay is missing in {visual.DestinationPath}.");
        }

        Vector3 sourceFaceCenter = source.transform.InverseTransformPoint(sourceFaceRenderer.bounds.center);
        Vector3 overlayCenter = copy.transform.InverseTransformPoint(overlayRenderer.bounds.center);
        if (Mathf.Abs(sourceFaceCenter.x - overlayCenter.x) > 0.001f ||
            Mathf.Abs(sourceFaceCenter.y - overlayCenter.y) > 0.001f ||
            sourceFaceRenderer.transform.localRotation != overlayRenderer.transform.localRotation ||
            sourceFaceRenderer.transform.localScale != overlayRenderer.transform.localScale)
        {
            throw new InvalidOperationException(
                $"[S76_VALIDATE] Original face size or vertical placement changed in {visual.DestinationPath}.");
        }

        string overlayTexturePath = AssetDatabase.GetAssetPath(overlayRenderer.sharedMaterial.mainTexture);
        string expectedTexturePath = string.IsNullOrEmpty(visual.BodyMaterialPath)
            ? AssetDatabase.GetAssetPath(originalFaceMaterial.mainTexture)
            : $"{GeneratedFaceRoot}/{visual.FaceAssetName}_OriginalFace.png";
        if (overlayTexturePath != expectedTexturePath)
        {
            throw new InvalidOperationException($"[S76_VALIDATE] Face is not based on the original texture in {visual.DestinationPath}.");
        }

        if (!string.IsNullOrEmpty(visual.BodyMaterialPath))
        {
            Material bodyMaterial = LoadMaterial(visual.BodyMaterialPath);
            Renderer copiedBodyFace = FindMatchingRenderer(copy, source, sourceFaceRenderer);
            if (copiedBodyFace == null || copiedBodyFace.sharedMaterial != bodyMaterial)
            {
                throw new InvalidOperationException(
                    $"[S76_VALIDATE] Original body texture is not visible below the face in {visual.DestinationPath}.");
            }
        }

        foreach (Transform child in copy.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "S76 Generated Face")
            {
                throw new InvalidOperationException($"[S76_VALIDATE] Arbitrary generated face remains in {visual.DestinationPath}.");
            }
        }
    }

    private static Material LoadMaterial(string assetPath)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (material == null)
        {
            throw new FileNotFoundException($"[S76] Material is missing: {assetPath}");
        }
        return material;
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
            throw new InvalidOperationException($"[S76] Face material is not assigned to {renderer.name}.");
        }
        renderer.sharedMaterials = materials;
    }

    private static void OffsetOverlayFromBody(Transform overlay, Bounds faceBounds, Bounds bodyBounds)
    {
        Vector3 outward = faceBounds.center - bodyBounds.center;
        if (outward.sqrMagnitude < 0.000001f)
        {
            outward = -overlay.forward;
        }

        float offset = Mathf.Max(0.001f, bodyBounds.size.z * 0.001f);
        overlay.position += outward.normalized * offset;
    }

    private static Texture2D CreateTransparentDifferenceTexture(
        Texture2D originalFace,
        Texture2D originalBody,
        string destinationPath)
    {
        int width = originalFace.width;
        int height = originalFace.height;
        Texture2D faceCopy = ReadTexture(originalFace, width, height);
        Texture2D bodyCopy = ReadTexture(originalBody, width, height);
        Texture2D transparentFace = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color32[] facePixels = faceCopy.GetPixels32();
        Color32[] bodyPixels = bodyCopy.GetPixels32();
        Color32[] resultPixels = new Color32[facePixels.Length];

        for (int i = 0; i < facePixels.Length; i++)
        {
            Color32 face = facePixels[i];
            Color32 body = bodyPixels[i];
            float redDifference = Mathf.Abs(face.r - body.r) / 255f;
            float greenDifference = Mathf.Abs(face.g - body.g) / 255f;
            float blueDifference = Mathf.Abs(face.b - body.b) / 255f;
            float difference = Mathf.Max(redDifference, Mathf.Max(greenDifference, blueDifference));
            float alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.035f, 0.11f, difference));
            resultPixels[i] = new Color32(face.r, face.g, face.b, (byte)Mathf.RoundToInt(alpha * face.a));
        }

        transparentFace.SetPixels32(resultPixels);
        transparentFace.Apply(false, false);

        string absolutePath = Path.GetFullPath(destinationPath);
        File.WriteAllBytes(absolutePath, transparentFace.EncodeToPNG());

        UnityEngine.Object.DestroyImmediate(faceCopy);
        UnityEngine.Object.DestroyImmediate(bodyCopy);
        UnityEngine.Object.DestroyImmediate(transparentFace);

        AssetDatabase.ImportAsset(destinationPath, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(destinationPath) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException($"[S76] Could not import transparent face: {destinationPath}");
        }

        importer.textureType = TextureImporterType.Default;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.sRGBTexture = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();

        Texture2D imported = AssetDatabase.LoadAssetAtPath<Texture2D>(destinationPath);
        if (imported == null)
        {
            throw new InvalidOperationException($"[S76] Transparent face was not imported: {destinationPath}");
        }
        return imported;
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

    private static Material CreateTransparentFaceMaterial(string assetPath, Texture2D texture)
    {
        bool usesScriptableRenderPipeline = GraphicsSettings.currentRenderPipeline != null;
        Shader shader = usesScriptableRenderPipeline
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        shader ??= Shader.Find("Sprites/Default");
        if (shader == null)
        {
            throw new InvalidOperationException("[S76] No supported transparent shader is available.");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, assetPath);
        }
        else
        {
            material.shader = shader;
        }

        material.name = Path.GetFileNameWithoutExtension(assetPath);
        material.mainTexture = texture;
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", Color.white);
        }
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", Color.white);
        }

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

    private static bool IsTransparentMaterial(Material material)
    {
        return material != null &&
            material.renderQueue >= (int)RenderQueue.Transparent &&
            material.GetTag("RenderType", false) == "Transparent";
    }

    private static Renderer FindMatchingRenderer(GameObject copy, GameObject source, Renderer sourceRenderer)
    {
        string relativePath = AnimationUtility.CalculateTransformPath(sourceRenderer.transform, source.transform);
        Transform copyTransform = string.IsNullOrEmpty(relativePath)
            ? copy.transform
            : copy.transform.Find(relativePath);
        return copyTransform != null ? copyTransform.GetComponent<Renderer>() : null;
    }

    private static void AssignPrefabs(S76_Main director)
    {
        director.onePrefab = LoadPrefab(OnePath);
        director.tenPrefab = LoadPrefab(TenPath);
        director.hundredPrefab = LoadPrefab(HundredPath);
        director.thousandPrefab = LoadPrefab(ThousandPath);
        director.tenThousandPrefab = LoadPrefab(TenThousandPath);
        director.hundredThousandPrefab = LoadPrefab(HundredThousandPath);
        director.millionPrefab = LoadPrefab(MillionPath);
        director.tenMillionPrefab = LoadPrefab(TenMillionPath);
        director.hundredMillionPrefab = LoadPrefab(HundredMillionPath);
        director.billionPrefab = LoadPrefab(BillionPath);
    }

    private static void AssignDialogue(S76_Main director)
    {
        director.dialogueClips = new AudioClip[23];
        for (int i = 0; i < director.dialogueClips.Length; i++)
        {
            string path = $"{AudioRoot}/{i + 1:000}.wav";
            director.dialogueClips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (director.dialogueClips[i] == null)
            {
                Debug.LogWarning($"[S76] Dialogue clip was not imported: {path}");
            }
        }
    }

    private static GameObject LoadPrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            throw new FileNotFoundException($"[S76] Generated prefab is missing: {path}");
        }
        return prefab;
    }

    private static GameObject SpawnPreview(string prefabPath, Vector3 groundPosition, float targetHeight)
    {
        GameObject instance = PrefabUtility.InstantiatePrefab(LoadPrefab(prefabPath)) as GameObject;
        if (instance == null)
        {
            throw new InvalidOperationException($"[S76_PREVIEW] Could not instantiate {prefabPath}.");
        }

        Bounds initialBounds = BoundsOf(instance);
        instance.transform.localScale *= targetHeight / Mathf.Max(0.001f, initialBounds.size.y);
        instance.transform.position = groundPosition;
        Bounds groundedBounds = BoundsOf(instance);
        instance.transform.position += Vector3.up * (groundPosition.y - groundedBounds.min.y);
        return instance;
    }

    private static Bounds CombinedBounds(params GameObject[] targets)
    {
        Bounds bounds = BoundsOf(targets[0]);
        for (int i = 1; i < targets.Length; i++)
        {
            bounds.Encapsulate(BoundsOf(targets[i]));
        }
        return bounds;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.fieldOfView = 40f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 250f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.045f, 0.13f);
        camera.transform.position = new Vector3(0f, 5f, -18f);
        camera.transform.LookAt(new Vector3(0f, 2.3f, 2.5f));
    }

    private static void CreateLighting()
    {
        GameObject moonLightObject = new GameObject("Moon Light");
        Light moonLight = moonLightObject.AddComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.intensity = 0.72f;
        moonLight.color = new Color(0.55f, 0.66f, 1f);
        moonLight.shadows = LightShadows.Soft;
        moonLightObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

        RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.27f);
    }

    private static void CreateEnvironment()
    {
        Material groundMaterial = CreateOrUpdateMaterial(
            MaterialRoot + "/NightGround.mat",
            new Color(0.035f, 0.09f, 0.075f),
            Color.black);
        Material logMaterial = CreateOrUpdateMaterial(
            MaterialRoot + "/CampfireLog.mat",
            new Color(0.18f, 0.055f, 0.02f),
            Color.black);
        Material fireMaterial = CreateOrUpdateMaterial(
            MaterialRoot + "/CampfireGlow.mat",
            new Color(1f, 0.24f, 0.025f),
            new Color(4f, 0.45f, 0.025f));
        Material starMaterial = CreateOrUpdateMaterial(
            MaterialRoot + "/Stars.mat",
            new Color(0.85f, 0.92f, 1f),
            new Color(1.2f, 1.5f, 2.3f));
        Material moonMaterial = CreateOrUpdateMaterial(
            MaterialRoot + "/Moon.mat",
            new Color(0.88f, 0.91f, 1f),
            new Color(0.32f, 0.38f, 0.62f));
        Material teddyMaterial = CreateOrUpdateMaterial(
            MaterialRoot + "/Teddy.mat",
            new Color(0.38f, 0.16f, 0.055f),
            Color.black);

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "S76 Night Ground";
        ground.transform.position = new Vector3(0f, -0.03f, 3f);
        ground.transform.localScale = new Vector3(11f, 1f, 8f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

        CreateCampfire(logMaterial, fireMaterial);
        CreateStarField(starMaterial, moonMaterial);
        CreateTeddy(teddyMaterial);
    }

    private static void CreateCampfire(Material logMaterial, Material fireMaterial)
    {
        GameObject root = new GameObject("Campfire");
        CreateLog(root.transform, logMaterial, 28f);
        CreateLog(root.transform, logMaterial, -28f);

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Campfire Flame";
        flame.transform.SetParent(root.transform, false);
        flame.transform.position = new Vector3(0f, 0.72f, 0.4f);
        flame.transform.localScale = new Vector3(0.68f, 1.3f, 0.55f);
        flame.GetComponent<Renderer>().sharedMaterial = fireMaterial;
        UnityEngine.Object.DestroyImmediate(flame.GetComponent<Collider>());

        GameObject fireLightObject = new GameObject("Campfire Light");
        fireLightObject.transform.SetParent(root.transform, false);
        fireLightObject.transform.position = new Vector3(0f, 1.2f, 0f);
        Light fireLight = fireLightObject.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.range = 14f;
        fireLight.intensity = 2.3f;
        fireLight.color = new Color(1f, 0.36f, 0.08f);
        fireLight.shadows = LightShadows.Soft;
    }

    private static void CreateLog(Transform parent, Material material, float yRotation)
    {
        GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        log.name = "Campfire Log";
        log.transform.SetParent(parent, false);
        log.transform.position = new Vector3(0f, 0.18f, 0.55f);
        log.transform.rotation = Quaternion.Euler(0f, yRotation, 90f);
        log.transform.localScale = new Vector3(0.2f, 1.15f, 0.2f);
        log.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(log.GetComponent<Collider>());
    }

    private static void CreateStarField(Material starMaterial, Material moonMaterial)
    {
        GameObject stars = new GameObject("Star Field");
        UnityEngine.Random.State previousState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(76001);

        for (int i = 0; i < 52; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            star.name = $"Star {i + 1:00}";
            star.transform.SetParent(stars.transform, false);
            star.transform.position = new Vector3(
                UnityEngine.Random.Range(-14f, 14f),
                UnityEngine.Random.Range(5.2f, 14.5f),
                UnityEngine.Random.Range(8f, 16f));
            float size = UnityEngine.Random.Range(0.045f, 0.13f);
            star.transform.localScale = Vector3.one * size;
            star.GetComponent<Renderer>().sharedMaterial = starMaterial;
            UnityEngine.Object.DestroyImmediate(star.GetComponent<Collider>());
        }

        UnityEngine.Random.state = previousState;

        GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        moon.name = "Moon";
        moon.transform.position = new Vector3(8.2f, 10.8f, 12f);
        moon.transform.localScale = Vector3.one * 1.55f;
        moon.GetComponent<Renderer>().sharedMaterial = moonMaterial;
        UnityEngine.Object.DestroyImmediate(moon.GetComponent<Collider>());
    }

    private static void CreateTeddy(Material material)
    {
        GameObject teddy = new GameObject("One Thousand's Teddy");
        CreateTeddyPart(teddy.transform, "Body", new Vector3(-3.25f, 0.43f, 1.05f), new Vector3(0.62f, 0.78f, 0.42f), material);
        CreateTeddyPart(teddy.transform, "Head", new Vector3(-3.25f, 1.05f, 1.04f), new Vector3(0.56f, 0.56f, 0.42f), material);
        CreateTeddyPart(teddy.transform, "Left Ear", new Vector3(-3.53f, 1.3f, 1.04f), Vector3.one * 0.22f, material);
        CreateTeddyPart(teddy.transform, "Right Ear", new Vector3(-2.97f, 1.3f, 1.04f), Vector3.one * 0.22f, material);
    }

    private static void CreateTeddyPart(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.position = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
    }

    private static void CreateUi(S76_Main director)
    {
        GameObject canvasObject = new GameObject("S76 UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject subtitlePanel = new GameObject("Subtitle Panel", typeof(RectTransform), typeof(Image));
        subtitlePanel.transform.SetParent(canvasObject.transform, false);
        Image panelImage = subtitlePanel.GetComponent<Image>();
        panelImage.color = new Color(0.015f, 0.018f, 0.055f, 0.82f);
        panelImage.raycastTarget = false;
        SetRect(panelImage.rectTransform, new Vector2(0.035f, 0.025f), new Vector2(0.965f, 0.235f));

        director.speakerText = CreateText(
            canvasObject.transform,
            "Speaker",
            new Vector2(0.065f, 0.164f),
            new Vector2(0.935f, 0.225f),
            32f,
            new Color(1f, 0.72f, 0.24f),
            FontStyles.Bold,
            TextAlignmentOptions.Center);

        director.subtitleText = CreateText(
            canvasObject.transform,
            "Subtitle",
            new Vector2(0.07f, 0.045f),
            new Vector2(0.93f, 0.17f),
            38f,
            Color.white,
            FontStyles.Normal,
            TextAlignmentOptions.Center);

        director.mathText = CreateText(
            canvasObject.transform,
            "Math",
            new Vector2(0.06f, 0.82f),
            new Vector2(0.94f, 0.95f),
            55f,
            new Color(0.92f, 0.96f, 1f),
            FontStyles.Bold,
            TextAlignmentOptions.Center);
        director.mathText.outlineColor = new Color(0.02f, 0.03f, 0.12f);
        director.mathText.outlineWidth = 0.25f;

        director.titleText = CreateText(
            canvasObject.transform,
            "Title",
            new Vector2(0.08f, 0.34f),
            new Vector2(0.92f, 0.76f),
            86f,
            new Color(1f, 0.82f, 0.32f),
            FontStyles.Bold,
            TextAlignmentOptions.Center);
        director.titleText.outlineColor = new Color(0.04f, 0.025f, 0.14f);
        director.titleText.outlineWidth = 0.3f;
        director.titleText.gameObject.SetActive(false);

        director.creditText = CreateText(
            canvasObject.transform,
            "Credits",
            new Vector2(0.12f, 0.32f),
            new Vector2(0.88f, 0.56f),
            42f,
            new Color(0.88f, 0.92f, 1f),
            FontStyles.Normal,
            TextAlignmentOptions.Center);
        director.creditText.gameObject.SetActive(false);

        GameObject fadeObject = new GameObject("Fade", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        fadeObject.transform.SetParent(canvasObject.transform, false);
        Image fadeImage = fadeObject.GetComponent<Image>();
        fadeImage.color = Color.black;
        fadeImage.raycastTarget = false;
        SetRect(fadeImage.rectTransform, Vector2.zero, Vector2.one);
        director.fadeCanvasGroup = fadeObject.GetComponent<CanvasGroup>();
        director.fadeCanvasGroup.alpha = 1f;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize,
        Color color,
        FontStyles style,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.text = string.Empty;
        SetRect(text.rectTransform, anchorMin, anchorMax);
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Material CreateOrUpdateMaterial(string path, Color baseColor, Color emissionColor)
    {
        bool usesScriptableRenderPipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        Shader shader = usesScriptableRenderPipeline
            ? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")
            : Shader.Find("Standard");
        if (shader == null)
        {
            throw new InvalidOperationException("[S76] No supported Lit shader is available.");
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
        material.color = baseColor;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", baseColor);
        }
        if (emissionColor.maxColorComponent > 0.001f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Bounds BoundsOf(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
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
}
