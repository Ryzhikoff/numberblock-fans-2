using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class S22_Main : MonoBehaviour
{
    [SerializeField] 
    private Camera _camera;
    public List<BlockInfo> blocksList;
    public int counter = 0;

    public int interpolationFramesCount = 300;
    private int elapsedFrames = 0;

    public Vector3 initCamPos = Vector3.zero;
    private Vector3 startPosition = Vector3.zero;
    private Vector3 endPosition = Vector3.zero;
    private GameObject currentBlock;
    public float delay = 2f;
    private bool needCameraMove = false;

    private void Start() {
        _camera = Camera.main;
        _camera.transform.position = initCamPos;
        SetNextPosition();
    }

    private void SetNextPosition() {
        if (counter >= blocksList.Count) {
            return;
        }
        endPosition = blocksList[counter].position;
        startPosition = _camera.transform.position;
        currentBlock = blocksList[counter].block;
        counter++;
        needCameraMove = true;
    }

    private void Update() {
        if (needCameraMove) {
            CameraMove();
            Camera.main.transform.LookAt(currentBlock.transform);
        }
    }

    private void CameraMove() {
        float t = (float) elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startPosition, endPosition, t);

        elapsedFrames++;
        if (elapsedFrames > interpolationFramesCount) {
            elapsedFrames = interpolationFramesCount;
        }

        _camera.transform.position = interpolatedPosition;

        // проверка на завершение движения
        if (Vector3.SqrMagnitude(interpolatedPosition - endPosition) < 0.05f * 0.05f) {
            elapsedFrames = 0;
            needCameraMove = false;
            Invoke(nameof(SetNextPosition), delay);
        }
    }
}

[System.Serializable]
public class BlockInfo {
    public GameObject block;
    public Vector3 position;
}
