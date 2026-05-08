using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class ProjectileLauncher
{
    private static readonly List<RaycastResult> UiRaycastResults = new List<RaycastResult>(16);

    private bool TryGetPointerDown(out Vector3 worldPosition, out int pointerId)
    {
        // Prefer touch on mobile.
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase != TouchPhase.Began)
                    continue;

                if (IsPointerOverUI(t.position, t.fingerId))
                    continue;

                worldPosition = GetWorldPositionFromScreen(t.position);
                pointerId = t.fingerId;
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI((Vector2)Input.mousePosition, -1))
            {
                worldPosition = default;
                pointerId = -1;
                return false;
            }

            worldPosition = GetWorldPositionFromScreen((Vector2)Input.mousePosition);
            pointerId = -1;
            return true;
        }

        worldPosition = default;
        pointerId = -1;
        return false;
    }

    private bool TryGetPointerMove(int pointerId, out Vector3 worldPosition)
    {
        if (pointerId >= 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId != pointerId)
                    continue;

                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                {
                    worldPosition = GetWorldPositionFromScreen(t.position);
                    return true;
                }

                break;
            }

            worldPosition = default;
            return false;
        }

        if (Input.GetMouseButton(0))
        {
            worldPosition = GetWorldPositionFromScreen((Vector2)Input.mousePosition);
            return true;
        }

        worldPosition = default;
        return false;
    }

    private bool TryGetPointerUp(int pointerId, out Vector3 worldPosition)
    {
        if (pointerId >= 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.fingerId != pointerId)
                    continue;

                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    worldPosition = GetWorldPositionFromScreen(t.position);
                    return true;
                }

                break;
            }

            worldPosition = default;
            return false;
        }

        if (Input.GetMouseButtonUp(0))
        {
            worldPosition = GetWorldPositionFromScreen((Vector2)Input.mousePosition);
            return true;
        }

        worldPosition = default;
        return false;
    }

    private static bool IsPointerOverUI(Vector2 screenPosition, int pointerId)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition,
            pointerId = pointerId,
        };

        UiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, UiRaycastResults);
        return UiRaycastResults.Count > 0;
    }
}
