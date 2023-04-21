using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basketball : MonoBehaviour
{
    public GameObject oneMillion;
    public float stopZ = -64f;
    public float speedMillion = 5;
    public float cameraRotateSpeed = 10;
    public Camera camera;


    public AudioClip clip1;
    public AudioClip clip2;
    public AudioClip clip3;

    private AudioSource audio1;
    private AudioSource audio2;
    private AudioSource audio3;
    private bool startMillionBool = false;
    private bool cameraRotate1 = false;
    private bool cameraRotate2 = false;

    private float cameraRotateStop1 = 228f;
    private float cameraRotateStop2 = 15f;
   

    private void Start() {
        audio1 = gameObject.AddComponent<AudioSource>();
        audio2 = gameObject.AddComponent<AudioSource>();
        audio3 = gameObject.AddComponent<AudioSource>();
        Invoke("startMillion", 0.5f);
        audio1.volume = 0.5f;
        audio1.PlayOneShot(clip1);
    }

    private void startMillion() {
        startMillionBool = true;
    }

    private void Update() {
        if(startMillionBool) {
            oneMillion.transform.position = new Vector3(
                oneMillion.transform.position.x,
                oneMillion.transform.position.y,
                oneMillion.transform.position.z + speedMillion * Time.deltaTime);
            if (oneMillion.transform.position.z >= stopZ) {
                startMillionBool = false;
                audio2.PlayOneShot(clip2);
                Invoke("startCameraRotate", 1);
            }
        }
        if (cameraRotate1) {
            camera.transform.Rotate(0, camera.transform.rotation.y + cameraRotateSpeed * Time.deltaTime, 0);
            if (camera.transform.eulerAngles.y >= cameraRotateStop1) {
                cameraRotate1 = false;
                cameraRotate2 = true;
                audio1.Stop();
                audio3.PlayOneShot(clip3);
            }
        }
        if (cameraRotate2) {
            camera.transform.Rotate(camera.transform.rotation.x + cameraRotateSpeed * Time.deltaTime, 0, 0);
            if (camera.transform.eulerAngles.x >= cameraRotateStop2) {
                cameraRotate2 = false;
                
            }
        }
    }

    private void startCameraRotate() {
        cameraRotate1 = true;
    }
}
