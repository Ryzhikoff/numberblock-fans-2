using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S38_Trigger : MonoBehaviour
{
    public S49_Main controller;
    private bool wasGenerated = false;
    private void OnTriggerEnter(Collider other) {
        if(!wasGenerated && other.CompareTag("Block")) {
            wasGenerated = true;
            print("OnTriggerEnter");
            controller.generateBlocks();
        }
    }
}
