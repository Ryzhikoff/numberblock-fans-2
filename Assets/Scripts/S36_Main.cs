using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class S36_Main : MonoBehaviour
{
    public List<GameObject> prefabList;
    public GameObject objectForTrigger;
    public float speedMove = 5f;
    public int counter = 0;
    public long stopCounter = 1000;
    public int rank = 100;
    private List<S36_Container> activeBlocks = new List<S36_Container>();
    public float startPositionX = 20;
    public float positionXforDelBlocks = -100;

    private Camera camera;

    private void Start() {
        camera = Camera.main;
        //createNewCombo();
        arrangeBlocks();
    }
    private void Update() {
        if (checkAddNewCombo())
            createNewCombo();

        moveBlock();

        if (checkForDeleteFirstBlock())
            delFisrtBlock();
    }

    private bool checkForDeleteFirstBlock() {
        var container = activeBlocks[0];
        var firstBlock = container.blocks[0];
        if (firstBlock.transform.position.x + container.sizeX < positionXforDelBlocks) {
            return true;
        }
        return false;
    }
    private void delFisrtBlock() {
        S36_Container container = activeBlocks[0];
        activeBlocks.RemoveAt(0);
        foreach(GameObject go in container.blocks) {
            Destroy(go);
        }
    }

    private void arrangeBlocks() {
        float x = 0;
        foreach (GameObject block in prefabList) {
            Scale scale = block.GetComponent<Scale>();
            GameObject go = Instantiate<GameObject>(block);
            go.transform.position = new Vector3(x, 0, 0);
            x += scale.x * 2;
        }
    }
    private bool checkAddNewCombo() {
        if (counter > stopCounter)
            return false;

        var container = activeBlocks[activeBlocks.Count - 1];
        var firstBlockPositionX = container.blocks[0].transform.position.x;
        if (firstBlockPositionX + container.sizeX * 2 < startPositionX) {
            return true;
        }
        return false;
    }

    /*    private void createNewCombo() {
            GameObject block = Instantiate<GameObject>(prefabList[counter]);
            block.transform.position = new Vector3(startPositionX, 0, 0);
            var list = new List<GameObject> {
                block
            };
            activeBlocks.Add(new S36_Container(block.GetComponent<Scale>().x, list));
            counter++;


        }*/

    private void createNewCombo() {

        var tempCounter = counter;
        var tempList = new List<GameObject>();
        var tempRank = rank;
        var tempSizeX = 0;
        var numberForText = 0;

        while (true) {

            GameObject go = null;
            int blockNumber = (int) Mathf.Floor(tempCounter / tempRank) * tempRank;

            if (tempCounter <= 10) {
                go = Instantiate<GameObject>(getBlock(tempCounter));
                tempCounter -= tempCounter;
            } else if (blockNumber != 0) {
                go = Instantiate<GameObject>(getBlock(blockNumber));
                tempCounter -= blockNumber;
            }

            if (go != null) {
                go.transform.position = new Vector3(startPositionX + tempSizeX, 0, 0);
                tempSizeX += go.GetComponent<Scale>().x;
                numberForText += (int) go.GetComponent<Scale>().number;
                tempList.Add(go);
            }

            if (tempCounter == 0) {
                GameObject g = Instantiate<GameObject>(objectForTrigger);
                g.transform.position = new Vector3(startPositionX - 1, 0, 0);
                g.name = numberForText.ToString();
                tempList.Add(g);
                break;
            }

            tempRank /= 10;
        }

        activeBlocks.Add(new S36_Container(tempSizeX, tempList));
        counter++;
        if (checkCameraMove()) {
            cameraMove();
        }

    }

    private GameObject getBlock(long blockNumber) {
       /* return prefabList[counter];*/
        foreach (GameObject block in prefabList) {
            if (int.Parse(block.name) == blockNumber) {
                return block;
            }
        }
        print("Block not founded " + blockNumber);
        return prefabList[0];
    }

    private bool checkCameraMove() {
        return counter switch {
            200 => true,
            300 => true,
            400 => true,
            500 => true,
            600 => true,
            700 => true,
            800 => true,
            900 => true,
            1000 => true,
            _ => false
        };
       /* return true;*/
    }

    private void cameraMove() {
        camera.GetComponent<S26_Main>().startCameraMove();
    }

    private void moveBlock() {
        var delta = speedMove * Time.deltaTime;
        foreach (S36_Container container in activeBlocks) {
            foreach (GameObject go in container.blocks) {
                go.transform.position += Vector3.left * delta;
            }
        }
    }
}
