using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S25_Minecart : MonoBehaviour {


    private S25_Main main;
    private float speed;

    public void onStart(S25_Main main, float speed) {
        this.main = main;
        this.speed = speed;
    }

    private void Update() {
        moveMinecart();
    }

    private void moveMinecart() {
        transform.position = new Vector3(
            transform.position.x - speed * Time.deltaTime,
            transform.position.y,
            transform.position.z);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag.Equals("Block")) {
            main.arrived();
            Destroy(gameObject);
        }
    }
}
