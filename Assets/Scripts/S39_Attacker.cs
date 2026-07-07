using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S39_Attacker : MonoBehaviour
{
    public Vector3 impulse;
    public float delay = 2f;
    public ForceMode forceMode = ForceMode.Force;

    private void Start() {
        Invoke("addImpulse", delay);
    }

    private void addImpulse() {
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = false;
        rigidbody.AddForce(impulse, forceMode);
    }
}
