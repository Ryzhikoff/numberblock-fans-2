using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollaider : MonoBehaviour
{
    public GameObject prefabOne;
    private Rigidbody rigidbody;
    public float collisionPower = 10f; // ���� ������������ ��� ������������

    public float blockCooldown = 1f; // ����� �������� ����� ��������� �� ������

    private float lastBlockBounceTime = -Mathf.Infinity; // ����� ���������� ������� �� �����

    private void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other) {
        Vector3 randomDirection = rigidbody.linearVelocity.normalized; // ����������� ����������� - ������� ��������
        Vector3 collisionNormal = other.contacts[0].normal; // ������� ����� ���������������

        switch (other.gameObject.tag)
        {
            case "Block":
                var scale = other.gameObject.GetComponent<Scale>();
                if (scale != null && scale.number != 1) {
                    createBlocks(other.gameObject.transform.position, scale);
                    Destroy(other.gameObject);
                }
                // ���������, ������ �� ���������� ������� � ������� ���������� ������������ � ������
                if (Time.time - lastBlockBounceTime >= blockCooldown) {
                    // ����������� � ��������������� �����������
                    //randomDirection = new Vector3(randomDirection.x, Mathf.Abs(randomDirection.y) + 1, randomDirection.z).normalized;
                    randomDirection = Vector3.Reflect(randomDirection, collisionNormal);
                    lastBlockBounceTime = Time.time; // ��������� ����� ���������� �������
                } else {
                    // ���������� ������������, ���� ��� �� ������ ���������� �������
                    return;
                }
                break;

            case "wall1":
            case "wall2":
            case "wall3":
            case "wall4":
                // ��������� �� �����
                randomDirection = Vector3.Reflect(randomDirection, collisionNormal) * 2;
                break;

            case "floor":
                // �������� ��� ����� � ���������� �������� � ������� �����������
                randomDirection = new Vector3(randomDirection.x, Mathf.Abs(randomDirection.y) + 1, randomDirection.z).normalized;
                break;

            case "roof":
                // ����������� ����
                randomDirection = new Vector3(randomDirection.x, -Mathf.Abs(randomDirection.y), randomDirection.z).normalized;
                break;
        }

        rigidbody.AddForce(randomDirection * collisionPower, ForceMode.Impulse);
    }

    private void createBlocks(Vector3 startPosition, Scale scale) {
        for (int x = 0; x < scale.x; x++) {
            for (int y = 0; y < scale.y; y++) { 
                for(int z = 0; z < scale.z; z++) {
                    var block = Instantiate(prefabOne);
                    block.transform.position = startPosition + new Vector3(x, y, z);
                }
            }
        }
    }
}
