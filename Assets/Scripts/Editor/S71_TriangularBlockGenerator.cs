using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Генератор треугольных числа-блоков для Scene_71 ("Восхождение треугольных чисел").
///
/// Треугольные числа: T_k = k*(k+1)/2  ->  1, 3, 6, 10, 15, 21, 28, 36, 45, 55.
/// Каждый блок собирается из единичных кубов в форме симметричной "пирамидки":
///   ряд 0 (низ) — k кубов, ряд 1 — (k-1), ... вершина — 1 куб (по центру).
///
/// ВАЖНО (чтобы не поломать существующие блоки):
///  - все префабы кладутся в ОТДЕЛЬНУЮ папку  Assets/Prefabs/Blocks/Triangular/
///  - материалы цвета и лица КОПИРУЮТСЯ в Triangular/Materials/ и назначаются копии,
///    поэтому правка цвета/текстуры треугольника не затрагивает оригинальные 1/3/6/10.
///
/// Для 1, 3, 6, 10 цвет+лицо берутся из готовых префабов (их копии).
/// Для 15, 21, 28, 36, 45, 55 готовых ассетов нет — генерируется уникальный цвет
/// (копия базового материала с новым оттенком) и подставляется лицо-заготовка.
/// Такие числа помечаются в имени материала суффиксом "_placeholder" — заменишь текстуру позже.
///
/// Запуск: меню  Tools/Scene_71/Generate Triangular Blocks
/// </summary>
public static class S71_TriangularBlockGenerator
{
    const string BlocksDir = "Assets/Prefabs/Blocks";
    const string OutDir    = "Assets/Prefabs/Blocks/Triangular";
    const string MatDir    = "Assets/Prefabs/Blocks/Triangular/Materials";

    // Сколько шагов генерировать (10 -> до 55).
    const int Steps = 10;

    // Для чисел без готовых ассетов: у какого числа брать базовый материал-донор (форма/шейдер),
    // и лицо-заготовку. Берём "10" — крупное число с цветом и лицом.
    const int DonorNumber = 10;

    [MenuItem("Tools/Scene_71/Generate Triangular Blocks")]
    public static void Generate()
    {
        EnsureFolders();

        var generated = new List<string>();
        for (int k = 1; k <= Steps; k++) {
            int number = k * (k + 1) / 2;
            string path = BuildOne(k, number);
            if (path != null) generated.Add($"T{number}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[S71] Готово. Сгенерировано {generated.Count} треугольных блоков в {OutDir}: {string.Join(", ", generated)}");
        EditorUtility.DisplayDialog("Scene_71",
            $"Сгенерировано {generated.Count} треугольных блоков:\n{string.Join(", ", generated)}\n\nПапка: {OutDir}", "OK");
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(OutDir))
            AssetDatabase.CreateFolder(BlocksDir, "Triangular");
        if (!AssetDatabase.IsValidFolder(MatDir))
            AssetDatabase.CreateFolder(OutDir, "Materials");
    }

    // ---------- построение одного треугольного блока ----------

    static string BuildOne(int k, int number)
    {
        Material body, face;
        ResolveMaterials(number, out body, out face);
        if (body == null) {
            Debug.LogWarning($"[S71] Не удалось получить материал тела для T{number} — пропуск.");
            return null;
        }

        var root = new GameObject($"T{number}");
        SafeSetTag(root, "Block");

        // Кубы треугольника. ВЛОЖЕННАЯ раскладка со сдвигом ряда на 0.5:
        //   cube(r,c).x = r*0.5 + c + 0.5 ,  y = r + 0.5 ,  пивот в (0,0,0) — левый нижний угол.
        // Каждый ряд симметричен относительно центра (x = k/2), поэтому фигура выглядит как
        // ровная пирамидка; при этом T(k-1) ТОЧНО совпадает с левой-нижней частью T(k),
        // так что новые единички стыкуются впритык (перфект-нест), без сдвига при слиянии.
        for (int r = 0; r < k; r++) {
            int n = k - r;                    // кубов в ряду
            for (int c = 0; c < n; c++) {
                float x = r * 0.5f + c + 0.5f;
                float y = r + 0.5f;
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"cube_r{r}_c{c}";
                cube.transform.SetParent(root.transform, false);
                cube.transform.localPosition = new Vector3(x, y, 0f);
                cube.GetComponent<MeshRenderer>().sharedMaterial = body;
                var col = cube.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);   // физика блоку не нужна
            }
        }

        // Одно общее лицо на переднем нижнем ряду по центру (front = -Z), x = k/2.
        if (face != null) {
            var faceQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            faceQuad.name = "Face";
            faceQuad.transform.SetParent(root.transform, false);
            // Встроенный Quad лицевой стороной смотрит в -Z, а камера в Scene_71 тоже с -Z,
            // поэтому поворот НЕ нужен (identity). С поворотом на 180° лицо смотрело бы внутрь блока.
            faceQuad.transform.localPosition = new Vector3(k * 0.5f, 0.5f, -0.501f);
            faceQuad.transform.localRotation = Quaternion.identity;
            float fs = Mathf.Clamp(k * 0.6f, 1f, 2.4f);
            faceQuad.transform.localScale = new Vector3(fs, fs, 1f);
            faceQuad.GetComponent<MeshRenderer>().sharedMaterial = face;
            var qcol = faceQuad.GetComponent<Collider>();
            if (qcol != null) Object.DestroyImmediate(qcol);
        }

