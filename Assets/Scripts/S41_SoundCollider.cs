using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S41_SoundCollider : MonoBehaviour
{
    private AudioSource audio;
    public AudioClip sound;
    public float volume;

    private void Start() {
        audio = gameObject.AddComponent<AudioSource>();
        audio.volume = volume;
    }

    private void OnCollisionEnter(Collision collision) {
        audio.PlayOneShot(sound);
    }
}
