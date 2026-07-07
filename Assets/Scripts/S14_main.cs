using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S14_main : MonoBehaviour
{
    //стартовые позиции блоков
    public List<Vector3> startPositions;
    //СТОП координата У
    public float stopY = 3.375f;
    //скорость падения
    public float speedDown = 3;
    //задержка при появлении
    public float delay = 1f;

    //главный счетчик
    private int counter = 0;
    private BlocksPrefab blocksPrefab;

    //двигаем?
    private bool needMove = false;

    //для передачи инфы от камеры
    public static bool callbackFromCamera = false;

    //временный ГО
    private GameObject currentBlock;
    //старый ГО
    private GameObject oldBlock;

    private SoundMaster soundMaster;

    private void Start() {
        blocksPrefab = gameObject.GetComponent<BlocksPrefab>();
        soundMaster = gameObject.GetComponent<SoundMaster>();
        prepareAddBlock();
    }

    private void prepareAddBlock() {
        if (startPositions.Count < counter+1) {
            print("Counter больше чем список стартовых позиций. Выходим");
            return;
        }
        counter++;
        Invoke("addBlock", delay);
    }

    private void addBlock() {
        GameObject go = Instantiate<GameObject>(blocksPrefab.getPrefab(counter));
        go.transform.position = startPositions[counter-1];
        go.transform.Rotate(new Vector3(0, 180, 0));
        go.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        currentBlock = go;
        needMove = true;
        soundMaster.playDownSound(true);
    }

    private void Update() {
        if (needMove)
            moveBlock();
        if (callbackFromCamera) {
            callbackFromCamera = false;
            soundMaster.playSound(counter);
            prepareAddBlock();
        }
    }

    private void moveBlock() {
        currentBlock.transform.position = new Vector3(
            currentBlock.transform.position.x,
            currentBlock.transform.position.y - speedDown*counter * Time.deltaTime,
            currentBlock.transform.position.z);
        if (currentBlock.transform.position.y <= stopY) {
            //выравниваем
            currentBlock.transform.position = new Vector3(
                currentBlock.transform.position.x,
                stopY,
                currentBlock.transform.position.z);
            needMove = false;
            soundMaster.playDownSound(false);
            soundMaster.playBoom();
            //перадаем лок в стоячий и удаляем старый
            if (oldBlock != null) {
                Destroy(oldBlock);
            }
            oldBlock = currentBlock;
            Camera.main.GetComponent<S14_CameraMove>().startCameraMove(counter);
        }
    }
}
