using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using static UnityEngine.GraphicsBuffer;

public class S26_Main : MonoBehaviour
{
    public List<Vector3> cameraPositions;
    public List<Vector3> cameraPositionsForShorts;
    public int interpolationFramesCount = 300;
    public float delayBetweenMoveAndRotate = 3;
    public float upToSpeedMove = 1.2f;
    int elapsedFrames = 0;
    private Camera camera;
    private bool needCameraMove = false;

    public Vector3 initCameraPosition = Vector3.zero;
    private Vector3 startCameraPosition = Vector3.zero;
    private Vector3 targetCameraPosition;
    private int counter = 0;
    private Quaternion rotation;

    public float speedRotate = 10;
    private bool needRotate = false;
    private float sumDegrees;

    public List<GameObject> listBlocks = new List<GameObject>();

    private void Start() {
        camera = Camera.main;
        rotation = camera.transform.rotation;
        camera.transform.position = initCameraPosition;
        startCameraMove();
        //Invoke(nameof(startCameraMove), delayBetweenMoveAndRotate);
    }

    public void startCameraMove() {
        if (counter >= cameraPositions.Count) {
            return;
        }
        startCameraPosition = camera.transform.position;
        targetCameraPosition = cameraPositions[counter];
        needCameraMove = true;
        counter++;
        camera.transform.rotation = rotation;
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startCameraPosition, targetCameraPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;
        if (Mathf.Abs(camera.transform.position.y - targetCameraPosition.y) < 0.05f 
            && Mathf.Abs(camera.transform.position.x - targetCameraPosition.x) < 0.05f) {
            elapsedFrames = 0;
            needCameraMove = false;
            Invoke(nameof(startCameraMove), delayBetweenMoveAndRotate);
            //startCameraMove();
            //interpolationFramesCount = (int) (interpolationFramesCount * upToSpeedMove);
            //needRotate = true;
            //counter++;
            //Invoke(nameof(startCameraMove), delayBetweenMoveAndRotate);
        }
    }           

    private void startRotate() {
        needRotate = true;
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
        camera.transform.RotateAround(listBlocks[counter].transform.position, Vector3.down, newAdd);
        camera.transform.LookAt(listBlocks[counter].transform);
        if(sumDegrees >= 360f) {
            needRotate = false;
            sumDegrees = 0;
            counter++;
            startCameraMove();
        }
    }
}
