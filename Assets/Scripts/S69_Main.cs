using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Главный контроллер сцены "Пирамида Чисел: Восхождение к Миллиону" (Scene_69).
/// Автоматически создаёт пирамиду из блоков по возрастанию чисел.
/// 
/// Уровни пирамиды:
/// Уровень 1: 1, 2, 3, 4, 5, 6, 7, 8, 9
/// Уровень 2: 10, 20, 30, 40, 50, 60, 70, 80, 90
/// Уровень 3: 100, 200, 300, 400, 500, 600, 700, 800, 900
/// Уровень 4: 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000
/// Уровень 5: 100000 → 1000000 (ФИНАЛ)
/// </summary>
public class S69_Main : MonoBehaviour
{
    [Header("Префабы блоков")]
    [Tooltip("Префабы для уровней 1-4 (9 блоков в каждом уровне)")]
    public List<GameObject> level1Blocks; // 1, 2, 3, 4, 5, 6, 7, 8, 9
    [Tooltip("Префабы для уровней 1-4 (9 блоков в каждом уровне)")]
    public List<GameObject> level2Blocks; // 10, 20, 30, 40, 50, 60, 70, 80, 90
    [Tooltip("Префабы для уровней 1-4 (9 блоков в каждом уровне)")]
    public List<GameObject> level3Blocks; // 100, 200, 300, 400, 500, 600, 700, 800, 900
    [Tooltip("Префабы для уровней 1-4 (9 блоков в каждом уровне)")]
    public List<GameObject> level4Blocks; // 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000
    
    [Header("Финальные блоки")]
    public GameObject hundredThousandPrefab; // 100,000
    public GameObject millionPrefab; // 1,000,000

    [Header("Настройки пирамиды")]
    [Tooltip("Расстояние между блоками по X")]
    public float blockSpacingX = 1.5f;
    [Tooltip("Расстояние между блоками по Y (высота уровня)")]
    public float levelHeight = 2f;
    [Tooltip("Смещение каждого уровня для формирования пирамиды")]
    public float levelOffset = 0f;
    [Tooltip("Расстояние между уровнями по Z (глубина пирамиды)")]
    public float levelSpacingZ = 2.5f; // Увеличено с 0.5f для большего пространства

    [Header("Камера")]
    public Camera mainCamera;
    public float cameraSpeed = 1.5f;
    public List<CameraPose> cameraPoses; // Позиции камеры для каждого этапа

    [Header("UI")]
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI totalSumText;
    public GameObject progressBarObject;
    public RectTransform progressBarFill;

    [Header("Звуки")]
    public AudioClip blockAppearSound;
    public AudioClip levelCompleteSound;
    public AudioClip finaleSound;
    public AudioClip backgroundMusic;
    private AudioSource audioSource;

    [Header("Эффекты")]
    public GameObject mergeEffectPrefab;
    public GameObject sparkleEffectPrefab;

    [Header("Тайминг")]
    public float delayBeforeStart = 2f;
    public float blockSpawnDelay = 0.8f; // Замедлено для лучшего восприятия
    public float levelCompleteDelay = 2.5f;
    public float finaleDelay = 3f;

    [Header("Настройки камеры")]
    [Tooltip("Камера следует за последним блоком с небольшим смещением")]
    public bool cameraFollowBlocks = true;
    [Tooltip("Смещение камеры относительно блока (X, Y, Z)")]
    public Vector3 cameraFollowOffset = new Vector3(0, 2, -10);
    [Tooltip("Плавность следования камеры")]
    public float cameraFollowSmoothness = 2f;
    [Tooltip("Целевая точка для камеры (куда смотрит камера)")]
    public Transform cameraLookTarget;
    
    [Header("Дополнительные настройки камеры")]
    [Tooltip("Автоматически корректировать смещение камеры на основе высоты блоков")]
    public bool autoAdjustCameraHeight = true;
    [Tooltip("Дополнительное смещение по Y на единицу высоты блока")]
    public float cameraHeightPerBlockUnit = 0.5f;
    [Tooltip("Дополнительное смещение по Z на единицу высоты блока")]
    public float cameraBackPerBlockUnit = 0.3f;
    [Tooltip("Минимальное расстояние камеры по Z")]
    public float minCameraDistance = -15f;

