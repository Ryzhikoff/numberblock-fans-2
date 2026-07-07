using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S55_Road : MonoBehaviour
{
    public GameObject ball;

    private void Update() {
        transform.position = new Vector3(transform.position.x, transform.position.y, ball.transform.position.z);
    }
}
