using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookAndMove : MonoBehaviour
{
    public GameObject target;
    public float speedRotate;

    private void FixedUpdate() {
        transform.RotateAround(target.transform.position, Vector3.up, speedRotate * Time.deltaTime);
        transform.LookAt(target.transform);
    }
}
