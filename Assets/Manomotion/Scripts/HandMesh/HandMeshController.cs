using UnityEngine;

public class HandMeshController : MonoBehaviour
{
    [SerializeField] GameObject handMesh;
    [SerializeField] LeftOrRightHand leftRightHand;
    [SerializeField] Vector3 rotationOffset;

    [Space(10)]
    [SerializeField] float depthMultiplier = 1;
    [SerializeField, Range(-1, 1)] float depthExtraValue = 0;

    void Update()
    {
        bool foundHand = TryGetHandInfo(out HandInfo handInfo, out int handIndex);
        handMesh.SetActive(foundHand);

        if (foundHand)
        {
            // TODO: Unity started going crazy, this should not be necessary.
            Vector3 pos = handInfo.trackingInfo.skeleton.jointPositions[0];
            if (pos == Vector3.up || pos == Vector3.zero)
            {
                handMesh.SetActive(false);
                return;
            }

            //Vector3 position = handInfo.tracking_info.skeleton.joints[0];
            //float depth = CalculateDepth(handInfo);
            //handMesh.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(position, depth);

            handMesh.transform.position = GetPosition(handIndex);

            Quaternion rotation = SkeletonManager.instance.GetHandRotation(handIndex);
            handMesh.transform.rotation = rotation * Quaternion.Euler(rotationOffset);
        }
    }

    private Vector3 GetPosition(int handIndex)
    {
        SkeletonManager skeleton = SkeletonManager.instance;
        return handIndex == 0 ? skeleton.joints[0].transform.position : skeleton.jointsSecond[0].transform.position;
    }

    /// <summary>
    /// Returns true and gives back the hand info of the left/right hand specified.
    /// </summary>
    private bool TryGetHandInfo(out HandInfo handInfo, out int handIndex)
    {
        handInfo = default;
        handIndex = 0;

        for (LeftOrRightHand hand = LeftOrRightHand.LEFT_HAND; hand <= LeftOrRightHand.RIGHT_HAND; hand++)
        {
            HandInfo currentHand = ManomotionManager.Instance.HandInfos[(int)hand];
            if (currentHand.gestureInfo.leftRightHand.Equals(leftRightHand))
            {
                handInfo = currentHand;
                handIndex = (int)hand;
                return true;
            }
        }

        return false;
    }
}
