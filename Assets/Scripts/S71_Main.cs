using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Scene_71 — "Восхождение треугольных чисел".
///
/// Идея: на экране растёт треугольная пирамидка чисел 1 → 3 → 6 → 10 → ... → 55.
/// На шаге k к фигуре добавляется ряд из k единичных блоков:
///   - текущая фигура "рассыпается" на единицы (позиции T(k-1)),
///   - k новых единиц прилетают справа-сверху,
///   - все единицы занимают позиции T(k), в момент касания — вспышка,
///   - единицы исчезают и появляется цельный треугольный блок числа T(k)
///     (свой цвет + одно лицо).
///
/// Треугольные префабы (T1, T3, T6, ... T55) генерируются заранее:
///   Tools/Scene_71/Generate Triangular Blocks.
/// Единичный "кирпичик" — обычный префаб блока "1".
/// </summary>
public class S71_Main : MonoBehaviour
{
    [Header("Префабы")]
    [Tooltip("Единичный блок-кирпичик (обычный префаб '1')")]
    public GameObject onePrefab;
    [Tooltip("Треугольные блоки по порядку шагов: T1, T3, T6, T10, T15, T21, T28, T36, T45, T55")]
    public List<GameObject> triangularPrefabs = new List<GameObject>();

    [Header("Геометрия фигуры")]
    [Tooltip("Точка-основание фигуры (центр нижнего ряда). Если пусто — берётся позиция этого объекта.")]
    public Transform figureOrigin;
    [Tooltip("Размер одного кубика (масштаб раскладки)")]
    public float blockSize = 1f;

    [Header("Тайминг")]
    public float delayBeforeStart = 1.5f;
    [Tooltip("Длительность полёта единиц на свои места")]
    public float flyDuration = 0.9f;
    [Tooltip("Пауза после сборки единиц перед слиянием")]
    public float mergePause = 0.35f;
    [Tooltip("Пауза любования готовым числом перед следующим шагом")]
    public float holdAfterMerge = 1.6f;
    [Tooltip("Разброс времени вылета единиц (каскад)")]
    public float perBlockStagger = 0.06f;

    [Header("Прилёт единиц")]
    [Tooltip("Откуда прилетают новые единицы (смещение от целевой точки)")]
    public Vector3 spawnOffset = new Vector3(14f, 10f, 0f);
    [Tooltip("Случайный разброс точки вылета")]
    public float spawnJitter = 3f;

    [Header("Камера")]
    public Camera mainCamera;
    public float cameraSmooth = 2.5f;
    [Tooltip("Базовое смещение камеры от центра фигуры")]
    public Vector3 cameraOffset = new Vector3(0f, 1.5f, -12f);
    [Tooltip("На сколько отъезжать назад на единицу высоты фигуры")]
    public float cameraBackPerHeight = 1.15f;

    [Header("UI")]
    public TextMeshProUGUI equationText;   // напр. "1 + 2 + 3 = 6"
    public TextMeshProUGUI numberText;     // крупно текущее число
    public TextMeshProUGUI titleText;      // заголовок / название

    [Header("Эффекты и звук")]
    public GameObject flashEffectPrefab;   // вспышка/частицы в момент слияния
    public AudioClip flySound;             // звук прилёта единицы
    public AudioClip mergeSound;           // звук слияния
    public AudioClip finaleSound;          // финальный аккорд
    public AudioClip backgroundMusic;
    [Tooltip("Проигрывать звук числа из Resources/sound/<N> при появлении")]
    public bool playNumberVoice = true;

    private AudioSource audioSource;
    private Vector3 originPos;
    private GameObject currentSolid;          // текущий цельный треугольный блок
    private Vector3 camTarget;
    private Vector3 figureCenter;             // центр текущей фигуры (для камеры и вспышки)
    private bool hasCamTarget;

    private int Steps => triangularPrefabs.Count;

    private void Start() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.8f;

