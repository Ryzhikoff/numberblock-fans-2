using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMove : MonoBehaviour
{
    [Header("Префаб земли")]
    public GameObject prefabGround;
    [Header("Родитель земли")]
    public GameObject parentGround;
    [Header("Стартовая позиция")]
    public Vector3 startPosition;
    [Header("Скорость движения")]
    public float speedMove;
    [Header("Точка удаления")]
    public float xStopPosition = -22f;
    [Header("Задержка")]
    public float delay;

    //Z и Y Координаты для земли
    public float Z = 761.5f;
    public float Y = -97.28f;

    private List<GameObject> list = new List<GameObject>(0);

    private bool _move;

    public bool move {
        get {
            return _move;
        }
        set {
            _move = value;
            if (value) {
                moveGround();
            }
        }
    }

    private void Start() {
        createFirstGround();
    }

    private void createFirstGround() {
        for (int x = -1000; x < 2001; x += 1000) {
            GameObject go = Instantiate<GameObject>(prefabGround);
            go.transform.parent = parentGround.transform;
            go.transform.position = new Vector3(x, Y, Z);
            list.Add(go);
        }
    }


    private void moveGround() {
        foreach (GameObject go in list) {
            go.transform.position = new Vector3(
                go.transform.position.x - speedMove,
                go.transform.position.y,
                go.transform.position.z);
        }

        //если дошли до края - удаляем первый объект - добавляем последний
        if (list[0].transform.position.x <= xStopPosition) {
            Destroy(list[0]);
            list.RemoveAt(0);

            GameObject go = Instantiate<GameObject>(prefabGround);
            go.transform.parent = parentGround.transform;
            go.transform.position = new Vector3(
                list[2].transform.position.x + 1000f,
                list[2].transform.position.y,
                list[2].transform.position.z);
            list.Add(go);
        }

        if (move) {
            Invoke("moveGround", delay);
        }
    }
}
