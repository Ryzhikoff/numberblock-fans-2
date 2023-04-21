using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S28_Main : MonoBehaviour
{
    public GameObject prefabOne;
    public Vector3 startPosition;
    public GameObject parentForBlocks;

    private GameObject currentPrefab;
    private List<GameObject> listBlocks = new List<GameObject>();

    private Camera camera;
    private Vector3 cameraTargetPosition;
    private bool needCameraMove = false;
    public int interpolationFramesCount = 600;
    private int elapsedFrames = 0;

    private int counter = 1;
    public int counterStop = 1000;

    public TextMeshProUGUI textCounter;

    private void Start() {
        camera = Camera.main;
        currentPrefab = prefabOne;
        addNewBlock();
    }

    private void addNewBlock() {
        if (counterStop == counter)
            return;

        setNewPositionForBlockAndCamera();

        GameObject go = Instantiate<GameObject>(currentPrefab);
        go.transform.position = startPosition;
        go.transform.parent = parentForBlocks.transform;
        listBlocks.Add(go);

        checkAndDelExtraBlocks();
        
        needCameraMove = true;
        counter++;

        setTextCounter();
    }

    

    private void setNewPositionForBlockAndCamera() {
        float scale = currentPrefab.GetComponent<Scale>(). scale.y;
        startPosition = new Vector3(
            startPosition.x,
            startPosition.y + scale,
            startPosition.z);

        cameraTargetPosition = new Vector3(
            camera.transform.position.x,
            camera.transform.position.y + scale,
            camera.transform.position.z);
    }

    private void Update() {
        if (needCameraMove)
            cameraMove();
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(camera.transform.position, cameraTargetPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;

        if( cameraTargetPosition.y - camera.transform.position.y < 0.05f ) {
            camera.transform.position = cameraTargetPosition;
            needCameraMove = false;
            
            addNewBlock();
        }
    }

    private void setTextCounter() {
        textCounter.text = counter.ToString();
    }

    private void checkAndDelExtraBlocks() {
        if (listBlocks.Count > 100) {
            GameObject g = listBlocks[0];
            listBlocks.RemoveAt(0);
            Destroy(g);
        }
    }
}
