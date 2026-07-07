using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class S59_Main : MonoBehaviour
{
    private Camera camera;
    public GameObject asteriod;
    public List<GameObject> blocksForMove;
    public int blockSpeed = 5;

    public Vector3 startCameraPosition;
    public Vector3 endCameraPosition;
    public long interpolationFramesCount;
    long elapsedFrames = 0;
    private bool needMove = true;
    

    private void Start() {
        camera = Camera.main;
    }

    private void Update() {
        MoveBlocks();
        CameraLookToBlocks();
        //LookToActeriod();
        //if (needMove) {
        //    CameraMove();
        //}
    }

    private void CameraLookToBlocks() {
        var block = blocksForMove[0];
        if (block == null)
            return;

        camera.transform.LookAt(block.transform);
    }

    private void MoveBlocks() {
        if (blocksForMove.Count == 0) return;

        foreach (var block in blocksForMove) {
            block.transform.position += Vector3.back * Time.deltaTime * blockSpeed;
        }

    }

    private void LookToActeriod() {
        if (asteriod == null) return;
        camera.transform.LookAt(asteriod.transform);
    }

    private void CameraMove() {
        float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
        camera.transform.position = Vector3.Lerp(startCameraPosition, endCameraPosition, interpolationRatio);

        elapsedFrames = (elapsedFrames + 1);

        if (Vector3.Distance(camera.transform.position, endCameraPosition) < 0.05f) {
            elapsedFrames = 0;
            needMove = false;
        }
    }
}
