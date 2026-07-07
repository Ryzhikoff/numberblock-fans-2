using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S45_Main : MonoBehaviour
{
    public List<GameObject> prefabs;

    public int counter = 0;
    public Vector3 finalPosition = Vector3.zero;
    public float speedMoveBlock = 5f;
    public float delayAfterNewBlock = 2f;
    public TextMeshProUGUI textMesh;

    private GameObject currentBlock;
    private List<GameObject> blocks = new List<GameObject>();
    private bool isMovingBlock = false;

    private Camera camera;

    private void Start() {
        camera = Camera.main;
        setNewBlock();
    }
    private void setNewBlock() {
        if (counter > prefabs.Count - 1) {
            return;
        }
        GameObject go = Instantiate<GameObject>(prefabs[counter]);
        blocks.Add(go);
        Scale scale = go.GetComponent<Scale>();
        var startPosition = new Vector3(
            finalPosition.x,
            finalPosition.y - scale.y,
            finalPosition.z);

        go.transform.position = startPosition;
        currentBlock = go;
        isMovingBlock = true;
        counter++;
    }

    private void Update() {
        if (isMovingBlock) {
            moveBlock();
        }
    }

    private void moveBlock() {
        foreach (GameObject block in blocks) {
            block.transform.position += speedMoveBlock * Time.deltaTime * Vector3.up;
            if (currentBlock.transform.position.y >= finalPosition.y) {
                isMovingBlock = false;
                currentBlock.transform.position = finalPosition;
                setText();
                Invoke(nameof(setNewBlock), delayAfterNewBlock);
            }
        }
    }

    private void setText() {
        var number = currentBlock.GetComponent<Scale>().number.ToString();
        textMesh.text = number;
    }
}
