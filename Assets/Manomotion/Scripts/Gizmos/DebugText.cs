using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI debugText;
    [SerializeField] int hand;

    string message = "";

    private void Update()
    {
        HandInfo handInfo = ManomotionManager.Instance.HandInfos[hand];
        GestureInfo gestureInfo = handInfo.gestureInfo;
        TrackingInfo trackingInfo = handInfo.trackingInfo;

        message = "";
        AddToMessage($"Occlusion res: {ManomotionManager.Instance.VisualizationInfo.occlusionRGB.width}x{ManomotionManager.Instance.VisualizationInfo.occlusionRGB.height}");
        AddToMessage($"{ManomotionManager.Instance.Width}x{ManomotionManager.Instance.Height}");
        AddToMessage(gestureInfo.leftRightHand.ToString());
        AddToMessage(ManomotionManager.Instance.ManomotionSession.orientation.ToString());
        AddToMessage(Camera.main.transform.rotation.ToString());

        AddToMessage($"Depth: {trackingInfo.depthEstimation}");
        AddToMessage($"Confidence: {trackingInfo.skeleton.confidence}");

        AddToMessage($"ManoClass: {gestureInfo.manoClass}");
        AddToMessage($"Continuous: {gestureInfo.manoGestureContinuous}");
        AddToMessage($"Trigger: {gestureInfo.manoGestureTrigger}");

        debugText.text = message;
    }

    private void AddToMessage(string text)
    {
        message += $"{text}\n";
    }
}
