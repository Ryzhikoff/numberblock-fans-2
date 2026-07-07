using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S66_Main : MonoBehaviour
{
    public List<GameObject> units = new();
    public List<GameObject> tens = new();
    public List<GameObject> handreds = new();

    public Vector3 startCamPos = Vector3.zero;
    public Vector3 endCamPos = Vector3.zero;

    private Camera camera;
    private bool needCameraMove;
    public int interpolationFramesCount = 300;
    private int elapsedFrames = 0;

    private Vector3 pos = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        BuildBlocks();
    }

    private void BuildBlocks() {
        foreach (GameObject pref in units) {
            var block = Instantiate(pref);
            block.transform.position = pos;
            pos += Vector3.right;
        }

        foreach (GameObject prefH in tens) { 
            foreach (GameObject prefU in units) {
                var block = Instantiate(prefH);
                block.transform.position = pos;
                var y = block.GetComponent<Scale>().y;

                var uBlock = Instantiate(prefU);
                uBlock.transform.position = pos + Vector3.up * y;

                pos += Vector3.right;
            }
        }

        var pr = handreds[0];
        var bl = Instantiate(pr);
        bl.transform.position = pos;
        needCameraMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (needCameraMove) {
            CameraMove();
                }
    }

    private void CameraMove() {
        //camera.transform.LookAt(targetGO.transform);

        float interpolationRatio = (float) elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startCamPos, endCamPos, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        camera.transform.position = interpolatedPosition;

        //var look = Quaternion.LookRotation(targetGO.transform.position - startCameraPosition);
        //camera.transform.rotation = Quaternion.Lerp(transform.rotation, look, Time.deltaTime * speedCameraRotation);

        if (elapsedFrames >= interpolationFramesCount) {
            elapsedFrames = 0;
            needCameraMove = false;

        }
    }
}
