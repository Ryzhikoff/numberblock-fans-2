using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S34_PlaySound : MonoBehaviour
{
    public S34_Main s34_main;
    private GameObject oldBlock;
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Block") {
            if (other.gameObject != oldBlock) {
                oldBlock = other.gameObject;
                s34_main.playSound();
            }
        }
    }
}
