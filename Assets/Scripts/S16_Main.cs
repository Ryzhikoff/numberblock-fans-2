using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S16_Main : MonoBehaviour
{
    public List<GameObject> blocks = new List<GameObject>();
    public float distanceRatio = 5f;
    public float lastZ = 0f;

    private void Start() {
        PlaceBlocks();
    }

    private void PlaceBlocks() {
        foreach (GameObject block in blocks) {
            Scale scale = block.GetComponent<Scale>();
            lastZ += (scale.z + scale.y * distanceRatio);
            Vector3 blockPos = new Vector3(
                0 - (scale.x / 2),
                0,
                lastZ);
            Instantiate(block);
            block.transform.position = blockPos;
        }
    }
}
