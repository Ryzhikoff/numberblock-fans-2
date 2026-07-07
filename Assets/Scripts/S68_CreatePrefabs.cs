#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Скрипт для создания префабов блоков для Scene_68
/// Запускается через Tools → S68 → Create Block Prefabs
/// </summary>
public class S68_CreatePrefabs : MonoBehaviour
{
    private static string prefabsBasePath = "Assets/Prefabs/Blocks";
    private static string scene68Path = "Assets/Prefabs/Blocks/Scene68";
    
    [MenuItem("Tools/S68/Create Block Prefabs")]
    public static void CreateAllPrefabs() {
        Debug.Log("=== Создание префабов блоков для Scene_68 ===");
        
        // Создаём папки
        CreateFolders();
        
        // Создаём префабы для каждой эпохи
        CreateAncientPrefabs();      // 1-10
        CreateMedievalPrefabs();     // 10-100
        CreateIndustrialPrefabs();   // 100-1000
        CreateModernPrefabs();       // 1000-Million
        CreateFuturePrefabs();       // Billion-Trillion
        CreateEternityPrefabs();     // Centillion
        
        Debug.Log("=== Все префабы созданы! ===");
        Debug.Log($"Путь: {scene68Path}");
        
        AssetDatabase.Refresh();
    }
    
    private static void CreateFolders() {
        if (!Directory.Exists(prefabsBasePath)) {
            Directory.CreateDirectory(prefabsBasePath);
            Debug.Log($"Создана папка: {prefabsBasePath}");
        }
        
        if (!Directory.Exists(scene68Path)) {
            Directory.CreateDirectory(scene68Path);
            Debug.Log($"Создана папка: {scene68Path}");
        }
    }
    
    #region Ancient Era (1-10)
    private static void CreateAncientPrefabs() {
        Debug.Log("\n--- Создание префабов Древней эпохи (1-10) ---");
        
        for (int i = 1; i <= 10; i++) {
            CreateBlockPrefab(i, i, new Color(0.8f, 0.5f, 0.2f), "Ancient");
        }
    }
    #endregion
    
    #region Medieval Era (10-100)
    private static void CreateMedievalPrefabs() {
        Debug.Log("\n--- Создание префабов Средневековой эпохи (10-100) ---");
        
        int[] values = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        foreach (int value in values) {
            CreateBlockPrefab(value, value, new Color(0.9f, 0.8f, 0.2f), "Medieval");
        }
    }
    #endregion
    
    #region Industrial Era (100-1000)
    private static void CreateIndustrialPrefabs() {
        Debug.Log("\n--- Создание префабов Индустриальной эпохи (100-1000) ---");
        
        int[] values = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
        foreach (int value in values) {
            CreateBlockPrefab(value, value, new Color(0.6f, 0.4f, 0.3f), "Industrial");
        }
    }
    #endregion
    
    #region Modern Era (1000-Million)
    private static void CreateModernPrefabs() {
        Debug.Log("\n--- Создание префабов Современной эпохи (1000-Million) ---");
        
        CreateBlockPrefab(1000, 1000, new Color(0.2f, 0.6f, 0.9f), "Modern");
        CreateBlockPrefab(2000, 2000, new Color(0.2f, 0.7f, 0.8f), "Modern");
        CreateBlockPrefab(5000, 5000, new Color(0.3f, 0.7f, 0.8f), "Modern");
        CreateBlockPrefab(10000, 10000, new Color(0.2f, 0.8f, 0.7f), "Modern");
        CreateBlockPrefab(100000, 100000, new Color(0.3f, 0.8f, 0.7f), "Modern");
        CreateBlockPrefab(1000000, 1000000, new Color(0.2f, 0.9f, 0.6f), "Modern", "Million");
    }
    #endregion
    
    #region Future Era (Billion-Trillion)
    private static void CreateFuturePrefabs() {
        Debug.Log("\n--- Создание префабов Будущего (Billion-Trillion) ---");
        
        CreateBlockPrefab(100000000, 100000000, new Color(0.6f, 0.2f, 0.9f), "Future", "HundredMillion");
        CreateBlockPrefab(1000000000, 1000000000, new Color(0.7f, 0.3f, 0.9f), "Future", "Billion");
        // Используем long для очень больших чисел
        CreateBlockPrefabLong(100000000000L, 100000000000L, new Color(0.8f, 0.4f, 0.9f), "Future", "HundredBillion");
        CreateBlockPrefabLong(1000000000000L, 1000000000000L, new Color(0.9f, 0.5f, 0.9f), "Future", "Trillion");
    }
    #endregion
    
