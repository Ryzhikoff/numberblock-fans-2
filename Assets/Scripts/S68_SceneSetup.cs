#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Редактор скрипт для автоматической настройки Scene_68
/// Добавляет меню Tools → Setup Scene 68
/// </summary>
public class S68_SceneSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Scene 68")]
    public static void SetupScene68() {
        Debug.Log("=== Настройка Scene 68 началась ===");
        
        // Находим GameManager по компоненту
        S68_Main main = Object.FindObjectOfType<S68_Main>();
        GameObject gameManager;
        
        if (main == null) {
            // Пробуем найти по имени
            gameManager = GameObject.Find("GameManager");
            if (gameManager == null) {
                // Создаём новый
                gameManager = new GameObject("GameManager");
                gameManager.transform.position = Vector3.zero;
                Debug.Log("Создан GameManager");
            }
            main = gameManager.AddComponent<S68_Main>();
            Debug.Log("Добавлен компонент S68_Main на GameManager");
        } else {
            gameManager = main.gameObject;
            Debug.Log("Найден GameManager с компонентом S68_Main");
        }
        
        // Находим EraManager
        S68_Era era = Object.FindObjectOfType<S68_Era>();
        GameObject eraManager;
        
        if (era == null) {
            eraManager = GameObject.Find("EraManager");
            if (eraManager == null) {
                eraManager = new GameObject("EraManager");
                eraManager.transform.position = Vector3.zero;
                Debug.Log("Создан EraManager");
            }
            era = eraManager.AddComponent<S68_Era>();
            Debug.Log("Добавлен компонент S68_Era на EraManager");
        } else {
            eraManager = era.gameObject;
            Debug.Log("Найден EraManager с компонентом S68_Era");
        }
        
        // Находим Narrator
        S68_Narrator narratorComp = Object.FindObjectOfType<S68_Narrator>();
        GameObject narrator;
        
        if (narratorComp == null) {
            narrator = GameObject.Find("Narrator");
            if (narrator == null) {
                narrator = new GameObject("Narrator");
                narrator.transform.position = Vector3.zero;
                Debug.Log("Создан Narrator");
            }
            narratorComp = narrator.AddComponent<S68_Narrator>();
            Debug.Log("Добавлен компонент S68_Narrator на Narrator");
        } else {
            narrator = narratorComp.gameObject;
            Debug.Log("Найден Narrator с компонентом S68_Narrator");
        }
        
        // Настраиваем ссылки
        if (main.mainCamera == null) {
            main.mainCamera = Camera.main;
        }
        
        if (main.blockSpawnPoint == null) {
            main.blockSpawnPoint = GameObject.Find("BlockSpawnPoint")?.transform;
        }
        if (main.portalSpawnPoint == null) {
            main.portalSpawnPoint = GameObject.Find("PortalSpawnPoint")?.transform;
        }
        
        if (main.blockSpawnPoint == null) {
            // Создаём точку спавна если не найдена
            GameObject spawnPoint = new GameObject("BlockSpawnPoint");
            spawnPoint.transform.position = new Vector3(0, 2, 0);
            main.blockSpawnPoint = spawnPoint.transform;
            Debug.Log("Создана точка спавна блоков");
        }
        if (main.portalSpawnPoint == null) {
            // Создаём точку спавна портала если не найдена
            GameObject portalPoint = new GameObject("PortalSpawnPoint");
            portalPoint.transform.position = new Vector3(-10, 2, 0);
            main.portalSpawnPoint = portalPoint.transform;
            Debug.Log("Создана точка спавна портала");
        }
        
        // Настраиваем EraManager
        if (era.mainController == null) {
            era.mainController = main;
        }
        
        // Создаём конфигурации эпох
        CreateEraConfigs(main);
        
        // Настраиваем повествование
        SetupNarratives(narratorComp);

        // Настраиваем UI ссылки
        SetupUI(main, narratorComp);

        Debug.Log("=== Настройка Scene 68 завершена! ===");
        Debug.Log("Теперь назначь префабы блоков в eraConfigs в инспекторе.");
        Debug.Log("Если UI тексты не назначены, выбери GameManager и назначь их вручную.");

        EditorUtility.SetDirty(main);
        EditorUtility.SetDirty(era);
        EditorUtility.SetDirty(narratorComp);
    }

    private static void SetupUI(S68_Main main, S68_Narrator narrator) {
        // Ищем Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) {
            Debug.LogWarning("Canvas не найден! UI тексты не будут назначены автоматически.");
            return;
        }

        // Ищем тексты по именам
        Transform canvasTransform = canvas.transform;
        
        if (main.eraTitleText == null) {
            Transform eraTitle = canvasTransform.Find("EraTitleText");
            if (eraTitle != null) {
                main.eraTitleText = eraTitle.GetComponent<TextMeshProUGUI>();
                Debug.Log("Найден EraTitleText");
            }
        }
        
        if (main.numberText == null) {
            Transform numberText = canvasTransform.Find("NumberText");
            if (numberText != null) {
                main.numberText = numberText.GetComponent<TextMeshProUGUI>();
                Debug.Log("Найден NumberText");
            }
        }
        
        if (main.eraDescriptionText == null) {
            Transform eraDesc = canvasTransform.Find("EraDescription");
            if (eraDesc != null) {
                main.eraDescriptionText = eraDesc.GetComponent<TextMeshProUGUI>();
                Debug.Log("Найден EraDescription");
            }
        }
        
        if (main.progressBarObject == null) {
            Transform progressBar = canvasTransform.Find("ProgressBar");
            if (progressBar != null) {
                main.progressBarObject = progressBar.gameObject;
                Debug.Log("Найден ProgressBar");
                
                // Ищем Fill
                Transform fill = progressBar.Find("ProgressBarFill");
                if (fill != null) {
                    main.progressBarFill = fill.GetComponent<RectTransform>();
                    Debug.Log("Найден ProgressBarFill");
                }
            }
        }
        
        // Настраиваем Narrator UI
        if (narrator.eraTitleText == null && main.eraTitleText != null) {
            narrator.eraTitleText = main.eraTitleText;
        }
    }
    
    private static void CreateEraConfigs(S68_Main main) {
        main.eraConfigs = new List<S68_EraConfig>();
        
        // Эпоха 1: Древний мир
        main.eraConfigs.Add(new S68_EraConfig {
            eraName = "Ancient World",
            description = "Numbers began with fingers and stones...",
            eraStyle = S68_EraStyle.Ancient,
            numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
            backgroundColor = new Color(0.6f, 0.4f, 0.2f),
            portalColor = new Color(0.8f, 0.5f, 0.2f)
        });
        
        // Эпоха 2: Средневековье
        main.eraConfigs.Add(new S68_EraConfig {
            eraName = "Medieval Trade",
            description = "Merchants counted coins in bustling markets...",
            eraStyle = S68_EraStyle.Medieval,
            numbers = new List<int> { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 },
            backgroundColor = new Color(0.8f, 0.7f, 0.3f),
            portalColor = new Color(1f, 1f, 0f)
        });
        
        // Эпоха 3: Индустриальная
        main.eraConfigs.Add(new S68_EraConfig {
            eraName = "Industrial Revolution",
            description = "Steam engines and factories multiplied production...",
            eraStyle = S68_EraStyle.Industrial,
            numbers = new List<int> { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000 },
            backgroundColor = new Color(0.4f, 0.3f, 0.2f),
            portalColor = new Color(1f, 0.5f, 0f)
        });
        
        // Эпоха 4: Современность
        main.eraConfigs.Add(new S68_EraConfig {
            eraName = "Digital Age",
            description = "Screens glow with data. Computers process millions...",
            eraStyle = S68_EraStyle.Modern,
            numbers = new List<int> { 1000, 2000, 5000, 10000, 100000, 1000000 },
            backgroundColor = new Color(0.2f, 0.4f, 0.6f),
            portalColor = new Color(0f, 1f, 1f)
        });
        
        // Эпоха 5: Будущее
        main.eraConfigs.Add(new S68_EraConfig {
            eraName = "Space Age",
            description = "Rockets pierce the sky. We count stars and planets...",
            eraStyle = S68_EraStyle.Future,
            numbers = new List<int> { 1000000, 100000000, 1000000000 },
            backgroundColor = new Color(0.1f, 0.1f, 0.3f),
            portalColor = new Color(1f, 0f, 1f)
        });
        
        // Эпоха 6: Вечность
        main.eraConfigs.Add(new S68_EraConfig {
            eraName = "Beyond Infinity",
            description = "Numbers transcend comprehension...",
            eraStyle = S68_EraStyle.Eternity,
            numbers = new List<int> { 1000000000 },
            backgroundColor = new Color(1f, 1f, 1f),
            portalColor = new Color(0.5f, 0f, 0.5f)
        });
        
        Debug.Log($"Создано {main.eraConfigs.Count} конфигураций эпох");
    }
    
    private static void SetupNarratives(S68_Narrator narrator) {
        narrator.ancientNarratives = NarrativePresets.GetAncientNarratives();
        narrator.medievalNarratives = NarrativePresets.GetMedievalNarratives();
        narrator.industrialNarratives = NarrativePresets.GetIndustrialNarratives();
        narrator.modernNarratives = NarrativePresets.GetModernNarratives();
        narrator.futureNarratives = NarrativePresets.GetFutureNarratives();
        narrator.eternityNarratives = NarrativePresets.GetEternityNarratives();
        
        Debug.Log("Настроено повествование для всех эпох");
    }
}
#endif
