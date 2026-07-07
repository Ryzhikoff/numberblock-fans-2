using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class S60_Main : MonoBehaviour {

    public List<GameObject> leftBlocks = new();
    public List<GameObject> rightBlocks = new();

    private GameObject currentLeftBlock;
    private GameObject currentRightBlock;

    public Vector3 startPositionForLeftBlock = Vector3.zero;
    private Vector3 _startPositionForLeftBlock = Vector3.zero;
    public Vector3 startPositionForRightBlock = Vector3.zero;

    public Vector3 endPositionForLeftBlock = Vector3.zero;
    public Vector3 endPositionForRightBlock = Vector3.zero;

    private int counter = 0;
    public int speed = 5;

    private bool movedLeftBlock = false;
    private bool movedRightBlock = false;

    // Start is called before the first frame update
    void Start() {
        PrepareMoveRightBlock();
    }

    private void Update() {
        if (movedLeftBlock) {
            MoveLeftBlock();
        }
        if (movedRightBlock) {
            MoveRightBlock();
        }
    }

    private void PrepareMoveLeftBlock() {
        print("PrepareMoveLeftBlock");
        if (counter >= leftBlocks.Count)
            return;

        currentLeftBlock = Instantiate(leftBlocks[counter]);
        var scale = currentLeftBlock.GetComponent<Scale>();

        _startPositionForLeftBlock = new Vector3(startPositionForLeftBlock.x, startPositionForLeftBlock.y - scale.y, startPositionForLeftBlock.z);

        currentLeftBlock.transform.position = _startPositionForLeftBlock;
        movedLeftBlock = true;
        counter++;
    }

    private void MoveLeftBlock() {
        if (currentLeftBlock != null) {
            currentLeftBlock.transform.position += speed * Time.deltaTime * Vector3.up;
            if (currentLeftBlock.transform.position.y >= endPositionForLeftBlock.y) {
                movedLeftBlock = false;
                Destroy(currentLeftBlock);
                PrepareMoveRightBlock();
            }
        }
    }

    private void PrepareMoveRightBlock() {

        print("PrepareMoveRightBlock");
        if (counter >= rightBlocks.Count)
            return;
        currentRightBlock = Instantiate(rightBlocks[counter]);
        currentRightBlock.transform.position = startPositionForRightBlock;
        movedRightBlock = true;
    }

    private void MoveRightBlock() {
        if (currentRightBlock != null) {
            currentRightBlock.transform.position += speed * Time.deltaTime * Vector3.down;
            if (currentRightBlock.transform.position.y <= endPositionForRightBlock.y) {
                movedRightBlock = false;
                Destroy (currentRightBlock);
                PrepareMoveLeftBlock();
            }
        }
    }



}