using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class S61_Main : MonoBehaviour
{
    public List<GameObject> gameObjects;
    public float z = 0;
    public List<Vector3> cameraPositions = new List<Vector3>();
    private Vector3 targetCameraPos;
    private Vector3 startCameraPos;

    public long interpolationFramesCount;
    long elapsedFrames = 0;
    public float delayForStart = 2;
    //public Camera camera;
    private bool needCameraMove;
    public int counter = 0;
    private Camera camera;
    public float delay = 3f;

    private void Start() {
        camera = Camera.main;
        //PlaeceBlocks();
        startCamearaMove();
    }

    private void startCamearaMove() {
        if (counter >= cameraPositions.Count) return;
        startCameraPos = Camera.main.transform.position;
        targetCameraPos = cameraPositions[counter];
        counter++;
        needCameraMove = true;
    }

    private void Update() {
        if (needCameraMove)
            cameraMove();
    }
    private void cameraMove() {
        float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startCameraPos, targetCameraPos, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;
        if (Mathf.Abs(camera.transform.position.y - targetCameraPos.y) < 0.05f) {
            needCameraMove = false;
            elapsedFrames = 0;
            Invoke(nameof(startCamearaMove), delay);
        }
    }

    private void PlaeceBlocks() {
        foreach (var block in gameObjects) {
            if (block != null) {
                var scale = block.GetComponent<Scale>();
                if (scale != null) {
                    var position = new Vector3(
                        0,
                        0,
                        z);
                    Instantiate(block, position, new Quaternion());
                    z += scale.z;
                }
            }
        }
    }
}
