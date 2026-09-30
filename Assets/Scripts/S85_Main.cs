using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Scene 85: a landscape, unhurried chain of ten stomps at original prefab sizes.</summary>
public class S85_Main : MonoBehaviour
{
    public const string HappyFaceName = "S85 Happy Face";
    public const string SmugFaceName = "S85 Smug Face";
    public const string ScaredFaceName = "S85 Scared Face";

    [Serializable]
    public sealed class Character
    {
        public int Number;
        public GameObject Prefab;
        public Vector3Int UnitGrid;
    }

    public Character[] Characters;
    public Camera ShotCamera;
    public S85_UnitBurst UnitBurst;
    public AudioClip ApproachClip;
    public AudioClip StompClip;
    public AudioClip BurstClip;
    public AudioClip RunClip;
    public AudioClip MusicClip;

    [Header("Audio mix")]
    [Range(0f, 1f)] public float ApproachVolume = 0.25f;
    [Range(0f, 1f)] public float StompVolume = 0.95f;
    [Range(0f, 1f)] public float BurstVolume = 0.68f;
    [Range(0f, 1f)] public float RunVolume = 0.5f;
    [Range(0f, 1f)] public float MusicVolume = 0.2f;

    [Header("Seconds per story beat; escape waits for the actual last One")]
    public float HappyHold = 3.5f;
    public float ApproachDuration = 5f;
    public float JumpDuration = 2.4f;
    public float CrushDuration = 1.65f;
    public float WinnerRiseDuration = 5f;
    public float WinnerHold = 1f;
    public float FinalHold = 7f;

    public int RoundIndex { get; private set; }
    public int CompletedRounds { get; private set; }
    public int CurrentNumber { get; private set; }
    public string Beat { get; private set; }
    public float BeatTime { get; private set; }
    public float SequenceTime { get; private set; }
    public bool SequenceComplete { get; private set; }
    public bool ContactIsValid { get; private set; } = true;
    public bool ScalesAreOriginal { get; private set; } = true;
    public bool MusicIsPlaying => musicSource != null && musicSource.isPlaying;
    public int MusicLoopCount { get; private set; }
    public readonly List<int> EscapedNumbers = new List<int>(10);

    private Actor current;
    private Actor challenger;
    private AudioSource effectsSource;
    private AudioSource movementSource;
    private AudioSource musicSource;
    private float previousMusicTime;
    private float sequenceStart;
    private Vector3 cameraTarget;
    private float cameraSize;
    private float shakeStrength;
    private float shakeRemaining;
    private readonly Quaternion cameraRotation = Quaternion.Euler(12f, -24f, 0f);

    private sealed class Actor
    {
        public Transform Root;
        public Transform Visual;
        public Bounds LocalBounds;
        public Vector3 OriginalScale;
        public Transform Happy;
        public Transform Smug;
        public Transform Scared;
        public int Number;
        public Vector3 Size => LocalBounds.size;
        public Bounds WorldBounds => new Bounds(Root.position + LocalBounds.center, LocalBounds.size);
    }

