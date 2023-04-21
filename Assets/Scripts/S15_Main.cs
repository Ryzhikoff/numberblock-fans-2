using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S15_Main : MonoBehaviour
{
    public List<Vector3> positions;
    private int counter = 1;

    private BlocksPrefab blocksPrefab;
    public FreeCamera freeCamera;

    public GameObject targetTransform;

    //текущий объект
    private GameObject currentGo;
    //скорость падения
    public float speedDown = 1;
    private bool needMove = false;

    //где останавливаемся
    private float stopYPosition = 0;

    private SoundMaster soundMaster;
    public Text text;

    private void Start() {
        blocksPrefab = gameObject.GetComponent<BlocksPrefab>();
        soundMaster = gameObject.GetComponent<SoundMaster>();
    }

    private void Update() {
        if (Input.GetKeyDown("space")) {
            createNewBlock();
        }

        if (needMove) {
            moveBlock();
        }
    }

    private void createNewBlock() {
        GameObject go = Instantiate<GameObject>(blocksPrefab.getPrefab(counter));
        go.transform.position = positions[counter - 1];

        //инициализации таргет-блока
        GameObject target = Instantiate<GameObject>(targetTransform);
        //Получаем размеры блока - они же смещение для Таргет-блока
        var offsetY = go.GetComponent<Scale>().scale.y;
        var offsetX = go.GetComponent<Scale>().scale.x / 2;
        var offsetZ = go.GetComponent<Scale>().scale.z / 2;

        target.transform.position = new Vector3(
            positions[counter - 1].x + offsetX,
            positions[counter - 1].y + offsetY,
            positions[counter - 1].z + offsetZ);
        //устанавливаем таргетТрансформ для каменры
        freeCamera.setTarget(target.transform);

        currentGo = go;
        needMove = true;
        
        soundMaster.boomSoundUp();
        soundMaster.playDownSound(true);
        
        print("Добавили блок: " + currentGo + " координаты: " + currentGo.transform.position);
    }

    private void moveBlock() {
        currentGo.transform.position = new Vector3(
            currentGo.transform.position.x,
            currentGo.transform.position.y - speedDown * Time.deltaTime,
            currentGo.transform.position.z);
        if (currentGo.transform.position.y <= stopYPosition) {
            speedDown += 10;
            soundMaster.playDownSound(false);
            soundMaster.playBoom();
            soundMaster.playSound(counter);
            needMove = false;
            setText();
            counter++;
            currentGo.transform.position = new Vector3(
                currentGo.transform.position.x,
                stopYPosition,
                currentGo.transform.position.z);
        }
    }

    private void setText() {
        string txt = "";

        switch (counter) {
            case 1: txt = "1";
                break;
            case 2: txt = " 10";
                break;
            case 3: txt = "100";
                break;
            case 4: txt = "1,000";
                break;
            case 5: txt = "10,000";
                break;
            case 6: txt = "100,000";
                break;
            case 7: txt = "1,000,000";
                break;
            default: txt = "1";
                break;
        }
        text.text = txt;
    }
}
