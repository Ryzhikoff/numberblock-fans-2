using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Blocks : MonoBehaviour
{
    //список всем блоков
    private List<GameObject> listBLocks;
    public float speedPosition = 20f;
    public float speedResize = 0.4f;

    public GameObject prefabOne;
    public GameObject prefabThousand;
    public GameObject prefabMillion;
    public GameObject prefabSquare;
    public GameObject prefabNotSquare;
    public GameObject prefabSquare1;
    public GameObject prefabSquare2;
    public GameObject prefabSquare3;
    public GameObject prefabSquare4;
    public GameObject prefabSquare5;
    public GameObject prefabSquare6;
    public GameObject prefabNotSquare1;
    public GameObject prefabNotSquare2;
    public GameObject prefabNotSquare3;
    public GameObject prefabNotSquare4;
    public GameObject prefabNotSquare5;
    public GameObject prefabNotSquare6;

    public Vector3 spawnPosition = Vector3.zero;
    public Vector3 spawnScale = new Vector3(100, 100, 100);
    public float stopPositionX = -150f;
    public float delaySpawnBlock = 2f;

    private bool blockSquare = true;

    public int counter = 5;
    AudioSource audio;
    AudioSource boomAudio;
    public AudioClip boomSound;
    public float boomVolume = 1.0f;

    Text textExponent;
    Text textExponent2;
    Text textNumber;
    Text textName;

    int exponentNUmber = 150;


    public bool moveBlock {
        get;
        set;
    }
  

    private void Start() {
        listBLocks = new List<GameObject>();
        audio = gameObject.AddComponent<AudioSource>();
        boomAudio = gameObject.AddComponent<AudioSource>();
        boomAudio.volume = boomVolume;

        textExponent = GameObject.Find("Exponent").GetComponent<Text>();
        textExponent2 = GameObject.Find("Exponent_").GetComponent<Text>();
        textName = GameObject.Find("Name").GetComponent<Text>();
        textNumber = GameObject.Find("Number").GetComponent<Text>();

        //textName.text = "One";
        //textNumber.text = "1";

        Invoke(nameof(spawnBLock), 0.5f);
        //Invoke("changeColor", 0.2f);
    }

    private void changeColor() {
        textExponent.color = new Color(Random.value, Random.value, Random.value);
        textExponent2.color = new Color(Random.value, Random.value, Random.value);
        textNumber.color = new Color(Random.value, Random.value, Random.value);
        textName.color = new Color(Random.value, Random.value, Random.value);

        Invoke("changeColor", 0.2f);
    }

    public void move () {
        if (listBLocks.Count > 0) {
            foreach (GameObject go in listBLocks) {
                if (go.transform.position.x > 101) {
                    speedPosition = 50f;
                } else {
                    speedPosition = 25f;
                }
                go.transform.position = new Vector3(
                    go.transform.position.x - speedPosition * Time.deltaTime,
                    go.transform.position.y,
                    go.transform.position.z);
            }

            if (listBLocks[listBLocks.Count - 1].transform.position.x < stopPositionX) {
                moveBlock = false;
                spawnBLock();
                
            }
        }
    }

    public void reSize() {
        if (listBLocks.Count > 0) {
            
            foreach (GameObject go in listBLocks) {
                if (go.transform.localScale.x > 100) {
                    speedResize = 225f;
                } else {
                    speedResize = 24f;
                }
                go.transform.localScale = new Vector3(
                    go.transform.localScale.x - speedResize * Time.deltaTime,
                    go.transform.localScale.y - speedResize * Time.deltaTime,
                    go.transform.localScale.z - speedResize * Time.deltaTime);
            }
            if (listBLocks[0].transform.localScale.x < 0.2) {
                GameObject g = listBLocks[0];
                listBLocks.Remove(g);
                Destroy(g);
            }
        }
    }

    public void spawnBLock() {
        if (listBLocks.Count == 0) {
            //Если сначала то ставим - ОДИН
            GameObject go = Instantiate<GameObject>(prefabOne);
            go.transform.position = new Vector3(101, 0, 0);
            go.transform.localScale = new Vector3(100, 100, 100);
            listBLocks.Add(go);
            
            //если сначала - то ставим ТЫСЯЧУ
            GameObject go1 = Instantiate<GameObject>(prefabThousand);
            go1.transform.position = spawnPosition;
            go1.transform.localScale = spawnScale;
            listBLocks.Add(go1);
        } else {
            GameObject go;
            if (blockSquare) {
                go = Instantiate<GameObject>(getPrefab(true));
            } else {
                go = Instantiate<GameObject>(getPrefab(false));
            }
            boomAudio.PlayOneShot(boomSound);
            blockSquare = !blockSquare;
            go.transform.position = spawnPosition;
            go.transform.localScale = spawnScale;
            listBLocks.Add(go);
        }
        counter++;
        AudioClip clip = (AudioClip)Resources.Load("sound/" + counter.ToString());
        audio.volume = 1.0f;
        print(clip);
        audio.PlayOneShot(clip);
        updateUi();
        
        // 50
        if (counter < 76) {
            Invoke("startMoveBlock", delaySpawnBlock);
        }

    }

    private GameObject getPrefab(bool isSquare) {
        int r = Random.Range(1, 7);
        if (isSquare) {
            return r switch {
                1 => prefabSquare1,
                2 => prefabSquare2,
                3 => prefabSquare3,
                4 => prefabSquare4,
                5 => prefabSquare5,
                6 => prefabSquare6,
                _ => prefabSquare1,
            };
            ;
        } else {
            return r switch {
                1 => prefabNotSquare1,
                2 => prefabNotSquare2,
                3 => prefabNotSquare3,
                4 => prefabNotSquare4,
                5 => prefabNotSquare5,
                6 => prefabNotSquare6,
                _ => prefabNotSquare6,
            };
            ;
        }
    }

    private void updateUi() {
        if (counter == 1) {
            textName.text = "One";
            textNumber.text = "1";
        } else {
            textExponent2.text = "10";
            textExponent.text = exponentNUmber.ToString();
            textNumber.text += " 000";
            textName.text = getName(counter - 1);

            exponentNUmber += 3;
        }
    }

    private string getName(int i) {
        return i switch {
            1 => "Thousand",
            2 => "Million",
            3 => "Billion",
            4 => "Trillion",
            5 => "Quadrillion",
            6 => "Quintillion",
            7 => "Sextillion",
            8 => "Septillion",
            9 => "Octillion",
            10 => "Nonillion",
            11 => "Decillion",
            12 => "Undecillion",
            13 => "Duodecillion",
            14 => "Tredecillion",
            15 => "Quattuordecillion",
            16 => "Quindecillion",
            17 => "Sexdecillion",
            18 => "Septendecillion",
            19 => "Octodecillion",
            20 => "Novemdecillion",
            21 => "Vigintillion",
            22 => "Unvigintillion",
            23 => "Duovigintillion",
            24 => "Trevigintillion",
            25 => "Quattuorvigintillion",
            26 => "Quinvigintillion",
            27 => "Sexvigintillion",
            28 => "Septenvigintillion",
            29 => "Octovigintillion",
            30 => "Novemvigintillion",
            31 => "Trigintillion",
            32 => "Untrigintillion",
            33 => "Duotrigintillion",
            34 => "Trestrigintillion",
            35 => "Quattuortrigintillion",
            36 => "Quintrigintillion",
            37 => "Sextrigintillion",
            38 => "Septentrigintillion",
            39 => "Octotrigintillion",
            40 => "Novemtrigintillion",
            41 => "Quadragintillion",
            42 => "Unquadragintillion",
            43 => "Duoquadragintillion",
            44 => "Trequadragintillion",
            45 => "Quattuorquadragintillion",
            46 => "Quinquadragintillion",
            47 => "Sexquadragintillion",
            48 => "Septenquadragintillion",
            49 => "Octoquadragintillion",
            50 => "Novemquadragintillion",
            51 => "Quinquagintillion",
            52 => "Unquinquagintillion",
            53 => "Duoquinquagintillion",
            54 => "Trequinquagintillion",
            55 => "Quattuorquinquagintillion",
            56 => "Quinquinquagintillion",
            57 => "Sexquinquagintillion",
            58 => "Septenquinquagintillion",
            59 => "Octoquinquagintillion",
            60 => "Novemquinquagintillion",
            61 => "Sexagintillion",
            62 => "Unsexagintillion",
            63 => "Duosexagintillion",
            64 => "Tresexagintillion",
            65 => "Quattuorsexagintillion",
            66 => "Quinsexagintillion",
            67 => "Sexsexagintillion",
            68 => "Septensexagintillion",
            69 => "Octosexagintillion",
            70 => "Novemsexagintillion",
            71 => "Septuagintillion",
            72 => "Unseptuagintillion",
            73 => "Duoseptuagintillion",
            74 => "Treseptuagintillion",
            75 => "Quattuorseptuagintillion",
            76 => "Quinseptuagintillion",
            77 => "Sexseptuagintillion",
            78 => "Septenseptuagintillion",
            79 => "Octoseptuagintillion",
            80 => "Novemseptuagintillion",
            81 => "Octogintillion",
            82 => "Unoctogintillion",
            83 => "Duooctogintillion",
            84 => "Treoctogintillion",
            85 => "Quattuoroctogintillion",
            86 => "Quinoctogintillion",
            87 => "Sexoctogintillion",
            88 => "Septoctogintillion",
            89 => "Octooctogintillion",
            90 => "Novemoctogintillion",
            91 => "Nonagintillion",
            92 => "Unnonagintillion",
            93 => "Duononagintillion",
            94 => "Trenonagintillion",
            95 => "Quattuornonagintillion",
            96 => "Quinnonagintillion",
            97 => "Sexnonagintillion",
            98 => "Septennonagintillion",
            99 => "Octononagintillion",
            100 => "Novemnonagintillion",
            101 => "Centillion",
            _ => "",
        };
    }

    public void startMoveBlock() {
        moveBlock = true;
    }
}
