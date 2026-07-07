using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Главный контроллер сцены битвы блоков (Scene_67).
/// Управляет раундами битвы: 50+50=100, 100+100=200, 200+200=400 и т.д.
/// </summary>
public class S67_Main : MonoBehaviour
{
    [Header("Префабы блоков")]
    public List<GameObject> blockPrefabs; // [50, 100, 200, 400, 800, 1000]
    
    [Header("Позиции спавна")]
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;
    public Transform centerMergePoint;
    
    [Header("Настройки раунда")]
    public float delayBeforeRound = 2f;
    public float delayAfterMerge = 3f;
    public float fightDuration = 3f;
    
    [Header("Камера")]
    public Camera battleCamera;
    public float cameraSpeed = 2f;
    public List<Vector3> cameraPositions; // Позиции для каждого раунда [50, 100, 200, 400, 800, 1000]
    private int currentCameraIndex = 0;
    
    [Header("UI")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI resultText;
    
    [Header("Звуки")]
    public AudioClip roundStartSound;
    public AudioClip victorySound;
    public AudioClip backgroundMusic;
    private AudioSource audioSource;
    
    [Header("Эффекты")]
    public GameObject mergeEffectPrefab;
    public GameObject sparkEffectPrefab;
    
    private int currentRound = 0;
    private S67_Fighter leftFighter;
    private S67_Fighter rightFighter;
    private GameObject currentResultBlock;
    private bool isFighting = false;
    private bool isMerging = false;
    private List<GameObject> activeBlocks = new List<GameObject>();
    
    private void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        
        if (backgroundMusic != null) {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }
        
        if (roundText != null) {
            roundText.text = "ROUND 1";
        }
        
        // Запуск первой битвы с задержкой
        Invoke("StartFirstRound", delayBeforeRound);
    }
    
    private void StartFirstRound() {
        currentRound = 0;
        StartRound();
    }
    
    /// <summary>
    /// Начать новый раунд битвы
    /// </summary>
    public void StartRound() {
        if (currentRound >= blockPrefabs.Count - 1) {
            // Все раунды завершены - показываем победителя
            ShowVictory();
            return;
        }
        
        isFighting = false;
        isMerging = false;
        
        // Очистить предыдущие блоки
        ClearPreviousBlocks();
        
        // Обновить UI
        if (roundText != null) {
            roundText.text = $"ROUND {currentRound + 1}";
        }
        
        // Спавн бойцов
        SpawnFighters();
        
        // Начать атаку через небольшую задержку
        Invoke("StartFight", delayBeforeRound);
    }
    
    private void SpawnFighters() {
        GameObject leftPrefab = blockPrefabs[currentRound];
        GameObject rightPrefab = blockPrefabs[currentRound];

        // Спавн левого бойца (смотрит вправо на противника)
        leftFighter = CreateFighter(leftPrefab, leftSpawnPoint.position, "Left", Quaternion.Euler(0, -90, 0));
        leftFighter.Initialize(this, rightSpawnPoint);

        // Спавн правого бойца (смотрит влево на противника)
        rightFighter = CreateFighter(rightPrefab, rightSpawnPoint.position, "Right", Quaternion.Euler(0, 90, 0));
        rightFighter.Initialize(this, leftSpawnPoint);

        activeBlocks.Add(leftFighter.gameObject);
        activeBlocks.Add(rightFighter.gameObject);
    }

    private S67_Fighter CreateFighter(GameObject prefab, Vector3 position, string name, Quaternion rotation) {
        GameObject fighterObj = Instantiate(prefab, position, rotation);
        fighterObj.name = $"{name}Fighter_Round{currentRound + 1}";

        S67_Fighter fighter = fighterObj.AddComponent<S67_Fighter>();
        fighter.speed = 3f;
        fighter.stopDistance = 1.5f;
        fighter.blockValue = GetBlockValue(currentRound);
        fighter.mergedBlockPrefab = blockPrefabs[currentRound + 1];
        fighter.mergeEffectPrefab = mergeEffectPrefab;

        return fighter;
    }
    
    private int GetBlockValue(int roundIndex) {
        // 50, 100, 200, 400, 800
        return (int) (50 * Mathf.Pow(2, roundIndex));
    }
    
