using PA_DronePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S16_CameraMove : MonoBehaviour {
    public List<Vector3> positions = new List<Vector3>();
    public int interpolationFramesCount = 300;
    private int elapsedFrames = 0;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 endPosition = Vector3.zero;
    private int counter = 0;
    public GameObject targetGo;
    public float speed = 20f;

    private void setNextPosition() {
        if (counter == positions.Count) {
            counter = 0;
        }
        endPosition = transform.position;
        startPosition = positions[counter];
        counter++;
    }

    // Update is called once per frame
    void Update() {
        //cameraMove();
        //transform.lookAt(targetGo.transform.position);
        //transform.RotateAround(targetGo.transform.position, Vector3.up, speed * Time.deltaTime);

    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startPosition, endPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        transform.position = interpolatedPosition;
        if (Mathf.Abs(transform.position.x - endPosition.x) < 0.05f && Mathf.Abs(transform.position.z - endPosition.z) < 0.05f ) {
            setNextPosition();
        }
    }

    public Transform target; // Цель, вокруг которой будет двигаться камера
    public float distance = 5.0f; // Расстояние от цели
    public float height = 2.0f; // Высота камеры над целью
    public float rotationSpeed = 1.0f; // Скорость вращения камеры

    private Vector3 offset; // Смещение камеры относительно цели

    void Start() {
        // Рассчитываем начальное смещение камеры относительно цели
        offset = new Vector3(0, height, -distance);
    }

    void LateUpdate() {
        if (target != null) {
            // Поворачиваем смещение вокруг цели вокруг оси Y
            Quaternion rotation = Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0);
            offset = rotation * offset;

            // Устанавливаем позицию камеры относительно цели
            transform.position = target.position + offset;

            // Смотрим на цель
            transform.LookAt(target.position);
        }
    }


}
