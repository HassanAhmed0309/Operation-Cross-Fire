using UnityEngine;

// A touch button or area on a panel. Purely visual plus a hit-test; disable Raycast Target on its Image.
[RequireComponent(typeof(RectTransform))]
public class TouchControl : MonoBehaviour
{
    [SerializeField] TouchControlId id;

    RectTransform rectTransform;
    Camera eventCamera;

    public TouchControlId Id => id;

    void Awake()
    {
        rectTransform = (RectTransform)transform;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas = canvas.rootCanvas;
            eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }
    }

    public bool Contains(Vector2 screenPosition) =>
        RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, eventCamera);
}
