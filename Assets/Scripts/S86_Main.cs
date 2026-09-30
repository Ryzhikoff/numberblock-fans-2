using System;
using System.Collections;
using UnityEngine;

/// <summary>Scene 86: silent side-scrolling collision short starring 100.</summary>
public class S86_Main : MonoBehaviour
{
    public const string HundredOriginalFaceName = "S82 100 Original Face";
    public const string HundredFocusedFaceName = "S82 100 Determined Face";
    public const string HundredShockedFaceName = "S82 100 Shocked Face";
    public const string HappyFaceName = "S85 Happy Face";
    public const string ScaredFaceName = "S85 Scared Face";
    public const string SmugFaceName = "S85 Smug Face";

    public const int ExpectedBurstCount = 3;
    public const int ExpectedUnitCount = 10 + 50 + 100;

    [Header("Characters")]
    public GameObject hundredPrefab;
    public GameObject tenPrefab;
    public GameObject fiftyPrefab;
    public GameObject thousandPrefab;

    [Header("Scene")]
    public Camera shotCamera;
    public S86_UnitBurst unitBurst;
    public Material flashMaterial;

    [Header("Existing project audio")]
    public AudioClip runClip;
    public AudioClip impactClip;
    public AudioClip burstClip;

    [Header("Timing")]
    public float openingHold = 0.45f;
    public float runSpeed = 10.5f;
    public float finalHold = 2.8f;

    public string Beat { get; private set; }
    public float BeatTime { get; private set; }
    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public int CollisionCount { get; private set; }
    public int DestroyedUnitCount { get; private set; }
    public bool HundredStayedCentered { get; private set; } = true;
    public bool ThousandStayedStanding { get; private set; } = true;
    public float RunProgress { get; private set; }
    public float CollisionFraming { get; private set; }

    private Actor runner;
    private Actor ten;
    private Actor fifty;
    private Actor thousand;
    private AudioSource effectsSource;
    private AudioSource runSource;
    private float sequenceStart;
    private float cameraFocusX;
    private float cameraFollowOffsetX;
    private float cameraDistance = 18f;
    private float cameraShake;
    private Vector3 cameraBasePosition;
    private Quaternion cameraBaseRotation;

    private sealed class Actor
    {
        public Transform Root;
        public Transform Visual;
        public BoxCollider BodyCollider;
        public Transform PrimaryFace;
        public Transform SecondaryFace;
        public Transform TertiaryFace;
        public float UniformScale;
        public Bounds Bounds => BoundsOf(Visual.gameObject);
        public Bounds BodyBounds => BoundsOf(BodyCollider);
    }

    private IEnumerator Start()
    {
        if (!ValidateReferences())
        {
            yield break;
        }

        shotCamera.aspect = 9f / 16f;
        shotCamera.orthographic = false;
        shotCamera.fieldOfView = 50f;
        shotCamera.allowHDR = false;
        foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer layer in
            shotCamera.GetComponents<UnityEngine.Rendering.PostProcessing.PostProcessLayer>())
        {
            layer.enabled = false;
        }

        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        runSource = gameObject.AddComponent<AudioSource>();
        runSource.playOnAwake = false;
        runSource.loop = true;
        runSource.spatialBlend = 0f;
        runSource.volume = 0.34f;
        runSource.clip = runClip;

        unitBurst.Prepare();
        if (!unitBurst.IsPrepared)
        {
            yield break;
        }

        SpawnCast();
        sequenceStart = Time.time;
        cameraFocusX = runner.Root.position.x;
        ApplyCamera(true);
        if (runClip != null)
        {
            runSource.Play();
        }

        yield return Hold("Focused hook", openingHold);
        yield return RunToTenByBodyContact("Run to frightened 10");
        yield return BreakVictim(ten, new Vector3Int(2, 5, 1), 10, 101);
        yield return RunTo(fifty, "Run to frightened 50");
        yield return BreakVictim(fifty, new Vector3Int(5, 10, 1), 50, 503);
        yield return RunTo(thousand, "Run to immovable 1000");
        yield return BreakRunnerOnThousand();

