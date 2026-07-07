using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S56_CameraMove : MonoBehaviour
{
    public Transform followTarget;

    void LateUpdate() {
        transform.position = followTarget.position;
    }
}
