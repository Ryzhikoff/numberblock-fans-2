using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class S21_Main : MonoBehaviour
{
    public GameObject prefabBlockOne;
    public GameObject prefabBlockOneThousand;
    public GameObject prefabBlockLine2;
    public GameObject prefabBlockLine3;
    public GameObject prefabBlockLine4;
    public GameObject prefabBlockLine5;
    public GameObject prefabBlockLine6;
    public GameObject prefabBlockLine7;
    public GameObject prefabBlockLine8;
    public GameObject prefabBlockLine9;
    public GameObject prefabBlockLine10;

    public GameObject prefabBlockLineThousand2;
    public GameObject prefabBlockLineThousand3;
    public GameObject prefabBlockLineThousand4;
    public GameObject prefabBlockLineThousand5;
    public GameObject prefabBlockLineThousand6;
    public GameObject prefabBlockLineThousand7;
    public GameObject prefabBlockLineThousand8;
    public GameObject prefabBlockLineThousand9;
    public GameObject prefabBlockLineThousand10;

    [Header("Speed Block to Down")]
    public float speedBlockDown = 5f;
    public float offsetStartY = 15f;

    public TextMeshProUGUI textCounter;
    private int counter = 1;

    private List<GameObject> listBlockLine = new List<GameObject>();
    private List<GameObject> listBlockLinePrefab;

    private int arrayLength = 10;
    private bool[,,] matrix;
    private bool needMoveDown = false;
    private GameObject currentBlock;
    private GameObject currentBlockLine;
    private Vector3 targetPosition;

    public AudioClip sound1;
    public AudioClip sound100;
    public AudioClip sound200;
    public AudioClip sound300;
    public AudioClip sound400;
    public AudioClip sound500;
    public AudioClip sound600;
    public AudioClip sound700;
    public AudioClip sound800;
    public AudioClip sound900;
    public AudioClip sound1000;
    public AudioClip boom;

    public float volumeSoundVoice = 0.7f;
    public float volumeSoundBoom = 0.1f;

    private AudioSource audioVoice;
    private AudioSource audioBoom;


    private void Start() {
        audioVoice = gameObject.AddComponent<AudioSource>();
        audioBoom = gameObject.AddComponent<AudioSource>();
        audioVoice.volume = volumeSoundVoice;
        audioBoom.volume = volumeSoundBoom;
        matrix = new bool[arrayLength, arrayLength, arrayLength];
        createListBlockLinekPrefab();
        firstStart();
        Invoke("addNewBlock", 1f);
    }

    private void firstStart() {
        matrix[0, 0, 0] = true;
        GameObject go = Instantiate(prefabBlockOne);
        go.transform.position = Vector3.zero;
        listBlockLine.Add(go);
        counter++;
    }

    private void addNewBlock() {
        if (matrix[arrayLength-1, arrayLength - 1, arrayLength - 1]) {
            deleteAllBlock();
            GameObject go = Instantiate<GameObject>(prefabBlockOneThousand);
            go.transform.position = Vector3.zero;
            
            return;
        }
        /*if (currentBlock != null) {
            Destroy(currentBlock);
        }*/

        GameObject goTemp = Instantiate<GameObject>(prefabBlockOne);
        targetPosition = getNextCoords();
        goTemp.transform.position = new Vector3(
            targetPosition.x,
            targetPosition.y + offsetStartY * 10,
            targetPosition.z);
        listBlockLine.Add(goTemp);
        currentBlock = goTemp;
        needMoveDown = true;
    }

    private void Update() {
        if (needMoveDown)
            moveBlockDown();
    }

    private Vector3 getNextCoords() {
        for (int y = 0; y < arrayLength; y++) {
            for (int x = 0; x < arrayLength; x++) {
                for (int z = 0; z < arrayLength; z++) {
                    if (matrix[x, y, z]) {
                        continue;
                    }
                    matrix[x, y, z] = true;
                    return new Vector3(x * 10, y * 10, z * 10);
                }
            }
        }
        return Vector3.zero;
    }

    private void moveBlockDown() {
        currentBlock.transform.position = new Vector3(
            currentBlock.transform.position.x,
            currentBlock.transform.position.y - speedBlockDown * Time.deltaTime,
            currentBlock.transform.position.z);
        if (currentBlock.transform.position.y <= targetPosition.y) {
            //print("currentBlock.transform.position.y: " + currentBlock.transform.position.y + " targetPosition.y: " + targetPosition.y);
            currentBlock.transform.position = targetPosition;
            textCounter.text = (counter * 1000).ToString("#,#", CultureInfo.InvariantCulture);
            playSound();
            counter++;
            needMoveDown = false;
            //checkNewOrChangeLineBlock();
            addNewBlock();
        }
    }

    private void checkNewOrChangeLineBlock() {
        //для первого случая, когда currentBlockLine еше не инициализирован
        if (currentBlockLine == null || ((int)targetPosition.z) == 0) {
            addBlockLine();
            return;
        }

        changeBlockLine();
       
    }

    private void addBlockLine() {

        if(currentBlockLine != null) {
            GameObject go = currentBlockLine;
            listBlockLine.Add(go);
        }

        currentBlockLine = Instantiate<GameObject>(prefabBlockOne);
        currentBlockLine.transform.position = targetPosition;
        
        addNewBlock();
    }

    private void changeBlockLine() {
        Destroy(currentBlockLine);
        currentBlockLine = Instantiate<GameObject>(listBlockLinePrefab[(int)targetPosition.z - 1]);
        currentBlockLine.transform.position = new Vector3(
            targetPosition.x,
            targetPosition.y,
            0);
        addNewBlock();
    }

    private void deleteAllBlock() {
        GameObject[] goList = GameObject.FindGameObjectsWithTag("Block");
        foreach(GameObject go in goList) {
            Destroy(go);
        }
    }

    private void playSound() {
        audioBoom.PlayOneShot(boom);
        switch (counter) {
            case 1:
                audioVoice.PlayOneShot(sound1);
                return;
            case 100:
                audioVoice.PlayOneShot(sound100);
                break;
            case 200:
                audioVoice.PlayOneShot(sound200);
                break;
            case 300:
                audioVoice.PlayOneShot(sound300);
                break;
            case 400:
                audioVoice.PlayOneShot(sound400);
                break;
            case 500:
                audioVoice.PlayOneShot(sound500);
                break;
            case 600:
                audioVoice.PlayOneShot(sound600);
                break;
            case 700:
                audioVoice.PlayOneShot(sound700);
                break;
            case 800:
                audioVoice.PlayOneShot(sound800);
                break;
            case 900:
                audioVoice.PlayOneShot(sound900);
                break;
            case 1000:
                audioVoice.PlayOneShot(sound1000);
                return;
            default:
                return;
        }
    }

    private void createListBlockLinekPrefab() {
        listBlockLinePrefab = new List<GameObject>() {
            prefabBlockLine2,
            prefabBlockLine3,
            prefabBlockLine4,
            prefabBlockLine5,
            prefabBlockLine6,
            prefabBlockLine7,
            prefabBlockLine8,
            prefabBlockLine9,
            prefabBlockLine10
        };
    }

}
