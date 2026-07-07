using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Контроллер повествования для Scene_68.
/// Отображает тексты повествования для каждой эпохи с анимацией и эффектами.
/// </summary>
public class S68_Narrator : MonoBehaviour
{
    [Header("UI ссылки")]
    public TextMeshProUGUI narrativeText;
    public TextMeshProUGUI eraTitleText;
    public TextMeshProUGUI numberFactText;

    [Header("Настройки текста")]
    public float textSpeed = 0.05f; // Скорость появления текста (сек на символ)
    public float displayDuration = 5f; // Сколько показывать текст
    public float fadeDuration = 1f; // Длительность исчезновения

    [Header("Настройки повествования")]
    public List<NarrativeSegment> ancientNarratives;
    public List<NarrativeSegment> medievalNarratives;
    public List<NarrativeSegment> industrialNarratives;
    public List<NarrativeSegment> modernNarratives;
    public List<NarrativeSegment> futureNarratives;
    public List<NarrativeSegment> eternityNarratives;

    [Header("Звуки")]
    public AudioClip typewriterSound;
    public AudioClip narrativeBackground;
    private AudioSource audioSource;

    [Header("Анимация")]
    public bool enableTypewriterEffect = true;
    public bool enableFadeIn = true;
    public bool enableFadeOut = true;

    private Coroutine currentNarrativeCoroutine;
    private bool isNarrating = false;

    private void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.3f;

        if (narrativeText != null) {
            narrativeText.text = "";
            narrativeText.alpha = 0;
        }
    }

    /// <summary>
    /// Начать повествование для эпохи
    /// </summary>
    public void StartNarration(S68_EraStyle era) {
        if (isNarrating && currentNarrativeCoroutine != null) {
            StopCoroutine(currentNarrativeCoroutine);
        }

        List<NarrativeSegment> segments = GetNarrativesForEra(era);
        
        if (segments != null && segments.Count > 0) {
            // Выбрать случайный сегмент или по порядку
            NarrativeSegment segment = segments[Random.Range(0, segments.Count)];
            currentNarrativeCoroutine = StartCoroutine(DisplayNarrative(segment));
        }
    }

    /// <summary>
    /// Начать повествование с конкретным текстом
    /// </summary>
    public void StartNarration(string text, float duration = 5f) {
        if (isNarrating && currentNarrativeCoroutine != null) {
            StopCoroutine(currentNarrativeCoroutine);
        }

        NarrativeSegment segment = new NarrativeSegment {
            text = text,
            duration = duration,
            numberFact = ""
        };
        
        currentNarrativeCoroutine = StartCoroutine(DisplayNarrative(segment));
    }

    private List<NarrativeSegment> GetNarrativesForEra(S68_EraStyle era) {
        return era switch {
            S68_EraStyle.Ancient => ancientNarratives,
            S68_EraStyle.Medieval => medievalNarratives,
            S68_EraStyle.Industrial => industrialNarratives,
            S68_EraStyle.Modern => modernNarratives,
            S68_EraStyle.Future => futureNarratives,
            S68_EraStyle.Eternity => eternityNarratives,
            _ => null
        };
    }

    private IEnumerator DisplayNarrative(NarrativeSegment segment) {
        isNarrating = true;

        // Обновить заголовок эпохи если есть
        if (eraTitleText != null && !string.IsNullOrEmpty(segment.eraTitle)) {
            eraTitleText.text = segment.eraTitle;
        }

        // Воспроизвести фоновый звук повествования
        if (narrativeBackground != null) {
            audioSource.PlayOneShot(narrativeBackground);
        }

        // Показать текст с эффектом печатной машинки
        if (enableTypewriterEffect) {
            yield return StartCoroutine(TypeText(segment.text));
        } else {
            if (narrativeText != null) {
                narrativeText.text = segment.text;
                narrativeText.alpha = 1;
            }
        }

        // Показать факт о числе если есть
        if (numberFactText != null && !string.IsNullOrEmpty(segment.numberFact)) {
            numberFactText.text = segment.numberFact;
            numberFactText.gameObject.SetActive(true);
        }

        // Ждать указанную длительность
        yield return new WaitForSeconds(segment.duration);

        // Скрыть факт о числе
        if (numberFactText != null) {
            numberFactText.gameObject.SetActive(false);
        }

        // Исчезновение
        if (enableFadeOut) {
            yield return StartCoroutine(FadeOut());
        }

        isNarrating = false;
    }

    private IEnumerator TypeText(string text) {
        if (narrativeText == null) yield break;

        narrativeText.text = "";
        narrativeText.alpha = 1;

        // Воспроизвести звук печатной машинки если есть
        bool hasTypewriterSound = typewriterSound != null;

        foreach (char c in text) {
            narrativeText.text += c;
            
            if (hasTypewriterSound) {
                audioSource.PlayOneShot(typewriterSound);
            }

            yield return new WaitForSeconds(textSpeed);
        }
    }

    private IEnumerator FadeIn() {
        if (narrativeText == null) yield break;

        float elapsed = 0f;
        narrativeText.alpha = 0;

        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            narrativeText.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }

        narrativeText.alpha = 1;
    }

    private IEnumerator FadeOut() {
        if (narrativeText == null) yield break;

        float elapsed = 0f;
        float startAlpha = narrativeText.alpha;

        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            narrativeText.alpha = Mathf.Lerp(startAlpha, 0, t);
            yield return null;
        }

        narrativeText.text = "";
        narrativeText.alpha = 0;
    }

    /// <summary>
    /// Остановить текущее повествование
    /// </summary>
    public void StopNarration() {
        if (currentNarrativeCoroutine != null) {
            StopCoroutine(currentNarrativeCoroutine);
            currentNarrativeCoroutine = null;
        }
        isNarrating = false;

        if (narrativeText != null) {
            narrativeText.text = "";
            narrativeText.alpha = 0;
        }
    }

    /// <summary>
    /// Пропустить текущий текст
    /// </summary>
    public void SkipCurrentNarrative() {
        if (currentNarrativeCoroutine != null) {
            StopCoroutine(currentNarrativeCoroutine);
        }

        if (narrativeText != null) {
            narrativeText.text = "";
            narrativeText.alpha = 0;
        }

        isNarrating = false;
    }
}

