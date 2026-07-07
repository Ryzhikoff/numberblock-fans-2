using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S65_Main1 : MonoBehaviour
{
    [SerializeField]
    public List<GroupBlock> groups = new();
    public int counter = 0;
    private int localIndex = 0;

    public Vector3 position = Vector3.zero;

    public GameObject targetGO;
    public Vector3 cameraOffset = Vector3.zero;
    private Camera camera;
    public Vector3 initCameraPos = new(3, 3, 3);
    private Vector3 targetCameraPosition;
    private Vector3 startCameraPosition;
    private bool needCameraMove;
    public int interpolationFramesCount = 300;
    private int elapsedFrames = 0;

    public float delay = 3f;
    private BlockData currentData;
    private bool isMoveToStart = false;

    public AudioSource audio;
    public float volume = 1.0f;

    public TextMeshProUGUI textMesh;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        audio = gameObject.AddComponent<AudioSource>();
        camera.transform.position = initCameraPos;
        PlaceBlock();
        PrepareCameraMove();
    }

    private void PlaceBlock() {
        if (counter < 0 || counter >= groups.Count)
            return;

        var blocks = groups[counter].blocks;
        foreach (BlockData data in blocks) {
            var block = Instantiate(data.prefab);
            var scale = block.GetComponent<Scale>();
            position += scale.x * Vector3.right;
            block.transform.position = position;
            position += scale.x * Vector3.right;
        }
        position = Vector3.zero;
    }

    private void ClearBlocks() {
        var blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject block in blocks) {
            Destroy(block);
        }
    }

    private void PrepareCameraMove() {
        if (counter < 0 || counter >= groups.Count) {
            print("end of groups, counter: " + counter);
            return;
        }
        var group = groups[counter];

        print($"counter: {counter} localIndex: {localIndex} group.blocks.Count: {group.blocks.Count}");

        if (localIndex < 0 || localIndex >= group.blocks.Count) {
            if (!isMoveToStart) {
                targetCameraPosition = initCameraPos;
                startCameraPosition = camera.transform.position;
                needCameraMove = true;
                isMoveToStart = true;
                SetText("");
                return;
            }
            // нужно перейти к след группе
            localIndex = 0;
            counter++;
            ClearBlocks();
            PlaceBlock();
            PrepareCameraMove();
        } else {
            isMoveToStart = false;
            var data = group.blocks[localIndex];
            currentData = data;
            SetNewPositionForCamera(data.prefab);
            //SetPositionForTargetGo(data.prefab);
            needCameraMove = true;
        }
    }

    private void SetNewPositionForCamera(GameObject block) {
        Vector3 scale = block.GetComponent<Scale>().scale;

        var blocks = GameObject.FindGameObjectsWithTag("Block");

        var origName = block.name;

        Vector3 blockPos = Vector3.zero;

        foreach(GameObject bl in blocks) {
            if (bl.name.Split("(")[0].Equals(origName)) {
                print(bl.name);
                blockPos = bl.transform.position;
                break;
            }
        }

        Vector3 newPos = new Vector3(
            blockPos.x + scale.x * cameraOffset.x,
            scale.y * cameraOffset.y,
            scale.y * cameraOffset.z);
        targetCameraPosition = newPos;
        startCameraPosition = camera.transform.position;
        print($"blockPos: {blockPos} newPos: {newPos}");
    }

    private void SetPositionForTargetGo(GameObject block) {
        Vector3 scale = block.GetComponent<Scale>().scale;
        Vector3 pos = new Vector3(
            block.transform.position.x + scale.x / 2f,
            scale.y * 0.4f,
            scale.z / 2f);
        targetGO.transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        if (needCameraMove) {
            CameraMove();
        }
    }

    private void CameraMove() {
        //camera.transform.LookAt(targetGO.transform);

        float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startCameraPosition, targetCameraPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;

        //var look = Quaternion.LookRotation(targetGO.transform.position - startCameraPosition);
        //camera.transform.rotation = Quaternion.Lerp(transform.rotation, look, Time.deltaTime * speedCameraRotation);

        if (elapsedFrames >= interpolationFramesCount) {
            elapsedFrames = 0;
            needCameraMove = false;
            EndCameraMove();

        }
    }


    private void EndCameraMove() {
        if (isMoveToStart) {
            PrepareCameraMove();
        } else {
            localIndex++;
            var clip = currentData.clip;
            var text = currentData.prefab.GetComponent<Scale>().number.ToString();
            SetText(text);
            audio.PlayOneShot(clip, volume);
            Invoke(nameof(PrepareCameraMove), delay);
        }
    }

    private void SetText(string number) {
        textMesh.text = number;
    }

    private void SetPositionForTargetGo(Scale scale) {
        Vector3 pos = new Vector3(
            scale.x / 2f,
            scale.y * 0.4f,
            scale.z / 2f);
        targetGO.transform.position = pos;
    }

    private void SetNewPositionForCamera(Scale scale) {

        Vector3 newPos = new Vector3(
            scale.x * cameraOffset.x,
            scale.y * cameraOffset.y,
            scale.y * cameraOffset.z);
        targetCameraPosition = newPos;
        startCameraPosition = camera.transform.position;
        needCameraMove = true;
    }

}

[Serializable]
public class GroupBlock {
    [SerializeField]
    public List<BlockData> blocks = new();
}

[Serializable]
public class BlockData {
    [SerializeField]
    public GameObject prefab;
    [SerializeField]
    public AudioClip clip;
}
