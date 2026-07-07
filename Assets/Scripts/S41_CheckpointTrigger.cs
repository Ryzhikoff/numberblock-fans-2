using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class S41_CheckpointTrigger : MonoBehaviour
{
    private List<GameObject> usedBlock = new();
    public S41_Main controller;
    public GameObject locked;
    private bool isFirst = true;
    private AudioSource audio;
    public AudioClip sound;
    public float volume;

    private void Start() {
        audio = gameObject.AddComponent<AudioSource>();
        audio.volume = volume;
    }

    private void OnTriggerEnter(Collider other) {
        if (!usedBlock.Contains(other.gameObject)) {
            usedBlock.Add(other.gameObject);
            if (isFirst) {
                audio.PlayOneShot(sound);
                isFirst = false;
                controller.checkInBlock(other.gameObject);
            }
        } 
        if (usedBlock.Count == controller.blocks.Count) {
            locked.SetActive(false);
        }
    }
}
