using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class S58_Main : MonoBehaviour
{
    public List<GameObject> prefabs;
    public GameObject target;
    public Vector3 startPositionForBlocks = Vector2.zero;
    public float xRatio = 2;

    private Camera camera;

    public long interpolationFramesCount;
    long elapsedFrames = 0;
    private float delay = 2;
    private bool needCameraMove;
    private Vector3 startCameraPosition;
    private Vector3 endCameraPosition;
    private int counter = 0;
    private List<GameObject> blocks = new();

    private Quaternion startRotation;
    private Quaternion endRotation;
    private bool rotating = false;
    private float rotationDuration;
    private float rotationElapsedTime;

    public TextMeshProUGUI textMesh;
    private string number = "";

    private void Start() {
        camera = Camera.main;
        PlaceBlocks();
        PrepareToMoveIntoNextBlock();
    }

    private void PlaceBlocks() {
        var position = startPositionForBlocks;
        foreach (GameObject block in prefabs) {
            if (block.TryGetComponent<Scale>(out var scale)) {
                position += Vector3.right * (scale.x * xRatio);
                position.z = -((float) scale.z / 2);
                var instance = Instantiate(block, position, Quaternion.identity);
                blocks.Add(instance);
            }
        }
    }

    private void PrepareToMoveIntoNextBlock() {
        if (counter < 0 || counter >= blocks.Count) return;
        var block = blocks[counter++];
        var scale = block.GetComponent<Scale>();
        var blockPos = block.transform.position;
        number = scale.number.ToString();

        target.transform.position = new Vector3(
            blockPos.x + (float) scale.x /2,
            blockPos.y + (float) scale.y /2,
            blockPos.z + ((float) scale.z / 2) * -1);

        startCameraPosition = camera.transform.position;
        endCameraPosition = new Vector3(
            blockPos.x,
            blockPos.y + (float) scale.y * 2,
            blockPos.z + ((float) scale.z / 2) * -1);
        //needCameraMove = true;

        startRotation = camera.transform.rotation;
        Vector3 directionToTarget = target.transform.position - camera.transform.position;
        endRotation = Quaternion.LookRotation(directionToTarget);

        float moveDuration = interpolationFramesCount / 60f; // предположим, что 60 FPS
        rotationDuration = moveDuration / 10f;
        rotationElapsedTime = 0f;
        rotating = true;

        print($"blockPos: {blockPos} target.position: {target.transform.position} endCameraPosition: {endCameraPosition} startCameraPosition: {startCameraPosition}");
    }

    private void Update() {
        if (needCameraMove) {
            CameraMove();
            camera.transform.LookAt(target.transform);
        }

        if (rotating) {
            RotateCamera();
        }
    }

    private void RotateCamera() {
        //print($"rotationElapsedTime: {rotationElapsedTime} rotationDuration: {rotationDuration}");
        if (rotationElapsedTime < rotationDuration) {
            rotationElapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(rotationElapsedTime / rotationDuration);
            camera.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
        } else {
            needCameraMove = true;
            rotating = false;
        }
    }

    private void CameraMove() {
        float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
        camera.transform.position = Vector3.Lerp(startCameraPosition, endCameraPosition, interpolationRatio);

        elapsedFrames = (elapsedFrames + 1);

        if (Vector3.Distance(camera.transform.position, endCameraPosition) < 0.05f) {
            elapsedFrames = 0;
            needCameraMove = false;
            textMesh.text = number;
            Invoke(nameof(PrepareToMoveIntoNextBlock), delay);
        }
    }
}
