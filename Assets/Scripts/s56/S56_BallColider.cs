using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class S56_BallColider : MonoBehaviour
{
    public GameObject prefabOne;
    private new Rigidbody rigidbody;

    private bool needCameraMove;
    public AudioClip clip;
    public float volume = 0.5f;
    private AudioSource audio;


    private void Start() {
        rigidbody = GetComponent<Rigidbody>();
        audio = gameObject.AddComponent<AudioSource>();
        audio.volume = volume;
    }

    private void OnCollisionEnter(Collision other) {
        if (rigidbody != null && other.gameObject.CompareTag("Block")) {
            var scale = other.gameObject.GetComponent<Scale>();
            if (scale != null && scale.number != 1) {
                createBlocks(other.gameObject.transform.position, scale);
                audio.PlayOneShot(clip);
                Destroy(other.gameObject);
            }
        }
    }

    private void createBlocks(Vector3 startPosition, Scale scale) {
        for (int x = 0; x < scale.x; x++) {
            for (int y = 0; y < scale.y; y++) {
                for (int z = 0; z < scale.z; z++) {
                    var block = Instantiate(prefabOne);
                    block.transform.position = startPosition + new Vector3(x, y, z);
                }
            }
        }
    }
}
