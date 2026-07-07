using UnityEngine;

public class S70_Starfield : MonoBehaviour
{
    [Header("Stars")]
    public int starCount = 5000; // Number of stars
    public float starFieldRadius = 5000f; // Star field radius
    public float starMinSize = 0.5f; // Minimum star size
    public float starMaxSize = 3f; // Maximum star size

    [Header("Star Colors")]
    public Color[] starColors = new Color[]
    {
        Color.white,
        new Color(1f, 0.9f, 0.8f), // Warm
        new Color(0.8f, 0.9f, 1f), // Cold
        new Color(1f, 0.8f, 0.8f), // Reddish
        new Color(0.8f, 1f, 0.8f)  // Greenish
    };

    [Header("Nebulae")]
    public int nebulaCount = 20;
    public float nebulaSize = 500f;
    public Color[] nebulaColors = new Color[]
    {
        new Color(0.5f, 0f, 1f, 0.1f),    // Purple
        new Color(0f, 0.5f, 1f, 0.1f),    // Blue
        new Color(1f, 0f, 0.5f, 0.1f),    // Pink
        new Color(0f, 1f, 0.5f, 0.05f)    // Turquoise
    };

    [Header("Particles")]
    public int particleCount = 1000;
    public float particleSpeed = 0.5f;
    public float particleSize = 2f;

    private GameObject starField;
    private GameObject nebulaField;
    private ParticleSystem particles;

    void Start()
    {
        CreateStarField();
        CreateNebulae();
        CreateParticles();
    }

    void CreateStarField()
    {
        starField = new GameObject("StarField");
        starField.transform.SetParent(transform);

        // Создаём материал для звёзд
        Material starMaterial = new Material(Shader.Find("Unlit/Transparent"));
        starMaterial.color = Color.white;

        // Создаём меш для звёзд
        MeshFilter meshFilter = starField.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = starField.AddComponent<MeshRenderer>();
        meshRenderer.material = starMaterial;

        // Генерируем звёзды
        Vector3[] vertices = new Vector3[starCount];
        Color[] colors = new Color[starCount];
        int[] triangles = new int[starCount * 6];

        for (int i = 0; i < starCount; i++)
        {
            // Случайная позиция на сфере
            float theta = Random.Range(0f, Mathf.PI * 2f);
            float phi = Mathf.Acos(Random.Range(-1f, 1f));
            float radius = starFieldRadius * Random.Range(0.5f, 1f);

            vertices[i] = new Vector3(
                radius * Mathf.Sin(phi) * Mathf.Cos(theta),
                radius * Mathf.Sin(phi) * Mathf.Sin(theta),
                radius * Mathf.Cos(phi)
            );

            // Случайный цвет
            colors[i] = starColors[Random.Range(0, starColors.Length)];

            // Создаём треугольники для каждой звезды (billboard)
            int vertexIndex = i * 4;
            int triangleIndex = i * 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.colors = colors;
        meshRenderer.material.SetPass(0);

        // Используем Point Cloud
        mesh.vertices = vertices;
        mesh.colors = colors;
    }

    void CreateNebulae()
    {
        nebulaField = new GameObject("NebulaField");
        nebulaField.transform.SetParent(transform);

        for (int i = 0; i < nebulaCount; i++)
        {
            GameObject nebula = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nebula.name = $"Nebula_{i}";
            nebula.transform.SetParent(nebulaField.transform);

            // Случайная позиция
            float theta = Random.Range(0f, Mathf.PI * 2f);
            float phi = Mathf.Acos(Random.Range(-1f, 1f));
            float radius = starFieldRadius * Random.Range(0.3f, 0.8f);

            nebula.transform.position = new Vector3(
                radius * Mathf.Sin(phi) * Mathf.Cos(theta),
                radius * Mathf.Sin(phi) * Mathf.Sin(theta),
                radius * Mathf.Cos(phi)
            );

            // Масштаб
            float scale = nebulaSize * Random.Range(0.5f, 2f);
            nebula.transform.localScale = Vector3.one * scale;

            // Материал с прозрачностью
            Material nebulaMaterial = new Material(Shader.Find("Standard"));
            nebulaMaterial.color = nebulaColors[Random.Range(0, nebulaColors.Length)];
            nebulaMaterial.SetFloat("_Mode", 3); // Transparent
            nebulaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            nebulaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            nebulaMaterial.SetInt("_ZWrite", 0);
            nebulaMaterial.DisableKeyword("_ALPHATEST_ON");
            nebulaMaterial.EnableKeyword("_ALPHABLEND_ON");
            nebulaMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            nebulaMaterial.renderQueue = 3000;

            nebula.GetComponent<Renderer>().material = nebulaMaterial;
            nebula.GetComponent<Collider>().enabled = false;
        }
    }

    void CreateParticles()
    {
        GameObject particleSystem = new GameObject("SpaceParticles");
        particleSystem.transform.SetParent(transform);

        particles = particleSystem.AddComponent<ParticleSystem>();

        var main = particles.main;
        main.maxParticles = particleCount;
        main.startSpeed = particleSpeed;
        main.startSize = particleSize;
        main.startLifetime = 10f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

        var emission = particles.emission;
        emission.rateOverTime = 50f;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = starFieldRadius * 0.5f;

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        Material particleMaterial = new Material(Shader.Find("Unlit/Transparent"));
        particleMaterial.color = new Color(1f, 1f, 1f, 0.5f);
        renderer.material = particleMaterial;
    }

    void Update()
    {
        // Медленное вращение звёздного неба
        if (starField != null)
        {
            starField.transform.Rotate(Vector3.up, Time.deltaTime * 0.01f);
        }

        if (nebulaField != null)
        {
            nebulaField.transform.Rotate(Vector3.up, Time.deltaTime * 0.005f);
        }
    }
}
