using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene 77: an 8:30 obstacle-course episode for One, Ten, and Hundred.
/// Each chapter has a fixed slot so the complete timeline is exactly 510 seconds.
/// </summary>
public class S77_Main : MonoBehaviour
{
    public const float EpisodeDuration = 510f;

    [Header("Original Numberblocks")]
    public GameObject onePrefab;
    public GameObject tenPrefab;
    public GameObject hundredPrefab;

    [Header("Scene-local transparent Hundred faces")]
    public Material cleanBackMaterial;
    public Material surprisedFaceMaterial;
    public Material determinedFaceMaterial;
    public Material sadFaceMaterial;

    [Header("Obstacle chapters (in timeline order)")]
    public GameObject[] obstacleRoots;
    public Transform spinnerBars;
    public Transform seesawDeck;
    public Transform liftingBridge;

    [Header("On-screen text")]
    public TextMesh stageTitle;
    public TextMesh caption;
    public TextMesh[] stageTitleOutline;
    public TextMesh[] captionOutline;

    [Header("Optional existing audio")]
    public AudioClip musicClip;
    public AudioClip impactClip;
    public AudioClip splitClip;
    public AudioClip successClip;

    [Header("Framing")]
    public float groundY = -3.05f;
    public float oneHeight = 1.15f;
    public float tenHeight = 4.75f;
    public float hundredHeight = 5.65f;

    public float EpisodeTime { get; private set; }
    public bool EpisodeComplete { get; private set; }
    public int CurrentChapter { get; private set; }

    private const string FrontBodyName = "S77 Front Body";
    private const string FaceOverlayName = "S77 Face Overlay";

    private enum StreamMode
    {
        Straight,
        Hoop,
        Weave,
        Stones,
        Spinner,
        Tunnel
    }

    private readonly List<GameObject> fragments = new List<GameObject>(100);
    private readonly List<Vector3> fragmentStarts = new List<Vector3>(100);
    private readonly List<Vector3> fragmentScales = new List<Vector3>(100);

    private Camera shotCamera;
    private AudioSource effectsSource;
    private AudioSource musicSource;
    private GameObject one;
    private GameObject ten;
    private GameObject hundred;
    private Renderer hundredFront;
    private Renderer hundredOverlay;
    private Material hundredOriginalFace;
    private float episodeStartTime;
    private float spinnerAngle;

    private IEnumerator Start()
    {
        ResolveEditorReferences();
        if (!ValidateReferences())
        {
            yield break;
        }

        shotCamera = Camera.main;
        if (shotCamera == null)
        {
            Debug.LogError("[S77] Scene needs a camera tagged MainCamera.", this);
            yield break;
        }

        ConfigureAudio();
        SetAllObstacles(false);
        SetText("NUMBERBLOCKS OBSTACLE COURSE", "ONE • TEN • ONE HUNDRED");
        EpisodeComplete = false;
        CurrentChapter = 0;
        episodeStartTime = Time.time;

        yield return RunFixedSlot(35f, IntroChapter());
        yield return RunFixedSlot(60f, LowBeamChapter());
        yield return RunFixedSlot(60f, HoopChapter());
        yield return RunFixedSlot(60f, ZigzagChapter());
        yield return RunFixedSlot(65f, SteppingStonesChapter());
        yield return RunFixedSlot(60f, SpinnerChapter());
        yield return RunFixedSlot(60f, BalanceBridgeChapter());
        yield return RunFixedSlot(70f, FinalTunnelChapter());
        yield return RunFixedSlot(40f, FinaleChapter());

        float remaining = EpisodeDuration - EpisodeTime;
        if (remaining > 0f)
        {
            yield return Hold(remaining);
        }

        EpisodeTime = EpisodeDuration;
        EpisodeComplete = true;
        Debug.Log("[S77] COMPLETE — the 8:30 obstacle-course episode reached 510 seconds.", this);
    }

    private void Update()
    {
        if (!EpisodeComplete)
        {
            EpisodeTime = Mathf.Min(EpisodeDuration, Time.time - episodeStartTime);
        }

        if (spinnerBars != null && spinnerBars.gameObject.activeInHierarchy)
        {
            spinnerAngle += 42f * Time.deltaTime;
            spinnerBars.localRotation = Quaternion.Euler(0f, 0f, spinnerAngle);
        }

        if (obstacleRoots != null && obstacleRoots.Length == 7)
        {
            for (int i = 0; i < obstacleRoots.Length; i++)
            {
                bool shouldBeVisible = CurrentChapter >= 1 && CurrentChapter <= 7 &&
                    i == CurrentChapter - 1;
                if (obstacleRoots[i] != null && obstacleRoots[i].activeSelf != shouldBeVisible)
                {
                    obstacleRoots[i].SetActive(shouldBeVisible);
                }
            }
        }
    }

    private IEnumerator RunFixedSlot(float duration, IEnumerator chapter)
    {
        float start = Time.time;
        yield return chapter;

        float remaining = duration - (Time.time - start);
        if (remaining > 0f)
        {
            yield return Hold(remaining);
        }
        else if (remaining < -0.1f)
        {
            Debug.LogWarning($"[S77] Chapter {CurrentChapter} exceeded its slot by {-remaining:F2}s.", this);
        }
    }

    private IEnumerator IntroChapter()
    {
        CurrentChapter = 0;
        SetAllObstacles(false);
        SpawnLineup();
        SetText("NUMBERBLOCKS OBSTACLE COURSE", "Seven challenges. Three very different sizes!");
        yield return Hold(5f);

        SetText("MEET THE TEAM", "ONE is small and quick");
        yield return Pulse(one, 5f, 0.34f);
        SetText("MEET THE TEAM", "TEN is tall: 2 blocks wide and 5 blocks high");
        yield return Pulse(ten, 6f, 0.28f);
        SetText("MEET THE TEAM", "ONE HUNDRED can become 100 ones");
        yield return Pulse(hundred, 6f, 0.24f);

        SetText("READY?", "The obstacle course begins!");
        yield return GroupBounce(5f);
        yield return Hold(3f);
    }

