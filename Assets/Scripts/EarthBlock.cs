using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EarthBlock : MonoBehaviour
{
    public GameObject planet1;
    public GameObject planet2;
    public GameObject planet3;
    public GameObject planet4;
    public Vector3 startPosition;
    public Vector3 startPositionSaturn;
    public Vector3 startPositionJupiter;
    public Vector3 startPositionSun;

    public BlocksPrefab blocksPrefab;

    //23
    //29
    private int counter = 1;
    private GameObject currentGo;

    public GameObject textGO;
    private Text text;
    public float delayChangeColor = 2f;

    private static bool _isAddBlock = true;
    public static bool isAddBlock2 = true;

    public static bool isAddBlock {
        get {
            return _isAddBlock;
        }
        set {
            _isAddBlock = value;
        }
    }

    private void Start() {
        text = textGO.GetComponent<Text>();
        //так как начинаем с больших блоков - устанавливаем true
        blocksPrefab.isBigBlock = false;
        //что бы начать с десяток
        blocksPrefab.counterForBig = 1;
        addBlock();
        changeTextColor();
    }

    //Септилион
    //Октиллион

    public void addBlock() {
        if (counter >= 14)
            return;
        //6
        //print("counter:  " + counter + " prefab: " + blocksPrefab.getPrefab(counter));

        //прошли все префабы Земли - ставим Сатурн
        if (counter == 6) {
            //print("прошли первый if");
            if (isAddBlock2) {
                //print("второй if");
                //ставим оба false, при достижение новой планеты - будет isAddBlock = true;
                isAddBlock = false;
                isAddBlock2 = false;

                switch (counter) {
                    case 6:
                        print("на месте");
                        startPosition = startPositionSaturn;
                        planet1 = planet2;
                        break;
                    case 13:
                        startPosition = startPositionJupiter;
                        Destroy(planet2);
                        planet1 = planet3;
                        break;
                    case 19:
                        startPosition = startPositionSun;
                        Destroy(planet3);
                        planet1 = planet4;
                        print("зашли в поле");
                        break;
                }
                return;
            }
        }

        if (isAddBlock) {
            isAddBlock2 = true;
            GameObject go = Instantiate<GameObject>(blocksPrefab.getPrefab(counter));
            go.transform.position = new Vector3(
                -(float)go.GetComponent<Scale>().scale.x / 2f,
                startPosition.y,
                startPosition.z);
            go.transform.Rotate(90f, 0, 0);
            go.transform.parent = planet1.transform;
            currentGo = go;
            setText();
            counter++;
        }
    }

    private void changeTextColor() {
        text.color = new Color(Random.value, Random.value, Random.value);
        Invoke("changeTextColor", delayChangeColor);
    }
    public void delBlock() {
        Destroy(currentGo);
        //4
        if (counter == 36)
            Camera.main.GetComponent<EarthCameraMove>().cameraMove(1);
        //5
        //if (counter == 29)
        //    Camera.main.GetComponent<EarthCameraMove>().cameraMove(2);
        //перемещение на 2 планету
        //7
        if (counter == 38)
            Camera.main.GetComponent<EarthCameraMove>().cameraMove(3);
        //10
        if (counter == 40)
            Camera.main.GetComponent<EarthCameraMove>().cameraMove(4);
        //перемещение на 3 планету
        if (counter == 13)
            Camera.main.GetComponent<EarthCameraMove>().cameraMove(5);
        if (counter == 16)
            Camera.main.GetComponent<EarthCameraMove>().cameraMove(6);
        //перемещение на 4 планету
        if (counter == 19)
            Camera.main.GetComponent<EarthCameraMove>().cameraMove(7);
        if (counter == 22)
            Camera.main.GetComponent<EarthCameraMove>().cameraMove(8);

    }
    
    private void setText() {
        switch (counter+2) {
            case 0: text.text = "1";
                break;
            case 1:
                text.text = "10";
                break;
            case 2:
                text.text = "100";
                break;
            case 3:
                text.text = "1,000";
                break;
            case 4:
                text.text = "10,000";
                break;
            case 5:
                text.text = "100,000";
                break;
            case 6:
                text.text = "1,000,000";
                break;
            case 7:
                text.text = "10,000,000";
                break;
            case 8:
                text.text = "100,000,000";
                break;
            case 9:
                text.text = "1,000,000,000";
                break;
            case 10:
                text.text = "10,000,000,000";
                break;
            case 11:
                text.text = "100,000,000,000";
                break;
            case 12:
                text.text = "1,000,000,000,000";
                break;
            case 13:
                text.text = "10,000,000,000,000";
                break;
            case 14:
                text.text = "100,000,000,000,000";
                break;
            case 15:
                text.text = "1,000,000,000,000,000";
                break;
            case 16:
                text.text = "10,000,000,000,000,000";
                break;
            case 17:
                text.text = "100,000,000,000,000,000";
                break;
            case 18:
                text.text = "1,000,000,000,000,000,000";
                break;
            case 19:
                text.text = "10,000,000,000,000,000,000";
                break;
            case 20:
                text.text = "100,000,000,000,000,000,000";
                break;
            case 21:
                text.text = "1,000,000,000,000,000,000,000";
                break;
            case 22:
                text.text = "10,000,000,000,000,000,000,000";
                break;
            case 23:
                text.text = "100,000,000,000,000,000,000,000";
                break;
            case 24:
                text.text = "1,000,000,000,000,000,000,000,000";
                break;
            case 25:
                text.text = "10,000,000,000,000,000,000,000,000";
                break;
            case 26:
                text.text = "100,000,000,000,000,000,000,000,000";
                break;
            case 27:
                text.text = "1,000,000,000,000,000,000,000,000,000";
                break;
            case 28:
                text.text = "10,000,000,000,000,000,000,000,000,000";
                break;
            case 29:
                text.text = "100,000,000,000,000,000,000,000,000,000";
                break;
            case 30:
                text.text = "1,000,000,000,000,000,000,000,000,000,000";
                break;
            case 31:
                text.text = "10,000,000,000,000,000,000,000,000,000,000";
                break;
            case 32:
                text.text = "100,000,000,000,000,000,000,000,000,000,000";
                break;
            case 33:
                text.text = "1,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 34:
                text.text = "10,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 35:
                text.text = "100,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 36:
                text.text = "1,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 37:
                text.text = "10,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 38:
                text.text = "100,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 39:
                text.text = "1,000,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 40:
                text.text = "10,000,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 41:
                text.text = "100,000,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 42:
                text.text = "1,000,000,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 43:
                text.text = "10,000,000,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            case 45:
                text.text = "100,000,000,000,000,000,000,000,000,000,000,000,000,000,000";
                break;
            default:
                text.text = "1,000,000,000,000";
                break;
        }
    }
}