    private List<List<GameObject>> spawnedBlocks = new List<List<GameObject>>();
    private int currentLevel = 0;
    private int currentBlockInLevel = 0;
    private bool isSpawning = false;
    private bool isLevelComplete = false;
    private GameObject currentMillionBlock;
    private bool isSceneFinished = false;
    private GameObject lastSpawnedBlock; // Последний созданный блок для следования камеры
    private Vector3 cameraBaseOffset; // Базовое смещение камеры
    private Vector3 currentCameraTarget; // Текущая целевая позиция камеры
    private bool hasCameraTarget = false; // Есть ли целевая позиция
    private float currentMaxBlockHeight = 0f; // Максимальная высота блоков для камеры

    private void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false;
        audioSource.volume = 0.7f;

        // Сохранить базовое смещение камеры
        if (mainCamera != null) {
            cameraBaseOffset = mainCamera.transform.position;
        }

        if (backgroundMusic != null) {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Запуск построения пирамиды с задержкой
        Invoke("StartPyramidBuilding", delayBeforeStart);
    }

    private void StartPyramidBuilding() {
        currentLevel = 0;
        StartLevel();
    }

    /// <summary>
    /// Начать построение текущего уровня
    /// </summary>
    public void StartLevel() {
        if (currentLevel >= 4) {
            // Все уровни построены - запускаем финал
            StartFinale();
            return;
        }

        isSpawning = true;
        isLevelComplete = false;
        currentBlockInLevel = 0;
        
        // Сбросить максимальную высоту для нового уровня
        currentMaxBlockHeight = 0f;
        hasCameraTarget = false;

        // Очистить предыдущий уровень (кроме финального)
        if (currentLevel > 0 && spawnedBlocks.Count > currentLevel - 1) {
            // Не очищаем - оставляем все уровни для красоты пирамиды
        }

        // Обновить UI
        UpdateLevelUI();

        // Получить блоки текущего уровня
        List<GameObject> currentLevelBlocks = GetLevelBlocks(currentLevel);
        if (currentLevelBlocks == null || currentLevelBlocks.Count == 0) {
            Debug.LogError($"Блоки для уровня {currentLevel} не назначены!");
            currentLevel++;
            Invoke("StartLevel", levelCompleteDelay);
            return;
        }

        // Создать список для хранения блоков уровня
        if (spawnedBlocks.Count <= currentLevel) {
            spawnedBlocks.Add(new List<GameObject>());
        }

        // Запустить спавн первого блока
        Invoke("SpawnNextBlock", blockSpawnDelay);
    }

    private List<GameObject> GetLevelBlocks(int levelIndex) {
        switch (levelIndex) {
            case 0: return level1Blocks;
            case 1: return level2Blocks;
            case 2: return level3Blocks;
            case 3: return level4Blocks;
            default: return null;
        }
    }

    private void SpawnNextBlock() {
        if (!isSpawning || isLevelComplete) return;

        List<GameObject> currentLevelBlocks = GetLevelBlocks(currentLevel);
        if (currentBlockInLevel >= currentLevelBlocks.Count) {
            // Все блоки уровня созданы
            isSpawning = false;
            isLevelComplete = true;
            CompleteLevel();
            return;
        }

        // Получить префаб блока
        GameObject blockPrefab = currentLevelBlocks[currentBlockInLevel];
        if (blockPrefab == null) {
            Debug.LogWarning($"Блок {currentBlockInLevel} на уровне {currentLevel} равен null!");
            currentBlockInLevel++;
            Invoke("SpawnNextBlock", blockSpawnDelay);
            return;
        }

        // Вычислить позицию для блока в пирамиде
        Vector3 blockPosition = CalculateBlockPosition(currentLevel, currentBlockInLevel);

        // Спавн блока
        GameObject newBlock = Instantiate(blockPrefab, blockPosition, Quaternion.identity);
        newBlock.name = $"Block_L{currentLevel + 1}_B{currentBlockInLevel + 1}_{GetBlockName(blockPrefab)}";
        spawnedBlocks[currentLevel].Add(newBlock);
        lastSpawnedBlock = newBlock; // Запомнить последний блок для камеры

        // Настроить физику: отключить гравитацию и сделать кинематическим
        EnsureBlockPhysics(newBlock);

        // Добавить эффект появления
        AddAppearEffect(newBlock);

        // Воспроизвести звук
        if (blockAppearSound != null) {
            audioSource.PlayOneShot(blockAppearSound);
        }

        currentBlockInLevel++;
        Invoke("SpawnNextBlock", blockSpawnDelay);
    }

