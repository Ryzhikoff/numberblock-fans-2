using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S48_Main : MonoBehaviour
{
    public float cameraMoveSpeed = 5f;
    public Vector3 startCameraPosition;
    public Vector3 targetCameraPosition;

    public float delayForStart = 2f;

    private bool needCameraMove = false;
    private Camera camera;

    public int interpolationFramesCount = 300;
    private int elapsedFrames = 0;

    private void Start() {
        camera = Camera.main;
        camera.transform.position = startCameraPosition;
        Invoke("startCameraMove", delayForStart);
    }

    private void startCameraMove() {

        needCameraMove = true;
    }

    private void Update() {
        if (needCameraMove) {
            cameraMove();
        }
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startCameraPosition, targetCameraPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;

        if( Mathf.Abs(camera.transform.position.z - targetCameraPosition.z) < 0.05f) {
            needCameraMove = false;
        }
    }
}
