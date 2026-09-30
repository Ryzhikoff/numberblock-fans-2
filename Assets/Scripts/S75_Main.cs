using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Scene 75: Hundred notices Five Thousand's shadow, panics, and breaks into 100 Ones.
/// The default timing adds up to exactly ten seconds.
/// </summary>
public class S75_Main : MonoBehaviour
{
    [Header("Character prefabs")]
    public GameObject hundredSmilingPrefab;
    public GameObject hundredSurprisedPrefab;
    public GameObject hundredScaredPrefab;
    [FormerlySerializedAs("thousandLookingAtHundredPrefab")]
    public GameObject fiveThousandLookingAtHundredPrefab;
    public GameObject onePrefab;

    [Header("Ten-second timing")]
    public float walkDuration = 1.55f;
    public float shadowDuration = 0.65f;
    public float surprisedHoldDuration = 0.45f;
    public float revealCameraDuration = 1.05f;
    [FormerlySerializedAs("thousandHoldDuration")]
    public float fiveThousandHoldDuration = 0.55f;
    public float returnCameraDuration = 0.85f;
    public float scaredHoldDuration = 1.5f;
    public float splitDuration = 0.25f;
    public float scatterDuration = 3.15f;

    [Header("Staging")]
    public float walkDistance = 4f;
    [FormerlySerializedAs("thousandOffset")]
    public Vector3 fiveThousandOffset = new Vector3(18f, 0f, 10f);
    public float hundredFramePadding = 1.28f;
    [FormerlySerializedAs("thousandFramePadding")]
    public float fiveThousandFramePadding = 1.08f;
    [Range(0f, 1f)] public float shadowDarkness = 0.58f;
    public float runnerMinSpeed = 5.5f;
    public float runnerMaxSpeed = 10f;

    [Header("Optional sound")]
    public AudioClip shadowClip;
    public AudioClip revealClip;
    public AudioClip scaredClip;
    public AudioClip splitClip;

    private readonly List<GameObject> ones = new List<GameObject>(100);
    private readonly List<Vector3> oneStartPositions = new List<Vector3>(100);
    private readonly List<float> oneGroundHeights = new List<float>(100);

    private Camera shotCamera;
    private AudioSource audioSource;
    private GameObject hundred;
    private GameObject fiveThousand;
    private float hundredShadowAmount;
    private Vector3 hundredShotPosition;
    private Quaternion hundredShotRotation;

