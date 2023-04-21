using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S18_groundMove : MonoBehaviour
{
    private void Update() {
        moveGround();
    }

    private void moveGround() {
        transform.Rotate(0, -0.6f, 0);
    }
}
