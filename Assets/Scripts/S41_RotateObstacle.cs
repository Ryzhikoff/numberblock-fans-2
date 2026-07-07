using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S41_RotateObstacle : MonoBehaviour
{
    public float speedRotation = 5;
    private void Update() {
        transform.Rotate(new Vector3(0,  speedRotation * Time.deltaTime, 0), Space.World);
    }
}
