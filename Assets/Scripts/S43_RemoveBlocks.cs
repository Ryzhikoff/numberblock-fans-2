using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S43_RemoveBlocks : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        Destroy(other.gameObject);
    }
}
