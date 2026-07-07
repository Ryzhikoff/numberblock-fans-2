using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Главный контроллер сцены "История Чисел" (Scene_68).
/// Управляет переходами между эпохами: Древний мир → Средневековье → Индустриальная → 
/// Современность → Будущее → Вечность
/// </summary>
public class S68_Main : MonoBehaviour
{
    [Header("Конфигурация эпох")]
    public List<S68_EraConfig> eraConfigs; // 6 эпох

    [Header("Позиции спавна")]
    public Transform blockSpawnPoint;
    public Transform portalSpawnPoint;

    [Header("Камера")]
    public Camera mainCamera;
    public float cameraSpeed = 2f;
    public List<Vector3> cameraPositions; // Позиции для каждой эпохи
    public List<Vector3> cameraRotations; // Повороты для каждой эпохи
    private int currentCameraIndex = 0;

    [Header("UI")]
    public TextMeshProUGUI eraTitleText;
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI eraDescriptionText;
    public GameObject progressBarObject;
    public RectTransform progressBarFill;

    [Header("Звуки")]
    public AudioClip eraTransitionSound;
    public List<AudioClip> backgroundMusic; // Музыка для каждой эпохи
    public List<AudioClip> numberSounds; // Звуки чисел
    private AudioSource audioSource;

    [Header("Эффекты")]
    public GameObject portalPrefab;
    public GameObject transitionEffectPrefab;

    [Header("Настройки тайминга")]
    public float eraDuration = 15f; // Длительность одной эпохи
    public float transitionDuration = 3f; // Длительность перехода
    public float delayBeforeStart = 2f; // Задержка перед стартом

    private int currentEraIndex = 0;
    private GameObject currentBlock;
    private GameObject currentPortal;
    private bool isTransitioning = false;
    private float eraTimer = 0f;
    private bool isSceneFinished = false;

    private void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.volume = 0.7f;

        // Скрыть UI переходов
        if (eraDescriptionText != null) {
            eraDescriptionText.gameObject.SetActive(false);
        }

