using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class S25_Main : MonoBehaviour
{
    public List<GameObject> blocks;
    public Vector3 startPositionMineCart;
    public Vector3 endPositionMineCart;
    public float speedMinecart = 5;
    private bool needMove = false;

    public Vector3 startPositionFirstBlock;

    public GameObject prefabMineCart;
    public GameObject prefabOne;
    public GameObject prefabOneHundred;
    public GameObject blockHolder;
    private int count = 0;

    private GameObject minecart;

    private Camera camera;

    public AudioClip clipMinecart;
    public AudioClip clipPoral;
    private AudioSource mediaMinecart;
    private AudioSource mediaPortal;
    public float volumeMinecart = 0.5f;
    public float volumePortal = 0.5f;

    public TextMeshProUGUI textMeshPro;

    /*private bool[,,] matrix = new bool[10,10,10];*/
    private bool[,] matrix = new bool[10,10];

    private void Start() {
        camera = Camera.main;
        mediaMinecart = gameObject.AddComponent<AudioSource>();
        mediaMinecart.volume = volumeMinecart;
        mediaMinecart.clip = clipMinecart;
        mediaMinecart.loop = true;
        mediaMinecart.Play();

        mediaPortal = gameObject.AddComponent<AudioSource>();
        mediaPortal.volume = volumePortal;


        addNewMineCart();
    }

    private void addNewMineCart() {
        minecart = Instantiate(prefabMineCart);
        minecart.transform.position = startPositionMineCart;
        needMove = true;
        mediaPortal.PlayOneShot(clipPoral);
    }

    private void removeMineCart() {
        GameObject.Destroy(minecart);
    }

    private void Update() {
        if (needMove) {
            moveMinecart();
        }
    }

    private void moveMinecart() {
        if(minecart != null) {
            minecart.transform.position += Vector3.left * (speedMinecart * Time.deltaTime);

            if(minecart.transform.position.x <= endPositionMineCart.x) {
                needMove = false;
                removeMineCart();
                arrived();
            }
        }
    }

    public void arrived() {
        count++;
        if (setBlockInMatix()) {
            addNewMineCart();
        } else {
            mediaMinecart.Stop();
            GameObject.Destroy(blockHolder);
            prefabOneHundred.SetActive(true);
        }
        if (count < 101) {
            textMeshPro.text = count.ToString();
        }
    }

    private bool setBlockInMatix() {
        for (int y = 0; y < 10; y++) {
            /*for (int z = 0; z < 10; z++) {*/
                for (int x = 0; x < 10; x++) {
/*                    print(matrix[x, y, z]);
                    print(x + " " + y + " " + z);*/
                    if (matrix[x, y]) continue;
                   
                    matrix[x, y] = true;

                    GameObject block = Instantiate<GameObject>(prefabOne);
                    block.transform.position = startPositionFirstBlock + new Vector3(x * -1, y, 0);
                    block.transform.parent = blockHolder.transform;

                    return true;
                }
            /*}*/
        }
        return false;
    }

}