using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the visualization of the finger width and position.
/// </summary>
public class FingerInfoGizmo : MonoBehaviour
{
    /// <summary>
    /// The distance between the two finger points.
    /// </summary>
    private float _widthBetweenFingerPoints;

    /// <summary>
    /// The left finger point gameobject.
    /// </summary>
    [SerializeField]
    private GameObject leftFingerPoint3D;

    /// <summary>
    /// The right finger point gameobject.
    /// </summary>
    [SerializeField]
    private GameObject rightFingerPoint3D;

    /// <summary>
    /// The linerenderer gameobject.
    /// </summary>
    [SerializeField]
    private GameObject tryOnLine;

    /// <summary>
    /// The getter for the width between the finger points.
    /// </summary>
    public float WidthBetweenFingerPoints
    {
        get
        {
            return _widthBetweenFingerPoints;
        }
    }

    /// <summary>
    /// The getter for the left finger point.
    /// </summary>
    public Vector3 LeftFingerPoint3DPosition
    {
        get
        {
            return leftFingerPoint3D.transform.position;
        }
    }

    /// <summary>
    /// The getter for the right finger point.
    /// </summary>
    public Vector3 RightFingerPoint3DPosition
    {
        get
        {
            return rightFingerPoint3D.transform.position;
        }
    }

    /// <summary>
    /// If SDK should run finger information ShowFingerInformation will calculate the normalized values to fit the hands position.
    /// if no hand is detected the left, right sphere and the tryOnLine will be disabled
    /// </summary>
    public void ShowFingerInformation(TrackingInfo trackingInfo, GestureInfo gestureInfo)
    {
        FingerInfo fingerInfo = trackingInfo.fingerInfo;

        float clampedDepthEstimation = Mathf.Clamp(trackingInfo.depthEstimation, 0.4f, 1);

        leftFingerPoint3D.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(new Vector3(fingerInfo.leftPoint.x, fingerInfo.leftPoint.y, trackingInfo.skeleton.jointPositions[13].z / SkeletonManager.instance.GetDepthDivider), clampedDepthEstimation * 1.5f);
        rightFingerPoint3D.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(new Vector3(fingerInfo.rightPoint.x, fingerInfo.rightPoint.y, trackingInfo.skeleton.jointPositions[13].z / SkeletonManager.instance.GetDepthDivider), clampedDepthEstimation * 1.5f);

        if (gestureInfo.manoClass == ManoClass.NO_HAND)
        {
            ActivateFingerGizmos(false);
        }
        else
        {
            ActivateFingerGizmos(true);
        }
    }

    private void ActivateFingerGizmos(bool status)
    {
        rightFingerPoint3D.SetActive(status);
        leftFingerPoint3D.SetActive(status);
        tryOnLine.SetActive(status);
    }
}
