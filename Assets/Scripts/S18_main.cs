using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S18_main : MonoBehaviour
{
    public Transform prefab1;
    public Transform prefab2;
    public Transform prefab3;
    public Transform prefab4;
    public Transform prefab5;
    public Transform prefab10;
    public Transform prefab100;
    public Transform prefab200;
    public Transform prefab300;
    public Transform prefab400;
    public Transform prefab500;
    public Transform prefab1000;

    private List<Transform> prefabList;

    public AudioClip clip1;
    public AudioClip clip2;
    public AudioClip clip3;
    public AudioClip clip4;
    public AudioClip clip5;
    public AudioClip clip10;
    public AudioClip clip100;
    public AudioClip clip200;
    public AudioClip clip300;
    public AudioClip clip400;
    public AudioClip clip500;
    public AudioClip clip1000;
    public AudioClip mainClip;
    private AudioSource audio;
    private AudioSource audioMainTheme;

    private List<AudioClip> clipList;

    private GameObject oldBlock;
    private GameObject newBlock;
    private int counter = 0;

    private void Start() {
        createPrefabList();
        createClipList();
        setInActive();

        audio = gameObject.AddComponent<AudioSource>();
        audioMainTheme = gameObject.AddComponent<AudioSource>();
        audioMainTheme.PlayOneShot(mainClip);
        audioMainTheme.volume = 0.3f;
        audioMainTheme.loop = true;
    }

    public void setActive(GameObject block) {
        //print("block: " + block + " prefabList[counter].gameObject " + prefabList[counter].gameObject);
        if (block == prefabList[counter].gameObject) {
            print(block);
            MeshRenderer[] renderers = block.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer render in renderers) {
                render.enabled = true;
            }
        }
    }

    public void playSound(GameObject block) {
        if (block == prefabList[counter].gameObject) {
            audio.PlayOneShot(clipList[counter]);
            counter++;
        }
    }

    public void setInActive (GameObject block) {
        block.SetActive(false);
    }

    private void createPrefabList() {
        prefabList = new List<Transform>() {
            prefab1,
            prefab2,
            prefab3,
            prefab4,
            prefab5,
            prefab10,
            prefab100,
            prefab200,
            prefab300,
            prefab400,
            prefab500,
            prefab1000
        };
    }

    private void createClipList() {
        clipList = new List<AudioClip>() {
            clip1,
            clip2,
            clip3,
            clip4,
            clip5,
            clip10,
            clip100,
            clip200,
            clip300,
            clip400,
            clip500,
            clip1000
        };
    }
    private void setInActive() {
        foreach (Transform prefab in prefabList) {
            MeshRenderer[] renderers = prefab.gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer render in renderers) {
                render.enabled = false;
            }
            //print("GO " + prefab.gameObject + " active: " + prefab.gameObject.activeSelf);
        }
    }
}
