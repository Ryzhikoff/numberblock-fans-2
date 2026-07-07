using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S17_camera : MonoBehaviour
{
    public Vector3 startPosition;
    public List<Vector3> listPosition;
    private bool needMove = false;
    private Vector3 targetPosition;
    public int interpolationFramesCount = 300;
    int elapsedFrames = 0;

    private void Start() {
        transform.position = startPosition;
    }

    public void checkMove(int counter) {
        switch (counter) {
            case 4:
                targetPosition = listPosition[0];
                break;
            case 5:
                targetPosition = listPosition[1];
                break;
            case 14:
                targetPosition = listPosition[2];
                break;
            case 15:
                targetPosition = listPosition[3];
                break;
            case 16:
                targetPosition = listPosition[4];
                break;
            case 19:
                targetPosition = listPosition[5];
                break;
            default:
                return;
        }
        needMove = true;
    }

    private void Update() {
        if (needMove) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(transform.position, targetPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;
            if (Mathf.Abs(transform.position.y - targetPosition.y) <= 0.5f) {
                needMove = false;
                elapsedFrames = 0;
            }
        }
    }
}
