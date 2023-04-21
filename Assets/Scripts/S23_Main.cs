using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S23_Main : MonoBehaviour
{
    public GameObject startGo;
    public GameObject prefabOneHundred;
    public GameObject prefabOneMillion;

    public float speedMoveDownBlock = 10;
    public float offsetY = 7;

    private int counter = 1;
    private Vector3 targetCoord;
    private GameObject currentGo;
    private List<GameObject> listBlock = new List<GameObject>();

    private bool needMoveBlock = false;

    private void Start() {
        targetCoord = startGo.transform.position;
        addNewBlock();
    }

    private void addNewBlock() {
        GameObject go = Instantiate<GameObject>(prefabOneHundred);
        targetCoord = new Vector3(
            targetCoord.x,
            targetCoord.y,
            targetCoord.z + 1);
        go.transform.position = new Vector3(
            targetCoord.x,
            targetCoord.y + offsetY,
            targetCoord.z);
        
        counter++;
        listBlock.Add(go);
        currentGo = go;
        needMoveBlock = true;
    }

    private void moveBlock() {
        currentGo.transform.position = new Vector3(
            currentGo.transform.position.x,
            currentGo.transform.position.y - speedMoveDownBlock * Time.deltaTime,
            currentGo.transform.position.z);
        if (currentGo.transform.position.y - targetCoord.y < 0.005f) {
            needMoveBlock = false;
            currentGo.transform.position = new Vector3(
                targetCoord.x,
                targetCoord.y,
                targetCoord.z);
            if (counter == 10) {
                GameObject go = Instantiate<GameObject>(prefabOneMillion);
                go.transform.position = startGo.transform.position;
                foreach (GameObject g in listBlock) {
                    Destroy(g);
                }
                Destroy(startGo);
            } else {
                addNewBlock();
            }
        }
    }

    private void Update() {
        if (needMoveBlock)
            moveBlock();
    }

    //Episod 2
    /*public GameObject prefabOneHundred;
    public GameObject startTen;
    private Vector3 startPosition;
    private GameObject[] listOfOne;
    
    private List<Vector3> targetPositions = new List<Vector3>();
    private bool needMoveTens = false;
    private bool needRotateTens = false;
    private bool needMoveStartTen = false;

    int elapsedFrames = 0;
    public int interpolationFramesCount = 300;
    public int interpolationFramesCount2 = 500;
    public float speedRotate = 10;

    private void Start() {
        listOfOne = GameObject.FindGameObjectsWithTag("Block");
        startPosition = new Vector3(
            startTen.transform.position.x - 4,
            startTen.transform.position.y,
            startTen.transform.position.z);
        createTargetPosirions();
        Invoke("go", 1f);

    }

    private void moveStartTen() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount2;
        Vector3 interpolatedPosition = Vector3.Lerp(startTen.transform.position, startPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount2 + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        startTen.transform.position = interpolatedPosition;
        if (Mathf.Abs(startTen.transform.position.x - startPosition.x) < 0.05f) {
            needMoveStartTen = false;
            needMoveTens = true;
            needRotateTens = true;
        }
    }

    private void createTargetPosirions() {
        for (int i = 1; i < 10; i++) {
            targetPositions.Add(new Vector3(
                startPosition.x + i,
                startPosition.y,
                startPosition.z));
        }
        
    }

    private void go() {
        needMoveStartTen = true;
    }

    private void rotateOnes() {
        foreach(GameObject go in listOfOne) {
            go.transform.Rotate(0, speedRotate * Time.deltaTime, 0);
        }
    }

    private void moveOnes() {
        for (int i = 0; i < listOfOne.Length; i++) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(listOfOne[i].transform.position, targetPositions[i], interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            listOfOne[i].transform.position = interpolatedPosition;
            
        }
        if (Mathf.Abs(listOfOne[0].transform.position.y - targetPositions[0].y) < 0.01f) {
            needMoveTens = false;
            needRotateTens = false;
            GameObject go = Instantiate<GameObject>(prefabOneHundred);
            go.transform.position = startPosition;
            destroyAll();

        }
    }

    private void destroyAll() {
        Destroy(startTen);
        foreach(GameObject go in listOfOne) {
            Destroy(go);
        }
    }


    private void Update() {
        if (needMoveTens)
            moveOnes();
        if (needRotateTens)
            rotateOnes();
        if (needMoveStartTen)
            moveStartTen();
    }

    //Episod 1
    /*public GameObject startOne;
    private float speedForOne = 5f;
    public float speedRotateCamera = 5f;
    private bool needMoveOne = false;
    private bool needCameraRotate = false;

    public GameObject spectatorGO;
    private List<GameObject> spectators = new List<GameObject>();
    private int counterJump = 0;
    private Camera mainCamera;

    private void Start() {
        mainCamera = Camera.main;
        createSpectators();
        addJump();
        Invoke("startMoveOne", 2f);
        
    }

    private void startMoveOne() {
        needMoveOne = true;
    }

    private void moveOne() {
        startOne.transform.position = new Vector3(
            startOne.transform.position.x,
            startOne.transform.position.y,
            startOne.transform.position.z - speedForOne * Time.deltaTime);
        if (startOne.transform.position.z <= 0) {
            needMoveOne = false;
            startCameraRotate();
        }
    }

    private void cameraRotate() {
        mainCamera.transform.Rotate(0, speedRotateCamera * Time.deltaTime, 0);
        if (mainCamera.transform.rotation.y >= 383) {
            needCameraRotate = false;
        }
    }
    
    private void startCameraRotate() {
        needCameraRotate = true;
    }

    //создаем список дочерних объектов
    private void createSpectators() {
        S23_JumpSpectator[] transforms = spectatorGO.GetComponentsInChildren<S23_JumpSpectator>();
        foreach (S23_JumpSpectator trans in transforms) {
            spectators.Add(trans.gameObject);
        }
        
    }

    private void Update() {
        if(needMoveOne) {
            moveOne();
        }
        if (needCameraRotate) {
            cameraRotate();
        }
    }
    private void addJump() {
        GameObject go = spectators[counterJump];
            //устанавливаем лимит
            go.gameObject.GetComponent<S23_JumpSpectator>().setLimit();
            //начиванием движение
            go.gameObject.GetComponent<S23_JumpSpectator>().jump(true);

        counterJump++;
        if (counterJump != spectators.Count) {
            Invoke("addJump", 0.1f);
        }
    }*/
}
