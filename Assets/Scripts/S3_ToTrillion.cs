using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S3_ToTrillion : MonoBehaviour
{
    public List<Vector3> startPositions;
    public float delayAfterDown;
    public float speedDown = 5;

    public GameObject prefabOne;
    public GameObject prefabOneHundred;
    public GameObject prefabOneThousand;
    public GameObject prefabOneMillion;
    public GameObject prefabOneBillion;
    public GameObject prefabOneTrillion;
    private List<GameObject> listBlock;
    private int counter = 0;

    private GameObject oldBlock;
    private GameObject currentBlock;
    private float stopY = 1;
    private bool needDown = false;

    private SoundMaster soundMaster;
    public AudioClip clipOne;
    public AudioClip clipHundred;
    public AudioClip clipThousand;
    public AudioClip clipMillion;
    public AudioClip clipBillion;
    public AudioClip clipTrillion;
    private AudioSource audio;
    private List<AudioClip> listClip;
   

    private void Start() {
        createListBlock();
        audio = gameObject.AddComponent<AudioSource>();
        soundMaster = gameObject.GetComponent<SoundMaster>();
        oldBlock = Instantiate<GameObject>(getPrefab());
        oldBlock.transform.position = startPositions[counter];
        Invoke("started", 2);
    }

    private void started() {
        //soundMaster.playSound(counter);
        counter++;
        playSound();
        print(oldBlock.name);
        Invoke("addNewBlock", delayAfterDown);
    }

    private void addNewBlock() {
        GameObject go = Instantiate<GameObject>(getPrefab());
        go.transform.position = startPositions[counter];
        currentBlock = go;
        counter++;
        needDown = true;
        soundMaster.playDownSound(true);
    }

    private void Update() {
        if (needDown) {
            downMove();
        }
    }

    public void stopMoveBlock() {
        needDown = false;
        Destroy(oldBlock);
        oldBlock = currentBlock;
        playSound();
        soundMaster.playBoom();
        soundMaster.playDownSound(false);
        //soundMaster.playBoom();
        speedDown *= 2;
        Invoke("addNewBlock", delayAfterDown);
    }

    private void downMove() {
        currentBlock.transform.position = new Vector3(
            currentBlock.transform.position.x,
            currentBlock.transform.position.y - speedDown * Time.deltaTime,
            currentBlock.transform.position.z);

        /*if (currentBlock.transform.position.y <= stopY) {
            needDown = false;
            currentBlock.transform.position = new Vector3(
                0,
                stopY,
                0);
            //soundMaster.playBoom();
            Invoke("addNewBlock", delayAfterDown);
        }*/
    }

    private GameObject getPrefab() {
        return listBlock[counter];
    }

    private void playSound() {
        audio.PlayOneShot(listClip[counter - 1]);
    }
    private void createListBlock() {
        listBlock = new List<GameObject> {
            prefabOne,
            prefabOneHundred,
            prefabOneThousand,
            prefabOneMillion,
            prefabOneBillion,
            prefabOneTrillion
        };
        listClip = new List<AudioClip> {
            clipOne,
            clipHundred,
            clipThousand,
            clipMillion,
            clipBillion,
            clipTrillion
        };
    }
}
