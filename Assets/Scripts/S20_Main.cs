using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class S20_Main : MonoBehaviour
{
    public GameObject prefabOne;
    public List<GameObject> prefabs;
    public Vector2 startBlockPositionLimit = new Vector2(-2.7f, 2.2f);
    public float startBlockPositionZ = 17;
    public float startBlockPositionY = 2.13f;
    public float speedMoveBlock = 10;
    public Vector2 delayBetweenAddNewBlockLimit = new Vector2(0.2f, 1.2f);
    private int counter = 0;
    public bool isGoing = true;

    public Vector3 startCameraPos;
    public Vector3 targetCameraPos;
    public long interpolationFramesCount;
    private long elapsedFrames = 0;
    private bool needCameraMove;
    private bool cameraWasMoved = false;

    public List<GameObject> extraRails;

    //последнее рандомное число
    private int lastRandom = 0;

    private List<GameObject> activeBlockList = new List<GameObject>();

    float[] baseArray = new float[] {
            -8.2f,
            -6.6f,
            -4.97f,
            -3.27f,
            -1.63f,
            0.03f,
            1.69f,
            3.34f,
            4.97f,
            6.6f,
            8.2f
        };

    float[] extraArray = new float[] {
            -8.2f,
            -6.6f,
            -4.97f,
            -3.27f,
            -1.63f,
            0.03f,
            1.69f,
            3.34f,
            4.97f,
            6.6f,
            8.2f,
            9.87f,
            11.57f,
            13.2f,
            14.8f,
            16.5f,
            18.15f,
            19.8f,
            21.4f,
            23.1f,
            24.7f,
            26.4f,
        };

    private float[] curretArray;

    private void Start() {
        Camera.main.transform.position = startCameraPos;
        curretArray = baseArray;
        foreach (var obj in extraRails) {
            obj.SetActive(false);
        }
        Invoke("addNewBlock", 1);
    }

    private void addNewBlock() {
        if (!isGoing)
            return;
        GameObject go = Instantiate(getPrefab());
        go.transform.position = new Vector3(
            getXRandomPosition(),
            startBlockPositionY,
            startBlockPositionZ);
        activeBlockList.Add(go);
        Invoke("addNewBlock", getDelayBetweenAddNewBlock());
    }

    private GameObject getPrefab() {
        int number;
        if (counter > 60_000) {
            number = 8;
        } else if (counter > 20_000) {
            number = 7;
        } else if (counter > 10_000) {
            number = 6;
        } else if (counter > 5_000) {
            number = 5;
        } else if (counter > 1000) {
            number = 4;
        } else if (counter > 500) {
            number = 3;
        } else if (counter > 100) {
            number = 2;
        } else {
            number = 0;
        }

        return prefabs[Random.Range(0, number)];
    }

    private float getXRandomPosition() {
        return curretArray[getRandomInt(curretArray.Length)];
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

        if (needCameraMove)
            cameraMove();
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

    public void UpdateCounter(int newCounter) {
        counter = newCounter;
        if (!cameraWasMoved && counter > 10_000) {
            cameraWasMoved = true;
            startBlockPositionZ = 43;
            needCameraMove = true;
            curretArray = extraArray;
            foreach (var obj in extraRails) {
                obj.SetActive(true);
            }
        }
    }

    private void cameraMove() {
        float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startCameraPos, targetCameraPos, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        Camera.main.transform.position = interpolatedPosition;
        if (Mathf.Abs(Camera.main.transform.position.y - targetCameraPos.y) < 0.05f) {
            needCameraMove = false;
            elapsedFrames = 0;
        }
    }

    private void SetAlpha(GameObject go, float alpha) {
        Renderer objectRenderer = go.GetComponent<Renderer>();

        if (objectRenderer != null) {
            // Get the current color of the material
            Color currentColor = objectRenderer.material.color;

            // Create a new color with the desired alpha
            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);

            // Apply the new color to the material
            objectRenderer.material.color = newColor;
        }
    }
}
