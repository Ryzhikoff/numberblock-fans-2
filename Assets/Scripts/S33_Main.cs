using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S33_Main : MonoBehaviour
{
    public List<GameObject> blocks;
    public List<GameObject> listSquarePrefab;
    public List<GameObject> listNotSquarePrefab;
    public GameObject prefabOne;
    public GameObject prefabOneThousand;

    public float speed = 0.5f;
    public float sizeBigBlockForDeleteAndNew = 10000f;
    public float extraDownY = 0.01f;

    private float delta;
    private bool isSquare = true;
    private GameObject blockForDel;

    public List<TextMeshProUGUI> textMeshes;
    private int counterForListTextMesh;

    public float sizeBlockForUpdateText = 1000f;

    public int counter = 202;
    private List<GameObject> listBlockUsed = new List<GameObject>();

    public AudioClip soundMoney;
    public float volumeMoney;
    private AudioSource audio;

    private void Start() {
        audio = gameObject.AddComponent<AudioSource>();
    }

    private void Update() {
        delta = speed * Time.deltaTime;
        enumBlocks();
    }


    private void enumBlocks() {
        float cumulativePersentDelta = 0;
        foreach (GameObject block in blocks) {

            float currentSize = block.transform.localScale.x;
            float persentDelta = currentSize / 100f * delta;
            cumulativePersentDelta += persentDelta;
            reSize(block, persentDelta);
            move(block, persentDelta, cumulativePersentDelta);

            if(checkBlockForDelete(block)) {
                blockForDel = block;
            }

            if (checkUpdateText(block)) {
                updateText(block);
                //playSound();
            }
            
        }


        if (blockForDel != null) {
            deleteBlocks();
            if (checkAddNewBlock())
                addNewBlock();
        }

    }

    private void move(GameObject block, float persentDelta, float cumulativePersentDelta) {
        block.transform.position = new Vector3(
            block.transform.position.x,
            block.transform.position.y - cumulativePersentDelta - extraDownY,
            block.transform.position.z - persentDelta);
    }
    private void reSize(GameObject block, float persentDelta) {
        Vector3 deltaVector3 = new Vector3(persentDelta,persentDelta, persentDelta);
        //print("currentSize: " + currentSize + " persentDelta: " + persentDelta + " deltaVector3: " + deltaVector3);
        block.transform.localScale += deltaVector3;
    }

    private bool checkBlockForDelete(GameObject block) {
       return block.transform.localScale.x >= sizeBigBlockForDeleteAndNew;
            
    }

    private bool checkAddNewBlock() {
        return counter > 1;
    }

    private void addNewBlock() {
        GameObject oldBlock = blocks[0];
        float x = oldBlock.transform.position.x;
        float y = oldBlock.transform.position.y + oldBlock.transform.localScale.x;
        float z = oldBlock.transform.position.z / 10f + 900f;
        float size = oldBlock.transform.localScale.x / 10f;

        GameObject newBlock = Instantiate<GameObject>(getPrefab());
        newBlock.transform.position = new Vector3(x, y, z);
        newBlock.transform.localScale = new Vector3(size, size, size);
        blocks.Insert(0, newBlock);
    }

    private void deleteBlocks() {
        blocks.Remove(blockForDel);
        listBlockUsed.Remove(blockForDel);
        Destroy(blockForDel);
        blockForDel = null;
    }

    private bool checkUpdateText(GameObject block) {
        foreach (GameObject blockUsed in listBlockUsed) {
            if (block == blockUsed)
                return false;
        }

        return block.transform.localScale.x >= sizeBlockForUpdateText; 
    }

    
    private void updateText(GameObject block) {

        listBlockUsed.Add(block);
        //TextMeshProUGUI textMesh = getTextMeshProUGUI();
        TextMeshProUGUI textMesh = textMeshes[0];
        string number = textMesh.text;

        if (number.Length < 4)
            return;

        /*if (counterForListTextMesh < 2) {
            number = number.Substring(4);
        } else {
            number = number.Substring(0, number.Length - 4);
        }*/
        number = number.Substring(0, number.Length - 4);
        textMesh.text = number;
        print(textMesh.text);
        print("Counter in updateText:" + counter);
    }

    private TextMeshProUGUI getTextMeshProUGUI() {
        if (textMeshes[counterForListTextMesh].text != "") {
            return textMeshes[counterForListTextMesh];
        }
        counterForListTextMesh++;
        return getTextMeshProUGUI();
    }

    private void playSound() {
        audio.volume = volumeMoney;
        audio.PlayOneShot(soundMoney);
    }

    private GameObject getPrefab() {
        counter--;
        print("Counter in getPrefab" + counter);

        if (counter == 1)
            return prefabOne;
        if (counter == 2)
            return prefabOneThousand;

        if (isSquare) {
            int index = Random.Range(0, listSquarePrefab.Count);
            isSquare = !isSquare;
            return listSquarePrefab[index];
        } else {
            int index = Random.Range(0, listNotSquarePrefab.Count);
            isSquare = !isSquare;
            return listNotSquarePrefab[index];
        }
    }
}
