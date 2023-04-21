using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManyOne : MonoBehaviour
{
    public GameObject onePrefab;
    public GameObject oneThousandPrefab;

    public float startRange = 10;
    public float endRange = 20;
    public int interpolationFramesCount = 300;
    public AudioClip collisionSound;
    int elapsedFrames = 0;
    private List<Vector3> coords;

    GameObject tempCube;
    Vector3 tempPosStart;
    Vector3 tempPosFinish;
    List<GameObject> listsCubes;

    private bool move = false;

    private Text countText;
    private int count = 0;
    private AudioSource audio;


    private void Start() {
        audio = gameObject.AddComponent<AudioSource>();
        audio.volume = 0.4f;
        coords = new List<Vector3>();
        listsCubes = new List<GameObject>();
        GameObject goText = GameObject.Find("CountBlock");
        countText = goText.GetComponent<Text>();
        createArrayCoord();
        addNewBlock();
        Invoke("changeTextColor", 0.3f);
    }

    private void addNewBlock() {
        if (coords.Count > 0) {
            GameObject go = Instantiate(onePrefab);
            listsCubes.Add(go);
            tempCube = go;
            tempPosStart = getRange();
            count++;
            tempCube.transform.position = tempPosStart;
            tempPosFinish = coords[Random.Range(0,coords.Count)];
            coords.Remove(tempPosFinish);
            move = true;
        } else {
            GameObject[] list = GameObject.FindGameObjectsWithTag("BlockOne");
            foreach (GameObject g in list) {
                Destroy(g);
            }
            GameObject oneThousand = Instantiate(oneThousandPrefab);
            oneThousand.transform.position = new Vector3(4.5f, 4.5f, -0.5f);
        }
    }

    private void Update() {
        if (count != 0) {
            countText.text = count.ToString();
        }
    }

    private void changeTextColor() {
        countText.color = new Color(Random.value, Random.value, Random.value);
        Invoke("changeTextColor", 0.3f);
    }

    private void FixedUpdate() {
        //Плавное двигаем блок
        if (move) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(tempPosStart, tempPosFinish, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            tempCube.transform.position = interpolatedPosition;

            if (Mathf.Abs(tempCube.transform.position.x) - Mathf.Abs(tempPosFinish.x) < 1f &&
                Mathf.Abs(tempCube.transform.position.y) - Mathf.Abs(tempPosFinish.y) < 1f &&
                Mathf.Abs(tempCube.transform.position.z) - Mathf.Abs(tempPosFinish.z) < 1f) {
                tempCube.transform.position = tempPosFinish;
                audio.PlayOneShot(collisionSound);
                elapsedFrames = 0;
                move = false;
                addNewBlock();
            }
        }
    }

    private void createArrayCoord() {
        for (int x = 0; x < 10; x++) {
            for (int y = 0; y < 10; y++) {
                for (int z = 0; z < 10; z++) {
                    coords.Add(new Vector3(x, y, z));
                }
            }
        }
    }
    private Vector3 getRange() {
        float[] list = new float[3];

        for (int i = 0; i < 3; i++) {
            if ( i == 2) {
                if ((list[0] < startRange && list[0] > -startRange) && (list[1] < startRange && list[1] > -startRange)) {
                    list[2] = Random.Range(startRange, endRange);
                } else {
                    list[2] = Random.Range(-endRange, endRange);
                }
            } else {
                list[i] = Random.Range(-endRange, endRange);
            }
        }
        
        return new Vector3(list[0], list[1], list[2]);  
    }
}
