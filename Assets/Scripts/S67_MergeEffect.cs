using UnityEngine;

/// <summary>
/// Простой эффект слияния/взрыва для сцены битвы.
/// Работает со сферой или любым объектом с MeshRenderer.
/// </summary>
public class S67_MergeEffect : MonoBehaviour
{
    [Header("Настройки")]
    public float duration = 0.5f;
    public float expandSpeed = 3f;
    public float startAlpha = 1f;
    public float endAlpha = 0f;
    
    [Header("Цвета")]
    public Color startColor = new Color(1f, 0.8f, 0f, 1f); // Жёлтый
    public Color endColor = new Color(1f, 0.3f, 0f, 0f);   // Оранжевый прозрачный
    
    private float elapsed = 0f;
    private MeshRenderer meshRenderer;
    private Material material;
    private Vector3 startScale;
    
    private void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        startScale = transform.localScale;
        
        // Создаём материал если нет
        if (meshRenderer == null) {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        // Создаём простой материал
        if (material == null) {
            material = new Material(Shader.Find("Sprites/Default"));
            if (material != null) {
                meshRenderer.material = material;
                material.color = startColor;
            }
        }
        
        // Уничтожить через duration секунд
        Destroy(gameObject, duration);
    }
    
    private void Update() {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        
        // Расширение
        float scale = 1f + expandSpeed * t;
        transform.localScale = startScale * scale;
        
        // Изменение цвета и прозрачности
        if (material != null) {
            material.color = Color.Lerp(startColor, endColor, t);
        }
        
        // Вращение для эффекта
        transform.Rotate(Vector3.up, 360f * Time.deltaTime);
        transform.Rotate(Vector3.right, 180f * Time.deltaTime);
    }
    
    private void OnDestroy() {
        if (material != null) {
            Destroy(material);
        }
    }
}
