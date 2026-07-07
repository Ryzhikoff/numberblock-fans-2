using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class S26_ : MonoBehaviour {
    public Vector3 startPostion;
    public List<Vector3> targetPositions;

    public List<AudioClip> blockClips;
    public AudioClip approachClip;
    public AudioClip fromTarget;
    private AudioSource audio;

    private Vector3 startMovePos;
    private Vector3 endMovePostion;

    public int interpolationFramesCount = 500;
    private int elapsedFrames = 0;

    public float delayTime = 3;
    private bool needMove = false;
    private int counter = 0;
    public float power = 3f;

    private MoveDirection moveDirection = MoveDirection.TARGET;

    private void Start() {
        audio = GetComponent<AudioSource>();
        transform.position = startPostion;
        prepareMoveToTarget();
    }

    private void prepareMoveToTarget() {
        if (counter >= targetPositions.Count) { return; }
        startMovePos = transform.position;
        endMovePostion = targetPositions[counter];
        needMove = true;
        moveDirection = MoveDirection.TARGET;
        audio.Stop();
        audio.PlayOneShot(approachClip);
    }

    private void prepareMoveToStart() {
        startMovePos = transform.position;
        endMovePostion = startPostion;
        needMove = true;
        moveDirection = MoveDirection.START;
        audio.Stop();
        audio.PlayOneShot(fromTarget);
    }

    private void Update() {
        if (needMove) {
            cameraMove();
        }
    }

    private void cameraMove() {

        float t = (float) elapsedFrames / interpolationFramesCount;
        float curvedT;

        // ¬ зависимости от направлени€ движени€:
        if (moveDirection == MoveDirection.START) {
            // ћедленный старт, быстрый конец (ускорение)
            curvedT = Mathf.Pow(t, power); // t^2
        } else {
            // Ѕыстрый старт, медленный конец (замедление)
            curvedT = 1 - Mathf.Pow(1 - t, power); // 1 - (1 - t)^2
        }

        Vector3 interpolatedPosition = Vector3.Lerp(startMovePos, endMovePostion, curvedT);
        elapsedFrames = (elapsedFrames + 1) % (interpolationFramesCount + 1); // сбрасываем elapsedFrames в ноль после достижени€ (interpolationFramesCount + 1)
        transform.position = interpolatedPosition;
        if (Mathf.Abs(transform.position.z - endMovePostion.z) < 0.05f) {
            needMove = false;
            elapsedFrames = 0;
            
            switch (moveDirection) {
                case MoveDirection.START:
                    prepareMoveToTarget();
                    break;
                case MoveDirection.TARGET:
                    audio.Stop();
                    audio.PlayOneShot(blockClips[counter]);
                    counter++;
                    Invoke(nameof(prepareMoveToStart), delayTime); 
                    break;
            }

        }
    }

    private enum MoveDirection {
        TARGET,
        START
    }
}
