using UnityEngine;
using UnityEngine.EventSystems;

public class TrajectoryLineClickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RadarManager radarManager;
    private bool isPointerDown = false;
    private float pointerDownTime = 0f;
    private float holdTimeToEdit = 0.3f;

    public void Initialize(RadarManager manager)
    {
        radarManager = manager;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (radarManager == null) return;

        isPointerDown = true;
        pointerDownTime = Time.time;

        Invoke(nameof(TriggerEditMode), holdTimeToEdit);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        CancelInvoke(nameof(TriggerEditMode));
    }

    private void TriggerEditMode()
    {
        if (isPointerDown && radarManager != null)
        {
            radarManager.StartEditMode();
        }
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
