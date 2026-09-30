using System.Collections;
using UnityEngine;

/// <summary>
/// Scene 73: "МИЛЛИОН против ВСЕХ".
/// Все степени десяти (1..100 000) объединяются против Миллиона,
/// складываются в башню... и получают всего 111 111.
/// Панчлайн: следующая степень десяти всегда больше суммы всех предыдущих.
///
/// Сценарий (всё автоматически, просто Play):
///   1. Интро — камера снизу вверх показывает огромный Миллион.
///   2. Команда — 1, 10, 100, 1000, 10 000, 100 000 по очереди прибегают в ряд.
///   3. Башня — блоки запрыгивают друг на друга, счётчик суммирует до 111 111.
///   4. Дуэль — общий план: башня 111 111 против Миллиона 1 000 000.
///   5. Финал — облёт камеры и вывод: "Миллион почти в 9 раз больше всех вместе!"
/// </summary>
public class S73_Main : MonoBehaviour
{
    [Header("Команда (по возрастанию: 1, 10, 100, 1000, 10000, 100000)")]
    public GameObject[] teamPrefabs;
    public long[] teamValues = { 1, 10, 100, 1000, 10000, 100000 };
    public string[] teamNames = { "ONE", "TEN", "HUNDRED", "THOUSAND", "TEN THOUSAND", "HUNDRED THOUSAND" };

    [Header("Миллион")]
    public GameObject millionPrefab;
    public long millionValue = 1000000;

    [Header("Расстановка")]
    public Vector3 millionPosition = new Vector3(60f, 0f, 0f); // где стоит Миллион
    public Vector3 rowStart = new Vector3(-45f, 0f, 0f);       // левый край ряда команды
    public float rowSpacing = 3f;                              // отступ между блоками в ряду
    public float arriveFromDistance = 60f;                     // откуда прибегают блоки (слева)

    [Header("Тайминги (секунды)")]
    public float introDuration = 5f;      // показ Миллиона
    public float arriveDuration = 1.2f;   // прибытие одного блока
    public float arrivePause = 2.5f;      // пауза на блоке (показ подписи)
    public float jumpDuration = 0.9f;     // прыжок на башню
    public float stackPause = 0.9f;       // пауза после каждого прыжка
    public float showdownDuration = 6f;   // общий план "башня против Миллиона"
    public float finaleOrbitSpeed = 10f;  // градусов в секунду на финальном облёте

    [Header("Подписи")]
    public float labelSize = 1.6f;        // высота цифр в юнитах
    public Color labelColor = Color.white;
    public Color counterColor = new Color(1f, 0.85f, 0.2f);

    [Header("Звук")]
    public AudioClip arriveClip;   // прибытие блока
    public AudioClip landClip;     // приземление на башню
    public AudioClip revealClip;   // общий план дуэли
    public AudioClip finaleClip;   // финальный вывод
    public AudioClip musicClip;    // фоновая музыка (loop)
    [Range(0f, 1f)] public float musicVolume = 0.35f;

    private Camera cam;
    private AudioSource sfx;
    private AudioSource music;
    private Font labelFont;
    private TextMesh counter;      // счётчик суммы над башней
    private GameObject million;
    private GameObject[] team;
    private long shownSum;         // что сейчас показывает счётчик

    void Start()
    {
        cam = Camera.main;
        sfx = gameObject.AddComponent<AudioSource>();
        music = gameObject.AddComponent<AudioSource>();
        if (musicClip != null)
        {
            music.clip = musicClip;
            music.loop = true;
            music.volume = musicVolume;
            music.Play();
        }
        StartCoroutine(Director());
    }

    IEnumerator Director()
    {
        SpawnAll();

        yield return Intro();
        yield return TeamArrives();
        yield return BuildTower();
        yield return Showdown();
        yield return Finale();
    }

    // ---------- спавн ----------

    void SpawnAll()
    {
        million = Spawn(millionPrefab, millionPosition, "Million");
        // Лицо у префабов смотрит в -Z; поворачиваем лицом к башне (в -X)
        million.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        GroundAt(million, millionPosition);

        team = new GameObject[teamPrefabs.Length];
        for (int i = 0; i < teamPrefabs.Length; i++)
        {
            team[i] = Spawn(teamPrefabs[i], RowSlot(i) + Vector3.left * arriveFromDistance, "Team_" + teamValues[i]);
            // У префабов команды лицо смотрит в +Z (в отличие от Миллиона) — поворот к Миллиону (+X)
            team[i].transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            team[i].SetActive(false); // появляются по одному
        }
    }

