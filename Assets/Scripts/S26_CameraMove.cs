using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S26_CameraMove : MonoBehaviour {
    public Vector3 startPoint;
    public Vector3 endPoint;

    public long interpolationFramesCount;
    long elapsedFrames = 0;
    public float delayForStart = 2;
    public Camera camera;
    private bool needCameraMove;


    private void Start() {
        //camera = Camera.main;
        camera.transform.position = startPoint;
        Invoke("startCameraMove", delayForStart);
        //Invoke("minus", 7);
    }

    private void minus() {
        interpolationFramesCount /= 2;
        Invoke("minus", 5);
    }


    private void Update() {
        if (needCameraMove)
            cameraMove();
    }

    private void startCameraMove() {
        needCameraMove = true;
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(camera.transform.position, endPoint, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;
        if (Mathf.Abs(camera.transform.position.z - endPoint.z) < 0.05f) {
            needCameraMove = false;
            
        }
    }
}
