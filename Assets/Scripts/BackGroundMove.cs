using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMove : MonoBehaviour
{
    public GameObject oneCube;
    public float Z = 2.5f;
    void Start() {
        oneCube = GameObject.Find("NumberOne");
        transform.position = new Vector3(oneCube.transform.position.x, oneCube.transform.position.y, Z);
    }

    // Update is called once per frame
    void Update() {
        transform.position = new Vector3(oneCube.transform.position.x, oneCube.transform.position.y, Z);
    }
}
