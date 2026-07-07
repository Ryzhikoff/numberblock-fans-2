using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S49_Main : MonoBehaviour
{
    public GameObject train;
    public float trainSpeed = 5f;
    public Vector3 trainStartPosition;
    public Vector3 trainStopPosition;
    private bool needMoveTrain = false;
    public GameObject targetGO;

    public Camera camera;
    public Vector3 cameraStartPosition;
    public Vector3 cameraFinishPosition;
    private bool needCameraMove = false;
    public Transform blockTransform;
    public bool isLookAtCamera = false;

    public int interpolationFramesCount = 100;
    private int elapsedFrames = 0;
    public float delayStart = 2f;

    public GameObject prefabOne;
    public GameObject block;

    public float posZForStartGenerate;
    public Vector3 startBlockGeneratePos;
    private bool isGenerated = false;

    public float boomVolume = 1.0f;
    public float musicVolume = 1.0f;
    public AudioClip clipBoom;
    public AudioClip clipBoomPrepare;
    public AudioClip clipPrepare;
    public AudioClip clipAfter;
    private AudioSource audio;
    private AudioSource boomAudio;

    private void Start() {
        camera = Camera.main;
        camera.transform.position = cameraStartPosition;
        train.transform.position = trainStartPosition;
        startBlockGeneratePos = block.transform.position;
        needCameraMove = true;
        var scale = block.GetComponent<Scale>();
        targetGO.transform.position = new(
            block.transform.position.x + scale.x / 2,
            block.transform.position.y + scale.y / 2,
            block.transform.position.z + scale.z / 2
            );
        Invoke("startAction", delayStart);
    }

    private void startAction() {
        needCameraMove = true;
        needMoveTrain = true;

        audio = gameObject.AddComponent<AudioSource>();
        audio.volume = musicVolume;
        audio.PlayOneShot(clipPrepare);

        boomAudio = gameObject.AddComponent<AudioSource>();
        boomAudio.volume = boomVolume;
        boomAudio.PlayOneShot(clipBoomPrepare);
    }

    private void Update() {
        if (needMoveTrain)
            moveTrain();
        if (needCameraMove)
            cameraMove();
        if(isLookAtCamera)
            camera.transform.LookAt(targetGO.transform);

        //if (checkTrainPositionForGenerate() && !isGenerated)
        //    generateBlocks();
    }

    private void moveTrain() {
        train.transform.position += Vector3.back * (trainSpeed * Time.deltaTime);
        if ( Mathf.Abs(train.transform.position.z - trainStopPosition.z) < 0.05f) {
            needMoveTrain = false;
        }
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(cameraStartPosition, cameraFinishPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;
        if (Mathf.Abs(camera.transform.position.x - cameraFinishPosition.x) < 0.05f) {
            needCameraMove = false;
            elapsedFrames = 0;

        }
    }

    public void generateBlocks() {
        isGenerated = true;
        var scale = block.GetComponent<Scale>();
        for (int x = 0; x < scale.x; x++) {
            for(int z = 0; z < scale.z; z++) {
                for(int y = 0; y < scale.y; y++) {
                    GameObject bl = Instantiate<GameObject>(prefabOne);
                    bl.transform.position = new Vector3(
                        startBlockGeneratePos.x + x,
                        startBlockGeneratePos.y + y,
                        startBlockGeneratePos.z + z);
                }
            }
        }
        Destroy(block);
        audio.Stop();
        audio.PlayOneShot(clipAfter);

        boomAudio.Stop();
        boomAudio.PlayOneShot(clipBoom);
    }

    private bool checkTrainPositionForGenerate() {
        //print("Mathf.Abs(train.transform.position.z - posZForStartGenerate): " + Mathf.Abs(train.transform.position.z - posZForStartGenerate));
        if (Mathf.Abs(train.transform.position.z - posZForStartGenerate) < 0.5f) {
            return true;
        }
        return false;
    }
}