    private IEnumerator LowBeamChapter()
    {
        CurrentChapter = 1;
        ActivateObstacle(0);
        SpawnLineup(true);
        SetText("CHALLENGE 1 — THE LOW BEAM", "Who can get underneath?");
        yield return Hold(5f);

        ClearActors();
        yield return null;
        one = SpawnActor(onePrefab, "One — Low Beam", oneHeight, -7.2f);
        SetText("THE LOW BEAM", "ONE walks straight underneath");
        yield return Hold(2f);
        yield return WalkToX(one, 7.1f, 6f, 0.08f);
        yield return Celebrate(one, 2f);

        ClearActors();
        yield return null;
        ten = SpawnActor(tenPrefab, "Ten — Low Beam", tenHeight, -7.2f);
        SetText("THE LOW BEAM", "TEN is too tall!");
        yield return WalkToX(ten, StopCenterBeforeObstacle(ten, 0), 4.5f, 0.12f);
        Play(impactClip);
        yield return Impact(ten, 1.6f);
        SetText("10 = 10 ONES", "Split, line up, and go!");
        yield return SplitActor(ten, onePrefab, 10, 2, 5, 1.6f);
        yield return StreamFragments(7.1f, 7.5f, StreamMode.Straight);
        yield return Hold(2f);

        ClearActors();
        yield return null;
        hundred = SpawnActor(hundredPrefab, "Hundred — Low Beam", hundredHeight, -7.2f);
        SetHundredExpression(null);
        SetText("THE LOW BEAM", "ONE HUNDRED cannot fit either");
        yield return WalkToX(hundred, StopCenterBeforeObstacle(hundred, 0), 4.5f, 0.1f);
        Play(impactClip);
        SetHundredExpression(surprisedFaceMaterial);
        yield return Impact(hundred, 1.6f);
        yield return Hold(2f);
        SetHundredExpression(determinedFaceMaterial);
        SetText("100 = 100 ONES", "A hundred tiny runners!");
        yield return Pulse(hundred, 1.8f, 0.12f);
        yield return SplitActor(hundred, onePrefab, 100, 10, 10, 1.8f);
        yield return StreamFragments(7.2f, 9f, StreamMode.Straight);
        SetText("CHALLENGE 1 COMPLETE", "Small pieces make a big difference");
        yield return Hold(3f);
    }

    private IEnumerator HoopChapter()
    {
        CurrentChapter = 2;
        ActivateObstacle(1);
        SpawnLineup(true);
        SetText("CHALLENGE 2 — THE JUMPING HOOP", "Up, through, and down!");
        yield return Hold(5f);

        ClearActors();
        yield return null;
        one = SpawnActor(onePrefab, "One — Hoop", oneHeight, -7.2f);
        SetText("THE JUMPING HOOP", "ONE takes a flying leap");
        yield return JumpThroughHoop(one, 7f, 6.5f, 2.8f);
        yield return Celebrate(one, 2f);

        ClearActors();
        yield return null;
        ten = SpawnActor(tenPrefab, "Ten — Hoop", tenHeight, -7.2f);
        SetText("THE JUMPING HOOP", "TEN turns sideways in the air");
        yield return JumpThroughHoop(ten, 7f, 9f, 3.25f, true);
        yield return Celebrate(ten, 2f);

        ClearActors();
        yield return null;
        hundred = SpawnActor(hundredPrefab, "Hundred — Hoop", hundredHeight, -7.2f);
        SetText("THE JUMPING HOOP", "ONE HUNDRED tries... but the hoop is too small");
        yield return JumpThroughHoop(hundred, StopCenterBeforeObstacle(hundred, 1), 6f, 1.2f);
        Play(impactClip);
        SetHundredExpression(surprisedFaceMaterial);
        yield return Impact(hundred, 1.4f);
        SetHundredExpression(sadFaceMaterial);
        SetText("OH NO!", "A sad moment... then a clever idea");
        yield return Hold(4.5f);
        SetHundredExpression(determinedFaceMaterial);
        yield return Pulse(hundred, 2f, 0.15f);
        SetText("100 = 100 ONES", "One hundred little jumps");
        yield return SplitActor(hundred, onePrefab, 100, 10, 10, 1.8f);
        yield return StreamFragments(7.2f, 10f, StreamMode.Hoop);
        SetText("CHALLENGE 2 COMPLETE", "Everyone made it through!");
        yield return Hold(3f);
    }

    private IEnumerator ZigzagChapter()
    {
        CurrentChapter = 3;
        ActivateObstacle(2);
        SpawnLineup(true);
        SetText("CHALLENGE 3 — ZIGZAG GATES", "Left, right, left!");
        yield return Hold(5f);

        ClearActors();
        yield return null;
        one = SpawnActor(onePrefab, "One — Zigzag", oneHeight, -7.2f);
        SetText("ZIGZAG GATES", "ONE dances around every post");
        yield return ZigzagAcross(one, 8.5f, 7f, 0.75f, 0f);
        yield return Celebrate(one, 2f);

        ClearActors();
        yield return null;
        ten = SpawnActor(tenPrefab, "Ten — Zigzag", tenHeight, -7.2f);
        SetText("ZIGZAG GATES", "TEN leans into every turn");
        yield return ZigzagAcross(ten, 8.5f, 10f, 0.6f, 13f);
        yield return Celebrate(ten, 2f);

        ClearActors();
        yield return null;
        hundred = SpawnActor(hundredPrefab, "Hundred — Zigzag", hundredHeight, -7.2f);
        SetText("ZIGZAG GATES", "ONE HUNDRED is much too wide");
        yield return WalkToX(hundred, StopCenterBeforeObstacle(hundred, 2), 4f, 0.08f);
        SetHundredExpression(surprisedFaceMaterial);
        yield return Impact(hundred, 1.3f);
        SetHundredExpression(determinedFaceMaterial);
        SetText("100 = 10 TENS", "Ten tall teammates take turns");
        yield return SplitActor(hundred, tenPrefab, 10, 5, 2, 2f);
        yield return StreamFragments(8.5f, 12f, StreamMode.Weave);
        SetText("CHALLENGE 3 COMPLETE", "Ten tens make one hundred");
        yield return Hold(4f);
    }

    private IEnumerator SteppingStonesChapter()
    {
        CurrentChapter = 4;
        ActivateObstacle(3);
        SpawnLineup(true);
        SetText("CHALLENGE 4 — STEPPING STONES", "Do not splash into the water!");
        yield return Hold(6f);

        ClearActors();
        yield return null;
        one = SpawnActor(onePrefab, "One — Stones", oneHeight, -7.2f);
        SetText("STEPPING STONES", "ONE hops: one, two, three, four");
        yield return SteppingStoneCross(one, 7.2f, 9f, 0.8f);
        yield return Celebrate(one, 2f);

        ClearActors();
        yield return null;
        ten = SpawnActor(tenPrefab, "Ten — Stones", tenHeight, -7.2f);
        SetText("STEPPING STONES", "TEN wobbles... and finds balance");
        yield return SteppingStoneCross(ten, 7.2f, 12f, 1.05f, true);
        yield return Celebrate(ten, 2f);

        ClearActors();
        yield return null;
        hundred = SpawnActor(hundredPrefab, "Hundred — Stones", hundredHeight, -7.2f);
        SetText("STEPPING STONES", "The stones cannot hold ONE HUNDRED");
        yield return WalkToX(hundred, -2.5f, 4f, 0.08f);
        SetHundredExpression(sadFaceMaterial);
        yield return Hold(3f);
        SetHundredExpression(determinedFaceMaterial);
        SetText("BUILD A MOVING BRIDGE", "100 ones hop from stone to stone");
        yield return SplitActor(hundred, onePrefab, 100, 10, 10, 1.8f);
        yield return StreamFragments(7.3f, 13f, StreamMode.Stones);
        SetText("CHALLENGE 4 COMPLETE", "Many little steps cross a big gap");
        yield return Hold(4f);
    }

