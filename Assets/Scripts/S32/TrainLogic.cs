using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainLogic : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float speed = 5f;

    private void Start() {
        transform.position = startPosition;   
    }

    private void Update() {
        if (transform.position.z > endPosition.z) {
            MoveTrain();
        } else {
            transform.position = startPosition;
        }
    }

    private void MoveTrain() {
        float delta = speed * Time.deltaTime;
        transform.position = new Vector3(
             transform.position.x,
             transform.position.y,
             transform.position.z - delta);
    }

}
