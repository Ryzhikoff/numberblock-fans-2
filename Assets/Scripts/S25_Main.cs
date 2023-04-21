using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S25_Main : MonoBehaviour
{
    public List<Vector3> cameraPosition;
    public Vector3 startPositionMineCart;
    public Vector3 startPositionFirstBlock;

    public float speedMinecart = 5;

    public GameObject prefabMineCart;
    public GameObject prefabOne;
    private int count = 1;


    private Camera camera;

    private void Start() {
        camera = Camera.main;
        camera.transform.position = cameraPosition[0];
        addNewMineCart();
    }

    private void addNewMineCart() {
        GameObject minecart = Instantiate(prefabMineCart);
        minecart.GetComponent<S25_Minecart>().onStart(this, speedMinecart);
        minecart.transform.position = startPositionMineCart;
    }

    public void arrived() {
        print("Приехал блок номер: " + count++);
        addNewMineCart();
    }
}
