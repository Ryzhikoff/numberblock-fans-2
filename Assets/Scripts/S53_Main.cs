using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S53_Main : MonoBehaviour {

    public List<Vector3> positions = new();

    public int interpolationFramesCount = 100;
    private int elapsedFrames = 0;
    private bool needCameraMove = false;
    private Vector3 cameraStartPosition = Vector3.zero;
    private Vector3 cameraFinishPosition = Vector3.zero;

    public GameObject targetForLook = null; 

    //public Camera camera = null;

    private int counter = 1;

    private void Start() {
        transform.position = positions[0];
        setNextPositionForMove();
    }

    private void setNextPositionForMove() { 
        print("setNextPositionForMove positions.Count: " + positions.Count + " counter " + counter);
        if (positions.Count <= counter) { return; }
        cameraStartPosition = transform.position;
        cameraFinishPosition = positions[counter];
        needCameraMove = true;  
        counter++; 
    }

    private void Update() {
        
        if (needCameraMove) {
            cameraMove();
            cameraLook();
        }
    }

    public void cameraLook() {
        transform.LookAt(targetForLook.transform.position);
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(cameraStartPosition, cameraFinishPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        transform.position = interpolatedPosition;

        if (Mathf.Abs(transform.position.x - cameraFinishPosition.x) < 0.05f) {
            needCameraMove = false;
            elapsedFrames = 0;
            setNextPositionForMove();
        }
    }
}
