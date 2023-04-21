using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FromOne : MonoBehaviour
{
    public GameObject prefabOne;
    public GameObject prefabThousand;
    public GameObject prefabMillion;
    public GameObject prefabSquare1;
    public GameObject prefabSquare2;
    public GameObject prefabSquare3;
    public GameObject prefabSquare4;
    public GameObject prefabSquare5;
    public GameObject prefabSquare6;
    public GameObject prefabSquare7;
    public GameObject prefabSquare8;
    public GameObject prefabNotSquare1;
    public GameObject prefabNotSquare2;
    public GameObject prefabNotSquare3;
    public GameObject prefabNotSquare4;
    public GameObject prefabNotSquare5;
    public GameObject prefabNotSquare6;
    public GameObject prefabNotSquare7;
    public GameObject prefabNotSquare8;
    public GameObject prefabNotSquare9;

    public Vector3 spawnPosition = new Vector3(0,45,0);
    public float speed = 5;
    public float speedReSize = 0.8f;

    private GameObject tempObject;
    private GameObject lastObject;
    private bool move = false;
    private bool reSize = false;
    private AudioSource audio;
    private int counter = 1;
    public AudioClip clipBoom;

    Text textExponent;
    Text textExponent2;
    Text textNumber;
    Text textName;
    int exponentNumber = 3;

    public AudioClip clip1;
    public AudioClip clip2;
    public AudioClip clip3;
    public AudioClip clip4;
    public AudioClip clip5;
    private int clipCounter = 1;
    private AudioSource audioMusic;

    private void Awake() {
        audioMusic = gameObject.AddComponent<AudioSource>();
        audioMusic.volume = 0.2f;
        audioMusic.clip = clip1;
        audioMusic.Play();
    }

    void Start()
    {
        lastObject = Instantiate<GameObject>(prefabOne);
        lastObject.transform.position = Vector3.zero;

        audio = gameObject.AddComponent<AudioSource>();

        textExponent = GameObject.Find("Exponent").GetComponent<Text>();
        textExponent2 = GameObject.Find("Exponent_").GetComponent<Text>();
        textName = GameObject.Find("Name").GetComponent<Text>();
        textNumber = GameObject.Find("Number").GetComponent<Text>();

        //novemdecilion
        textNumber.text = " ";

        /*textNumber.text = "1 000 000 000 000 000 000 000 000 000 000 " +
                            "000 000 000 000 000 000 000 000 000 000 " +
                            "000 000 000 000 000 000 000 000 000 000 " +
                            "000 000 000 000 000 000 000 000 000 000 " +
                            "000 000 000 000 000 000 000 000 000 000 " +
                            "000 000 000 000 000 000 000 000 000 000 " +
                            "000 000 000 000 000 000 000 000 000 000 " +
                            "000 000 000 000 000 000 000 000 000 000 " +
                            "000 000 000 000 000 000 000 000 000 000 ";*/

        updateUi();
        Invoke("playSound", 0.5f);

        Invoke("addBlock", 0.7f);

    }

    private GameObject getPrefab (bool isSquare) {
        int r = Random.Range(1, 10);
        if (isSquare) {
            if (r == 9) {
                r -= 1;
            }
            switch (r) {
                case 1:
                    return prefabSquare1;
                case 2:
                    return prefabSquare2;
                case 3:
                    return prefabSquare3;
                case 4:
                    return prefabSquare4;
                case 5:
                    return prefabSquare5;
                case 6:
                    return prefabSquare6;
                case 7:
                    return prefabSquare7;
                case 8:
                    return prefabSquare8;
                default: return prefabSquare1;
            }
        } else {
                switch (r) {
                    case 1: return prefabNotSquare1;
                    case 2: return prefabNotSquare2;
                    case 3: return prefabNotSquare3;
                    case 4: return prefabNotSquare4;
                    case 5: return prefabNotSquare5;
                    case 6: return prefabNotSquare6;
                    case 7: return prefabNotSquare7;
                    case 8: return prefabNotSquare8;
                    case 9: return prefabNotSquare9;
                default: return prefabNotSquare8;
            }
        }
    }

    private void addBlock() {

        if (counter < 102) {
            counter++;
            if (counter == 2) {
                tempObject = Instantiate<GameObject>(prefabThousand);
            } else if (counter == 3) {
                tempObject = Instantiate<GameObject>(prefabMillion);
            } else if (isEven(counter)) {
                tempObject = Instantiate<GameObject>(getPrefab(false));
            } else {
                tempObject = Instantiate<GameObject>(getPrefab(true));
            }

            tempObject.transform.position = spawnPosition;
            if (counter == 2) {
                tempObject.transform.localScale = new Vector3(10, 10, 10);
            } else {
                tempObject.transform.localScale = new Vector3(100, 100, 100);
            }

            Invoke("startMoveBlock", 2);
        }
    }

    private void playSound() {
        AudioClip clip = (AudioClip)Resources.Load(counter.ToString());
        audio.volume = 1.0f;
        audio.PlayOneShot(clip);
    }

    private void checkSound() {
        if (!audioMusic.isPlaying) {
            clipCounter++;
            if(clipCounter > 5) {
                clipCounter = 1;
            }
            switch (clipCounter) {
                case 1: audioMusic.clip = clip1;
                    break;
                case 2:
                    audioMusic.clip = clip2;
                    break;
                case 3:
                    audioMusic.clip = clip3;
                    break;
                case 4:
                    audioMusic.clip = clip4;
                    break;
                case 5:
                    audioMusic.clip = clip5;
                    break;
            }
            audioMusic.Play();
        }
    }

    private void Update() {
        checkSound();
    }
    void FixedUpdate()
    {
        if (move) {
            tempObject.transform.position = new Vector3(
                    tempObject.transform.position.x,
                    tempObject.transform.position.y - speed * Time.deltaTime,
                    tempObject.transform.position.z);
            if (tempObject.transform.position.y <= 0.2) {
                tempObject.transform.position = Vector3.zero;
                move = false;
                Destroy(lastObject);
                audio.volume = 0.2f;
                audio.PlayOneShot(clipBoom);
                Invoke("startReSize", 1.1f);
            }
        }
        if (reSize) {
            tempObject.transform.localScale = new Vector3(
                    tempObject.transform.localScale.x - (tempObject.transform.localScale.x / 100 * 90) * Time.deltaTime*2,
                    tempObject.transform.localScale.y - (tempObject.transform.localScale.y / 100 * 90) * Time.deltaTime*2,
                    tempObject.transform.localScale.z - (tempObject.transform.localScale.z / 100 * 90) * Time.deltaTime*2);
            if (tempObject.transform.localScale.x <= 1) {
                reSize = false;
                updateUi();
                playSound();
                lastObject = tempObject;
                addBlock();
            }
        }
        
    }

    private void updateUi() {
        if (counter == 1) {
            textName.text = "One";
            textNumber.text = "1";
        } else {
            textExponent2.text = "10";
            textExponent.text = exponentNumber.ToString();
            textNumber.text = textNumber.text + " 000";
            textName.text = getName(counter - 1);

            exponentNumber += 3;
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
        move = true;
    }

    public void startReSize() {
        reSize = true;
    }

    private bool isEven(int a) {
        return (a % 2) == 0;
    }
}
