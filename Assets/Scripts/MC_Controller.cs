using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class MC_Controller : MonoBehaviour
{
    public Camera camera;

    //префабы блоков
    public GameObject prefabThousand;
    public GameObject prefabMillion;
    public GameObject prefabBillion;
    public GameObject prefabTrillion;

    //в старте назначаем one, от него берем координаты и размер
    public GameObject prefabOne;
    private GameObject targetGo;
    //текущий префаб, что рисуем сейчас
    private GameObject currentPrefab;
    private List<GameObject> prefabList;

    private MC_Camera mC_Camera;

    private List<Vector3> cameraPositionList = new List<Vector3> {
        //one
        new Vector3(3.526f, -7.641f, -32.87f),
        //thousand
        new Vector3(8.387f, -3.079f, -34.196f),
        //Million
        new Vector3(22.09f, 9.06f, -41.93f),
        //Billion
        new Vector3(56.17f, 41.73f, -57.56f),
        //Trillion
        new Vector3(189.22f, 169.92f, -131.74f)
    };

    private int counterMove = 0;
    private int counter = 0;

    private bool[,,] matrix;

    //музыка в старте
    public AudioClip firstClip;
    //звук уменьшения размера
    public AudioClip audioClipDownSize;
    //звуки блоков
    public AudioClip audioClipOne;
    public AudioClip audioClipOneThousand;
    public AudioClip audioClipOneMillion;
    public AudioClip audioClipOneBillion;
    public AudioClip audioClipOneTrillion;
    //список звуков
    private List<AudioClip> prefabSoundList;

    //звук для стройки блоков
    public AudioClip musicBuildClip;
    private AudioSource musicBuild;
    private AudioSource audio;

    //скорость уменьшения размера
    private float speedChangeSizeBlock = 3f;
    private bool changeSizeBlock = false;
    private Vector3 targetSize;

    //задержка между блоками
    public float delayAfterNewBlock = 0.03f;

    //text
    public GameObject textObject;
    private Text textNumber;
    long counterText = 1;

    private void Start() {
        //музыка во время строительства        
        musicBuild = gameObject.AddComponent<AudioSource>();
        musicBuild.clip = musicBuildClip;

        //музыка старта + озвучка блоков
        audio = gameObject.AddComponent<AudioSource>();
        audio.clip = firstClip;
        audio.volume = 0.8f;
        audio.Play();

        createListPrefab();

        mC_Camera = camera.GetComponent<MC_Camera>();
        currentPrefab = prefabList[counter];
        targetGo = prefabOne;
        //текст
        textNumber = textObject.GetComponent<Text>();
        Invoke("startMove", 1f);
    }

    private void Update() {

        // 1
        if (mC_Camera.startPaintBlock) {
            mC_Camera.startPaintBlock = false;
            if (counterMove == 1) {
                playSoundBlock();
            } else {
                startBuild();
            }
        }

        // 2
        if (changeSizeBlock) {
            targetGo.transform.localScale = new Vector3(
                targetGo.transform.localScale.x - (targetGo.transform.localScale.x / 100 * 80) * Time.deltaTime,
                targetGo.transform.localScale.y - (targetGo.transform.localScale.y / 100 * 80) * Time.deltaTime,
                targetGo.transform.localScale.z - (targetGo.transform.localScale.z / 100 * 80) * Time.deltaTime);
            //print(targetGo.transform.localScale + " " + targetSize);
            //print(Mathf.Abs(targetGo.transform.localScale.x - targetSize.x));
            if ( targetGo.transform.localScale.x <= targetSize.x) {
                changeSizeBlock = false;
                startMove();
            }
        }
    }

    private void changeScale() {
        if (counter != 0) {
            audio.PlayOneShot(audioClipDownSize);
            targetSize = new Vector3(
                targetGo.transform.localScale.x / 100 * 30,
                targetGo.transform.localScale.y / 100 * 30,
                targetGo.transform.localScale.z / 100 * 30);
            changeSizeBlock = true;
        } else {
            startMove();
        }
    }

    private void startBuild() {
        musicBuild.Play();
        matrix = new bool[10, 10, 10];
        addBlock();
    }

    private void addBlock() {
        //первый элемент уже стоит
        matrix[0, 0, 0] = true;
        //проходимся по матрице
        for (int y = 0; y < 10; y++) {
            for (int z = 0; z < 10; z++) {
                for (int x = 0; x < 10; x++) {
                    //если в текущих координатах блок стоит - идем дальше
                    if (matrix[x,y,z]) { continue; }
                    //добавляем блок с коорданатами: таргет блок + текущая позиция, везде 1..
                    GameObject go = Instantiate<GameObject>(prefabList[counter]);

                    //ставим размер таргет блока
                    go.transform.localScale = targetGo.transform.localScale;

                    //go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                    go.transform.position = new Vector3(
                        targetGo.transform.position.x + targetGo.transform.localScale.x * x,
                        targetGo.transform.position.y + targetGo.transform.localScale.x * y,
                        targetGo.transform.position.z + targetGo.transform.localScale.x * z);
                    matrix[x, y, z] = true;

                    //устанавливаем текст
                    setText();

                    //если последний элемент матрицы true
                    if (matrix[9,9,9] == true) {
                        GameObject[] gList = GameObject.FindGameObjectsWithTag(getTag());
                        print("gList.size " + gList.Length + " counter: " + counter + " getTag(): " + getTag());
                        foreach (GameObject gg in gList) {
                            Destroy(gg);
                        }
                        GameObject g = Instantiate<GameObject>(prefabList[counter + 1]);
                        g.transform.position = targetGo.transform.position;
                        //устанавлиаем размер блока равен размеры тещих * 10
                        g.transform.localScale = new Vector3(
                            targetGo.transform.localScale.x * 10,
                            targetGo.transform.localScale.y * 10,
                            targetGo.transform.localScale.z * 10);
                        targetGo = g;
                        counter++;
                        
                        playSoundBlock();
                        
                    } else {
                        Invoke("addBlock", delayAfterNewBlock);
                    }
                    //выходим из цикла ждем Invoke
                    return;
                }
            }
        }
    }

    private void playSoundBlock() {

        //stop start sound
        audio.Stop();
        musicBuild.Pause();
        //play block sound
        audio.clip = prefabSoundList[counter];
        audio.Play();
        if (counter < 4) {
            Invoke("changeScale", 3f);
        }
    }

    private void startMove() {
        mC_Camera.targetPosition = cameraPositionList[counterMove];
        counterMove++;
        //mC_Camera.targetPosition = new Vector3(11.21f, -21.1f, 84.76f);
        mC_Camera.cameraMove = true;

    }

    private void setText() {
        long s = 0;
        switch (counter) {
            case 0: s = 1;
                break;
            case 1: s = 1000;
                break;
            case 2: s = 1000000;
                break;
            case 3: s = 1000000000;
                break;
            default: s = 1;
                break;
        }
        counterText += s;
        textNumber.text = counterText.ToString("#,#", CultureInfo.InvariantCulture);
    }

    private string getTag() {
        switch (counter) {
            case 0: return "Block";
            case 1: return "Thousand";
            case 2: return "Million";
            case 3: return "Billion";
            default: return "";
        }
    }

    private void createListPrefab() {

        //список префабов
        prefabList = new List<GameObject> {
            prefabOne,
            prefabThousand,
            prefabMillion,
            prefabBillion,
            prefabTrillion
        };

        //список звуков
        prefabSoundList = new List<AudioClip> {
            audioClipOne,
            audioClipOneThousand,
            audioClipOneMillion,
            audioClipOneBillion,
            audioClipOneTrillion
        };
    }
}