/// <summary>
/// Сегмент повествования
/// </summary>
[System.Serializable]
public class NarrativeSegment
{
    [TextArea(3, 5)]
    public string text = "Narrative text...";
    public string eraTitle = "";
    [TextArea(1, 3)]
    public string numberFact = "";
    public float duration = 5f;
}

/// <summary>
/// Расширения для готовых текстов повествования
/// </summary>
public static class NarrativePresets
{
    public static List<NarrativeSegment> GetAncientNarratives() {
        return new List<NarrativeSegment> {
            new NarrativeSegment {
                eraTitle = "ANCIENT WORLD",
                text = "Long ago, humans counted on their fingers. One, two, three... The first numbers were born.",
                numberFact = "The number 1 represents unity - the beginning of all things.",
                duration = 6f
            },
            new NarrativeSegment {
                eraTitle = "ANCIENT WORLD",
                text = "Cave people marked tallys on bones and stones. Each mark was a number, a memory.",
                numberFact = "Ancient bones show tally marks from 35,000 years ago!",
                duration = 6f
            },
            new NarrativeSegment {
                eraTitle = "ANCIENT WORLD",
                text = "From fingers to stones - numbers helped track days, animals, and seasons.",
                numberFact = "The number 10 comes from counting all our fingers.",
                duration = 5f
            }
        };
    }

