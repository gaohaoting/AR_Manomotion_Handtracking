using UnityEngine;
using System;
using System.Collections;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

/// <summary>
/// The ManomotionManager handles the communication with the SDK.
/// </summary>
[AddComponentMenu("ManoMotion/ManoMotion Manager Standard")]
[RequireComponent(typeof(ManoEvents), typeof(InputManagerBase))]
public class ManomotionManagerStandard : ManomotionManager
{
    IntPtr renderEventFunc;

    #region Awake/Start 

    protected override void Awake()
    {
        inputManager = GetComponent<InputManagerBase>();

        if (instance == null)
        {
            base.Awake();
            instance = this;
            ManoUtils.OnOrientationChanged += HandleOrientationChanged;
            InputManagerBase.OnChangeCamera += HandleOrientationChanged;
            InputManagerBase.OnAddonSet += HandleAddOnSet;
            InputManagerBase.OnFrameInitialized += HandleManomotionFrameUpdated;
            InputManagerBase.OnFrameInitializedPointer += HandleManoMotionFrameInitializedPointer;
            InputManagerBase.OnFrameInitializedPointers += HandleManoMotionFrameInitializedPointers;
            InputManagerBase.OnFrameUpdated += HandleNewFrame;
            InputManagerBase.OnFrameResized += HandleManomotionFrameUpdated;
            InputManagerStereoBase.OnCameraInfoUpdated += HandleCameraInfoUpdated;
        }
        else
        {
            gameObject.SetActive(false);
            Debug.LogWarning("More than 1 Manomotionmanager in scene");
            return;
        }

        SetManoMotionSettings(ImageFormat.BGRA_FORMAT, license.LicenseKey);
        InstantiateHandInfos();
        InitiateLibrary();
        SetUnityConditions();

        renderEventFunc = GetRenderEventFunc();
        StartCoroutine(Process());
    }

    IEnumerator Process()
    {
        while (true)
        {
            yield return null;
            if (inputManager.IsFrameUpdated())
            {
                GL.IssuePluginEvent(renderEventFunc, 1);
            }
            ProcessFrame(); 
        }
    }

    private void OnDestroy()
    {
        ManoUtils.OnOrientationChanged = null;
        InputManagerBase.OnChangeCamera = null;
        InputManagerBase.OnAddonSet = null;
        InputManagerBase.OnFrameInitialized = null;
        InputManagerBase.OnFrameInitializedPointer = null;
        InputManagerBase.OnFrameInitializedPointers = null;
        InputManagerBase.OnFrameUpdated = null;
        InputManagerBase.OnFrameResized = null;
        InputManagerStereoBase.OnCameraInfoUpdated = null;
    }

    /// <summary>
    /// Initiates the library.
    /// </summary>
    protected void InitiateLibrary()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        setBundleID(license.BundleID);
#endif
        Init();
    }

    #endregion

    #region update_methods

    /// <summary>
    /// Updates the orientation information as captured from the device to the Session
    /// </summary>
    protected void HandleOrientationChanged()
    {
        manomotionSession.orientation = ManoUtils.Instance.Orientation;

        if (inputManager.IsFrontFacing)
        {
            manomotionSession.orientation = manomotionSession.orientation switch
            {
                SupportedOrientation.PORTRAIT => SupportedOrientation.PORTRAIT_FRONT_FACING,
                SupportedOrientation.PORTRAIT_UPSIDE_DOWN => SupportedOrientation.PORTRAIT_UPSIDE_DOWN_FRONT_FACING,
                SupportedOrientation.LANDSCAPE_LEFT => SupportedOrientation.LANDSCAPE_LEFT_FRONT_FACING,
                SupportedOrientation.LANDSCAPE_RIGHT => SupportedOrientation.LANDSCAPE_RIGHT_FRONT_FACING,
                _ => SupportedOrientation.UNKNOWN
            };
        }

        HandleManomotionFrameUpdated(currentManomotionFrame);
    }

    /// <summary>
    /// Respond to the event of a ManoMotionFrame being sent for processing.
    /// </summary>
    /// <param name="newFrame">The new frame to process</param>
    protected void HandleNewFrame(ManoMotionFrame newFrame)
    {
        if (visualizationInfo.occlusionRGB.width * visualizationInfo.occlusionRGB.height != MRframePixelColors.Length)
        {
            Debug.LogErrorFormat("UpdateTexturesWithNewInfo error MRFrame width {0} height{1} MRframepixelcolors length {2}", visualizationInfo.occlusionRGB.width, visualizationInfo.occlusionRGB.height, MRframePixelColors.Length);
            return;
        }

        // Update camera rgb texture
        visualizationInfo.rgbImage = newFrame.texture;
        visualizationInfo.rgbImage.Apply();
        OnManoMotionFrameProcessed?.Invoke();

        // Update hand occlusion textures
        visualizationInfo.occlusionRGB.SetPixels32(MRframePixelColors);
        visualizationInfo.occlusionRGB.Apply();
        visualizationInfo.occlusionRGBsecond.SetPixels32(MRframePixelColors1);
        visualizationInfo.occlusionRGBsecond.Apply();
    }

    /// <summary>
    /// Sets the Application to not go to sleep mode as well as the requested framerate.
    /// </summary>
    protected void SetUnityConditions()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    #endregion
}