using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SuccessHandler : MonoBehaviour
{
    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeStrength = 0.12f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [Header("References")]
    [SerializeField] private FeedbackManager feedback;

    public void PlaySuccess(Rigidbody ballBody)
    {
        if (debugLogs)
            Debug.Log("[BallThrow] SuccessHandler.PlaySuccess", this);

        if (feedback == null)
            feedback = FindAnyObjectByType<FeedbackManager>(FindObjectsInactive.Include);

        if (feedback != null)
            feedback.Shake(shakeDuration, shakeStrength);

        if (ballBody != null)
            StartCoroutine(SettleRoutine(ballBody));
    }

    private IEnumerator SettleRoutine(Rigidbody body)
    {
        float elapsed = 0f;
        while (elapsed < 0.35f && body != null)
        {
            elapsed += Time.deltaTime;
            body.linearVelocity = Vector3.Lerp(body.linearVelocity, Vector3.zero, 0.06f);
            body.angularVelocity = Vector3.Lerp(body.angularVelocity, Vector3.zero, 0.06f);
            yield return null;
        }
    }
}
