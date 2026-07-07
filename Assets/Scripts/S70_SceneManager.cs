using UnityEngine;

public class S70_SceneManager : MonoBehaviour
{
    [Header("Component References")]
    public S70_TrillionBlocks blockGenerator;
    public S70_CameraFlyPath cameraFly;
    public S70_NumberDisplay numberDisplay;
    public S70_Starfield starfield;

    [Header("Scene Settings")]
    public bool useCustomSkybox = true;
    public Material skyboxMaterial;

    [Header("Lighting")]
    public Light mainLight;
    public Color ambientLightColor = new Color(0.3f, 0.3f, 0.5f);
    public float lightIntensity = 1.5f;

    [Header("Post Processing")]
    public bool enableBloom = true;
    public float bloomIntensity = 1.5f;
    public bool enableColorGrading = true;
    public ColorGradingMode colorGradingMode = ColorGradingMode.Cinematic;

    public enum ColorGradingMode
    {
        Cinematic,
        Vibrant,
        Natural
    }

    void Start()
    {
        SetupScene();
        SetupLighting();
        SetupPostProcessing();
    }

    void SetupScene()
    {
        // Устанавливаем скайбокс
        if (useCustomSkybox && skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            RenderSettings.skybox.SetFloat("_Rotation", 0);
        }

        // Устанавливаем цвет окружения
        RenderSettings.ambientLight = ambientLightColor;
    }

    void SetupLighting()
    {
        if (mainLight == null)
        {
            // Создаём основной источник света
            GameObject lightGO = new GameObject("MainLight");
            mainLight = lightGO.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        mainLight.intensity = lightIntensity;
        mainLight.color = Color.white;
        mainLight.shadows = LightShadows.Soft;
    }

    void SetupPostProcessing()
    {
        // Здесь можно добавить настройку постобработки через Unity Post-Processing Stack
        // Для этого нужно добавить Post-Processing Volume на сцену
        
        Debug.Log("Постобработка настроена. Bloom: " + enableBloom + ", Color Grading: " + enableColorGrading);
    }

    void Update()
    {
        // Обработка клавиш для управления
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (cameraFly != null)
            {
                cameraFly.autoFly = !cameraFly.autoFly;
                Debug.Log("Автоматический полёт: " + cameraFly.autoFly);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Перезапуск сцены
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // Вспомогательный метод для получения прогресса
    public float GetProgress(long currentNumber, long maxNumber)
    {
        if (maxNumber == 0) return 0f;
        
        // Используем логарифмическую шкалу для больших чисел
        float currentLog = Mathf.Log10(Mathf.Max(1, currentNumber));
        float maxLog = Mathf.Log10(Mathf.Max(1, maxNumber));
        
        return currentLog / maxLog;
    }

    // Форматирование больших чисел для отображения
    public string FormatBigNumber(long number)
    {
        if (number >= 1000000000000)
        {
            return (number / 1000000000000f).ToString("F2") + "T";
        }
        else if (number >= 1000000000)
        {
            return (number / 1000000000f).ToString("F2") + "B";
        }
        else if (number >= 1000000)
        {
            return (number / 1000000f).ToString("F2") + "M";
        }
        else if (number >= 1000)
        {
            return (number / 1000f).ToString("F2") + "K";
        }
        else
        {
            return number.ToString();
        }
    }
}
