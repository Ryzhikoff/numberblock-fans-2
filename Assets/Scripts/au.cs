using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class au : MonoBehaviour
{
    public AudioClip clipOne;
    public AudioClip clipHundreg;
    public AudioClip clipThousand;
    public AudioClip clipMillion;
    public AudioClip clipBillion;
    public AudioClip clipTrillion;

    private AudioSource audio;
    private int inrcSound = 0;

    public float delay = 5.0f;
    private Vector3[] coords = {
        //one hundred
        new Vector3(28.08f,13.89f,-10.11f),
        //one thusand
        new Vector3(52.75f,12.35f,-8.44f),
        //one million
        new Vector3(201.2f,112.2f,-67.3f),
        //one billion
        new Vector3(1567f,1319f,-851f),
        //one trillion
        new Vector3(17557f,11839f,-6781f)};

    private Vector3 pos1 = new Vector3(5764, 1644, -16434);
    private Vector3 pos2 = new Vector3(0,3,-10);
    private Vector3 pos;
    private bool forCheck = true;
    private bool finalMove = false;

    public int interpolationFramesCount = 1000;
    public int interpolationFramesFinal = 1000;
    int elapsedFrames = 0;
    private bool cameraMove = false;
    private bool[] listBool = new bool[5];
   

    void Start()
    {
        pos = pos1;
        audio = gameObject.AddComponent<AudioSource>();
        Invoke("playVoice", 2);
        Invoke("cameraStart", delay);
    }

    private void cameraStart() {
        cameraMove = true;
    }

    private void playVoice() {
        switch (inrcSound) {
            case 0: 
                audio.PlayOneShot(clipOne);
                break;
            case 1: 
                audio.PlayOneShot(clipHundreg);
                break;
            case 2: 
                audio.PlayOneShot(clipThousand);
                break;
            case 3:
                audio.PlayOneShot(clipMillion);
                break;
            case 4:
                audio.PlayOneShot(clipBillion);
                break;
            case 5:
                audio.PlayOneShot(clipTrillion);
                break;
            default:
                audio.PlayOneShot(clipOne);
                break;
        }
        inrcSound++;
    }
   
    void Update()
    {
        if (cameraMove) {
            for (int i = 0; i < listBool.Length; i++) {
                if (listBool[i]) continue;

                float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
                Vector3 interpolatedPosition = Vector3.Lerp(transform.position, coords[i], interpolationRatio);
                elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
                transform.position = interpolatedPosition;

                if (coords[i].x - transform.position.x < 0.05) {
                    cameraMove = false;
                    elapsedFrames = 0;
                    listBool[i] = true;
                    playVoice();
                    if (inrcSound < 6) {
                        Invoke("cameraStart", delay);
                    } else {
                        Invoke("cameraFinalMove", delay);
                    }
                }
                return;
            }
        }

        if (finalMove) {
            float interpolationRatio = (float)elapsedFrames / interpolationFramesFinal;
            Vector3 interpolatedPosition = Vector3.Lerp(transform.position, pos, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesFinal + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;

            if (Mathf.Abs(pos.z) - Mathf.Abs(transform.position.z) < 0.05) {
                if (forCheck) {
                    pos = pos2;
                    elapsedFrames = 0;
                    forCheck = false;
                }
            }
        }
    }

    private void cameraFinalMove() {
        finalMove = true;
        transform.Rotate(-30, 0, 0);
        transform.Rotate(0, 30, 0);
    }
}
