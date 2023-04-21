using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{
    private Blocks blocks;

    private void Start() {
        blocks = (Blocks) gameObject.GetComponent<Blocks>();
        //blocks.spawnBLock();
    }

    private void FixedUpdate() {
        if (blocks.moveBlock) {
            blocks.move();
            blocks.reSize();
        }
    }
}
