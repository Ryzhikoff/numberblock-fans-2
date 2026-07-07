using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S56_RollingBall : MonoBehaviour
{
    public float speed = 5f; // Скорость движения
    public Vector3 direction = Vector3.forward; // Направление движения
    public GameObject prefabOne;
    private Rigidbody rb;

    private bool needCameraMove = false;
    private Vector3 cameraStartPosition;
    private Vector3 cameraEndPosition;
    public int cameraMoveDistance = 20;
    private int counter = 1;
    public int interpolationFramesCount = 300;
    int elapsedFrames = 0;

    public AudioClip clip;
    public float volume = 0.5f;
    private AudioSource audio;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = direction.normalized * speed;
        audio = gameObject.AddComponent<AudioSource>();
        audio.volume = volume;
        
    }

    void FixedUpdate() {
        // Поддерживаем постоянную скорость
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnCollisionEnter(Collision other) {
        if (rb != null && other.gameObject.CompareTag("Block")) {
            var scale = other.gameObject.GetComponent<Scale>();
            if (scale != null && scale.number != 1) {
                createBlocks(other.gameObject.transform.position, scale);
                Destroy(other.gameObject);
                audio.PlayOneShot(clip, volume);
                startCameraMove();
            }
        }
    }

    private void startCameraMove() {
        needCameraMove = true;
        cameraStartPosition = Camera.main.transform.position;
        cameraEndPosition = new Vector3(
            Camera.main.transform.position.x, 
            Camera.main.transform.position.y,
            Camera.main.transform.position.z + cameraMoveDistance);
        counter++;
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

    private void Update() {
        if (needCameraMove) {
            float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
            Vector3 interpolatedPosition = Vector3.Lerp(cameraStartPosition, cameraEndPosition, interpolationRatio);
            elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
            Camera.main.transform.position = interpolatedPosition;
            if ( Mathf.Abs(Camera.main.transform.position.z - cameraEndPosition.z) < 0.05 ) {
                //Camera.main.transform.position = cameraEndPosition;
                needCameraMove = false;
            }
        }
    }

    //void OnCollisionEnter(Collision collision) {
    //    Rigidbody otherRb = collision.rigidbody;

    //    if (otherRb != null) {
    //        // Отталкиваем объект от шара
    //        Vector3 pushDirection = collision.transform.position - transform.position;
    //        pushDirection.y = 0; // Убираем вертикальную составляющую, если не хотим подбрасывания

    //        otherRb.AddForce(pushDirection.normalized * 10f, ForceMode.Impulse);
    //    }
    //}
}
