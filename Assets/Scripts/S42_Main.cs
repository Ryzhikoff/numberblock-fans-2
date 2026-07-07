using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class S42_Main : MonoBehaviour
{
    public List<GameObject> blocks;
    public GameObject parentFoGo;
    public float speedMove;
    public float speedResize;
    private float deltaMove;
    private float deltaResize;
    public float extraAddY = 0.1f;

    public float scaleFirstForAdd = 0.1f;
    public int counter = 0;
    public float positionYForDel = 15f;

    public List<GameObject> prefabs;

    private bool needAddBlocks = false;

    private void Update() {
        deltaMove = speedMove * Time.deltaTime;
        deltaResize = speedResize * Time.deltaTime;
        enumBlocks();
        if(needAddBlocks) {
            addBlock(3);
            delBlocks(1);
            needAddBlocks = false;
        }
    }

    private void enumBlocks() {
        float cumulativeDeltaY = 0;
        float lastBlockYPosition = 0;


        foreach (GameObject block in blocks) {

            block.transform.localScale = new Vector3(
                block.transform.localScale.x / 100 * (100 - deltaResize),
                block.transform.localScale.y / 100 * (100 - deltaResize),
                block.transform.localScale.z / 100 * (100 - deltaResize));

            //cumulativeDeltaY ;

            float scaleY = block.GetComponent<Scale>().y;
            float fixScaleY = scaleY * block.transform.localScale.y;
            //lastBlockYPosition += fixScaleY;

            float y = 0;
            if (lastBlockYPosition == 0) {
                y = block.transform.position.y + deltaMove;
            } else {
                y = lastBlockYPosition - fixScaleY;
            }

            block.transform.position = new Vector3(
                block.transform.position.x,
                y,
                block.transform.position.z);

            lastBlockYPosition = block.transform.position.y;
            //block.transform.position += Vector3.up * delta;
            //print("name: " + block.name + " block.transform.lossyScale: " + block.transform.lossyScale + " block.transform.localScale: " + block.transform.localScale);
        }

        if (blocks[0].transform.position.y > positionYForDel) {
            needAddBlocks = true;
        }

    }

    private void addBlock(int countBlock) {
        for (int i = 0; i < countBlock; i++) {

            GameObject newBlock = Instantiate<GameObject>(getPrefab());
            Vector3 position = blocks[blocks.Count - 1].transform.position;
            position += Vector3.down * newBlock.GetComponent<Scale>().y;
            newBlock.transform.position = position;

            float scaleOldBlock = blocks[0].transform.localScale.x;
            float localScale = scaleOldBlock / 0.1f;

            newBlock.transform.localScale = new Vector3(localScale, localScale, localScale);
            blocks.Add(newBlock);
            print("Добавили блок: " + newBlock.name);
        }
    }

    private GameObject getPrefab() {
        switch (counter) {
            case 1:
                counter++;
                return prefabs[0];
            case 2:
                counter++;
                return prefabs[1];
            case 3:
                counter = 1;
                return prefabs[2];
            default:
                return prefabs[1];
        }
    }

    private void delBlocks(int countBlock) {
        List<GameObject> listBLocks = new List<GameObject>();
        for (int i = 0; i < countBlock; i++) {
            listBLocks.Add(blocks[i]);
        }

        foreach(GameObject go in listBLocks) {
            blocks.Remove(go);
            Destroy(go);
        }
    }
}