    private IEnumerator Start()
    {
        if (ShotCamera == null || UnitBurst == null || Characters == null || Characters.Length != 11)
        {
            Debug.LogError("[S85] Camera, burst and all eleven characters are required.", this);
            yield break;
        }
        ShotCamera.aspect = 16f / 9f;
        ShotCamera.orthographic = false;
        ShotCamera.fieldOfView = 36f;
        ShotCamera.nearClipPlane = 0.1f;
        ShotCamera.farClipPlane = 10000f;
        // The shared bootstrap enables neon bloom on every camera; this film keeps the original face palette.
        foreach (UnityEngine.Rendering.PostProcessing.PostProcessLayer layer in ShotCamera.GetComponents<UnityEngine.Rendering.PostProcessing.PostProcessLayer>())
            layer.enabled = false;
        ShotCamera.allowHDR = false;
        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.volume = 1f;
        movementSource = gameObject.AddComponent<AudioSource>();
        movementSource.playOnAwake = false;
        movementSource.loop = true;
        movementSource.volume = RunVolume;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = MusicVolume;
        musicSource.clip = MusicClip;
        UnitBurst.Prepare();
        if (!UnitBurst.IsPrepared) yield break;

        sequenceStart = Time.time;
        if (MusicClip != null)
        {
            musicSource.Play();
            Debug.Log($"[S85_AUDIO] {MusicClip.name}: volume {MusicVolume:F2}, loop enabled, length {MusicClip.length:F2}s.");
        }
        current = SpawnActor(Characters[0]);
        if (current == null) yield break;
        CurrentNumber = current.Number;
        SetExpression(current, HappyFaceName);
        FrameBounds(current.WorldBounds, 1.7f, out cameraTarget, out cameraSize);
        ApplyCamera();

        for (RoundIndex = 0; RoundIndex < Characters.Length - 1; RoundIndex++)
        {
            if (RoundIndex == 0) yield return Hold("Happy", HappyHold);
            challenger = SpawnActor(Characters[RoundIndex + 1]);
            if (challenger == null) yield break;
            SetExpression(challenger, SmugFaceName);
            float gap = Mathf.Max(2f, Mathf.Min(current.Size.x, challenger.Size.x) * 0.45f);
            Vector3 approachEnd = current.Root.position + Vector3.right *
                (current.Size.x * 0.5f + challenger.Size.x * 0.5f + gap);
            Bounds pair = current.WorldBounds;
            pair.Encapsulate(new Bounds(approachEnd + challenger.LocalBounds.center, challenger.Size));
            // Reserve room for a genuine airborne hop without changing either character's size.
            float jumpHeight = Mathf.Max(2.5f, current.Size.y * 0.55f);
            pair.Encapsulate(new Vector3(approachEnd.x, challenger.Size.y + current.Size.y + jumpHeight, 0f));
            FrameBounds(pair, 1.22f, out Vector3 pairTarget, out float pairSize);
            float startX = pairTarget.x + pairSize * ShotCamera.aspect * 1.25f + challenger.Size.x;
            Vector3 approachStart = new Vector3(startX, 0f, 0f);
            challenger.Root.position = approachStart;
            Vector3 oldTarget = cameraTarget;
            float oldSize = cameraSize;
            PlayEffect(ApproachClip, ApproachVolume);

            yield return Animate("Approach", ApproachDuration, t =>
            {
                float ease = Smooth(t);
                cameraTarget = Vector3.Lerp(oldTarget, pairTarget, ease);
                cameraSize = Mathf.Lerp(oldSize, pairSize, ease);
                challenger.Root.position = Vector3.Lerp(approachStart, approachEnd, ease);
                challenger.Root.position += Vector3.up * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 7f)) *
                    Mathf.Min(0.7f, challenger.Size.y * 0.018f) * Mathf.Sin(t * Mathf.PI);
            });
            challenger.Root.position = approachEnd;
            SetExpression(current, ScaredFaceName);

            Vector3 landing = new Vector3(current.Root.position.x, current.Size.y, 0f);
            yield return Animate("Jump", JumpDuration, t =>
            {
                challenger.Root.position = Vector3.Lerp(approachEnd, landing, Smooth(t));
                challenger.Root.position += Vector3.up * Mathf.Sin(t * Mathf.PI) * jumpHeight;
            });
            challenger.Root.position = landing;
            VerifyContact(current.Size.y);
            PlayEffect(StompClip, StompVolume);
            shakeStrength = pairSize * 0.006f;
            shakeRemaining = 0.6f;
            Vector3 loserPosition = current.Root.position;
            yield return Animate("Crush", CrushDuration, t =>
            {
                float ease = Smooth(t);
                float compression = Mathf.Lerp(1f, 0.2f, ease);
                current.Root.localScale = new Vector3(1f + 0.22f * ease, compression, 1f + 0.12f * ease);
                challenger.Root.position = new Vector3(loserPosition.x, current.Size.y * compression, 0f);
                VerifyContact(current.Size.y * compression);
            });

