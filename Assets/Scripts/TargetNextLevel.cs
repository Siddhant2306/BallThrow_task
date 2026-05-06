using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TargetNextLevel : MonoBehaviour
{
    [Header("Filtering")]
    [SerializeField] private ProjectileLauncher ball;
    [SerializeField] private bool requireLaunchedBall = true;
    [SerializeField] private bool onlyOncePerAttempt = true;

    [Header("Events")]
    public UnityEvent onTargetHit = new UnityEvent();

    private bool hasTriggered;

    private void OnEnable()
    {
        ResolveBall();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void ResolveBall()
    {
        if (ball != null)
            return;

        ball = FindAnyObjectByType<ProjectileLauncher>(FindObjectsInactive.Include);
    }

    private void Subscribe()
    {
        if (ball == null)
            return;

        ball.Reset -= HandleBallReset;
        ball.Reset += HandleBallReset;
    }

    private void Unsubscribe()
    {
        if (ball == null)
            return;

        ball.Reset -= HandleBallReset;
    }

    private void HandleBallReset()
    {
        hasTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTrigger(other.attachedRigidbody);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryTrigger(collision.rigidbody);
    }

    private void TryTrigger(Rigidbody otherBody)
    {
        if (onlyOncePerAttempt && hasTriggered)
            return;

        if (otherBody == null)
            return;

        if (!otherBody.TryGetComponent(out ProjectileLauncher hitBall))
            return;

        if (ball == null)
        {
            ball = hitBall;
            Subscribe();
        }

        if (ball != null && hitBall != ball)
            return;

        if (requireLaunchedBall && !hitBall.HasLaunched)
            return;

        hasTriggered = true;

        Debug.Log("Target hit! TODO: proceed to next level (stub).", this);
        onTargetHit?.Invoke();
    }
}
