using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S51_Camera : MonoBehaviour
{
    public GameObject activeBlock;
    public float offsetY = 2.7f;
    public float positionZ = 20f;

    private void Update() {
        if (activeBlock != null) {
            transform.position = new Vector3(
                activeBlock.transform.position.x,
                activeBlock.transform.position.y + offsetY,
                positionZ);
        }
    }
}
