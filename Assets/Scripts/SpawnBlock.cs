using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnBlock : MonoBehaviour
{
    public float timeRespawn = 0.2f;
    public GameObject cubeOnePrefab;
    public GameObject cubeOneThousandPrefab;
    public AudioClip clip;
    public AudioClip oneThousandClip;
    public AudioClip voiceOneHungred;
    public AudioClip voiceOneThousand;
    public AudioClip voiceOneMillion;
    public AudioClip laugh;
    public float pitchLevel = 0;
    //public GameObject camera;

    public float speedOneThousand = 0.5f;

    private float xOneThousandStart = 4.5f;
    private float yOneThousandStart = 25;
    private bool isPaintOneThousand = false;
    private bool[] listOneThousand = new bool[9];
    private List<GameObject> oneThousandGoList;
    
    private AudioSource audio;
    private AudioSource audioVoice;
    private AudioSource audioLaugh;

    private int score = 0;

    private Text text;
    GameObject camera;

    //real Thousand
    private bool isPaintThousand = false;
    public GameObject thusandPrefab;
    private bool[,,] thousandListBool = new bool[10,10,10];
    private List<GameObject> thousandListGo;

    private Vector3 tempXStart;
    private Vector3 tempFinishCoord;
    private GameObject tempCube;
    public float speedTHousand = 2f;

    private bool[,] listBlock = new bool[10,10];

    //million
    public GameObject millionPrefab;


    void Start()
    {
        camera = GameObject.FindGameObjectWithTag("MainCamera");
        audio = gameObject.AddComponent<AudioSource>();
        audio.volume = 0.3f;
        audioVoice = gameObject.AddComponent<AudioSource>();
        audioVoice.volume = 1f;
        audioLaugh = gameObject.AddComponent<AudioSource>();
        oneThousandGoList = new List<GameObject>();
        GameObject goText = GameObject.Find("Score");
        text = goText.GetComponent<Text>();
        
        Invoke("paintControl", timeRespawn);
        Invoke("changeTextColor", 0.4f);

        thousandListGo = new List<GameObject>();
    }

    private void changeTextColor() {
        text.color = new Color(Random.value, Random.value, Random.value);
        Invoke("changeTextColor", 0.4f);
    }
    private void paintControl() {
        
        if (!listBlock[9, 9]) {
            paintOneBlock();
            Invoke("paintControl", timeRespawn);
        } else {
            Invoke("playVoiceOneHungred", 0.7f);
            Invoke("paintOneThousand", 3f);
        }
    }

    private void FixedUpdate() {
        if(isPaintOneThousand && !listOneThousand[8]) {
            for (int i = 0; i < 9; i++) {
                if (listOneThousand[i]) continue;
                if (oneThousandGoList.Count == i) {
                    
                    GameObject go = Instantiate(cubeOneThousandPrefab);
                    go.transform.position = new Vector3(xOneThousandStart, yOneThousandStart, i + 1);
                    oneThousandGoList.Add(go);

                } else {
                    oneThousandGoList[i].transform.position = new Vector3(
                        oneThousandGoList[i].transform.position.x,
                        oneThousandGoList[i].transform.position.y - speedOneThousand * Time.deltaTime,
                        oneThousandGoList[i].transform.position.z);
                    
                    if (oneThousandGoList[i].transform.position.y < 5.0f) {
                        listOneThousand[i] = true;
                        audioLaugh.PlayOneShot(laugh);
                        score += 100;
                        if (listOneThousand[8]) {
                            audioVoice.PlayOneShot(voiceOneThousand);

                            GameObject go = Instantiate(thusandPrefab);
                            go.transform.position = new Vector3(0, 0, 0);
                            tempFinishCoord = go.transform.position;

                            thousandListGo.Add(go);
                            thousandListBool[0, 0, 0] = true;
                            
                            isPaintOneThousand = false;


                            GameObject[] oneList = GameObject.FindGameObjectsWithTag("OneHungred");
                            foreach (GameObject goOne in oneList) {
                                Destroy(goOne);
                            }

                            Invoke("paintThousand", 3f);
                        }
                    }
                }
                return;
            }
        }
    }

    private void newThousandBlock() {
        score += 1000;
        GameObject go = Instantiate(thusandPrefab);
        Vector3 pos = getCoord();
        go.transform.position = pos;
        tempFinishCoord = pos;
        thousandListGo.Add(go);
        if (thousandListGo.Count < 1000) {
            Invoke("newThousandBlock", 0.05f);
        } else {
            foreach (GameObject g in thousandListGo) {
                Destroy(g);
            }
            GameObject goM = Instantiate(millionPrefab);
            goM.transform.position = Vector3.zero;
            audioVoice.PlayOneShot(voiceOneMillion);
        }
    }

    private void paintThousand() {
        isPaintThousand = true;
        camera.GetComponent<CameraMoveOne>().finishPos = new Vector3(121.2f, 114.1f, -65.1f);
        camera.transform.Rotate(30,0, 0, Space.Self);  
        camera.GetComponent<CameraMoveOne>().cameraMove = true;
        Invoke("newThousandBlock", 0.2f);
    }

    private Vector3 getCoord() {
        for (int y = 0; y < 10; y++) {
            for (int z = 0; z < 10; z++) {
                for (int x = 0; x < 10; x++) {
                    if (thousandListBool[y, z, x]) continue;
                    thousandListBool[y, z, x] = true;
                    return new Vector3(x * 10, y * 10, z * 10);
                }
            }
        }
        return Vector3.zero;
    }

    private void Update() {
        if (score != 0) {
            text.text = score.ToString();
        }
    }

    private void playVoiceOneHungred() {
        audioVoice.PlayOneShot(voiceOneHungred);
    }

    private void paintOneThousand() {
        isPaintOneThousand = true;
        camera.GetComponent<CameraMoveOne>().finishPos = new Vector3(16f, 8.5f, -14.5f);
        camera.GetComponent<CameraMoveOne>().cameraMove = true;
    }

    private void paintOneBlock() {
        for (int y = 0; y < 10; y++) {
            for (int x = 0; x < 10; x++) {
                if (listBlock[x, y]) continue;
                listBlock[x, y] = true;
                //если последний блок == рисуем сотню
                if (y == 9 && x == 9) {
                    GameObject go = Instantiate(cubeOneThousandPrefab);
                    go.transform.position = new Vector3(4.5f, 5.0f, 0);

                    score = 100;
                    //удаляем все ваны
                    GameObject[] oneList = GameObject.FindGameObjectsWithTag("BlockOne");
                    foreach (GameObject goOne in oneList) {
                        Destroy(goOne);
                    }

                } else {
                    GameObject go = Instantiate(cubeOnePrefab);
                    go.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
                    score++;
                }
                playSound();
                return;
            }
        }
    }

    private void playSound() {
        if (!listBlock[9, 9]) {
            
            pitchLevel += 0.01f;
            audio.pitch = pitchLevel;
            audio.PlayOneShot(clip);
        } else {
            audioVoice.PlayOneShot(oneThousandClip);
        }
    }
}
