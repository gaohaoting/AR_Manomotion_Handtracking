using UnityEngine;

public class JointRotator : MonoBehaviour
{
    [SerializeField] LeftOrRightHand leftRightHand;
    [SerializeField, Range(0, 20)] int jointIndex;
    [SerializeField] Vector3 rotationOffset;

    void Update()
    {
        if (TryGetHandInfo(out HandInfo handInfo))
        {
            transform.localRotation = handInfo.GetFingerJointRotation(jointIndex) * Quaternion.Euler(rotationOffset);
        }
    }

    /// <summary>
    /// Returns true and gives back the hand info of the left/right hand specified.
    /// </summary>
    private bool TryGetHandInfo(out HandInfo handInfo)
    {
        handInfo = default;

        for (LeftOrRightHand hand = LeftOrRightHand.LEFT_HAND; hand <= LeftOrRightHand.RIGHT_HAND; hand++)
        {
            HandInfo currentHand = ManomotionManager.Instance.HandInfos[(int)hand];
            if (currentHand.gestureInfo.leftRightHand.Equals(leftRightHand))
            {
                handInfo = currentHand;
                return true;
            }
        }

        return false;
    }
}
