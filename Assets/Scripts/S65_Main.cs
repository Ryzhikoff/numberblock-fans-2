using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using UnityEngine;

public class S65_Main : MonoBehaviour
{
    // ... (Остальные публичные переменные остаются без изменений)
    public List<GameObject> units;
    public List<GameObject> tens;
    public List<GameObject> hundreds;
    // public List<GameObject> thousands; // Тысячи пока не используем, но можно добавить

    public float speed = 5f;
    public Vector3 direction = Vector3.left;
    public GameObject container;

    private List<GameObject> blocks = new List<GameObject>();
    // Переименовал в totalXOffset, чтобы было понятнее, что это суммарный отступ
    private float totalXOffset = 0f; 
    private const float spacing = 1f;

    void Start()
    {
        GenerateNumberBlocks(100);
    }

    void Update()
    {
        // moveBlocks();
    }

    void GenerateNumberBlocks(int maxNumber)
    {
        for (int i = 1; i <= maxNumber; i++)
        {
            CreateNumber(i);
        }
    }

    void CreateNumber(int num)
    {
        int h = (num / 100) % 10;
        int t = (num / 10) % 10;
        int u = num % 10;

        // Здесь мы сохраняем высоту основания для текущего числа
        float currentNumberBaseY = 0; 
        // Временная переменная для накопления высоты компонентов числа
        float currentComponentYOffset = 0;

        if (h > 0) {
            // Передаем временный оффсет по ссылке
            SpawnPart(hundreds[h - 1], ref currentComponentYOffset);
        }
        if (t > 0) {
            SpawnPart(tens[t - 1], ref currentComponentYOffset);
        }
        if (u > 0) {
            SpawnPart(units[u - 1], ref currentComponentYOffset);
        }

        // После сборки всех компонентов числа, сдвигаем X-курсор для следующего числа.
        // Ширина каждого "столбика" NumberBlock всегда равна 1 (по оси X).
        totalXOffset += 1f + spacing; // Ширина числа (1) + требуемый пробел (1)
    }

    void SpawnPart(GameObject prefab, ref float yOffset)
    {
        GameObject go = Instantiate(prefab, container.transform);
        Scale scaleInfo = go.GetComponent<Scale>();

        // Если Pivot в центре, позиционируем объект:
        // Используем totalXOffset для X-позиции всего числа.
        // Используем yOffset + половина_высоты_текущего_блока для Y-позиции.
        float halfHeight = scaleInfo.y / 2f;
        go.transform.localPosition = new Vector3(totalXOffset, yOffset + halfHeight, 0);

        // Обновляем yOffset для следующего компонента, который будет ставиться сверху.
        yOffset += scaleInfo.y;

        blocks.Add(go);
    }

    private void moveBlocks()
    {
        foreach (GameObject go in blocks)
        {
            if (go != null)
                go.transform.position += speed * Time.deltaTime * direction;
        }
    }
}