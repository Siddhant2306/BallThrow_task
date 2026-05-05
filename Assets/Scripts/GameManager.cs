using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProjectileLauncher launcher;
    [SerializeField] private Rigidbody ballBody;

    [Header("Fail Conditions")]
    [SerializeField] private float minYBeforeFail = -8f;
    [SerializeField] private float maxFlightTime = 8f;
    [SerializeField] private float restSpeed = 0.25f;
    [SerializeField] private float restTime = 0.9f;

    [Header("Events")]
    public UnityEvent onLose = new UnityEvent();

    private bool isActiveAttempt;
    private bool isFinished;
    private float launchTime;
    private float restTimer;

    private void Awake()
    {
        if (launcher == null)
            launcher = FindAnyObjectByType<ProjectileLauncher>();

        if (launcher != null)
        {
            if (ballBody == null)
                ballBody = launcher.GetComponent<Rigidbody>();

            launcher.Launched += HandleLaunched;
            launcher.Reset += HandleReset;
        }
    }

    private void OnDestroy()
    {
        if (launcher != null)
        {
            launcher.Launched -= HandleLaunched;
            launcher.Reset -= HandleReset;
        }
    }

    private void Update()
    {
        if (!isActiveAttempt || isFinished || ballBody == null)
            return;

        if (Time.time - launchTime > maxFlightTime)
        {
            Lose();
            return;
        }

        if (ballBody.position.y < minYBeforeFail)
        {
            Lose();
            return;
        }

        float speed = ballBody.linearVelocity.magnitude;
        if (speed <= restSpeed)
            restTimer += Time.deltaTime;
        else
            restTimer = 0f;

        if (restTimer >= restTime)
            Lose();
    }

    private void HandleLaunched(Vector3 _)
    {
        isActiveAttempt = true;
        isFinished = false;
        launchTime = Time.time;
        restTimer = 0f;
    }

    private void HandleReset()
    {
        isActiveAttempt = false;
        isFinished = false;
        restTimer = 0f;
    }

    private void Lose()
    {
        if (isFinished)
            return;

        isFinished = true;
        isActiveAttempt = false;

        onLose?.Invoke();
    }
}
