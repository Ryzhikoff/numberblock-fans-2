#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class S70_AutoSetup : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showLog = true;
    private System.Text.StringBuilder logMessages = new System.Text.StringBuilder();

    [MenuItem("Tools/Scene 70 - Авто Настройка")]
    public static void ShowWindow()
    {
        GetWindow<S70_AutoSetup>("Авто Настройка Scene 70");
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Space(10);
        GUILayout.Label("🎬 АВТО НАСТРОЙКА SCENE 70", EditorStyles.boldLabel);
        GUILayout.Label("От 1 до ТРИЛЛИОНА - Космическое Путешествие", EditorStyles.miniLabel);
        
        GUILayout.Space(10);
        
        // Информация
        EditorGUILayout.HelpBox(
            "Этот инструмент автоматически настроит всю сцену:\n" +
            "• Создаст все необходимые GameObject\n" +
            "• Добавит и настроит скрипты\n" +
            "• Найдёт и назначит префабы блоков\n" +
            "• Создаст UI с TextMeshPro\n" +
            "• Настроит камеру и освещение\n\n" +
            "Просто нажмите кнопку ниже!",
            MessageType.Info
        );

        GUILayout.Space(10);

        // Кнопка настройки
        GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
        if (GUILayout.Button("🚀 АВТОМАТИЧЕСКИ НАСТРОИТЬ СЦЕНУ", GUILayout.Height(50)))
        {
            SetupScene();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        // Кнопка очистки
        GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
        if (GUILayout.Button("🗑️ ОЧИСТИТЬ СЦЕНУ", GUILayout.Height(30)))
        {
            ClearScene();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);

        // Лог
        if (showLog && logMessages.Length > 0)
        {
            GUILayout.Label("📋 ЛОГ НАСТРОЙКИ:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(logMessages.ToString(), MessageType.None);
        }

        EditorGUILayout.EndScrollView();
    }

    void ClearLog()
    {
        logMessages.Clear();
    }

    void Log(string message)
    {
        logMessages.AppendLine(message);
        Debug.Log(message);
    }

    void SetupScene()
    {
        ClearLog();
        Log("=== НАЧАЛО НАСТРОЙКИ СЦЕНЫ ===\n");

        try
        {
            // 1. Находим или создаём контейнер
            GameObject blocksContainer = FindOrCreateObject("BlocksContainer");
            Log("✅ Создан/найден BlocksContainer");

            // 2. Настраиваем камеру
            SetupCamera();
            Log("✅ Настроена Main Camera");

            // 3. Создаём UI
            GameObject uiObjects = SetupUI();
            Log("✅ Создан UI Canvas с элементами");

            // 4. Создаём Starfield
            GameObject starfield = FindOrCreateObject("Starfield");
            SetupStarfield(starfield);
            Log("✅ Настроен Starfield");

            // 5. Создаём SceneManager
            GameObject sceneManager = FindOrCreateObject("SceneManager");
            SetupSceneManager(sceneManager, blocksContainer, uiObjects);
            Log("✅ Настроен SceneManager");

            // 6. Находим и назначаем префабы
            SetupBlockPrefabs(blocksContainer);
            Log("✅ Найдены и назначены префабы блоков");

            // 7. Настраиваем освещение
            SetupLighting();
            Log("✅ Настроено освещение");

            // 8. Сохраняем сцену
            Log("\n=== НАСТРОЙКА ЗАВЕРШЕНА ===");
            Log("🎉 ВСЁ ГОТОВО! Можете запускать сцену!");
            Log("\nУправление:");
            Log("  Space - Вкл/Выкл автопилот камеры");
            Log("  WASD + Мышь - Ручное управление");
            Log("  R - Перезапуск сцены");
            Log("  Esc - Выход");

            EditorUtility.DisplayDialog("Настройка завершена!", 
                "Сцена успешно настроена!\n\n" +
                "Нажмите Play для запуска.\n" +
                "Space - переключение автопилота\n" +
                "WASD - ручное управление", 
                "Отлично!");
        }
        catch (System.Exception e)
        {
            Log("❌ ОШИБКА: " + e.Message);
            EditorUtility.DisplayDialog("Ошибка", 
                "Произошла ошибка при настройке:\n" + e.Message, 
                "OK");
        }

        Repaint();
    }

    GameObject FindOrCreateObject(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
        }
        return obj;
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            mainCamera.tag = "MainCamera";
            cameraObj.AddComponent<AudioListener>();
        }

        // Добавляем скрипт полёта
        S70_CameraFlyPath flyPath = mainCamera.GetComponent<S70_CameraFlyPath>();
        if (flyPath == null)
        {
            flyPath = mainCamera.gameObject.AddComponent<S70_CameraFlyPath>();
        }

        // Настраиваем параметры
        mainCamera.farClipPlane = 10000f;
        mainCamera.fieldOfView = 60f;

        flyPath.autoFly = true;
        flyPath.autoGeneratePath = true;
        flyPath.pathPointCount = 100;
        flyPath.pathRadius = 300f;
        flyPath.pathHeight = 300f;
        flyPath.pathSpirals = 5f;
        flyPath.flySpeed = 2f;
        flyPath.smoothTime = 0.8f;

        // Позиция камеры - начинаем с хорошего ракурса
        mainCamera.transform.position = new Vector3(100, 50, -150);
        mainCamera.transform.LookAt(Vector3.zero);
    }

    GameObject SetupUI()
    {
        // Ищем или создаём Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Создаём контейнер для UI
        GameObject uiContainer = FindOrCreateObject("UI_Container");
        uiContainer.transform.SetParent(canvas.transform, false);

        // Создаём Number Text
        GameObject numberTextObj = CreateTextObject("NumberText", 
            new Vector2(0, 100), 
            72, 
            TextAlignmentOptions.Center,
            new Color(1f, 0.9f, 0.3f));
        numberTextObj.transform.SetParent(uiContainer.transform, false);

        // Создаём Name Text
        GameObject nameTextObj = CreateTextObject("NameText", 
            new Vector2(0, 20), 
            48, 
            TextAlignmentOptions.Center,
            Color.white);
        nameTextObj.transform.SetParent(uiContainer.transform, false);

        // Создаём Description Text
        GameObject descTextObj = CreateTextObject("DescriptionText", 
            new Vector2(0, -40), 
            32, 
            TextAlignmentOptions.Center,
            new Color(0.8f, 0.8f, 0.8f));
        descTextObj.transform.SetParent(uiContainer.transform, false);

        // Добавляем скрипт отображения
        S70_NumberDisplay display = uiContainer.GetComponent<S70_NumberDisplay>();
        if (display == null)
        {
            display = uiContainer.AddComponent<S70_NumberDisplay>();
        }

        // Назначаем текстовые поля
        display.numberText = numberTextObj.GetComponent<TextMeshProUGUI>();
        display.nameText = nameTextObj.GetComponent<TextMeshProUGUI>();
        display.descriptionText = descTextObj.GetComponent<TextMeshProUGUI>();

        return uiContainer;
    }

    GameObject CreateTextObject(string name, Vector2 position, int fontSize, 
        TextAlignmentOptions alignment, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.AddComponent<RectTransform>();

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.font = GetTMPFont();
        text.text = name;

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(800, 100);

        return textObj;
    }

    TMP_FontAsset GetTMPFont()
    {
        // Ищем стандартный шрифт TMP
        string[] guids = AssetDatabase.FindAssets("LiberationSans SDF");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }

        // Если не нашли, ищем любой SDF шрифт
        guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }

        return null;
    }

    void SetupBlockPrefabs(GameObject blocksContainer)
    {
        S70_TrillionBlocks blockGen = blocksContainer.GetComponent<S70_TrillionBlocks>();
        if (blockGen == null)
        {
            blockGen = blocksContainer.AddComponent<S70_TrillionBlocks>();
        }

        // Настраиваем контейнер
        blockGen.container = blocksContainer.transform;

        // Ищем префабы
        blockGen.prefabOne = FindPrefab("1", "One");
        blockGen.prefabTen = FindPrefab("10");
        blockGen.prefabHundred = FindPrefab("100");
        blockGen.prefabThousand = FindPrefab("1000", "Thousand");
        blockGen.prefabMillion = FindPrefab("OneMillion", "Million");
        blockGen.prefabBillion = blockGen.prefabMillion; // Используем тот же
        blockGen.prefabTrillion = blockGen.prefabMillion; // Используем тот же

        // Настройки генерации
        blockGen.spacing = 10f;
        blockGen.spiralRadius = 100f;
        blockGen.spiralHeight = 20f;
        blockGen.blocksPerSpiral = 50;

        // LOD настройки
        blockGen.lodDistance1 = 150f;
        blockGen.lodDistance2 = 600f;
        blockGen.lodDistance3 = 2500f;

        // Масштаб блоков
        blockGen.transform.localScale = Vector3.one;
    }

    GameObject FindPrefab(params string[] names)
    {
        foreach (string name in names)
        {
            // Ищем в Assets
            string[] guids = AssetDatabase.FindAssets(name + " t:Prefab");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Log("  Найдено префаб: " + path);
                return prefab;
            }
        }

        Log("  ⚠️ Префаб не найден для: " + string.Join(", ", names));
        return null;
    }

    void SetupStarfield(GameObject starfield)
    {
        S70_Starfield star = starfield.GetComponent<S70_Starfield>();
        if (star == null)
        {
            star = starfield.AddComponent<S70_Starfield>();
        }

        star.starCount = 3000;
        star.starFieldRadius = 4000f;
        star.nebulaCount = 15;
        star.particleCount = 500;
    }

    void SetupSceneManager(GameObject sceneManager, GameObject blocks, GameObject ui)
    {
        S70_SceneManager manager = sceneManager.GetComponent<S70_SceneManager>();
        if (manager == null)
        {
            manager = sceneManager.AddComponent<S70_SceneManager>();
        }

        // Назначаем ссылки
        manager.blockGenerator = blocks.GetComponent<S70_TrillionBlocks>();
        manager.cameraFly = Camera.main.GetComponent<S70_CameraFlyPath>();
        manager.numberDisplay = ui.GetComponent<S70_NumberDisplay>();
        manager.starfield = GameObject.Find("Starfield").GetComponent<S70_Starfield>();

        // Настройки
        manager.ambientLightColor = new Color(0.2f, 0.2f, 0.4f);
        manager.lightIntensity = 1.5f;
    }

    void SetupLighting()
    {
        Light light = FindObjectOfType<Light>();
        if (light == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
        }

        light.intensity = 1.5f;
        light.color = Color.white;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Настраиваем окружение
        RenderSettings.ambientLight = new Color(0.2f, 0.2f, 0.4f);
        RenderSettings.fog = false;
    }

    void ClearScene()
    {
        if (!EditorUtility.DisplayDialog("Очистка сцены", 
            "Вы уверены, что хотите удалить все созданные объекты?\n\n" +
            "Это действие нельзя отменить!", 
            "Да, очистить", "Отмена"))
        {
            return;
        }

        string[] objectsToDelete = new string[]
        {
            "BlocksContainer",
            "UI_Container",
            "Canvas",
            "Starfield",
            "SceneManager"
        };

        foreach (string name in objectsToDelete)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Undo.DestroyObjectImmediate(obj);
            }
        }

        Log("🗑️ Сцена очищена");
        Repaint();
    }
}
#endif
