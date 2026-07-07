using UnityEngine;

/// <summary>
/// Скрипт бойца для сцены битвы блоков.
/// Отвечает за движение к противнику, слияние и эффекты.
/// </summary>
public class S67_Fighter : MonoBehaviour
{
    [Header("Настройки движения")]
    public float speed = 5f;
    public float stopDistance = 2f;
    
    [Header("Настройки слияния")]
    public int blockValue = 50;
    public GameObject mergedBlockPrefab;
    
    [Header("Эффекты")]
    public GameObject mergeEffectPrefab;
    public AudioClip mergeSound;
    public AudioClip hitSound;
    
    [Header("Ссылки")]
    private S67_Main mainController;
    private AudioSource audioSource;
    private Rigidbody rigidbody;
    private bool isMoving = false;
    private bool isMerged = false;
    private Transform target;
    
    public bool IsMoving => isMoving;
    public bool IsMerged => isMerged;
    
    private void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
        
        if (rigidbody == null) {
            rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
    }
    
    /// <summary>
    /// Инициализация бойца перед битвой
    /// </summary>
    public void Initialize(S67_Main main, Transform targetTransform) {
        mainController = main;
        target = targetTransform;
    }
    
    /// <summary>
    /// Начать движение к противнику
    /// </summary>
    public void StartAttack() {
        isMoving = true;
    }
    
    /// <summary>
    /// Остановить движение
    /// </summary>
    public void StopAttack() {
        isMoving = false;
    }
    
    private void Update() {
        if (isMoving && !isMerged && target != null) {
            MoveTowardsTarget();
        }
    }
    
    private void MoveTowardsTarget() {
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= stopDistance) {
            isMoving = false;
            OnReachedTarget();
        } else {
            // Двигаемся только по оси X к цели, без поворота
            Vector3 moveDirection = new Vector3(direction.x, 0, 0);
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }
    
    private void OnReachedTarget() {
        // Воспроизвести звук удара
        if (hitSound != null) {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Сообщить контроллеру о достижении цели
        if (mainController != null) {
            mainController.OnFighterReachedTarget(this);
        }
    }
    
    /// <summary>
    /// Выполнить слияние в новый блок
    /// </summary>
    public void Merge() {
        if (isMerged) return;
        
        isMerged = true;
        isMoving = false;
        
        // Воспроизвести эффект слияния
        if (mergeEffectPrefab != null) {
            Instantiate(mergeEffectPrefab, transform.position, transform.rotation);
        }
        
        // Воспроизвести звук слияния
        if (mergeSound != null) {
            audioSource.PlayOneShot(mergeSound);
        }
        
        // Создать новый блок
        if (mergedBlockPrefab != null) {
            GameObject newBlock = Instantiate(mergedBlockPrefab, transform.position, transform.rotation);
            
            // Масштабировать новый блок в зависимости от значения
            Scale newScale = newBlock.GetComponent<Scale>();
            if (newScale != null) {
                float scaleFactor = Mathf.Sqrt(blockValue * 2 / 100f);
                newScale.scale = new Vector3(
                    newScale.scale.x * scaleFactor,
                    newScale.scale.y * scaleFactor,
                    newScale.scale.z * scaleFactor
                );
            }
        }
        
        // Удалить текущего бойца
        Invoke("DestroySelf", 0.5f);
    }
    
    private void DestroySelf() {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Воспроизвести эффект победы
    /// </summary>
    public void PlayVictoryEffect() {
        // Анимация победы (прыжок, свечение и т.д.)
        StartCoroutine(VictoryAnimation());
    }
    
    private System.Collections.IEnumerator VictoryAnimation() {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Прыжок
            transform.position = startPos + Vector3.up * Mathf.Sin(t * Mathf.PI) * 2f;
            
            yield return null;
        }
        
        transform.position = startPos;
    }
}
