using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S41_AddForce : MonoBehaviour
{
    private float force;
    private S41_MovingObstacle obstacle;
    public bool rigth = true;
    private void Start() {
        obstacle = GetComponentInParent<S41_MovingObstacle>();
        force = obstacle.force;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag.Equals("Block")) {
            bool isRigthMove = obstacle.isMoveRigth;
            if (rigth == isRigthMove) {
                Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
                if (rigth) {
                    rb.AddForce(Vector3.right * force, ForceMode.Impulse);
                } else {
                    rb.AddForce(Vector3.left * force, ForceMode.Impulse);
                }
            }
        }
    }
}
