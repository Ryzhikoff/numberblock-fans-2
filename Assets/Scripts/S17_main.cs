using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S17_main : MonoBehaviour
{

    public List<Vector3> startList;
    public List<Vector3> stopList;
    public float speedDown = 2;

    public GameObject prefab1;
    public GameObject prefab2;
    public GameObject prefab3;
    public GameObject prefab4;
    public GameObject prefab5;
    public GameObject prefab10;
    public GameObject prefab100;
    public GameObject prefab200;
    public GameObject prefab300;
    public GameObject prefab400;
    public GameObject prefab500;
    public GameObject prefab1000;
    private List<GameObject> blockList;

    public AudioClip clip1;
    public AudioClip clip2;
    public AudioClip clip3;
    public AudioClip clip4;
    public AudioClip clip5;
    public AudioClip clip10;
    public AudioClip clip20;
    public AudioClip clip30;
    public AudioClip clip40;
    public AudioClip clip50;
    public AudioClip clip60;
    public AudioClip clip70;
    public AudioClip clip80;
    public AudioClip clip90;
    public AudioClip clip100;
    public AudioClip clip200;
    public AudioClip clip300;
    public AudioClip clip400;
    public AudioClip clip500;
    public AudioClip clip1000;
    public AudioClip mainClip;
    private AudioSource audio;
    private AudioSource audioMainTheme;

    private GameObject currentBlock;
    private GameObject oldBlock;
    private int counter = 0;
    private bool needDown = false;
    private float delay = 2f;


    private void Start() {
        audio = gameObject.AddComponent<AudioSource>();
        audioMainTheme = gameObject.AddComponent<AudioSource>();
        audioMainTheme.PlayOneShot(mainClip);
        audioMainTheme.volume = 0.3f;
        createGoList();
        Invoke("newBlock", 1);
    }

    private void newBlock() {
        if (counter > 19) {
            print("stop");
            return;
        }
        currentBlock = Instantiate<GameObject>(getPrefab());
        currentBlock.transform.position = startList[counter];
        //print("New Block. Prefab: " + getPrefab() + " counter:  " + counter + " startPos: " + startList[counter] + " stopPos: " + stopList[counter]);
        needDown = true;
    }

    private void Update() {
        if (needDown) {
            currentBlock.transform.position = new Vector3(
                currentBlock.transform.position.x,
                currentBlock.transform.position.y - speedDown*Time.deltaTime,
                currentBlock.transform.position.z);
            if ( currentBlock.transform.position.y <= stopList[counter].y) {
                needDown = false;
                changeBlock();
                playSound();
                moveCamera();
                counter++;
                Invoke("newBlock", delay);
            }
        }
    }
    
    private void changeBlock() {
        if (counter < 6) {
            if (oldBlock != null)
                Destroy(oldBlock);
            Destroy(currentBlock);
            oldBlock = Instantiate<GameObject>(blockList[counter]);
            oldBlock.transform.position = stopList[0];
        } else if (counter < 14) {
            Destroy(currentBlock);
            GameObject go = Instantiate<GameObject>(getPrefab());
            go.transform.position = stopList[counter];
        } else {
            GameObject[] gList = GameObject.FindGameObjectsWithTag("Block");
            foreach (GameObject g in gList) {
                Destroy(g);
            }
            switch (counter) {
                case 14:
                    oldBlock = Instantiate<GameObject>(blockList[6]);
                    oldBlock.transform.position = stopList[0];
                    break;
                case 15:
                    oldBlock = Instantiate<GameObject>(blockList[7]);
                    oldBlock.transform.position = stopList[0];
                    break;
                case 16:
                    oldBlock = Instantiate<GameObject>(blockList[8]);
                    oldBlock.transform.position = stopList[0];
                    break;
                case 17:
                    oldBlock = Instantiate<GameObject>(blockList[9]);
                    oldBlock.transform.position = stopList[0];
                    break;
                case 18:
                    oldBlock = Instantiate<GameObject>(blockList[10]);
                    oldBlock.transform.position = stopList[0];
                    break;
                case 19:
                    oldBlock = Instantiate<GameObject>(blockList[11]);
                    oldBlock.transform.position = stopList[0];
                    break;
            }
        }
    }

    private void moveCamera() {
        Camera.main.GetComponent<S17_camera>().checkMove(counter);
    }

    private GameObject getPrefab() {
        switch (counter) {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
                //блок 1
                return blockList[0];
            case 5:
                //блок 5
                return blockList[4];
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
                //блок 10
                return blockList[5];
            case 15:
            case 16:
            case 17:
            case 18:
                //блок 100
                return blockList[6];
            case 19:
                //блок 500
                return blockList[10];
            default:
                return blockList[0];
        }
    }

    private void playSound() {
        switch (counter) {
            case 0:
                audio.PlayOneShot(clip1);
                break;
            case 1:
                audio.PlayOneShot(clip2);
                break;
            case 2:
                audio.PlayOneShot(clip3);
                break;
            case 3:
                audio.PlayOneShot(clip4);
                break;
            case 4:
                audio.PlayOneShot(clip5);
                break;
            case 5:
                audio.PlayOneShot(clip10);
                break;
            case 6:
                audio.PlayOneShot(clip20);
                break;
            case 7:
                audio.PlayOneShot(clip30);
                break;
            case 8:
                audio.PlayOneShot(clip40);
                break;
            case 9:
                audio.PlayOneShot(clip50);
                break;
            case 10:
                audio.PlayOneShot(clip60);
                break;
            case 11:
                audio.PlayOneShot(clip70);
                break;
            case 12:
                audio.PlayOneShot(clip80);
                break;
            case 13:
                audio.PlayOneShot(clip90);
                break;
            case 14:
                audio.PlayOneShot(clip100);
                break;
            case 15:
                audio.PlayOneShot(clip200);
                break;
            case 16:
                audio.PlayOneShot(clip300);
                break;
            case 17:
                audio.PlayOneShot(clip400);
                break;
            case 18:
                audio.PlayOneShot(clip500);
                break;
            case 19:
                audio.PlayOneShot(clip1000);
                break;
        }
    }

    private void createGoList() {
        blockList = new List<GameObject>() {
            prefab1,
            prefab2,
            prefab3,
            prefab4,
            prefab5,
            prefab10,
            prefab100,
            prefab200,
            prefab300,
            prefab400,
            prefab500,
            prefab1000};
    }
}
