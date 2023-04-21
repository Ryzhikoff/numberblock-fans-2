using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FromOneToGoogle : MonoBehaviour
{
    public GameObject prefabOne;
    public GameObject prefabTen;
    public GameObject prefabHundred;
    public GameObject prefabThousand; //3
    public GameObject prefabTenThousand;
    public GameObject prefabOneHundredThousand;
    public GameObject prefabMillion;

    public GameObject prefabTenMillion; //7
    public GameObject prefabOneHundredMillion;
    public GameObject prefabBillion;

    public GameObject prefabTenBillion;
    public GameObject prefabOneHundredBillion;
    public GameObject prefabTrillion;

    public AudioClip clipOne; //1
    public AudioClip clipTen; //10
    public AudioClip clipHundred; //100
    public AudioClip clipThousand; //1 000
    public AudioClip clipTenThousand; //10 000
    public AudioClip clipOneHundredThousand; //100 000
    public AudioClip clipMillion; //1 000 000

    public AudioClip clipTenMillion; //10 000 000
    public AudioClip clipOneHundredMillion; //100 000 000
    public AudioClip clipBillion; //1 000 000 000

    public AudioClip clipTenBillion; //10 000 000 000
    public AudioClip clipOneHundredBillion; //100 000 000 000
    public AudioClip clipTrillion; //1 000 000 000 000

    private AudioSource audio;
    private AudioSource audioForBoom;
    public AudioClip clipBigBoom;


    public Vector3 startPosition = Vector3.zero;

    public SoundMaster soundMaster;

    //
    private int counter = 0;
    private SizeBlock state;

    //текущий префаб
    private GameObject currentPrefab;
    private GameObject currentGo;
    //объект для ресайза
    private GameObject parentGO;
    //координаты движения
    private Vector3 startCoords;
    private Vector3 currentCoords;
    private Vector3 targetCoords;


    private bool[,] matrixOne;
    public float speedOne = 20;
    private bool[,] matrixTen;
    private bool[] matrixHundred;

    private bool cameraMove = false;
    private int interpolationFramesCount = 100;
    int elapsedFrames = 0;

    private Vector3 currentPosition;
    private Vector3 targetPosition;
    public List<Vector3> cameraCoords = new List<Vector3> {
        new Vector3(4.832f, 6.061f, -6.237f),
        new Vector3(13.6f, 12.089f, -11.011f),
        new Vector3(15.562f, 13.265f, -10.772f),
        new Vector3(48.15f, 52.54f, -35.03f),
        new Vector3(115.75f, 98.29f, -68.58f),
        new Vector3(166.4f, 120.6f, -82f),
        new Vector3(178.04f, 120.6f, -76.61f)
    };

    public TextMeshProUGUI textUi;
    
    private long textCounter = 1;
    private long rank = 1;

    private bool resize = false;

    private void Start() {
        state = SizeBlock.Null;

        audio = gameObject.AddComponent<AudioSource>();
        audioForBoom = gameObject.AddComponent<AudioSource>();

        Camera.main.transform.position = cameraCoords[0];
        
        Invoke("firstInit", 2f);
    }

    private void firstInit() {
        setNextPrefab();
        GameObject go = Instantiate<GameObject>(currentPrefab);
        go.transform.position = startPosition;
        setText();
        //координаты куда будем ставить следующий блок
        targetCoords = new Vector3(
            go.transform.position.x,
            go.transform.position.y + go.transform.localScale.y,
            go.transform.position.z);
        startCoords = new Vector3(
            targetCoords.x,
            targetCoords.y + go.transform.localScale.y * 10,
            targetCoords.x);

        currentGo = Instantiate<GameObject>(currentPrefab);
        currentGo.transform.position = startCoords;


        matrixOne = new bool[2, 5];
        //первый уже стоит
        matrixOne[0, 0] = true;
        matrixOne[0, 1] = true;

        playSound(counter + 1);
        Invoke("startOne", 1);

    }


    private void Update() {
        
        switch (state) {
            case SizeBlock.One:
                paintOne();
                break;
            case SizeBlock.Ten:
                paintTen();
                break;
            case SizeBlock.Hundred:
                paintHundred();
                break;
            
            default:
                
                break;
        }

        if (cameraMove) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(currentPosition, targetPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            Camera.main.transform.position = interpolatedPosition;
            if (Mathf.Abs(Camera.main.transform.position.x - targetPosition.x) < 0.05f) {
                elapsedFrames = 0;
                interpolationFramesCount = 100;
                cameraMove = false;
                currentPosition = Camera.main.transform.position;
            }
        }

        if (resize) {
            parentGO.transform.localScale = new Vector3(
                    parentGO.transform.localScale.x - (parentGO.transform.localScale.x / 100 * 90) * Time.deltaTime,
                    parentGO.transform.localScale.y - (parentGO.transform.localScale.y / 100 * 90) * Time.deltaTime,
                    parentGO.transform.localScale.z - (parentGO.transform.localScale.z / 100 * 90) * Time.deltaTime);
            if (parentGO.transform.localScale.x <= 1) {
                resize = false;
            }
        }
    }

    private void startOne() {
        matrixOne = new bool[2, 5];
        //первый уже стоит
        matrixOne[0, 0] = true;
        matrixOne[0, 1] = true;
        state = SizeBlock.One;
    }

    
    private void paintOne() {
        
        currentGo.transform.position = new Vector3(
            currentGo.transform.position.x,
            currentGo.transform.position.y - speedOne * Time.deltaTime,
            currentGo.transform.position.z);

        //если встал на место...
        if (currentGo.transform.position.y <= targetCoords.y) {
            playBoom();
            setText();
            currentGo.transform.position = targetCoords;
            //print("currentGo.transform.position.y: " + currentGo.transform.position.y + " targetCoords.y: " + targetCoords.y);
            if (!matrixOne[1, 4]) {
                for (int x = 0; x < 2; x++) {
                    for (int y = 0; y < 5; y++) {
                        if (matrixOne[x, y]) { continue; }

                        float scaleY = currentGo.GetComponent<Scale>().scale.y;
                        float scaleX = currentGo.GetComponent<Scale>().scale.x;

                        int i;
                        if (counter < 4) { i = 10; } else {
                            i = 5;
                        }

                        //обновляем целевые и стартовые координаты для слудющего блока
                        targetCoords = new Vector3(
                             x * scaleX,
                             y * scaleY,
                             currentGo.transform.position.z);
                        startCoords = new Vector3(
                            targetCoords.x,
                            targetCoords.y + scaleY * 10,
                            targetCoords.z);
                        //инициализируем следующий блок
                        GameObject go = Instantiate<GameObject>(currentPrefab);
                        currentGo = go;
                        currentGo.transform.position = startCoords;
                        //print("x: " + x + " y: " + y + " targetCoords: " + targetCoords + " startCoords: " + startCoords);
                        matrixOne[x, y] = true;

                        return;
                    }
                }
            } else {
                counter++;
                playSound(counter + 1);
                setNextPrefab();
                clearBlocks();
                //инициализирует Ten
                parentGO = Instantiate<GameObject>(currentPrefab);
                parentGO.transform.position = startPosition;

                float scaleY = parentGO.GetComponent<Scale>().scale.y;
                float scaleX = parentGO.GetComponent<Scale>().scale.x;

                targetCoords = new Vector3(
                    parentGO.transform.position.x + scaleX,
                    parentGO.transform.position.y,
                    parentGO.transform.position.z);
                startCoords = new Vector3(
                    targetCoords.x,
                    targetCoords.y + scaleY * 2,
                    targetCoords.z);

                currentGo = Instantiate<GameObject>(currentPrefab);
                currentGo.transform.position = startCoords;
                state = SizeBlock.Null;
                Invoke("cameraMoveStart", 2);
                Invoke("startTen", 4);
                
            }
        }
    }

    private void startTen() {
        matrixTen = new bool[5, 2];
        matrixTen[0, 0] = true;
        matrixTen[1, 0] = true;
        state = SizeBlock.Ten;
    }
    private void paintTen() {
        currentGo.transform.position = new Vector3(
            currentGo.transform.position.x,
            currentGo.transform.position.y - speedOne * Time.deltaTime,
            currentGo.transform.position.z);

        //если встал на место...
        if (currentGo.transform.position.y <= targetCoords.y) {
            playBoom();
            setText();
            currentGo.transform.position = targetCoords;
            //print("currentGo.transform.position.y: " + currentGo.transform.position.y + " targetCoords.y: " + targetCoords.y);
            if (!matrixTen[4, 1]) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 5; x++) {
                        if (matrixTen[x, y]) { continue; }

                        float scaleY = currentGo.GetComponent<Scale>().scale.y;
                        float scaleX = currentGo.GetComponent<Scale>().scale.x;

                        //обновляем целевые и стартовые координаты для слудющего блока
                        targetCoords = new Vector3(
                             x * scaleX,
                             y * scaleY,
                             currentGo.transform.position.z);
                        startCoords = new Vector3(
                            targetCoords.x,
                            targetCoords.y + scaleY*2,
                            targetCoords.z);
                        //инициализируем следующий блок
                        GameObject go = Instantiate<GameObject>(currentPrefab);
                        currentGo = go;
                        currentGo.transform.position = startCoords;
                        //print("x: " + x + " y: " + y + " targetCoords: " + targetCoords + " startCoords: " + startCoords);
                        matrixTen[x, y] = true;

                        return;
                    }
                }
            } else {
                counter++;
                playSound(counter + 1);
                setNextPrefab();
                clearBlocks();
                //инициализирует Hundred
                parentGO = Instantiate<GameObject>(currentPrefab);
                parentGO.transform.position = startPosition;

                float scaleY = currentGo.GetComponent<Scale>().scale.y;
                float scaleZ = currentGo.GetComponent<Scale>().scale.z;

                targetCoords = new Vector3(
                    parentGO.transform.position.x,
                    parentGO.transform.position.y,
                    parentGO.transform.position.z + scaleZ);

                startCoords = new Vector3(
                    targetCoords.x,
                    targetCoords.y + scaleY *1.5f,
                    targetCoords.z);

                currentGo = Instantiate<GameObject>(currentPrefab);
                currentGo.transform.position = startCoords;
                state = SizeBlock.Null;
                Invoke("cameraMoveStart", 2);
                Invoke("startHundred", 3);
            }
        }
    }

    private void startHundred() {
        matrixHundred = new bool[10];
        matrixHundred[0] = true;
        matrixHundred[1] = true;
        state = SizeBlock.Hundred;
        if (counter > 4) {
            speedOne *= 2;
        }
    }
    private void paintHundred() {
        currentGo.transform.position = new Vector3(
              currentGo.transform.position.x,
              currentGo.transform.position.y - speedOne * Time.deltaTime,
              currentGo.transform.position.z);
        //если встал на место...
        if (currentGo.transform.position.y <= targetCoords.y) {
            playBoom();
            setText();
            currentGo.transform.position = targetCoords;
            //print("currentGo.transform.position.y: " + currentGo.transform.position.y + " targetCoords.y: " + targetCoords.y);
            if (!matrixHundred[9]) {
                for (int z = 0; z < 10; z++) {
                    if (matrixHundred[z])
                        continue;

                    float scaleY = currentGo.GetComponent<Scale>().scale.y;
                    float scaleZ = currentGo.GetComponent<Scale>().scale.z;

                    //обновляем целевые и стартовые координаты для слудющего блока
                    targetCoords = new Vector3(
                         0,
                         0,
                         z * scaleZ);
                    startCoords = new Vector3(
                        targetCoords.x,
                        targetCoords.y + scaleY * 1.5f,
                        targetCoords.z);

                    //инициализируем следующий блок
                    GameObject go = Instantiate<GameObject>(currentPrefab);
                    currentGo = go;
                    currentGo.transform.position = startCoords;
                    //print("x: " + x + " y: " + y + " targetCoords: " + targetCoords + " startCoords: " + startCoords);
                    matrixHundred[z] = true;

                    return;
                }
            } else {
                counter++;
                playSound(counter + 1);
                setNextPrefab();
                clearBlocks();
                //инициализирует Thousand
                parentGO = Instantiate<GameObject>(currentPrefab);
                parentGO.transform.position = startPosition;

                targetCoords = new Vector3(
                    parentGO.transform.position.x,
                    parentGO.transform.position.y + parentGO.GetComponent<Scale>().scale.y,
                    parentGO.transform.position.z);
                //растояние в высоту
                int i;
                if (counter < 4) { i = 10; } else {
                    i = 2;
                } 
                startCoords = new Vector3(
                    targetCoords.x,
                    targetCoords.y + parentGO.GetComponent<Scale>().scale.y * 10,
                    targetCoords.x);

                state = SizeBlock.Null;

                // < 6 - До миллиона
                // < 9 - миллиард
                if (counter < 12) {
                    currentGo = Instantiate<GameObject>(currentPrefab);
                    currentGo.transform.position = startCoords;
                    
                    speedOne *= 2;
                    Invoke("cameraMoveStart", 2);
                    Invoke("startOne", 4);
                }
            }
        }
    }

    
    private void setNextPrefab() {
        switch (counter) {
            case 0:
                currentPrefab = prefabOne;
                break;
            case 1:
                currentPrefab = prefabTen;
                break;
            case 2:
                currentPrefab = prefabHundred;
                break;
            case 3:
                currentPrefab = prefabThousand;
                break;
            case 4:
                currentPrefab = prefabTenThousand;
                break;
            case 5:
                currentPrefab = prefabOneHundredThousand;
                break;
            case 6:
                currentPrefab = prefabMillion;
                break;
            case 7:
                currentPrefab = prefabTenMillion;
                break;
            case 8:
                currentPrefab = prefabOneHundredMillion;
                break;
            case 9:
                currentPrefab = prefabBillion;
                break;
            case 10:
                currentPrefab = prefabTenBillion;
                break;
            case 11:
                currentPrefab = prefabOneHundredBillion;
                break;
            case 12:
                currentPrefab = prefabTrillion;
                break;
            default:
                break;
        }
    }

    private void setText() {

        textUi.text = textCounter.ToString("#,#", CultureInfo.InvariantCulture);
        textCounter += rank;
        //print("Counter: " + counter + " textCounter: " + textCounter + " rank: " + rank);
        if (textCounter == rank*10) {
            rank *= 10;
        }
    }

    private void clearBlocks() {
        GameObject[] gList = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject g in gList) {
            Destroy(g);
        }
    }

    private void startResize() {
        resize = true;
    }
    private void cameraMoveStart() {
        //startResize();
        targetPosition = cameraCoords[counter];
        currentPosition = Camera.main.transform.position;
        cameraMove = true;
    }

    private void playSound(int c) {
        switch (c) {
            case 1: audio.PlayOneShot(clipOne);
                break;
            case 2: audio.PlayOneShot(clipTen);
                break;
            case 3: audio.PlayOneShot(clipHundred);
                break;
            case 4: audio.PlayOneShot(clipThousand);
                break;
            case 5: audio.PlayOneShot(clipTenThousand);
                break;
            case 6:
                audio.PlayOneShot(clipOneHundredThousand);
                break;
            case 7:
                audio.PlayOneShot(clipMillion);
                break;
            case 8:
                audio.PlayOneShot(clipTenMillion);
                break;
            case 9:
                audio.PlayOneShot(clipOneHundredMillion);
                break;
            case 10:
                audio.PlayOneShot(clipBillion);
                break;
            case 11:
                audio.PlayOneShot(clipTenBillion);
                break;
            case 12:
                audio.PlayOneShot(clipOneHundredBillion);
                break;
            case 13:
                audio.PlayOneShot(clipTrillion);
                break;
        }
    }

    private void playBoom() {
        if (counter >= 7) {
            audioForBoom.PlayOneShot(clipBigBoom);
        } else if (counter >= 3) {
            soundMaster.playBoom();
        }
    }
}




public enum SizeBlock {
    Null,
    One,
    Ten,
    Hundred,
    Thousand
}
