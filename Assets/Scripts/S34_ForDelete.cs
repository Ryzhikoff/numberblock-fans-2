using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S34_ForDelete : MonoBehaviour
{

    public S34_Main s34_main;
    private GameObject oldBlock;
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Block") {
            if (other.gameObject != oldBlock) {
                print(other.name);
                oldBlock = other.gameObject;
                s34_main.collidered(other.gameObject);
            }
        }   
    }
}