    private IEnumerator SpinnerChapter()
    {
        CurrentChapter = 5;
        ActivateObstacle(4);
        SpawnLineup(true);
        SetText("CHALLENGE 5 — SPINNING BARS", "Watch... wait... GO!");
        yield return Hold(6f);

        ClearActors();
        yield return null;
        one = SpawnActor(onePrefab, "One — Spinner", oneHeight, -7.2f);
        SetText("SPINNING BARS", "ONE waits for the perfect gap");
        yield return Hold(3f);
        yield return WalkToX(one, 7.2f, 5f, 0.12f);
        yield return Celebrate(one, 2f);

        ClearActors();
        yield return null;
        ten = SpawnActor(tenPrefab, "Ten — Spinner", tenHeight, -7.2f);
        SetText("SPINNING BARS", "TEN ducks under one bar and leans past the next");
        yield return DuckAndDash(ten, 7.2f, 10f);
        yield return Celebrate(ten, 2f);

        ClearActors();
        yield return null;
        hundred = SpawnActor(hundredPrefab, "Hundred — Spinner", hundredHeight, -7.2f);
        SetText("SPINNING BARS", "There is no gap big enough for ONE HUNDRED");
        yield return WalkToX(hundred, -3.1f, 4f, 0.08f);
        SetHundredExpression(surprisedFaceMaterial);
        Play(impactClip);
        yield return Impact(hundred, 1.5f);
        SetHundredExpression(determinedFaceMaterial);
        SetText("100 = 100 ONES", "A stream of ones slips between the bars");
        yield return SplitActor(hundred, onePrefab, 100, 10, 10, 1.8f);
        yield return StreamFragments(7.3f, 12f, StreamMode.Spinner);
        SetText("CHALLENGE 5 COMPLETE", "Timing beats size");
        yield return Hold(4f);
    }

    private IEnumerator BalanceBridgeChapter()
    {
        CurrentChapter = 6;
        ActivateObstacle(5);
        SpawnLineup(true);
        ResetBridge();
        SetText("CHALLENGE 6 — THE BALANCE BRIDGE", "Two buttons lift the bridge");
        yield return Hold(6f);

        SetText("THE BALANCE BRIDGE", "ONE presses the first button");
        yield return WalkToX(one, 0f, 4f, 0.08f);
        yield return PressPose(one, 2f);
        yield return LiftBridge(0.25f, 2f);
        SetText("NOT ENOUGH WEIGHT", "The bridge only moves a little");
        yield return Hold(2f);

        SetText("THE BALANCE BRIDGE", "TEN presses the second button");
        yield return WalkToX(ten, 1.5f, 4f, 0.08f);
        yield return PressPose(ten, 2f);
        yield return LiftBridge(0.65f, 3f);
        SetText("ALMOST!", "Both buttons need help");
        yield return Hold(2f);

        SetText("TEAMWORK", "ONE and TEN hold the buttons — ONE HUNDRED crosses");
        SetHundredExpression(determinedFaceMaterial);
        yield return LiftBridge(1f, 3f);
        yield return WalkAcrossRaisedBridge(hundred, 7.2f, 9f);
        SetHundredExpression(null);
        yield return Celebrate(hundred, 3f);

        SetText("NOW SWITCH!", "ONE HUNDRED holds the bridge for ONE and TEN");
        PlaceOnGround(hundred, 1.5f);
        yield return PressPose(hundred, 2f);
        PlaceOnGround(one, -7.1f);
        PlaceOnGround(ten, -5.4f);
        yield return WalkPairAcross(one, ten, 7.2f, 9f);
        SetText("CHALLENGE 6 COMPLETE", "Different sizes are useful in different ways");
        yield return Hold(4f);
    }

    private IEnumerator FinalTunnelChapter()
    {
        CurrentChapter = 7;
        ActivateObstacle(6);
        SpawnLineup(true);
        SetText("FINAL CHALLENGE — THE SHRINKING TUNNEL", "Each doorway is smaller than the last");
        yield return Hold(7f);

        ClearActors();
        yield return null;
        one = SpawnActor(onePrefab, "One — Final Tunnel", oneHeight, -7.2f);
        SetText("THE SHRINKING TUNNEL", "Every doorway fits ONE");
        yield return WalkToX(one, 8.6f, 8f, 0.09f);
        yield return Celebrate(one, 2f);

        ClearActors();
        yield return null;
        ten = SpawnActor(tenPrefab, "Ten — Final Tunnel", tenHeight, -7.2f);
        SetText("THE SHRINKING TUNNEL", "TEN reaches the smallest doorway");
        yield return WalkToX(ten, 5f, 7f, 0.1f);
        Play(impactClip);
        yield return Impact(ten, 1.5f);
        SetText("10 = 10 ONES", "Ten ones race through the final opening");
        yield return SplitActor(ten, onePrefab, 10, 2, 5, 1.6f);
        yield return StreamFragments(8.6f, 8f, StreamMode.Tunnel);

        ClearActors();
        yield return null;
        hundred = SpawnActor(hundredPrefab, "Hundred — Final Tunnel", hundredHeight, -7.2f);
        SetText("THE SHRINKING TUNNEL", "ONE HUNDRED cannot enter the first doorway");
        yield return WalkToX(hundred, StopCenterBeforeObstacle(hundred, 6), 5f, 0.08f);
        SetHundredExpression(surprisedFaceMaterial);
        Play(impactClip);
        yield return Impact(hundred, 1.5f);
        SetHundredExpression(sadFaceMaterial);
        SetText("ONE LAST TRY", "Feeling sad is okay. Then we make a new plan.");
        yield return Hold(4f);
        SetHundredExpression(determinedFaceMaterial);
        yield return Pulse(hundred, 2.5f, 0.16f);
        SetText("100 = 100 ONES", "The whole team streams through!");
        yield return SplitActor(hundred, onePrefab, 100, 10, 10, 2f);
        yield return StreamFragments(8.6f, 13f, StreamMode.Tunnel);
        SetText("FINAL CHALLENGE COMPLETE", "All seven obstacles cleared!");
        yield return Hold(4f);
    }

    private IEnumerator FinaleChapter()
    {
        CurrentChapter = 8;
        SetAllObstacles(false);
        SpawnLineup();
        SetText("OBSTACLE COURSE COMPLETE!", "ONE • TEN • ONE HUNDRED");
        Play(successClip);
        yield return GroupBounce(7f);
        yield return Hold(4f);

        SetText("SMALL CAN SLIP THROUGH", "ONE finds the tiniest spaces");
        yield return Pulse(one, 5f, 0.36f);
        SetText("TALL CAN REACH HIGH", "TEN bends, balances, and jumps");
        yield return Pulse(ten, 5f, 0.28f);
        SetText("BIG CAN BECOME MANY", "ONE HUNDRED shares the work");
        SetHundredExpression(determinedFaceMaterial);
        yield return Pulse(hundred, 5f, 0.24f);
        SetHundredExpression(null);

        SetText("NUMBERBLOCKS OBSTACLE COURSE", "Great teamwork — see you next time!");
        yield return GroupBounce(6f);
        yield return Hold(5f);
    }

