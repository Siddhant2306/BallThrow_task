using BallThrow.Gameplay;
using UnityEngine;

[DisallowMultipleComponent]
public class FailureHandler : MonoBehaviour
{

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.16f;
    [SerializeField] private float shakeStrength = 0.16f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [Header("References")]
    [SerializeField] private FeedbackManager feedback;

    public void PlayFailure(ProjectileLauncher launcher, Rigidbody ballBody, Vector3 at, FailureReason reason, Collider hitCollider)
    {
        if (debugLogs)
            Debug.Log($"[BallThrow] FailureHandler.PlayFailure reason={reason} at={at} hit={(hitCollider != null ? hitCollider.name : "none")}", this);

        if (feedback == null)
            feedback = FindAnyObjectByType<FeedbackManager>(FindObjectsInactive.Include);

        if (feedback != null)
        {
            feedback.Shake(shakeDuration, shakeStrength);
            feedback.FlashFailure();
        }
    }

}
