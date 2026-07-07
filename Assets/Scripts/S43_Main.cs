using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S43_Main : MonoBehaviour
{
    public List<GameObject> blocks;
    public Vector3 startBlockPosition = Vector3.zero;
    public float delayBetweenBlock = 5;
    public GameObject blockOne;
    public float yForTransform;
    public TextMeshProUGUI textMesh;
    public TextMeshProUGUI textMesh2;

    public AudioClip soundFailDown;
    public AudioClip soundExplosion;
    public AudioClip soundBoom;
    private AudioSource audioSource;

    private Camera camera;

    private GameObject currentBlock;
    private int counter = 0;

    public int speedRotate = 5;
    public int sign = 1;
    public GameObject targetOfRotate;

    private void Start() {
        camera = Camera.main;
        audioSource = gameObject.AddComponent<AudioSource>();
        createBlock();
    }

    private void createBlock() {
        if (counter >= blocks.Count)
            return;
        GameObject block = Instantiate(blocks[counter]);
        var scale = block.GetComponent<Scale>();

        block.transform.position = new Vector3(-(scale.x / 2), startBlockPosition.y, -(scale.z / 2));
        print(block.transform.position);
        currentBlock = block;
        audioSource.Stop();
        audioSource.PlayOneShot(soundFailDown);
        textMesh.text = currentBlock.GetComponent<Scale>().number.ToString();
        textMesh2.text = currentBlock.GetComponent<Scale>().number.ToString();
    }

    private void Update() {
        if (currentBlock  != null && currentBlock.transform.position.y <= yForTransform) {
            transformBlock();
        }

        if (currentBlock != null) {
            camera.transform.LookAt(currentBlock.transform.position);
        }
        camera.transform.RotateAround(targetOfRotate.transform.position, Vector3.up, speedRotate * Time.deltaTime * sign);
    }

    private void transformBlock() {
        var scale = currentBlock.GetComponent<Scale>();
        for (int x = 0; x < scale.x; x++) {
            for (int y = 0; y < scale.y; y++) {
                for(int z = 0; z < scale.z; z++) {
                    GameObject block = Instantiate(blockOne);
                    block.transform.position = new Vector3(x, y, z) + currentBlock.transform.position;
                }
            }
        }
        audioSource.Stop();
        audioSource.PlayOneShot(soundExplosion);
        audioSource.PlayOneShot(soundBoom);
        Destroy(currentBlock);
        currentBlock = null;
        counter++;
        Invoke("createBlock", delayBetweenBlock);
    }
}
