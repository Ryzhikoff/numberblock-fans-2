using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S24_Main : MonoBehaviour
{
    public List<Vector3> positions = new List<Vector3>();
    public float delayBetweenMove = 3f;
    public Camera camera;

    private bool needCameraMove = false;
    private int counter = 0;

    public int interpolationFramesCount = 300;
    public float coefIFC = 5;
    int elapsedFrames = 0;
    private Vector3 finishPos;

    private void Start() {
        //camera = Camera.main;
        transform.position = positions[0];
        Invoke("startMove", delayBetweenMove);
    }

    private void startMove() {
        finishPos = positions[counter];
        counter++;
        needCameraMove = true;
    }

    private void Update() {
        if (needCameraMove) {
            cameraMove();
        }
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(transform.position, finishPos, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        transform.position = interpolatedPosition;
        if (Mathf.Abs(transform.position.x - finishPos.x) < 0.01) {
            needCameraMove = false;
            elapsedFrames = 0;
            interpolationFramesCount = (int) (interpolationFramesCount * coefIFC);
            if (counter < positions.Count + 1) {
                Invoke("startMove", delayBetweenMove);
            }
        }
    }
}
