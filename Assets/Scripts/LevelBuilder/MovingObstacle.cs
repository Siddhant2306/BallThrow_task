using UnityEngine;

[DisallowMultipleComponent]
public class MovingObstacle : MonoBehaviour
{
    public enum MovementType
    {
        Horizontal,
        Vertical,
        Rotator,
        Pendulum,
        Oscillate,
    }

    [Header("Type")]
    [SerializeField] private MovementType type = MovementType.Horizontal;

    [Header("Common")]
    [SerializeField] private float amplitude = 1.2f;
    [SerializeField] private float speed = 1.6f;
    [SerializeField] private bool useLocalSpace = true;
    [SerializeField] private bool randomizeStartPhase = true;

    [Header("Rotation (Rotator / Pendulum)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;
    [SerializeField] private float rotationDegrees = 120f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Rigidbody rb;
    private Vector3 basePosition;
    private Quaternion baseRotation;
    private float phase;
    private float startTime;

    public MovementType Type => type;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        CaptureBasePose();
    }

    private void OnEnable()
    {
        startTime = Time.time;
        if (randomizeStartPhase)
            phase = Random.value * Mathf.PI * 2f;
    }

    private void CaptureBasePose()
    {
        basePosition = transform.position;
        baseRotation = transform.rotation;
    }

    public void Configure(MovementType newType, float newAmplitude, float newSpeed)
    {
        type = newType;
        amplitude = Mathf.Max(0f, newAmplitude);
        speed = Mathf.Max(0.01f, newSpeed);

        CaptureBasePose();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (debugLogs)
            Debug.Log($"[BallThrow] MovingObstacle.Configure type={type} amp={amplitude:0.00} speed={speed:0.00}", this);
    }

    public void ResetMotion()
    {
        CaptureBasePose();
        startTime = Time.time;
        if (randomizeStartPhase)
            phase = Random.value * Mathf.PI * 2f;

        if (debugLogs)
            Debug.Log("[BallThrow] MovingObstacle.ResetMotion", this);
    }

    private void FixedUpdate()
    {
        float t = (Time.time - startTime) * speed + phase;
        float s = Mathf.Sin(t);

        switch (type)
        {
            case MovementType.Horizontal:
                Move(basePosition + Axis(Vector3.right) * (amplitude * s));
                break;
            case MovementType.Vertical:
                Move(basePosition + Axis(Vector3.up) * (amplitude * s));
                break;
            case MovementType.Oscillate:
                // Slightly different feel than "Vertical" by using a smoother range.
                Move(basePosition + Axis(Vector3.up) * (amplitude * (0.5f + 0.5f * s)));
                break;
            case MovementType.Rotator:
                // Continuous, readable rotation.
                Rotate(transform.rotation * Quaternion.AngleAxis(rotationDegrees * Time.fixedDeltaTime, rotationAxis));
                break;
            case MovementType.Pendulum:
                Rotate(baseRotation * Quaternion.AngleAxis(rotationDegrees * s, rotationAxis));
                break;
        }
    }

    private Vector3 Axis(Vector3 localAxis)
    {
        if (!useLocalSpace)
            return localAxis.normalized;

        return transform.TransformDirection(localAxis).normalized;
    }

    private void Move(Vector3 worldPosition)
    {
        if (rb != null)
        {
            rb.MovePosition(worldPosition);
            return;
        }

        transform.position = worldPosition;
    }

    private void Rotate(Quaternion worldRotation)
    {
        if (rb != null)
        {
            rb.MoveRotation(worldRotation);
            return;
        }

        transform.rotation = worldRotation;
    }
}
