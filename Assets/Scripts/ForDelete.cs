using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForDelete : MonoBehaviour
{
    public GameObject controllerGO;
    public EarthBlock earth;
    private void Start() {
        earth = controllerGO.GetComponent<EarthBlock>();
    }
    private void OnTriggerEnter(Collider other) {
        //print("name: " + other.name + " tag: " + other.tag);
        if (other.tag == "Block") {
            earth.delBlock();
            earth.addBlock();
        }
    }
}
