using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Контроллер эпохи для Scene_68.
/// Управляет декорациями, окружением и объектами текущей эпохи.
/// </summary>
public class S68_Era : MonoBehaviour
{
    [Header("Ссылка на главный контроллер")]
    public S68_Main mainController;

    [Header("Текущие объекты эпохи")]
    private GameObject currentGround;
    private List<GameObject> currentDecorations = new List<GameObject>();
    private S68_EraConfig currentConfig;

    [Header("Префабы для разных эпох")]
    public List<GameObject> ancientDecorations;   // Камни, кости, пещера
    public List<GameObject> medievalDecorations; // Монеты, рынки, замки
    public List<GameObject> industrialDecorations; // Вагонетки, шестерёнки, заводы
    public List<GameObject> modernDecorations;   // Экраны, компьютеры, города
    public List<GameObject> futureDecorations;   // Звёзды, планеты, ракеты
    public List<GameObject> eternityDecorations; // Частицы, свет, аура

    [Header("Префабы земли")]
    public GameObject ancientGround;
    public GameObject medievalGround;
    public GameObject industrialGround;
    public GameObject modernGround;
    public GameObject futureGround;
    public GameObject eternityGround;

    [Header("Настройки спавна")]
    public Vector3 spawnAreaSize = new Vector3(50f, 1f, 50f);
    public int decorationCount = 5;

    private void Start() {
        if (mainController == null) {
            mainController = FindObjectOfType<S68_Main>();
        }
    }

    /// <summary>
    /// Применить настройки эпохи
    /// </summary>
    public void ApplyEra(S68_EraConfig config) {
        currentConfig = config;

        // Очистить старые объекты
        ClearEraObjects();

        // Применить землю
        ApplyGround(config.eraStyle);

        // Применить декорации
        ApplyDecorations(config.eraStyle);

        // Применить дополнительные префабы из конфига
        ApplyConfigDecorations(config);
    }

    private void ApplyGround(S68_EraStyle style) {
        GameObject groundPrefab = GetGroundPrefab(style);
        
        if (groundPrefab != null) {
            currentGround = Instantiate(groundPrefab, Vector3.zero, Quaternion.identity);
            currentGround.name = $"Ground_{style}";
        } else {
            // Создать простую плоскость если префаб не найден
            currentGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
            currentGround.name = $"Ground_{style}_Placeholder";
            currentGround.transform.position = Vector3.zero;
            
            // Применить материал по стилю
            ApplyGroundMaterial(currentGround, style);
        }
    }

    private GameObject GetGroundPrefab(S68_EraStyle style) {
        return style switch {
            S68_EraStyle.Ancient => ancientGround,
            S68_EraStyle.Medieval => medievalGround,
            S68_EraStyle.Industrial => industrialGround,
            S68_EraStyle.Modern => modernGround,
            S68_EraStyle.Future => futureGround,
            S68_EraStyle.Eternity => eternityGround,
            _ => null
        };
    }

    private void ApplyGroundMaterial(GameObject ground, S68_EraStyle style) {
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer == null) {
            renderer = ground.AddComponent<Renderer>();
        }

        Material material = new Material(Shader.Find("Standard"));
        
