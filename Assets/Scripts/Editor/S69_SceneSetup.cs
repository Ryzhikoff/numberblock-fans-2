using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Утилита для быстрого создания и настройки Scene_69.
/// 
/// Использование:
/// 1. Открыть Scene_69.unity
/// 2. Menu → Tools → Scene_69 → Setup Scene Automatically
/// </summary>
public class S69_SceneSetup : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showInstructions = true;

    [MenuItem("Tools/Scene_69/Setup Scene Automatically")]
    public static void ShowWindow() {
        GetWindow<S69_SceneSetup>("Scene_69 Setup");
    }

    [MenuItem("Tools/Scene_69/Quick Setup (One Click)")]
    public static void QuickSetup() {
        Debug.Log("🚀 Начинаю быструю настройку Scene_69...");

        // Найти или создать GameManager
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager == null) {
            gameManager = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gameManager, "Create GameManager");
            Debug.Log("✅ Создан GameManager");
        }

        // Добавить или получить S69_Main
        S69_Main mainScript = gameManager.GetComponent<S69_Main>();
        if (mainScript == null) {
            mainScript = gameManager.AddComponent<S69_Main>();
            Undo.RegisterCompleteObjectUndo(gameManager, "Add S69_Main");
            Debug.Log("✅ Добавлен S69_Main");
        }

        // Найти редактор и вызвать авто-настройку
        Editor editor = Editor.CreateEditor(mainScript);
        var methodInfo = editor.GetType().GetMethod("AutoConfigureScene", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (methodInfo != null) {
            methodInfo.Invoke(editor, null);
        } else {
            Debug.LogWarning("⚠️ Не удалось найти метод AutoConfigureScene. Настройте вручную через инспектор.");
        }

        DestroyImmediate(editor);

        // Сохранить сцену
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("✅ Scene_69 готова к запуску! Нажмите Play.");
    }

    private void OnGUI() {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Заголовок
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 18;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🎬 Scene_69: Pyramid Setup", titleStyle);
        EditorGUILayout.Space(10);

        // Инструкция
        showInstructions = EditorGUILayout.Foldout(showInstructions, "📖 Instructions");
        if (showInstructions) {
            EditorGUILayout.HelpBox(
                "Этот инструмент поможет настроить сцену Scene_69 автоматически.\n\n" +
                "Шаги:\n" +
                "1. Убедитесь, что открыта сцена Scene_69.unity\n" +
                "2. Нажмите 'Quick Setup (One Click)' в меню Tools → Scene_69\n" +
                "3. Или используйте кнопку ниже для ручной настройки\n\n" +
                "Скрипт автоматически найдёт все префабы блоков и настроит сцену.",
                MessageType.Info);
        }

        EditorGUILayout.Space(15);

        // Кнопки
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.background = MakeTexture(20, 20, new Color(0.4f, 0.8f, 0.4f));

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("🛠️ Setup Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🚀 Quick Setup (One Click)", buttonStyle, GUILayout.Height(40))) {
            QuickSetup();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("📋 Open Setup Documentation", GUILayout.Height(30))) {
            string docPath = "Assets/Scenes/Scene_69_SETUP.md";
            if (System.IO.File.Exists(docPath)) {
                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(docPath, 1);
            } else {
                EditorUtility.DisplayDialog("Documentation Not Found", 
                    $"File not found: {docPath}", "OK");
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Проверка сцены
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("📊 Scene Status", EditorStyles.boldLabel);

        CheckSceneStatus();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Дополнительные опции
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("⚙️ Advanced", EditorStyles.boldLabel);

        if (GUILayout.Button("🗑️ Remove S69_Main Component")) {
            GameObject gm = GameObject.Find("GameManager");
            if (gm != null) {
                S69_Main comp = gm.GetComponent<S69_Main>();
                if (comp != null) {
                    Undo.DestroyObjectImmediate(comp);
                    Debug.Log("❌ S69_Main удалён из GameManager");
                }
            }
        }

        if (GUILayout.Button("🎬 Create New Scene from Template")) {
            CreateNewSceneFromTemplate();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
    }

    private void CheckSceneStatus() {
        GameObject gameManager = GameObject.Find("GameManager");
        S69_Main mainScript = gameManager != null ? gameManager.GetComponent<S69_Main>() : null;

        DrawStatusRow("GameManager exists", gameManager != null);
        DrawStatusRow("S69_Main component", mainScript != null);
        
        if (mainScript != null) {
            DrawStatusRow("Level 1 Blocks", mainScript.level1Blocks != null && mainScript.level1Blocks.Count > 0);
            DrawStatusRow("Level 2 Blocks", mainScript.level2Blocks != null && mainScript.level2Blocks.Count > 0);
            DrawStatusRow("Level 3 Blocks", mainScript.level3Blocks != null && mainScript.level3Blocks.Count > 0);
            DrawStatusRow("Level 4 Blocks", mainScript.level4Blocks != null && mainScript.level4Blocks.Count > 0);
            DrawStatusRow("100K Prefab", mainScript.hundredThousandPrefab != null);
            DrawStatusRow("1M Prefab", mainScript.millionPrefab != null);
            DrawStatusRow("Main Camera", mainScript.mainCamera != null);
        }

        EditorGUILayout.Space(5);
        
        bool allGood = gameManager != null && mainScript != null &&
                       mainScript.level1Blocks != null && mainScript.level1Blocks.Count > 0 &&
                       mainScript.level2Blocks != null && mainScript.level2Blocks.Count > 0 &&
                       mainScript.level3Blocks != null && mainScript.level3Blocks.Count > 0 &&
                       mainScript.level4Blocks != null && mainScript.level4Blocks.Count > 0 &&
                       mainScript.hundredThousandPrefab != null &&
                       mainScript.millionPrefab != null;

        if (allGood) {
            EditorGUILayout.HelpBox("✅ Scene is ready! Press Play to test.", MessageType.None);
        } else {
            EditorGUILayout.HelpBox("⚠️ Scene is not fully configured. Click 'Quick Setup' to auto-configure.", MessageType.Warning);
        }
    }

    private void DrawStatusRow(string label, bool status) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(200));
        EditorGUILayout.LabelField(status ? "✓" : "✗", GUILayout.Width(30));
        EditorGUILayout.EndHorizontal();
    }

    private void CreateNewSceneFromTemplate() {
        string scenePath = "Assets/Scenes/Scene_69.unity";
        
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            // Создать новую сцену
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            
            // Создать GameManager
            GameObject gameManager = new GameObject("GameManager");
            gameManager.AddComponent<S69_Main>();
            
            // Создать камеру
            GameObject cameraObj = new GameObject("Main Camera");
            Camera cam = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0, 3, -12);
            cameraObj.transform.rotation = Quaternion.Euler(15, 0, 0);
            
            // Создать свет
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            
            Debug.Log("✅ Создана новая сцена с базовой настройкой");
            Debug.Log("ℹ️ Теперь используйте 'Quick Setup' для автоматической настройки");
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
