#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Скрипт для автоматического назначения префабов в S68_Main
/// Запускается через Tools → S68 → Auto Assign Prefabs
/// </summary>
public class S68_AutoAssignPrefabs : MonoBehaviour
{
    private static string prefabsPath = "Assets/Prefabs/Blocks/Scene68";
    
    [MenuItem("Tools/S68/Auto Assign Prefabs to Scene_68")]
    public static void AutoAssignPrefabs() {
        Debug.Log("=== Автоматическое назначение префабов ===");
        
        // Проверяем, существует ли папка с префабами
        if (!Directory.Exists(prefabsPath)) {
            Debug.LogError($"Папка с префабами не найдена: {prefabsPath}");
            Debug.LogError("Сначала запусти: Tools → S68 → Create Block Prefabs");
            return;
        }
        
        // Находим S68_Main в сцене
        S68_Main main = Object.FindObjectOfType<S68_Main>();
        if (main == null) {
            Debug.LogError("S68_Main не найден! Открой Scene_68.unity и запусти Tools → Setup Scene 68");
            return;
        }
        
        // Получаем список всех префабов
        Dictionary<string, GameObject> prefabs = LoadAllPrefabs();
        
        if (prefabs.Count == 0) {
            Debug.LogError("Префабы не найдены! Создай их через Tools → S68 → Create Block Prefabs");
            return;
        }
        
        Debug.Log($"Найдено префабов: {prefabs.Count}");
        
        // Назначаем префабы для каждой эпохи
        AssignAncientPrefabs(main, prefabs);
        AssignMedievalPrefabs(main, prefabs);
        AssignIndustrialPrefabs(main, prefabs);
        AssignModernPrefabs(main, prefabs);
        AssignFuturePrefabs(main, prefabs);
        AssignEternityPrefabs(main, prefabs);
        
        // Сохраняем изменения
        EditorUtility.SetDirty(main);
        
        Debug.Log("=== Автоматическое назначение завершено! ===");
        Debug.Log("Теперь можно нажать Play ▶");
    }
    
