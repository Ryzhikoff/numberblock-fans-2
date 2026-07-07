using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S57_Main : MonoBehaviour {
    public GameObject prefabOne;
    public GameObject prefabThousand;
    private GameObject currentBlock;
    public List<GameObject> prefabs;
    private int counter = 0;
    private bool[,,] matrix;

    public float speed;
    private Vector3 targetPosition;
    public float delayAfterBlock = 1;
    public int yOffSet = 10;
    private bool needMove = false;
    public TextMeshProUGUI textMesh;

    private void Start() {
        setCurrentPrefab();
        setTagerPostion();
    }

    private void setCurrentPrefab() {
        if (counter >= prefabs.Count)
            return;

        var scale = prefabThousand.GetComponent<Scale>();
        matrix = new bool[scale.x, scale.y, scale.z];
    }

    private void setTagerPostion() {
        // Получаем размеры матрицы из компонента Scale
        var scale = prefabThousand.GetComponent<Scale>();
        int sizeX = scale.x;
        int sizeY = scale.y;
        int sizeZ = scale.z;

        // Собираем все возможные X,Z пары, где есть хотя бы один неиспользованный Y
        List<Vector2Int> availableXZ = new List<Vector2Int>();

        for (int x = 0; x < sizeX; x++) {
            for (int z = 0; z < sizeZ; z++) {
                // Проверяем, есть ли в этом столбце (x,z) неиспользованные Y
                for (int y = 0; y < sizeY; y++) {
                    if (!matrix[x, y, z]) {
                        availableXZ.Add(new Vector2Int(x, z));
                        break;
                    }
                }
            }
        }

        // Если нет доступных позиций - выходим
        if (availableXZ.Count == 0) {
            Debug.Log("No available positions in matrix");
            var blocks = GameObject.FindGameObjectsWithTag("Block");
            foreach (var block in blocks) {
                Destroy(block);
            }
            var thousand = Instantiate(prefabThousand);
            thousand.transform.position = Vector3.zero;
            return;
        }

        // Выбираем случайную X,Z пару
        Vector2Int randomXZ = availableXZ[Random.Range(0, availableXZ.Count)];
        int selectedX = randomXZ.x;
        int selectedZ = randomXZ.y;

        // Находим минимальный незанятый Y для выбранных X,Z
        int selectedY = 0;
        for (int y = 0; y < sizeY; y++) {
            if (!matrix[selectedX, y, selectedZ]) {
                selectedY = y;
                break;
            }
        }

        // Помечаем позицию как занятую
        matrix[selectedX, selectedY, selectedZ] = true;

        // Устанавливаем целевую позицию (здесь нужно адаптировать под вашу систему координат)
        targetPosition = new Vector3(selectedX, selectedY, selectedZ);
        GameObject obj = Instantiate<GameObject>(prefabOne);
        obj.transform.position = targetPosition + Vector3.up * yOffSet;
        currentBlock = obj;

        // Для отладки можно вывести выбранную позицию
        Debug.Log($"Selected position: X={selectedX}, Y={selectedY}, Z={selectedZ}");
        needMove = true;
    }

    private void Update() {
        if (needMove) {
            moveBlock();
        }
    }

    private void moveBlock() {

        //currentBlock.transform.position = Vector3.down * speedMoveDown * Time.deltaTime;
        currentBlock.transform.position = new Vector3(
            currentBlock.transform.position.x,
            currentBlock.transform.position.y - speed * Time.deltaTime,
            currentBlock.transform.position.z

        );

        if (currentBlock.transform.position.y <= targetPosition.y) {
            currentBlock.transform.position = targetPosition;
            counter++;
            needMove = false;
            setText();
            Invoke(nameof(setTagerPostion), delayAfterBlock);
        }
    }

    private void setText() {
        textMesh.text = counter.ToString();
    }
}
