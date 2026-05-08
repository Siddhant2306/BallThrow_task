using UnityEngine;

public partial class GameManager
{
    private void EnsureReferencesBound()
    {
        if (!autoFindReferences)
            return;

        if (launcher != null)
            return;

        VLog("EnsureReferencesBound: launcher was null, rebinding references.");
        ResolveReferences();
        HookLauncher();
    }

    private void ResolveReferences()
    {
        if (launcher == null)
            launcher = FindAnyObjectByType<ProjectileLauncher>(FindObjectsInactive.Include);

        if (ballBody == null && launcher != null)
            ballBody = launcher.GetComponent<Rigidbody>();

        if (ballState == null && launcher != null)
            ballState = launcher.GetComponent<BallStateController>();

        if (successHandler == null)
            successHandler = FindAnyObjectByType<SuccessHandler>(FindObjectsInactive.Include);

        if (failureHandler == null)
            failureHandler = FindAnyObjectByType<FailureHandler>(FindObjectsInactive.Include);

        if (ui == null)
            ui = FindAnyObjectByType<UIController>(FindObjectsInactive.Include);

        if (feedback == null)
            feedback = FindAnyObjectByType<FeedbackManager>(FindObjectsInactive.Include);

        VLog($"ResolveReferences: launcher={(launcher != null)} ballBody={(ballBody != null)} ballState={(ballState != null)} ui={(ui != null)} feedback={(feedback != null)}");
    }

    private void HookLauncher()
    {
        if (launcher == null)
            return;

        VLog("HookLauncher: subscribing launcher events.");

        launcher.Launched -= HandleLaunched;
        launcher.Launched += HandleLaunched;

        launcher.Reset -= HandleReset;
        launcher.Reset += HandleReset;
    }

    private void UnhookLauncher()
    {
        if (launcher == null)
            return;

        VLog("UnhookLauncher: unsubscribing launcher events.");

        launcher.Launched -= HandleLaunched;
        launcher.Reset -= HandleReset;
    }
}
