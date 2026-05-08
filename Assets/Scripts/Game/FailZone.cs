using BallThrow.Gameplay;
using UnityEngine;

[DisallowMultipleComponent]
public class FailZone : MonoBehaviour
{
    [SerializeField] private bool debugLogs = true;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;
        if (body == null)
            return;

        ProjectileLauncher launcher = body.GetComponent<ProjectileLauncher>();
        if (launcher == null)
            return;

        if (debugLogs)
            Debug.Log($"[BallThrow] FailZone hit by {launcher.name}", this);

        if (GameManager.Instance != null)
            GameManager.Instance.Fail(FailureReason.FailZone, body.position, other);
    }
}

