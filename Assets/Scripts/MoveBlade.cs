using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBlade : MonoBehaviour
{
    public float speed = 1.0f;
    public AudioClip clip;
    AudioSource audio;
    void FixedUpdate() {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
