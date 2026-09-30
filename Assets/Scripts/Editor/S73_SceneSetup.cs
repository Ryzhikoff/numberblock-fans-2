using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Быстрая настройка Scene_73 ("МИЛЛИОН против ВСЕХ").
/// Создаёт/открывает сцену, ставит камеру, свет, землю, GameManager с S73_Main
/// и автоматически подставляет префабы степеней десяти и звуки.
///
/// Порядок работы:
///   1) Tools/Scene_73/Quick Setup Scene
///   2) Play (или запись через Unity Recorder)
/// </summary>
public static class S73_SceneSetup
{
    const string ScenePath = "Assets/Scenes/Scene_73.unity";
    const string GroundMatPath = "Assets/Materials/Materials/S73_Ground.mat";

    static readonly string[] TeamPrefabPaths =
    {
        "Assets/Prefabs/Blocks/1.prefab",
        "Assets/Prefabs/Blocks/10.prefab",
        "Assets/Prefabs/Blocks/100.prefab",
        "Assets/Prefabs/Blocks/1000.prefab",
        "Assets/Prefabs/Blocks/prefabTenThousand.prefab",
        "Assets/Prefabs/Blocks/OneHundredThousand.prefab",
    };
    const string MillionPath = "Assets/Prefabs/Blocks/OneMillion.prefab";

    [MenuItem("Tools/Scene_73/Quick Setup Scene")]
    public static void QuickSetup()
    {
        // Сцена: открываем существующую или создаём новую
        Scene active = SceneManager.GetActiveScene();
        if (active.path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (System.IO.File.Exists(ScenePath))
                EditorSceneManager.OpenScene(ScenePath);
            else
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        // GameManager + режиссёр
        var gm = GameObject.Find("GameManager") ?? new GameObject("GameManager");
        var main = gm.GetComponent<S73_Main>() ?? gm.AddComponent<S73_Main>();

        // Обновляем сериализованные поля: сцена могла сохранить старые значения
        main.teamNames = new[] { "ONE", "TEN", "HUNDRED", "THOUSAND", "TEN THOUSAND", "HUNDRED THOUSAND" };
        main.arriveDuration = 1.2f;
        main.arrivePause = 2.5f;

        // Камера
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.farClipPlane = 20000f;
        cam.clearFlags = CameraClearFlags.Skybox;

        // Свет
        var light = Object.FindFirstObjectByType<Light>();
        if (light == null)
        {
            var lightGo = new GameObject("Directional Light");
            light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
        }
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Земля
        var ground = GameObject.Find("S73_Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "S73_Ground";
        }
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(1000f, 1f, 1000f); // 10 000 x 10 000 юнитов
        ground.GetComponent<Renderer>().sharedMaterial = GroundMaterial();

        // Префабы
        main.teamPrefabs = new GameObject[TeamPrefabPaths.Length];
        int found = 0;
        for (int i = 0; i < TeamPrefabPaths.Length; i++)
        {
            main.teamPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(TeamPrefabPaths[i]);
            if (main.teamPrefabs[i] != null) found++;
            else Debug.LogWarning($"[S73] Не найден префаб: {TeamPrefabPaths[i]} — назначь вручную.");
        }
        main.millionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MillionPath);
        if (main.millionPrefab == null)
            Debug.LogWarning($"[S73] Не найден {MillionPath} — назначь вручную.");

        // Звуки (что найдётся — остальное можно назначить вручную)
        main.arriveClip = LoadClip("Assets/Sound/collCube.wav");
        main.landClip = LoadClip("Assets/Sound/brick.mp3");
        main.revealClip = LoadClip("Assets/Sound/1000000.wav");
        main.finaleClip = LoadClip("Assets/Sound/million.wav");

        EditorUtility.SetDirty(gm);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);

        Debug.Log($"[S73] Сцена настроена и сохранена: {ScenePath}. Префабов команды: {found}/{TeamPrefabPaths.Length}. Нажми Play.");
        EditorUtility.DisplayDialog("Scene_73 — МИЛЛИОН против ВСЕХ",
            $"Готово.\nКоманда: {found}/{TeamPrefabPaths.Length}\nМиллион: {(main.millionPrefab != null ? "ок" : "НЕ НАЙДЕН")}\n\n" +
            "Нажми Play — весь ролик отыграет автоматически.\nФоновую музыку можно назначить в поле Music Clip.", "OK");
    }

    static AudioClip LoadClip(string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning($"[S73] Не найден звук: {path}");
        return clip;
    }

    static Material GroundMaterial()
    {
        var green = new Color(0.45f, 0.75f, 0.35f); // трава

        // Копируем ЦЕЛИКОМ дефолтный материал текущего рендер-пайплайна (шейдер + ключворды +
        // свойства) — материал, собранный вручную из Shader.Find, в URP рендерится розовым
        var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        Material template = rp != null ? rp.defaultMaterial : null;
        Material fresh = template != null
            ? new Material(template)
            : new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));

        var mat = AssetDatabase.LoadAssetAtPath<Material>(GroundMatPath);
        if (mat == null)
        {
            mat = fresh;
            AssetDatabase.CreateAsset(mat, GroundMatPath);
        }
        else
        {
            mat.shader = fresh.shader;
            mat.CopyPropertiesFromMaterial(fresh); // чинит уже созданный розовый материал
            Object.DestroyImmediate(fresh);
        }
        mat.color = green;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", green);
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        return mat;
    }
}
