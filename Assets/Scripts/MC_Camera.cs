using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MC_Camera : MonoBehaviour
{
    //начальные координаты камеры
    public Vector3 cameraStartPosition = new Vector3(81.2f, 39.8f, -81.4f);
    //координаты каменры для One
    public Vector3 cameraOnePosition = new Vector3(9.34f, -3.65f, -35.5f);

    //плавность хода камеры
    private int interpolationFramesCount = 300;
    int elapsedFrames = 0;

    //позиция откуда наводимся
    private Vector3 currentPosition;

    //позиция куда наводимся
    private Vector3 _targetPosition;
    public Vector3 targetPosition {
        get {
            return _targetPosition;
        }
        set {
            _targetPosition = value;
        }
    }

    /// <summary>
    /// Перед тем запустить камеру - установить targetPosition
    /// </summary>
    //запуск движения камеры
    private bool _cameraMove = false;
    public bool cameraMove {
        get {
            return _cameraMove;
        }
        set {
            _cameraMove = value;
        }
    }

    private bool _startPaintBlock = false;
    public bool startPaintBlock {
        get {
            return _startPaintBlock;
        }
        set {
            _startPaintBlock = value;
        }
    }

    private void Start() {
        transform.position = cameraStartPosition;
        currentPosition = transform.position;
    }

    private void FixedUpdate() {
        if (cameraMove) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(currentPosition, targetPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;
            if ( Mathf.Abs(transform.position.x - targetPosition.x) < 0.5f) {
                elapsedFrames = 0;
                interpolationFramesCount = 100;
                cameraMove = false;
                currentPosition = transform.position;
                Invoke("setStartPaintBlockTrue", 0.1f);
            }
        }
    }

    private void setStartPaintBlockTrue() {
        startPaintBlock = true;
    }
}
