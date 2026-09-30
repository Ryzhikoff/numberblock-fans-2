using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

// Automatically enables Bloom (neon glow) in every scene at runtime:
// instantiates the NeonBloomRig prefab from Resources and attaches a
// PostProcessLayer to every active camera, so none of the 128 scenes
// need to be edited by hand.
public class NeonBloomBootstrap : MonoBehaviour
{
    public PostProcessResources resources;

    static NeonBloomBootstrap instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
            return;
        var prefab = Resources.Load<GameObject>("NeonBloomRig");
        if (prefab == null)
        {
            Debug.LogWarning("[NeonBloom] NeonBloomRig prefab not found in Resources");
            return;
        }
        Instantiate(prefab).name = "NeonBloomRig";
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        AttachToCameras();
    }

    void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AttachToCameras();
    }

    void AttachToCameras()
    {
        foreach (var cam in Camera.allCameras)
        {
            if (cam.GetComponent<PostProcessLayer>() != null)
                continue;
            var layer = cam.gameObject.AddComponent<PostProcessLayer>();
            layer.Init(resources);
            layer.volumeTrigger = cam.transform;
            layer.volumeLayer = ~0;
            layer.antialiasingMode = PostProcessLayer.Antialiasing.None;
            cam.allowHDR = true;
        }
    }
}
