using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Builds and validates the silent vertical Scene 84 short.</summary>
public static class S84_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_84.unity";
    private const string AssetRoot = "Assets/Prefabs/Blocks/Scene84";
    private const string TextureRoot = AssetRoot + "/FaceTextures";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string PrefabRoot = AssetRoot + "/Prefabs";

    private const string OnePath = "Assets/Prefabs/Blocks/1.prefab";
    private const string ThousandSourcePath = "Assets/Prefabs/Blocks/1000.prefab";
    private const string FiveThousandSourcePath = "Assets/Prefabs/Blocks/5000.prefab";
    private const string ThousandOriginalFaceMaterialPath =
        "Assets/Materials/Materials/oneThousandFace.mat";
    private const string ThousandCleanBodyMaterialPath =
        "Assets/Materials/Materials/OneThousandTexture 1.mat";
    private const string ThousandSurpriseTexturePath =
        "Assets/Prefabs/Blocks/Scene80/FaceTextures/Thousand_ExtremeSurprise.png";
    private const string FiveThousandOriginalFaceMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/500-face.mat";
    private const string FiveThousandCleanBodyMaterialPath =
        "Assets/Prefabs/Blocks/materials/Materials/500-back.mat";

    private const string CunningTexturePath =
        TextureRoot + "/FiveThousand_Cunning.png";
    private const string SurpriseTexturePath =
        TextureRoot + "/FiveThousand_Surprised.png";
    private const string CunningMaterialPath =
        MaterialRoot + "/FiveThousand_Cunning_Transparent.mat";
    private const string SurpriseMaterialPath =
        MaterialRoot + "/FiveThousand_Surprised_Transparent.mat";
    private const string ThousandSurpriseMaterialPath =
        MaterialRoot + "/Thousand_Surprised_Transparent.mat";
    private const string ThousandPrefabPath =
        PrefabRoot + "/Thousand_Surprise.prefab";
    private const string FiveThousandPrefabPath =
        PrefabRoot + "/FiveThousand_Expressions.prefab";

    private const string ApproachPath = "Assets/Sound/zvuk-priblijeniya.mp3";
    private const string StompPath = "Assets/Sound/boom_metal.wav";
    private const string BurstPath = "Assets/Sound/destroy_blocks.mp3";
    private const string RunPath = "Assets/Sound/runaway.mp3";

    private static readonly float[] CaptureTimes =
    {
        0.8f,
        1.75f,
        3.45f,
        4.22f,
        4.82f,
        5.32f,
        7.28f,
        9.38f,
        10.38f,
        14.88f
    };

    private static bool[] capturedFrames;
    private static bool playModeStarted;
    private static bool playModePassed;
    private static double playModeStartTime;

    [MenuItem("Tools/Scene_84/Quick Setup Silent 1000 vs 5000 Short")]
    public static void QuickSetup()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        GenerateFiveThousandFaces();
        ConfigureFaceTexture(CunningTexturePath);
        ConfigureFaceTexture(SurpriseTexturePath);

        Material cunningMaterial = CreateTransparentMaterial(
            CunningMaterialPath,
            LoadAsset<Texture2D>(CunningTexturePath));
        Material surpriseMaterial = CreateTransparentMaterial(
            SurpriseMaterialPath,
            LoadAsset<Texture2D>(SurpriseTexturePath));
        Material thousandSurpriseMaterial = CreateTransparentMaterial(
            ThousandSurpriseMaterialPath,
            LoadAsset<Texture2D>(ThousandSurpriseTexturePath));
        CreateThousandVariant(thousandSurpriseMaterial);
        CreateFiveThousandVariant(cunningMaterial, surpriseMaterial);

        Material groundMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S84_Ground.mat",
            new Color(0.32f, 0.76f, 0.36f));
        Material hillMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S84_Hills.mat",
            new Color(0.18f, 0.58f, 0.31f));
        Material cloudMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S84_Clouds.mat",
            new Color(0.97f, 0.99f, 1f));
        Material dustMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S84_Dust.mat",
            new Color(0.95f, 0.73f, 0.3f));
        Material shadowMaterial = CreateOpaqueMaterial(
            MaterialRoot + "/S84_Shadow.mat",
            new Color(0.08f, 0.18f, 0.16f, 1f));
        Material flashMaterial = CreateTransparentColorMaterial(
            MaterialRoot + "/S84_Flash.mat",
            new Color(1f, 0.95f, 0.62f, 0.78f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCamera();
        CreateLighting();
        BoxCollider groundCollider = CreateEnvironment(
            groundMaterial,
            hillMaterial,
            cloudMaterial);
        Transform shadow = CreateApproachingShadow(shadowMaterial);

        GameObject burstObject = new GameObject("Exact Ones — 1000 and 5000 GPU Escape");
        S84_UnitBurst burst = burstObject.AddComponent<S84_UnitBurst>();
        burst.onePrefab = LoadAsset<GameObject>(OnePath);
        burst.groundCollider = groundCollider;

        GameObject managerObject = new GameObject("GameManager — Silent 1000 vs 5000 Timeline");
        S84_Main director = managerObject.AddComponent<S84_Main>();
        director.onePrefab = LoadAsset<GameObject>(OnePath);
        director.thousandPrefab = LoadAsset<GameObject>(ThousandPrefabPath);
        director.fiveThousandPrefab = LoadAsset<GameObject>(FiveThousandPrefabPath);
        director.unitBurst = burst;
        director.approachingShadow = shadow;
        director.dustMaterial = dustMaterial;
        director.flashMaterial = flashMaterial;
        director.characterScale = 0.255f;
        director.approachClip = LoadAsset<AudioClip>(ApproachPath);
        director.stompClip = LoadAsset<AudioClip>(StompPath);
        director.burstClip = LoadAsset<AudioClip>(BurstPath);
        director.runClip = LoadAsset<AudioClip>(RunPath);
        EditorUtility.SetDirty(burst);
        EditorUtility.SetDirty(director);

        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            throw new IOException($"[S84] Unity could not save the scene: {ScenePath}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "[S84] Scene_84 created: silent vertical stomp, scene-local 5000 expressions, " +
            "five stacked Thousands, and exact 1,000/5,000-unit escapes.");

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog(
                "Scene_84 — 1000 vs 5000",
                "Готово. Выберите Game View 9:16 и нажмите Play. Ролик длится 14,6 секунды.",
                "OK");
        }
    }

    [MenuItem("Tools/Scene_84/Validate Silent 1000 vs 5000 Short")]
    public static void ValidateScene()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("[S84_VALIDATE] Scene is missing.", ScenePath);
        }

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        S84_Main director = UnityEngine.Object.FindAnyObjectByType<S84_Main>();
        S84_UnitBurst burst = UnityEngine.Object.FindAnyObjectByType<S84_UnitBurst>();
        if (director == null || burst == null)
        {
            throw new InvalidOperationException("[S84_VALIDATE] Director or unit burst is missing.");
        }

        if (AssetDatabase.GetAssetPath(director.onePrefab) != OnePath ||
            AssetDatabase.GetAssetPath(director.thousandPrefab) != ThousandPrefabPath ||
            AssetDatabase.GetAssetPath(director.fiveThousandPrefab) != FiveThousandPrefabPath ||
            director.unitBurst != burst || burst.onePrefab != director.onePrefab)
        {
            throw new InvalidOperationException("[S84_VALIDATE] Numberblock references are incomplete.");
        }

        if (!(burst.groundCollider is BoxCollider) || director.approachingShadow == null ||
            director.dustMaterial == null || director.flashMaterial == null ||
            director.approachClip == null || director.stompClip == null ||
            director.burstClip == null || director.runClip == null)
        {
            throw new InvalidOperationException("[S84_VALIDATE] Environment, effects, or audio is incomplete.");
        }

        Camera camera = Camera.main;
        if (camera == null || camera.name != "Main Camera — Vertical 9x16" ||
            Mathf.Abs(camera.fieldOfView - 36f) > 0.001f)
        {
            throw new InvalidOperationException("[S84_VALIDATE] Vertical camera is misconfigured.");
        }

        if (!(S84_Main.ShadowStartTime < S84_Main.GiantRevealTime &&
              S84_Main.GiantRevealTime < S84_Main.StompContactTime &&
              S84_Main.StompContactTime < S84_Main.CrushBurstTime &&
              S84_Main.CrushBurstTime < S84_Main.GiantSurpriseTime &&
              S84_Main.GiantSurpriseTime < S84_Main.SplitTime &&
              S84_Main.SplitTime < S84_Main.StackSurpriseTime &&
              S84_Main.StackSurpriseTime < S84_Main.FinalBurstTime &&
              S84_Main.FinalBurstTime < S84_Main.SequenceDuration))
        {
            throw new InvalidOperationException("[S84_VALIDATE] Timeline beats overlap or are out of order.");
        }

        if (S84_UnitBurst.ThousandUnitCount != 10 * 10 * 10 ||
            S84_UnitBurst.FiveThousandUnitCount != 10 * 50 * 10 ||
            S84_UnitBurst.MaximumUnitCount != 5000)
        {
            throw new InvalidOperationException("[S84_VALIDATE] Exact unit counts changed.");
        }

        ValidateTransparentFace(CunningTexturePath, 521, 521);
        ValidateTransparentFace(SurpriseTexturePath, 521, 521);
        ValidateImporter(CunningTexturePath);
        ValidateImporter(SurpriseTexturePath);
        ValidateThousandVariant();
        ValidateFiveThousandVariant();
        ValidateSourcesUnchanged();

        Debug.Log(
            "[S84_VALIDATE] PASS — vertical 9:16, silent ordered timeline, original anatomy, " +
            "transparent expressions, exact 1,000 and 5,000 unit grids, and source prefabs preserved.");
    }

    /// <summary>Runs the whole short at 2x and captures all important silent beats.</summary>
    public static void RunPlayModeValidation()
    {
        if (!Application.isBatchMode)
        {
            throw new InvalidOperationException("[S84_PLAY] This entry point is batch-mode only.");
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
            Debug.Log("[S84_PLAY] Entered Play Mode at 2x speed.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && playModeStarted)
        {
            Time.timeScale = 1f;
            EditorApplication.update -= PlayModeValidationTick;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;

            if (playModePassed)
            {
                Debug.Log(
                    $"[S84_PLAY] PASS — complete short and all {CaptureTimes.Length} keyframes ran cleanly.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[S84_PLAY] FAIL — the complete short did not reach its ending.");
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

        S84_Main director = UnityEngine.Object.FindAnyObjectByType<S84_Main>();
        if (director != null)
        {
            for (int i = 0; i < CaptureTimes.Length; i++)
            {
                if (!capturedFrames[i] && director.SequenceTime >= CaptureTimes[i])
                {
                    capturedFrames[i] = true;
                    CaptureRuntimeFrame($"/private/tmp/scene84-{i + 1:00}.png");
                }
            }

            if (director.SequenceComplete)
            {
                playModePassed = AllFramesCaptured() &&
                    director.CrushStarted &&
                    director.FirstBurstTriggered &&
                    director.SplitTriggered &&
                    director.StackSurprised &&
                    director.FinalBurstTriggered &&
                    director.VisibleThousandCount == 0 &&
                    director.unitBurst.ActiveUnitCount == S84_UnitBurst.FiveThousandUnitCount &&
                    director.unitBurst.LastBurstCount == S84_UnitBurst.FiveThousandUnitCount &&
                    director.unitBurst.EscapedUnitCount > 0;
                EditorApplication.isPlaying = false;
                return;
            }
        }

        if (EditorApplication.timeSinceStartup - playModeStartTime > 24d)
        {
            Debug.LogError("[S84_PLAY] Timed out after 24 real seconds.");
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
            throw new InvalidOperationException("[S84_PLAY] Main Camera disappeared.");
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

        Debug.Log($"[S84_PLAY] Captured {path}");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Prefabs/Blocks", "Scene84");
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

    private static void GenerateFiveThousandFaces()
    {
        WriteFiveThousandExpression(CunningTexturePath, false);
        WriteFiveThousandExpression(SurpriseTexturePath, true);
        AssetDatabase.ImportAsset(CunningTexturePath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(SurpriseTexturePath, ImportAssetOptions.ForceSynchronousImport);
    }

    private static void WriteFiveThousandExpression(string path, bool surprised)
    {
        const int width = 521;
        const int height = 521;
        Color[] pixels = new Color[width * height];
        Color navy = new Color(0.14f, 0.2f, 0.38f, 1f);
        Color black = new Color(0.018f, 0.016f, 0.024f, 1f);
        Color white = new Color(0.97f, 0.98f, 0.98f, 1f);
        Color red = new Color(0.92f, 0.32f, 0.38f, 1f);

        Vector2 starCenter = new Vector2(165f, 354f);
        Vector2[] star = CreateStar(starCenter, 142f, 67f, -90f);
        DrawPolygon(pixels, width, height, star, navy);

        if (surprised)
        {
            DrawEllipse(pixels, width, height, 165f, 352f, 83f, 104f, white);
            DrawEllipse(pixels, width, height, 165f, 343f, 27f, 35f, black);
            DrawEllipse(pixels, width, height, 156f, 355f, 7f, 8f, white);

            DrawEllipse(pixels, width, height, 410f, 352f, 94f, 119f, navy);
            DrawEllipse(pixels, width, height, 410f, 352f, 73f, 98f, white);
            DrawEllipse(pixels, width, height, 410f, 343f, 27f, 36f, black);
            DrawEllipse(pixels, width, height, 401f, 355f, 7f, 8f, white);

            DrawCapsule(pixels, width, height, 101f, 477f, 218f, 490f, 10f, navy);
            DrawCapsule(pixels, width, height, 359f, 490f, 467f, 477f, 10f, navy);

            DrawEllipse(pixels, width, height, 286f, 102f, 65f, 77f, red);
            DrawEllipse(pixels, width, height, 286f, 105f, 43f, 57f, black);
            DrawEllipse(pixels, width, height, 286f, 73f, 29f, 17f, red);
        }
        else
        {
            DrawEllipse(pixels, width, height, 165f, 350f, 84f, 102f, white);
            DrawEllipse(pixels, width, height, 183f, 322f, 37f, 48f, black);
            DrawEllipse(pixels, width, height, 171f, 338f, 8f, 9f, white);
            DrawCapsule(pixels, width, height, 94f, 429f, 225f, 386f, 13f, navy);

            DrawEllipse(pixels, width, height, 410f, 350f, 94f, 118f, navy);
            DrawEllipse(pixels, width, height, 410f, 350f, 73f, 97f, white);
            DrawEllipse(pixels, width, height, 387f, 320f, 37f, 49f, black);
            DrawEllipse(pixels, width, height, 375f, 337f, 8f, 9f, white);
            DrawCapsule(pixels, width, height, 343f, 386f, 474f, 429f, 13f, navy);

            DrawCapsule(pixels, width, height, 201f, 127f, 343f, 145f, 29f, red);
            DrawCapsule(pixels, width, height, 211f, 132f, 333f, 145f, 16f, black);
            DrawCapsule(pixels, width, height, 231f, 143f, 317f, 151f, 5f, white);
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static Vector2[] CreateStar(Vector2 center, float outerRadius, float innerRadius, float angle)
    {
        Vector2[] points = new Vector2[10];
        for (int i = 0; i < points.Length; i++)
        {
            float radius = i % 2 == 0 ? outerRadius : innerRadius;
            float radians = (angle + i * 36f) * Mathf.Deg2Rad;
            points[i] = center + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
        }
        return points;
    }

    private static void DrawPolygon(
        Color[] pixels,
        int width,
        int height,
        Vector2[] polygon,
        Color color)
    {
        float minX = polygon[0].x;
        float maxX = polygon[0].x;
        float minY = polygon[0].y;
        float maxY = polygon[0].y;
        foreach (Vector2 point in polygon)
        {
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxY = Mathf.Max(maxY, point.y);
        }

        int startX = Mathf.Clamp(Mathf.FloorToInt(minX) - 1, 0, width - 1);
        int endX = Mathf.Clamp(Mathf.CeilToInt(maxX) + 1, 0, width - 1);
        int startY = Mathf.Clamp(Mathf.FloorToInt(minY) - 1, 0, height - 1);
        int endY = Mathf.Clamp(Mathf.CeilToInt(maxY) + 1, 0, height - 1);
        Vector2[] samples =
        {
            new Vector2(0.25f, 0.25f),
            new Vector2(0.75f, 0.25f),
            new Vector2(0.25f, 0.75f),
            new Vector2(0.75f, 0.75f)
        };

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                int covered = 0;
                foreach (Vector2 sample in samples)
                {
                    if (PointInPolygon(new Vector2(x + sample.x, y + sample.y), polygon))
                    {
                        covered++;
                    }
                }
                if (covered > 0)
                {
                    BlendPixel(pixels, y * width + x, color, covered / 4f);
                }
            }
        }
    }

    private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];
            float verticalDelta = b.y - a.y;
            bool crosses = (a.y > point.y) != (b.y > point.y) &&
                Mathf.Abs(verticalDelta) > 0.0001f &&
                point.x < (b.x - a.x) * (point.y - a.y) / verticalDelta + a.x;
            if (crosses)
            {
                inside = !inside;
            }
        }
        return inside;
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
        float lengthSquared = Mathf.Max(segment.sqrMagnitude, 0.001f);

        for (int py = minY; py <= maxY; py++)
        {
            for (int px = minX; px <= maxX; px++)
            {
                Vector2 point = new Vector2(px + 0.5f, py + 0.5f);
                float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
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

        pixels[index] = new Color(
            (color.r * sourceAlpha + destination.r * destination.a * (1f - sourceAlpha)) / outputAlpha,
            (color.g * sourceAlpha + destination.g * destination.a * (1f - sourceAlpha)) / outputAlpha,
            (color.b * sourceAlpha + destination.b * destination.a * (1f - sourceAlpha)) / outputAlpha,
            outputAlpha);
    }

    private static void ConfigureFaceTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("[S84] Face texture was not imported.", path);
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

    private static Material CreateTransparentMaterial(string path, Texture2D texture)
    {
        Material material = CreateOrLoadMaterial(path, true);
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
            : Shader.Find("Standard");
        shader ??= Shader.Find("Unlit/Transparent");
        if (shader == null)
        {
            throw new InvalidOperationException("[S84] No color-capable transparent shader is available.");
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
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Lit")
            : Shader.Find("Standard");
        shader ??= Shader.Find("Unlit/Color");
        if (shader == null)
        {
            throw new InvalidOperationException("[S84] No opaque shader is available.");
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
        material.renderQueue = -1;
        SetMaterialColor(material, color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.08f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateOrLoadMaterial(string path, bool transparent)
    {
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Unlit")
            : Shader.Find("Unlit/Transparent");
        shader ??= Shader.Find("Sprites/Default");
        if (shader == null)
        {
            throw new InvalidOperationException("[S84] No transparent shader is available.");
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
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;
        SetFloatIfPresent(material, "_Surface", 1f);
        SetFloatIfPresent(material, "_Blend", 0f);
        SetFloatIfPresent(material, "_Mode", 3f);
        SetFloatIfPresent(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
        SetFloatIfPresent(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        if (material.shader != null && material.shader.name == "Standard")
        {
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        else
        {
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
    }

    private static void SetTextureIfPresent(Material material, string property, Texture texture)
    {
        if (material.HasProperty(property))
        {
            material.SetTexture(property, texture);
        }
    }

    private static void SetFloatIfPresent(Material material, string property, float value)
    {
        if (material.HasProperty(property))
        {
            material.SetFloat(property, value);
        }
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private static void CreateThousandVariant(Material surpriseMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(ThousandSourcePath);
        if (root == null)
        {
            throw new FileNotFoundException("[S84] Original 1000 prefab is missing.", ThousandSourcePath);
        }

        try
        {
            Material originalFace = LoadAsset<Material>(ThousandOriginalFaceMaterialPath);
            Material cleanBody = LoadAsset<Material>(ThousandCleanBodyMaterialPath);
            Renderer front = FindRendererUsingMaterial(root, originalFace);
            if (front == null)
            {
                throw new InvalidOperationException("[S84] Original 1000 front face was not found.");
            }

            Vector3 outward = FaceOutward(front);
            GameObject originalOverlay = UnityEngine.Object.Instantiate(
                front.gameObject,
                front.transform.parent,
                false);
            originalOverlay.name = S84_Main.ThousandOriginalFaceName;
            ConfigureOverlay(originalOverlay, originalFace);
            originalOverlay.transform.position += outward * 0.006f;

            GameObject surprisedOverlay = UnityEngine.Object.Instantiate(
                originalOverlay,
                originalOverlay.transform.parent,
                false);
            surprisedOverlay.name = S84_Main.ThousandSurprisedFaceName;
            ConfigureOverlay(surprisedOverlay, surpriseMaterial);
            surprisedOverlay.transform.position += outward * 0.003f;
            surprisedOverlay.SetActive(false);

            front.gameObject.name = "S84 Thousand Clean Front Body";
            ReplaceMaterial(front, originalFace, cleanBody);
            root.name = "Thousand_Surprise";
            PrefabUtility.SaveAsPrefabAsset(root, ThousandPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void CreateFiveThousandVariant(Material cunningMaterial, Material surpriseMaterial)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(FiveThousandSourcePath);
        if (root == null)
        {
            throw new FileNotFoundException("[S84] Original 5000 prefab is missing.", FiveThousandSourcePath);
        }

        try
        {
            Material originalFace = LoadAsset<Material>(FiveThousandOriginalFaceMaterialPath);
            Material cleanBody = LoadAsset<Material>(FiveThousandCleanBodyMaterialPath);
            Renderer front = FindRendererUsingMaterial(root, originalFace);
            if (front == null)
            {
                throw new InvalidOperationException("[S84] Original 5000 front face was not found.");
            }

            Vector3 outward = FaceOutward(front);
            GameObject cunningOverlay = UnityEngine.Object.Instantiate(
                front.gameObject,
                front.transform.parent,
                false);
            cunningOverlay.name = S84_Main.FiveThousandCunningFaceName;
            ConfigureOverlay(cunningOverlay, cunningMaterial);
            cunningOverlay.transform.position += outward * 0.006f;

            GameObject surprisedOverlay = UnityEngine.Object.Instantiate(
                cunningOverlay,
                cunningOverlay.transform.parent,
                false);
            surprisedOverlay.name = S84_Main.FiveThousandSurprisedFaceName;
            ConfigureOverlay(surprisedOverlay, surpriseMaterial);
            surprisedOverlay.transform.position += outward * 0.003f;
            surprisedOverlay.SetActive(false);

            front.gameObject.name = "S84 Five Thousand Clean Front Body";
            ReplaceMaterial(front, originalFace, cleanBody);
            root.name = "FiveThousand_Expressions";
            PrefabUtility.SaveAsPrefabAsset(root, FiveThousandPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Vector3 FaceOutward(Renderer front)
    {
        // Offset strictly along the source face-plane normal. Using the center
        // of the full 5000 body would also introduce a vertical shift because
        // its face lives only on the topmost thousand-sized cube.
        return -front.transform.forward.normalized;
    }

    private static void ConfigureOverlay(GameObject overlay, Material material)
    {
        Renderer renderer = overlay.GetComponent<Renderer>();
        if (renderer == null)
        {
            throw new InvalidOperationException("[S84] Face overlay has no Renderer.");
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
            throw new InvalidOperationException("[S84] Expected face material was not assigned.");
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

    private static void RemoveColliders(GameObject target)
    {
        foreach (Collider collider in target.GetComponents<Collider>())
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera — Vertical 9x16");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.aspect = 9f / 16f;
        camera.fieldOfView = 36f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 180f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.48f, 0.82f, 0.98f);
        camera.transform.position = new Vector3(0f, 2.25f, -16.5f);
        camera.transform.LookAt(new Vector3(-0.65f, 1.25f, 0f));
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.22f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
        RenderSettings.ambientLight = new Color(0.62f, 0.64f, 0.68f);
    }

    private static BoxCollider CreateEnvironment(
        Material groundMaterial,
        Material hillMaterial,
        Material cloudMaterial)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground — Bright Short Stage";
        ground.transform.position = new Vector3(0f, -0.18f, 0f);
        ground.transform.localScale = new Vector3(24f, 0.36f, 18f);
        ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        BoxCollider groundCollider = ground.GetComponent<BoxCollider>();

        CreateHill(new Vector3(-6.8f, 1.1f, 4.2f), new Vector3(8.5f, 2.4f, 2.2f), hillMaterial);
        CreateHill(new Vector3(6.4f, 0.85f, 4.8f), new Vector3(9.2f, 1.9f, 2.4f), hillMaterial);
        CreateCloud(new Vector3(-3.9f, 11.8f, 5.6f), 1.05f, cloudMaterial);
        CreateCloud(new Vector3(3.6f, 9.9f, 6.1f), 0.82f, cloudMaterial);
        return groundCollider;
    }

    private static void CreateHill(Vector3 position, Vector3 scale, Material material)
    {
        GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hill.name = "Background Hill";
        hill.transform.position = position;
        hill.transform.localScale = scale;
        hill.GetComponent<Renderer>().sharedMaterial = material;
        RemoveColliders(hill);
    }

    private static void CreateCloud(Vector3 position, float scale, Material material)
    {
        GameObject root = new GameObject("Background Cloud");
        root.transform.position = position;
        Vector3[] offsets =
        {
            new Vector3(-0.75f, 0f, 0f),
            new Vector3(0f, 0.18f, 0f),
            new Vector3(0.72f, -0.02f, 0f)
        };
        Vector3[] scales =
        {
            new Vector3(1.1f, 0.58f, 0.35f),
            new Vector3(1.35f, 0.82f, 0.4f),
            new Vector3(1.05f, 0.55f, 0.34f)
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = "Cloud Puff";
            puff.transform.SetParent(root.transform, false);
            puff.transform.localPosition = offsets[i] * scale;
            puff.transform.localScale = scales[i] * scale;
            puff.GetComponent<Renderer>().sharedMaterial = material;
            RemoveColliders(puff);
        }
    }

    private static Transform CreateApproachingShadow(Material material)
    {
        GameObject shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shadow.name = "Approaching 5000 Shadow";
        shadow.transform.position = new Vector3(1.25f, 0.035f, 0.18f);
        shadow.transform.localScale = new Vector3(0.05f, 0.018f, 0.05f);
        Renderer renderer = shadow.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        RemoveColliders(shadow);
        shadow.SetActive(false);
        return shadow.transform;
    }

    private static void ValidateTransparentFace(string path, int expectedWidth, int expectedHeight)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!texture.LoadImage(bytes, false) || texture.width != expectedWidth ||
                texture.height != expectedHeight)
            {
                throw new InvalidOperationException($"[S84_VALIDATE] Invalid face dimensions: {path}");
            }

            Color32[] pixels = texture.GetPixels32();
            int transparent = 0;
            int visible = 0;
            foreach (Color32 pixel in pixels)
            {
                if (pixel.a <= 2) transparent++;
                if (pixel.a >= 245) visible++;
            }
            if (transparent < pixels.Length / 3 || visible < pixels.Length / 12 ||
                pixels[0].a > 2 || pixels[expectedWidth - 1].a > 2 ||
                pixels[(expectedHeight - 1) * expectedWidth].a > 2 || pixels[pixels.Length - 1].a > 2)
            {
                throw new InvalidOperationException(
                    $"[S84_VALIDATE] Face lacks genuine transparent padding: {path}");
            }

            if (pixels[354 * expectedWidth + 165].a < 245 ||
                pixels[352 * expectedWidth + 410].a < 245 ||
                pixels[108 * expectedWidth + 286].a < 245)
            {
                throw new InvalidOperationException(
                    $"[S84_VALIDATE] Star eye, oval eye, or mouth is missing: {path}");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    private static void ValidateImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || importer.textureType != TextureImporterType.Default ||
            importer.alphaSource != TextureImporterAlphaSource.FromInput ||
            !importer.alphaIsTransparency || importer.mipmapEnabled ||
            importer.npotScale != TextureImporterNPOTScale.None ||
            importer.wrapMode != TextureWrapMode.Clamp ||
            importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            throw new InvalidOperationException($"[S84_VALIDATE] Face importer is incorrect: {path}");
        }
    }

    private static void ValidateThousandVariant()
    {
        GameObject prefab = LoadAsset<GameObject>(ThousandPrefabPath);
        Transform original = FindChild(prefab.transform, S84_Main.ThousandOriginalFaceName);
        Transform surprised = FindChild(prefab.transform, S84_Main.ThousandSurprisedFaceName);
        Transform clean = FindChild(prefab.transform, "S84 Thousand Clean Front Body");
        ValidateOverlayPair(original, surprised, clean, ThousandSurpriseMaterialPath);
    }

    private static void ValidateFiveThousandVariant()
    {
        GameObject prefab = LoadAsset<GameObject>(FiveThousandPrefabPath);
        Transform cunning = FindChild(prefab.transform, S84_Main.FiveThousandCunningFaceName);
        Transform surprised = FindChild(prefab.transform, S84_Main.FiveThousandSurprisedFaceName);
        Transform clean = FindChild(prefab.transform, "S84 Five Thousand Clean Front Body");
        ValidateOverlayPair(cunning, surprised, clean, SurpriseMaterialPath);

        Renderer cunningRenderer = cunning.GetComponent<Renderer>();
        if (AssetDatabase.GetAssetPath(cunningRenderer.sharedMaterial) != CunningMaterialPath)
        {
            throw new InvalidOperationException("[S84_VALIDATE] Cunning 5000 material is incorrect.");
        }
    }

    private static void ValidateOverlayPair(
        Transform primary,
        Transform surprised,
        Transform clean,
        string surpriseMaterialPath)
    {
        if (primary == null || surprised == null || clean == null)
        {
            throw new InvalidOperationException("[S84_VALIDATE] A face overlay is missing.");
        }
        if (!Approximately(primary.localPosition.x, clean.localPosition.x) ||
            !Approximately(primary.localPosition.y, clean.localPosition.y) ||
            Quaternion.Angle(primary.localRotation, clean.localRotation) > 0.01f ||
            Vector3.Distance(primary.localScale, clean.localScale) > 0.001f ||
            Mathf.Abs(primary.localPosition.z - clean.localPosition.z) > 0.02f ||
            Mathf.Abs(surprised.localPosition.z - clean.localPosition.z) > 0.02f)
        {
            throw new InvalidOperationException("[S84_VALIDATE] Face overlay transform drifted from source.");
        }

        Renderer primaryRenderer = primary.GetComponent<Renderer>();
        Renderer surprisedRenderer = surprised.GetComponent<Renderer>();
        if (primaryRenderer == null || surprisedRenderer == null ||
            primaryRenderer.shadowCastingMode != ShadowCastingMode.Off ||
            surprisedRenderer.shadowCastingMode != ShadowCastingMode.Off ||
            primaryRenderer.receiveShadows || surprisedRenderer.receiveShadows ||
            AssetDatabase.GetAssetPath(surprisedRenderer.sharedMaterial) != surpriseMaterialPath)
        {
            throw new InvalidOperationException("[S84_VALIDATE] Face overlay renderer is incorrect.");
        }
    }

    private static void ValidateSourcesUnchanged()
    {
        GameObject thousand = LoadAsset<GameObject>(ThousandSourcePath);
        GameObject fiveThousand = LoadAsset<GameObject>(FiveThousandSourcePath);
        Material thousandFace = LoadAsset<Material>(ThousandOriginalFaceMaterialPath);
        Material fiveThousandFace = LoadAsset<Material>(FiveThousandOriginalFaceMaterialPath);
        if (FindRendererUsingMaterial(thousand, thousandFace) == null ||
            FindRendererUsingMaterial(fiveThousand, fiveThousandFace) == null)
        {
            throw new InvalidOperationException("[S84_VALIDATE] A shared source prefab was modified.");
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

    private static bool Approximately(float a, float b)
    {
        return Mathf.Abs(a - b) <= 0.001f;
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
            throw new FileNotFoundException($"[S84] Required asset is missing: {path}", path);
        }
        return asset;
    }
}
