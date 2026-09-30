using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the vertical Scene 80 collision short.</summary>
public static class S80_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_80.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene80";
    private const string TextureRoot = AssetRoot + "/FaceTextures";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string ThousandPrefabPath = AssetRoot + "/Thousand_Collision.prefab";

    private const string OnePath = "Assets/Prefabs/Blocks/1.prefab";
    private const string ThousandSourcePath = "Assets/Prefabs/Blocks/1000.prefab";
    private const string OriginalFaceMaterialPath =
        "Assets/Materials/Materials/oneThousandFace.mat";
    private const string CleanBodyMaterialPath =
        "Assets/Materials/Materials/OneThousandTexture 1.mat";
    private const string SurprisedTexturePath = TextureRoot + "/Thousand_Surprised.png";
    private const string ExtremeTexturePath = TextureRoot + "/Thousand_ExtremeSurprise.png";
    private const string DeterminedTexturePath = TextureRoot + "/Thousand_Determined.png";
    private const string SurprisedMaterialPath = MaterialRoot + "/Thousand_Surprised_Transparent.mat";
    private const string ExtremeMaterialPath = MaterialRoot + "/Thousand_ExtremeSurprise_Transparent.mat";
    private const string DeterminedMaterialPath = MaterialRoot + "/Thousand_Determined_Transparent.mat";

    private const string WhooshPath = "Assets/Sound/zvuk-priblijeniya.mp3";
    private const string ImpactPath = "Assets/Sound/collCube.wav";
    private const string ImpactAccentPath = "Assets/Sound/boom_metal.wav";
    private const string FinalImpactPath = "Assets/Sound/Big Explosion Cut Off.mp3";

    private static readonly float[] CaptureTimes =
    {
        1.2f,
        2.05f,
        5.05f,
        8.35f,
        9.2f,
        15.3f
    };

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

    [MenuItem("Tools/Scene_80/Quick Setup Collision Short")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        GenerateExpressionTextures();
        ConfigureFaceTexture(SurprisedTexturePath);
        ConfigureFaceTexture(ExtremeTexturePath);
        ConfigureFaceTexture(DeterminedTexturePath);
        Material surprisedMaterial = CreateTransparentMaterial(
            SurprisedMaterialPath,
            LoadAsset<Texture2D>(SurprisedTexturePath));
        Material extremeMaterial = CreateTransparentMaterial(
            ExtremeMaterialPath,
            LoadAsset<Texture2D>(ExtremeTexturePath));
        Material determinedMaterial = CreateTransparentMaterial(
            DeterminedMaterialPath,
            LoadAsset<Texture2D>(DeterminedTexturePath));
        CreateThousandVariant(surprisedMaterial, extremeMaterial, determinedMaterial);

        Material groundMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S80_Ground.mat",
            new Color(0.31f, 0.73f, 0.31f));
        Material hillMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S80_Hills.mat",
            new Color(0.19f, 0.58f, 0.28f));
        Material cloudMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S80_Clouds.mat",
            new Color(0.97f, 0.99f, 1f));
        Material dustMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S80_Dust.mat",
            new Color(1f, 0.78f, 0.25f));
        Material flashMaterial = CreateFlashMaterial(MaterialRoot + "/S80_Flash.mat");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        BoxCollider groundCollider = CreateEnvironment(groundMaterial, hillMaterial, cloudMaterial);

        GameObject burstObject = new GameObject("2,000 Real Ones — GPU Collision Burst");
        S80_UnitBurst burst = burstObject.AddComponent<S80_UnitBurst>();
        burst.onePrefab = LoadAsset<GameObject>(OnePath);
        burst.groundCollider = groundCollider;

        GameObject gameManager = new GameObject("GameManager — Three Collision Timeline");
        S80_Main director = gameManager.AddComponent<S80_Main>();
        director.onePrefab = LoadAsset<GameObject>(OnePath);
        director.thousandPrefab = LoadAsset<GameObject>(ThousandPrefabPath);
        director.thousandScale = 0.29f;
        director.offscreenDistance = 6.35f;
        director.insideRetreatExtra = 0.78f;
        director.dustMaterial = dustMaterial;
        director.flashMaterial = flashMaterial;
        director.unitBurst = burst;
        director.whooshClip = LoadAsset<AudioClip>(WhooshPath);
        director.impactClip = LoadAsset<AudioClip>(ImpactPath);
        director.impactAccentClip = LoadAsset<AudioClip>(ImpactAccentPath);
        director.finalImpactClip = LoadAsset<AudioClip>(FinalImpactPath);
        EditorUtility.SetDirty(burst);
        EditorUtility.SetDirty(director);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S80] Unity could not save the scene: {ScenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "[S80] Scene_80 created: three escalating collisions, two custom surprise " +
            "expressions, vertical framing, and exactly 2,000 instanced Ones.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_80 — Thousand collision short",
                "Готово. Выберите Game View 9:16 и нажмите Play. Ролик длится 15,5 секунды.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_80/Validate Collision Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S80_VALIDATE] Scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S80_Main director = UnityEngine.Object.FindAnyObjectByType<S80_Main>();
        S80_UnitBurst burst = UnityEngine.Object.FindAnyObjectByType<S80_UnitBurst>();
        if (director == null || burst == null)
        {
            throw new InvalidOperationException("[S80_VALIDATE] Scene director or unit burst is missing.");
        }

        if (AssetDatabase.GetAssetPath(director.onePrefab) != OnePath ||
            AssetDatabase.GetAssetPath(director.thousandPrefab) != ThousandPrefabPath ||
            director.unitBurst != burst || burst.onePrefab != director.onePrefab)
        {
            throw new InvalidOperationException("[S80_VALIDATE] Numberblock references are incomplete.");
        }

        if (!(burst.groundCollider is BoxCollider) || director.dustMaterial == null ||
            director.flashMaterial == null || director.whooshClip == null ||
            director.impactClip == null || director.impactAccentClip == null ||
            director.finalImpactClip == null)
        {
            throw new InvalidOperationException("[S80_VALIDATE] Environment, effects, or audio is incomplete.");
        }

        Camera camera = Camera.main;
        if (camera == null || camera.name != "Main Camera — Vertical 9x16" ||
            Mathf.Abs(camera.fieldOfView - 44f) > 0.01f)
        {
            throw new InvalidOperationException("[S80_VALIDATE] Vertical 9:16 camera is misconfigured.");
        }

        if (S80_UnitBurst.TotalUnitCount != 2000 ||
            S80_UnitBurst.UnitsPerThousand != 1000 ||
            S80_UnitBurst.GridSize != 10)
        {
            throw new InvalidOperationException("[S80_VALIDATE] Each Thousand must contain 10 x 10 x 10 Ones.");
        }

        if (!(S80_Main.FirstImpactTime < S80_Main.FirstRetreatStartTime &&
              S80_Main.FirstRetreatEndTime < S80_Main.SecondChargeStartTime &&
              S80_Main.SecondImpactTime < S80_Main.SecondRetreatStartTime &&
              S80_Main.SecondRetreatEndTime < S80_Main.FinalChargeStartTime &&
              S80_Main.FinalChargeStartTime < S80_Main.FinalImpactTime &&
              S80_Main.FinalImpactTime < S80_Main.SequenceDuration))
        {
            throw new InvalidOperationException("[S80_VALIDATE] Timeline beats overlap or are out of order.");
        }

        ValidateThousandVariant();
        Debug.Log(
            "[S80_VALIDATE] PASS — vertical 9:16, three ordered collisions, two escalating " +
            "expressions, exact 10 x 10 x 10 composition per Thousand, and 2,000 rendered Ones.");
    }

    /// <summary>Runs the complete short at 2x and captures all important story beats.</summary>
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S80_PLAY] This entry point is batch-mode only.");
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
            Debug.Log("[S80_PLAY] Entered Play Mode at 2x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;

            if (playModePassed)
            {
                Debug.Log($"[S80_PLAY] PASS — complete short and all {CaptureTimes.Length} keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S80_PLAY] FAIL — the complete short did not reach its ending.");
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

        S80_Main director = UnityEngine.Object.FindAnyObjectByType<S80_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene80-{i + 1:00}.png");
                }
            }

            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() &&
                    director.CollisionCount == 3 &&
                    director.ExpressionLevel == 3 &&
                    director.BurstTriggered &&
                    director.unitBurst.Active &&
                    director.unitBurst.RenderedUnitCount == S80_UnitBurst.TotalUnitCount &&
                    director.unitBurst.SettledUnitCount == S80_UnitBurst.TotalUnitCount;
                EditorApplication.isPlaying = false;
                return;
            }
        }

        if (EditorApplication.timeSinceStartup - playModeStartTime > 22d)
        {
            Debug.LogError("[S80_PLAY] Timed out after 22 real seconds.");
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
            throw new InvalidOperationException("[S80_PLAY] Main Camera disappeared.");
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

        Debug.Log($"[S80_PLAY] Captured {path}");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene80");
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

    private static void GenerateExpressionTextures()
    {
        Material originalFaceMaterial = LoadAsset<Material>(OriginalFaceMaterialPath);
        Material cleanBodyMaterial = LoadAsset<Material>(CleanBodyMaterialPath);
        Texture2D faceTexture = originalFaceMaterial.mainTexture as Texture2D;
        Texture2D bodyTexture = cleanBodyMaterial.mainTexture as Texture2D;
        if (faceTexture == null || bodyTexture == null)
        {
            throw new InvalidOperationException("[S80] Original Thousand face/body textures are missing.");
        }

        Texture2D faceCopy = ReadTexture(faceTexture, faceTexture.width, faceTexture.height);
        Texture2D bodyCopy = ReadTexture(bodyTexture, faceTexture.width, faceTexture.height);
        try
        {
            PixelBounds target = FindDifferenceBounds(
                faceCopy.GetPixels32(),
                bodyCopy.GetPixels32(),
                faceCopy.width,
                faceCopy.height,
                0.2f);
            if (!target.IsValid || target.Width < faceCopy.width * 0.12f ||
                target.Height < faceCopy.height * 0.12f)
            {
                target = new PixelBounds
                {
                    MinX = Mathf.RoundToInt(faceCopy.width * 0.18f),
                    MinY = Mathf.RoundToInt(faceCopy.height * 0.16f),
                    MaxX = Mathf.RoundToInt(faceCopy.width * 0.82f),
                    MaxY = Mathf.RoundToInt(faceCopy.height * 0.86f)
                };
            }

            WriteExpressionTexture(SurprisedTexturePath, faceCopy.width, faceCopy.height, target, false);
            WriteExpressionTexture(ExtremeTexturePath, faceCopy.width, faceCopy.height, target, true);
            WriteDeterminedExpressionTexture(
                DeterminedTexturePath,
                faceCopy.width,
                faceCopy.height,
                target);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(faceCopy);
            UnityEngine.Object.DestroyImmediate(bodyCopy);
        }

        AssetDatabase.ImportAsset(SurprisedTexturePath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(ExtremeTexturePath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(DeterminedTexturePath, ImportAssetOptions.ForceSynchronousImport);
    }

    private static void WriteExpressionTexture(
        string path,
        int width,
        int height,
        PixelBounds target,
        bool extreme)
    {
        Color[] pixels = new Color[width * height];
        float x = target.MinX;
        float y = target.MinY;
        float w = target.Width;
        float h = target.Height;
        Color black = new Color(0.025f, 0.018f, 0.03f, 1f);
        Color white = new Color(1f, 0.99f, 0.98f, 1f);
        Color mouth = extreme
            ? new Color(0.95f, 0.2f, 0.28f, 1f)
            : new Color(0.96f, 0.34f, 0.38f, 1f);

        float eyeRadius = w * (extreme ? 0.49f : 0.46f);
        float eyeRadiusX = eyeRadius;
        float eyeRadiusY = eyeRadius;
        float eyeY = y + h * (extreme ? 0.66f : 0.65f);
        float eyeX = x + w * 0.5f;

        DrawEllipse(pixels, width, height, eyeX, eyeY, eyeRadiusX, eyeRadiusY, black);
        DrawEllipse(
            pixels, width, height, eyeX, eyeY,
            eyeRadiusX * 0.78f, eyeRadiusY * 0.79f, white);

        float pupilRadius = eyeRadiusX * (extreme ? 0.22f : 0.31f);
        float pupilY = eyeY - eyeRadiusY * (extreme ? 0.04f : 0.08f);
        DrawEllipse(pixels, width, height, eyeX, pupilY, pupilRadius, pupilRadius * 1.15f, black);
        DrawEllipse(
            pixels, width, height,
            eyeX - pupilRadius * 0.22f, pupilY + pupilRadius * 0.28f,
            pupilRadius * 0.22f, pupilRadius * 0.24f, white);

        float browY = y + h * (extreme ? 0.925f : 0.89f);
        float browRadius = Mathf.Max(2f, w * 0.025f);
        DrawCapsule(
            pixels, width, height,
            x + w * 0.25f, browY - h * 0.02f,
            x + w * 0.48f, browY + h * (extreme ? 0.055f : 0.035f),
            browRadius, black);
        DrawCapsule(
            pixels, width, height,
            x + w * 0.52f, browY + h * (extreme ? 0.055f : 0.035f),
            x + w * 0.75f, browY - h * 0.02f,
            browRadius, black);

        float mouthY = y + h * (extreme ? 0.15f : 0.16f);
        float mouthRadiusX = w * (extreme ? 0.165f : 0.125f);
        float mouthRadiusY = h * (extreme ? 0.205f : 0.15f);
        DrawEllipse(pixels, width, height, x + w * 0.5f, mouthY, mouthRadiusX, mouthRadiusY, black);
        DrawEllipse(
            pixels, width, height,
            x + w * 0.5f, mouthY - mouthRadiusY * 0.31f,
            mouthRadiusX * 0.68f, mouthRadiusY * 0.38f, mouth);

        if (extreme)
        {
            Color sweatOutline = new Color(0.04f, 0.32f, 0.58f, 1f);
            Color sweat = new Color(0.3f, 0.82f, 1f, 1f);
            float sweatX = x + w * 1.08f;
            float sweatY = y + h * 0.68f;
            DrawEllipse(pixels, width, height, sweatX, sweatY, w * 0.055f, h * 0.11f, sweatOutline);
            DrawEllipse(pixels, width, height, sweatX, sweatY, w * 0.038f, h * 0.082f, sweat);
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static void WriteDeterminedExpressionTexture(
        string path,
        int width,
        int height,
        PixelBounds target)
    {
        Color[] pixels = new Color[width * height];
        float x = target.MinX;
        float y = target.MinY;
        float w = target.Width;
        float h = target.Height;
        float eyeX = x + w * 0.5f;
        float eyeY = y + h * 0.66f;
        float eyeRadius = w * 0.49f;

        Color outline = new Color(0.25f, 0.025f, 0.035f, 1f);
        Color black = new Color(0.018f, 0.012f, 0.018f, 1f);
        Color white = new Color(1f, 0.99f, 0.98f, 1f);
        Color mouthRed = new Color(0.63f, 0.035f, 0.055f, 1f);

        // Keep the canonical Thousand anatomy: one large, perfectly round eye.
        DrawEllipse(pixels, width, height, eyeX, eyeY, eyeRadius, eyeRadius, outline);
        DrawEllipse(
            pixels, width, height,
            eyeX, eyeY,
            eyeRadius * 0.82f, eyeRadius * 0.82f,
            white);

        float pupilRadius = eyeRadius * 0.3f;
        DrawEllipse(
            pixels, width, height,
            eyeX + eyeRadius * 0.12f,
            eyeY - eyeRadius * 0.08f,
            pupilRadius,
            pupilRadius * 1.08f,
            black);
        DrawEllipse(
            pixels, width, height,
            eyeX + eyeRadius * 0.03f,
            eyeY + pupilRadius * 0.16f,
            pupilRadius * 0.22f,
            pupilRadius * 0.22f,
            white);

        // Scene 77 character: a heavy diagonal brow turns the round eye into
        // the same focused, narrowed obstacle-course expression.
        DrawCapsule(
            pixels, width, height,
            x + w * 0.1f,
            y + h * 0.95f,
            x + w * 0.91f,
            y + h * 0.76f,
            w * 0.075f,
            outline);

        // Wide confident grin, while preserving the Thousand's face scale.
        float mouthX = x + w * 0.5f;
        float mouthY = y + h * 0.15f;
        DrawEllipse(pixels, width, height, mouthX, mouthY, w * 0.41f, h * 0.145f, outline);
        DrawEllipse(pixels, width, height, mouthX, mouthY, w * 0.32f, h * 0.075f, black);
        DrawCapsule(
            pixels, width, height,
            mouthX - w * 0.15f,
            mouthY + h * 0.012f,
            mouthX + w * 0.15f,
            mouthY + h * 0.012f,
            w * 0.025f,
            white);
        DrawCapsule(
            pixels, width, height,
            mouthX,
            mouthY - h * 0.055f,
            mouthX,
            mouthY + h * 0.055f,
            w * 0.012f,
            mouthRed);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static void DrawEllipse(
        Color[] pixels,
        int width,
        int height,
        float centerX,
        float centerY,
        float radiusX,
        float radiusY,
        Color color)
    {
        int minX = Mathf.Clamp(Mathf.FloorToInt(centerX - radiusX - 2f), 0, width - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(centerX + radiusX + 2f), 0, width - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt(centerY - radiusY - 2f), 0, height - 1);
        int maxY = Mathf.Clamp(Mathf.CeilToInt(centerY + radiusY + 2f), 0, height - 1);
        float edge = Mathf.Max(0.004f, 1.5f / Mathf.Max(1f, Mathf.Min(radiusX, radiusY)));

        for (int py = minY; py <= maxY; py++)
        {
            float dy = (py + 0.5f - centerY) / Mathf.Max(radiusY, 0.001f);
            for (int px = minX; px <= maxX; px++)
            {
                float dx = (px + 0.5f - centerX) / Mathf.Max(radiusX, 0.001f);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float coverage = Mathf.Clamp01((1f - distance) / edge);
                if (coverage > 0f)
                {
                    BlendPixel(pixels, py * width + px, color, coverage);
                }
            }
        }
    }

    private static void DrawCapsule(
        Color[] pixels,
        int width,
        int height,
        float ax,
        float ay,
        float bx,
        float by,
        float radius,
        Color color)
    {
        int minX = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(ax, bx) - radius - 2f), 0, width - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(ax, bx) + radius + 2f), 0, width - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(ay, by) - radius - 2f), 0, height - 1);
        int maxY = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(ay, by) + radius + 2f), 0, height - 1);
        Vector2 start = new Vector2(ax, ay);
        Vector2 segment = new Vector2(bx - ax, by - ay);
        float segmentLengthSquared = Mathf.Max(segment.sqrMagnitude, 0.001f);

        for (int py = minY; py <= maxY; py++)
        {
            for (int px = minX; px <= maxX; px++)
            {
                Vector2 point = new Vector2(px + 0.5f, py + 0.5f);
                float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / segmentLengthSquared);
                float distance = Vector2.Distance(point, start + segment * t);
                float coverage = Mathf.Clamp01(radius + 1.5f - distance);
                if (coverage > 0f)
                {
                    BlendPixel(pixels, py * width + px, color, coverage);
                }
            }
        }
    }

    private static void BlendPixel(Color[] pixels, int index, Color color, float coverage)
    {
        float sourceAlpha = Mathf.Clamp01(color.a * coverage);
        Color destination = pixels[index];
        float outputAlpha = sourceAlpha + destination.a * (1f - sourceAlpha);
        if (outputAlpha <= 0.0001f)
        {
            return;
        }

        Color output = new Color(
            (color.r * sourceAlpha + destination.r * destination.a * (1f - sourceAlpha)) / outputAlpha,
            (color.g * sourceAlpha + destination.g * destination.a * (1f - sourceAlpha)) / outputAlpha,
            (color.b * sourceAlpha + destination.b * destination.a * (1f - sourceAlpha)) / outputAlpha,
            outputAlpha);
        pixels[index] = output;
    }

    private static PixelBounds FindDifferenceBounds(
        Color32[] facePixels,
        Color32[] bodyPixels,
        int width,
        int height,
        float threshold)
    {
        PixelBounds bounds = new PixelBounds
        {
            MinX = width,
            MinY = height,
            MaxX = -1,
            MaxY = -1
        };
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
                if (difference <= threshold || face.a <= 8)
                {
                    continue;
                }

                bounds.MinX = Mathf.Min(bounds.MinX, x);
                bounds.MinY = Mathf.Min(bounds.MinY, y);
                bounds.MaxX = Mathf.Max(bounds.MaxX, x);
                bounds.MaxY = Mathf.Max(bounds.MaxY, y);
            }
        }
        return bounds;
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

    private static void ConfigureFaceTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("[S80] Face texture was not imported.", path);
        }

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
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
            throw new InvalidOperationException("[S80] No transparent shader is available.");
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

    private static void CreateThousandVariant(
        Material surprisedMaterial,
        Material extremeMaterial,
        Material determinedMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(ThousandSourcePath);
        if (root == null)
        {
            throw new FileNotFoundException("[S80] Original 1000 prefab is missing.", ThousandSourcePath);
        }

        try
        {
            Material originalFaceMaterial = LoadAsset<Material>(OriginalFaceMaterialPath);
            Material cleanBodyMaterial = LoadAsset<Material>(CleanBodyMaterialPath);
            Renderer front = FindRendererUsingMaterial(root, originalFaceMaterial);
            if (front == null)
            {
                throw new InvalidOperationException("[S80] Original 1000 front face was not found.");
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
            originalOverlay.name = S80_Main.OriginalFaceName;
            ConfigureOverlay(originalOverlay, originalFaceMaterial);
            originalOverlay.transform.position += outward * 0.006f;

            GameObject surprisedOverlay = UnityEngine.Object.Instantiate(
                originalOverlay,
                originalOverlay.transform.parent,
                false);
            surprisedOverlay.name = S80_Main.SurprisedFaceName;
            ConfigureOverlay(surprisedOverlay, surprisedMaterial);
            surprisedOverlay.transform.position += outward * 0.002f;
            surprisedOverlay.SetActive(false);

            GameObject extremeOverlay = UnityEngine.Object.Instantiate(
                originalOverlay,
                originalOverlay.transform.parent,
                false);
            extremeOverlay.name = S80_Main.ExtremeFaceName;
            ConfigureOverlay(extremeOverlay, extremeMaterial);
            extremeOverlay.transform.position += outward * 0.004f;
            extremeOverlay.SetActive(false);

            GameObject determinedOverlay = UnityEngine.Object.Instantiate(
                originalOverlay,
                originalOverlay.transform.parent,
                false);
            determinedOverlay.name = S80_Main.DeterminedFaceName;
            ConfigureOverlay(determinedOverlay, determinedMaterial);
            determinedOverlay.transform.position += outward * 0.006f;
            determinedOverlay.SetActive(false);

            front.gameObject.name = "S80 Clean Front Body";
            ReplaceMaterial(front, originalFaceMaterial, cleanBodyMaterial);
            root.name = "Thousand_Collision";
            PrefabUtility.SaveAsPrefabAsset(root, ThousandPrefabPath);
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
            throw new InvalidOperationException("[S80] Face overlay has no Renderer.");
        }
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.sortingOrder = 20;
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
            throw new InvalidOperationException("[S80] Original face material was not assigned to the front.");
        }
        renderer.sharedMaterials = materials;
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

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Vertical 9x16");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.fieldOfView = 44f;
        camera.aspect = 9f / 16f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 160f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.48f, 0.81f, 0.97f);
        camera.transform.position = new Vector3(0f, 2.35f, -16.8f);
        camera.transform.LookAt(new Vector3(0f, 1.52f, 0.3f));
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(43f, -31f, 0f);
        RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f);
    }

    private static BoxCollider CreateEnvironment(
        Material groundMaterial,
        Material hillMaterial,
        Material cloudMaterial)
    {
        GameObject environment = new GameObject("S80 Bright Collision Field");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Grass Ground";
        ground.transform.SetParent(environment.transform, true);
        ground.transform.position = new Vector3(0f, 0f, 8f);
        ground.transform.localScale = new Vector3(24f, 1f, 22f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        RemoveColliders(ground);
        BoxCollider groundCollider = ground.AddComponent<BoxCollider>();
        groundCollider.size = new Vector3(10f, 0.12f, 10f);
        groundCollider.center = new Vector3(0f, -0.06f, 0f);

        CreateBackdropSphere(
            "Left Hill",
            new Vector3(-8.5f, -1.7f, 14f),
            new Vector3(13f, 5.4f, 2.4f),
            hillMaterial,
            environment.transform);
        CreateBackdropSphere(
            "Right Hill",
            new Vector3(8.2f, -1.9f, 14.5f),
            new Vector3(14f, 5.8f, 2.5f),
            hillMaterial,
            environment.transform);

        GameObject clouds = new GameObject("Soft Clouds");
        clouds.transform.SetParent(environment.transform, false);
        CreateCloud(new Vector3(-3.4f, 7.3f, 12f), 0.9f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(3.2f, 10.8f, 13f), 0.72f, cloudMaterial, clouds.transform);
        CreateCloud(new Vector3(-2.2f, 14f, 13.5f), 0.58f, cloudMaterial, clouds.transform);
        return groundCollider;
    }

    private static void CreateCloud(
        Vector3 center,
        float scale,
        Material material,
        Transform parent)
    {
        CreateBackdropSphere(
            "Cloud Puff", center + Vector3.left * 0.72f * scale,
            new Vector3(1.45f, 0.72f, 0.35f) * scale, material, parent);
        CreateBackdropSphere(
            "Cloud Puff", center + Vector3.up * 0.22f * scale,
            new Vector3(1.55f, 0.92f, 0.38f) * scale, material, parent);
        CreateBackdropSphere(
            "Cloud Puff", center + Vector3.right * 0.78f * scale,
            new Vector3(1.38f, 0.68f, 0.34f) * scale, material, parent);
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
        Material template = GraphicsSettings.currentRenderPipeline != null
            ? GraphicsSettings.currentRenderPipeline.defaultMaterial
            : null;
        Shader shader = template == null ? Shader.Find("Standard") : template.shader;
        if (shader == null)
        {
            throw new InvalidOperationException("[S80] No opaque shader is available.");
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
            throw new InvalidOperationException("[S80] No flash shader is available.");
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

        SetMaterialColor(material, new Color(1f, 0.83f, 0.16f, 0.72f));
        ConfigureTransparentBlend(material);
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(1f, 0.45f, 0.03f) * 1.8f);
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

    private static void ValidateThousandVariant()
    {
        GameObject prefab = LoadAsset<GameObject>(ThousandPrefabPath);
        Transform original = FindChild(prefab.transform, S80_Main.OriginalFaceName);
        Transform surprised = FindChild(prefab.transform, S80_Main.SurprisedFaceName);
        Transform extreme = FindChild(prefab.transform, S80_Main.ExtremeFaceName);
        Transform determined = FindChild(prefab.transform, S80_Main.DeterminedFaceName);
        Transform cleanBody = FindChild(prefab.transform, "S80 Clean Front Body");
        if (original == null || surprised == null || extreme == null ||
            determined == null || cleanBody == null)
        {
            throw new InvalidOperationException("[S80_VALIDATE] Thousand expression prefab is incomplete.");
        }

        Renderer surprisedRenderer = surprised.GetComponent<Renderer>();
        Renderer extremeRenderer = extreme.GetComponent<Renderer>();
        Renderer determinedRenderer = determined.GetComponent<Renderer>();
        if (AssetDatabase.GetAssetPath(surprisedRenderer.sharedMaterial) != SurprisedMaterialPath ||
            AssetDatabase.GetAssetPath(extremeRenderer.sharedMaterial) != ExtremeMaterialPath ||
            AssetDatabase.GetAssetPath(determinedRenderer.sharedMaterial) != DeterminedMaterialPath)
        {
            throw new InvalidOperationException("[S80_VALIDATE] Surprise materials are not assigned.");
        }
    }

    private static Transform FindChild(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
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
    }

    private static T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            throw new FileNotFoundException($"[S80] Required asset is missing: {path}", path);
        }
        return asset;
    }
}