    GameObject Spawn(GameObject prefab, Vector3 pos, string name)
    {
        GameObject go = prefab != null
            ? Instantiate(prefab, pos, Quaternion.identity, transform)
            : GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        return go;
    }

    /// <summary>Слот i-го блока в ряду: слева направо по возрастанию.
    /// Курсор идёт по левым краям, а вернуть надо ЦЕНТР блока — иначе широкие блоки наезжают на соседей.</summary>
    Vector3 RowSlot(int i)
    {
        float cursor = rowStart.x;
        for (int k = 0; k < i; k++)
            cursor += Width(k) + rowSpacing;
        return new Vector3(cursor + Width(i) * 0.5f, 0f, rowStart.z);
    }

    float Width(int k)
    {
        return Size(team != null && team[k] != null ? team[k] : teamPrefabs[k]).x;
    }

    /// <summary>Поставить объект основанием на землю в точке (x,z).</summary>
    void GroundAt(GameObject go, Vector3 pos)
    {
        Bounds b = GetBounds(go);
        go.transform.position += new Vector3(pos.x - b.center.x, -b.min.y, pos.z - b.center.z);
    }

    // ---------- фаза 1: интро ----------

    IEnumerator Intro()
    {
        Bounds mb = GetBounds(million);

        // Нижний ракурс у подножия Миллиона, камера медленно отъезжает и поднимается
        Vector3 lowPos = mb.center + new Vector3(-mb.size.x * 0.9f, -mb.extents.y * 0.8f, -mb.size.z * 0.9f);

        TextMesh title = MakeLabel(Format(millionValue) + "\nMILLION",
            new Vector3(mb.center.x, mb.max.y + labelSize * 2f, mb.center.z), labelSize * 3f, counterColor);

        // В финальный кадр должен влезть и Миллион целиком, и титул над ним
        Bounds shot = mb;
        shot.Encapsulate(new Vector3(mb.center.x, mb.max.y + labelSize * 12f, mb.center.z));
        Vector3 fullPos = FramePosition(shot, -135f, 1.6f);

        cam.transform.position = lowPos;
        cam.transform.LookAt(mb.center + Vector3.up * mb.extents.y * 0.5f);

        yield return Glide(lowPos, fullPos, mb.center, shot.center, introDuration);
        Destroy(title.gameObject);
    }

    // ---------- фаза 2: команда прибывает ----------

    IEnumerator TeamArrives()
    {
        for (int i = 0; i < team.Length; i++)
        {
            GameObject b = team[i];
            b.SetActive(true);
            GroundAt(b, RowSlot(i) + Vector3.left * arriveFromDistance);

            Vector3 from = b.transform.position;
            GroundAt(b, RowSlot(i));
            Vector3 to = b.transform.position;
            b.transform.position = from;

            // Камера встречает блок у его слота; в кадр должна влезть и подпись
            Bounds slotB = new Bounds(to + Vector3.up * Size(b).y * 0.5f, Size(b));
            float halfW = Mathf.Max(teamNames[i].Length, 9) * labelSize * 0.4f;
            slotB.Encapsulate(new Vector3(to.x - halfW, to.y + Size(b).y + labelSize * 4f, to.z));
            slotB.Encapsulate(new Vector3(to.x + halfW, to.y, to.z));
            CutTo(FramePosition(slotB, -160f, 1.6f), slotB.center);

            if (arriveClip != null) sfx.PlayOneShot(arriveClip);

            // Подбегает с лёгкими подскоками
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, arriveDuration);
                float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                Vector3 p = Vector3.Lerp(from, to, s);
                p.y += Mathf.Abs(Mathf.Sin(s * Mathf.PI * 3f)) * 0.6f * (1f - s);
                b.transform.position = p;
                yield return null;
            }
            b.transform.position = to;

