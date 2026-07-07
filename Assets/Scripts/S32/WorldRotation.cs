using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRotation : MonoBehaviour
{
    public bool isRotated = false;

    public float speedRotation = 5f;

    private void Start() {
        
        isRotated = true;

        var h = transform.lossyScale.x;
    }

    private void Update() {
        if (isRotated) {
            rotateWorld();
        }
    }

    private void rotateWorld() {
        transform.Rotate(new Vector3(0, 0, speedRotation * Time.deltaTime), Space.World);
    }
}