    private IEnumerator Start()
    {
        ResolveEditorPrefabReferences();
        if (!ValidateReferences())
        {
            yield break;
        }

        shotCamera = Camera.main;
        if (shotCamera == null)
        {
            Debug.LogError("Scene 75 needs a camera tagged MainCamera.", this);
            yield break;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        Vector3 hundredStart = new Vector3(-walkDistance * 0.5f, 0f, 0f);
        hundred = SpawnGrounded(hundredSmilingPrefab, hundredStart, "Hundred (Smiling)");
        Vector3 hundredEnd = hundred.transform.position + Vector3.right * walkDistance;
        fiveThousand = SpawnGrounded(
            fiveThousandLookingAtHundredPrefab,
            hundredEnd + fiveThousandOffset,
            "Five Thousand (Looking At Hundred)");

        Bounds finalHundredBounds = BoundsOf(hundred);
        finalHundredBounds.center += Vector3.right * walkDistance;
        PrepareOneGrid(finalHundredBounds, hundred.transform.position + Vector3.right * walkDistance);
        FrameNow(BoundsOf(hundred), hundredFramePadding);

        yield return WalkHundred(hundredEnd);
        yield return CoverWithShadow();

        SwapHundred(hundredSurprisedPrefab, "Hundred (Surprised)");
        yield return SurprisedBeat();

        hundredShotPosition = shotCamera.transform.position;
        hundredShotRotation = shotCamera.transform.rotation;
        Play(revealClip);
        yield return MoveCameraToFiveThousand();
        yield return new WaitForSeconds(fiveThousandHoldDuration);
        yield return MoveCamera(hundredShotPosition, hundredShotRotation, returnCameraDuration);

        SwapHundred(hundredScaredPrefab, "Hundred (Scared)");
        Play(scaredClip);
        yield return ScaredBeat();

        yield return SplitHundred();
        yield return ScatterOnes();
    }

    private bool ValidateReferences()
    {
        List<string> missing = new List<string>();
        if (hundredSmilingPrefab == null)
        {
            missing.Add(nameof(hundredSmilingPrefab));
        }
        if (hundredSurprisedPrefab == null)
        {
            missing.Add(nameof(hundredSurprisedPrefab));
        }
        if (hundredScaredPrefab == null)
        {
            missing.Add(nameof(hundredScaredPrefab));
        }
        if (fiveThousandLookingAtHundredPrefab == null)
        {
            missing.Add(nameof(fiveThousandLookingAtHundredPrefab));
        }
        if (onePrefab == null)
        {
            missing.Add(nameof(onePrefab));
        }

        if (missing.Count > 0)
        {
            Debug.LogError($"Scene 75 is missing character prefabs: {string.Join(", ", missing)}.", this);
            return false;
        }

        return true;
    }

    private void ResolveEditorPrefabReferences()
    {
#if UNITY_EDITOR
        hundredSmilingPrefab = LoadEditorPrefab(
            "Assets/Prefabs/Blocks/Scene75/Hundred_Smiling.prefab",
            hundredSmilingPrefab);
        hundredSurprisedPrefab = LoadEditorPrefab(
            "Assets/Prefabs/Blocks/Scene75/Hundred_Surprised.prefab",
            hundredSurprisedPrefab);
        hundredScaredPrefab = LoadEditorPrefab(
            "Assets/Prefabs/Blocks/Scene75/Hundred_Scared.prefab",
            hundredScaredPrefab);
        fiveThousandLookingAtHundredPrefab = LoadEditorPrefab(
            "Assets/Prefabs/Blocks/Scene75/FiveThousand_LookingAtHundred.prefab",
            fiveThousandLookingAtHundredPrefab);
        onePrefab = LoadEditorPrefab(
            "Assets/Prefabs/Blocks/Scene75/One_Runner.prefab",
            onePrefab);
#endif
    }

#if UNITY_EDITOR
    private static GameObject LoadEditorPrefab(string path, GameObject current)
    {
        GameObject expected = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return expected != null ? expected : current;
    }
#endif

    private GameObject SpawnGrounded(GameObject prefab, Vector3 position, string objectName)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);
        instance.name = objectName;
        PrepareForAnimation(instance);

