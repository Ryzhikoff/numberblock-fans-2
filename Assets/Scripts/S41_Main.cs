using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class S41_Main : MonoBehaviour
{
    private Camera camera;
    public float speedCameraMove = 0.1f;
    public GameObject startLocked;
    public List<GameObject> blocks;
    public GameObject background;
    public List<GameObject> backAndBorders;
    public GameObject panel;
    public TextMeshProUGUI prefabTextCounter;

    public TextMeshProUGUI countdownTextMesh;
    public int timeCountdown = 5;
    private AudioSource audioTimer;
    public AudioClip soundTimer;
    public float volumeTimer;
    public AudioClip soundStart;

    public AudioClip soundBackground;
    public float volumeBackground = 0.5f;


    private void Start() {
        camera = Camera.main;
        audioTimer = gameObject.AddComponent<AudioSource>();
        audioTimer.volume = volumeTimer;
        countDownTimer();
        //createTextCounters();
    }

    private void countDownTimer() {
        if (timeCountdown > 0) {
            countdownTextMesh.text = timeCountdown.ToString();
            timeCountdown--;
            audioTimer.PlayOneShot(soundTimer);
            Invoke("countDownTimer", 1f);
        } else {
            countdownTextMesh.text = "GO!!!";
            audioTimer.PlayOneShot(soundStart);
            Invoke("clearText", 1.5f);
            createTextCounters();
            startLocked.SetActive(false);
        }
    }

    private void clearText() {
        countdownTextMesh.text = "";
        audioTimer.volume = volumeBackground;
        audioTimer.PlayOneShot(soundBackground);
    }

    private void createTextCounters() {
        foreach (GameObject block in blocks) {
            TextMeshProUGUI text = Instantiate<TextMeshProUGUI>(prefabTextCounter);
            text.name = block.name;
            text.text = "Block " + block.name + ": 0";
            text.transform.parent = panel.transform;
        }
    }

    private void FixedUpdate() {
        cameraMove();
        moveBackAndBorders();
    }

    private void moveBackAndBorders() {
        foreach (GameObject go in backAndBorders) {
            go.transform.position = new Vector3(
                go.transform.position.x,
                camera.transform.position.y,
                go.transform.position.z);
        }
    }

    private void cameraMove() {
        Vector3 newPos = new Vector3(
            camera.transform.position.x,
            getMinimumPositionOfBlocksByY(),
            camera.transform.position.z);
        camera.transform.position = Vector3.Lerp(
            camera.transform.position,
            newPos,
            speedCameraMove);
    }

    private float getMinimumPositionOfBlocksByY() {
        float min = 100;
        foreach(GameObject block in blocks) {
            if (block.transform.position.y < min) {
                min = block.transform.position.y;
            }
        }
        return min;
    }

    private float getAvaragePositionOfBlocksByY() {
        List<float> positions = new List<float>();
        foreach (GameObject block in blocks) {
            positions.Add(block.transform.position.y);
        }
        float sum = 0;
        foreach(float y in positions) {
            sum += y;
        }
        float avarage = sum / positions.Count;
        return avarage;
    }

    public void checkInBlock(GameObject block) {
        addScoreToBlock(block.name);
    }

    private void addScoreToBlock(string blockName) {
        Transform[] transforms = panel.GetComponentsInChildren<Transform>();
        foreach (Transform trans in transforms) {
            if (trans.gameObject.name.Equals(blockName)) {
                TextMeshProUGUI textMesh = trans.gameObject.GetComponent<TextMeshProUGUI>();
                string text = textMesh.text;
                int oldScore = int.Parse(text.Split(" ")[2]);
                textMesh.text = "Block " + blockName + ": " + ++oldScore;
                return;
            }
        }
    }
}
