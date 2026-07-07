using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S63_Main : MonoBehaviour
{
    public int value = 3;
    public TextMeshProUGUI textMesh;
    public TextMeshProUGUI textMesh2;
    public GameObject barrier;

    public AudioClip startClip;
    public float volumeStart = 1.0f;

    public AudioClip afterClip;
    public float volumeAfter = 1.0f;

    private AudioSource audioStart;
    private AudioSource audioAfter;

    void Start()
    {
        audioStart = gameObject.AddComponent<AudioSource>();
        audioStart.volume = volumeStart;
        audioAfter = gameObject.AddComponent<AudioSource>();
        audioAfter.volume = volumeAfter;

        audioStart.PlayOneShot(startClip);

        textMesh.text = value.ToString();
        Invoke(nameof(Count), 1f);
    }

    private void Count() {
        value--;
        if (value == 0) {
            textMesh.text = "";
            barrier.SetActive(false);
            textMesh2.text = "";
            audioAfter.PlayOneShot(afterClip);
        } else {
            textMesh.text = value.ToString();
            Invoke(nameof(Count), 1f);
        }
    }
}
