using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S64_Main : MonoBehaviour
{
    public List<GameObject> prefabs;
    public Vector3 startPosition = Vector3.zero;
    
    public float offsetZ = -20;
    public int blockCount = 16;
    public int distance = 1;
    private float stopZ = 0;

    public float speed = 5f;
    private List<GameObject> blocks = new();
    private bool needMove = false;
    public int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        startNewBlock();
    }

private void startNewBlock() {
    if (counter >= prefabs.Count)
        return;

    var prefab = prefabs[counter];
    // Получаем scale один раз из префаба или создаем временный объект
    Scale scale = prefab.GetComponent<Scale>();
    float step = scale.x + distance;
    int centerIndex = blockCount / 2; // Для 17 это будет 8

    for (int i = 0; i < blockCount; ++i) {
        var block = Instantiate(prefab);
        
        // Вычисляем X: если i=8, смещение 0. Если i=7, смещение -1*step и т.д.
        float posX = (i - centerIndex) * step - (scale.x / 2f);
        
        // Если вам нужно, чтобы центральный блок (8) был ровно в -x (как в вашем коде):
        // float posX = (i - 8) * step - (scale.x / 2f);

        block.transform.position = new Vector3(posX, startPosition.y, startPosition.z);
        
        stopZ = offsetZ - scale.z;
        blocks.Add(block);
    }

    needMove = true;
    counter++;
}

    // private void startNewBlock() {
    //     if (counter >= prefabs.Count)
    //         return;

    //     for (int i = 0; i < blockCount; ++i) {

    //         var block = Instantiate(prefabs[counter]);
    //         Scale scale = block.GetComponent<Scale>();
    //         float x = scale.x / 2;

    //         var pos = i switch {
    //             0 => new Vector3(8 * (-x - scale.x - distance), startPosition.y, startPosition.z),
    //             1 => new Vector3(7 * (-x - scale.x - distance), startPosition.y, startPosition.z),
    //             2 => new Vector3(6 * (-x - scale.x - distance), startPosition.y, startPosition.z),
    //             3 => new Vector3(5 * (-x - scale.x - distance), startPosition.y, startPosition.z),
    //             4 => new Vector3(4 * (-x - scale.x - distance), startPosition.y, startPosition.z),
    //             5 => new Vector3(3 * (-x - scale.x - distance), startPosition.y, startPosition.z),
    //             6 => new Vector3(2 * (-x - scale.x - distance), startPosition.y, startPosition.z),
    //             7 => new Vector3(-x - scale.x - distance, startPosition.y, startPosition.z),
    //             8 => new Vector3(-x, startPosition.y, startPosition.z),
    //             9 => new Vector3(x + distance, startPosition.y, startPosition.z),
    //             10 => new Vector3(2 * (x + distance) + distance, startPosition.y, startPosition.z),
    //             11 => new Vector3(3 * (x + distance) + distance, startPosition.y, startPosition.z),
    //             12 => new Vector3(4 * (x + distance) + distance, startPosition.y, startPosition.z),
    //             13 => new Vector3(5 * (x + distance) + distance, startPosition.y, startPosition.z),
    //             14 => new Vector3(6 * (x + distance) + distance, startPosition.y, startPosition.z),
    //             15 => new Vector3(7 * (x + distance) + distance, startPosition.y, startPosition.z),
    //             16 => new Vector3(8 * (x + distance) + distance, startPosition.y, startPosition.z),
    //             _ => new Vector3(-x, startPosition.y, startPosition.z),
    //         };
    //         block.transform.position = pos;
    //         stopZ = offsetZ - scale.z;
    //         blocks.Add(block);
    //     }
    //     needMove = true;
    //     counter++;
          
    // }

    // Update is called once per frame
    void Update()
    {
        if (needMove) {
            moveBlock();
        }
    }

    private void moveBlock() {
        var tempBlocks = new List<GameObject>(blocks);
        foreach (var block in blocks) {
            block.transform.position += speed * Time.deltaTime * Vector3.forward;

            if (block.transform.position.z < stopZ) {
                tempBlocks.Remove(block);
                Destroy(block);
            }
        }

        if (tempBlocks.Count == 0) {
            blocks.Clear();
            needMove = false;
            startNewBlock();
        }
    }
}