    private void SpawnLineup(bool beforeObstacle = false)
    {
        ClearActors();
        float oneX = beforeObstacle ? -8.7f : -7.1f;
        float tenX = beforeObstacle ? -7.1f : -3.9f;
        float hundredX = beforeObstacle ? -3.4f : 1.35f;
        one = SpawnActor(onePrefab, "One — Main", oneHeight, oneX);
        ten = SpawnActor(tenPrefab, "Ten — Main", tenHeight, tenX);
        hundred = SpawnActor(hundredPrefab, "Hundred — Main", hundredHeight, hundredX);
        SetHundredExpression(null);
    }

    private GameObject SpawnActor(GameObject prefab, string actorName, float height, float x)
    {
        GameObject actor = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
        actor.name = actorName;
        PrepareForAnimation(actor);
        ScaleToHeight(actor, height);
        PlaceOnGround(actor, x);

        if (prefab == hundredPrefab)
        {
            hundred = actor;
            hundredFront = FindRenderer(actor, FrontBodyName);
            hundredOverlay = FindRenderer(actor, FaceOverlayName);
            hundredOriginalFace = hundredFront != null ? hundredFront.sharedMaterial : null;
        }

        return actor;
    }

    private void ClearActors()
    {
        ClearFragments();
        DeactivateAndDestroy(one);
        DeactivateAndDestroy(ten);
        DeactivateAndDestroy(hundred);
        one = null;
        ten = null;
        hundred = null;
        hundredFront = null;
        hundredOverlay = null;
        hundredOriginalFace = null;
    }

