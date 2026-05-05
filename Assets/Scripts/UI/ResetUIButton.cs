using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ResetUIButton : MonoBehaviour
{
    [SerializeField] private ProjectileLauncher launcher;
    [SerializeField] private bool autoFindLauncher = true;
    [SerializeField] private bool logIfMissing = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveListener(ResetNow);
        button.onClick.AddListener(ResetNow);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(ResetNow);
    }

    public void ResetNow()
    {
        if (launcher == null && autoFindLauncher)
            launcher = FindAnyObjectByType<ProjectileLauncher>(FindObjectsInactive.Include);

        if (launcher == null)
        {
            if (logIfMissing)
                Debug.LogWarning("ResetUIButton: No ProjectileLauncher found to reset.", this);
            return;
        }

        launcher.ResetLauncher();
    }
}
