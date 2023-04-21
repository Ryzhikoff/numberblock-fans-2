using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class S31_Main : MonoBehaviour
{
    public List<Vector3> positions;
    public List<GameObject> targetObject;

    public int counter = 0;
    public float speed = 10;

    public float delayBetweenCameraMove = 1f;

    int elapsedFrames = 0;
    public int interpolationFramesCount = 20000;
    private bool needCameraMove = false;
    private Vector3 targetCameraPosition;

    private Vector3 direction;

    private void Start() {
        transform.position = positions[counter];
        transform.LookAt(targetObject[counter].transform);
        counter++;
        Invoke("startCameraMove", delayBetweenCameraMove);
    }

    private void startCameraMove() {
        if (counter >= positions.Count)
            return;

        targetCameraPosition = positions[counter];
        
        needCameraMove = true;
    }


    private void Update() {
        if (needCameraMove)
            cameraMove();
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(transform.position, targetCameraPosition, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        transform.position = interpolatedPosition;

        //transform.LookAt(targetObject[counter].transform);
        var look = Quaternion.LookRotation(targetObject[counter].transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, look, Time.deltaTime * speed);


        if ((Mathf.Abs(transform.position.z - targetCameraPosition.z) < 0.05f) &&
            (Mathf.Abs(transform.position.y - targetCameraPosition.y) < 0.05f) &&
            (Mathf.Abs(transform.position.x - targetCameraPosition.x) < 0.05f)) {

            needCameraMove = false;
            elapsedFrames = 0;
            counter++;
            Invoke("startCameraMove", delayBetweenCameraMove);
        }
    }
}