        // Запуск первой эпохи с задержкой
        Invoke("StartFirstEra", delayBeforeStart);
    }

    private void StartFirstEra() {
        currentEraIndex = 0;
        StartEra();
    }

    /// <summary>
    /// Начать новую эпоху
    /// </summary>
    public void StartEra() {
        if (currentEraIndex >= eraConfigs.Count) {
            // Все эпохи завершены - показываем финал
            ShowFinale();
            return;
        }

        isTransitioning = false;
        eraTimer = 0f;

        // Очистить предыдущие объекты
        ClearPreviousObjects();

        // Получить конфигурацию текущей эпохи
        S68_EraConfig config = eraConfigs[currentEraIndex];

        // Обновить UI
        UpdateUI(config);

        // Применить настройки эпохи (небо, окружение)
        ApplyEraSettings(config);

        // Спавн числа
        SpawnNumber(config);

        // Воспроизвести музыку эпохи
        PlayEraMusic(config);

        // Запустить таймер перехода
        eraTimer = eraDuration;
    }

    private void SpawnNumber(S68_EraConfig config) {
        if (config.numbers.Count == 0) return;

        // Берём случайное число из списка эпохи
        int numberValue = config.numbers[Random.Range(0, config.numbers.Count)];
        GameObject numberPrefab = GetNumberPrefabFromConfig(config, numberValue);

        if (numberPrefab != null && blockSpawnPoint != null) {
            currentBlock = Instantiate(numberPrefab, blockSpawnPoint.position, Quaternion.identity);
            currentBlock.name = $"Number_{config.eraName}_{numberValue}";

            // Обновить текст числа
            if (numberText != null) {
                numberText.text = FormatNumber(numberValue);
            }
        }
    }

    private GameObject GetNumberPrefabFromConfig(S68_EraConfig config, int value) {
        // Сначала ищем в префабах эпохи
        if (config.blockPrefabs != null && config.blockPrefabs.Count > 0) {
            // Ищем префаб с нужным числом
            foreach (GameObject blockPrefab in config.blockPrefabs) {
                if (blockPrefab != null) {
                    Scale scaleComp = blockPrefab.GetComponent<Scale>();
                    if (scaleComp != null && scaleComp.number == value) {
                        return blockPrefab;
                    }
                }
            }
            // Если не нашли точное совпадение, берём первый доступный
            return config.blockPrefabs[Random.Range(0, config.blockPrefabs.Count)];
        }

        // Если префабов нет в конфиге, ищем в Resources
        string prefabName = value.ToString();
        GameObject foundPrefab = Resources.Load<GameObject>("Prefabs/" + prefabName);

        if (foundPrefab == null) {
            // Создаём заглушку
            Debug.LogWarning($"Префаб {prefabName} не найден, создаём заглушку");
            foundPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            foundPrefab.name = $"Block_{prefabName}_Placeholder";
            
            Scale scaleComp = foundPrefab.AddComponent<Scale>();
            scaleComp.scale = Vector3.one * GetScaleForNumber(value);
            
            Rigidbody rb = foundPrefab.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        return foundPrefab;
    }

    private float GetScaleForNumber(int number) {
        if (number <= 10) return 1f;
        if (number <= 100) return 2f;
        if (number <= 1000) return 3f;
        if (number <= 10000) return 4f;
        return 5f;
    }

    private void ApplyEraSettings(S68_EraConfig config) {
        // Смена Skybox
        if (config.skybox != null) {
            RenderSettings.skybox = config.skybox;
        }

        // Смена освещения
        if (config.ambientColor.a > 0f) {
            RenderSettings.ambientLight = config.ambientColor;
        }

        // Смена фона камеры
        if (mainCamera != null && config.skybox == null) {
            mainCamera.backgroundColor = config.backgroundColor;
        }
    }

    private void PlayEraMusic(S68_EraConfig config) {
        if (currentEraIndex < backgroundMusic.Count && backgroundMusic[currentEraIndex] != null) {
            audioSource.clip = backgroundMusic[currentEraIndex];
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void UpdateUI(S68_EraConfig config) {
        if (eraTitleText != null) {
            eraTitleText.text = config.eraName.ToUpper();
        }

        if (eraDescriptionText != null) {
            eraDescriptionText.text = config.description;
            eraDescriptionText.gameObject.SetActive(true);
            Invoke("HideDescription", 3f);
        }

        // Обновить прогресс бар
        UpdateProgressBar();
    }

    private void HideDescription() {
        if (eraDescriptionText != null) {
            eraDescriptionText.gameObject.SetActive(false);
        }
    }

    private void UpdateProgressBar() {
        if (progressBarFill != null && eraConfigs.Count > 0) {
            float progress = (float)(currentEraIndex + 1) / eraConfigs.Count;
            progressBarFill.localScale = new Vector3(progress, 1f, 1f);
        }
    }

    private void Update() {
        if (isTransitioning || isSceneFinished) return;

        // Таймер эпохи
        eraTimer -= Time.deltaTime;

        if (eraTimer <= 0f && !isTransitioning) {
            // Время эпохи вышло - начинаем переход
            StartTransition();
        }

        // Плавное движение камеры
        UpdateCamera();
    }

    private void StartTransition() {
        isTransitioning = true;

        // Воспроизвести звук перехода
        if (eraTransitionSound != null) {
            audioSource.PlayOneShot(eraTransitionSound);
        }

        // Создать портал
        if (portalPrefab != null && portalSpawnPoint != null) {
            currentPortal = Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);
            currentPortal.name = $"Portal_Era{currentEraIndex + 1}";
        }

        // Показать описание следующей эпохи
        ShowNextEraPreview();

        // Завершить переход через delay
        Invoke("CompleteTransition", transitionDuration);
    }

    private void ShowNextEraPreview() {
        if (currentEraIndex + 1 < eraConfigs.Count) {
            S68_EraConfig nextConfig = eraConfigs[currentEraIndex + 1];
            if (eraDescriptionText != null) {
                eraDescriptionText.text = $"→ {nextConfig.eraName}: {nextConfig.description}";
                eraDescriptionText.gameObject.SetActive(true);
            }
        }
    }

    private void CompleteTransition() {
        // Удалить портал
        if (currentPortal != null) {
            Destroy(currentPortal);
        }

        // Скрыть описание
        if (eraDescriptionText != null) {
            eraDescriptionText.gameObject.SetActive(false);
        }

        // Перейти к следующей эпохе
        currentEraIndex++;
        currentCameraIndex++;

        // Запустить следующую эпоху
        StartEra();
    }

    private void UpdateCamera() {
        if (mainCamera != null && currentCameraIndex < cameraPositions.Count) {
            Vector3 targetPos = cameraPositions[currentCameraIndex];
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * cameraSpeed);

            if (currentCameraIndex < cameraRotations.Count) {
                Quaternion targetRot = Quaternion.Euler(cameraRotations[currentCameraIndex]);
                mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRot, Time.deltaTime * cameraSpeed);
            }
        }
    }

    private void ShowFinale() {
        isSceneFinished = true;

        if (eraTitleText != null) {
            eraTitleText.text = "BEYOND INFINITY";
        }

        if (numberText != null) {
            numberText.text = "∞";
        }

        if (eraDescriptionText != null) {
            eraDescriptionText.text = "Numbers are eternal. From 1 to Centillion and beyond...";
            eraDescriptionText.gameObject.SetActive(true);
        }

        // Финальный эффект
        if (transitionEffectPrefab != null) {
            Instantiate(transitionEffectPrefab, blockSpawnPoint.position, Quaternion.identity);
        }

        // Финальная музыка
        if (backgroundMusic.Count > 0 && currentEraIndex < backgroundMusic.Count) {
            audioSource.clip = backgroundMusic[backgroundMusic.Count - 1];
            audioSource.Play();
        }

        // Скрыть прогресс бар
        if (progressBarObject != null) {
            progressBarObject.SetActive(false);
        }
    }

    private void ClearPreviousObjects() {
        if (currentBlock != null) {
            Destroy(currentBlock);
            currentBlock = null;
        }
        if (currentPortal != null) {
            Destroy(currentPortal);
            currentPortal = null;
        }
    }

    private string FormatNumber(int value) {
        if (value >= 1000000) {
            return (value / 1000000f).ToString("F1") + "M";
        } else if (value >= 1000) {
            return (value / 1000f).ToString("F1") + "K";
        }
        return value.ToString();
    }

    private void OnDestroy() {
        if (audioSource != null) {
            audioSource.Stop();
        }
    }

    // Для отладки - пропустить к следующей эпохе
    private void OnGUI() {
        if (GUILayout.Button("Next Era (Debug)")) {
            if (!isTransitioning) {
                eraTimer = 0f;
            }
        }
    }
}

