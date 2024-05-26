using UnityEngine;
using UnityEngine.Events;

public class BoundingBoxOverlap2D : MonoBehaviour
{
    [SerializeField] BoundingBoxUI boundingBoxUI;
    [SerializeField] RectTransform canvas, boundingBoxVisualization;
    [SerializeField] UnityEvent<Collider2D[]> OnIntersection;
    [SerializeField] UnityEvent OnIntersectionStop;

    Bounds bounds;
    bool isIntersecting = false;

    private void LateUpdate()
    {
        BoundingBox bb = boundingBoxUI.BoundingBox;
        Vector3 center = CalculateCenter(bb);
        Vector3 size = CalculateSize(bb);

        bounds = new Bounds(center, size);

        size.x = Mathf.InverseLerp(0, Screen.width, size.x) * canvas.sizeDelta.x;
        size.y = Mathf.InverseLerp(0, Screen.height, size.y) * canvas.sizeDelta.y;

        boundingBoxVisualization.position = bounds.center;
        boundingBoxVisualization.sizeDelta = size;
        boundingBoxVisualization.gameObject.SetActive(boundingBoxUI.Activated);

        CheckIntersection();
    }

    private Vector3 CalculateCenter(BoundingBox bb)
    {
        Vector3 center = bb.topLeft + Vector3.right * bb.width * 0.5f + Vector3.down * bb.height * 0.5f;
        center = ManoUtils.Instance.CalculateScreenPosition(center, true);
        return center;
    }

    private Vector3 CalculateSize(BoundingBox bb)
    {
        Vector3 topLeft = bb.topLeft;
        Vector3 botRight = bb.topLeft + Vector3.right * bb.width + Vector3.down * bb.height;

        topLeft = ManoUtils.Instance.CalculateScreenPosition(topLeft, false);
        botRight = ManoUtils.Instance.CalculateScreenPosition(botRight, false);

        float width = botRight.x - topLeft.x;
        float height = topLeft.y - botRight.y;

        Vector3 size = new Vector3(width, height);
        return size;
    }

    private void CheckIntersection()
    {
        Collider2D[] colliders = Physics2D.OverlapAreaAll(bounds.min, bounds.max);
        bool intersects = colliders.Length > 0;

        if (intersects)
        {
            isIntersecting = intersects;
            OnIntersection?.Invoke(colliders);
        }
        else if (intersects != isIntersecting)
        {
            OnIntersectionStop?.Invoke();
        }
    }
}