using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class S26_Main : MonoBehaviour
{
    public List<Vector3> cameraPositions;
    public List<Vector3> cameraPositionsForShorts;
    public int interpolationFramesCount = 300;
    public float delayBetweenCameraMove = 3;
    public float upToSpeedMove = 1.2f;
    int elapsedFrames = 0;
    private Camera camera;
    private bool needCameraMove = false;
    private Vector3 targetCameraPosition;
    private int counter = 0;

    public float speedRotate = 10;
    private bool needRotate = false;
    private float sumDegrees;

    public List<GameObject> listBlocks = new List<GameObject>();

    private void Start() {
        transform.position = cameraPositions[0];
        //startCameraMove();
        Invoke("startCameraMove", delayBetweenCameraMove);
    }

    private void startCameraMove() {
        counter++;
        if (counter >= cameraPositions.Count) {
            return;
        }
        targetCameraPosition = cameraPositions[counter];
        needCameraMove = true;
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(transform.position, targetCameraPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        transform.position = interpolatedPosition;
        if (Mathf.Abs(transform.position.z - targetCameraPosition.z) < 0.05f) {
            needCameraMove = false;
            interpolationFramesCount = (int) (interpolationFramesCount * upToSpeedMove);
            //needRotate = true;
            Invoke("startCameraMove", delayBetweenCameraMove);
        }
    }

    private void Update() {
        if (needCameraMove)
            cameraMove();
        if (needRotate)
            rotate();
}
    private void rotate() {
        float newAdd = speedRotate * Time.deltaTime;
        sumDegrees += newAdd;
        transform.RotateAround(listBlocks[counter].transform.position, Vector3.down, newAdd);
        transform.LookAt(listBlocks[counter].transform);
        if(sumDegrees >= 360f) {
            needRotate = false;
            sumDegrees = 0;
            counter++;
            startCameraMove();
        }
    }
}