        // Маркер числа (Scale тут неприменим — форма не параллелепипед).
        var tb = root.AddComponent<TriangularBlock>();
        tb.step = k;
        tb.number = number;

        // Кинематический Rigidbody — как у остальных блоков в проекте.
        var rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        string prefabPath = $"{OutDir}/T{number}.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return prefabPath;
    }

    // ---------- материалы: копии в Triangular/Materials ----------

    static void ResolveMaterials(int number, out Material body, out Material face)
    {
        body = null;
        face = null;

        // 0) ГЛАВНОЕ: если треугольный префаб уже существует — ПЕРЕИСПОЛЬЗУЕМ его текущие
        //    материалы (цвет и лицо) как есть. Так все твои правки материалов/лиц сохраняются:
        //    файлы .mat не копируются и не перетираются, меняется только геометрия.
        var already = AssetDatabase.LoadAssetAtPath<GameObject>($"{OutDir}/T{number}.prefab");
        if (already != null) {
            var faceT = already.transform.Find("Face");
            if (faceT != null) {
                var fr = faceT.GetComponent<MeshRenderer>();
                if (fr != null) face = fr.sharedMaterial;
            }
            foreach (var mr in already.GetComponentsInChildren<MeshRenderer>(true)) {
                if (mr.gameObject.name.StartsWith("cube_")) { body = mr.sharedMaterial; break; }
            }
            if (body == null) {                       // подстраховка
                Material b2, f2; ExtractFromPrefab(already, out b2, out f2);
                body = b2; if (face == null) face = f2;
            }
            if (body != null) return;                 // используем как есть, ничего не создаём
        }

        // 1) Первый раз: берём цвет+лицо из готового префаба числа (1/3/6/10...).
        var src = AssetDatabase.LoadAssetAtPath<GameObject>($"{BlocksDir}/{number}.prefab");
        if (src != null) {
            Material srcBody, srcFace;
            ExtractFromPrefab(src, out srcBody, out srcFace);
            if (srcBody != null) body = CopyMaterial(srcBody, $"T{number}_body");
            if (srcFace != null) face = CopyMaterial(srcFace, $"T{number}_face");
            if (body != null) return;
        }

        // 2) Готового ассета нет — генерируем уникальный цвет из донора + лицо-заготовку.
        var donor = AssetDatabase.LoadAssetAtPath<GameObject>($"{BlocksDir}/{DonorNumber}.prefab");
        if (donor != null) {
            Material dBody, dFace;
            ExtractFromPrefab(donor, out dBody, out dFace);
            if (dBody != null) {
                body = CopyMaterial(dBody, $"T{number}_body_placeholder");
                body.color = HueFor(number);   // уникальный оттенок
            }
            if (dFace != null) {
                face = CopyMaterial(dFace, $"T{number}_face_placeholder");
            }
        }
    }

    /// <summary>Тело = самый частый материал среди граней; лицо = материал со словом "face".</summary>
    static void ExtractFromPrefab(GameObject prefab, out Material body, out Material face)
    {
        body = null;
        face = null;
        var counts = new Dictionary<Material, int>();
        foreach (var mr in prefab.GetComponentsInChildren<MeshRenderer>(true)) {
            var m = mr.sharedMaterial;
            if (m == null) continue;
            if (m.name.ToLower().Contains("face") && face == null) face = m;
            counts.TryGetValue(m, out int cnt);
            counts[m] = cnt + 1;
        }
        int best = -1;
        foreach (var kv in counts) {
            bool isFace = kv.Key == face;
            if (kv.Value > best && (!isFace || counts.Count == 1)) {
                best = kv.Value;
                body = kv.Key;
            }
        }
        // если лицо не нашли по имени — берём наименее частый (одиночную грань)
        if (face == null && counts.Count > 1) {
            int worst = int.MaxValue;
            foreach (var kv in counts) {
                if (kv.Key != body && kv.Value < worst) { worst = kv.Value; face = kv.Key; }
            }
        }
    }

    static Material CopyMaterial(Material src, string newName)
    {
        string dst = $"{MatDir}/{newName}.mat";
        string srcPath = AssetDatabase.GetAssetPath(src);
        // Если материал — самостоятельный .mat-ассет, копируем файл (сохраняем шейдер/настройки).
        if (!string.IsNullOrEmpty(srcPath) && srcPath.EndsWith(".mat")) {
            AssetDatabase.DeleteAsset(dst); // перезапись при повторной генерации
            if (AssetDatabase.CopyAsset(srcPath, dst))
                return AssetDatabase.LoadAssetAtPath<Material>(dst);
        }
        // Иначе — создаём новый ассет как клон.
        var clone = new Material(src);
        AssetDatabase.DeleteAsset(dst);
        AssetDatabase.CreateAsset(clone, dst);
        return clone;
    }

    /// <summary>Стабильный уникальный оттенок для чисел без готового цвета.</summary>
    static Color HueFor(int number)
    {
        // Золотое сечение по кругу оттенков — цвета заметно различаются.
        float h = (number * 0.61803398875f) % 1f;
        return Color.HSVToRGB(h, 0.72f, 0.95f);
    }

    static void SafeSetTag(GameObject go, string tag)
    {
        try { go.tag = tag; }
        catch { /* тег отсутствует в проекте — оставляем Untagged */ }
    }
}
