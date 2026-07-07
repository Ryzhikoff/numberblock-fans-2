using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class S40_Main : MonoBehaviour
{
    private Camera camera;

    public Vector3 cameraStartPos;
    public Vector3 cameraStopPos;

    public int interpolationFramesCount = 300;
    int elapsedFrames = 0;
    private bool needCameraMove = false;
    public GameObject targtGoForCamera;
    public List<GameObject> blocks;
    private int counter = 0;
    private bool isBlockedButton = false;
    private float delayBlockedButton = 1f;

    private void Start() {
        camera = Camera.main;
        needCameraMove = true;
        camera.transform.position = cameraStartPos;
        camera.transform.LookAt(targtGoForCamera.transform);
    }

    private void Update() {
        if (needCameraMove) {
            cameraMove();
        }
        if (!isBlockedButton && Input.GetKey(KeyCode.E)) {
            changeBlock();
            isBlockedButton = true;
            Invoke("unlock", delayBlockedButton);
        }
    }

    private void changeBlock() {
        if (counter != blocks.Count) {
            blocks[counter + 1].gameObject.SetActive(true);
            try {
                blocks[counter - 1].gameObject.SetActive(false);
            } catch (Exception e) {
                print(e);
            }
            counter++;
        }
    }

    private void unlock() {
        isBlockedButton = false;
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(cameraStartPos, cameraStopPos, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;
        camera.transform.LookAt(targtGoForCamera.transform);
        if (Mathf.Abs(camera.transform.position.y - cameraStopPos.y) < 0.01) {
            needCameraMove = false;
            elapsedFrames = 0;
            
        }
    }
}
