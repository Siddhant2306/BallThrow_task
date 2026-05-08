using BallThrow.Gameplay;
using UnityEngine;

[DisallowMultipleComponent]
public class BallHardImpactFail : MonoBehaviour
{
    [SerializeField] private float minImpactSpeed = 12f;
    [SerializeField] private bool onlyWhenLaunched = true;
    [SerializeField] private bool ignoreTargetCollisions = true;
    [SerializeField] private bool debugLogs = true;

    private ProjectileLauncher launcher;
    private Rigidbody body;

    private void Awake()
    {
        launcher = GetComponent<ProjectileLauncher>();
        body = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameManager.Instance == null)
            return;

        if (onlyWhenLaunched && launcher != null && !launcher.HasLaunched)
            return;

        if (ignoreTargetCollisions && collision.collider != null && collision.collider.GetComponentInParent<TargetNextLevel>(includeInactive: true) != null)
        {
            if (debugLogs)
                Debug.Log($"[BallThrow] BallHardImpactFail: ignoring collision with Target ({collision.collider.name})", this);
            return;
        }

        float speed = collision.relativeVelocity.magnitude;
        if (speed < minImpactSpeed)
            return;

        Vector3 at = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;

        if (debugLogs)
            Debug.Log($"[BallThrow] BallHardImpactFail: speed={speed:0.00} >= {minImpactSpeed:0.00} hit={collision.collider.name}", this);

        GameManager.Instance.Fail(FailureReason.HardImpact, at, collision.collider);
    }
}