    private static void DeactivateAndDestroy(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(false);
            Destroy(target);
        }
    }

    private IEnumerator SplitActor(
        GameObject source,
        GameObject fragmentPrefab,
        int count,
        int columns,
        int rows,
        float duration)
    {
        ClearFragments();
        Bounds sourceBounds = BoundsOf(source);
        float cellWidth = sourceBounds.size.x / columns;
        float cellHeight = sourceBounds.size.y / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns && fragments.Count < count; column++)
            {
                GameObject fragment = Instantiate(fragmentPrefab, Vector3.zero, Quaternion.identity, transform);
                fragment.name = $"Split Piece {fragments.Count + 1:000}";
                PrepareForAnimation(fragment);

                Bounds fragmentBounds = BoundsOf(fragment);
                float fit = 0.82f * Mathf.Min(
                    cellWidth / Mathf.Max(0.001f, fragmentBounds.size.x),
                    cellHeight / Mathf.Max(0.001f, fragmentBounds.size.y));
                fragment.transform.localScale *= fit;
                fragmentBounds = BoundsOf(fragment);

                Vector3 cellCenter = new Vector3(
                    sourceBounds.min.x + (column + 0.5f) * cellWidth,
                    sourceBounds.min.y + (row + 0.5f) * cellHeight,
                    -0.18f);
                fragment.transform.position += cellCenter - fragmentBounds.center;
                fragments.Add(fragment);
                fragmentStarts.Add(fragment.transform.position);
                fragmentScales.Add(fragment.transform.localScale);
                fragment.SetActive(false);
            }
        }

        Play(splitClip);
        source.SetActive(false);
        for (int i = 0; i < fragments.Count; i++)
        {
            fragments[i].SetActive(true);
        }
        yield return DropFragmentsToGround(duration);
    }

    private IEnumerator DropFragmentsToGround(float duration)
    {
        int count = fragments.Count;
        Vector3[] landingPositions = new Vector3[count];
        float landingRight = LandingRightForChapter();
        for (int i = 0; i < count; i++)
        {
            GameObject fragment = fragments[i];
            Bounds bounds = BoundsOf(fragment);
            float landingX;
            float landingBottom;
            if (count <= 10)
            {
                landingX = landingRight - (count - 1 - i) * bounds.size.x * 1.03f;
                landingBottom = groundY;
            }
            else
            {
                const int columns = 18;
                int column = i % columns;
                int visualRow = (i / columns) % 3;
                int depthLayer = i / (columns * 3);
                landingX = landingRight - column * bounds.size.x * 0.93f + depthLayer * 0.035f;
                landingBottom = groundY + visualRow * bounds.size.y * 0.92f;
            }
            landingPositions[i] = fragment.transform.position + new Vector3(
                landingX - bounds.center.x,
                landingBottom - bounds.min.y,
                DepthLayerOffset(i, count));
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            float eased = t * t;
            for (int i = 0; i < count; i++)
            {
                GameObject fragment = fragments[i];
                fragment.transform.position = Vector3.Lerp(fragmentStarts[i], landingPositions[i], eased);
                fragment.transform.rotation = Quaternion.Euler(
                    0f,
                    0f,
                    Mathf.Sin(t * Mathf.PI + i * 0.37f) * (1f - t) * 3f);
                ClampAboveGround(fragment);
            }
            yield return null;
        }

        for (int i = 0; i < count; i++)
        {
            fragments[i].transform.position = landingPositions[i];
            fragments[i].transform.rotation = Quaternion.identity;
            ClampAboveGround(fragments[i]);
            fragmentStarts[i] = fragments[i].transform.position;
        }
    }

    private float LandingRightForChapter()
    {
        switch (CurrentChapter)
        {
            case 1: return 0.05f;
            case 2: return 0.45f;
            case 3: return 0.05f;
            case 4: return -0.8f;
            case 5: return -0.35f;
            case 7: return -0.8f;
            default: return -0.1f;
        }
    }

    private static float DepthLayerOffset(int index, int count)
    {
        if (count <= 10)
        {
            return -index * 0.002f;
        }
        return -(index / 54) * 0.015f;
    }

    private IEnumerator StreamFragments(float endX, float duration, StreamMode mode)
    {
        int count = fragments.Count;
        Vector3[] destinations = new Vector3[count];
        Vector3[] centerOffsets = new Vector3[count];
        float[] phase = new float[count];
        float maxDelay = count > 20 ? 2.2f : 1.1f;
        System.Random random = new System.Random(7700 + CurrentChapter * 101 + count);

        for (int i = 0; i < count; i++)
        {
            destinations[i] = PositionWithBottom(
                fragments[i],
                endX + Mathf.Lerp(-0.6f, 0.6f, (float)random.NextDouble()),
                groundY);
            centerOffsets[i] = BoundsOf(fragments[i]).center - fragments[i].transform.position;
            phase[i] = (float)random.NextDouble() * Mathf.PI * 2f;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < count; i++)
            {
                GameObject fragment = fragments[i];
                if (fragment == null || !fragment.activeSelf)
                {
                    continue;
                }

                float delay = count <= 1 ? 0f : maxDelay * i / (count - 1f);
                float t = Mathf.Clamp01((elapsed - delay) / Mathf.Max(0.001f, duration - maxDelay));
                if (t <= 0f)
                {
                    continue;
                }

                fragment.transform.position = FragmentPath(
                    fragmentStarts[i], destinations[i], t, phase[i], centerOffsets[i], mode);
                fragment.transform.rotation = Quaternion.Euler(
                    0f, 0f, Mathf.Sin(t * Mathf.PI * 8f + phase[i]) * 7f);
                fragment.transform.localScale = fragmentScales[i];
                ClampAboveGround(fragment);
            }
            yield return null;
        }

        ClearFragments();
    }

    private Vector3 FragmentPath(
        Vector3 start,
        Vector3 end,
        float t,
        float phase,
        Vector3 centerOffset,
        StreamMode mode)
    {
        float eased = Mathf.SmoothStep(0f, 1f, t);
        Vector3 position = Vector3.Lerp(start, end, eased);

        if (mode == StreamMode.Hoop)
        {
            Vector3 hoop = new Vector3(3.05f, -0.25f + Mathf.Sin(phase) * 0.38f, -0.18f) -
                centerOffset;
            Vector3 incomingControl = hoop + new Vector3(-2f, 1.25f, 0f);
            Vector3 outgoingControl = hoop + new Vector3(2f, -1.25f, 0f);
            position = t < 0.6f
                ? QuadraticBezier(start, incomingControl, hoop, t / 0.6f)
                : QuadraticBezier(hoop, outgoingControl, end, (t - 0.6f) / 0.4f);
        }
        else if (mode == StreamMode.Weave)
        {
            position.y += Mathf.Abs(Mathf.Sin(t * Mathf.PI * 10f + phase * 0.08f)) * 0.11f;
        }
        else if (mode == StreamMode.Stones)
        {
            float overWater = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.08f, 0.22f, t)) *
                (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.78f, 0.94f, t)));
            position.y += overWater * 0.75f +
                Mathf.Abs(Mathf.Sin(t * Mathf.PI * 8f + phase * 0.18f)) * 0.62f;
        }
        else if (mode == StreamMode.Spinner)
        {
            position.y += Mathf.Abs(Mathf.Sin(t * Mathf.PI * 9f + phase * 0.08f)) * 0.12f;
        }
        else if (mode == StreamMode.Tunnel)
        {
            position.y += Mathf.Abs(Mathf.Sin(t * Mathf.PI * 6f + phase * 0.12f)) * 0.3f;
            position.z = Mathf.Lerp(-0.18f, 1.2f, t);
            position.y = Mathf.Lerp(position.y, groundY + 0.18f, Mathf.SmoothStep(0.58f, 0.9f, t));
        }
        else
        {
            position.y += Mathf.Abs(Mathf.Sin(t * Mathf.PI * 5f + phase * 0.08f)) * 0.22f;
        }

        return position;
    }

    private void ClearFragments()
    {
        for (int i = 0; i < fragments.Count; i++)
        {
            DeactivateAndDestroy(fragments[i]);
        }
        fragments.Clear();
        fragmentStarts.Clear();
        fragmentScales.Clear();
    }

    private IEnumerator WalkToX(GameObject actor, float endX, float duration, float bounce)
    {
        if (actor == null)
        {
            yield break;
        }

        Vector3 start = actor.transform.position;
        Vector3 end = start + Vector3.right * (endX - BoundsOf(actor).center.x);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            float eased = Mathf.SmoothStep(0f, 1f, t);
            actor.transform.position = Vector3.Lerp(start, end, eased) +
                Vector3.up * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 10f)) * bounce;
            actor.transform.rotation = Quaternion.Euler(
                0f, 0f, Mathf.Sin(t * Mathf.PI * 10f) * bounce * 18f);
            ClampAboveGround(actor);
            yield return null;
        }
        actor.transform.position = end;
        actor.transform.rotation = Quaternion.identity;
        ClampAboveGround(actor);
    }

    private IEnumerator JumpThroughHoop(
        GameObject actor,
        float endX,
        float duration,
        float height,
        bool turnSideways = false)
    {
        Vector3 startCenter = BoundsOf(actor).center;
        Vector3 endCenter = new Vector3(endX, startCenter.y, startCenter.z);
        float elapsed = 0f;

        if (endX < 0f)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
                Vector3 desiredCenter = Vector3.Lerp(startCenter, endCenter, Mathf.SmoothStep(0f, 1f, t)) +
                    Vector3.up * Mathf.Sin(t * Mathf.PI) * height;
                actor.transform.rotation = Quaternion.Euler(0f, 0f, -Mathf.Sin(t * Mathf.PI) * 5f);
                MoveCenterTo(actor, desiredCenter);
                ClampAboveGround(actor);
                yield return null;
            }
            actor.transform.rotation = Quaternion.identity;
            MoveCenterTo(actor, endCenter);
            ClampAboveGround(actor);
            yield break;
        }

        const float crossingTime = 0.6f;
        Vector3 hoopCenter = new Vector3(3.05f, -0.25f, -0.05f);
        Vector3 incomingControl = hoopCenter + new Vector3(-2.15f, height * 0.55f, 0f);
        Vector3 outgoingControl = hoopCenter + new Vector3(2.15f, -height * 0.55f, 0f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            Vector3 desiredCenter;
            if (t <= crossingTime)
            {
                desiredCenter = QuadraticBezier(
                    startCenter,
                    incomingControl,
                    hoopCenter,
                    t / crossingTime);
            }
            else
            {
                desiredCenter = QuadraticBezier(
                    hoopCenter,
                    outgoingControl,
                    endCenter,
                    (t - crossingTime) / (1f - crossingTime));
            }

            float turn = t <= crossingTime
                ? Mathf.SmoothStep(0f, 1f, t / crossingTime)
                : Mathf.SmoothStep(1f, 0f, (t - crossingTime) / (1f - crossingTime));
            actor.transform.rotation = Quaternion.Euler(
                0f,
                0f,
                turnSideways ? turn * 90f : Mathf.Sin(t * Mathf.PI * 2f) * 4f);
            MoveCenterTo(actor, desiredCenter);
            ClampAboveGround(actor);
            yield return null;
        }

        actor.transform.rotation = Quaternion.identity;
        MoveCenterTo(actor, endCenter);
        ClampAboveGround(actor);
    }

    private IEnumerator ZigzagAcross(GameObject actor, float endX, float duration, float amplitude, float lean)
    {
        Vector3 start = actor.transform.position;
        Vector3 end = start + Vector3.right * (endX - BoundsOf(actor).center.x);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            float wave = Mathf.Sin(t * Mathf.PI * 6f);
            actor.transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t)) +
                Vector3.up * Mathf.Abs(wave) * amplitude * Mathf.Sin(t * Mathf.PI);
            actor.transform.rotation = Quaternion.Euler(0f, 0f, -wave * lean);
            ClampAboveGround(actor);
            yield return null;
        }
        actor.transform.position = end;
        actor.transform.rotation = Quaternion.identity;
        ClampAboveGround(actor);
    }

    private IEnumerator SteppingStoneCross(
        GameObject actor,
        float endX,
        float duration,
        float hop,
        bool wobble = false)
    {
        float[] xPoints =
        {
            BoundsOf(actor).center.x, 0.1f, 1.75f, 3.4f, 5.05f, 6.7f, endX
        };
        float[] bottomPoints =
        {
            groundY, -2.32f, -2.32f, -2.32f, -2.32f, -2.32f, groundY
        };
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            float path = t * (xPoints.Length - 1);
            int segment = Mathf.Min(xPoints.Length - 2, Mathf.FloorToInt(path));
            float localT = path - segment;
            float desiredX = Mathf.Lerp(xPoints[segment], xPoints[segment + 1], localT);
            float desiredBottom = Mathf.Lerp(bottomPoints[segment], bottomPoints[segment + 1], localT) +
                Mathf.Sin(localT * Mathf.PI) * hop;
            actor.transform.rotation = Quaternion.Euler(
                0f, 0f, wobble ? Mathf.Sin(localT * Mathf.PI * 2f) * 8f : 0f);
            MoveBottomCenterTo(actor, desiredX, desiredBottom);
            ClampAboveGround(actor);
            yield return null;
        }
        actor.transform.rotation = Quaternion.identity;
        MoveBottomCenterTo(actor, endX, groundY);
        ClampAboveGround(actor);
    }

    private IEnumerator DuckAndDash(GameObject actor, float endX, float duration)
    {
        Vector3 start = actor.transform.position;
        Vector3 end = start + Vector3.right * (endX - BoundsOf(actor).center.x);
        Vector3 scale = actor.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            float duck = Mathf.Clamp01(Mathf.Sin(t * Mathf.PI * 3f));
            actor.transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
            actor.transform.localScale = Vector3.Scale(scale, new Vector3(1f + duck * 0.15f, 1f - duck * 0.28f, 1f));
            actor.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 5f) * 9f);
            ClampAboveGround(actor);
            yield return null;
        }
        actor.transform.position = end;
        actor.transform.localScale = scale;
        actor.transform.rotation = Quaternion.identity;
        ClampAboveGround(actor);
    }

    private IEnumerator WalkPairAcross(GameObject first, GameObject second, float endX, float duration)
    {
        float firstStartX = BoundsOf(first).center.x;
        float secondStartX = BoundsOf(second).center.x;
        float firstEndX = endX - 1.2f;
        float secondEndX = endX + 0.7f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 8f));
            float firstX = Mathf.Lerp(firstStartX, firstEndX, t);
            float secondX = Mathf.Lerp(secondStartX, secondEndX, t);
            MoveBottomCenterTo(first, firstX, BridgeSurfaceAt(firstX) + bounce * 0.1f);
            MoveBottomCenterTo(second, secondX, BridgeSurfaceAt(secondX) + bounce * 0.1f);
            ClampAboveGround(first);
            ClampAboveGround(second);
            yield return null;
        }
    }

    private IEnumerator WalkAcrossRaisedBridge(GameObject actor, float endX, float duration)
    {
        float startX = BoundsOf(actor).center.x;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            float x = Mathf.Lerp(startX, endX, t);
            float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 10f)) * 0.1f;
            actor.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 10f) * 1.8f);
            MoveBottomCenterTo(actor, x, BridgeSurfaceAt(x) + bounce);
            ClampAboveGround(actor);
            yield return null;
        }
        actor.transform.rotation = Quaternion.identity;
        MoveBottomCenterTo(actor, endX, groundY);
    }

    private float BridgeSurfaceAt(float x)
    {
        float rise = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(2f, 2.45f, x)) *
            (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(6.65f, 7.25f, x)));
        return groundY + rise * 0.34f;
    }

    private IEnumerator Impact(GameObject actor, float duration)
    {
        Vector3 position = actor.transform.position;
        Vector3 scale = actor.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            float bump = Mathf.Sin(t * Mathf.PI) * Mathf.Exp(-t * 1.8f);
            actor.transform.position = position + Vector3.left * bump * 0.35f;
            actor.transform.localScale = Vector3.Scale(scale, new Vector3(1f - bump * 0.08f, 1f + bump * 0.07f, 1f));
            actor.transform.rotation = Quaternion.Euler(0f, 0f, -bump * 4f);
            ClampAboveGround(actor);
            if (shotCamera != null)
            {
                shotCamera.transform.position = new Vector3(
                    Mathf.Sin(Time.time * 68f) * bump * 0.035f,
                    0.25f + Mathf.Cos(Time.time * 59f) * bump * 0.035f,
                    -20f);
            }
            yield return null;
        }
        actor.transform.position = position;
        actor.transform.localScale = scale;
        actor.transform.rotation = Quaternion.identity;
        ClampAboveGround(actor);
        ResetCamera();
    }

    private IEnumerator Pulse(GameObject actor, float duration, float amount)
    {
        if (actor == null)
        {
            yield break;
        }
        Vector3 position = actor.transform.position;
        Vector3 scale = actor.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.001f, duration));
            float pulse = Mathf.Sin(t * Mathf.PI * 4f) * Mathf.Sin(t * Mathf.PI);
            actor.transform.localScale = scale * (1f + pulse * amount * 0.18f);
            actor.transform.position = position + Vector3.up * Mathf.Abs(pulse) * amount;
            actor.transform.rotation = Quaternion.Euler(0f, 0f, pulse * 4f);
            ClampAboveGround(actor);
            yield return null;
        }
        actor.transform.position = position;
        actor.transform.localScale = scale;
        actor.transform.rotation = Quaternion.identity;
        ClampAboveGround(actor);
    }

    private IEnumerator Celebrate(GameObject actor, float duration)
    {
        Play(successClip);
        yield return Pulse(actor, duration, 0.34f);
    }

    private IEnumerator GroupBounce(float duration)
    {
        Vector3 oneStart = one != null ? one.transform.position : Vector3.zero;
        Vector3 tenStart = ten != null ? ten.transform.position : Vector3.zero;
        Vector3 hundredStart = hundred != null ? hundred.transform.position : Vector3.zero;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (one != null)
            {
                one.transform.position = oneStart + Vector3.up * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 6f)) * 0.42f;
            }
            if (ten != null)
            {
                ten.transform.position = tenStart + Vector3.up * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 6f + 0.6f)) * 0.32f;
            }
            if (hundred != null)
            {
                hundred.transform.position = hundredStart + Vector3.up * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 6f + 1.2f)) * 0.24f;
            }
            yield return null;
        }
        if (one != null) one.transform.position = oneStart;
        if (ten != null) ten.transform.position = tenStart;
        if (hundred != null) hundred.transform.position = hundredStart;
    }

    private IEnumerator PressPose(GameObject actor, float duration)
    {
        Vector3 scale = actor.transform.localScale;
        Vector3 position = actor.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float press = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            actor.transform.localScale = Vector3.Scale(scale, new Vector3(1f + press * 0.08f, 1f - press * 0.12f, 1f));
            actor.transform.position = position + Vector3.down * press * 0.15f;
            ClampAboveGround(actor);
            yield return null;
        }
        actor.transform.localScale = scale;
        actor.transform.position = position;
        ClampAboveGround(actor);
    }

    private IEnumerator LiftBridge(float amount, float duration)
    {
        if (liftingBridge == null || seesawDeck == null)
        {
            yield return Hold(duration);
            yield break;
        }

        Quaternion deckStart = seesawDeck.localRotation;
        Vector3 bridgeStart = liftingBridge.localPosition;
        Quaternion deckEnd = Quaternion.Euler(0f, 0f, Mathf.Lerp(-18f, 0f, amount));
        Vector3 bridgeEnd = new Vector3(bridgeStart.x, Mathf.Lerp(-4f, -2.88f, amount), bridgeStart.z);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            seesawDeck.localRotation = Quaternion.Slerp(deckStart, deckEnd, t);
            liftingBridge.localPosition = Vector3.Lerp(bridgeStart, bridgeEnd, t);
            yield return null;
        }
    }

    private void ResetBridge()
    {
        if (seesawDeck != null)
        {
            seesawDeck.localRotation = Quaternion.Euler(0f, 0f, -18f);
        }
        if (liftingBridge != null)
        {
            liftingBridge.localPosition = new Vector3(
                liftingBridge.localPosition.x,
                -4f,
                liftingBridge.localPosition.z);
        }
    }

    private void SetHundredExpression(Material expression)
    {
        if (hundredFront == null || hundredOverlay == null)
        {
            return;
        }

        bool showOverlay = expression != null;
        hundredFront.sharedMaterial = showOverlay ? cleanBackMaterial : hundredOriginalFace;
        hundredOverlay.sharedMaterial = expression != null ? expression : surprisedFaceMaterial;
        hundredOverlay.gameObject.SetActive(showOverlay);
    }

    private void ActivateObstacle(int index)
    {
        for (int i = 0; i < obstacleRoots.Length; i++)
        {
            if (obstacleRoots[i] != null)
            {
                obstacleRoots[i].SetActive(i == index);
            }
        }
        ResetCamera();
    }

    private float StopCenterBeforeObstacle(GameObject actor, int obstacleIndex)
    {
        if (actor == null)
        {
            return -7f;
        }
        if (obstacleRoots == null || obstacleIndex < 0 ||
            obstacleIndex >= obstacleRoots.Length || obstacleRoots[obstacleIndex] == null)
        {
            return BoundsOf(actor).center.x;
        }

        Bounds actorBounds = BoundsOf(actor);
        Bounds obstacleBounds = BoundsOf(obstacleRoots[obstacleIndex]);
        return obstacleBounds.min.x - actorBounds.extents.x - 0.04f;
    }

    private void SetAllObstacles(bool active)
    {
        if (obstacleRoots == null)
        {
            return;
        }
        for (int i = 0; i < obstacleRoots.Length; i++)
        {
            if (obstacleRoots[i] != null)
            {
                obstacleRoots[i].SetActive(active);
            }
        }
    }

    private void SetText(string title, string subtitle)
    {
        if (stageTitle != null)
        {
            stageTitle.text = title;
        }
        if (caption != null)
        {
            caption.text = subtitle;
        }
        SetOutlineText(stageTitleOutline, title);
        SetOutlineText(captionOutline, subtitle);
    }

    private static void SetOutlineText(TextMesh[] outline, string value)
    {
        if (outline == null)
        {
            return;
        }
        for (int i = 0; i < outline.Length; i++)
        {
            if (outline[i] != null)
            {
                outline[i].text = value;
            }
        }
    }

    private IEnumerator Hold(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ConfigureAudio()
    {
        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.loop = true;
        musicSource.volume = 0.2f;
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    private void Play(AudioClip clip)
    {
        if (effectsSource != null && clip != null)
        {
            effectsSource.PlayOneShot(clip);
        }
    }

    private static void PrepareForAnimation(GameObject target)
    {
        Rigidbody[] bodies = target.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].useGravity = false;
            bodies[i].isKinematic = true;
            bodies[i].detectCollisions = false;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    private static void ScaleToHeight(GameObject actor, float targetHeight)
    {
        Bounds bounds = BoundsOf(actor);
        actor.transform.localScale *= targetHeight / Mathf.Max(0.001f, bounds.size.y);
    }

    private void PlaceOnGround(GameObject actor, float x)
    {
        Bounds bounds = BoundsOf(actor);
        actor.transform.position += new Vector3(x - bounds.center.x, groundY - bounds.min.y, -bounds.center.z);
    }

    private static void MoveCenterTo(GameObject actor, Vector3 desiredCenter)
    {
        Bounds bounds = BoundsOf(actor);
        actor.transform.position += desiredCenter - bounds.center;
    }

    private static void MoveBottomCenterTo(GameObject actor, float desiredX, float desiredBottom)
    {
        Bounds bounds = BoundsOf(actor);
        actor.transform.position += new Vector3(
            desiredX - bounds.center.x,
            desiredBottom - bounds.min.y,
            0f);
    }

    private void ClampAboveGround(GameObject actor)
    {
        Bounds bounds = BoundsOf(actor);
        if (bounds.min.y < groundY)
        {
            actor.transform.position += Vector3.up * (groundY - bounds.min.y);
        }
    }

    private static Vector3 PositionWithBottom(GameObject actor, float x, float y)
    {
        Bounds bounds = BoundsOf(actor);
        return actor.transform.position + new Vector3(x - bounds.center.x, y - bounds.min.y, -0.18f - bounds.center.z);
    }

    private void ResetCamera()
    {
        if (shotCamera != null)
        {
            shotCamera.orthographic = true;
            shotCamera.orthographicSize = 5.55f;
            shotCamera.transform.position = new Vector3(0f, 0.25f, -20f);
            shotCamera.transform.rotation = Quaternion.identity;
        }
    }

    private static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float inverse = 1f - t;
        return inverse * inverse * a + 2f * inverse * t * b + t * t * c;
    }

    private static Renderer FindRenderer(GameObject root, string objectName)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject.name == objectName)
            {
                return renderers[i];
            }
        }
        return null;
    }

    private static Bounds BoundsOf(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        Bounds bounds = new Bounds(target.transform.position, Vector3.one);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].gameObject.activeInHierarchy)
            {
                continue;
            }
            if (!found)
            {
                bounds = renderers[i].bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }
        return bounds;
    }

    private bool ValidateReferences()
    {
        List<string> missing = new List<string>();
        if (onePrefab == null) missing.Add(nameof(onePrefab));
        if (tenPrefab == null) missing.Add(nameof(tenPrefab));
        if (hundredPrefab == null) missing.Add(nameof(hundredPrefab));
        if (cleanBackMaterial == null) missing.Add(nameof(cleanBackMaterial));
        if (surprisedFaceMaterial == null) missing.Add(nameof(surprisedFaceMaterial));
        if (determinedFaceMaterial == null) missing.Add(nameof(determinedFaceMaterial));
        if (sadFaceMaterial == null) missing.Add(nameof(sadFaceMaterial));
        if (obstacleRoots == null || obstacleRoots.Length != 7)
        {
            missing.Add(nameof(obstacleRoots));
        }
        else
        {
            for (int i = 0; i < obstacleRoots.Length; i++)
            {
                if (obstacleRoots[i] == null)
                {
                    missing.Add($"{nameof(obstacleRoots)}[{i}]");
                }
            }
        }
        if (stageTitle == null) missing.Add(nameof(stageTitle));
        if (caption == null) missing.Add(nameof(caption));

        if (missing.Count > 0)
        {
            Debug.LogError(
                $"[S77] Scene or character references are incomplete: {string.Join(", ", missing)}.",
                this);
        }
        return missing.Count == 0;
    }

    private void ResolveEditorReferences()
    {
#if UNITY_EDITOR
        onePrefab = LoadEditorAsset("Assets/Prefabs/Blocks/1.prefab", onePrefab);
        tenPrefab = LoadEditorAsset("Assets/Prefabs/Blocks/10.prefab", tenPrefab);
        hundredPrefab = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene77/Prefabs/Hundred_DoorRunner.prefab", hundredPrefab);
        cleanBackMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/materials/Materials/newMillion 1.mat", cleanBackMaterial);
        surprisedFaceMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene77/Materials/Hundred_Surprised_Transparent.mat",
            surprisedFaceMaterial);
        determinedFaceMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene77/Materials/Hundred_Determined_Transparent.mat",
            determinedFaceMaterial);
        sadFaceMaterial = LoadEditorAsset(
            "Assets/Prefabs/Blocks/Scene77/Materials/Hundred_Sad_Transparent.mat",
            sadFaceMaterial);