    #region Eternity Era (Centillion+)
    private static void CreateEternityPrefabs() {
        Debug.Log("\n--- Создание префабов Вечности (Centillion+) ---");
        
        CreateBlockPrefab(1000000000, 1000000000, new Color(1f, 0.8f, 0.5f), "Eternity", "Centillion");
        CreateBlockPrefab(1000000000, 1000000000, new Color(0.8f, 1f, 0.8f), "Eternity", "Googol");
        CreateBlockPrefab(1000000000, 1000000000, new Color(1f, 1f, 0.8f), "Eternity", "Infinity");
    }
    #endregion
    
    private static void CreateBlockPrefab(int numberValue, int displayNumber, Color color, string era, string specialName = null) {
        string prefabName = specialName != null ? specialName : numberValue.ToString();
        string prefabPath = $"{scene68Path}/{prefabName}.prefab";
        
        // Проверяем, существует ли уже префаб
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) {
            Debug.Log($"  ⊘ Пропущен: {prefabName} (уже существует)");
            return;
        }
        
        // Создаём GameObject
        GameObject blockObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blockObj.name = $"Block_{prefabName}";
        
        // Настраиваем размер в зависимости от числа
        float scale = GetScaleForNumber(numberValue);
        blockObj.transform.localScale = new Vector3(scale, scale, scale);
        
        // Настраиваем материал с цветом эпохи
        Renderer renderer = blockObj.GetComponent<Renderer>();
        SetRendererColor(renderer, color);
        
        // Добавляем компонент Scale
        Scale scaleComp = blockObj.AddComponent<Scale>();
        scaleComp.scale = blockObj.transform.localScale;
        
        // Добавляем Rigidbody (кинематический)
        Rigidbody rb = blockObj.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = blockObj.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // Добавляем NameBlock если нет
        NameBlock nameBlock = blockObj.GetComponent<NameBlock>();
        if (nameBlock == null) {
            nameBlock = blockObj.AddComponent<NameBlock>();
        }
        nameBlock.text = displayNumber >= 1000000 ? (displayNumber / 1000000f).ToString("F1") + "M" : displayNumber.ToString();
        nameBlock.textSize = 40;
        nameBlock.textColor = new Color(0.1215f, 0.1568f, 0.2117f);
        
        // Сохраняем как префаб
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(blockObj, prefabPath);
        
        // Удаляем объект из сцены
        DestroyImmediate(blockObj);
        
