using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the visualization for the hand contour.
/// </summary>
public class ContourGizmo : MonoBehaviour
{
    /// <summary>
    /// Line renderer for drawing the hand contour.
    /// </summary>
    [SerializeField]
    private LineRenderer contourLineRenderer;

    /// <summary>
    /// If no linerenderer is set this will get the Linerenderer from the GameObject
    /// </summary>
    private void Awake()
    {
        if (contourLineRenderer == null)
        {
            contourLineRenderer = GetComponent<LineRenderer>();
        }
    }

    /// <summary>
    /// This will calculate the new ContourPoints and set the positions of the LineRenderer.
    /// </summary>
    public void ShowContour(TrackingInfo trackingInfo)
    {
        int amountOfContourPoints = trackingInfo.numberOfContourPoints;
        Vector3[] newContourPoints = new Vector3[amountOfContourPoints];

        float contourDepthPosition = trackingInfo.skeleton.jointPositions[0].z;

        if (ManomotionManager.Instance.ManomotionSession.enabledFeatures.contour != 0)
        {
            for (int i = 0; i < amountOfContourPoints; i++)
            {
                newContourPoints[i] = ManoUtils.Instance.CalculateNewPositionWithDepth(new Vector3(trackingInfo.contourPoints[i].x, trackingInfo.contourPoints[i].y, contourDepthPosition / SkeletonManager.instance.GetDepthDivider), SkeletonManager.instance.GetClampedDepthEstimation * 1.5f);
            }

            contourLineRenderer.positionCount = amountOfContourPoints;
            contourLineRenderer.SetPositions(newContourPoints);
        }
    }
}
