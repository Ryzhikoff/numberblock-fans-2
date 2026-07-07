using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S23_JumpSpectator : MonoBehaviour
{
    private float offSet = 0.25f;
    private float speed = 0.5f;
    private float maxY;
    private float minY;
    private bool needJump = false;
    private int coef = 1;

    public void setLimit() {
        minY = transform.position.y;
        maxY = minY + offSet;
    }

    private void jump() {
        checkCoef();
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + (coef * speed) * Time.deltaTime,
            transform.position.z);
    }

    private void checkCoef() {
        if (transform.position.y >= maxY) {
            coef = -1;
        } else if (transform.position.y <= minY) {
            coef = 1;
        }
    }

    public void jump(bool stateJump) {
        needJump = stateJump;
    }

    private void Update() {
        if (needJump) {
            jump();
        }
    }
}
