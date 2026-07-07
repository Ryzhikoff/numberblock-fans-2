using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class S44_Main : MonoBehaviour
{
    public List<GameObject> prefabs;
    public float startSize = 0.1f;
    public float speedResize = 1;
    public float targetSize = 1;
    public Vector3 startBlockPosition = new Vector3(-0.1f, 0, -0.1f);
    public Vector3 fixedPosition = new Vector3(0f, 0f, 0f);

    public float delayAfterNewBlock = 2f;
    private bool needRize = false;
    private GameObject currectBlock;
    private GameObject lastBlock;
    public int counter = 0;

    public GameObject targetGoForCamera;
    private Camera camera;
    public TextMeshProUGUI textMesh;

    private void Start() {
        camera = Camera.main;
        Invoke("createNewBlock", delayAfterNewBlock);
    }

    private void createNewBlock() {
        if (counter == prefabs.Count) {
            return;
        }
        
        GameObject go = Instantiate<GameObject>(prefabs[counter]);
        go.transform.position = startBlockPosition;
        go.transform.localScale = new Vector3(startSize, startSize, startSize);
        currectBlock = go;
        needRize = true;
        counter++;
    }

    private void Update() {
        if (needRize) {
            resizeBlock();
            newPositionForTargetGo();
            cameraLookAt();
        }
    }

    private void resizeBlock() {
        float delta = speedResize * Time.deltaTime;
        float newSize = currectBlock.transform.localScale.x + delta;
        currectBlock.transform.localScale = new Vector3(newSize, newSize, newSize);
        if (currectBlock.transform.localScale.x >= targetSize) {
            currectBlock.transform.localScale = new Vector3(1f, 1f, 1f);
            currectBlock.transform.position = fixedPosition;
            needRize = false;
            Invoke("createNewBlock", delayAfterNewBlock);
            if( lastBlock != null) {
                Destroy(lastBlock);
            }
            lastBlock = currectBlock;
            setText();
        }
    }

    private void newPositionForTargetGo() {
        float blockSize = currectBlock.transform.localScale.x;
        Scale scale = currectBlock.GetComponent<Scale>();

        targetGoForCamera.transform.position = new Vector3(scale.x * blockSize, scale.y * blockSize, scale.z * blockSize);
    }

    private void cameraLookAt() {
        camera.transform.LookAt(targetGoForCamera.transform);
    }

    private void setText() {
        Scale scale = currectBlock.GetComponent<Scale>();
        textMesh.text = scale.number.ToString();
    }
}
