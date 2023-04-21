using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOffSet : MonoBehaviour
{
    public Vector3 offset = new Vector3(13f, 9f, -7f);

    private Vector3 targetPosition;
    private Vector3 currentPosition;
    //плавность хода камеры
    public int interpolationFramesCount = 5;
    private int elapsedFrames = 0;
    private bool changePosition = false;
    private Vector3 tempVector;
    private float tempX = 0;
    private Vector3 startPosition;
    private Vector3 endPosition = new Vector3(111.97f, 112.02f, -175.78f);
    

    private void Start() {
        tempVector = transform.position;
        startPosition = transform.position;
    }
    public void setView(int counter) {
        targetPosition = new Vector3(
            endPosition.x/100*counter + startPosition.x,
            endPosition.y/100*counter + startPosition.y,
            endPosition.z/100*counter + startPosition.z);

        currentPosition = transform.position;
        changePosition = true;

        print("currentPposition: " + currentPosition + " targetPosition: " + targetPosition);

        /* tempVector = new Vector3(
             tempVector.x + 1f,
             tempVector.y + 0.9f,
             tempVector.z - 1.3f);
         targetPosition = tempVector;        
         currentPosition = transform.position;
         //temp
         elapsedFrames = 0;
         changePosition = true;
         print("tempVector: " + tempVector + " targetPosition: " + targetPosition + " currentPosition: " + currentPosition);*/
    }
    private void Update() {
        if (changePosition) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(currentPosition, targetPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;
            if (targetPosition.x == transform.position.x) {
                transform.position = targetPosition;
                changePosition = false;
            }
        }
    }
}
