using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class S36_Container {

    private float _sizeX;

    private List<GameObject> _blocks;

    public float sizeX {
        get => _sizeX;
    }

    public List<GameObject> blocks {
        get => _blocks;
    }

    public S36_Container(float sizeX, List<GameObject> blocks) {
        _sizeX = sizeX;
        _blocks = blocks;
    }

}
