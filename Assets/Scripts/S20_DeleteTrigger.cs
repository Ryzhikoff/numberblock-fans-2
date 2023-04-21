using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S20_DeleteTrigger : MonoBehaviour
{
    public GameObject controllerGO;
    private S20_Main controller;
    public float speedDisappearance = 30;
    public float speedStepUp = 1.2f;
    private List<GameObject> listBlock = new List<GameObject>();
    private int counter = 0;
    public TextMeshProUGUI textCounter;
    public TextMeshProUGUI speedCounter;
    public AudioClip soundCollider;
    public AudioClip sound1;
    public AudioClip sound100;
    public AudioClip sound200;
    public AudioClip sound300;
    public AudioClip sound400;
    public AudioClip sound500;
    public AudioClip sound600;
    public AudioClip sound700;
    public AudioClip sound800;
    public AudioClip sound900;
    public AudioClip sound1000;
    public AudioClip soundOther;
    public AudioClip sound1100;
    public AudioClip sound1200;
    public AudioClip sound1300;
    public AudioClip sound1400;
    public AudioClip sound1500;
    public AudioClip sound1600;
    public AudioClip sound1700;
    public AudioClip sound1800;
    public AudioClip sound1900;
    public AudioClip sound2000;
    public AudioClip sound3000;
    public AudioClip sound4000;
    public AudioClip sound5000;
    public AudioClip sound6000;
    public AudioClip sound7000;
    public AudioClip sound8000;
    public AudioClip sound9000;
    public AudioClip sound10000;
    public float volumeSoundCoin = 0.7f;
    public float volumeSoundVoice = 0.7f;
    public float volumeSoundBoom = 0.1f;
    private AudioSource audioCoin;
    private AudioSource audioVoice;
    private AudioSource audioBoom;

    private void Start() {
        controller = controllerGO.GetComponent<S20_Main>();
        audioCoin = gameObject.AddComponent<AudioSource>();
        audioVoice = gameObject.AddComponent<AudioSource>();
        audioBoom = gameObject.AddComponent<AudioSource>();
        audioCoin.volume = volumeSoundCoin;
        audioVoice.volume = volumeSoundVoice;
        audioBoom.volume = volumeSoundBoom;
        speedCounter.text = controller.speedMoveBlock.ToString();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Block") {
            //audioCoin.PlayOneShot(soundCollider);
            //listBlock.Add(other.gameObject);
            textCounter.text = counter.ToString();
            playSound();

            controller.delBlockInList(other.gameObject);
            Destroy(other.gameObject);

            if (counter == 1000) {
                controller.isGoing = false;
                clearAllBlock();
            }
            counter++;
        }
    }

    private void clearAllBlock() {
        GameObject[] gList = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject go in gList) {
            Destroy(go);
        }
    }

    private void playSound() {
        switch (counter) {
            case 1:
                audioVoice.PlayOneShot(sound1);
                return;
            case 100:
                audioVoice.PlayOneShot(sound100);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 200:
                audioVoice.PlayOneShot(sound200);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 300:
                audioVoice.PlayOneShot(sound300);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 400:
                audioVoice.PlayOneShot(sound400);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 500:
                audioVoice.PlayOneShot(sound500);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 600:
                audioVoice.PlayOneShot(sound600);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 700:
                audioVoice.PlayOneShot(sound700);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 800:
                audioVoice.PlayOneShot(sound800);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 900:
                audioVoice.PlayOneShot(sound900);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1000:
                audioVoice.PlayOneShot(sound1000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1100:
                audioVoice.PlayOneShot(sound1100);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1200:
                audioVoice.PlayOneShot(sound1200);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1300:
                audioVoice.PlayOneShot(sound1300);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1400:
                audioVoice.PlayOneShot(sound1400);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1500:
                audioVoice.PlayOneShot(sound1500);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1600:
                audioVoice.PlayOneShot(sound1600);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1700:
                audioVoice.PlayOneShot(sound1700);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1800:
                audioVoice.PlayOneShot(sound1800);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 1900:
                audioVoice.PlayOneShot(sound1900);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 2000:
                audioVoice.PlayOneShot(sound2000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 3000:
                audioVoice.PlayOneShot(sound3000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 4000:
                audioVoice.PlayOneShot(sound4000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 5000:
                audioVoice.PlayOneShot(sound5000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 6000:
                audioVoice.PlayOneShot(sound6000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 7000:
                audioVoice.PlayOneShot(sound7000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 8000:
                audioVoice.PlayOneShot(sound8000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 9000:
                audioVoice.PlayOneShot(sound9000);
                audioBoom.PlayOneShot(soundOther);
                break;
            case 10000:
                audioVoice.PlayOneShot(sound10000);
                audioBoom.PlayOneShot(soundOther);
                return;
            default:
                return;
        }
        controller.speedMoveBlock *= speedStepUp;
        speedCounter.text = String.Format("{0:0.00}", controller.speedMoveBlock);
        controller.delayBetweenAddNewBlockLimit = new Vector2(
            controller.delayBetweenAddNewBlockLimit.x / speedStepUp, 
            controller.delayBetweenAddNewBlockLimit.y / speedStepUp);
    }

    private void Update() {
        /*if (listBlock.Count > 0)
            //disappearance();
            delOldBlock();*/
    }

    private void delOldBlock() {
        List <GameObject> delList = new List<GameObject>();
        foreach (GameObject go in listBlock) {
            if (go.transform.position.z < -11) {
                delList.Add(go);
            }
        }

        foreach(GameObject go in delList) {
            listBlock.Remove(go);
            controller.delBlockInList(go);
            Destroy(go);
        }
    }

    private void disappearance() {
        List<GameObject> delList = new List<GameObject>();
        //перебираем ГО в списке
        foreach (GameObject go in listBlock) {
            //Получаем Transform всем дочерних го
            Transform[]  childTransform = go.GetComponentsInChildren<Transform>();
            //Перебираем Дочерние Transform
            foreach (Transform transform in childTransform) {
                
                //Список Мэшей в каждом дочернем объекте
                MeshRenderer[] meshes = transform.gameObject.GetComponentsInChildren<MeshRenderer>();

                foreach (MeshRenderer mesh in meshes) {
                    mesh.material.color = new Color(
                        mesh.material.color.r,
                        mesh.material.color.g,
                        mesh.material.color.b,
                        mesh.material.color.a - speedDisappearance * Time.deltaTime);
                    if (mesh.material.color.a <= 0) {
                        delList.Add(go);
                    }
                }
            }
        }

        if (delList.Count > 0) {
            foreach (GameObject block in delList) {
                controller.delBlockInList(block);
                listBlock.Remove(block);
                Destroy(block);
            }
        }

    }
}
