using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;

public class S14_CameraMove : MonoBehaviour
{
    public List<Vector3> positionsList;
    public TextMeshProUGUI textMeshNumber;
    public TextMeshProUGUI textMeshNumberName;

    public List<string> numbers = new List<string> {
        "1",
        "100",
        "1,000",
        "1,000,000",
        "1,000,000,000",
        "1,000,000,000,000"
    };

    public List<string> names = new List<string> {
        "One",
        "One hundred",
        "One thousand",
        "One million",
        "One billion",
        "One trillion",
    };

    //плавность хода камеры
    public int interpolationFramesCount = 300;
    int elapsedFrames = 0;
    private bool cameraMove = false;
    private Vector3 currentPosition, targetPosition;
    private int counter = 0;

    private void Start() {
        transform.position = positionsList[0];
        cameraMove = false;
        startCameraMove(counter);
    }

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
        targetPosition = positionsList[counter];
        cameraMove = true;
    }

    private void FixedUpdate() {
        if (cameraMove) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(currentPosition, targetPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;
            if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.005f) {
                setText();
                transform.position = targetPosition;
                elapsedFrames = 0;
                interpolationFramesCount = 100;
                cameraMove = false;
                currentPosition = transform.position;
                counter++;
                startCameraMove(counter);
                //S14_main.callbackFromCamera = true;
            }
        }
    }

    private void setText() {
        textMeshNumber.text = numbers[counter];
        textMeshNumberName.text = names[counter];
    }
}
