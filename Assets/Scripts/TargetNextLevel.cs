using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class TargetNextLevel : MonoBehaviour
{
    [Header("Filtering")]
    [SerializeField] private ProjectileLauncher ball;
    [SerializeField] private float minSuccessSpeed = 2f;
    [SerializeField] private bool requireLaunchedBall = true;
    [SerializeField] private bool onlyOncePerAttempt = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogOnSuccess = true;

    [Header("Events")]
    public UnityEvent onTargetHit = new UnityEvent();

    private bool hasTriggered;

    private void Log(string message)
    {
        if (!debugLogOnSuccess)
            return;

        Debug.Log($"[BallThrow] TargetNextLevel: {message}", this);
    }

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
        // If this component lives on a prefab asset, the Inspector can only reference other assets.
        // That can leave `ball` pointing to the Ball prefab asset instead of the in-scene ball instance.
        if (ball != null)
        {
            Scene scene = ball.gameObject.scene;
            bool isSceneObject = scene.IsValid() && scene.isLoaded;
            if (!isSceneObject)
            {
                Log($"ResolveBall: clearing non-scene ball reference '{ball.name}' (likely prefab asset).");
                ball = null;
            }
        }

        if (ball != null)
            return;

        ball = FindAnyObjectByType<ProjectileLauncher>(FindObjectsInactive.Include);
        if (ball != null)
            Log($"ResolveBall: found scene ball '{ball.name}'.");
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
        Log($"OnTriggerEnter: other='{other.name}' otherRB='{(other.attachedRigidbody != null ? other.attachedRigidbody.name : "null")}'");
        TryTrigger(other.attachedRigidbody);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Log($"OnCollisionEnter: other='{collision.collider.name}' rb='{(collision.rigidbody != null ? collision.rigidbody.name : "null")}'");
        TryTrigger(collision.rigidbody);
    }

    private void TryTrigger(Rigidbody otherBody)
    {
        if (onlyOncePerAttempt && hasTriggered)
        {
            Log("TryTrigger: ignored (already triggered this attempt).");
            return;
        }

        if (otherBody == null)
        {
            Log("TryTrigger: ignored (otherBody was null).");
            return;
        }

        if (!otherBody.TryGetComponent(out ProjectileLauncher hitBall))
        {
            Log($"TryTrigger: ignored (Rigidbody '{otherBody.name}' has no ProjectileLauncher).");
            return;
        }

        if (ball == null)
        {
            ball = hitBall;
            Subscribe();
            Log($"TryTrigger: bound ball to '{ball.name}'.");
        }

        if (ball != null && hitBall != ball)
        {
            Log($"TryTrigger: ignored (hitBall '{hitBall.name}' != bound ball '{ball.name}').");
            return;
        }

        if (requireLaunchedBall && !hitBall.HasLaunched)
        {
            Log("TryTrigger: ignored (ball not launched yet).");
            return;
        }

        hasTriggered = true;

        Log($"Target hit! Attempting success. speed={otherBody.linearVelocity.magnitude:0.00} min={minSuccessSpeed:0.00}");

        bool successAccepted = false;

        if (GameManager.Instance != null)
        {
            successAccepted = GameManager.Instance.TrySuccess(otherBody, transform, minSuccessSpeed);
            Log($"TrySuccess returned {successAccepted} (GM state={GameManager.Instance.State}).");
        }
        else
        {
            Log("Target hit, but no GameManager.Instance found (success not applied).");
        }

        if (successAccepted)
        {
            GoalPulseOnSuccess pulse =
                GetComponentInParent<GoalPulseOnSuccess>(includeInactive: true) ??
                GetComponentInChildren<GoalPulseOnSuccess>(includeInactive: true);

            if (pulse != null)
                pulse.PlayPulse();

            onTargetHit?.Invoke();
        }
        else
        {
            Log("Success was not accepted (min speed / wrong ball / already ended).");
        }
    }
}
