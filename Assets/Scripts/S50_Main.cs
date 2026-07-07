using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S50_Main : MonoBehaviour
{
    public GameObject boat;
    public GameObject usualOne;
    public GameObject sadOne;
    public GameObject ship;

    public float speedBoat = 5f;
    public Vector3 boatStartPosition;
    public Vector3 boatStopPosition;
    public float positionForChangeFace;

    private bool isBoatMove = true;
    private bool isChangedFace = false;

    private bool isShipMove = true;
    public Vector3 shipStartPosition;
    public Vector3 shipStopPosition;
    public float shipSpeed = 5;

    private void Start() {
        boat.transform.position = boatStartPosition;
        ship.transform.position = shipStartPosition;
    }

    private void Update() {
        if (isBoatMove) {
            moveBoat();
        }

        if (isShipMove)
            moveShip();
    }
    private void moveBoat() {
        boat.transform.position += Vector3.left * (speedBoat * Time.deltaTime);
        if (boat.transform.position.x < positionForChangeFace && !isChangedFace) {
            changeFace();
        }
        if (boat.transform.position.z < boatStopPosition.z) {
            isBoatMove = false;
        }
    }

    private void changeFace() {
        usualOne.SetActive(false);
        sadOne.SetActive(true);

        isChangedFace = true;
    }

    private void moveShip() {
        ship.transform.position += Vector3.left * (shipSpeed * Time.deltaTime);

        if (ship.transform.position.x > shipStopPosition.x)
            isShipMove = false;
    }
}
