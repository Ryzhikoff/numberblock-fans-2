#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Скрипт для добавления Main Camera на сцену если её нет
/// </summary>
public class S68_AddCamera : MonoBehaviour
{
    [MenuItem("Tools/S68/Add Main Camera If Missing")]
    public static void AddCameraIfMissing() {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null) {
            // Создаём камеру
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.transform.position = new Vector3(0, 8, -25);
            cameraObj.transform.rotation = Quaternion.Euler(15, 0, 0);
            cameraObj.tag = "MainCamera";
            
            Camera cam = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            
            Debug.Log("✅ Main Camera добавлена на сцену!");
            Debug.Log($"   Позиция: {cameraObj.transform.position}");
            Debug.Log($"   Поворот: {cameraObj.transform.rotation.eulerAngles}");
            
            // Выделяем камеру
            Selection.activeGameObject = cameraObj;
        } else {
            Debug.Log($"✅ Main Camera уже существует: {mainCamera.gameObject.name}");
            Debug.Log($"   Позиция: {mainCamera.transform.position}");
        }
    }
}
#endif
