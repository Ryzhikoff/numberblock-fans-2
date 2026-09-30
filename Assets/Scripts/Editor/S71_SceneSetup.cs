using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Быстрая настройка Scene_71 ("Восхождение треугольных чисел").
/// Создаёт GameManager с S71_Main, камеру, простое UI и автоматически подставляет:
///  - единичный префаб "1"  (Assets/Prefabs/Blocks/1.prefab)
///  - треугольные префабы T1..T55 (Assets/Prefabs/Blocks/Triangular/)
///
/// Порядок работы:
///   1) Tools/Scene_71/Generate Triangular Blocks   (сначала сгенерировать блоки)
///   2) Открыть/создать сцену Scene_71
///   3) Tools/Scene_71/Quick Setup Scene
///   4) Play  (или запись через Unity Recorder)
/// </summary>
public static class S71_SceneSetup
{
    const string OnePath = "Assets/Prefabs/Blocks/1.prefab";
    const string TriDir  = "Assets/Prefabs/Blocks/Triangular";

    [MenuItem("Tools/Scene_71/Quick Setup Scene")]
    public static void QuickSetup()
    {
        // GameManager
        var gm = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        var main = gm.GetComponent<S71_Main>() ?? gm.AddComponent<S71_Main>();

        // Origin
        var origin = GameObject.Find("FigureOrigin") ?? new GameObject("FigureOrigin");
        origin.transform.position = Vector3.zero;
        main.figureOrigin = origin.transform;

        // Камера
        var cam = Camera.main;
        if (cam == null) {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.transform.position = new Vector3(0f, 4f, -14f);
        cam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.07f, 0.12f);
        main.mainCamera = cam;

        // Свет
        if (Object.FindObjectOfType<Light>() == null) {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // Префабы
        main.onePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(OnePath);
        if (main.onePrefab == null)
            Debug.LogWarning($"[S71] Не найден {OnePath} — назначь единичный блок вручную.");

        main.triangularPrefabs = LoadTriangularOrdered();
        if (main.triangularPrefabs.Count == 0)
            Debug.LogWarning("[S71] Треугольные префабы не найдены. Сначала: Tools/Scene_71/Generate Triangular Blocks");

        // UI
        SetupUI(main);

        EditorUtility.SetDirty(gm);
        Debug.Log($"[S71] Сцена настроена. Треугольных префабов: {main.triangularPrefabs.Count}. Нажми Play.");
        EditorUtility.DisplayDialog("Scene_71",
            $"Готово.\nЕдиничный блок: {(main.onePrefab != null ? "ок" : "НЕ НАЙДЕН")}\nТреугольных: {main.triangularPrefabs.Count}\n\nНажми Play или пиши через Unity Recorder.", "OK");
    }

    /// <summary>Загрузить T1,T3,T6,...,T55 строго по порядку шагов.</summary>
    static List<GameObject> LoadTriangularOrdered()
    {
        var list = new List<GameObject>();
        for (int k = 1; k <= 10; k++) {
            int number = k * (k + 1) / 2;
            var go = AssetDatabase.LoadAssetAtPath<GameObject>($"{TriDir}/T{number}.prefab");
            if (go != null) list.Add(go);
        }
        return list;
    }

    static void SetupUI(S71_Main main)
    {
        var canvasGo = GameObject.Find("Canvas_S71");
        Canvas canvas;
        if (canvasGo == null) {
            canvasGo = new GameObject("Canvas_S71");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        } else {
            canvas = canvasGo.GetComponent<Canvas>();
        }

        main.titleText    = MakeText(canvas, "Title",    new Vector2(0f, -60f),  TextAlignmentOptions.Top,    48, "TRIANGULAR NUMBERS");
        main.equationText = MakeText(canvas, "Equation", new Vector2(0f,  120f), TextAlignmentOptions.Bottom, 40, "1");
        main.numberText   = MakeText(canvas, "Number",   new Vector2(0f,  40f),  TextAlignmentOptions.Bottom, 90, "1");
    }

    static TextMeshProUGUI MakeText(Canvas canvas, string name, Vector2 anchoredPos, TextAlignmentOptions align, float size, string text)
    {
        var existing = canvas.transform.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null) go.transform.SetParent(canvas.transform, false);
        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        var rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0.5f, align == TextAlignmentOptions.Top ? 1f : 0f);
        rt.anchorMax = rt.anchorMin;
        rt.pivot = new Vector2(0.5f, align == TextAlignmentOptions.Top ? 1f : 0f);
        rt.sizeDelta = new Vector2(900f, 140f);
        rt.anchoredPosition = anchoredPos;
        return tmp;
    }
}
