using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Редактор скрипта для автоматической настройки Scene_69.
/// Находит все префабы блоков и настраивает сцену.
/// 
/// Использование:
/// 1. Открыть Scene_69.unity
/// 2. Создать пустой объект "GameManager"
/// 3. Добавить компонент S69_Main
/// 4. В инспекторе нажать кнопку "AUTO-CONFIGURE SCENE"
/// </summary>
[CustomEditor(typeof(S69_Main))]
public class S69_MainEditor : Editor
{
    private S69_Main mainScript;
    private bool isConfiguring = false;

    private void OnEnable() {
        mainScript = (S69_Main)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        EditorGUILayout.Space(20);

        // Кнопка автоматической настройки
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.background = MakeTexture(20, 20, new Color(0.4f, 0.8f, 0.4f));

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🛠️ Scene Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🚀 AUTO-CONFIGURE SCENE", buttonStyle, GUILayout.Height(40))) {
            AutoConfigureScene();
        }

        EditorGUILayout.HelpBox(
            "Эта кнопка автоматически:\n" +
            "1. Найдёт все префабы блоков в Assets/Prefabs/Blocks/\n" +
            "2. Распределит их по уровням пирамиды\n" +
            "3. Настроит камеру и UI\n" +
            "4. Создаст точки спавна и эффекты",
            MessageType.Info);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Кнопка создания сцены
        if (GUILayout.Button("🎬 CREATE SCENE FROM SCRATCH", GUILayout.Height(30))) {
            CreateSceneFromScratch();
        }

        EditorGUILayout.HelpBox(
            "Создаст всю сцену с нуля:\n" +
            "- GameManager с S69_Main\n" +
            "- Camera с правильными настройками\n" +
            "- Свет и Skybox\n" +
            "- UI элементы (TextMeshPro)",
            MessageType.Info);