/// <summary>
/// Конфигурация эпохи
/// </summary>
[System.Serializable]
public class S68_EraConfig
{
    [Header("Основная информация")]
    public string eraName = "New Era";
    [TextArea(3, 5)]
    public string description = "Description of this era...";
    public S68_EraStyle eraStyle = S68_EraStyle.Ancient;

    [Header("Числа")]
    public List<int> numbers; // Числа для этой эпохи
    
    [Header("Префабы блоков")]
    public List<GameObject> blockPrefabs; // Префабы для этой эпохи

    [Header("Визуальные настройки")]
    public Material skybox;
    public Color backgroundColor = new Color(0f, 0f, 0f);
    public Color ambientColor = new Color(0f, 0f, 0f, 0f);
    public GameObject groundPrefab;
    public List<GameObject> decorationPrefabs;

    [Header("Портал")]
    public Color portalColor = new Color(1f, 1f, 1f);
    public float portalSize = 3f;
}

/// <summary>
/// Стили эпох для разных визуальных представлений
/// </summary>
public enum S68_EraStyle
{
    Ancient,      // Простые кубы, камни
    Medieval,     // Кирпичики, монеты
    Industrial,   // Вагонетки, механизмы
    Modern,       // Siri-стиль, технологии
    Future,       // Светящиеся, космос
    Eternity      // Абстрактные, свет
}
