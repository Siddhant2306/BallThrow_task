using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class SuccessHandler : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem successParticles;

    [Header("Audio")]
    [SerializeField] private AudioClip successSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    [Header("Time")]
    [SerializeField] private bool slowTime = true;
    [Range(0.05f, 1f)]
    [SerializeField] private float slowTimeScale = 0.75f;
    [SerializeField] private float slowTimeSeconds = 0.35f;

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.12f;
    [SerializeField] private float shakeStrength = 0.12f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private FeedbackManager feedback;

    private Coroutine slowMoRoutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    public void ResetState()
    {
        if (slowMoRoutine != null)
        {
            StopCoroutine(slowMoRoutine);
            slowMoRoutine = null;
        }
    }

    public void PlaySuccess(ProjectileLauncher launcher, Rigidbody ballBody, Transform goalTransform, ParticleSystem overrideParticles, AudioClip overrideSound)
    {
        if (debugLogs)
            Debug.Log("[BallThrow] SuccessHandler.PlaySuccess", this);

        if (feedback == null)
            feedback = FindAnyObjectByType<FeedbackManager>(FindObjectsInactive.Include);

        if (feedback != null)
            feedback.Shake(shakeDuration, shakeStrength);

        ParticleSystem prefab = overrideParticles != null ? overrideParticles : successParticles;
        if (prefab != null)
        {
            Vector3 pos = goalTransform != null ? goalTransform.position : (ballBody != null ? ballBody.position : transform.position);
            ParticleSystem ps = Instantiate(prefab, pos, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, 4f);
        }

        AudioClip clip = overrideSound != null ? overrideSound : successSound;
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, soundVolume);

        if (ballBody != null)
        {
            StartCoroutine(SettleRoutine(ballBody));
        }

        if (slowTime)
        {
            if (slowMoRoutine != null)
                StopCoroutine(slowMoRoutine);
            slowMoRoutine = StartCoroutine(SlowMoRoutine());
        }
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

    private IEnumerator SlowMoRoutine()
    {
        float oldScale = Time.timeScale;
        float oldFixed = Time.fixedDeltaTime;

        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = oldFixed * slowTimeScale;

        yield return new WaitForSecondsRealtime(slowTimeSeconds);

        Time.timeScale = oldScale;
        Time.fixedDeltaTime = oldFixed;

        slowMoRoutine = null;
    }
}
