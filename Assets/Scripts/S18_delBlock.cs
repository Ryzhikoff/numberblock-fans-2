using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S18_delBlock : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Block") {
            MeshRenderer[] renderers = other.gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer render in renderers) {
                render.enabled = false;
            }
        }
    }
}