            Bounds bb = GetBounds(b);
            TextMesh tag = MakeLabel(teamNames[i] + "\n" + Format(teamValues[i]),
                new Vector3(bb.center.x, bb.max.y + labelSize, bb.center.z), labelSize, labelColor);
            yield return new WaitForSeconds(arrivePause);
            Destroy(tag.gameObject);
        }

        // Общий взгляд на собравшуюся команду
        Bounds all = GetBounds(team[0]);
        foreach (GameObject g in team) all.Encapsulate(GetBounds(g));
        yield return Glide(cam.transform.position, FramePosition(all, -180f, 1.3f),
            GetBounds(team[team.Length - 1]).center, all.center, 2f);
        yield return new WaitForSeconds(1f);
    }

    // ---------- фаза 3: башня ----------

    IEnumerator BuildTower()
    {
        // Основание — самый большой (последний), остальные прыгают сверху по убыванию
        GameObject baseBlock = team[team.Length - 1];
        Bounds towerB = GetBounds(baseBlock);

        shownSum = teamValues[teamValues.Length - 1];
        counter = MakeLabel(Format(shownSum),
            new Vector3(towerB.center.x, towerB.max.y + labelSize * 2f, towerB.center.z),
            labelSize * 1.8f, counterColor);

        for (int i = team.Length - 2; i >= 0; i--)
        {
            GameObject b = team[i];
            Bounds bb = GetBounds(b);

            // Цель: центр по X/Z как у башни, основание на её вершине
            Vector3 target = new Vector3(
                towerB.center.x + (b.transform.position.x - bb.center.x),
                towerB.max.y + (b.transform.position.y - bb.min.y),
                towerB.center.z + (b.transform.position.z - bb.center.z));

            // Камера держит и башню, и прыгуна, и счётчик над башней
            Bounds shot = towerB;
            shot.Encapsulate(bb);
            shot.Encapsulate(new Bounds(new Vector3(towerB.center.x, towerB.max.y + bb.size.y, towerB.center.z), bb.size));
            shot.Encapsulate(new Vector3(towerB.center.x, towerB.max.y + bb.size.y + labelSize * 6f, towerB.center.z));
            CutTo(FramePosition(shot, -170f, 1.5f), shot.center);

            yield return Jump(b, target, towerB.max.y + bb.size.y);
            if (landClip != null) sfx.PlayOneShot(landClip);

            towerB.Encapsulate(GetBounds(b));
            yield return TickCounter(shownSum + teamValues[i], 0.5f);
            counter.transform.position = new Vector3(towerB.center.x, towerB.max.y + labelSize * 2f, towerB.center.z);
            yield return new WaitForSeconds(stackPause);
        }
    }

    IEnumerator Jump(GameObject b, Vector3 target, float peakY)
    {
        Vector3 from = b.transform.position;
        float height = Mathf.Max(peakY - Mathf.Max(from.y, target.y), 1.5f) + 2f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, jumpDuration);
            float s = Mathf.Clamp01(t);
            Vector3 p = Vector3.Lerp(from, target, s);
            p.y += height * 4f * s * (1f - s); // парабола
            b.transform.position = p;
            yield return null;
        }
        b.transform.position = target;
    }

    IEnumerator TickCounter(long to, float duration)
    {
        long from = shownSum;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            counter.text = Format((long)Mathf.Lerp(from, to, Mathf.Clamp01(t)));
            yield return null;
        }
        shownSum = to;
        counter.text = Format(to);
    }

    // ---------- фаза 4: дуэль ----------

    IEnumerator Showdown()
    {
        if (revealClip != null) sfx.PlayOneShot(revealClip);

        Bounds towerB = TowerBounds();
        Bounds mb = GetBounds(million);
        Bounds both = towerB;
        both.Encapsulate(mb);

        float topY = Mathf.Max(towerB.max.y, mb.max.y);
        // Размер титра — от ширины общего плана, иначе на отъезде он превращается в точку
        float vsSize = Mathf.Max(labelSize * 2.5f, both.size.x * 0.05f);
        TextMesh vs = MakeLabel(Format(shownSum) + "   VS   " + Format(millionValue),
            new Vector3(both.center.x, topY + labelSize * 5f, both.center.z),
            vsSize, counterColor);
        both.Encapsulate(new Vector3(both.center.x, topY + labelSize * 5f + vsSize * 2f, both.center.z));

        // Счётчик суммы над башней тоже укрупняем под общий план
        if (counter != null)
            SetLabelSize(counter, Mathf.Max(labelSize * 1.8f, both.size.x * 0.035f));

        // Медленный отъезд с показа башни на общий план с Миллионом
        yield return Glide(cam.transform.position, FramePosition(both, -180f, 1.35f),
            towerB.center, both.center, showdownDuration * 0.6f);
        yield return new WaitForSeconds(showdownDuration * 0.4f);
        Destroy(vs.gameObject);
    }

    // ---------- фаза 5: финал ----------

    IEnumerator Finale()
    {
        if (finaleClip != null) sfx.PlayOneShot(finaleClip);

        Bounds towerB = TowerBounds();
        Bounds mb = GetBounds(million);
        Bounds both = towerB;
        both.Encapsulate(mb);

        float topY = Mathf.Max(towerB.max.y, mb.max.y);
        float msgSize = Mathf.Max(labelSize * 2.2f, both.size.x * 0.04f);
        TextMesh msg = MakeLabel("MILLION IS BIGGER THAN ALL OF THEM TOGETHER\nALMOST 9 TIMES BIGGER!",
            new Vector3(both.center.x, topY + labelSize * 5f, both.center.z),
            msgSize, counterColor);
        both.Encapsulate(new Vector3(both.center.x, topY + labelSize * 5f + msgSize * 3.5f, both.center.z));

        // Бесконечный медленный облёт — обрезается при монтаже
        float radius = Vector3.Distance(FramePosition(both, -180f, 1.35f), both.center);
        float angle = 180f;
        while (true)
        {
            angle += finaleOrbitSpeed * Time.deltaTime;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
            Vector3 pos = both.center + dir * radius;
            pos.y = both.center.y + both.size.y * 0.35f;
            cam.transform.position = pos;
            cam.transform.LookAt(both.center);
            yield return null;
        }
    }

    // ---------- камера ----------

    /// <summary>Позиция, с которой bounds целиком помещается в кадр. yaw в градусах вокруг Y (0 = смотрим с +Z).</summary>
    Vector3 FramePosition(Bounds b, float yawDeg, float padding)
    {
        float halfSize = Mathf.Max(b.extents.y, b.extents.x * 0.7f, b.extents.z * 0.7f) * padding;
        float distance = halfSize / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float rad = yawDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(rad), 0.25f, Mathf.Cos(rad)).normalized;
        return b.center + dir * Mathf.Max(distance, b.extents.magnitude + 2f);
    }

    void CutTo(Vector3 pos, Vector3 look)
    {
        cam.transform.position = pos;
        cam.transform.LookAt(look);
    }

    IEnumerator Glide(Vector3 fromPos, Vector3 toPos, Vector3 fromLook, Vector3 toLook, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            cam.transform.position = Vector3.Lerp(fromPos, toPos, s);
            cam.transform.LookAt(Vector3.Lerp(fromLook, toLook, s));
            yield return null;
        }
    }

    // ---------- утилиты ----------

    Bounds TowerBounds()
    {
        Bounds b = GetBounds(team[team.Length - 1]);
        foreach (GameObject g in team) b.Encapsulate(GetBounds(g));
        return b;
    }

    Vector3 Size(GameObject go)
    {
        return go != null ? GetBounds(go).size : Vector3.one;
    }

    Bounds GetBounds(GameObject go)
    {
        Renderer[] rs = go.GetComponentsInChildren<Renderer>(false);
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        return b;
    }

    void SetLabelSize(TextMesh tm, float size)
    {
        tm.characterSize = size * 10f / tm.fontSize;
    }

    TextMesh MakeLabel(string text, Vector3 pos, float size, Color color)
    {
        if (labelFont == null)
        {
            try { labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { labelFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        }
        GameObject go = new GameObject("label");
        go.transform.SetParent(transform, false);
        go.transform.position = pos;

        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.font = labelFont;
        tm.fontSize = 80;
        tm.characterSize = size * 10f / tm.fontSize;
        tm.anchor = TextAnchor.LowerCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.color = color;
        go.GetComponent<MeshRenderer>().sharedMaterial = labelFont.material;
        go.AddComponent<Billboard>(); // всегда лицом к камере
        return tm;
    }

    class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            Camera c = Camera.main;
            if (c != null)
                transform.rotation = Quaternion.LookRotation(transform.position - c.transform.position);
        }
    }

    static string Format(long n)
    {
        return n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