            Bounds crushedBounds = BoundsOf(current.Visual.gameObject);
            UnitBurst.Begin(crushedBounds, Characters[RoundIndex].UnitGrid, current.Number, ShotCamera,
                -challenger.Size.z * 0.5f - 2f);
            current.Root.gameObject.SetActive(false);
            PlayEffect(BurstClip, BurstVolume);
            Vector3 pressedPosition = challenger.Root.position;
            yield return Animate("Burst", 0.8f, t =>
            {
                challenger.Root.position = Vector3.Lerp(pressedPosition, new Vector3(pressedPosition.x, 0f, 0f), Smooth(t));
            });
            SetExpression(challenger, HappyFaceName);
            VerifyOriginalScale(challenger);
            movementSource.clip = RunClip;
            if (RunClip != null) movementSource.Play();

            // Follow the real, unit-sized Ones at ground level when the giants are too large
            // to show individual cubes in the wide establishing shot.
            Vector3 escapeStartTarget = cameraTarget;
            float escapeStartSize = cameraSize;
            float escapeSize = Mathf.Clamp(current.Size.x * 0.8f + 6f, 7f, 38f);
            Vector3 escapeTarget = new Vector3(loserPosition.x, escapeSize * 0.62f, -challenger.Size.z * 0.5f);
            yield return Animate("EscapeReframe", 2.5f, t =>
            {
                cameraTarget = Vector3.Lerp(escapeStartTarget, escapeTarget, Smooth(t));
                cameraSize = Mathf.Lerp(escapeStartSize, Mathf.Min(escapeStartSize, escapeSize), Smooth(t));
            });
            UnitBurst.AllowEscapeCompletion = true;
            Vector3 recoveryStart = cameraTarget;
            float recoverySize = cameraSize;
            FrameBounds(challenger.WorldBounds, 1.65f, out Vector3 winnerTarget, out float winnerSize);
            // Start rising immediately after the descent, while the Ones keep running.
            yield return Animate("WinnerRise", WinnerRiseDuration, t =>
            {
                cameraTarget = Vector3.Lerp(recoveryStart, winnerTarget, Smooth(t));
                cameraSize = Mathf.Lerp(recoverySize, winnerSize, Smooth(t));
            });
            Debug.Log($"[S85] RISE {challenger.Number}: {UnitBurst.ActiveUnitCount} Ones still running.");
            yield return Hold("Winner", WinnerHold);
            SetBeat("Escape");
            float escapeStartTime = Time.time;
            // A new opponent still requires the actual last One to have left the frame.
            while (!UnitBurst.AllEscaped)
            {
                BeatTime = Time.time - escapeStartTime;
                yield return null;
            }
            movementSource.Stop();
            EscapedNumbers.Add(current.Number);
            Debug.Log($"[S85] CLEAR {current.Number}: {UnitBurst.LastVisibleCount} visible Ones, " +
                $"{UnitBurst.EscapedUnitCount} escaped; extra wait at the top {Time.time - escapeStartTime:F2}s; next character may enter.");
            UnitBurst.Hide();
            Destroy(current.Root.gameObject);
            current = challenger;
            challenger = null;
            CurrentNumber = current.Number;
            CompletedRounds++;
        }
        yield return Hold("Final", FinalHold);
        SequenceComplete = true;
        Beat = "Complete";
        Debug.Log($"[S85] COMPLETE: {CompletedRounds} stomps; original scales; " +
            $"every escape finished before the next round; duration {SequenceTime:F1}s.");
    }

    private Actor SpawnActor(Character character)
    {
        if (character.Prefab == null)
        {
            Debug.LogError($"[S85] Missing prefab for {character.Number}.", this);
            return null;
        }
        GameObject root = new GameObject($"S85 Numberblock {character.Number}");
        root.transform.SetParent(transform, false);
        GameObject visual = Instantiate(character.Prefab, root.transform);
        visual.transform.localPosition = Vector3.zero;
        foreach (MonoBehaviour behaviour in visual.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled = false;
        foreach (Animator animator in visual.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.useGravity = false;
        }
        Bounds bounds = BoundsOf(visual);
        visual.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        Actor actor = new Actor
        {
            Root = root.transform,
            Visual = visual.transform,
            LocalBounds = BoundsOf(visual),
            OriginalScale = character.Prefab.transform.localScale,
            Happy = FindChild(visual.transform, HappyFaceName),
            Smug = FindChild(visual.transform, SmugFaceName),
            Scared = FindChild(visual.transform, ScaredFaceName),
            Number = character.Number
        };
        if (actor.Happy == null || actor.Smug == null || actor.Scared == null)
        {
            Debug.LogError($"[S85] {character.Number} is missing an expression layer.", visual);
            Destroy(root);
            return null;
        }
        VerifyOriginalScale(actor);
        return actor;
    }

    private void VerifyOriginalScale(Actor actor)
    {
        bool valid = actor.Visual.localScale == actor.OriginalScale && actor.Root.localScale == Vector3.one;
        ScalesAreOriginal &= valid;
        if (!valid) Debug.LogError($"[S85] Original scale changed for {actor.Number}.", this);
    }

    private void VerifyContact(float expectedHeight)
    {
        Bounds attackerBounds = BoundsOf(challenger.Visual.gameObject);
        bool valid = Mathf.Abs(attackerBounds.min.y - expectedHeight) < 0.035f;
        ContactIsValid &= valid;
        if (!valid) Debug.LogError($"[S85] Stomp contact gap/intersection: {attackerBounds.min.y - expectedHeight}.", this);
    }

    private void SetBeat(string beat)
    {
        Beat = beat;
        BeatTime = 0f;
    }

    private IEnumerator Hold(string beat, float duration)
    {
        yield return Animate(beat, duration, null);
    }

    private IEnumerator Animate(string beat, float duration, Action<float> update)
    {
        SetBeat(beat);
        float start = Time.time;
        while (Time.time - start < duration)
        {
            BeatTime = Time.time - start;
            update?.Invoke(Mathf.Clamp01(BeatTime / duration));
            yield return null;
        }
        BeatTime = duration;
        update?.Invoke(1f);
    }

    private void LateUpdate()
    {
        SequenceTime = Time.time - sequenceStart;
        if (MusicIsPlaying)
        {
            float musicTime = musicSource.time;
            if (musicTime + 0.1f < previousMusicTime)
            {
                MusicLoopCount++;
                Debug.Log($"[S85_AUDIO] Music loop {MusicLoopCount} at sequence time {SequenceTime:F2}s.");
            }
            previousMusicTime = musicTime;
        }
        ApplyCamera();
    }

    private void ApplyCamera()
    {
        if (ShotCamera == null) return;
        Vector3 forward = cameraRotation * Vector3.forward;
        float distance = Mathf.Max(16f, cameraSize / Mathf.Tan(ShotCamera.fieldOfView * 0.5f * Mathf.Deg2Rad));
        ShotCamera.transform.SetPositionAndRotation(cameraTarget - forward * distance, cameraRotation);
        if (shakeRemaining > 0f)
        {
            shakeRemaining -= Time.deltaTime;
            ShotCamera.transform.position += ShotCamera.transform.right *
                Mathf.Sin(Time.time * 58f) * shakeStrength * Mathf.Clamp01(shakeRemaining / 0.6f);
        }
    }

    private void FrameBounds(Bounds bounds, float padding, out Vector3 target, out float size)
    {
        Quaternion inverse = Quaternion.Inverse(cameraRotation);
        Vector3 min = Vector3.one * float.PositiveInfinity;
        Vector3 max = Vector3.one * float.NegativeInfinity;
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = bounds.center + Vector3.Scale(bounds.extents,
                new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
            Vector3 projected = inverse * corner;
            min = Vector3.Min(min, projected);
            max = Vector3.Max(max, projected);
        }
        target = cameraRotation * ((min + max) * 0.5f);
        size = Mathf.Max((max.y - min.y) * 0.5f, (max.x - min.x) * 0.5f / (16f / 9f)) * padding + 1f;
    }

    private void PlayEffect(AudioClip clip, float volume)
    {
        if (clip != null) effectsSource.PlayOneShot(clip, volume);
    }

    private static float Smooth(float value) => value * value * (3f - 2f * value);

    private static void SetExpression(Actor actor, string expression)
    {
        actor.Happy.gameObject.SetActive(expression == HappyFaceName);
        actor.Smug.gameObject.SetActive(expression == SmugFaceName);
        actor.Scared.gameObject.SetActive(expression == ScaredFaceName);
    }

    private static Transform FindChild(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child;
        return null;
    }

    public static Bounds BoundsOf(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled) continue;
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
}
