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

    Text textExponent;
    Text textExponent2;
    Text textNumber;
    Text textName;

    int exponentNUmber = 3;


    public bool moveBlock {
        get;
        set;
    }
  

    private void Start() {
        listBLocks = new List<GameObject>();
        audio = gameObject.AddComponent<AudioSource>();

        textExponent = GameObject.Find("Exponent").GetComponent<Text>();
        textExponent2 = GameObject.Find("Exponent_").GetComponent<Text>();
        textName = GameObject.Find("Name").GetComponent<Text>();
        textNumber = GameObject.Find("Number").GetComponent<Text>();


        Invoke("spawnBLock", 0.5f);
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
            int k = 0;
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
            GameObject go = Instantiate<GameObject>(getPrefab(true));
            go.transform.position = new Vector3(101, 0, 0);
            go.transform.localScale = new Vector3(100, 100, 100);
            listBLocks.Add(go);
            
            //если сначала - то ставим ТЫСЯЧУ
            GameObject go1 = Instantiate<GameObject>(getPrefab(false));
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
            blockSquare = !blockSquare;
            go.transform.position = spawnPosition;
            go.transform.localScale = spawnScale;
            listBLocks.Add(go);
        }
        AudioClip clip = (AudioClip)Resources.Load("sound/" + counter.ToString());
        audio.volume = 1.0f;
        print(clip);
        audio.PlayOneShot(clip);
        updateUi();
        counter++;
        //22
        if (counter < 103) {
            Invoke("startMoveBlock", delaySpawnBlock);
        }

    }

    private GameObject getPrefab(bool isSquare) {
        int r = Random.Range(1, 7);
        if (isSquare) {
            switch (r) {
                case 1: return prefabSquare1;
                case 2: return prefabSquare2;
                case 3: return prefabSquare3;
                case 4: return prefabSquare4;
                case 5: return prefabSquare5;
                case 6: return prefabSquare6;
                default: return prefabSquare1;
            };
        } else {
            switch (r) {
                case 1: return prefabNotSquare1;
                case 2: return prefabNotSquare2;
                case 3: return prefabNotSquare3;
                case 4: return prefabNotSquare4;
                case 5: return prefabNotSquare5;
                case 6: return prefabNotSquare6;
                default: return prefabNotSquare6;
            };
        }
    }

    private void updateUi() {
        if (counter == 1) {
            textName.text = "One";
            textNumber.text = "1";
        } else {
            textExponent2.text = "10";
            textExponent.text = exponentNUmber.ToString();
            textNumber.text = textNumber.text + " 000";
            textName.text = getName(counter - 1);

            exponentNUmber += 3;
        }
    }

    private string getName(int i) {
        switch (i) {
            case 1:
                return "Thousand";
            case 2:
                return "Million";
            case 3:
                return "Billion";
            case 4:
                return "Trillion";
            case 5:
                return "Quadrillion";
            case 6:
                return "Quintillion";
            case 7:
                return "Sextillion";
            case 8:
                return "Septillion";
            case 9:
                return "Octillion";
            case 10:
                return "Nonillion";
            case 11:
                return "Decillion";
            case 12:
                return "Undecillion";
            case 13:
                return "Duodecillion";
            case 14:
                return "Tredecillion";
            case 15:
                return "Quattuordecillion";
            case 16:
                return "Quindecillion";
            case 17:
                return "Sexdecillion";
            case 18:
                return "Septendecillion";
            case 19:
                return "Octodecillion";
            case 20:
                return "Novemdecillion";
            case 21:
                return "Vigintillion";
            case 22:
                return "Unvigintillion";
            case 23:
                return "Duovigintillion";
            case 24:
                return "Trevigintillion";
            case 25:
                return "Quattuorvigintillion";
            case 26:
                return "Quinvigintillion";
            case 27:
                return "Sexvigintillion";
            case 28:
                return "Septenvigintillion";
            case 29:
                return "Octovigintillion";
            case 30:
                return "Novemvigintillion";
            case 31:
                return "Trigintillion";
            case 32:
                return "Untrigintillion";
            case 33:
                return "Duotrigintillion";
            case 34:
                return "Trestrigintillion";
            case 35:
                return "Quattuortrigintillion";
            case 36:
                return "Quintrigintillion";
            case 37:
                return "Sextrigintillion";
            case 38:
                return "Septentrigintillion";
            case 39:
                return "Octotrigintillion";
            case 40:
                return "Novemtrigintillion";
            case 41:
                return "Quadragintillion";
            case 42:
                return "Unquadragintillion";
            case 43:
                return "Duoquadragintillion";
            case 44:
                return "Trequadragintillion";
            case 45:
                return "Quattuorquadragintillion";
            case 46:
                return "Quinquadragintillion";
            case 47:
                return "Sexquadragintillion";
            case 48:
                return "Septenquadragintillion";
            case 49:
                return "Octoquadragintillion";
            case 50:
                return "Novemquadragintillion";
            case 51:
                return "Quinquagintillion";
            case 52:
                return "Unquinquagintillion";
            case 53:
                return "Duoquinquagintillion";
            case 54:
                return "Trequinquagintillion";
            case 55:
                return "Quattuorquinquagintillion";
            case 56:
                return "Quinquinquagintillion";
            case 57:
                return "Sexquinquagintillion";
            case 58:
                return "Septenquinquagintillion";
            case 59:
                return "Octoquinquagintillion";
            case 60:
                return "Novemquinquagintillion";
            case 61:
                return "Sexagintillion";
            case 62:
                return "Unsexagintillion";
            case 63:
                return "Duosexagintillion";
            case 64:
                return "Tresexagintillion";
            case 65:
                return "Quattuorsexagintillion";
            case 66:
                return "Quinsexagintillion";
            case 67:
                return "Sexsexagintillion";
            case 68:
                return "Septensexagintillion";
            case 69:
                return "Octosexagintillion";
            case 70:
                return "Novemsexagintillion";
            case 71:
                return "Septuagintillion";
            case 72:
                return "Unseptuagintillion";
            case 73:
                return "Duoseptuagintillion";
            case 74:
                return "Treseptuagintillion";
            case 75:
                return "Quattuorseptuagintillion";
            case 76:
                return "Quinseptuagintillion";
            case 77:
                return "Sexseptuagintillion";
            case 78:
                return "Septenseptuagintillion";
            case 79:
                return "Octoseptuagintillion";
            case 80:
                return "Novemseptuagintillion";
            case 81:
                return "Octogintillion";
            case 82:
                return "Unoctogintillion";
            case 83:
                return "Duooctogintillion";
            case 84:
                return "Treoctogintillion";
            case 85:
                return "Quattuoroctogintillion";
            case 86:
                return "Quinoctogintillion";
            case 87:
                return "Sexoctogintillion";
            case 88:
                return "Septoctogintillion";
            case 89:
                return "Octooctogintillion";
            case 90:
                return "Novemoctogintillion";
            case 91:
                return "Nonagintillion";
            case 92:
                return "Unnonagintillion";
            case 93:
                return "Duononagintillion";
            case 94:
                return "Trenonagintillion";
            case 95:
                return "Quattuornonagintillion";
            case 96:
                return "Quinnonagintillion";
            case 97:
                return "Sexnonagintillion";
            case 98:
                return "Septennonagintillion";
            case 99:
                return "Octononagintillion";
            case 100:
                return "Novemnonagintillion";
            case 101:
                return "Centillion";
            default:
                return "";
        }

    }

    public void startMoveBlock() {
        moveBlock = true;
    }
}