        Debug.Log($"  ✓ Создан: {prefabName} (scale: {scale}, color: {color})");
    }
    
    private static void SetRendererColor(Renderer renderer, Color color) {
        if (renderer == null) return;
        
        // Пробуем создать материал со стандартным шейдером
        Material material = new Material(Shader.Find("Standard"));
        
        // Если не получилось, пробуем другие шейдеры
        if (material == null || material.shader == null) {
            material = new Material(Shader.Find("Legacy Shaders/Diffuse"));
        }
        if (material == null || material.shader == null) {
            material = new Material(Shader.Find("Unlit/Color"));
        }
        
        // Если шейдер найден, назначаем цвет
        if (material != null && material.shader != null) {
            material.color = color;
            renderer.sharedMaterial = material;
        } else {
            // Если шейдер не найден, используем встроенный материал
            // Получаем стандартный материал Unity
            Material defaultMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            if (defaultMaterial != null && defaultMaterial.shader != null) {
                defaultMaterial.color = color;
                renderer.sharedMaterial = defaultMaterial;
            } else {
                // Последняя попытка - используем материал по умолчанию
                renderer.material.color = color;
            }
        }
    }
    
    private static float GetScaleForNumber(int number) {
        // Базовый масштаб для 1 = 1
        // Для больших чисел уменьшаем масштаб
        if (number <= 10) return 1f;
        if (number <= 100) return 2f;
        if (number <= 1000) return 3f;
        if (number <= 10000) return 4f;
        if (number <= 100000) return 5f;
        if (number <= 1000000) return 6f;
        if (number <= 100000000) return 7f;
        if (number <= 1000000000) return 8f;
        return 10f;
    }
    
    private static void CreateBlockPrefabLong(long numberValue, long displayNumber, Color color, string era, string specialName = null) {
        string prefabName = specialName != null ? specialName : numberValue.ToString();
        string prefabPath = $"{scene68Path}/{prefabName}.prefab";
        
        // Проверяем, существует ли уже префаб
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) {
            Debug.Log($"  ⊘ Пропущен: {prefabName} (уже существует)");
            return;
        }
        
        // Создаём GameObject
        GameObject blockObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        blockObj.name = $"Block_{prefabName}";
        
        // Настраиваем размер в зависимости от числа
        float scale = GetScaleForNumberLong(numberValue);
        blockObj.transform.localScale = new Vector3(scale, scale, scale);
        
        // Добавляем материал с цветом эпохи
        Renderer renderer = blockObj.GetComponent<Renderer>();
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        renderer.material = material;
        
        // Добавляем компонент Scale
        Scale scaleComp = blockObj.AddComponent<Scale>();
        scaleComp.scale = blockObj.transform.localScale;
        
        // Добавляем Rigidbody (кинематический)
        Rigidbody rb = blockObj.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = blockObj.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // Добавляем NameBlock если нет
        NameBlock nameBlock = blockObj.GetComponent<NameBlock>();
        if (nameBlock == null) {
            nameBlock = blockObj.AddComponent<NameBlock>();
        }
        nameBlock.text = FormatNumber(displayNumber);
        nameBlock.textSize = 40;
        nameBlock.textColor = new Color(0.1215f, 0.1568f, 0.2117f);
        
        // Сохраняем как префаб
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(blockObj, prefabPath);
        
        // Удаляем объект из сцены
        DestroyImmediate(blockObj);
        
        Debug.Log($"  ✓ Создан: {prefabName} (scale: {scale}, color: {color})");
    }
    
    private static float GetScaleForNumberLong(long number) {
        if (number <= 10) return 1f;
        if (number <= 100) return 2f;
        if (number <= 1000) return 3f;
        if (number <= 10000) return 4f;
        if (number <= 100000) return 5f;
        if (number <= 1000000) return 6f;
        if (number <= 100000000) return 7f;
        if (number <= 1000000000) return 8f;
        return 10f;
    }
    
    private static string FormatNumber(long number) {
        if (number >= 1000000000000) return (number / 1000000000000f).ToString("F1") + "T";
        if (number >= 1000000000) return (number / 1000000000f).ToString("F1") + "B";
        if (number >= 1000000) return (number / 1000000f).ToString("F1") + "M";
        if (number >= 1000) return (number / 1000f).ToString("F0") + "K";
        return number.ToString();
    }
    
    private static Material CreateColoredMaterial(Color color) {
        // Создаём материал используя встроенный шейдер
        // В Unity 2018+ используем Standard, в более старых - другой подход
        Material material = new Material(Shader.Find("Standard"));
        
        // Если Standard не найден, пробуем другие варианты
        if (material == null || material.shader == null) {
            // Пробуем найти любой доступный шейдер
            string[] shaderNames = {
                "Standard",
                "Legacy Shaders/Diffuse",
                "Unlit/Color",
                "Sprites/Default",
                "GUI/Text Shader"
            };
            
            foreach (string shaderName in shaderNames) {
                Shader shader = Shader.Find(shaderName);
                if (shader != null) {
                    material = new Material(shader);
                    break;
                }
            }
        }
        
        // Если всё ещё не нашли, создаём через Built-in Render Pipeline
        if (material == null || material.shader == null) {
            // Создаём простой цветной материал
            material = new Material(Shader.Find("Hidden/InternalErrorShader"));
            if (material == null || material.shader == null) {
                // Последняя попытка - создаём материал без шейдера (будет белый)
                material = new Material((Shader)null);
            }
        }
        
        material.color = color;
        return material;
    }
}
#endif
