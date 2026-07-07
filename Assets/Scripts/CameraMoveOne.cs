using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveOne : MonoBehaviour
{

    public int interpolationFramesCount = 300;
    int elapsedFrames = 0;
    private Vector3 _finishPos;
    
    public Vector3 finishPos {
        get => _finishPos;
        set {
            elapsedFrames = 0;
            _finishPos = value;
        }
       
    }
    public bool cameraMove {
        get;
        set;
    }

    private void Start() {
        
    }

    private void Update() {
        if(cameraMove) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(transform.position, _finishPos, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;
        }
    }
}
