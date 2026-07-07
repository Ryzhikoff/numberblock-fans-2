using System.Collections.Generic;
using UnityEngine;

public class S70_TrillionBlocks : MonoBehaviour
{
    [Header("Block Prefabs")]
    public GameObject prefabOne;
    public GameObject prefabTen;
    public GameObject prefabHundred;
    public GameObject prefabThousand;
    public GameObject prefabMillion;
    public GameObject prefabBillion;
    public GameObject prefabTrillion;

    [Header("Generation Settings")]
    public long maxNumber = 1000000000000; // 1 trillion
    public float spacing = 10f; // Distance between blocks
    public float spiralRadius = 100f; // Spiral radius
    public float spiralHeight = 20f; // Height per spiral turn
    public int blocksPerSpiral = 50; // Blocks per spiral turn

    [Header("Container")]
    public Transform container;

    [Header("LOD Settings")]
    public float lodDistance1 = 100f; // LOD0 distance (high quality)
    public float lodDistance2 = 500f; // LOD1 distance (medium quality)
    public float lodDistance3 = 2000f; // LOD2 distance (low quality)

    private List<BlockData> allBlocks = new List<BlockData>();
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        GenerateBlocks();
    }

    void Update()
    {
        UpdateLOD();
    }

    void GenerateBlocks()
    {
        // Генерируем блоки по степеням 10
        long[] powers = new long[] { 
            1, 10, 100, 1000, 10000, 100000, 
            1000000, 10000000, 100000000, 1000000000, 
            10000000000, 100000000000, 1000000000000 
        };

        string[] names = new string[] {
            "One", "Ten", "Hundred", "Thousand", "Ten Thousand", "Hundred Thousand",
            "Million", "Ten Million", "Hundred Million", "Billion",
            "Ten Billion", "Hundred Billion", "Trillion"
        };

        for (int i = 0; i < powers.Length; i++)
        {
            Vector3 position = CalculateSpiralPosition(i, powers.Length);
            GameObject block = InstantiateBlock(GetPrefabForPower(i), position, i);
            
            BlockData data = new BlockData
            {
                gameObject = block,
                number = powers[i],
                name = names[i],
                powerIndex = i,
                basePosition = position
            };

            allBlocks.Add(data);
        }
    }

    Vector3 CalculateSpiralPosition(int index, int total)
    {
        // Спиральная формула для расположения блоков
        float angle = (index * 2f * Mathf.PI) / 5f; // Меньше блоков на виток
        float radius = spiralRadius + (index * 30f); // Больше расстояние между блоками
        float height = index * 20f; // Больше высота между блоками

        return new Vector3(
            Mathf.Cos(angle) * radius,
            height,
            Mathf.Sin(angle) * radius
        );
    }

    GameObject GetPrefabForPower(int powerIndex)
    {
        switch (powerIndex)
        {
            case 0: return prefabOne;
            case 1: return prefabTen;
            case 2: return prefabHundred;
            case 3: return prefabThousand;
            case 4: return prefabThousand;
            case 5: return prefabThousand;
            case 6: return prefabMillion;
            case 7: return prefabMillion;
            case 8: return prefabMillion;
            case 9: return prefabBillion;
            case 10: return prefabBillion;
            case 11: return prefabBillion;
            case 12: return prefabTrillion;
            default: return prefabOne;
        }
    }

    GameObject InstantiateBlock(GameObject prefab, Vector3 position, int lodLevel)
    {
        if (prefab == null)
        {
            // Если префаб не найден, создаём простой куб
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.transform.localScale = Vector3.one * (1f + lodLevel * 2f);
            cube.name = $"Block_{lodLevel}";
            return cube;
        }

        GameObject instance = Instantiate(prefab, container);
        instance.transform.position = position;
        instance.transform.localScale = Vector3.one * (1f + lodLevel * 0.5f);
        instance.name = $"Block_LOD{lodLevel}";

        return instance;
    }

    void UpdateLOD()
    {
        if (mainCamera == null) return;

        Vector3 cameraPos = mainCamera.transform.position;

        foreach (BlockData block in allBlocks)
        {
            if (block.gameObject == null) continue;

            float distance = Vector3.Distance(cameraPos, block.gameObject.transform.position);
            
            // Определяем уровень детализации
            int lodLevel = 0;
            if (distance > lodDistance3)
                lodLevel = 3;
            else if (distance > lodDistance2)
                lodLevel = 2;
            else if (distance > lodDistance1)
                lodLevel = 1;

            // Обновляем видимость и масштаб
            UpdateBlockLOD(block, lodLevel, distance);
        }
    }

    void UpdateBlockLOD(BlockData block, int lodLevel, float distance)
    {
        if (block.gameObject == null) return;

        // Включаем/выключаем объект в зависимости от расстояния
        bool shouldBeActive = lodLevel < 3;
        block.gameObject.SetActive(shouldBeActive);

        if (shouldBeActive)
        {
            // Масштабируем в зависимости от LOD
            float scale = 1f + block.powerIndex * 0.5f;
            block.gameObject.transform.localScale = Vector3.one * scale;
        }
    }

    public class BlockData
    {
        public GameObject gameObject;
        public long number;
        public string name;
        public int powerIndex;
        public Vector3 basePosition;
    }
}
