using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMove : MonoBehaviour
{
    Rigidbody rigidbody;
    public float power = 5f;
    private void Start() {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.AddForce(0,0, power, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Barier") {
            rigidbody.AddForce(Random.value*10, Random.value*10, Random.value*10, ForceMode.Impulse);
        }
    }
}
