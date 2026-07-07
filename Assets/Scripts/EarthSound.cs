using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthSound : MonoBehaviour
{
    private static int _soundCounter = 0;

    public static int soundCounter  {
       get {
            return _soundCounter;
        }
        set {
            _soundCounter = value;
        }
    }
        
    public AudioClip clipOne;
    public AudioClip clipTen;
    public AudioClip clipHundred;
    public AudioClip clipThousand;
    public AudioClip clipTenThousand;
    public AudioClip clipHundredThousand;
    public AudioClip clipMillion;
    public AudioClip clipTenMillion;
    public AudioClip clipHundredMillion;
    public AudioClip clipBillion;
    public AudioClip clipTenBillion;
    public AudioClip clipHundredBillion;
    public AudioClip clipTrillion;

    public float volumeClip;

    public AudioClip clipApplause;
    public float volumeApplause;
    public AudioClip mainTheme;
    public float volumeMainTheme;

    public static AudioSource audioMain;
    private AudioSource audioApplause;
    public static AudioSource audioClip;

    public static List<AudioClip> clipList;

    private void Start() {
        audioMain = gameObject.AddComponent<AudioSource>();
        audioApplause = gameObject.AddComponent<AudioSource>();
        audioClip = gameObject.AddComponent<AudioSource>();
        createClipList();

        audioMain.clip = mainTheme;
        audioMain.volume = volumeMainTheme;
        audioMain.Play();

    }

    private void createClipList() {
        clipList = new List<AudioClip> {
            clipOne,
            clipTen,
            clipHundred,
            clipThousand,
            clipTenThousand,
            clipHundredThousand,
            clipMillion,
            clipTenMillion,
            clipHundredMillion,
            clipBillion,
            clipTenBillion,
            clipHundredBillion,
            clipTrillion
        };
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Block") {
            audioMain.Pause();
            audioClip.PlayOneShot(clipList[soundCounter]);
            //audioApplause.PlayOneShot(clipApplause);
            soundCounter++;
        }
    }

    private void Update() {
        if (!audioMain.isPlaying) {
            if (!audioClip.isPlaying) {
                audioMain.Play();
            }
        }
    }
}
