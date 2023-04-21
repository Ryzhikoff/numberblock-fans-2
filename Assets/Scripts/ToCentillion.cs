using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * 1. Для первого старта private bool _isBigBlock = false
 * 2. Изменить координаты старта блоков и координаты камеры
 */

public class ToCentillion : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 startPositionAfterCameraMove;
    public float stopY = 0;
    public float speedDown = 2;
    public float speedDownAfterCameraMove = 40;
    public float delayAfterDownToReSize = 0.5f;
    public float delayAfterNewBlockToDown = 1f;
    public float percentReSize = 9f;

    private int counter = 0;
    //блок который падает
    private GameObject currentBlock;
    //блок который остался
    private GameObject oldBlock;

    private BlocksPrefab blocksPrefab;
    private SoundMaster soundMaster;

    private bool isNeedDown = false;
    private bool isNeedMove = false;
    private bool isNeedReSize = false;
    private Vector3 startScale;

    public Vector3 startCameraPosition;
    public Vector3 cameraPoint1;
    private bool isCameraMove = false;
    public int interpolationFramesCount = 300;
    int elapsedFrames = 0;

    Text textExponent;
    Text textExponent2;
    Text textNumber;
    Text textName;

    private int counterNumberDigit = 1;

    private void Start() {
        soundMaster = gameObject.GetComponent<SoundMaster>();
        blocksPrefab = gameObject.GetComponent<BlocksPrefab>();
        textExponent = GameObject.Find("Exponent").GetComponent<Text>();
        textExponent2 = GameObject.Find("Exponent_").GetComponent<Text>();
        textName = GameObject.Find("Name").GetComponent<Text>();
        textNumber = GameObject.Find("Number").GetComponent<Text>();
        Camera.main.transform.position = startCameraPosition;

        Invoke("addNewBlock", 1);
        //soundMaster.playSound(200);
    }

    private void addNewBlock() {
        counter++;
        //print(blocksPrefab.getPrefab(counter));
        GameObject go = Instantiate<GameObject>(blocksPrefab.getPrefab(counter));
        go.transform.position = startPosition;
        currentBlock = go;
        Invoke("startDown", delayAfterNewBlockToDown);
    }

    private void startDown() {
        isNeedDown = true;
        soundMaster.playDownSound(true);
    }

    private void Update() {
        if (isNeedDown)
            downBlock();
        if (isCameraMove)
            cameraMove();
        if (isNeedReSize)
            reSizeBlock();
    }

    private void downBlock() {
        currentBlock.transform.position = new Vector3(
            currentBlock.transform.position.x,
            currentBlock.transform.position.y - speedDown * Time.deltaTime,
            currentBlock.transform.position.z);
        
        if (currentBlock.transform.position.y <= stopY) {
            isNeedDown = false;
            currentBlock.transform.position = new Vector3(
                0,
                stopY,
                0);
            soundMaster.playBoom();
            
            if (oldBlock != null)
                Destroy(oldBlock);
            oldBlock = currentBlock;
            updateUi();
            soundMaster.playDownSound(false);
            soundMaster.playBoom();
            soundMaster.playSound(counter);
            Invoke("startReSizeBlock", delayAfterDownToReSize);
        }
    }

    private void moveBlock() {

    }

    private void startReSizeBlock() {
        isNeedReSize = true;
        startScale = oldBlock.GetComponent<Scale>().scale;

    }
    private void reSizeBlock() {

        //Nonnilian - 30
        // 91
        if (counter == 303)
            return;

        //если не первый блок и разоядность делится на 3 - значит меням размер
        if (divisibleBy3() && counter > 4) {
            oldBlock.transform.localScale = new Vector3(
                oldBlock.transform.localScale.x - (oldBlock.transform.localScale.x / 100 * percentReSize) * Time.deltaTime,
                oldBlock.transform.localScale.y - (oldBlock.transform.localScale.y / 100 * percentReSize) * Time.deltaTime,
                oldBlock.transform.localScale.z - (oldBlock.transform.localScale.z / 100 * percentReSize) * Time.deltaTime);
            
            startScale = new Vector3(
                startScale.x - (startScale.x / 100 * percentReSize) * Time.deltaTime,
                startScale.y - (startScale.y / 100 * percentReSize) * Time.deltaTime,
                startScale.z - (startScale.z / 100 * percentReSize) * Time.deltaTime);

            if (startScale.x <= 1) {
                isNeedReSize = false;
                addNewBlock();
            }
        } else {
            
            isNeedReSize = false;

            if (counter == 4) {
                isCameraMove = true;
                startPosition = startPositionAfterCameraMove;
                speedDown = speedDownAfterCameraMove;
            } else {
                addNewBlock();
            }
        }
        
    }

    private bool divisibleBy3() {
        if ((counter-1) % 3 == 0) {
            return true;
        } else {
            return false;
        }
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(Camera.main.transform.position, cameraPoint1, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        Camera.main.transform.position = interpolatedPosition;
        if (MathF.Abs(Camera.main.transform.position.y - cameraPoint1.y) < 0.05) {
            isCameraMove = false;
            elapsedFrames = 0;
            addNewBlock();
        }
    }

    private void updateUi() {
        if (counter == 1) {
            textName.text = getNameNumberDigit();
            textNumber.text = "1";
        } else {
            textNumber.text = parseBySpace();
            textName.text = getNameNumberDigit() + getName((counter - 1) / 3);

            textExponent2.text = "10";
            textExponent.text = (counter-1).ToString();

            //print(parseBySpace() + " " + getName((counter-1) / 3) + " magicNumber " + (counter - 1) / 3);
        }
    }

    private string parseBySpace() {
        string numberString = textNumber.text + "0";
        numberString = numberString.Replace(" ", "");
        char[] list = numberString.ToCharArray();
        string tempStr = "";
        int c = 0;
        for (int i = list.Length - 1; i > -1 ; i--) {
            if (c == 3) {
                c = 0;
                tempStr = " " + tempStr;
            }

            tempStr = list[i] + tempStr;
            c++;
        }
        return tempStr;
    }

    private string getNameNumberDigit() {
        switch (counterNumberDigit) {
            case 1:
                counterNumberDigit++;
                return "One ";
            case 2:
                counterNumberDigit++;
                return "Ten ";
            case 3:
                counterNumberDigit = 1;
                return "One Hundred ";
            default:
                return "";
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
}

