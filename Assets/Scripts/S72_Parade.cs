using System.Collections;
using UnityEngine;

public class S72_Parade : MonoBehaviour
{
    [Header("Blocks")]
    public GameObject[] blockPrefabs;   // префабы 1..100 по порядку
    public float spacing = 2f;          // отступ между блоками

    [Header("Camera")]
    public float moveDuration = 1.5f;   // время перелёта к следующему блоку
    public float pauseDuration = 2f;    // задержка на блоке перед следующим движением
    public Vector3 cameraOffset = new Vector3(6f, 5f, -12f); // взгляд влево и вниз

    [Header("Labels")]
    public bool showLabels = true;      // подпись с числом над каждым блоком
    public float labelHeight = 0.6f;    // отступ подписи от верха блока
    public float labelSize = 1.2f;      // высота цифр в юнитах
    public Color labelColor = Color.black;

    private Vector3[] lookPoints;
    private Font labelFont;

    void Start()
    {
        SpawnRow();
        StartCoroutine(CameraTour());
    }

    void SpawnRow()
    {
        lookPoints = new Vector3[blockPrefabs.Length];
        float cursor = 0f;
        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            if (blockPrefabs[i] == null) continue;
            GameObject go = Instantiate(blockPrefabs[i], Vector3.zero, Quaternion.identity, transform);
            go.name = (i + 1).ToString();
            Bounds b = GetBounds(go);
            // левый край блока на cursor, основание на y = 0
            go.transform.position = new Vector3(cursor - b.min.x, -b.min.y, 0f);
            b = GetBounds(go);
            lookPoints[i] = b.center;
            if (showLabels) CreateLabel(i + 1, b, go.transform);
            cursor = b.max.x + spacing;
        }
    }

    void CreateLabel(int number, Bounds b, Transform parent)
    {
        if (labelFont == null)
        {
            try { labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { labelFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        }
        GameObject go = new GameObject("label_" + number);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(b.center.x, b.max.y + labelHeight, b.center.z);

        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = number.ToString();
        tm.font = labelFont;
        tm.fontSize = 80;
        tm.characterSize = labelSize * 10f / tm.fontSize;
        tm.anchor = TextAnchor.LowerCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = labelColor;
        go.GetComponent<MeshRenderer>().sharedMaterial = labelFont.material;
    }

    Bounds GetBounds(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>();
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    IEnumerator CameraTour()
    {
        Camera cam = Camera.main;
        if (cam == null || lookPoints.Length == 0) yield break;
        Transform ct = cam.transform;

        ct.position = lookPoints[0] + cameraOffset;
        ct.LookAt(lookPoints[0]);
        yield return new WaitForSeconds(pauseDuration);

        for (int i = 1; i < lookPoints.Length; i++)
        {
            Vector3 fromPos = ct.position;
            Vector3 fromLook = lookPoints[i - 1];
            Vector3 toPos = lookPoints[i] + cameraOffset;
            Vector3 toLook = lookPoints[i];

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, moveDuration);
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                ct.position = Vector3.Lerp(fromPos, toPos, s);
                ct.LookAt(Vector3.Lerp(fromLook, toLook, s));
                yield return null;
            }
            yield return new WaitForSeconds(pauseDuration);
        }
    }
}
