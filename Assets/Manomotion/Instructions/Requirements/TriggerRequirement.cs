using UnityEngine;

[CreateAssetMenu(fileName = "New InteractionRequirement", menuName = "ManoMotion/Instructions/TriggerRequirement")]
public class TriggerRequirement : InstructionRequirement
{
    [SerializeField] ManoGestureTrigger triggerGesture;

    public override bool IsFulfilled()
    {
        for (int i = 0; i <= ManomotionManager.Instance.ManomotionSession.enabledFeatures.twoHands; i++)
        {
            HandInfo handInfo = ManomotionManager.Instance.HandInfos[i];
            if (handInfo.gestureInfo.manoGestureTrigger.Equals(triggerGesture))
            {
                return true;
            }
        }
        return false;
    }
}