        originPos = figureOrigin != null ? figureOrigin.position : transform.position;

        if (backgroundMusic != null) {
            var music = gameObject.AddComponent<AudioSource>();
            music.clip = backgroundMusic;
            music.loop = true;
            music.volume = 0.35f;
            music.Play();
        }

        StartCoroutine(Run());
    }

    private IEnumerator Run() {
        yield return new WaitForSeconds(delayBeforeStart);

        // Шаг 1: просто показать единицу (T1).
        currentSolid = SpawnSolid(0);
        UpdateUI(1, 1);
        PlayNumberVoice(1);
        FrameFigure(1);
        yield return new WaitForSeconds(holdAfterMerge);

        // Шаги 2..N.
        for (int k = 2; k <= Steps; k++) {
            yield return StartCoroutine(GrowToStep(k));
        }

        // Финал.
        if (finaleSound != null) audioSource.PlayOneShot(finaleSound);
        if (titleText != null) titleText.text = "TRIANGULAR NUMBERS";
        yield return null;
    }

    /// <summary>
    /// Вырастить фигуру с шага k-1 до k.
    /// ВАЖНО: текущий цельный блок НЕ рассыпается на единицы — он остаётся на месте,
    /// а k новых единичек прилетают и ложатся ВПРИТЫК по правой грани и вершине,
    /// точно достраивая T(k-1) до T(k) (вложенная раскладка, без сдвига). Затем вспышка
    /// и замена на цельный T(k). Левый-нижний угол фигуры зафиксирован на origin.
    /// </summary>
    private IEnumerator GrowToStep(int k) {
        long number = (long)k * (k + 1) / 2;
        int addCount = k;                              // на шаге k добавляется ровно k единиц

        // Новые единицы ложатся ВПРИТЫК по правой грани + вершине будущего треугольника T(k).
        // Раскладка вложенная (как в генераторе): самый правый куб ряда r имеет
        //   x = k - 0.5*r - 0.5 ,  y = r + 0.5  (локально от origin — левого нижнего угла фигуры).
        // Эти k кубов ровно достраивают текущий блок T(k-1) до T(k) без сдвига.
        var newOnes = new List<GameObject>();
        var moves = new List<(GameObject go, Vector3 from, Vector3 to)>();
        for (int r = 0; r < addCount; r++) {
            float x = (k - 0.5f * r - 0.5f) * blockSize;   // ЦЕНТР куба ячейки (r, правый край)
            float y = (r + 0.5f) * blockSize;
            Vector3 cellCenter = originPos + new Vector3(x, y, 0f);

            var one = Instantiate(onePrefab, cellCenter, Quaternion.identity);
            one.transform.localScale = Vector3.one * blockSize;
            NeutralizePhysics(one);

            // Поправка на пивот префаба "1": целимся так, чтобы ЦЕНТР куба попал в ячейку.
            Vector3 pivotOffset = PivotOffset(one);
            Vector3 to = cellCenter - pivotOffset;
            Vector3 from = to + spawnOffset + Random.insideUnitSphere * spawnJitter;
            one.transform.position = from;

            newOnes.Add(one);
            moves.Add((one, from, to));
        }

        FrameFigure(k);

        float stagger = 0f;
        foreach (var m in moves) {
            StartCoroutine(FlyTo(m.go, m.from, m.to, stagger, true));
            stagger += perBlockStagger;
        }
        yield return new WaitForSeconds(flyDuration + stagger + mergePause);

        // Слияние: вспышка -> убрать текущий блок и налетевшие единицы -> поставить T(k).
        SpawnFlash(figureCenter);
        if (mergeSound != null) audioSource.PlayOneShot(mergeSound);
        if (currentSolid != null) Destroy(currentSolid);
        foreach (var one in newOnes) if (one != null) Destroy(one);

        currentSolid = SpawnSolid(k - 1);              // индекс в списке = шаг-1
        UpdateUI(k, number);
        PlayNumberVoice(number);

        yield return new WaitForSeconds(holdAfterMerge);
    }

    private GameObject SpawnSolid(int index) {
        if (index < 0 || index >= triangularPrefabs.Count || triangularPrefabs[index] == null) {
            Debug.LogWarning($"[S71] Нет треугольного префаба с индексом {index}. Запусти Tools/Scene_71/Generate Triangular Blocks и назначь список.");
            return null;
        }
        var go = Instantiate(triangularPrefabs[index], originPos, Quaternion.identity);
        go.transform.localScale = Vector3.one * blockSize;
        return go;
    }

    // ---------- анимация полёта ----------

    private IEnumerator FlyTo(GameObject go, Vector3 from, Vector3 to, float delay, bool isNew) {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (go == null) yield break;

        if (isNew && flySound != null) audioSource.PlayOneShot(flySound, 0.5f);

        float t = 0f;
        Quaternion startRot = go.transform.rotation;
        // небольшой заброс по дуге для живости
        Vector3 mid = (from + to) * 0.5f + Vector3.up * (isNew ? 2.5f : 0.6f);
        while (t < flyDuration) {
            t += Time.deltaTime;
            float u = Mathf.SmoothStep(0f, 1f, t / flyDuration);
            // квадратичная Безье через mid
            Vector3 a = Vector3.Lerp(from, mid, u);
            Vector3 b = Vector3.Lerp(mid, to, u);
            go.transform.position = Vector3.Lerp(a, b, u);
            if (isNew) go.transform.rotation = Quaternion.Slerp(startRot, Quaternion.identity, u);
            yield return null;
        }
        go.transform.position = to;
        go.transform.rotation = Quaternion.identity;
    }

    // ---------- камера ----------

    private void FrameFigure(int k) {
        // Центр фигуры: пивот в левом-нижнем углу, фигура шириной ~k и высотой k (в кубах).
        float height = k * blockSize;
        figureCenter = originPos + new Vector3(k * 0.5f * blockSize, height * 0.5f, 0f);
        if (mainCamera == null) return;
        Vector3 off = cameraOffset;
        off.z -= height * cameraBackPerHeight;
        off.y += height * 0.25f;
        camTarget = figureCenter + off;
        hasCamTarget = true;
    }

    private void LateUpdate() {
        if (mainCamera == null || !hasCamTarget) return;
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position, camTarget, Time.deltaTime * cameraSmooth);
        mainCamera.transform.rotation = Quaternion.Slerp(
            mainCamera.transform.rotation,
            Quaternion.LookRotation(figureCenter - mainCamera.transform.position),
            Time.deltaTime * cameraSmooth);
    }

    // ---------- утилиты ----------

    private void UpdateUI(int k, long number) {
        if (equationText != null) {
            if (k == 1) equationText.text = "1";
            else {
                var sb = new System.Text.StringBuilder();
                for (int i = 1; i <= k; i++) { if (i > 1) sb.Append(" + "); sb.Append(i); }
                sb.Append(" = ").Append(number);
                equationText.text = sb.ToString();
            }
        }
        if (numberText != null) numberText.text = number.ToString();
    }

    private void PlayNumberVoice(long number) {
        if (!playNumberVoice) return;
        var clip = Resources.Load<AudioClip>("sound/" + number);
        if (clip != null) audioSource.PlayOneShot(clip);
    }

    private void SpawnFlash(Vector3 pos) {
        if (flashEffectPrefab != null) Instantiate(flashEffectPrefab, pos, Quaternion.identity);
    }

    private static void NeutralizePhysics(GameObject go) {
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        foreach (var col in go.GetComponentsInChildren<Collider>()) col.enabled = false;
    }

    /// <summary>Смещение центра видимого меша относительно пивота (для точного позиционирования по ячейке).</summary>
    private static Vector3 PivotOffset(GameObject go) {
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return Vector3.zero;
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return b.center - go.transform.position;
    }
}
