using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S32_Matrix
{
    private int _x;
    private int _y;
    private int _z;

    public int x {
        get => _x;
    }

    public int y {
        get => _y;
    }
    public int z {
        get => _z;
    }

    public bool[,,] array;

    public S32_Matrix(int x, int y, int z) {
        _x = x;
        _y = y;
        _z = z;
        array = new bool[x, y, z];
    }

    public S32_Matrix(float x, float y, float z) 
        : this((int)x, (int)y, (int)z) { }

    public override string ToString() {
        return string.Format("x = {0}, y = {1}, z = {2}", x, y, z);
    }
}
