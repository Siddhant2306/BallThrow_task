using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class FeedbackManager : MonoBehaviour
{
    [Header("Shake")]
    [SerializeField] private float defaultShakeDuration = 0.12f;
    [SerializeField] private float defaultShakeStrength = 0.12f;

    [Header("Flash")]
    [SerializeField] private float defaultFlashDuration = 0.18f;
    [SerializeField] private Color failFlashColor = new Color(1f, 0.1f, 0.1f, 0.25f);

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Coroutine shakeRoutine;
    private Coroutine flashRoutine;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CanvasGroup flashGroup;

    private Vector3 camBaseLocalPos;
    private bool hasCapturedCamBasePos;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        CaptureCameraBasePos();

        if (flashGroup != null)
            flashGroup.alpha = 0f;
    }

    private void CaptureCameraBasePos()
    {
        if (cameraTransform == null)
            return;

        camBaseLocalPos = cameraTransform.localPosition;
        hasCapturedCamBasePos = true;
    }

    public void Shake(float duration = -1f, float strength = -1f)
    {
        if (duration <= 0f)
            duration = defaultShakeDuration;
        if (strength <= 0f)
            strength = defaultShakeStrength;

        if (debugLogs)
            Debug.Log($"[BallThrow] FeedbackManager.Shake duration={duration:0.00} strength={strength:0.00}", this);

        if (cameraTransform == null)
            return;

        // camera shake relative to the camera's CURRENT position.
        CaptureCameraBasePos();

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    public void FlashFailure(float duration = -1f)
    {
        Flash(failFlashColor, duration <= 0f ? defaultFlashDuration : duration);
    }

    public void Flash(Color color, float duration)
    {
        if (flashGroup == null)
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] FeedbackManager.Flash skipped (no flashGroup assigned).", this);
            return;
        }

        if (debugLogs)
            Debug.Log($"[BallThrow] FeedbackManager.Flash duration={duration:0.00} color={color}", this);

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine(color, duration));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        if (!hasCapturedCamBasePos)
            CaptureCameraBasePos();

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            float x = (Random.value * 2f - 1f) * strength;
            float y = (Random.value * 2f - 1f) * strength;
            cameraTransform.localPosition = camBaseLocalPos + new Vector3(x, y, 0f);

            yield return null;
        }

        cameraTransform.localPosition = camBaseLocalPos;
        shakeRoutine = null;
    }

    private IEnumerator FlashRoutine(Color color, float duration)
    {

        float half = Mathf.Max(0.01f, duration * 0.5f);

        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            flashGroup.alpha = Mathf.Clamp01(t / half);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            flashGroup.alpha = 1f - Mathf.Clamp01(t / half);
            yield return null;
        }

        flashGroup.alpha = 0f;
        flashRoutine = null;
    }
}