        switch (style) {
            case S68_EraStyle.Ancient:
                material.color = new Color(0.6f, 0.4f, 0.2f); // Коричневый
                break;
            case S68_EraStyle.Medieval:
                material.color = new Color(0.5f, 0.5f, 0.5f); // Серый камень
                break;
            case S68_EraStyle.Industrial:
                material.color = new Color(0.3f, 0.3f, 0.3f); // Тёмный асфальт
                break;
            case S68_EraStyle.Modern:
                material.color = new Color(0.7f, 0.7f, 0.7f); // Бетон
                break;
            case S68_EraStyle.Future:
                material.color = new Color(0.2f, 0.2f, 0.4f); // Космический синий
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.1f, 0.1f, 0.3f));
                break;
            case S68_EraStyle.Eternity:
                material.color = new Color(1f, 1f, 1f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
                break;
        }

        renderer.material = material;
    }

    private void ApplyDecorations(S68_EraStyle style) {
        List<GameObject> decorationPrefabs = GetDecorationPrefabs(style);
        
        if (decorationPrefabs == null || decorationPrefabs.Count == 0) {
            // Если нет префабов, создать простые объекты
            CreatePlaceholderDecorations(style);
            return;
        }

        // Спавн декораций
        for (int i = 0; i < decorationCount; i++) {
            GameObject prefab = decorationPrefabs[Random.Range(0, decorationPrefabs.Count)];
            Vector3 position = GetRandomSpawnPosition();
            
            GameObject decoration = Instantiate(prefab, position, Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            ));
            decoration.name = $"Decoration_{style}_{i}";
            currentDecorations.Add(decoration);
        }
    }

    private List<GameObject> GetDecorationPrefabs(S68_EraStyle style) {
        return style switch {
            S68_EraStyle.Ancient => ancientDecorations,
            S68_EraStyle.Medieval => medievalDecorations,
            S68_EraStyle.Industrial => industrialDecorations,
            S68_EraStyle.Modern => modernDecorations,
            S68_EraStyle.Future => futureDecorations,
            S68_EraStyle.Eternity => eternityDecorations,
            _ => null
        };
    }

    private void CreatePlaceholderDecorations(S68_EraStyle style) {
        int placeholderCount = 3;
        
        for (int i = 0; i < placeholderCount; i++) {
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            placeholder.name = $"Placeholder_{style}_{i}";
            
            Vector3 position = GetRandomSpawnPosition();
            position.y = 0.5f;
            placeholder.transform.position = position;
            
            // Масштаб и цвет по эпохе
            placeholder.transform.localScale = new Vector3(
                Random.Range(0.5f, 2f),
                Random.Range(1f, 4f),
                Random.Range(0.5f, 2f)
            );

            ApplyDecorationColor(placeholder, style);
            
            currentDecorations.Add(placeholder);
        }
    }

    private void ApplyDecorationColor(GameObject obj, S68_EraStyle style) {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) {
            renderer = obj.AddComponent<Renderer>();
        }

        Material material = new Material(Shader.Find("Standard"));
        
        switch (style) {
            case S68_EraStyle.Ancient:
                material.color = new Color(0.8f, 0.6f, 0.3f); // Песочный
                break;
            case S68_EraStyle.Medieval:
                material.color = new Color(0.8f, 0.8f, 0.2f); // Золотой
                break;
            case S68_EraStyle.Industrial:
                material.color = new Color(0.5f, 0.3f, 0.1f); // Ржавый
                break;
            case S68_EraStyle.Modern:
                material.color = new Color(0.2f, 0.6f, 0.9f); // Голубой техно
                break;
            case S68_EraStyle.Future:
                material.color = new Color(0.6f, 0.2f, 0.9f); // Фиолетовый космос
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.3f, 0.1f, 0.5f));
                break;
            case S68_EraStyle.Eternity:
                material.color = new Color(0.5f, 1f, 1f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.3f, 0.5f, 0.5f));
                break;
        }

        renderer.material = material;
    }

    private void ApplyConfigDecorations(S68_EraConfig config) {
        if (config.decorationPrefabs == null) return;

        foreach (GameObject prefab in config.decorationPrefabs) {
            if (prefab != null) {
                Vector3 position = GetRandomSpawnPosition();
                GameObject decoration = Instantiate(prefab, position, Quaternion.identity);
                decoration.name = $"ConfigDecoration_{prefab.name}";
                currentDecorations.Add(decoration);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition() {
        return new Vector3(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            0f,
            Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
        );
    }

    private void ClearEraObjects() {
        // Удалить землю
        if (currentGround != null) {
            Destroy(currentGround);
            currentGround = null;
        }

        // Удалить декорации
        foreach (GameObject decoration in currentDecorations) {
            if (decoration != null) {
                Destroy(decoration);
            }
        }
        currentDecorations.Clear();
    }

    /// <summary>
    /// Создать специальный эффект для эпохи
    /// </summary>
    public void PlayEraEffect(S68_EraStyle style) {
        switch (style) {
            case S68_EraStyle.Ancient:
                CreateDustEffect();
                break;
            case S68_EraStyle.Medieval:
                CreateCoinSparkle();
                break;
            case S68_EraStyle.Industrial:
                CreateSteamEffect();
                break;
            case S68_EraStyle.Modern:
                CreateDigitalParticles();
                break;
            case S68_EraStyle.Future:
                CreateStarField();
                break;
            case S68_EraStyle.Eternity:
                CreateAuroraEffect();
                break;
        }
    }

    private void CreateDustEffect() {
        // Эффект пыли/песка
        Debug.Log("Ancient era: Dust effect");
    }

    private void CreateCoinSparkle() {
        // Эффект блеска монет
        Debug.Log("Medieval era: Coin sparkle effect");
    }

    private void CreateSteamEffect() {
        // Эффект пара
        Debug.Log("Industrial era: Steam effect");
    }

    private void CreateDigitalParticles() {
        // Цифровые частицы
        Debug.Log("Modern era: Digital particles");
    }

    private void CreateStarField() {
        // Звёздное поле
        Debug.Log("Future era: Star field");
    }

    private void CreateAuroraEffect() {
        // Эффект северного сияния
        Debug.Log("Eternity era: Aurora effect");
    }

    private void OnDestroy() {
        ClearEraObjects();
    }
}