        // Проверка конфигурации
        EditorGUILayout.Space(10);
        CheckConfiguration();
    }

    private void AutoConfigureScene() {
        isConfiguring = true;

        Undo.RecordObject(mainScript, "Auto-Configure S69_Main");

        Debug.Log("🔍 Начинаю автоматическую настройку Scene_69...");

        // Найти все префабы блоков
        List<GameObject> allBlocks = FindAllBlockPrefabs();
        Debug.Log($"📦 Найдено префабов: {allBlocks.Count}");

        // Распределить по уровням
        List<GameObject> level1 = new List<GameObject>();
        List<GameObject> level2 = new List<GameObject>();
        List<GameObject> level3 = new List<GameObject>();
        List<GameObject> level4 = new List<GameObject>();
        GameObject hundredThousand = null;
        GameObject million = null;

        foreach (GameObject block in allBlocks) {
            if (block == null) continue;

            Scale scaleComp = block.GetComponent<Scale>();
            if (scaleComp == null) continue;

            long number = scaleComp.number;
            string blockName = block.name.ToLower();

            // Распределение по уровням
            if (number >= 1 && number <= 9) {
                level1.Add(block);
            }
            else if (number >= 10 && number <= 90) {
                level2.Add(block);
            }
            else if (number >= 100 && number <= 900) {
                level3.Add(block);
            }
            else if (number >= 1000 && number <= 9000) {
                level4.Add(block);
            }
            else if (number == 100000 || blockName.Contains("hundredthousand")) {
                hundredThousand = block;
            }
            else if (number == 1000000 || blockName.Contains("million")) {
                million = block;
            }
        }

        // Сортировка уровней по возрастанию числа
        level1 = SortBlocksByNumber(level1);
        level2 = SortBlocksByNumber(level2);
        level3 = SortBlocksByNumber(level3);
        level4 = SortBlocksByNumber(level4);

        // Назначить в скрипт
        mainScript.level1Blocks = level1;
        mainScript.level2Blocks = level2;
        mainScript.level3Blocks = level3;
        mainScript.level4Blocks = level4;
        mainScript.hundredThousandPrefab = hundredThousand;
        mainScript.millionPrefab = million;

        // Настроить камеру
        SetupCamera();

        // Создать UI если нет
        SetupUI();

        // Найти эффекты
        SetupEffects();

        EditorUtility.SetDirty(mainScript);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ Автоматическая настройка завершена!");
        Debug.Log($"📊 Уровень 1: {level1.Count} блоков");
        Debug.Log($"📊 Уровень 2: {level2.Count} блоков");
        Debug.Log($"📊 Уровень 3: {level3.Count} блоков");
        Debug.Log($"📊 Уровень 4: {level4.Count} блоков");
        Debug.Log($"🎯 100K: {(hundredThousand != null ? "✓" : "✗")}");
        Debug.Log($"🎯 1M: {(million != null ? "✓" : "✗")}");

        isConfiguring = false;
    }

    private List<GameObject> FindAllBlockPrefabs() {
        List<GameObject> blocks = new List<GameObject>();

        // Поиск в Assets/Prefabs/Blocks/
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Blocks" });
        
        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Пропустить эффекты и не-блоки
            if (path.ToLower().Contains("mergeeffect") || 
                path.ToLower().Contains("skibidi") ||
                path.ToLower().Contains("animblock")) {
                continue;
            }

            // ИСКЛЮЧИТЬ блоки из Scene_68 (не корректно отображаются)
            if (path.ToLower().Contains("scene68") || 
                path.ToLower().Contains("scene_68") ||
                path.ToLower().Contains("68_")) {
                Debug.LogWarning($"⚠️ Пропущен блок из Scene_68: {path}");
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && prefab.GetComponent<Scale>() != null) {
                blocks.Add(prefab);
            }
        }

        return blocks;
    }

    private List<GameObject> SortBlocksByNumber(List<GameObject> blocks) {
        return blocks.OrderBy(b => {
            Scale scale = b.GetComponent<Scale>();
            return scale != null ? scale.number : 0;
        }).ToList();
    }

    private void SetupCamera() {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null) {
            GameObject camObj = new GameObject("Main Camera");
            mainCamera = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }

        // Настроить позиции камеры для каждого уровня
        if (mainScript.cameraPoses == null || mainScript.cameraPoses.Count == 0) {
            mainScript.cameraPoses = new List<CameraPose>();
            
            // Уровень 1 - близко снизу
            mainScript.cameraPoses.Add(CreateCameraPose(
                new Vector3(0, 3, -18),
                Quaternion.Euler(15, 0, 0),
                2f
            ));
            
            // Уровень 2 - чуть выше и дальше
            mainScript.cameraPoses.Add(CreateCameraPose(
                new Vector3(0, 7, -22),
                Quaternion.Euler(20, 0, 0),
                2f
            ));
            
            // Уровень 3 - ещё выше и дальше
            mainScript.cameraPoses.Add(CreateCameraPose(
                new Vector3(0, 12, -28),
                Quaternion.Euler(25, 0, 0),
                2f
            ));
            
            // Уровень 4 - широко для обзора всей пирамиды
            mainScript.cameraPoses.Add(CreateCameraPose(
                new Vector3(0, 18, -35),
                Quaternion.Euler(30, 0, 0),
                2f
            ));
            
            // Финал - общий вид с высоты
            mainScript.cameraPoses.Add(CreateCameraPose(
                new Vector3(0, 25, -45),
                Quaternion.Euler(35, 0, 0),
                3f
            ));
        }

        mainScript.mainCamera = mainCamera;
        
        // Включить следование камеры за блоками
        mainScript.cameraFollowBlocks = true;
        mainScript.cameraFollowOffset = new Vector3(0, 4, -18); // Увеличено Z для новой глубины
        mainScript.cameraFollowSmoothness = 1.5f; // Более плавное движение
        
        // Настроить авто-корректировку камеры
        mainScript.autoAdjustCameraHeight = true;
        mainScript.cameraHeightPerBlockUnit = 0.4f;
        mainScript.cameraBackPerBlockUnit = 0.3f; // Увеличено для большего отъезда
        mainScript.minCameraDistance = -25f; // Увеличено минимальное расстояние
        
        // Установить начальную позицию
        if (mainScript.cameraPoses.Count > 0) {
            CameraPose firstPose = mainScript.cameraPoses[0];
            mainCamera.transform.position = firstPose.position;
            mainCamera.transform.rotation = firstPose.rotation;
        }

        Debug.Log("📷 Камера настроена (режим следования за блоками с авто-корректировкой)");
    }

    private CameraPose CreateCameraPose(Vector3 position, Quaternion rotation, float duration) {
        CameraPose pose = new CameraPose();
        pose.position = position;
        pose.rotation = rotation;
        pose.duration = duration;
        return pose;
    }

    private void SetupUI() {
        // Найти или создать Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) {
            canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Найти или создать тексты
        mainScript.levelTitleText = FindOrCreateText("LevelTitleText", canvasObj.transform, new Vector2(0, 250), 48);
        mainScript.totalSumText = FindOrCreateText("TotalSumText", canvasObj.transform, new Vector2(0, 200), 36);
        
        // Создать прогресс бар
        SetupProgressBar(canvasObj.transform);

        Debug.Log("📺 UI настроен");
    }

    private TextMeshProUGUI FindOrCreateText(string name, Transform parent, Vector2 position, int fontSize) {
        TextMeshProUGUI existing = GameObject.Find(name)?.GetComponent<TextMeshProUGUI>();
        if (existing != null) {
            return existing;
        }

        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        textObj.transform.localPosition = position;
        
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        if (rectTransform == null) {
            rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 80);
        }

        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = name.Replace("Text", "").ToUpper();
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;
        
        // Добавить Outline для читаемости
        textObj.AddComponent<Outline>();
        textObj.AddComponent<Shadow>();

        return textMesh;
    }

    private void SetupProgressBar(Transform parent) {
        GameObject progressObj = GameObject.Find("ProgressBar");
        if (progressObj == null) {
            progressObj = new GameObject("ProgressBar");
            progressObj.transform.SetParent(parent, false);
            progressObj.transform.localPosition = new Vector2(0, -250);
            
            RectTransform rect = progressObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 30);
            
            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(progressObj.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(progressObj.transform, false);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.4f, 0.8f, 0.4f, 1f);
            
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = new Vector2(400, 30);
            
            mainScript.progressBarObject = progressObj;
            mainScript.progressBarFill = fillRect;
        } else {
            mainScript.progressBarObject = progressObj;
            mainScript.progressBarFill = progressObj.transform.Find("Fill")?.GetComponent<RectTransform>();
        }
    }

    private void SetupEffects() {
        // Поиск префабов эффектов
        string[] effectGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Blocks" });
        
        foreach (string guid in effectGuids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.ToLower().Contains("mergeeffect")) {
                GameObject effect = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (mainScript.mergeEffectPrefab == null) {
                    mainScript.mergeEffectPrefab = effect;
                }
                if (mainScript.sparkleEffectPrefab == null) {
                    mainScript.sparkleEffectPrefab = effect;
                }
            }
        }

        Debug.Log("✨ Эффекты настроены");
    }

    private void CreateSceneFromScratch() {
        // Создать новую сцену
        UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        
        // Создать GameManager
        GameObject gameManager = new GameObject("GameManager");
        S69_Main mainScript = gameManager.AddComponent<S69_Main>();
        
        // Создать камеру
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cam = cameraObj.AddComponent<Camera>();
        cameraObj.tag = "MainCamera";
        
        // Настроить свет
        Light light = GameObject.Find("Directional Light")?.GetComponent<Light>();
        if (light == null) {
            GameObject lightObj = new GameObject("Directional Light");
            light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        Debug.Log("🎬 Сцена создана! Теперь нажмите 'AUTO-CONFIGURE SCENE'");
    }

    private void CheckConfiguration() {
        EditorGUILayout.LabelField("📋 Конфигурация:", EditorStyles.boldLabel);
        
        bool allConfigured = true;
        
        CheckField("Level 1 Blocks", mainScript.level1Blocks != null && mainScript.level1Blocks.Count > 0, ref allConfigured);
        CheckField("Level 2 Blocks", mainScript.level2Blocks != null && mainScript.level2Blocks.Count > 0, ref allConfigured);
        CheckField("Level 3 Blocks", mainScript.level3Blocks != null && mainScript.level3Blocks.Count > 0, ref allConfigured);
        CheckField("Level 4 Blocks", mainScript.level4Blocks != null && mainScript.level4Blocks.Count > 0, ref allConfigured);
        CheckField("100K Prefab", mainScript.hundredThousandPrefab != null, ref allConfigured);
        CheckField("1M Prefab", mainScript.millionPrefab != null, ref allConfigured);
        CheckField("Main Camera", mainScript.mainCamera != null, ref allConfigured);
        CheckField("Level Title UI", mainScript.levelTitleText != null, ref allConfigured);
        CheckField("Total Sum UI", mainScript.totalSumText != null, ref allConfigured);

        EditorGUILayout.Space(5);
        if (allConfigured) {
            EditorGUILayout.HelpBox("✅ Всё настроено! Можно запускать сцену.", MessageType.None);
        } else {
            EditorGUILayout.HelpBox("⚠️ Не всё настроено. Нажмите 'AUTO-CONFIGURE SCENE'", MessageType.Warning);
        }
    }

    private void CheckField(string label, bool isConfigured, ref bool allConfigured) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(150));
        EditorGUILayout.LabelField(isConfigured ? "✓" : "✗", GUILayout.Width(20));
        EditorGUILayout.EndHorizontal();
        
        if (!isConfigured) {
            allConfigured = false;
        }
    }

    private Texture2D MakeTexture(int width, int height, Color color) {
        Texture2D texture = new Texture2D(width, height);
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                texture.SetPixel(i, j, color);
            }
        }
        texture.Apply();
        return texture;
    }
}
