using UnityEngine;

public static class RotationUtility
{
    // Values to make it so x is right, y is up and z is forward
    static Vector3[] handRotationOffsets =
    {
        new Vector3(0, -10, 180),
        new Vector3(180, 0, -10)
    };

    /// <summary>
    /// Get corrected hand rotation for the given hand.
    /// </summary>
    public static Quaternion GetHandRotation(this HandInfo handInfo)
    {
        if (handInfo.trackingInfo.worldSkeleton.jointNormalizedRotations == null)
            return Quaternion.identity;

        // Get the rotation of the wrist joint on the world skeleton.
        Quaternion wristRotation = handInfo.trackingInfo.worldSkeleton.jointNormalizedRotations[0];
        LeftOrRightHand hand = handInfo.gestureInfo.leftRightHand;

        if (hand != LeftOrRightHand.LEFT_HAND && hand != LeftOrRightHand.RIGHT_HAND) 
            return Quaternion.identity;

        // Get all necessary rotations.
        Quaternion cameraRotation = Camera.main.transform.rotation;
        Quaternion rotationOffset = Quaternion.Euler(handRotationOffsets[(int)hand]);
        Quaternion correctedRotation = CorrectHandRotation(wristRotation, hand);

        // Multiply rotations together to get the final rotation.
        return cameraRotation * rotationOffset * correctedRotation;
    }

    public static Quaternion GetFingerJointRotation(this HandInfo handInfo, int jointIndex)
    {
        Quaternion[] rotations = handInfo.trackingInfo.worldSkeleton.jointNormalizedRotations;
        if (rotations == null || rotations[jointIndex].IsNaN())
            return Quaternion.identity;

        return CorrectFingerJointRotation(rotations[jointIndex], handInfo.gestureInfo.leftRightHand);
    }

    public static bool IsNaN(this Quaternion q)
    {
        return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
    }

    /// <summary>
    /// Corrects the direction of rotation axes depending on which hand it is.
    /// </summary>
    static Quaternion CorrectHandRotation(Quaternion q, LeftOrRightHand hand)
    {
        Quaternion corrected = hand switch
        {
            LeftOrRightHand.LEFT_HAND => new Quaternion(-q.x, q.y, q.z, -q.w),
            LeftOrRightHand.RIGHT_HAND => new Quaternion(q.x, q.y, -q.z, -q.w),
            _ => Quaternion.identity
        };

        return corrected;
    }

    static Quaternion CorrectFingerJointRotation(Quaternion q, LeftOrRightHand hand)
    {
        Quaternion corrected = hand switch
        {
            LeftOrRightHand.LEFT_HAND => new Quaternion(q.y, -q.x, q.z, -q.w),
            LeftOrRightHand.RIGHT_HAND => new Quaternion(-q.y, q.x, q.z, -q.w),
            _ => Quaternion.identity
        };

        return corrected;
    }
}