        Bounds bounds = BoundsOf(instance);
        instance.transform.position += Vector3.up * (position.y - bounds.min.y);
        return instance;
    }

    private static void PrepareForAnimation(GameObject target)
    {
        Rigidbody[] bodies = target.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody body in bodies)
        {
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private IEnumerator WalkHundred(Vector3 destination)
    {
        Vector3 start = hundred.transform.position;
        Quaternion startRotation = hundred.transform.rotation;
        float elapsed = 0f;

        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, walkDuration));
            float eased = Mathf.SmoothStep(0f, 1f, t);
            Vector3 position = Vector3.Lerp(start, destination, eased);
            position.y += Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4f)) * 0.38f;
            hundred.transform.position = position;
            hundred.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * 3f) * startRotation;
            FrameNow(BoundsOf(hundred), hundredFramePadding);
            yield return null;
        }

        hundred.transform.position = destination;
        hundred.transform.rotation = Quaternion.identity;
        FrameNow(BoundsOf(hundred), hundredFramePadding);
    }

    private IEnumerator CoverWithShadow()
    {
        Play(shadowClip);
        float elapsed = 0f;

        while (elapsed < shadowDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.001f, shadowDuration)));
            SetHundredShadow(t);
            yield return null;
        }

        SetHundredShadow(1f);
    }

    private IEnumerator SurprisedBeat()
    {
        Vector3 basePosition = hundred.transform.position;
        float elapsed = 0f;

        while (elapsed < surprisedHoldDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, surprisedHoldDuration));
            hundred.transform.position = basePosition + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.18f;
            hundred.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI) * -2f);
            yield return null;
        }

        hundred.transform.position = basePosition;
        hundred.transform.rotation = Quaternion.identity;
    }

    private IEnumerator MoveCameraToFiveThousand()
    {
        Bounds bounds = BoundsOf(fiveThousand);
        Vector3 targetPosition = FrameAtAngle(
            bounds,
            fiveThousandFramePadding,
            new Vector3(-0.18f, -0.16f, -1f));
        Quaternion targetRotation = Quaternion.LookRotation(bounds.center - targetPosition);
        yield return MoveCamera(targetPosition, targetRotation, revealCameraDuration);
    }

    private IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        Vector3 startPosition = shotCamera.transform.position;
        Quaternion startRotation = shotCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration)));
            shotCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            shotCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        shotCamera.transform.position = targetPosition;
        shotCamera.transform.rotation = targetRotation;
    }

    private IEnumerator ScaredBeat()
    {
        Bounds bounds = BoundsOf(hundred);
        Vector3 startCameraPosition = shotCamera.transform.position;
        Quaternion startCameraRotation = shotCamera.transform.rotation;
        Vector3 closeCameraPosition = Frame(bounds, 1.1f);
        Quaternion closeCameraRotation = Quaternion.LookRotation(bounds.center - closeCameraPosition);
        Vector3 basePosition = hundred.transform.position;
        float elapsed = 0f;

        while (elapsed < scaredHoldDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, scaredHoldDuration));
            float push = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.3f));
            shotCamera.transform.position = Vector3.Lerp(startCameraPosition, closeCameraPosition, push);
            shotCamera.transform.rotation = Quaternion.Slerp(startCameraRotation, closeCameraRotation, push);
            hundred.transform.position = basePosition + Vector3.right * Mathf.Sin(elapsed * 38f) * 0.08f;
            hundred.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(elapsed * 42f) * 1.6f);
            yield return null;
        }

        hundred.transform.position = basePosition;
        hundred.transform.rotation = Quaternion.identity;
    }

    private IEnumerator SplitHundred()
    {
        Play(splitClip);
        Vector3 originalScale = hundred.transform.localScale;
        float elapsed = 0f;
        bool revealedOnes = false;

        while (elapsed < splitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, splitDuration));
            float scale = t < 0.38f
                ? Mathf.Lerp(1f, 1.12f, t / 0.38f)
                : Mathf.Lerp(1.12f, 0f, (t - 0.38f) / 0.62f);
            hundred.transform.localScale = originalScale * scale;

            if (!revealedOnes && t >= 0.48f)
            {
                revealedOnes = true;
                foreach (GameObject one in ones)
                {
                    one.SetActive(true);
                }
            }
            yield return null;
        }

        Destroy(hundred);
    }

    private void PrepareOneGrid(Bounds hundredBounds, Vector3 hundredRootPosition)
    {
        Vector3 hundredCenter = hundredBounds.center;
        for (int row = 0; row < 10; row++)
        {
            for (int column = 0; column < 10; column++)
            {
                int index = row * 10 + column;
                GameObject one = Instantiate(onePrefab, hundredRootPosition, Quaternion.identity, transform);
                one.name = $"One {index + 1:000}";
                PrepareForAnimation(one);

                Bounds oneBounds = BoundsOf(one);
                Vector3 cellCenter = new Vector3(
                    hundredBounds.min.x + column + 0.5f,
                    hundredBounds.min.y + row + 0.5f,
                    hundredBounds.min.z - 0.02f);
                one.transform.position += cellCenter - oneBounds.center;

                oneBounds = BoundsOf(one);
                ones.Add(one);
                oneStartPositions.Add(one.transform.position);
                oneGroundHeights.Add(one.transform.position.y - oneBounds.min.y + hundredBounds.min.y);
                one.SetActive(false);
            }
        }

        if (ones.Count != 100)
        {
            Debug.LogError($"Scene 75 expected 100 Ones, but prepared {ones.Count}.", this);
        }
    }

    private IEnumerator ScatterOnes()
    {
        const int seed = 75;
        System.Random random = new System.Random(seed);
        Vector3[] directions = new Vector3[ones.Count];
        float[] speeds = new float[ones.Count];
        float[] phases = new float[ones.Count];

        for (int i = 0; i < ones.Count; i++)
        {
            float evenAngle = Mathf.PI * 2f * i / Mathf.Max(1, ones.Count);
            float jitter = Mathf.Lerp(-0.16f, 0.16f, (float)random.NextDouble());
            float angle = evenAngle + jitter;
            directions[i] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            speeds[i] = Mathf.Lerp(runnerMinSpeed, runnerMaxSpeed, (float)random.NextDouble());
            phases[i] = (float)random.NextDouble() * Mathf.PI * 2f;
        }

        Bounds originalHundredArea = new Bounds(
            AverageOneCenters(),
            new Vector3(18f, 13f, 10f));
        Vector3 cameraTarget = Frame(originalHundredArea, 1.12f);
        Quaternion cameraRotation = Quaternion.LookRotation(originalHundredArea.center - cameraTarget);
        Vector3 cameraStart = shotCamera.transform.position;
        Quaternion rotationStart = shotCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < scatterDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, scatterDuration));
            float cameraT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.28f));
            shotCamera.transform.position = Vector3.Lerp(cameraStart, cameraTarget, cameraT);
            shotCamera.transform.rotation = Quaternion.Slerp(rotationStart, cameraRotation, cameraT);

            float landingT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.68f));
            for (int i = 0; i < ones.Count; i++)
            {
                Vector3 position = oneStartPositions[i] + directions[i] * speeds[i] * elapsed;
                position.y = Mathf.Lerp(oneStartPositions[i].y, oneGroundHeights[i], landingT);
                if (landingT >= 1f)
                {
                    position.y += Mathf.Abs(Mathf.Sin(elapsed * 8.5f + phases[i])) * 0.18f;
                }
                else
                {
                    position.y += Mathf.Sin(landingT * Mathf.PI) * 0.7f;
                }

                ones[i].transform.position = position;
                ones[i].transform.rotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Sin(elapsed * 9f + phases[i]) * 7f);
            }
            yield return null;
        }
    }

    private Vector3 AverageOneCenters()
    {
        Vector3 total = Vector3.zero;
        foreach (GameObject one in ones)
        {
            total += BoundsOf(one).center;
        }
        return total / Mathf.Max(1, ones.Count);
    }

    private void SwapHundred(GameObject replacementPrefab, string objectName)
    {
        Transform oldTransform = hundred.transform;
        GameObject replacement = Instantiate(
            replacementPrefab,
            oldTransform.position,
            oldTransform.rotation,
            transform);
        replacement.transform.localScale = oldTransform.localScale;
        replacement.name = objectName;
        PrepareForAnimation(replacement);
        Destroy(hundred);
        hundred = replacement;
        SetHundredShadow(hundredShadowAmount);
    }

    private void SetHundredShadow(float amount)
    {
        hundredShadowAmount = Mathf.Clamp01(amount);
        if (hundred == null)
        {
            return;
        }

        float brightness = Mathf.Lerp(1f, 1f - shadowDarkness, hundredShadowAmount);
        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        Renderer[] renderers = hundred.GetComponentsInChildren<Renderer>();

        foreach (Renderer targetRenderer in renderers)
        {
            Material[] materials = targetRenderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material source = materials[materialIndex];
                if (source == null)
                {
                    continue;
                }

                properties.Clear();
                targetRenderer.GetPropertyBlock(properties, materialIndex);
                bool changed = false;

                if (source.HasProperty("_Color"))
                {
                    properties.SetColor("_Color", Darken(source.GetColor("_Color"), brightness));
                    changed = true;
                }
                if (source.HasProperty("_BaseColor"))
                {
                    properties.SetColor("_BaseColor", Darken(source.GetColor("_BaseColor"), brightness));
                    changed = true;
                }

                if (changed)
                {
                    targetRenderer.SetPropertyBlock(properties, materialIndex);
                }
            }
        }
    }

    private static Color Darken(Color color, float brightness)
    {
        return new Color(
            color.r * brightness,
            color.g * brightness,
            color.b * brightness,
            color.a);
    }

    private void Play(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private Vector3 Frame(Bounds bounds, float padding)
    {
        float verticalHalf = bounds.extents.y * padding;
        float horizontalHalf = bounds.extents.x * padding / Mathf.Max(0.01f, shotCamera.aspect);
        float requiredHalf = Mathf.Max(verticalHalf, horizontalHalf);
        float distance = requiredHalf / Mathf.Tan(shotCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        return bounds.center + Vector3.back * (distance + bounds.extents.z + 0.5f);
    }

    private Vector3 FrameAtAngle(Bounds bounds, float padding, Vector3 directionFromTarget)
    {
        float verticalHalf = bounds.extents.y * padding;
        float horizontalHalf = bounds.extents.x * padding / Mathf.Max(0.01f, shotCamera.aspect);
        float requiredHalf = Mathf.Max(verticalHalf, horizontalHalf);
        float distance = requiredHalf / Mathf.Tan(shotCamera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        distance += bounds.extents.magnitude * 0.35f;
        return bounds.center + directionFromTarget.normalized * distance;
    }

    private void FrameNow(Bounds bounds, float padding)
    {
        shotCamera.transform.position = Frame(bounds, padding);
        shotCamera.transform.LookAt(bounds.center);
    }

    private static Bounds BoundsOf(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(target.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }
}