    private Vector3 CalculateBlockPosition(int levelIndex, int blockIndex) {
        // Базовая позиция по Y
        float y = levelIndex * levelHeight;

        // Получить ширину текущего блока (для учёта размера)
        float currentBlockWidth = 1f; // По умолчанию
        if (currentLevel < GetLevelBlocks(currentLevel).Count) {
            GameObject prefab = GetLevelBlocks(currentLevel)[blockIndex];
            if (prefab != null) {
                Scale scaleComp = prefab.GetComponent<Scale>();
                if (scaleComp != null) {
                    // Учитываем ширину блока (X размер)
                    currentBlockWidth = Mathf.Max(1, scaleComp.x);
                }
            }
        }

        // Пирамида: каждый уровень смещён к центру
        int totalBlocksInLevel = 9;
        
        // Рассчитать общую ширину всех предыдущих блоков на этом уровне
        float totalWidthOccupied = 0f;
        for (int i = 0; i < blockIndex; i++) {
            GameObject prevPrefab = GetLevelBlocks(levelIndex)[i];
            if (prevPrefab != null) {
                Scale prevScale = prevPrefab.GetComponent<Scale>();
                if (prevScale != null) {
                    totalWidthOccupied += Mathf.Max(1, prevScale.x);
                } else {
                    totalWidthOccupied += 1f;
                }
            }
        }
        
        // X позиция: учитываем ширину предыдущих блоков + половина ширины текущего
        float x = totalWidthOccupied + (currentBlockWidth / 2f) - (GetTotalLevelWidth(levelIndex) / 2f);

        // Z позиция: увеличенное расстояние для лучшего обзора пирамиды
        float z = levelIndex * levelSpacingZ;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Рассчитать общую ширину всех блоков на уровне
    /// </summary>
    private float GetTotalLevelWidth(int levelIndex) {
        List<GameObject> levelBlocks = GetLevelBlocks(levelIndex);
        if (levelBlocks == null) return 9f;
        
        float totalWidth = 0f;
        foreach (GameObject prefab in levelBlocks) {
            if (prefab != null) {
                Scale scale = prefab.GetComponent<Scale>();
                if (scale != null) {
                    totalWidth += Mathf.Max(1, scale.x);
                } else {
                    totalWidth += 1f;
                }
            }
        }
        return totalWidth;
    }

    private string GetBlockName(GameObject prefab) {
        Scale scaleComp = prefab.GetComponent<Scale>();
        if (scaleComp != null) {
            return scaleComp.number.ToString();
        }
        return prefab.name.Replace("(Clone)", "").Trim();
    }

    private void AddAppearEffect(GameObject block) {
        // Анимация появления: масштаб от 0 до 1 (замедлено для плавности)
        block.transform.localScale = Vector3.zero;

        LeanTweenObject(block, Vector3.one, 0.8f); // Увеличено с 0.5f до 0.8f
        
        // Обновить максимальную высоту для камеры
        UpdateMaxBlockHeight(block);
    }

    /// <summary>
    /// Обновить максимальную высоту блоков (для авто-корректировки камеры)
    /// </summary>
    private void UpdateMaxBlockHeight(GameObject block) {
        if (!autoAdjustCameraHeight) return;
        
        Scale scaleComp = block.GetComponent<Scale>();
        if (scaleComp != null) {
            // Высота блока = позиция Y + высота блока
            float blockTopY = block.transform.position.y + scaleComp.y;
            if (blockTopY > currentMaxBlockHeight) {
                currentMaxBlockHeight = blockTopY;
            }
        }
    }

    /// <summary>
    /// Гарантирует правильные настройки физики для блока
    /// useGravity = false, isKinematic = true
    /// </summary>
    private void EnsureBlockPhysics(GameObject block) {
        Rigidbody rb = block.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = block.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.isKinematic = true;
        
        // Дополнительно: отключаем коллизию для красоты
        Collider col = block.GetComponent<Collider>();
        if (col != null) {
            col.enabled = false;
        }
    }

    private void LeanTweenObject(GameObject obj, Vector3 targetScale, float duration) {
        StartCoroutine(ScaleCoroutine(obj, targetScale, duration));
    }

    private IEnumerator ScaleCoroutine(GameObject obj, Vector3 targetScale, float duration) {
        float elapsed = 0f;
        Vector3 startScale = obj.transform.localScale;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            obj.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        obj.transform.localScale = targetScale;
    }

    private void CompleteLevel() {
        // Воспроизвести звук завершения уровня
        if (levelCompleteSound != null) {
            audioSource.PlayOneShot(levelCompleteSound);
        }

        // Создать эффект завершения уровня
        CreateLevelCompleteEffect(currentLevel);

        // Обновить UI с суммой уровня
        UpdateLevelSumUI(currentLevel);

        // Обновить прогресс бар
        UpdateProgressBar();

        // Перейти к следующему уровню
        currentLevel++;
        
        // Сменить позицию камеры
        ChangeCameraPose(currentLevel);

        Invoke("StartLevel", levelCompleteDelay);
    }

    private void CreateLevelCompleteEffect(int levelIndex) {
        if (sparkleEffectPrefab != null && spawnedBlocks.Count > levelIndex && spawnedBlocks[levelIndex].Count > 0) {
            // Создать эффект в центре уровня
            Vector3 centerPos = Vector3.zero;
            foreach (GameObject block in spawnedBlocks[levelIndex]) {
                if (block != null) {
                    centerPos += block.transform.position;
                }
            }
            centerPos /= spawnedBlocks[levelIndex].Count;
            centerPos += Vector3.up * 2f;

            Instantiate(sparkleEffectPrefab, centerPos, Quaternion.identity);
        }
    }

    private void UpdateLevelUI() {
        if (levelTitleText != null) {
            levelTitleText.text = $"LEVEL {currentLevel + 1}";
        }
    }

    private void UpdateLevelSumUI(int levelIndex) {
        if (totalSumText != null) {
            long sum = GetLevelSum(levelIndex);
            totalSumText.text = FormatNumber(sum);
        }
    }

    private long GetLevelSum(int levelIndex) {
        long sum = 0;
        if (spawnedBlocks.Count > levelIndex) {
            foreach (GameObject block in spawnedBlocks[levelIndex]) {
                if (block != null) {
                    Scale scaleComp = block.GetComponent<Scale>();
                    if (scaleComp != null) {
                        sum += scaleComp.number;
                    }
                }
            }
        }
        return sum;
    }

    private void UpdateProgressBar() {
        if (progressBarFill != null) {
            float progress = (float)(currentLevel + 1) / 5f; // 4 уровня + финал
            progressBarFill.localScale = new Vector3(progress, 1f, 1f);
        }
    }

    private void ChangeCameraPose(int poseIndex) {
        if (mainCamera != null && poseIndex < cameraPoses.Count) {
            CameraPose pose = cameraPoses[poseIndex];
            if (pose != null) {
                StartCoroutine(MoveCameraToPose(pose));
            }
        }
    }

    private IEnumerator MoveCameraToPose(CameraPose pose) {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < pose.duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / pose.duration);
            
            mainCamera.transform.position = Vector3.Lerp(startPos, pose.position, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, pose.rotation, t);
            yield return null;
        }

