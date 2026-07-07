using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S38_RandomStartAnimation : MonoBehaviour
{
    public Animator animator;
    private AudioSource audio;

    private void Start() {
        audio = gameObject.GetComponent<AudioSource>();
        animator.enabled = false;
        var delay = Random.Range(0.1f, 1.3f);
        Invoke("startAnimator", delay);
    }
    
    private void startAnimator() {
        animator.enabled = true;
        //audio.Play();
    }
}
