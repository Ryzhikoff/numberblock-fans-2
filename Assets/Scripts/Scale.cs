using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale : MonoBehaviour
{
    public Vector3 scale;

    public int x {
        get {
            return (int) scale.x;
        }
    }

    public int y {
        get {
            return (int) scale.y;
        }
    }

    public int z {
        get {
            return (int) scale.z;
        }
    }
}
