using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GoalPulseOnSuccess : MonoBehaviour
{
    [SerializeField] private Transform pulseTransform;
    [SerializeField] private float scaleMultiplier = 1.12f;
    [SerializeField] private float pulseDuration = 0.35f;
    [SerializeField] private bool useUnscaledTime = true;

    [SerializeField] private Color glowColor = new Color(0.2f, 1f, 0.6f, 1f);
    [SerializeField] private float glowIntensity = 2.2f;
    [SerializeField] private bool enableEmissionKeyword = true;

    [SerializeField] private bool debugLogs = true;

    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    private const string EmissionKeyword = "_EMISSION";

    private readonly List<Material> glowMaterials = new List<Material>(16);
    private readonly List<Color> baseEmissions = new List<Color>(16);

    private Coroutine routine;

    private void Awake()
    {
        if (pulseTransform == null)
            pulseTransform = transform;
    }

    public void PlayPulse()
    {
        if (debugLogs)
            Debug.Log("[BallThrow] GoalPulseOnSuccess.PlayPulse", this);

        if (pulseTransform == null)
            pulseTransform = transform;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        BuildGlowMaterials();
        if (glowMaterials.Count == 0)
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] GoalPulseOnSuccess: no materials with _EmissionColor found under goal.", this);

            routine = null;
            yield break;
        }

        Vector3 startScale = pulseTransform.localScale;
        Vector3 peakScale = startScale * Mathf.Max(0.01f, scaleMultiplier);

        float duration = Mathf.Max(0.01f, pulseDuration);
        float half = duration * 0.5f;

        for (float t = 0f; t < half;)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, half));
            u = 1f - Mathf.Pow(1f - u, 3f);

            pulseTransform.localScale = Vector3.LerpUnclamped(startScale, peakScale, u);
            ApplyEmission(u);
            yield return null;
        }

        for (float t = 0f; t < half;)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, half));
            u = u * u * u;

            pulseTransform.localScale = Vector3.LerpUnclamped(peakScale, startScale, u);
            ApplyEmission(1f - u);
            yield return null;
        }

        pulseTransform.localScale = startScale;
        RestoreEmission();
        routine = null;
    }

    private void BuildGlowMaterials()
    {
        glowMaterials.Clear();
        baseEmissions.Clear();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        for (int r = 0; r < renderers.Length; r++)
        {
            Renderer renderer = renderers[r];
            if (renderer == null)
                continue;

            // Use .materials so we don't modify shared asset materials.
            Material[] mats = renderer.materials;
            for (int m = 0; m < mats.Length; m++)
            {
                Material mat = mats[m];
                if (mat == null || !mat.HasProperty(EmissionId))
                    continue;

                glowMaterials.Add(mat);
                baseEmissions.Add(mat.GetColor(EmissionId));
            }
        }

        if (debugLogs)
            Debug.Log($"[BallThrow] GoalPulseOnSuccess: glow materials={glowMaterials.Count}", this);
    }

    private void ApplyEmission(float strength01)
    {
        float strength = Mathf.Clamp01(strength01);
        Color emission = glowColor * (glowIntensity * strength);

        for (int i = 0; i < glowMaterials.Count; i++)
        {
            Material mat = glowMaterials[i];
            if (mat == null)
                continue;

            // URP emission can fail to update if it starts at black; enforce keyword after setting.
            mat.SetColor(EmissionId, emission);
            if (enableEmissionKeyword)
                mat.EnableKeyword(EmissionKeyword);
        }
    }

    private void RestoreEmission()
    {
        for (int i = 0; i < glowMaterials.Count; i++)
        {
            Material mat = glowMaterials[i];
            if (mat == null)
                continue;

            mat.SetColor(EmissionId, baseEmissions[i]);
            if (enableEmissionKeyword)
                mat.EnableKeyword(EmissionKeyword);
        }
    }
}

