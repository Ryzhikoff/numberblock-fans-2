using UnityEngine;

public class S70_CameraFlyPath : MonoBehaviour
{
    [Header("Camera Settings")]
    public float flySpeed = 1f; // Flight speed
    public bool autoFly = true; // Automatic flight
    public float mouseSensitivity = 2f; // Mouse sensitivity

    [Header("Path Points")]
    public Transform[] pathPoints; // Points for camera path
    public bool loopPath = false; // Loop the path

    [Header("Path Animation")]
    public AnimationCurve pathCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Smoothness curve
    public float smoothTime = 0.5f; // Smoothing time

    [Header("Auto Path Generation")]
    public bool autoGeneratePath = true;
    public int pathPointCount = 50; // Number of path points
    public float pathRadius = 200f; // Path radius
    public float pathHeight = 100f; // Path height
    public float pathSpirals = 3f; // Number of spirals

    private int currentPointIndex = 0;
    private float t = 0f;
    private Vector3 velocity = Vector3.zero;
    private Transform cameraTransform;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        cameraTransform = transform;

        if (autoGeneratePath && pathPoints == null || pathPoints.Length == 0)
        {
            GenerateSpiralPath();
        }

        // Инициализируем углы камеры
        Vector3 angles = cameraTransform.eulerAngles;
        rotationX = angles.x;
        rotationY = angles.y;
    }

    void Update()
    {
        if (autoFly && pathPoints != null && pathPoints.Length > 0)
        {
            FlyAlongPath();
        }
        else
        {
            HandleMouseInput();
        }
    }

    void FlyAlongPath()
    {
        if (pathPoints.Length < 2) return;

        t += Time.deltaTime * flySpeed / pathPoints.Length;

        if (t >= 1f)
        {
            t = 0f;
            currentPointIndex++;

            if (currentPointIndex >= pathPoints.Length - 1)
            {
                if (loopPath)
                {
                    currentPointIndex = 0;
                }
                else
                {
                    currentPointIndex = pathPoints.Length - 2;
                    t = 1f;
                    autoFly = false;
                    return;
                }
            }
        }

        int fromIndex = currentPointIndex;
        int toIndex = Mathf.Min(currentPointIndex + 1, pathPoints.Length - 1);

        Vector3 fromPos = pathPoints[fromIndex].position;
        Vector3 toPos = pathPoints[toIndex].position;

        // Применяем кривую для плавности
        float curveT = pathCurve.Evaluate(t);

        // Плавно интерполируем позицию
        Vector3 targetPosition = Vector3.Lerp(fromPos, toPos, curveT);
        cameraTransform.position = Vector3.SmoothDamp(
            cameraTransform.position, 
            targetPosition, 
            ref velocity, 
            smoothTime
        );

        // Поворачиваем камеру в направлении движения
        if (toIndex < pathPoints.Length)
        {
            Vector3 direction = toPos - fromPos;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                cameraTransform.rotation = Quaternion.Slerp(
                    cameraTransform.rotation, 
                    targetRotation, 
                    Time.deltaTime * 2f
                );
            }
        }
    }

    void HandleMouseInput()
    {
        // Управление мышью для ручного режима
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        cameraTransform.eulerAngles = new Vector3(rotationX, rotationY, 0f);

        // Движение WASD
        float moveSpeed = 50f * Time.deltaTime;
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) move -= Vector3.forward;
        if (Input.GetKey(KeyCode.A)) move -= Vector3.right;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;
        if (Input.GetKey(KeyCode.Space)) move += Vector3.up;
        if (Input.GetKey(KeyCode.LeftShift)) move -= Vector3.up;

        if (move != Vector3.zero)
        {
            move = cameraTransform.TransformDirection(move);
            cameraTransform.position += move * moveSpeed;
        }
    }

    void GenerateSpiralPath()
    {
        pathPoints = new Transform[pathPointCount];

        for (int i = 0; i < pathPointCount; i++)
        {
            float t = (float)i / pathPointCount;
            float angle = t * pathSpirals * 2f * Mathf.PI;
            float radius = pathRadius * (1f + t * 2f);
            float height = t * pathHeight;

            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                height,
                Mathf.Sin(angle) * radius
            );

            // Создаём пустой GameObject для точки пути
            GameObject point = new GameObject($"PathPoint_{i}");
            point.transform.position = position;
            point.transform.SetParent(transform.parent);

            pathPoints[i] = point.transform;
        }
    }

    void OnDrawGizmos()
    {
        // Рисуем путь в редакторе
        if (pathPoints != null && pathPoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                if (pathPoints[i] != null && pathPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
                }
            }

            // Рисуем точки пути
            Gizmos.color = Color.red;
            foreach (Transform point in pathPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 2f);
                }
            }
        }
    }
}
