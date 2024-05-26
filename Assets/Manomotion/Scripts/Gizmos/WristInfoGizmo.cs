using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the visualization of the wrist width and position.
/// </summary>
public class WristInfoGizmo : MonoBehaviour
{
    /// <summary>
    /// Wrist info, gets the information from ManoMotionManager.
    /// </summary>
   // private WristInfo wristInfo;

    /// <summary>
    /// The distance between the two wrist points.
    /// </summary>
    private float _widthBetweenWristPoints;

    /// <summary>
    /// The left wrist point gameobject.
    /// </summary>
    [SerializeField]
    private GameObject leftWrist3D;

    /// <summary>
    /// The right wrist point gameobject.
    /// </summary>
    [SerializeField]
    private GameObject rightWrist3D;

    /// <summary>
    /// The linerenderer gameobject.
    /// </summary>
    [SerializeField]
    private GameObject tryOnLine;

    /// <summary>
    /// The getter for the distance between the 2 Vector3 positions.
    /// </summary>
    public float WidthBetweenWristPoints
    {
        get
        {
            return _widthBetweenWristPoints;
        }
    }

    /// <summary>
    /// The getter for the left wrist position.
    /// </summary>
    public Vector3 LeftWristPoint3DPosition
    {
        get
        {
            return leftWrist3D.transform.position;
        }
    }

    /// <summary>
    /// The getter for the right wrist postion.
    /// </summary>
    public Vector3 RightWristPoint3DPosition
    {
        get
        {
            return rightWrist3D.transform.position;
        }
    }

    /// <summary>
    /// If SDK should run wrist information ShowWristInformation will calculate the normalized values to fit the hands position.
    /// if no hand is detected the left, right sphere and the tryOnLine will be disabled
    /// </summary>
    public void ShowWristInformation(TrackingInfo trackingInfo, GestureInfo gestureInfo)
    {

        WristInfo wristInfo = trackingInfo.wristInfo;

        _widthBetweenWristPoints = (Vector3.Distance(wristInfo.leftPoint, wristInfo.rightPoint));

        float clampedDepthEstimation = Mathf.Clamp(trackingInfo.depthEstimation, 0.4f, 1);

        leftWrist3D.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(new Vector3(wristInfo.leftPoint.x, wristInfo.leftPoint.y, trackingInfo.skeleton.jointPositions[0].z / SkeletonManager.instance.GetDepthDivider) , clampedDepthEstimation * 1.5f);
        rightWrist3D.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(new Vector3(wristInfo.rightPoint.x, wristInfo.rightPoint.y, trackingInfo.skeleton.jointPositions[0].z / SkeletonManager.instance.GetDepthDivider), clampedDepthEstimation * 1.5f);

        if (gestureInfo.manoClass == ManoClass.NO_HAND)
        {
            ActivateWristGizmos(false);
        }
        else
        {
            ActivateWristGizmos(true);
        }
    }

    private void ActivateWristGizmos(bool status)
    {
        rightWrist3D.SetActive(status);
        leftWrist3D.SetActive(status);
        tryOnLine.SetActive(status);
    }
}
