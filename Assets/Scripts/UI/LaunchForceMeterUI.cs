using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LaunchForceMeterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProjectileLauncher launcher;
    [SerializeField] private bool autoFindLauncher = true;

    [Header("UI Output (pick one)")]
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Behavior")]
    [SerializeField] private bool hideWhenIdle = false;
    [SerializeField] private bool hideAfterLaunch = true;
    [SerializeField] private float smoothing = 18f;
    [SerializeField] private bool useUnscaledTime = false;

    private float displayedValue;
    private ProjectileLauncher subscribedLauncher;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>(includeInactive: true);

        if (canvasGroup == null)
            canvasGroup = GetComponentInChildren<CanvasGroup>(includeInactive: true);

        if (fillImage == null && slider == null)
        {
            Image[] images = GetComponentsInChildren<Image>(includeInactive: true);
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].type == Image.Type.Filled)
                {
                    fillImage = images[i];
                    break;
                }
            }
        }

        SetValue(0f);
        SetVisible(true);
    }

    private void OnEnable()
    {
        ResolveLauncher();
        ResubscribeIfNeeded();
    }

    private void OnDisable()
    {
        Unsubscribe(subscribedLauncher);
        subscribedLauncher = null;
    }

    private void Update()
    {
        if (launcher == null && autoFindLauncher)
        {
            ResolveLauncher();
            ResubscribeIfNeeded();
        }

        float target = (launcher != null && !launcher.HasLaunched) ? launcher.CurrentPower01 : 0f;
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float t = 1f - Mathf.Exp(-Mathf.Max(0.01f, smoothing) * Mathf.Max(0f, dt));

        displayedValue = Mathf.Lerp(displayedValue, target, t);
        SetValue(displayedValue);

        if (hideWhenIdle)
            SetVisible(launcher != null && launcher.IsDragging && !launcher.HasLaunched);
    }

    private void ResolveLauncher()
    {
        if (launcher != null || !autoFindLauncher)
            return;

        launcher = FindAnyObjectByType<ProjectileLauncher>(FindObjectsInactive.Include);
    }

    private void ResubscribeIfNeeded()
    {
        if (launcher == subscribedLauncher)
            return;

        Unsubscribe(subscribedLauncher);
        Subscribe(launcher);
        subscribedLauncher = launcher;
    }

    private void Subscribe(ProjectileLauncher l)
    {
        if (l == null)
            return;

        l.Launched -= HandleLaunched;
        l.Launched += HandleLaunched;

        l.Reset -= HandleReset;
        l.Reset += HandleReset;
    }

    private void Unsubscribe(ProjectileLauncher l)
    {
        if (l == null)
            return;

        l.Launched -= HandleLaunched;
        l.Reset -= HandleReset;
    }

    private void HandleLaunched(Vector3 _)
    {
        if (hideAfterLaunch)
            SetVisible(false);
    }

    private void HandleReset()
    {
        displayedValue = 0f;
        SetValue(0f);
        SetVisible(true);
    }

    private void SetValue(float value01)
    {
        value01 = Mathf.Clamp01(value01);

        if (slider != null)
            slider.value = value01;

        if (fillImage != null)
            fillImage.fillAmount = value01;
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
