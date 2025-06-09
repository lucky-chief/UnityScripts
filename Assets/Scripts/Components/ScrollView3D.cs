
using UnityEngine;

[ExecuteAlways]
public class ScrollView3D : MonoBehaviour
{
    [SerializeField] private float scrollItemWidth = 16;
    [SerializeField] private float scrollSpeed = 2;
    [SerializeField] private float snapSpeed = 10f;
    [SerializeField] private float swipTimeThreshold = 0.15f;
    [SerializeField] private int swipPixelsThreshold = 40;
    [SerializeField] private Rect viewRect = new Rect(0, 0, 1, 1);
    [SerializeField] private Transform offset;

    private float pointerDownTime, pointerUpTime;
    private Vector2 pointerUpPosition, pointerDownPosition, lastPointerPosition;
    private bool isDragging, isSnapping;
    private Vector3 desiredSnapPosition;
    private int currentPageIndex = 0, snappingToPageIndex;

    void OnEnable()
    {
        lastPointerPosition = Input.mousePosition;
    }

    void Update()
    {
        float delta = Input.mousePosition.x - lastPointerPosition.x;

        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            pointerDownTime = Time.time;
            lastPointerPosition =
            pointerDownPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            isSnapping = true;
            pointerUpTime = Time.time;
            pointerUpPosition = Input.mousePosition;
            CalculateSnapMovement();
        }
        Scroll(delta);
        Snap();
        lastPointerPosition = Input.mousePosition;
    }

    void Scroll(float delta)
    {
        if (!isDragging) return;
        float scrollSpeedMultiplier = 1f;
        bool isBounds = Mathf.Abs(offset.GetChild(currentPageIndex).position.x) < 1f;
        if (isBounds && delta > 0 && currentPageIndex == 0) scrollSpeedMultiplier = 0.1f;
        if (isBounds && delta < 0 && currentPageIndex == offset.childCount - 1) scrollSpeedMultiplier = 0.1f;
        offset.localPosition += new Vector3(delta * scrollSpeed * scrollSpeedMultiplier * Time.deltaTime, 0, 0);
    }

    void Snap()
    {
        if (!isSnapping) return;
        offset.localPosition = Vector3.Lerp(offset.localPosition, desiredSnapPosition, Time.deltaTime * snapSpeed);

        if (Vector3.Distance(offset.localPosition, desiredSnapPosition) < 0.01f)
        {
            isSnapping = false;
            currentPageIndex = snappingToPageIndex;
            offset.localPosition = desiredSnapPosition;
        }
    }

    bool ReconizeToSwipPage(out int incresement)
    {
        incresement = 0;
        if (pointerUpTime - pointerDownTime >= swipTimeThreshold) return false;
        if (Mathf.Abs(pointerUpPosition.x - pointerDownPosition.x) <= swipPixelsThreshold) return false;
        incresement = (pointerUpPosition.x - pointerDownPosition.x) > 0 ? -1 : 1;
        return true;
    }

    void CalculateSnapMovement()
    {
        int nearestIndex = -1;
        if (ReconizeToSwipPage(out int incresement))
        {
            currentPageIndex += incresement;
            currentPageIndex = nearestIndex = Mathf.Clamp(currentPageIndex, 0, offset.childCount - 1);
        }
        else
        {
            float minDistance = float.MaxValue;
            for (int i = 0; i < offset.childCount; i++)
            {
                float absDistance = Mathf.Abs(offset.GetChild(i).position.x);
                if (minDistance > absDistance)
                {
                    minDistance = absDistance;
                    nearestIndex = i;
                }
            }
        }

        snappingToPageIndex = nearestIndex;
        Transform nearest = offset.GetChild(nearestIndex);
        desiredSnapPosition = Vector3.right * (offset.localPosition.x - nearest.position.x);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(viewRect.center, viewRect.size);
    }
}