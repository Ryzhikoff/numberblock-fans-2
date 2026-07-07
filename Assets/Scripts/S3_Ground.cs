using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S3_Ground : MonoBehaviour
{
    public S3_ToTrillion controller;
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Block") {
            controller.stopMoveBlock();
        }
    }
}