        runSource.Stop();
        yield return Hold("Smug 1000 finish", finalHold);
        SequenceTime = Time.time - sequenceStart;
        SequenceComplete = true;
        Beat = "Complete";
        Debug.Log(
            $"[S86] COMPLETE — {CollisionCount} collisions, {unitBurst.BurstCount} exact bursts, " +
            $"{unitBurst.TotalSpawned} original Ones, {SequenceTime:0.00}s, silent story.",
            this);
    }

    private void SpawnCast()
    {
        runner = SpawnActor(hundredPrefab, "S86 Runner 100", 0.60f,
            HundredFocusedFaceName, HundredOriginalFaceName, HundredShockedFaceName);
        ten = SpawnActor(tenPrefab, "S86 Frightened 10", 0.60f,
            ScaredFaceName, HappyFaceName, SmugFaceName);
        fifty = SpawnActor(fiftyPrefab, "S86 Frightened 50", 0.60f,
            ScaredFaceName, HappyFaceName, SmugFaceName);
        thousand = SpawnActor(thousandPrefab, "S86 Immovable 1000", 0.92f,
            HappyFaceName, SmugFaceName, ScaredFaceName);

        runner.Root.position = Vector3.zero;
        ten.Root.position = new Vector3(-22f, 0f, 0f);
        fifty.Root.position = new Vector3(-48f, 0f, 0f);
        thousand.Root.position = new Vector3(-82f, 0f, 0f);
        SetFace(runner, 0);
        SetFace(ten, 0);
        SetFace(fifty, 0);
        SetFace(thousand, 0);
    }

    private Actor SpawnActor(
        GameObject prefab,
        string actorName,
        float uniformScale,
        string primaryFace,
        string secondaryFace,
        string tertiaryFace)
    {
        GameObject rootObject = new GameObject(actorName);
        rootObject.transform.SetParent(transform, false);
        GameObject visual = Instantiate(prefab, rootObject.transform);
        visual.name = prefab.name;
        visual.transform.localPosition = Vector3.zero;
        BoxCollider bodyCollider = visual.GetComponent<BoxCollider>();
        foreach (MonoBehaviour behaviour in visual.GetComponentsInChildren<MonoBehaviour>(true))
        {
            behaviour.enabled = false;
        }
        foreach (Animator animator in visual.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
        }
        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }
        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        Bounds bounds = BoundsOf(visual);
        visual.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        rootObject.transform.localScale = Vector3.one * uniformScale;
        Actor actor = new Actor
        {
            Root = rootObject.transform,
            Visual = visual.transform,
            BodyCollider = bodyCollider,
            PrimaryFace = FindChild(visual.transform, primaryFace),
            SecondaryFace = FindChild(visual.transform, secondaryFace),
            TertiaryFace = FindChild(visual.transform, tertiaryFace),
            UniformScale = uniformScale
        };
        if (actor.PrimaryFace == null || actor.SecondaryFace == null || actor.TertiaryFace == null ||
            actor.BodyCollider == null)
        {
            Debug.LogError($"[S86] {prefab.name} is missing a required expression layer.", prefab);
        }
        return actor;
    }

    private IEnumerator RunToTenByBodyContact(string beat)
    {
        SetFace(runner, 0);
        Bounds runnerBody = runner.BodyBounds;
        Bounds targetBody = ten.BodyBounds;
        const float requiredBodyPenetration = 0.20f;
        float startRunnerMinX = runnerBody.min.x;
        float travelDistance = Mathf.Max(0.01f,
            runnerBody.min.x - targetBody.max.x + requiredBodyPenetration);
        float startOffset = cameraFollowOffsetX;
        float startDistance = cameraDistance;
        float startTime = Time.time;

        Beat = beat;
        BeatTime = 0f;
        while (ten.Root.gameObject.activeInHierarchy)
        {
            runnerBody = runner.BodyBounds;
            targetBody = ten.BodyBounds;
            float bodyPenetration = targetBody.max.x - runnerBody.min.x;
            if (bodyPenetration >= requiredBodyPenetration)
            {
                // This overlap was already rendered on the preceding frame.
                break;
            }

            float step = Mathf.Min(runSpeed * Time.deltaTime,
                requiredBodyPenetration - bodyPenetration + 0.02f);
            runner.Root.position += Vector3.left * step;
            BeatTime = Time.time - startTime;
            RunProgress = Mathf.Clamp01(
                (startRunnerMinX - runner.BodyBounds.min.x) / travelDistance);
            cameraFocusX = runner.Root.position.x;
            float settle = 1f - Smooth(Mathf.Clamp01(RunProgress / 0.28f));
            float reveal = Smooth(Mathf.Clamp01((RunProgress - 0.54f) / 0.46f));
            CollisionFraming = Mathf.Max(settle, reveal);
            cameraFollowOffsetX = Mathf.Lerp(0f, -2.2f, reveal) + startOffset * settle;
            cameraDistance = Mathf.Lerp(18f, 24f, reveal) + (startDistance - 18f) * settle;
            AnimateRunCycle(RunProgress, travelDistance / runSpeed);
            yield return null;
        }

        RunProgress = 1f;
        CollisionFraming = 1f;
        cameraFollowOffsetX = -2.2f;
        cameraDistance = 24f;
        runner.Visual.localPosition = Vector3.zero;
        runner.Visual.localRotation = Quaternion.identity;
    }

    private IEnumerator RunTo(Actor target, string beat)
    {
        SetFace(runner, 0);
        Bounds runnerBounds = runner.Bounds;
        Bounds targetBounds = target.Bounds;
        float contactX = targetBounds.center.x + targetBounds.extents.x + runnerBounds.extents.x -
            (runnerBounds.center.x - runner.Root.position.x);
        float startX = runner.Root.position.x;
        float distance = Mathf.Abs(contactX - startX);
        float duration = Mathf.Max(0.35f, distance / runSpeed);
        float startOffset = cameraFollowOffsetX;
        float startDistance = cameraDistance;
        bool finalTarget = target == thousand;
        float collisionOffset = finalTarget ? -2.8f : -2.2f;
        float collisionDistance = finalTarget ? 29f : 24f;

        yield return Animate(beat, duration, progress =>
        {
            RunProgress = progress;
            float x = Mathf.Lerp(startX, contactX, progress);
            runner.Root.position = new Vector3(x, 0f, 0f);
            cameraFocusX = x;
            // Return briefly to the centred running shot, then reveal the next
            // victim with 100 deliberately composed on the right side.
            float settle = 1f - Smooth(Mathf.Clamp01(progress / 0.28f));
            float reveal = Smooth(Mathf.Clamp01((progress - 0.54f) / 0.46f));
            CollisionFraming = Mathf.Max(settle, reveal);
            cameraFollowOffsetX = Mathf.Lerp(0f, collisionOffset, reveal) + startOffset * settle;
            cameraDistance = Mathf.Lerp(18f, collisionDistance, reveal) + (startDistance - 18f) * settle;
            AnimateRunCycle(progress, duration);
        });
        RunProgress = 1f;
        CollisionFraming = 1f;
        cameraFollowOffsetX = collisionOffset;
        cameraDistance = collisionDistance;
        runner.Root.position = new Vector3(contactX, 0f, 0f);
        runner.Visual.localPosition = Vector3.zero;
        runner.Visual.localRotation = Quaternion.identity;
    }

    private IEnumerator BreakVictim(Actor victim, Vector3Int grid, int exactCount, int seed)
    {
        Bounds victimBounds = victim.Bounds;
        TriggerImpact(victimBounds.center);
        CollisionCount++;
        unitBurst.Begin(victimBounds, grid, exactCount, seed);
        DestroyedUnitCount += exactCount;
        victim.Root.gameObject.SetActive(false);

        Vector3 startPosition = runner.Root.position;
        Vector3 startScale = Vector3.one * runner.UniformScale;
        yield return Animate($"100 smashes {exactCount}", 0.30f, progress =>
        {
            float pulse = Mathf.Sin(progress * Mathf.PI);
            runner.Root.position = startPosition + Vector3.left * (1.65f * progress);
            runner.Root.localScale = Vector3.Scale(startScale,
                new Vector3(1f + pulse * 0.09f, 1f - pulse * 0.13f, 1f + pulse * 0.04f));
            cameraFocusX = runner.Root.position.x;
        });
        runner.Root.localScale = startScale;
    }

    private IEnumerator BreakRunnerOnThousand()
    {
        Bounds runnerBounds = runner.Bounds;
        Vector3 thousandPosition = thousand.Root.position;
        TriggerImpact(runnerBounds.center);
        CollisionCount++;
        SetFace(thousand, 1);
        unitBurst.Begin(runnerBounds, new Vector3Int(10, 10, 1), 100, 1009);
        DestroyedUnitCount += 100;
        runner.Root.gameObject.SetActive(false);

        Vector3 originalScale = Vector3.one * thousand.UniformScale;
        float impactDistance = cameraDistance;
        yield return Animate("100 breaks on 1000", 0.55f, progress =>
        {
            float recoil = Mathf.Sin(progress * Mathf.PI) * 0.035f;
            thousand.Root.localScale = Vector3.Scale(originalScale,
                new Vector3(1f + recoil, 1f - recoil, 1f + recoil));
            cameraFocusX = Mathf.Lerp(cameraFocusX, thousandPosition.x, Smooth(progress));
            cameraFollowOffsetX = Mathf.Lerp(cameraFollowOffsetX, 0f, Smooth(progress));
            cameraDistance = Mathf.Lerp(impactDistance, 22f, Smooth(progress));
        });
        thousand.Root.localScale = originalScale;
        cameraFocusX = thousandPosition.x;
        cameraFollowOffsetX = 0f;
        cameraDistance = 22f;
        ThousandStayedStanding &= Vector3.Distance(thousand.Root.position, thousandPosition) < 0.001f;
    }

    private void AnimateRunCycle(float progress, float duration)
    {
        float cycles = Mathf.Max(1f, duration * 4.8f);
        float phase = progress * cycles * Mathf.PI * 2f;
        runner.Visual.localPosition = Vector3.up * (0.10f + Mathf.Abs(Mathf.Sin(phase)) * 0.18f);
        runner.Visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * 2.3f);
    }

    private void TriggerImpact(Vector3 position)
    {
        cameraShake = 0.42f;
        if (impactClip != null)
        {
            effectsSource.PlayOneShot(impactClip, 0.90f);
        }
        if (burstClip != null)
        {
            effectsSource.PlayOneShot(burstClip, 0.52f);
        }
        StartCoroutine(ImpactFlash(position));
    }

    private IEnumerator ImpactFlash(Vector3 position)
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "S86 Impact Flash";
        flash.transform.position = new Vector3(position.x, position.y, -0.9f);
        flash.transform.localScale = Vector3.one * 0.15f;
        Destroy(flash.GetComponent<Collider>());
        Renderer renderer = flash.GetComponent<Renderer>();
        renderer.sharedMaterial = flashMaterial;
        float start = Time.time;
        const float duration = 0.16f;
        while (Time.time - start < duration)
        {
            float progress = Mathf.Clamp01((Time.time - start) / duration);
            flash.transform.localScale = Vector3.one * Mathf.Lerp(0.15f, 2.7f, Smooth(progress));
            yield return null;
        }
        Destroy(flash);
    }

    private IEnumerator Hold(string beat, float duration)
    {
        yield return Animate(beat, duration, null);
    }

    private IEnumerator Animate(string beat, float duration, Action<float> update)
    {
        Beat = beat;
        BeatTime = 0f;
        float start = Time.time;
        while (Time.time - start < duration)
        {
            BeatTime = Time.time - start;
            float progress = Mathf.Clamp01(BeatTime / duration);
            update?.Invoke(progress);
            yield return null;
        }
        BeatTime = duration;
        update?.Invoke(1f);
    }

    private void LateUpdate()
    {
        if (sequenceStart > 0f)
        {
            SequenceTime = Time.time - sequenceStart;
        }
        ApplyCamera(false);
    }

    private void ApplyCamera(bool immediate)
    {
        if (shotCamera == null)
        {
            return;
        }
        bool exactRunnerFollow = runner != null && runner.Root.gameObject.activeInHierarchy;
        // Bobbing and tilting slightly shift the rendered bounds around the actor
        // root, so follow the visible body centre rather than only its root pivot.
        float framingX = exactRunnerFollow ? runner.Bounds.center.x + cameraFollowOffsetX : cameraFocusX;
        Vector3 targetPosition = new Vector3(framingX, 4.55f, -cameraDistance);
        Vector3 lookTarget = new Vector3(framingX, 3.15f, 0f);
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - targetPosition, Vector3.up);
        if (immediate || exactRunnerFollow)
        {
            cameraBasePosition = targetPosition;
            cameraBaseRotation = targetRotation;
        }
        else
        {
            cameraBasePosition = Vector3.Lerp(cameraBasePosition, targetPosition, 1f - Mathf.Exp(-18f * Time.deltaTime));
            cameraBaseRotation = Quaternion.Slerp(cameraBaseRotation, targetRotation, 1f - Mathf.Exp(-18f * Time.deltaTime));
        }
        shotCamera.transform.SetPositionAndRotation(cameraBasePosition, cameraBaseRotation);
        if (cameraShake > 0f)
        {
            cameraShake = Mathf.Max(0f, cameraShake - Time.deltaTime);
            float amount = cameraShake * 0.22f;
            shotCamera.transform.position += shotCamera.transform.right * Mathf.Sin(Time.time * 71f) * amount;
            shotCamera.transform.position += shotCamera.transform.up * Mathf.Sin(Time.time * 53f) * amount * 0.5f;
        }

        // The centred invariant applies to the travelling shot. The deliberate
        // pre-impact composition moves 100 right to reveal the victim.
        if (exactRunnerFollow && cameraShake <= 0f && CollisionFraming < 0.1f)
        {
            Vector3 viewport = shotCamera.WorldToViewportPoint(runner.Bounds.center);
            HundredStayedCentered &= Mathf.Abs(viewport.x - 0.5f) < 0.035f;
        }
    }

    private bool ValidateReferences()
    {
        bool valid = hundredPrefab != null && tenPrefab != null && fiftyPrefab != null &&
            thousandPrefab != null && shotCamera != null && unitBurst != null &&
            unitBurst.onePrefab != null && flashMaterial != null;
        if (!valid)
        {
            Debug.LogError("[S86] Character, camera, burst or effect references are incomplete.", this);
        }
        return valid;
    }

    private static void SetFace(Actor actor, int activeIndex)
    {
        actor.PrimaryFace.gameObject.SetActive(activeIndex == 0);
        actor.SecondaryFace.gameObject.SetActive(activeIndex == 1);
        actor.TertiaryFace.gameObject.SetActive(activeIndex == 2);
    }

    private static Transform FindChild(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child;
            }
        }
        return null;
    }

    public static Bounds BoundsOf(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
            {
                continue;
            }
            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        return bounds;
    }

    private static Bounds BoundsOf(BoxCollider collider)
    {
        if (collider == null)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }
        Vector3 extents = collider.size * 0.5f;
        bool found = false;
        Bounds bounds = new Bounds();
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = collider.center + Vector3.Scale(extents, new Vector3(
                (i & 1) == 0 ? -1f : 1f,
                (i & 2) == 0 ? -1f : 1f,
                (i & 4) == 0 ? -1f : 1f));
            Vector3 world = collider.transform.TransformPoint(corner);
            if (!found)
            {
                bounds = new Bounds(world, Vector3.zero);
                found = true;
            }
            else
            {
                bounds.Encapsulate(world);
            }
        }
        return bounds;
    }

    private static float Smooth(float value)
    {
        return value * value * (3f - 2f * value);
    }
}
