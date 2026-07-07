using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class SoundData {
    public int counter;
    public long number;
    public AudioClip clip;
}

public class CameraMoveByList : MonoBehaviour {

    public Vector3 startPosition = Vector3.zero;
    public List<Vector3> listPosition;
    public int interpolationFramesCount = 300;

    public List<SoundData> soundDatas = new();
    private bool needMove = false;
    private int elapsedFrames = 0;
    private Vector3 targetPosition;
    private Vector3 currentPosition;
    private int counter = 0;
    public float delayAfterMove = 3f;
    public TextMeshProUGUI textMesh;
    public float soundVolume = 1.0f;
    private AudioSource audio;

    // Start is called before the first frame update
    void Start() {
        transform.position = startPosition;
        prepareMove();
        audio = gameObject.AddComponent<AudioSource>();
        audio.volume = soundVolume;
    }

    private void prepareMove() {
        if (counter >= listPosition.Count) { return; }   
        targetPosition = listPosition[counter];
        currentPosition = transform.position;
        needMove = true;
    }

    private void Update() {
        if (needMove) {
            float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(currentPosition, targetPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            transform.position = interpolatedPosition;
            if (elapsedFrames >= interpolationFramesCount) {
                needMove = false;
                elapsedFrames = 0;
                PlaySoundAndSetText();
                counter++;
                Invoke(nameof(prepareMove), delayAfterMove);
            }
        }
    }

    private void PlaySoundAndSetText() {
        foreach (var data in soundDatas) {
            if (counter == data.counter) {
                audio.PlayOneShot(data.clip);
                textMesh.text = data.number.ToString("#,0");
            }
        }
    }
}
