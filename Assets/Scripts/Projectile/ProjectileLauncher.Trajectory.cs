using System;
using UnityEngine;

public partial class ProjectileLauncher
{
    private void CreateTrajectoryDots()
    {
        if (trajectoryDotPrefab == null || dotCount <= 0)
        {
            dots = Array.Empty<GameObject>();
            return;
        }

        dots = new GameObject[dotCount];
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(trajectoryDotPrefab, transform);
            dot.transform.localPosition = Vector3.zero;
            dot.layer = IgnoreRaycastLayer;
            dot.SetActive(false);
            dots[i] = dot;
        }
    }

    private void ShowTrajectory(Vector3 launchVelocity)
    {
        if (dots.Length == 0)
            return;

        float step = Mathf.Max(0.001f, timeStep);
        float radius = Mathf.Max(0.001f, cachedBallRadius * Mathf.Max(0.01f, trajectoryCollisionRadiusMultiplier));
        Vector3 previousPosition = transform.position;

        for (int i = 0; i < dots.Length; i++)
        {
            float t = i * step;

            Vector3 worldPosition =
                transform.position +
                launchVelocity * t +
                0.5f * Physics.gravity * t * t;

            worldPosition.z = transform.position.z;

            Vector3 segment = worldPosition - previousPosition;
            float segmentDistance = segment.magnitude;

            if (segmentDistance > 0.0001f)
            {
                Vector3 segmentDirection = segment / segmentDistance;

                Vector3 castOrigin = previousPosition + segmentDirection * 0.001f;
                float castDistance = Mathf.Max(0f, segmentDistance - 0.001f);

                if (castDistance > 0f && Physics.SphereCast(castOrigin, radius, segmentDirection, out RaycastHit hit, castDistance, trajectoryCollisionMask, trajectoryQueryTriggers))
                {
                    Vector3 hitPoint = hit.point;
                    hitPoint.z = transform.position.z;

                    if (dots[i] != null)
                    {
                        dots[i].transform.position = hitPoint;
                        dots[i].SetActive(true);
                    }

                    for (int j = i + 1; j < dots.Length; j++)
                    {
                        if (dots[j] != null)
                            dots[j].SetActive(false);
                    }

                    return;
                }
            }

            if (dots[i] != null)
            {
                dots[i].transform.position = worldPosition;
                dots[i].SetActive(true);
            }

            previousPosition = worldPosition;
        }
    }

    private void HideTrajectory()
    {
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] != null)
                dots[i].SetActive(false);
        }
    }

    private static float ResolveBallRadius(Transform t, Collider c)
    {
        if (c is SphereCollider sphere)
        {
            float maxAxis = Mathf.Max(Mathf.Abs(t.lossyScale.x), Mathf.Abs(t.lossyScale.y), Mathf.Abs(t.lossyScale.z));
            return Mathf.Max(0.001f, sphere.radius * maxAxis);
        }

        if (c != null)
        {
            Bounds b = c.bounds;
            return Mathf.Max(0.001f, Mathf.Min(b.extents.x, b.extents.y, b.extents.z));
        }

        return 0.25f;
    }
}

