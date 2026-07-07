using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S70_NumberDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    [Header("Display Settings")]
    public string[] numberNames = new string[]
    {
        "One", "Ten", "Hundred", "Thousand", "Ten Thousand", "Hundred Thousand",
        "Million", "Ten Million", "Hundred Million", "Billion",
        "Ten Billion", "Hundred Billion", "TRILLION"
    };

    public string[] descriptions = new string[]
    {
        "The beginning of the journey...",
        "First ten",
        "A hundred - getting serious",
        "First thousand! 🎉",
        "Ten thousands",
        "Almost a million",
        "ONE MILLION! 🎉",
        "Ten millions",
        "Hundred millions",
        "ONE BILLION! 🚀",
        "Ten billions",
        "Hundred billions",
        "ONE TRILLION! 🌌✨"
    };

    [Header("Effects")]
    public float textAnimationSpeed = 0.5f;
    public Color glowColor = new Color(1f, 0.9f, 0.3f, 1f);

    private int currentIndex = 0;
    private float animationTimer = 0f;

    void Start()
    {
        UpdateDisplay(0);
    }

    void Update()
    {
        // Автоматическая смена чисел для демонстрации
        animationTimer += Time.deltaTime;
        
        if (animationTimer >= textAnimationSpeed)
        {
            animationTimer = 0f;
            currentIndex = (currentIndex + 1) % numberNames.Length;
            UpdateDisplay(currentIndex);
        }

        // Эффект свечения
        if (numberText != null)
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.3f + 0.7f;
            numberText.color = glowColor * pulse;
        }
    }

    void UpdateDisplay(int index)
    {
        if (index < 0 || index >= numberNames.Length) return;

        // Формируем число
        long number = (long)Mathf.Pow(10, index);
        string numberString = FormatNumber(number);

        // Обновляем UI
        if (numberText != null)
        {
            numberText.text = numberString;
        }

        if (nameText != null)
        {
            nameText.text = numberNames[index];
        }

        if (descriptionText != null)
        {
            descriptionText.text = descriptions[index];
        }

        // Анимация появления
        AnimateTextAppearance();
    }

    string FormatNumber(long number)
    {
        if (number >= 1000000000000)
        {
            return "1,000,000,000,000";
        }
        else if (number >= 1000000000)
        {
            return number.ToString("N0");
        }
        else
        {
            return number.ToString("N0");
        }
    }

    void AnimateTextAppearance()
    {
        if (numberText != null)
        {
            // Анимация масштабирования
            numberText.transform.localScale = Vector3.one * 1.5f;
            LeanScale(numberText.transform, Vector3.one, 0.3f);
        }

        if (nameText != null)
        {
            nameText.transform.localScale = Vector3.one * 1.3f;
            LeanScale(nameText.transform, Vector3.one, 0.3f);
        }
    }

    void LeanScale(Transform transform, Vector3 targetScale, float duration)
    {
        StartCoroutine(ScaleCoroutine(transform, targetScale, duration));
    }

    System.Collections.IEnumerator ScaleCoroutine(Transform transform, Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    // Публичный метод для ручного обновления
    public void SetNumber(int index)
    {
        if (index >= 0 && index < numberNames.Length)
        {
            currentIndex = index;
            animationTimer = 0f;
            UpdateDisplay(index);
        }
    }
}
