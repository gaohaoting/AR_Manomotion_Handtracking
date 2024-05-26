#if ARFOUNDATION_EXISTS
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


/// <summary>
/// Handler for ARFoundation front facing camera.
/// </summary>
public class ARFoundationCameraSwitcher : MonoBehaviour
{
    ARCameraManager m_CameraManager;
    ARSessionOrigin _ARSessionOrigin;
    ARSession _ARSession;
    ARFaceManager _ARFaceManager;
    InputManagerArFoundation input_manager;

    public static Action<CameraFacingDirection> OnCameraFacingChanged;

    public void Start()
    {
        m_CameraManager = FindObjectOfType<ARCameraManager>();
        _ARSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        _ARSession = FindObjectOfType<ARSession>();
        _ARFaceManager = FindObjectOfType<ARFaceManager>();
        input_manager = FindObjectOfType<InputManagerArFoundation>();

    }


    /// <summary>
    /// On button press callback to toggle the requested camera facing direction.
    /// </summary>
    public void OnSwapCameraButtonPress()
    {
        Debug.Assert(m_CameraManager != null, "camera manager cannot be null");
        CameraFacingDirection newFacingDirection = CameraFacingDirection.None;
        switch (m_CameraManager.requestedFacingDirection)
        {
            case CameraFacingDirection.World:
                _ARFaceManager.enabled = true;
                newFacingDirection = CameraFacingDirection.User;
                _ARSession.GetComponent<ARSession>().requestedTrackingMode = TrackingMode.RotationOnly;
                _ARSessionOrigin.GetComponent<ARPlaneManager>().requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
                break;
            case CameraFacingDirection.User:
                _ARFaceManager.enabled = false;
                _ARSession.GetComponent<ARSession>().requestedTrackingMode = TrackingMode.PositionAndRotation;
                _ARSessionOrigin.GetComponent<ARPlaneManager>().requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
                newFacingDirection = CameraFacingDirection.World;
                break;      
        }
       
        Debug.Log($"Switching ARCameraManager.requestedFacingDirection from {m_CameraManager.requestedFacingDirection} to {newFacingDirection}");
        m_CameraManager.requestedFacingDirection = newFacingDirection;
        OnCameraFacingChanged(newFacingDirection);
    }

}
#endif
