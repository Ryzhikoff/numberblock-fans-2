using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S20_Main : MonoBehaviour
{
    public GameObject prefabOne;
    public Vector2 startBlockPositionLimit = new Vector2(-2.7f, 2.2f);
    public float startBlockPositionZ = 17;
    public float startBlockPositionY = 2.13f;
    public float speedMoveBlock = 10;
    public Vector2 delayBetweenAddNewBlockLimit = new Vector2(0.2f, 1.2f);
    private int counter = 1000;
    public bool isGoing = true;

    //последнее рандомное число
    private int lastRandom = 0;

    private List<GameObject> activeBlockList = new List<GameObject>();
    
    private void Start() {
        Invoke("addNewBlock", 1);
    }

    private void addNewBlock() {
        if (!isGoing)
            return;
        GameObject go = Instantiate<GameObject>(prefabOne);
        go.transform.position = new Vector3(
            getXRandomPosition(),
            startBlockPositionY,
            startBlockPositionZ);
        activeBlockList.Add(go);
        counter++;
        Invoke("addNewBlock", getDelayBetweenAddNewBlock());
    }

    private float getXRandomPosition() {
        float[] array = new float[] {
            -4.97f,
            -3.27f,
            -1.63f,
            0.03f,
            1.69f,
            3.34f,
            4.97f
        };

        return array[getRandomInt(array.Length)];
    }

    //Получаем рандомный элемент массива, ПРИ повторе  - выбираем другой
    private int getRandomInt(int max) {
        int random = Random.Range(0, max);
        if (random == lastRandom) {
            random = getRandomInt(max);
        }
        lastRandom = random;
        return random;
    }

    private float getDelayBetweenAddNewBlock() {
        return Random.Range(delayBetweenAddNewBlockLimit.x, delayBetweenAddNewBlockLimit.y);
    }

    private void Update() {
        if (activeBlockList.Count > 0)
            moveAllBlock();
    }

    public void delBlockInList(GameObject block) {
        activeBlockList.Remove(block);
    }

    private void moveAllBlock() {
        foreach (GameObject block in activeBlockList) {
            block.transform.position = new Vector3(
                block.transform.position.x,
                block.transform.position.y,
                block.transform.position.z - speedMoveBlock * Time.deltaTime);
        }
    }
}
