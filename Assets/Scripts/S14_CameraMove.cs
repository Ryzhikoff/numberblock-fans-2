using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S14_CameraMove : MonoBehaviour
{
    public List<Vector3> positionsList;

    //плавность хода камеры
    private int interpolationFramesCount = 100;
    int elapsedFrames = 0;
    private bool cameraMove = false;
    private Vector3 currentPosition, targetPosition;

    /// <summary>
    /// Передать текущий Counter
    /// </summary>
    /// <param name="counter"></param>
    public void startCameraMove(int counter) {
        if (positionsList.Count < counter) {
            print("Counter больше чем координатов для движения камеры. Выходим");
            return;
        }
        currentPosition = transform.position;
        targetPosition = positionsList[counter - 1];
        cameraMove = true;
    }

    private void FixedUpdate() {
        if (cameraMove) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(currentPosition, targetPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;
            if (Mathf.Abs(transform.position.y - targetPosition.y) < 0.05f) {
                transform.position = targetPosition;
                elapsedFrames = 0;
                interpolationFramesCount = 100;
                cameraMove = false;
                currentPosition = transform.position;
                S14_main.callbackFromCamera = true;
            }
        }
    }
}