        mainCamera.transform.position = pose.position;
        mainCamera.transform.rotation = pose.rotation;
    }

    private void StartFinale() {
        isSceneFinished = true;

        // Обновить UI
        if (levelTitleText != null) {
            levelTitleText.text = "FINAL LEVEL";
        }
        if (totalSumText != null) {
            totalSumText.text = "1,000,000";
        }

        // Скрыть прогресс бар
        if (progressBarObject != null) {
            progressBarObject.SetActive(false);
        }

        // Спавн 100,000
        if (hundredThousandPrefab != null) {
            Vector3 hundredKPos = CalculateBlockPosition(4, 0);
            GameObject hundredKBlock = Instantiate(hundredThousandPrefab, hundredKPos, Quaternion.identity);
            hundredKBlock.name = "Block_100000";
            AddAppearEffect(hundredKBlock);
            
            StartCoroutine(SpawnMillionAfterDelay(hundredKBlock));
        }
    }

    private IEnumerator SpawnMillionAfterDelay(GameObject hundredKBlock) {
        yield return new WaitForSeconds(2f);

        // Спавн 1,000,000
        if (millionPrefab != null) {
            Vector3 millionPos = CalculateBlockPosition(5, 0);
            currentMillionBlock = Instantiate(millionPrefab, millionPos, Quaternion.identity);
            currentMillionBlock.name = "Block_1000000_MILLION";
            AddAppearEffect(currentMillionBlock);

            // Удалить 100K блок
            if (hundredKBlock != null) {
                Destroy(hundredKBlock);
            }

            // Финальные эффекты
            yield return new WaitForSeconds(1f);
            CreateFinaleEffects();

            // Финальная музыка
            if (finaleSound != null) {
                audioSource.PlayOneShot(finaleSound);
            }

            // Финальная позиция камеры
            ChangeCameraPose(cameraPoses.Count - 1);

            // Показать финальный текст
            if (levelTitleText != null) {
                levelTitleText.text = "ONE MILLION!";
            }
        }
    }

    private void CreateFinaleEffects() {
        // Создать множество эффектов вокруг миллиона
        if (sparkleEffectPrefab != null && currentMillionBlock != null) {
            for (int i = 0; i < 8; i++) {
                Vector3 effectPos = currentMillionBlock.transform.position + 
                    new Vector3(
                        Random.Range(-3f, 3f),
                        Random.Range(2f, 5f),
                        Random.Range(-3f, 3f)
                    );
                Instantiate(sparkleEffectPrefab, effectPos, Quaternion.identity);
            }
        }
    }

    private void Update() {
        // Плавное следование камеры за последним блоком
        if (cameraFollowBlocks && mainCamera != null && lastSpawnedBlock != null) {
            FollowLastBlock();
        }
    }

    /// <summary>
    /// Камера плавно следует за последним созданным блоком
    /// С учётом высоты блоков и автоматической корректировкой
    /// </summary>
    private void FollowLastBlock() {
        if (lastSpawnedBlock == null) return;

        // Целевая позиция камеры: позиция блока + смещение
        Vector3 targetPosition = CalculateCameraTargetPosition(lastSpawnedBlock.transform.position);
        
        // Если это новая целевая позиция - начинаем плавное движение
        if (!hasCameraTarget) {
            currentCameraTarget = targetPosition;
            hasCameraTarget = true;
        } else {
            // Плавный переход к новой целевой позиции (Lerp для плавности)
            currentCameraTarget = Vector3.Lerp(currentCameraTarget, targetPosition, Time.deltaTime * cameraFollowSmoothness);
        }
        
        // Применяем позицию
        mainCamera.transform.position = currentCameraTarget;

        // Камера смотрит на последний блок или на целевую точку
        if (cameraLookTarget != null) {
            mainCamera.transform.LookAt(cameraLookTarget);
        } else {
            mainCamera.transform.LookAt(lastSpawnedBlock.transform.position);
        }
    }

    /// <summary>
    /// Рассчитать целевую позицию камеры с учётом высоты блоков
    /// </summary>
    private Vector3 CalculateCameraTargetPosition(Vector3 blockPosition) {
        Vector3 offset = cameraFollowOffset;
        
        // Авто-корректировка на основе высоты блоков
        if (autoAdjustCameraHeight && currentMaxBlockHeight > 0) {
            // Добавить смещение по Y на основе максимальной высоты
            float heightAdjustment = currentMaxBlockHeight * cameraHeightPerBlockUnit;
            float backAdjustment = currentMaxBlockHeight * cameraBackPerBlockUnit;
            
            offset.y += heightAdjustment;
            offset.z -= backAdjustment;
        }
        
        // Убедиться, что камера не слишком близко
        float minZ = blockPosition.z + minCameraDistance;
        float targetZ = blockPosition.z + offset.z;
        if (targetZ < minZ) {
            targetZ = minZ;
        }
        
        return new Vector3(
            blockPosition.x + offset.x,
            blockPosition.y + offset.y,
            targetZ
        );
    }

    private string FormatNumber(long value) {
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

    // Для отладки - пропустить к следующему уровню
    private void OnGUI() {
        if (GUILayout.Button("Next Level (Debug)")) {
            if (!isLevelComplete && isSpawning) {
                isSpawning = false;
                isLevelComplete = true;
                CompleteLevel();
            }
        }
    }
}

/// <summary>
/// Конфигурация позиции камеры
/// </summary>
[System.Serializable]
public class CameraPose
{
    public Vector3 position;
    public Quaternion rotation;
    [Tooltip("Длительность перехода к этой позиции (сек)")]
    public float duration = 2f;
}
