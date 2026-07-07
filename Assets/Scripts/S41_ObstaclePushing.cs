using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S41_ObstaclePushing : MonoBehaviour
{
    public float forceRigthLeft = 10;
    public float forceUp = 5;
    public bool isRigth = true;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag.Equals("Block")) {
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            if (isRigth) {
                rb.AddForce(Vector3.right * forceRigthLeft, ForceMode.Impulse);
            } else {
                rb.AddForce(Vector3.left * forceRigthLeft, ForceMode.Impulse);
            }
            rb.AddForce(Vector3.up * forceUp, ForceMode.Impulse);
        }
    }
}
