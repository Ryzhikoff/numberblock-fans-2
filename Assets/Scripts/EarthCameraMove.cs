using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthCameraMove : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 point3 = new Vector3(-3f, 62.2f, -2212f);
    public Vector3 point4 = new Vector3(-3f, 1242f, -5055f);
    public Vector3 point5;
    public Vector3 point6;
    public Vector3 point7;
    public Vector3 point8;

    private bool move = false;

    public int interpolationFramesCount = 300;
    private int elapsedFrames = 0;
    private Vector3 targetPosition;
    private Vector3 currentPosition;

    public GameObject controllerGO;
    public EarthBlock earth;

    private void Start() {
        transform.position = startPosition;
        earth = controllerGO.GetComponent<EarthBlock>();
    }

    public void cameraMove(int i) {
        if (i == 1) { targetPosition = point1; }
        if (i == 2) { targetPosition = point2; }
        if (i == 3) { targetPosition = point3; }
        if (i == 4) { targetPosition = point4; }
        if (i == 5) { targetPosition = point5; }
        if (i == 6) { targetPosition = point6; }
        if (i == 7) { targetPosition = point7; }
        if (i == 8) { targetPosition = point8; }
        
        currentPosition = transform.position;
        move = true;
    }

    private void Update() {
        if (move) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(currentPosition, targetPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;
            if (targetPosition.z == transform.position.z) {
                transform.position = targetPosition;
                move = false;
                if (!EarthBlock.isAddBlock) {
                    EarthBlock.isAddBlock = true;
                    EarthBlock.isAddBlock2 = false;
                    earth.addBlock();
                }
            }
        }
    }
}
