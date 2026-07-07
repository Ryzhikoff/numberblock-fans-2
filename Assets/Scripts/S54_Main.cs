using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class S54_Main : MonoBehaviour
{
    public List<GameObject> prefabList;

    public GameObject targetGO;
    public Vector3 cameraPositionOffset = new Vector3(1.5f, 1.5f, -1.3f);
    public float speedRotate = 5;

    private int counter = 0;
    private GameObject currentBlock;

    private Camera camera;
    private Vector3 targetCameraPosition;
    private Vector3 startCameraPosition;
    private bool needCameraMove;
    public int interpolationFramesCount = 300;
    private int elapsedFrames = 0;
    private float sumDegrees = 0;
    private bool needCameraRotate = false;

    public AudioClip clipMove;
    public float clipMoveVolume = 1;
    public AudioClip clipRotate;
    public List<AudioClip> clipsRotate;
    public float clipRotateVolume = 1;
    private AudioSource audio;

    public TextMeshProUGUI textMeshProUGUI;

    private void Start() {
        camera = Camera.main;
        audio = gameObject.AddComponent<AudioSource>();
        addBlock();
    }

    private void addBlock() {
        if (counter > prefabList.Count - 1) return;

        Vector3 oldBlockPosition = currentBlock == null ? Vector3.zero : currentBlock.transform.position + new Vector3(currentBlock.GetComponent<Scale>().x, 0, 0);

        currentBlock = Instantiate(prefabList[counter]);

        
        currentBlock.transform.position = oldBlockPosition + new Vector3(currentBlock.GetComponent<Scale>().x, 0, 0);

        setPositionForTargetGo();
        setNewPositionForCamera();
        counter++;
        needCameraMove = true;


    }

    private void setPositionForTargetGo() {
        Vector3 scale = prefabList[counter].GetComponent<Scale>().scale;
        Vector3 pos = new Vector3(
            currentBlock.transform.position.x + (scale.x / 2f),
            currentBlock.transform.position.y + (scale.y * 0.4f),
            currentBlock.transform.position.z + (scale.z / 2f));
        targetGO.transform.position = pos;
    }

    private void setNewPositionForCamera() {
        Vector3 scale = prefabList[counter].GetComponent<Scale>().scale;

        Vector3 newPos = new Vector3(
            currentBlock.transform.position.x + scale.x * cameraPositionOffset.x,
            currentBlock.transform.position.y + scale.y * cameraPositionOffset.y,
            currentBlock.transform.position.z + scale.y * cameraPositionOffset.z);
        targetCameraPosition = newPos;
        startCameraPosition = camera.transform.position;
        needCameraMove = true;
        playSound(SoundType.MOVE);
    }


    private void Update() {
        if (needCameraMove) {
            cameraMove();
        } 

        if (needCameraRotate) {
            rotateCamera();
        }
    }

    private void cameraMove() {
        //camera.transform.LookAt(targetGO.transform);

        float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startCameraPosition, targetCameraPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;

        if (Mathf.Abs(camera.transform.position.x - targetCameraPosition.x) < 0.05f) {
            camera.transform.position = targetCameraPosition;
            setText();
            elapsedFrames = 0;
            needCameraMove = false;
            needCameraRotate = true;
            playSound(SoundType.ROTATE);

        }
    }

    private void setText() {
        var number =  currentBlock.GetComponent<Scale>().number.ToString("#,#", CultureInfo.InvariantCulture);
        textMeshProUGUI.text = number;
    }

    private void rotateCamera() {
        float newAdd = speedRotate * Time.deltaTime;
        sumDegrees += newAdd;
        camera.transform.RotateAround(targetGO.transform.position, Vector3.down, newAdd);
        camera.transform.LookAt(targetGO.transform);
        if (sumDegrees >= 360f) {
            sumDegrees = 0;
            needCameraRotate = false;
            addBlock();
        }

    }

    private void playSound(SoundType type) {
        switch (type) {
            case SoundType.MOVE:
                audio.volume = clipMoveVolume;
                audio.clip = clipMove;
                break;
            case SoundType.ROTATE:
                audio.volume = clipRotateVolume;
                audio.clip = clipsRotate[UnityEngine.Random.Range(0, clipsRotate.Count - 1)];
                ;
                break;
        }

        audio.Play();
    }

    enum SoundType {
        MOVE,
        ROTATE
    }
}
