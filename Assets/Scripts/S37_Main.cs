using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class S37_Main : MonoBehaviour
{
    public List<GameObject> prefabs;
    private List<GameObject> blocks = new List<GameObject>();
    public Vector3 position = Vector3.zero;
    public Vector3 coefOffSetCamera;
    public float delayAfterCameraMove = 0.5f;
    public int counter = 0;
    private Camera camera;

    public int interpolationFramesCount = 100;
    private int elapsedFrames = 0;

    private Vector3 cameraStartPosition;
    private Vector3 cameraFinishPosition;
    private bool needCameraMove = false;

    public TextMeshProUGUI textUi;

    private void Start() {
        camera = Camera.main;
        arrangeBlocks();
        Invoke("prepareCameraMove", delayAfterCameraMove);
    }

    private void arrangeBlocks() {
        foreach(GameObject prefab in prefabs) {
            Scale scale = prefab.GetComponent<Scale>();
            GameObject block = Instantiate<GameObject>(prefab);
            block.transform.position = position;
            position += Vector3.right * scale.x;
            blocks.Add(block);
        }
    }

    private void prepareCameraMove() {
        if (!isNeedCameraMove()) {
            return;
        }

        cameraStartPosition = camera.transform.position;
        cameraFinishPosition = getFinishPosition();
        counter++;
        needCameraMove = true;
    }

    private bool isNeedCameraMove() {
        return counter < prefabs.Count;
    }

    private Vector3 getFinishPosition() {
        Scale scale = blocks[counter].GetComponent<Scale>();
        var positionBlock = blocks[counter].transform.position;
        var z = scale.y / 2f * coefOffSetCamera.z;
        var finishPos = new Vector3(
            positionBlock.x + scale.x * coefOffSetCamera.x,
            positionBlock.y + scale.y * coefOffSetCamera.y,
            z);
        print("target name: " + prefabs[counter].name + " scale: " + scale + " positionBlock: " + positionBlock + " finishPos: " + finishPos);
        return finishPos;
            
    }

    private void Update() {
        if (needCameraMove) {
            cameraMove();
        }
    }
    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(cameraStartPosition, cameraFinishPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;
        if (Mathf.Abs(camera.transform.position.x - cameraFinishPosition.x) < 0.05f) {
            needCameraMove = false;
            elapsedFrames = 0;
            setTextOnUi();
            Invoke("prepareCameraMove", delayAfterCameraMove);
        }
    }

    private void setTextOnUi() {
        Scale scale = blocks[counter - 1].GetComponent<Scale>();
        long number = (long)scale.x * (long)scale.y * (long)scale.z;
        print("scale: " + scale + " number: " + number);
        textUi.text = number.ToString("#,#", CultureInfo.InvariantCulture);
    }
}
