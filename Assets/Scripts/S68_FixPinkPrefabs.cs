#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Скрипт для удаления розовых префабов и создания новых
/// Запускается через Tools → S68 → Fix Pink Prefabs
/// </summary>
public class S68_FixPinkPrefabs : MonoBehaviour
{
    private static string prefabsPath = "Assets/Prefabs/Blocks/Scene68";
    
    [MenuItem("Tools/S68/Fix Pink Prefabs (Delete and Recreate)")]
    public static void FixPinkPrefabs() {
        Debug.Log("=== Исправление розовых префабов ===");
        
        if (!Directory.Exists(prefabsPath)) {
            Debug.Log("Папка с префабами не найдена. Сначала создай префабы.");
            return;
        }
        
        // Удаляем все префабы в папке
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsPath });
        int deletedCount = 0;
        
        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Проверяем, есть ли розовый материал
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) {
                Renderer renderer = prefab.GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null) {
                    // Удаляем материал если он розовый или битый
                    if (renderer.sharedMaterial.color == Color.magenta || 
                        renderer.sharedMaterial.shader == null) {
                        AssetDatabase.DeleteAsset(path);
                        deletedCount++;
                        Debug.Log($"  Удалён: {Path.GetFileName(path)}");
                    }
                }
            }
        }
        
        Debug.Log($"\nУдалено префабов: {deletedCount}");
        Debug.Log("\nТеперь запусти: Tools → S68 → Create Block Prefabs");
        
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Tools/S68/Delete All Scene68 Prefabs")]
    public static void DeleteAllPrefabs() {
        if (!Directory.Exists(prefabsPath)) {
            Debug.Log("Папка не найдена.");
            return;
        }
        
        if (!EditorUtility.DisplayDialog("Удалить все префабы?", 
            $"Это удалит все префабы из {prefabsPath}\n\nВы уверены?", 
            "Да, удалить", "Отмена")) {
            return;
        }
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsPath });
        int deletedCount = 0;
        
        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
            deletedCount++;
        }
        
        Debug.Log($"Удалено префабов: {deletedCount}");
        AssetDatabase.Refresh();
    }
}
#endif