    private void StartFight() {
        isFighting = true;

        // Воспроизвести звук начала раунда
        if (roundStartSound != null) {
            audioSource.PlayOneShot(roundStartSound);
        }

        // Начать движение бойцов
        if (leftFighter != null) {
            leftFighter.StartAttack();
        }
        if (rightFighter != null) {
            rightFighter.StartAttack();
        }

        // Камера смотрит на центр
        if (battleCamera != null && centerMergePoint != null) {
            //battleCamera.transform.LookAt(centerMergePoint);
        }
    }
    
    /// <summary>
    /// Вызывается когда боец достиг цели
    /// </summary>
    public void OnFighterReachedTarget(S67_Fighter fighter) {
        if (isMerging) return;
        
        // Проверить достиг ли другой боец тоже
        bool leftReached = leftFighter == null || !leftFighter.IsMoving;
        bool rightReached = rightFighter == null || !rightFighter.IsMoving;
        
        if (leftReached && rightReached) {
            isMerging = true;
            Invoke("PerformMerge", 0.5f);
        }
    }
    
    private void PerformMerge() {
        // Создать эффект слияния
        if (mergeEffectPrefab != null) {
            Instantiate(mergeEffectPrefab, centerMergePoint.position, Quaternion.identity);
        }
        
        // Скрыть старых бойцов
        if (leftFighter != null) {
            leftFighter.gameObject.SetActive(false);
        }
        if (rightFighter != null) {
            rightFighter.gameObject.SetActive(false);
        }

        // Создать результат слияния
        int resultIndex = currentRound + 1;
        GameObject resultPrefab = blockPrefabs[resultIndex];
        currentResultBlock = Instantiate(resultPrefab, centerMergePoint.position, Quaternion.identity);
        currentResultBlock.name = $"Result_Block_Round{currentRound + 1}";
        activeBlocks.Add(currentResultBlock);
        
        // Обновить UI
        if (resultText != null) {
            resultText.text = $"{GetBlockValue(currentRound)} + {GetBlockValue(currentRound)} = {GetBlockValue(currentRound) * 2}";
            resultText.gameObject.SetActive(true);
        }
        
        // Воспроизвести звук победы
        if (victorySound != null) {
            audioSource.PlayOneShot(victorySound);
        }

        // Перейти к следующему раунду
        currentRound++;
        currentCameraIndex++; // Следующая позиция камеры

        Invoke("StartNextRound", delayAfterMerge);
    }
    
    private void StartNextRound() {
        if (resultText != null) {
            resultText.gameObject.SetActive(false);
        }
        
        StartRound();
    }
    
    private void ClearPreviousBlocks() {
        foreach (GameObject block in activeBlocks) {
            if (block != null) {
                Destroy(block);
            }
        }
        activeBlocks.Clear();
    }
    
    private void ShowVictory() {
        if (roundText != null) {
            roundText.text = "VICTORY!";
        }

        if (resultText != null) {
            resultText.text = $"{GetBlockValue(currentRound - 1)} x 2 = {GetBlockValue(currentRound)}";
            resultText.gameObject.SetActive(true);
        }

        // Показать финальный блок
        if (currentResultBlock != null && blockPrefabs.Count > 0) {
            GameObject finalBlock = blockPrefabs[blockPrefabs.Count - 1];
            GameObject victoryBlock = Instantiate(finalBlock, centerMergePoint.position, Quaternion.identity);
            victoryBlock.name = "VICTORY_BLOCK";

            S67_Fighter victoryFighter = victoryBlock.AddComponent<S67_Fighter>();
            victoryFighter.PlayVictoryEffect();
        }

        // Камера на финальную позицию (последняя в списке)
        if (cameraPositions.Count > 0) {
            battleCamera.transform.position = cameraPositions[cameraPositions.Count - 1];
            battleCamera.transform.LookAt(centerMergePoint);
        }
    }

    private void Update() {
        // Камера плавно движется к текущей позиции (без изменения поворота)
        if (battleCamera != null && currentCameraIndex < cameraPositions.Count) {
            Vector3 targetPos = cameraPositions[currentCameraIndex];
            battleCamera.transform.position = Vector3.Lerp(battleCamera.transform.position, targetPos, Time.deltaTime * cameraSpeed);
            // Поворот камеры не меняется - используется твой настроенный поворот
        }
    }
    
    private void OnDestroy() {
        if (audioSource != null) {
            audioSource.Stop();
        }
    }
}
