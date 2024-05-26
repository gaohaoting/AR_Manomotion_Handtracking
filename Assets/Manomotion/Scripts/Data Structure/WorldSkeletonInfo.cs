using UnityEngine;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// Contains information about the skeleton joints.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
[Serializable]
public struct WorldSkeletonInfo
{
    /// <summary>
    /// Position of the joints.
    /// normalized values between 0 and 1
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
    public Vector3[] jointPositions;

    /// <summary>
    /// Orientation of the joints.
    /// normalized values between 0 and 1
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
    public Quaternion[] jointNormalizedRotations;
}