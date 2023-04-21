using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using TMPro;
using UnityEngine;

public class S32_Main : MonoBehaviour
{
    public List<GameObject> prefabList;
    private GameObject currentPrefab;

    public GameObject parentGo;

    public TextMeshProUGUI text;
    private long coef = 1;
    private long counterForText = 1;

    public float delayAfterAddBlock = 0.5f;
    public float delayAfterNewMatrix = 2f;

    public int counter = 0;

    private S32_Matrix matrix;

    private bool reverseZ;
    private bool reverseX;

    public List<Vector3> cameraPositions;
    private Vector3 targetCameraPosition;
    private bool needCameraMove;
    public int interpolationFramesCount = 300;
    private int elapsedFrames = 0;
    private Camera camera;

    private GameObject currentBlock;
    private Vector3 targetPosition;
    public float coefForY = 10;
    public float speedMoveDown = 10;
    private bool needMoveBlock = false;

    public GameObject targetGO;
    public Vector3 cameraPositionOffset;
    public float speedCameraRotation = 10;

    private void Start() {
        camera = Camera.main;
        //camera.transform.LookAt(targetGO.transform);
        //camera.transform.position = cameraPositions[counter];
        prepareAddBlock();
    }

    private void prepareAddBlock() {
        setMatrixAndCurrentPrefab();
        reverseX = false;
        reverseZ = false;
        setPositionForTargetGo();
        setNewPositionForCamera();
        Invoke("addBlock", delayAfterNewMatrix);
    }

    private void setMatrixAndCurrentPrefab() {
        currentPrefab = prefabList[counter];

        Scale scaleNext = prefabList[counter + 1].GetComponent<Scale>();
        Scale scaleCur = currentPrefab.GetComponent<Scale>();

        matrix = new S32_Matrix(
            (int)scaleNext.scale.x / scaleCur.scale.x,
            (int)scaleNext.scale.y / scaleCur.scale.y,
            (int)scaleNext.scale.z / scaleCur.scale.z);
       // print(currentPrefab.name);
        print("New Matrix: " + matrix);
        matrix.array[0, 0, 0] = true;
        counter++;
    }

    private void setNewPositionForCamera() {
        Vector3 scale = prefabList[counter].GetComponent<Scale>().scale;

        Vector3 newPos = new Vector3(
            scale.x * cameraPositionOffset.x,
            scale.y * cameraPositionOffset.y,
            scale.y * cameraPositionOffset.z);
        targetCameraPosition = newPos;
        needCameraMove = true;
    }

    private void setPositionForTargetGo() {
        Vector3 scale = prefabList[counter].GetComponent<Scale>().scale;
        Vector3 pos = new Vector3(
            scale.x / 2f,
            scale.y * 0.4f,
            scale.z / 2f);
        targetGO.transform.position = pos;
    }

    private void addBlock() {

        if (matrix.array[matrix.x -1, matrix.y -1, matrix.z -1]) {
            endAddBlock();
            return;
        }

        for (int y = 0; y < matrix.y; y++) {
            for (int x = 0; x < matrix.x; x++) {
                for (int z = 0; z < matrix.z; z++) {
                    if (matrix.array[x, y, z])
                        continue;

                    currentBlock = Instantiate<GameObject>(currentPrefab);
                    //block.transform.SetParent(parentGo.transform);
                    targetPosition = getTargetPosition(x, y, z);
                    currentBlock.transform.position = getBlockPosition();

                    matrix.array[x, y, z] = true;

                    reverseZ = (z == matrix.z - 1) ? changeReverseZAndCheckReverseX(x) : reverseZ;
                    //Invoke("addBlock", delayAfterAddBlock);
                    needMoveBlock = true;
                    return;
                }
            }
        }
    }

    private void moveBlock() {
        currentBlock.transform.position = new Vector3(
        currentBlock.transform.position.x,
        currentBlock.transform.position.y - speedMoveDown * Time.deltaTime,
        currentBlock.transform.position.z);

        if (currentBlock.transform.position.y <= targetPosition.y) {
            currentBlock.transform.position = targetPosition;
            currentBlock.transform.SetParent(parentGo.transform);
            needMoveBlock = false;      
            setText();
            Invoke("addBlock", delayAfterAddBlock);
        }
    }

    private bool changeReverseZAndCheckReverseX(int x) {
        reverseX = (x == matrix.x - 1) ? !reverseX : reverseX;
        return !reverseZ;
    }

    private Vector3 getTargetPosition(int x, int y, int z) {
        Scale scale = currentPrefab.GetComponent<Scale>();

        int posX = reverseX ? matrix.x - 1 - x : x;
        int posZ = reverseZ ? matrix.z - 1 - z : z;

        //print(string.Format("x = {0}, y = {1}, z = {2}, posX = {3}, posZ = {4}, reverseX = {5}, reverseZ = {6}", x, y, z, posX, posZ, reverseX, reverseZ));

        Vector3 pos = new Vector3(
            posX * scale.scale.x,
            y * scale.scale.y,
            posZ * scale.scale.z);

        return pos;
    }

    private Vector3 getBlockPosition() {
        float y = currentPrefab.GetComponent<Scale>().scale.y;
        Vector3 pos = new Vector3(
            targetPosition.x,
            targetPosition.y + y * coefForY,
            targetPosition.z);
        return pos;
    }

    private void endAddBlock() {
        clearAllBlocks();
        setNextBlock();
        coef *= newCoefForText();
        //Invoke("startCameraMove", delayAfterNewMatrix);
        if (counter < prefabList.Count - 1) {
            prepareAddBlock();
        }
    }

    private long newCoefForText() {
        Scale oldScale = currentPrefab.GetComponent<Scale>();
        Scale newScale = prefabList[counter].GetComponent<Scale>();
        long oldSum = oldScale.x * oldScale.y * oldScale.z;
        long newSum = newScale.x * newScale.y * newScale.z;
        long delta = newSum / oldSum;
        print("oldScale: " + oldScale.scale + " newScale: " + newScale.scale + " delta: " + delta);
        return delta;
    }
    private void clearAllBlocks() {
        GameObject[] goList = GameObject.FindGameObjectsWithTag("Block");
        foreach(GameObject go in goList) {
            Destroy(go);
        }
    }

    private void setNextBlock() {
        GameObject block = Instantiate<GameObject>(prefabList[counter]);
        block.transform.position = Vector3.zero;
    }

    private void FixedUpdate() {
        if (needCameraMove)
            cameraMove();
        if (needMoveBlock)
            moveBlock();
    }

    private void startCameraMove() {
        targetCameraPosition = cameraPositions[counter - 1];
        needCameraMove = true;
    }

    private void cameraMove() {
        //camera.transform.LookAt(targetGO.transform);

        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(camera.transform.position, targetCameraPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;

        var look = Quaternion.LookRotation(targetGO.transform.position - camera.transform.position);
        camera.transform.rotation = Quaternion.Lerp(transform.rotation, look, Time.deltaTime * speedCameraRotation);

        if (Mathf.Abs(camera.transform.position.y - targetCameraPosition.y) < 0.5f) {
            elapsedFrames = 0;
            needCameraMove = false;

        }
    }

    private void setText() {
        counterForText = counterForText + coef;
        text.text = counterForText.ToString("#,#", CultureInfo.InvariantCulture);
    }
}
