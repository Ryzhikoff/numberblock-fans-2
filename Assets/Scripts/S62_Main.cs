using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class S62_Main : MonoBehaviour
{
    public List<GameObject> blocks;
    private GameObject currentBlock;
    public Vector3 position = Vector3.zero;
    public float delay = 0.5f;
    private int counter = 0;

    private void Start() {
        PlaceNextBlock();
    }

    private void PlaceNextBlock() {
        if (counter >= blocks.Count) {
            counter = 0;
        }

        if (currentBlock != null) {
            Destroy(currentBlock);
        }

        currentBlock = Instantiate(blocks[counter], position, Quaternion.identity);
        counter++;

        Invoke(nameof(PlaceNextBlock), delay);
    }
}
