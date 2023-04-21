using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthSound1 : MonoBehaviour
{

    public GameObject controller;
    private SoundMaster soundMaster;

    private void Start() {
        soundMaster = controller.GetComponent<SoundMaster>();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Block") {
            soundMaster.playSound();
            /*EarthSound.audioMain.Pause();
            EarthSound.audioClip.PlayOneShot(EarthSound.clipList[EarthSound.soundCounter]);
            //audioApplause.PlayOneShot(clipApplause);
            EarthSound.soundCounter++;*/
        }
    }

}
