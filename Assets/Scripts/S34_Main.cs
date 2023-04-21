using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S34_Main : MonoBehaviour
{
    public GameObject planet;
    public float speedRotatePlanet = -5f;

    public List<GameObject> listPrefab;
    private int counter;

    public Vector3 startPosition;
    public Vector3 startPosition2;

    private bool isFirstPosition = true;

    public List<Vector3> cameraPositions;
    private int counterCameraPosition;

    private Camera camera;
    private bool needCameraMove;

    public int interpolationFramesCount = 500;
    int elapsedFrames = 0;
    public List<AudioClip> clips;
    private AudioSource audio;

    private void Start() {
        audio = gameObject.AddComponent<AudioSource>();
        camera = Camera.main;
        camera.transform.position = cameraPositions[0];
        addNewBlock();
    }

    private void addNewBlock() {
        if (counter >= listPrefab.Count)
            return;

        if (counter == 4 || counter == 5)
            startCameraMove();
        
        GameObject block = Instantiate<GameObject>(getNextPrefab());
        block.transform.position = getStartPosition(block);
        block.transform.Rotate(90, 0, 0);
        block.transform.parent = planet.transform;
    }

    private Vector3 getStartPosition(GameObject block) {
        Vector3 pos;
        if (isFirstPosition) {
            isFirstPosition = !isFirstPosition;
            pos =  startPosition;
        } else {
            isFirstPosition = !isFirstPosition;
            pos = startPosition2;
        }

        Scale scale = block.GetComponent<Scale>();
        return new Vector3(
            pos.x - scale.scale.x / 2,
            pos.y,
            pos.z);
    }

    private void Update() {
        rotatePlanet();

        if (needCameraMove)
            cameraMove();
    }

    private void rotatePlanet() {
        planet.transform.Rotate(speedRotatePlanet * Time.deltaTime, 0, 0);
    }

    private void cameraMove() {
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(camera.transform.position, cameraPositions[counterCameraPosition], interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;
        if (Mathf.Abs(camera.transform.position.y - cameraPositions[counterCameraPosition].y) < 0.05f) {
            elapsedFrames = 0;
            needCameraMove = false;
        }
    }

    private void startCameraMove() {
        needCameraMove = true;
        counterCameraPosition++;
    }

    private GameObject getNextPrefab() {
        return listPrefab[counter++];
    }

    public void collidered(GameObject block) {
        delBlock(block);
        addNewBlock();
    }
    private void delBlock(GameObject block) {
        print("delBlock: " + block.name);
        Destroy(block);
    }

    public void playSound() {
        audio.PlayOneShot(clips[counter-1]);
    }
}
