using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S41_MovingObstacle : MonoBehaviour
{
    public float speedMove;
    public float rigthX = 3f;
    public float leftX = -30f;
    public bool isMoveRigth = true;
    public float force = 5;

    private void FixedUpdate() {
        move();
    }

    private void move() {
        if (isMoveRigth) {
            transform.position += Vector3.right * speedMove * Time.deltaTime;
            if (transform.position.x >= rigthX) {
                isMoveRigth = false;
            }
        } else {
            transform.position += Vector3.left * speedMove * Time.deltaTime;
            if (transform.position.x <= leftX) {
                isMoveRigth = true;
            }
        }
    }
}