    private static Dictionary<string, GameObject> LoadAllPrefabs() {
        Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsPath });
        
        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab != null) {
                string name = Path.GetFileNameWithoutExtension(path);
                prefabs[name] = prefab;
                Debug.Log($"  Загружен префаб: {name}");
            }
        }
        
        return prefabs;
    }
    
    #region Assign Functions
    
    private static void AssignAncientPrefabs(S68_Main main, Dictionary<string, GameObject> prefabs) {
        if (main.eraConfigs.Count < 1) return;
        
        Debug.Log("\n--- Настройка Ancient World (1-10) ---");
        
        List<GameObject> eraPrefabs = new List<GameObject>();
        
        for (int i = 1; i <= 10; i++) {
            string key = i.ToString();
            if (prefabs.ContainsKey(key)) {
                eraPrefabs.Add(prefabs[key]);
            } else {
                Debug.LogWarning($"  Префаб {key} не найден!");
            }
        }
        
        main.eraConfigs[0].blockPrefabs = eraPrefabs;
        main.eraConfigs[0].numbers = GetNumbersFromPrefabs(eraPrefabs);
        Debug.Log($"  Назначено префабов: {eraPrefabs.Count}/10");
    }
    
    private static void AssignMedievalPrefabs(S68_Main main, Dictionary<string, GameObject> prefabs) {
        if (main.eraConfigs.Count < 2) return;
        
        Debug.Log("\n--- Настройка Medieval Trade (10-100) ---");
        
        List<GameObject> eraPrefabs = new List<GameObject>();
        int[] values = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
        
        foreach (int value in values) {
            string key = value.ToString();
            if (prefabs.ContainsKey(key)) {
                eraPrefabs.Add(prefabs[key]);
            } else {
                Debug.LogWarning($"  Префаб {key} не найден!");
            }
        }
        
        main.eraConfigs[1].blockPrefabs = eraPrefabs;
        main.eraConfigs[1].numbers = GetNumbersFromPrefabs(eraPrefabs);
        Debug.Log($"  Назначено префабов: {eraPrefabs.Count}/10");
    }
    
    private static void AssignIndustrialPrefabs(S68_Main main, Dictionary<string, GameObject> prefabs) {
        if (main.eraConfigs.Count < 3) return;
        
        Debug.Log("\n--- Настройка Industrial Revolution (100-1000) ---");
        
        List<GameObject> eraPrefabs = new List<GameObject>();
        int[] values = { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 };
        
        foreach (int value in values) {
            string key = value.ToString();
            if (prefabs.ContainsKey(key)) {
                eraPrefabs.Add(prefabs[key]);
            } else {
                Debug.LogWarning($"  Префаб {key} не найден!");
            }
        }
        
        main.eraConfigs[2].blockPrefabs = eraPrefabs;
        main.eraConfigs[2].numbers = GetNumbersFromPrefabs(eraPrefabs);
        Debug.Log($"  Назначено префабов: {eraPrefabs.Count}/10");
    }
    
    private static void AssignModernPrefabs(S68_Main main, Dictionary<string, GameObject> prefabs) {
        if (main.eraConfigs.Count < 4) return;
        
        Debug.Log("\n--- Настройка Digital Age (1000-Million) ---");
        
        List<GameObject> eraPrefabs = new List<GameObject>();
        int[] values = { 1000, 2000, 5000, 10000, 100000, 1000000 };
        
        foreach (int value in values) {
            string key = value.ToString();
            if (prefabs.ContainsKey(key)) {
                eraPrefabs.Add(prefabs[key]);
            }
        }
        
        // Пробуем найти Million
        if (prefabs.ContainsKey("Million")) {
            eraPrefabs.Add(prefabs["Million"]);
        }
        
        main.eraConfigs[3].blockPrefabs = eraPrefabs;
        main.eraConfigs[3].numbers = GetNumbersFromPrefabs(eraPrefabs);
        Debug.Log($"  Назначено префабов: {eraPrefabs.Count}");
    }
    
    private static void AssignFuturePrefabs(S68_Main main, Dictionary<string, GameObject> prefabs) {
        if (main.eraConfigs.Count < 5) return;
        
        Debug.Log("\n--- Настройка Space Age (Billion-Trillion) ---");
        
        List<GameObject> eraPrefabs = new List<GameObject>();
        
        // HundredMillion
        if (prefabs.ContainsKey("HundredMillion")) {
            eraPrefabs.Add(prefabs["HundredMillion"]);
        }
        
        // Billion
        if (prefabs.ContainsKey("Billion")) {
            eraPrefabs.Add(prefabs["Billion"]);
        }
        
        // HundredBillion
        if (prefabs.ContainsKey("HundredBillion")) {
            eraPrefabs.Add(prefabs["HundredBillion"]);
        }
        
        // Trillion
        if (prefabs.ContainsKey("Trillion")) {
            eraPrefabs.Add(prefabs["Trillion"]);
        }
        
        main.eraConfigs[4].blockPrefabs = eraPrefabs;
        main.eraConfigs[4].numbers = GetNumbersFromPrefabs(eraPrefabs);
        Debug.Log($"  Назначено префабов: {eraPrefabs.Count}");
    }
    
    private static void AssignEternityPrefabs(S68_Main main, Dictionary<string, GameObject> prefabs) {
        if (main.eraConfigs.Count < 6) return;
        
        Debug.Log("\n--- Настройка Beyond Infinity (Centillion+) ---");
        
        List<GameObject> eraPrefabs = new List<GameObject>();
        
        // Centillion
        if (prefabs.ContainsKey("Centillion")) {
            eraPrefabs.Add(prefabs["Centillion"]);
        }
        
        // Googol
        if (prefabs.ContainsKey("Googol")) {
            eraPrefabs.Add(prefabs["Googol"]);
        }
        
        // Infinity
        if (prefabs.ContainsKey("Infinity")) {
            eraPrefabs.Add(prefabs["Infinity"]);
        }
        
        main.eraConfigs[5].blockPrefabs = eraPrefabs;
        main.eraConfigs[5].numbers = GetNumbersFromPrefabs(eraPrefabs);
        Debug.Log($"  Назначено префабов: {eraPrefabs.Count}");
    }
    
    #endregion
    
    private static List<int> GetNumbersFromPrefabs(List<GameObject> prefabs) {
        List<int> numbers = new List<int>();
        
        foreach (GameObject prefab in prefabs) {
            if (prefab != null) {
                Scale scaleComp = prefab.GetComponent<Scale>();
                if (scaleComp != null) {
                    // Вычисляем число из масштаба
                    long number = scaleComp.number;
                    numbers.Add(number > int.MaxValue ? int.MaxValue : (int)number);
                }
            }
        }
        
        return numbers;
    }
    
    [MenuItem("Tools/S68/Debug/Show Era Configs")]
    public static void DebugEraConfigs() {
        S68_Main main = Object.FindObjectOfType<S68_Main>();
        if (main == null) {
            Debug.LogError("S68_Main не найден!");
            return;
        }
        
        Debug.Log("=== Конфигурации эпох ===");
        
        for (int i = 0; i < main.eraConfigs.Count; i++) {
            Debug.Log($"\nЭпоха {i + 1}: {main.eraConfigs[i].eraName}");
            Debug.Log($"  Чисел: {main.eraConfigs[i].numbers.Count}");
            
            if (main.eraConfigs[i].numbers.Count > 0) {
                string numbers = "";
                int count = 0;
                foreach (int num in main.eraConfigs[i].numbers) {
                    if (count < 5) {
                        numbers += num + ", ";
                    }
                    count++;
                }
                if (main.eraConfigs[i].numbers.Count > 5) {
                    numbers += "...";
                }
                Debug.Log($"  Пример: [{numbers.TrimEnd(',', ' ')}]");
            }
        }
    }
}
#endif