#endif
        ResolveSceneReferences();
    }

    private void ResolveSceneReferences()
    {
        string[] obstacleNames =
        {
            "Obstacle 1 — Low Beam",
            "Obstacle 2 — Jumping Hoop",
            "Obstacle 3 — Zigzag Gates",
            "Obstacle 4 — Stepping Stones",
            "Obstacle 5 — Spinning Bars",
            "Obstacle 6 — Balance Bridge",
            "Obstacle 7 — Shrinking Tunnel"
        };

        if (obstacleRoots == null || obstacleRoots.Length != obstacleNames.Length)
        {
            obstacleRoots = new GameObject[obstacleNames.Length];
        }
        for (int i = 0; i < obstacleNames.Length; i++)
        {
            if (obstacleRoots[i] == null)
            {
                obstacleRoots[i] = FindSceneGameObject(obstacleNames[i]);
            }
        }

        spinnerBars = spinnerBars != null
            ? spinnerBars
            : FindSceneTransform("Spinner Pivot");
        seesawDeck = seesawDeck != null
            ? seesawDeck
            : FindSceneTransform("Balance Deck Pivot");
        liftingBridge = liftingBridge != null
            ? liftingBridge
            : FindSceneTransform("Lifting Bridge");
        stageTitle = stageTitle != null
            ? stageTitle
            : FindSceneText("S77 Stage Title");
        caption = caption != null
            ? caption
            : FindSceneText("S77 Stage Caption");

        stageTitleOutline = ResolveOrCreateOutline(
            stageTitleOutline,
            stageTitle,
            "S77 Stage Title Outline",
            0.035f);
        captionOutline = ResolveOrCreateOutline(
            captionOutline,
            caption,
            "S77 Stage Caption Outline",
            0.03f);
    }

    private GameObject FindSceneGameObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].scene == gameObject.scene && objects[i].name == objectName)
            {
                return objects[i];
            }
        }
        return null;
    }

    private Transform FindSceneTransform(string objectName)
    {
        GameObject target = FindSceneGameObject(objectName);
        return target != null ? target.transform : null;
    }

    private TextMesh FindSceneText(string objectName)
    {
        GameObject target = FindSceneGameObject(objectName);
        return target != null ? target.GetComponent<TextMesh>() : null;
    }

    private TextMesh[] ResolveOrCreateOutline(
        TextMesh[] current,
        TextMesh source,
        string namePrefix,
        float thickness)
    {
        if (OutlineIsComplete(current))
        {
            return current;
        }

        List<TextMesh> existing = new List<TextMesh>();
        TextMesh[] sceneTexts = Resources.FindObjectsOfTypeAll<TextMesh>();
        for (int i = 0; i < sceneTexts.Length; i++)
        {
            if (sceneTexts[i].gameObject.scene == gameObject.scene &&
                sceneTexts[i].gameObject.name.StartsWith(namePrefix))
            {
                existing.Add(sceneTexts[i]);
            }
        }
        existing.Sort((left, right) => string.CompareOrdinal(left.gameObject.name, right.gameObject.name));
        if (existing.Count >= 8)
        {
            return existing.GetRange(0, 8).ToArray();
        }

        if (source == null)
        {
            return current;
        }

        Vector2[] directions =
        {
            new Vector2(-1f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, -1f), new Vector2(0f, 1f),
            new Vector2(-0.72f, -0.72f), new Vector2(-0.72f, 0.72f),
            new Vector2(0.72f, -0.72f), new Vector2(0.72f, 0.72f)
        };
        TextMesh[] created = new TextMesh[directions.Length];
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject outlineObject = Instantiate(source.gameObject, source.transform.parent);
            outlineObject.name = $"{namePrefix} Runtime {i + 1}";
            outlineObject.transform.position = source.transform.position + new Vector3(
                directions[i].x * thickness,
                directions[i].y * thickness,
                0.1f);
            TextMesh outline = outlineObject.GetComponent<TextMesh>();
            outline.color = new Color(0.055f, 0.09f, 0.16f);
            created[i] = outline;
        }
        return created;
    }

    private static bool OutlineIsComplete(TextMesh[] outline)
    {
        if (outline == null || outline.Length != 8)
        {
            return false;
        }
        for (int i = 0; i < outline.Length; i++)
        {
            if (outline[i] == null)
            {
                return false;
            }
        }
        return true;
    }

#if UNITY_EDITOR
    private static T LoadEditorAsset<T>(string path, T current) where T : Object
    {
        T expected = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        return expected != null ? expected : current;
    }
#endif
}
