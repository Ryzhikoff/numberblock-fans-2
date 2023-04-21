using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToGoogle : MonoBehaviour {
    //Список блоков
    public GameObject prefabOne;
    public GameObject prefabTen;
    public GameObject prefabOneHundred;
    public GameObject prefabOneThousand;
    public GameObject prefabTenThousand;
    public GameObject prefabOneHundredThousand;
    public GameObject prefabBigSquare;
    public GameObject prefabBigNotSquare;
    public GameObject prefabBigTen;
    public GameObject prefabBigOneHundred;

    //список активных блоков
    private List<GameObject> gameObjectList = new List<GameObject>();

    //главный счетчик
    private int counter = 1;
    //счетчик для больших блоков
    private int counterForBig = 1;
    //квадратный блок или нет
    private bool isSquare = false;
    //начинаем большие блоки - на больших true
    private bool isBigBlock = false;

    [Header("Стартовая позиция")]
    public Vector3 startPosition;
    [Header("Скорость движения блоков")]
    public float speedBlock = 2;
    [Header("Процент изменения размера")]
    public float speedResize = 0.95f;
    [Header("Задержка при движении")]
    public float delayMove = 0.3f;
    //для движения
    private bool moveBlock = false;
    private GameObject tempGO;
    //задержка между движением
    public float delay = 2;
    //объект движения земли
    private GroundMove ground;
    private List<string> listNumber;
    public GameObject textUi;
    private Text textNumber;

    public GameObject numberName;
    private Text textNumberName;
    private SoundMaster SoundMaster;

    private bool isDozens = false;

    public void Start() {
        ground = GetComponent<GroundMove>();
        textNumber = textUi.GetComponent<Text>();
        textNumberName = numberName.GetComponent<Text>();
        SoundMaster = GetComponent<SoundMaster>();
        Invoke("addBlock", 1);
        createListNumber();
    }

    /// <summary>
    /// Возвращение актуального префаба
    /// </summary>
    /// <returns>Возвращает актуальный префаб</returns>
    private GameObject getPrefab() {
        //print("counter: " + counter);
        GameObject link = null;
        if (!isBigBlock) {
            switch (counter) {
                case 1: link = prefabOne;
                    break;
                case 2: link = prefabTen;
                    break;
                case 3: link = prefabOneHundred;
                    break;
                case 4: link = prefabOneThousand;
                    break;
                case 5: link = prefabTenThousand;
                    break;
                case 6: link = prefabOneHundredThousand;
                    // с маленькими закончили - начинаем большие
                    isBigBlock = true;
                    break;
            }
        } else {
            //стали большие блоки
            switch (counterForBig) {
                case 1:
                    if (isSquare) {
                        link = prefabBigSquare;
                    } else {
                        link = prefabBigNotSquare;
                    }
                    isSquare = !isSquare;
                    counterForBig++;
                    break;
                case 2:
                    link = prefabBigTen;
                    counterForBig++;
                    break;
                case 3:
                    link = prefabBigOneHundred;
                    counterForBig = 1;
                    break;
            }
        }
        counter++;
        return link;
    }

    private void addBlock() {
        GameObject go = Instantiate<GameObject>(getPrefab());
        go.transform.position = startPosition;
        //go.transform.localScale = go.GetComponent<Scale>().scale;
        tempGO = go;
        gameObjectList.Add(go);
        //если больше трех - удаляем первый блок
        if (gameObjectList.Count > 3) {
            Destroy(gameObjectList[0]);
            gameObjectList.RemoveAt(0);
        }
        //print("Добавили блок: " + go.name + " Блоков в списке: " + gameObjectList.Count + " первый блок: " + gameObjectList[0]);
        startMoveBlock();
    }

    private void startMoveBlock() {
        ground.move = true;
        moveBlocks();
        //moveBlock = true;
    }

    private void moveBlocks() {
        foreach (GameObject go in gameObjectList) {
            //двигаем
            go.transform.position = new Vector3(
                go.transform.position.x - speedBlock,
                go.transform.position.y,
                go.transform.position.z); ;
            //уменьшаем размер
            go.transform.localScale = new Vector3(
                go.transform.localScale.x * speedResize,
                go.transform.localScale.y * speedResize,
                go.transform.localScale.z * speedResize);
        }
        if (tempGO.transform.position.x <= 0) {
            //print("tempGO.name: " + tempGO.name + " tempGo.transform.position.x: " + tempGO.transform.position.x);
            //moveBlock = false;
            playSound();
            setText();
            ground.move = false;
            //32 - один заход
            if (counter < 105) {
                Invoke("addBlock", delay);
            }
        } else {
            Invoke("moveBlocks", delayMove);
        }
    }

    private void playSound() {
        SoundMaster.playSound(counter-1);
    }

    private void setText() {
        string tempStr;
        switch (counter -1) {
            case 1:
                tempStr = "One";
                break;
            case 2:
                tempStr = "Ten";
                break;
            case 3:
                tempStr = "One Hundred";
                break;
            case 102:
                tempStr = "!!! ONE GOOGOL !!!";
                changeColor();
                break;
            default:
                if ((counter-1) % 3 == 0) {
                    //сотни
                    tempStr = "One Hundred " + getNumberName((counter - 2) / 3);
                } else if (isDozens) {
                    //десятки
                    tempStr = "Ten " + getNumberName((counter - 2) / 3);
                    isDozens = !isDozens;
                } else {
                    //единицы
                    tempStr = "One " + getNumberName((counter - 2) / 3);
                    isDozens = !isDozens;
                }
                break;
        }

        textNumberName.text = tempStr;
        print("counter : " + counter + " text.name: " + textNumberName.text);
        textNumber.text = listNumber[counter - 2];
    }


    private void changeColor() {
        textNumberName.color = new Color(Random.value, Random.value, Random.value);
        textNumber.color = new Color(Random.value, Random.value, Random.value);
        Invoke("changeColor", 0.3f);
    }
    private void createListNumber() {
        listNumber = new List<string> {
            "1",
            "10",
            "100",
            "1,000",
            "10,000",
            "100,000",
            "1,000,000",
            "10,000,000",
            "100,000,000",
            "1,000,000,000",
            "10,000,000,000",
            "100,000,000,000",
            "1,000,000,000,000",
            "10,000,000,000,000",
            "100,000,000,000,000",
            "1,000,000,000,000,000",
            "10,000,000,000,000,000",
            "100,000,000,000,000,000",
            "1,000,000,000,000,000,000",
            "10,000,000,000,000,000,000",
            "100,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
            "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000",
        };
    }

    private string getNumberName(int i) {
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
            /*case 34:
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
                return "Centillion";*/
            default:
                return null;
        }

    }
}