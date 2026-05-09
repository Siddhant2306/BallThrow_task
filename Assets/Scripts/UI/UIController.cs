using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIController : MonoBehaviour
{
    [Header("End Screen References")]
    [SerializeField] private CanvasGroup endScreenGroup;
    [SerializeField] private RectTransform endScreenPanel;
    [SerializeField] private Text titleText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button retryButton;

    [Header("Animation")]
    [SerializeField] private float popDuration = 0.22f;

    [Header("Behavior")]
    [Tooltip("If enabled, Success screen shows RETRY in addition to NEXT.")]
    [SerializeField] private bool showRetryOnSuccess = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Action onNext;
    private Action onRetry;
    private Coroutine popRoutine;

    private void Awake()
    {
        HideEndScreen();
    }

    public void HideEndScreen()
    {
        if (endScreenGroup != null)
        {
            endScreenGroup.alpha = 0f;
            endScreenGroup.interactable = false;
            endScreenGroup.blocksRaycasts = false;
        }

        if (endScreenPanel != null)
            endScreenPanel.localScale = Vector3.one;

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
    }

    public void ShowSuccess(Action onNext, Action onRetry)
    {
        if (debugLogs)
            Debug.Log("[BallThrow] UIController.ShowSuccess", this);

        this.onNext = onNext;
        this.onRetry = onRetry;

        if (!HasRequiredRefs())
            return;

        titleText.text = "LEVEL COMPLETE";

        nextButton.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(showRetryOnSuccess && onRetry != null);

        nextButton.onClick.RemoveListener(HandleNext);
        nextButton.onClick.AddListener(HandleNext);

        if (retryButton.gameObject.activeSelf)
        {
            retryButton.onClick.RemoveListener(HandleRetry);
            retryButton.onClick.AddListener(HandleRetry);
        }

        ShowAndPop();
    }

    public void ShowFailure(Action onRetry)
    {
        if (debugLogs)
            Debug.Log("[BallThrow] UIController.ShowFailure", this);

        this.onNext = null;
        this.onRetry = onRetry;

        if (!HasRequiredRefs())
            return;

        titleText.text = "FAILED";

        nextButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(true);

        retryButton.onClick.RemoveListener(HandleRetry);
        retryButton.onClick.AddListener(HandleRetry);

        ShowAndPop();
    }

    private bool HasRequiredRefs()
    {
        if (endScreenGroup == null || endScreenPanel == null || titleText == null || nextButton == null || retryButton == null)
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] UIController missing references. Assign End Screen refs in the Inspector.", this);
            return false;
        }

        return true;
    }

    private void ShowAndPop()
    {
        endScreenGroup.alpha = 1f;
        endScreenGroup.interactable = true;
        endScreenGroup.blocksRaycasts = true;

        if (popRoutine != null)
            StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        float duration = Mathf.Max(0.01f, popDuration);
        float t = 0f;

        Vector3 start = Vector3.one * 0.75f;
        Vector3 end = Vector3.one;
        endScreenPanel.localScale = start;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            u = 1f - Mathf.Pow(1f - u, 3f);
            endScreenPanel.localScale = Vector3.LerpUnclamped(start, end, u);
            yield return null;
        }

        endScreenPanel.localScale = end;
        popRoutine = null;
    }

    private void HandleNext()
    {
        if (debugLogs)
            Debug.Log("[BallThrow] UIController: NEXT", this);

        HideEndScreen();
        onNext?.Invoke();
    }

    private void HandleRetry()
    {
        if (debugLogs)
            Debug.Log("[BallThrow] UIController: RETRY", this);

        HideEndScreen();
        onRetry?.Invoke();
    }
}

