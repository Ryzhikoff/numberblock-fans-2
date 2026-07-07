using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class S38_Scibidi : MonoBehaviour
{
    public int interpolationFramesCount = 100;
    private int elapsedFrames = 0;

    public Vector3 cameraStartPosition;
    public Vector3 cameraFinishPosition;
    private bool needCameraMove = false;
    private Camera camera;
    public float delay = 7f;
    public float speedRun = 5f;
    public GameObject forCameraTarget;

    public List<GameObject> goListForRun;
    public GameObject goForFlush;
    public float speedDown = 5f;
    public float speedRotate = 10f;
    public float speedChangeSize = 10f;
    private bool isFlush = false;
    public float delayFlush = 3.767f;

    public GameObject prefabFlyBlock;
    public Vector3 startFlyPosition;
    public float force = 30f;
    public float delayAfterNewBlock = 0.5f;

    public bool isRun = false;

    private void Start() {
        camera = Camera.main;
        //Invoke("startFlush", delayFlush);
        //camera.transform.position = cameraStartPosition;
        //Invoke("startCameraMove", delay);
        //addForce();
    }

    private void addForce() {
        GameObject go = Instantiate<GameObject>(prefabFlyBlock);
        go.transform.position = startFlyPosition;
        Rigidbody rigidbody = go.GetComponent<Rigidbody>();
        rigidbody.AddForce(transform.up * force, ForceMode.Impulse);
        Invoke("addForce", delayAfterNewBlock);
    }

    private void startFlush() {
        isFlush = true;
    }

    private void run() {

        float delta = speedRun * Time.deltaTime;
        foreach (GameObject go in goListForRun) {
            go.transform.position = new Vector3(
                go.transform.position.x,
                go.transform.position.y,
                go.transform.position.z - delta);
        }
    }

    private void startCameraMove() {
        needCameraMove = true;
    }

    private void Update() {
        if (isRun)
            run();
        cameraLook();
        if (needCameraMove)
            cameraMove();

        if (isFlush)
            flush();
        
    }

    public void stopRun() {
        isRun = false;
    }
    private void flush() {
        float deltaTime = Time.deltaTime;

        goForFlush.transform.Rotate(0, speedRotate * deltaTime, 0);

        goForFlush.transform.position += Vector3.down * speedDown * deltaTime;

        goForFlush.transform.localScale = new Vector3(
            goForFlush.transform.localScale.x - goForFlush.transform.localScale.x * (speedChangeSize / 100),
            goForFlush.transform.localScale.y - goForFlush.transform.localScale.y * (speedChangeSize / 100),
            goForFlush.transform.localScale.z - goForFlush.transform.localScale.z * (speedChangeSize / 100));

    }

    

    public void cameraLook() {
        camera.transform.LookAt(forCameraTarget.transform.position);
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
}
