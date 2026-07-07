using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ForBlockOne : MonoBehaviour
{
    public Material mat2;
    public Material mat3;
    public Material mat4;
    public Material mat5;
    public Material mat6;
    public Material mat7;
    public Material mat8;
    public Material mat9;
    public Material mat10;

    public Transform Left;
    public Transform Right;
    public Transform Top;
    public Transform Down;
    public Transform Back;

    public void setNewMaterial(Vector3 targetPosition) {
        Renderer renderer;
        Left.localScale = new Vector3(targetPosition.z + 1, 1, 1);
        Right.localScale = new Vector3(targetPosition.z + 1, 1, 1);
        Top.localScale = new Vector3(targetPosition.z + 1, 1, 1);
        Down.localScale = new Vector3(targetPosition.z + 1, 1, 1);

        Left.position = new Vector3(Left.position.x, Left.position.y, (targetPosition.z + 1) / 2);
        Right.position = new Vector3(Right.position.x, Right.position.y, (targetPosition.z + 1) / 2);
        Top.position = new Vector3(Top.position.x, Top.position.y, (targetPosition.z + 1) / 2);
        Down.position = new Vector3(Down.position.x, Down.position.y, (targetPosition.z + 1) / 2);

        renderer = Left.gameObject.GetComponent<Renderer>();
        renderer.material = getMaterial((int)targetPosition.z + 1);

        renderer = Right.gameObject.GetComponent<Renderer>();
        renderer.material = getMaterial((int)targetPosition.z + 1);

        renderer = Top.gameObject.GetComponent<Renderer>();
        renderer.material = getMaterial((int)targetPosition.z + 1);

        renderer = Down.gameObject.GetComponent<Renderer>();
        renderer.material = getMaterial((int)targetPosition.z + 1);

        Back.position = new Vector3(
            Back.transform.position.x,
            Back.transform.position.y,
            (targetPosition.z +1));
    }

    private Material getMaterial(int number) {
        switch (number) {
            case 2:
                return mat2;
            case 3:
                return mat3;
            case 4:
                return mat4;
            case 5:
                return mat5;
            case 6:
                return mat6;
            case 7:
                return mat7;
            case 8:
                return mat8;
            case 9:
                return mat9;
            case 10:
                return mat2;
            default:
                return mat2;
        }
    }
}
