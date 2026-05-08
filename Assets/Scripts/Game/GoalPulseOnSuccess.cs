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

    private readonly List<Renderer> glowRenderers = new List<Renderer>(32);
    private readonly List<Color> baseEmissions = new List<Color>(32);

    private MaterialPropertyBlock mpb;
    private Coroutine routine;

    private void Awake()
    {
        if (pulseTransform == null)
            pulseTransform = transform;

        mpb = new MaterialPropertyBlock();
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
        BuildGlowTargets();

        if (glowRenderers.Count == 0)
        {
            if (debugLogs)
                Debug.LogWarning("[BallThrow] GoalPulseOnSuccess: no renderers with _EmissionColor found under goal.", this);

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
            u = 1f - (1f - u) * (1f - u) * (1f - u);

            pulseTransform.localScale = Vector3.LerpUnclamped(startScale, peakScale, u);
            ApplyEmission(glowColor * (glowIntensity * u));
            yield return null;
        }

        for (float t = 0f; t < half;)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, half));
            u = u * u * u;

            pulseTransform.localScale = Vector3.LerpUnclamped(peakScale, startScale, u);
            ApplyEmission(glowColor * (glowIntensity * (1f - u)));
            yield return null;
        }

        pulseTransform.localScale = startScale;
        RestoreEmission();
        routine = null;
    }

    private void BuildGlowTargets()
    {
        glowRenderers.Clear();
        baseEmissions.Clear();

        GetComponentsInChildren(true, glowRenderers);

        for (int i = 0; i < glowRenderers.Count; i++)
        {
            Renderer r = glowRenderers[i];
            if (r == null || !TryGetBaseEmission(r, out Color baseEmission))
            {
                glowRenderers.RemoveAt(i);
                i--;
                continue;
            }

            baseEmissions.Add(baseEmission);
        }

        if (debugLogs)
            Debug.Log($"[BallThrow] GoalPulseOnSuccess: glow renderers={glowRenderers.Count}", this);
    }

    private bool TryGetBaseEmission(Renderer r, out Color baseEmission)
    {
        baseEmission = Color.black;
        Material[] mats = r.sharedMaterials;
        if (mats == null || mats.Length == 0)
            return false;

        bool hasEmission = false;
        bool captured = false;
        for (int i = 0; i < mats.Length; i++)
        {
            Material mat = mats[i];
            if (mat == null || !mat.HasProperty(EmissionId))
                continue;

            hasEmission = true;

            if (enableEmissionKeyword)
                mat.EnableKeyword(EmissionKeyword);

            if (!captured)
            {
                captured = true;
                try { baseEmission = mat.GetColor(EmissionId); } catch { baseEmission = Color.black; }
            }
        }

        return hasEmission;
    }

    private void ApplyEmission(Color emission)
    {
        for (int i = 0; i < glowRenderers.Count; i++)
        {
            Renderer r = glowRenderers[i];
            if (r == null)
                continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, emission);
            r.SetPropertyBlock(mpb);
        }
    }

    private void RestoreEmission()
    {
        for (int i = 0; i < glowRenderers.Count; i++)
        {
            Renderer r = glowRenderers[i];
            if (r == null)
                continue;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, baseEmissions[i]);
            r.SetPropertyBlock(mpb);
        }
    }
}
