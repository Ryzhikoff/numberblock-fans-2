using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class SoundInfo {
    public int number;
    internal bool passed = false;
    public AudioClip clip;
}

public class S20_DeleteTrigger : MonoBehaviour {
    public GameObject controllerGO;
    private S20_Main controller;
    public float speedDisappearance = 30;
    public float speedStepUp = 1.2f;
    private List<GameObject> listBlock = new List<GameObject>();
    private int counter = 0;
    public TextMeshProUGUI textCounter;
    public TextMeshProUGUI speedCounter;
    public AudioClip soundCollider;
   
    public float volumeSoundCoin = 0.7f;
    public float volumeSoundVoice = 0.7f;
    public float volumeSoundBoom = 0.1f;
    private AudioSource audioCoin;
    private AudioSource audioVoice;
    private AudioSource audioBoom;

    public List<SoundInfo> soundInfos;

    public LayerMask blockLayerMask; // укажите слой блоков
    public Vector3 triggerBoxSize = new Vector3(1f, 1f, 1f); // подберите размер зоны триггера
    private HashSet<GameObject> processedBlocks = new HashSet<GameObject>();

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

    //private void OnTriggerEnter(Collider other) {
    //    if (other.tag == "Block") {
    //        //audioCoin.PlayOneShot(soundCollider);
    //        //listBlock.Add(other.gameObject);
    //        textCounter.text = counter.ToString();
    //        playSound();

    //        controller.delBlockInList(other.gameObject);
    //        Destroy(other.gameObject);

    //        if (counter == 10000) {
    //            controller.isGoing = false;
    //            clearAllBlock();
    //        }
    //        counter++;
    //    }
    //}

    private void OnTriggerEnter(Collider other) {
        // Флаг processedBlocks предотвращает двойную обработку
        if (other.CompareTag("Block") && !processedBlocks.Contains(other.gameObject)) {
            HandleBlock(other.gameObject);
            processedBlocks.Add(other.gameObject);
        }
    }

    private void FixedUpdate() {
        Collider[] overlapped = Physics.OverlapBox(transform.position, triggerBoxSize / 2f, Quaternion.identity, blockLayerMask);
        foreach (Collider collider in overlapped) {
            GameObject block = collider.gameObject;
            if (!processedBlocks.Contains(block)) {
                HandleBlock(block);
                processedBlocks.Add(block);
            }
        }
    }

    private void HandleBlock(GameObject block) {
        // Полная имитация логики внутри OnTriggerEnter для блока
        //audioCoin.PlayOneShot(soundCollider);
        //listBlock.Add(block);
        Scale scale = block.GetComponent<Scale>();
        counter += (int) scale.number;
        controller.UpdateCounter(counter);
        textCounter.text = counter.ToString();
        preparePlaySound();

        controller.delBlockInList(block);
        Destroy(block);

        if (counter >= 100_000) {
            controller.isGoing = false;
            clearAllBlock();
        }
    }

    private void clearAllBlock() {
        GameObject[] gList = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject go in gList) {
            Destroy(go);
        }
    }

    private void preparePlaySound() {
        foreach (var info in soundInfos) {
            if (counter >= info.number && !info.passed) {
                playSound(info.clip);
                info.passed = true;
                return;
            }
        }
    }

    private void playSound(AudioClip clip) {
        
        audioVoice.PlayOneShot(clip);
        //audioBoom.PlayOneShot(soundOther);
                
        controller.speedMoveBlock *= speedStepUp;
        speedCounter.text = String.Format("{0:0.00}", controller.speedMoveBlock);
        controller.delayBetweenAddNewBlockLimit = new Vector2(
            controller.delayBetweenAddNewBlockLimit.x / speedStepUp, 
            controller.delayBetweenAddNewBlockLimit.y / speedStepUp);
    }

    private void Update() {
        if (listBlock.Count > 0)
            disappearance();
        delOldBlock();
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
