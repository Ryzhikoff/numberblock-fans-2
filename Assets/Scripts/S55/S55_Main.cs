using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S55_Main : MonoBehaviour
{
    public GameObject ball;
    private Camera camera;
    public Vector3 cameraStartPosition;

    private void Start() {
        camera = Camera.main;
        camera.transform.position = cameraStartPosition;
    }

    private void Update() {
        camera.transform.LookAt(ball.transform.position);
    }

    private void cameraMove() {
        if (ball != null) {
            camera.transform.position = new Vector3(
                ball.transform.position.x, 
                camera.transform.position.y,
                camera.transform.position.z);
        }
    }
}
