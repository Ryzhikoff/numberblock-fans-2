using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S56_Controller : MonoBehaviour
{
    public List<GameObject> prefabs;
    public int distanceBetween = 10;
    // Start is called before the first frame update
    void Start()
    {
        createBlocks();
    }

    private void createBlocks() {
        for (int i = 0; i < prefabs.Count; i++) {
            GameObject prefab = prefabs[i];
            if (prefab != null) {
                Instantiate(prefab);
                var scale = prefab.GetComponent<Scale>();
                prefab.transform.position = Vector3.zero + Vector3.forward * distanceBetween * (i + 1) + Vector3.left * scale.x / 2;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