    public static List<NarrativeSegment> GetMedievalNarratives() {
        return new List<NarrativeSegment> {
            new NarrativeSegment {
                eraTitle = "MEDIEVAL TRADE",
                text = "In bustling marketplaces, merchants counted coins. Numbers became wealth.",
                numberFact = "The word 'cent' comes from Latin 'centum' meaning 100.",
                duration = 6f
            },
            new NarrativeSegment {
                eraTitle = "MEDIEVAL TRADE",
                text = "Gold coins clinked as traders exchanged goods. Mathematics powered commerce.",
                numberFact = "Medieval merchants invented double-entry bookkeeping.",
                duration = 5f
            },
            new NarrativeSegment {
                eraTitle = "MEDIEVAL TRADE",
                text = "From 10 to 100 - numbers grew with trade routes across kingdoms.",
                numberFact = "The Silk Road spread numerical knowledge across continents.",
                duration = 5f
            }
        };
    }

    public static List<NarrativeSegment> GetIndustrialNarratives() {
        return new List<NarrativeSegment> {
            new NarrativeSegment {
                eraTitle = "INDUSTRIAL REVOLUTION",
                text = "Steam engines roared. Factories multiplied production. Numbers reached hundreds.",
                numberFact = "The first mechanical computers used gears to calculate.",
                duration = 6f
            },
            new NarrativeSegment {
                eraTitle = "INDUSTRIAL REVOLUTION",
                text = "Minecarts carried coal. Trains connected cities. Scale transformed the world.",
                numberFact = "1000 items could now be made in hours, not months.",
                duration = 5f
            },
            new NarrativeSegment {
                eraTitle = "INDUSTRIAL REVOLUTION",
                text = "From hundreds to thousands - the age of mass production began.",
                numberFact = "The assembly line could produce 1000 units per day.",
                duration = 5f
            }
        };
    }

    public static List<NarrativeSegment> GetModernNarratives() {
        return new List<NarrativeSegment> {
            new NarrativeSegment {
                eraTitle = "DIGITAL AGE",
                text = "Screens glow with data. Computers process millions of calculations per second.",
                numberFact = "Your phone has more computing power than NASA's moon mission.",
                duration = 6f
            },
            new NarrativeSegment {
                eraTitle = "DIGITAL AGE",
                text = "The internet connects billions. Numbers became invisible, yet everywhere.",
                numberFact = "Over 1 million emails are sent every second.",
                duration = 5f
            },
            new NarrativeSegment {
                eraTitle = "DIGITAL AGE",
                text = "From thousands to millions - the digital revolution changed everything.",
                numberFact = "1 million seconds = 11.5 days. 1 billion seconds = 31.7 years!",
                duration = 6f
            }
        };
    }

    public static List<NarrativeSegment> GetFutureNarratives() {
        return new List<NarrativeSegment> {
            new NarrativeSegment {
                eraTitle = "SPACE AGE",
                text = "Rockets pierce the sky. We count stars, planets, galaxies.",
                numberFact = "There are more stars than grains of sand on Earth.",
                duration = 6f
            },
            new NarrativeSegment {
                eraTitle = "SPACE AGE",
                text = "Billions become trillions as we measure the cosmos itself.",
                numberFact = "The observable universe is 93 billion light-years across.",
                duration = 5f
            },
            new NarrativeSegment {
                eraTitle = "SPACE AGE",
                text = "From millions to billions - humanity reaches for the infinite.",
                numberFact = "1 trillion seconds = 31,709 years!",
                duration = 5f
            }
        };
    }

    public static List<NarrativeSegment> GetEternityNarratives() {
        return new List<NarrativeSegment> {
            new NarrativeSegment {
                eraTitle = "BEYOND INFINITY",
                text = "Numbers transcend comprehension. Centillion is just the beginning.",
                numberFact = "A centillion has 303 zeros. More than atoms in the universe!",
                duration = 7f
            },
            new NarrativeSegment {
                eraTitle = "BEYOND INFINITY",
                text = "Beyond trillions lie numbers without names. Mathematics meets philosophy.",
                numberFact = "Infinity is not a number - it's a concept without end.",
                duration = 6f
            },
            new NarrativeSegment {
                eraTitle = "BEYOND INFINITY",
                text = "From one to infinity - the journey of numbers is eternal.",
                numberFact = "Every number ever conceived exists in the realm of mathematics.",
                duration = 6f
            }
        };
    }
}
