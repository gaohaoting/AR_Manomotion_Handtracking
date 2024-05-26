using UnityEngine;

public class TriggerMarkerVisualizer : MonoBehaviour
{
    [SerializeField] LeftOrRightHand handLeftRight;
    [SerializeField] ManoGestureTrigger gesture;
    [SerializeField] TriggerMarker marker;

    private void Update()
    {
        if (ManomotionManager.Instance.TryGetHandInfo(handLeftRight, out HandInfo handInfo))
        {
            GestureInfo gestureInfo = handInfo.gestureInfo;

            if (gestureInfo.manoGestureTrigger.Equals(gesture))
            {
                marker.Activate(transform.position);
            }
        }
    }
}