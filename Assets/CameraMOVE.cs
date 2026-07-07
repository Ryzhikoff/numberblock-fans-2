using UnityEngine;

public class CameraMOVE : MonoBehaviour
{
    public Vector3 startPostion = new Vector3();
    public Vector3 endPostion = new Vector3();

    public long interpolationFramesCount;
    long elapsedFrames = 0;
    public float delayForStart = 2;
    public Camera camera;
    private bool needCameraMove = false;

    private void Start() {
        startCameraMove();
    }

    private void Update() {
        if (needCameraMove)
            cameraMove();
    }

    private void startCameraMove() {
       
        needCameraMove = true;
    }


    private void cameraMove() { 
        float interpolationRatio = (float)elapsedFrames / interpolationFramesCount;
        Vector3 interpolatedPosition = Vector3.Lerp(startPostion, endPostion, interpolationRatio);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижения (interpolationFramesCount + 1)
        transform.position = interpolatedPosition;
        if (Mathf.Abs(transform.position.y - endPostion.y) < 0.05f) {
            needCameraMove = false;
            
        }
    }
}
