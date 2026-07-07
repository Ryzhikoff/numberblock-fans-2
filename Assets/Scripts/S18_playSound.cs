using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S18_playSound : MonoBehaviour
{
    public S18_main controller;

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Block") {
            controller.playSound(other.gameObject);
        }
    }
}
