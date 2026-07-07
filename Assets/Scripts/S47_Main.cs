using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.GraphicsBuffer;

public class S47_Main : MonoBehaviour
{
    public GameObject blockOne;
    public Camera camera;
    public float speedRotate = 5f;

    private float deltaY = 0f;
    private float oldPositionYBlockOne = 0f;

    private void Start() {
        oldPositionYBlockOne = blockOne.transform.position.y;
    }


    private void Update() {
        updateDeltaY();
        cameraRotate();
        cameraMoveToBlockOne();
    }

    private void updateDeltaY() {
        deltaY = oldPositionYBlockOne - blockOne.transform.position.y;
        oldPositionYBlockOne = blockOne.transform.position.y;
    }

    private void cameraRotate() {
        camera.transform.RotateAround(blockOne.transform.position, Vector3.up, speedRotate * Time.deltaTime);
    }

    private void cameraMoveToBlockOne() {
        camera.transform.position += Vector3.down * deltaY;
    }
}
