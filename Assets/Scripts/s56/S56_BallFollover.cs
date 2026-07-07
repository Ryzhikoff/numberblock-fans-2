using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S56_BallFollover : MonoBehaviour
{
    public Rigidbody ballRigidbody;
    public Vector3 offset;

    void FixedUpdate() {
        transform.position = ballRigidbody.position + offset;
    }
}
