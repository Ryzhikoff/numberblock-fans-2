using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour
{
    public GameObject cloudPrefab;

    public float distantionAfterCloud = 3000f;
    public Vector3 startPosition;
    

    private List<GameObject> cloudsList = new List<GameObject>();

    private void Start() {
        createNewCloud(startPosition);
    }

    private void createNewCloud(Vector3 position) {
        GameObject cloud = Instantiate<GameObject>(cloudPrefab);
        cloud.transform.position = position;
        CloudsGenerator cloudsGenerator = cloud.GetComponent<CloudsGenerator>();
        cloudsGenerator.setScale(
            position,
            new Vector3(position.x * 2 * (-1), position.y, position.z + distantionAfterCloud));
        cloudsList.Add(cloud);
    }

    private void Update() {
        checkPositionForAdd();
        checkPositionForDelete();
    }

    private void checkPositionForAdd() {
        GameObject lastCloud = cloudsList[cloudsList.Count - 1];
        if (lastCloud.transform.position.z < startPosition.z) {
            createNewCloud(startPosition + Vector3.forward * distantionAfterCloud);
        }
    }

    private void checkPositionForDelete() {
        if (cloudsList.Count < 2)
            return;
        GameObject firstCloud = cloudsList[0];
        if(firstCloud.transform.position.z < startPosition.z - distantionAfterCloud*2) {
            cloudsList.Remove(firstCloud);
            Destroy(firstCloud);
        }
    }
}
