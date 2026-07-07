using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S39_Main : MonoBehaviour
{
    public GameObject prefabOne;
    public GameObject targetGO;
    public GameObject thousand;
    public GameObject attakerGO;
    public List<GameObject> blocks;
    public GameObject prefabThousand;
    private GameObject currentThousand;
    public Vector3 startPosition;

    public float speedRotate = 10f;
    public int posYForTransform;
    public int counter = 0;
    public int stopCounter = 10;

    private Camera camera;
    private bool stopper = true;

    private void Start() {
        camera = Camera.main;
        newBlock();
    }

    private void newBlock() {
        GameObject go = GameObject.Instantiate<GameObject>(prefabThousand);
        go.transform.position = startPosition;
        currentThousand = go;
        counter++;
    }

    private void Update() {
        if (stopCounter > counter && checkAttakerPosition()) {
            startAttack();
        }
        cameraLookAt();
        cameraRotate();
    }

    private bool checkAttakerPosition() {
        return currentThousand.transform.position.y < posYForTransform; 
    }

    private void startAttack() {
        stopper = false;

        Destroy(currentThousand);
        newBlock();
        
        for (int z = -5; z < 5; z++) {
            for (int y = posYForTransform; y < posYForTransform + 10; y++) {
                for (int x = -3; x < 7; x++) {
                    GameObject go = Instantiate<GameObject>(prefabOne);
                    go.transform.position = new Vector3(x, y, z);
                }
            }
        }
    }

    private void cameraLookAt() {
        camera.transform.LookAt(targetGO.transform);
    }

    private void cameraRotate() {
        camera.transform.RotateAround(targetGO.transform.position, Vector3.up, speedRotate * Time.deltaTime);
    }
}
