using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public partial class ProjectileLauncher : MonoBehaviour
{
    [Header("Trajectory Preview")]
    [SerializeField] private GameObject trajectoryDotPrefab;
    [SerializeField] private int dotCount = 20;
    [SerializeField] private float timeStep = 0.08f;
    [SerializeField] private LayerMask trajectoryCollisionMask = ~0;
    [SerializeField] private QueryTriggerInteraction trajectoryQueryTriggers = QueryTriggerInteraction.Ignore;
    [Tooltip("Multiplier applied to the ball collider radius when sphere-casting the preview path.")]
    [SerializeField] private float trajectoryCollisionRadiusMultiplier = 1.0f;

    [Header("Launch")]
    [SerializeField] private float forceMultiplier = 8f;
    [SerializeField] private float maxDragDistance = 2.5f;

    [Header("Reset")]
    [SerializeField] private Transform resetPoint;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private const int IgnoreRaycastLayer = 2;

    private Rigidbody rb;
    private Camera cam;
    private GameObject[] dots = Array.Empty<GameObject>();
    private float cachedBallRadius = 0.25f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 dragStartWorld;
    private bool isDragging;
    private bool hasLaunched;
    private int activePointerId = -1;

    public bool HasLaunched => hasLaunched;

    public event Action<Vector3> Launched;
    public event Action Reset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("Main Camera not found. Tag your camera as MainCamera.");
            enabled = false;
            return;
        }

        if (trajectoryDotPrefab == null && debugLogs)
            Debug.LogWarning("Trajectory Dot Prefab is not assigned. Trajectory preview will be disabled.");

        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        CaptureInitialPose();

        cachedBallRadius = ResolveBallRadius(transform, GetComponent<Collider>());
        CreateTrajectoryDots();
    }

    private void CaptureInitialPose()
    {
        if (resetPoint != null)
        {
            initialPosition = resetPoint.position;
            initialRotation = resetPoint.rotation;
        }
        else
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }
    }

    private void Update()
    {
        if (hasLaunched)
            return;

        if (TryGetPointerDown(out Vector3 worldDownPosition, out int pointerId))
            BeginDrag(worldDownPosition, pointerId);

        if (!isDragging)
            return;

        if (TryGetPointerMove(activePointerId, out Vector3 worldMovePosition))
            UpdateDrag(worldMovePosition);

        if (TryGetPointerUp(activePointerId, out Vector3 worldUpPosition))
            EndDrag(worldUpPosition);
    }

    private void BeginDrag(Vector3 worldDownPosition, int pointerId)
    {
        dragStartWorld = worldDownPosition;
        isDragging = true;
        activePointerId = pointerId;

        if (debugLogs)
            Debug.Log($"Drag started at: {dragStartWorld} (pointerId={pointerId})");
    }

    private void UpdateDrag(Vector3 worldPosition)
    {
        Vector3 dragVector = ComputeDragVector(worldPosition);
        Vector3 launchVelocity = ComputeLaunchVelocity(dragVector);
        ShowTrajectory(launchVelocity);
    }

    private void EndDrag(Vector3 worldUpPosition)
    {
        Vector3 dragVector = ComputeDragVector(worldUpPosition);
        Vector3 launchVelocity = ComputeLaunchVelocity(dragVector);

        if (debugLogs)
            Debug.Log($"Launching with drag: {dragVector} velocity: {launchVelocity}");

        LaunchWithVelocity(launchVelocity);

        isDragging = false;
        activePointerId = -1;
        HideTrajectory();
    }

    private Vector3 ComputeDragVector(Vector3 currentWorldPosition)
    {
        Vector3 dragVector = dragStartWorld - currentWorldPosition;
        dragVector.z = 0f;
        return Vector3.ClampMagnitude(dragVector, maxDragDistance);
    }

    private Vector3 ComputeLaunchVelocity(Vector3 dragVector)
    {
        Vector3 launchVelocity = dragVector * forceMultiplier;
        launchVelocity.z = 0f;
        return launchVelocity;
    }

    private void LaunchWithVelocity(Vector3 launchVelocity)
    {
        hasLaunched = true;

        rb.useGravity = true;
        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = launchVelocity;

        if (debugLogs)
            Debug.Log("Launch velocity: " + launchVelocity);

        Launched?.Invoke(launchVelocity);
    }

    private Vector3 GetWorldPositionFromScreen(Vector2 screenPosition)
    {
        float distanceFromCamera = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 p = new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera);

        Vector3 world = cam.ScreenToWorldPoint(p);
        world.z = transform.position.z;

        return world;
    }

    public void ResetLauncher(Vector3 resetPosition, Quaternion resetRotation)
    {
        hasLaunched = false;
        isDragging = false;
        activePointerId = -1;

        rb.useGravity = false;
        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(resetPosition, resetRotation);
        rb.Sleep();

        HideTrajectory();
        Reset?.Invoke();
    }

    public void ResetLauncher(Vector3 resetPosition)
    {
        ResetLauncher(resetPosition, initialRotation);
    }

    public void ResetLauncher()
    {
        ResetLauncher(initialPosition, initialRotation);
    }

    private void OnValidate()
    {
        dotCount = Mathf.Max(0, dotCount);
        timeStep = Mathf.Max(0.001f, timeStep);
        maxDragDistance = Mathf.Max(0.01f, maxDragDistance);
        forceMultiplier = Mathf.Max(0.01f, forceMultiplier);
        trajectoryCollisionRadiusMultiplier = Mathf.Max(0.01f, trajectoryCollisionRadiusMultiplier);
    }
}

