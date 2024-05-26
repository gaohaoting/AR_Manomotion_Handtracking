using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public abstract class ManomotionManager : MonoBehaviour
{
    //[Tooltip("Insert the key gotten from the webpage here https://www.manomotion.com/my-account/licenses/")]
    [SerializeField] protected ManoMotionLicense license;
    [SerializeField] protected ManoLicenseStatus manoLicense;
    protected ManoSettings manoSettings;
    protected InputManagerBase inputManager;

    /// The information about the hand
    [SerializeField] protected HandInfo[] handInfos;
    protected VisualizationInfo visualizationInfo;

    /// The information about the Session values, the SDK settings
    [SerializeField] protected Session manomotionSession;

    /// The width and height of the images processed
    protected int width, height;
    protected int fps, processingTime;

    protected bool initialized;
    protected float fpsCooldown = 0;
    protected int frameCount = 0;

    /// Frame pixels
    protected Color32[] MRframePixelColors, MRframePixelColors1;
    protected ManoMotionFrame currentManomotionFrame;
    protected ImageFormat currentImageFormat;

    /// Smoothing vlaue
    [HideInInspector] public float oneEuroFilterSmoothing;

    protected static ManomotionManager instance;

    #region Events

    ///Sends information after each frame is processed by the SDK.
    public static Action OnManoMotionFrameProcessed;

    ///Sends information after the license is checked by the SDK.
    public static Action OnManoMotionLicenseInitialized;

    ///Sends information when changing between 2D and 3D joints
    public static Action<SkeletonModel> OnSkeletonActivated;

    ///Sends information when changing the smoothing value
    public static Action<float> OnSmoothingValueChanged;

    public static Action OnTwoHandsToggle;

    public static Action<bool> OnContourToggle;

    #endregion

    #region Properties
    internal int ProcessingTime => processingTime;

    internal int Fps => fps;

    internal int Width => width;

    internal int Height => height;

    internal VisualizationInfo VisualizationInfo => visualizationInfo;

    internal HandInfo[] HandInfos => handInfos;

    public Session ManomotionSession => manomotionSession;

    public static ManomotionManager Instance => instance;

    public string LicenseKey => license.LicenseKey;

    public ManoLicenseStatus ManoLicense => manoLicense;

    public ManoSettings ManoSettings => manoSettings;

    public InputManagerBase InputManager => inputManager;

    #endregion

    #region Library imports

#if UNITY_IOS
    const string library = "__Internal";
#elif UNITY_ANDROID
    const string library = "manomotion";
#else
    const string library = "manomotion";
#endif

    [DllImport(library)]
    private static extern void init(ManoSettings settings, ref ManoLicenseStatus manoLicense);

    [DllImport(library)]
    private static extern void stop();

#if UNITY_EDITOR || UNITY_STANDALONE
    [DllImport(library)]
    protected static extern void setBundleID(string bundleID);
#endif

    [DllImport(library)]
    protected static extern void SetTextureFromUnity(IntPtr textureHandleLeft, int width, int height, int splittingFactor);

    [DllImport(library)]
    protected static extern void SetTexturesFromUnity(IntPtr textureHandleLeft, IntPtr textureHandleRight, int width, int height, int deviceType, int splittingFactor);

    /// <summary>
    /// Tell the SDK to process the current frame
    /// </summary>
    [DllImport(library)]
    protected static extern IntPtr GetRenderEventFunc();

    [DllImport(library)]
    private static extern int copyHandInfo(ref HandInfo first_hand_info, ref HandInfo second_hand_info, ref Session manomotion_session);

    [DllImport(library)]
    private static extern void setMRFrameArrays(Color32[] frame, Color32[] frameSecond);

    [DllImport(library)]
    private static extern void setResolution(int width, int height);

    [DllImport(library)]
    private static extern void getPerformanceInfo(ref int processingTime, ref int getImageTime);

#if !UNITY_IOS
    [DllImport(library)]
    private static extern void setCameraParameterRGBUnity(CameraInfo[] camera, int elements);

#endif

#endregion

#region init_wrappers

    protected void Init()
    {
        manoLicense = new ManoLicenseStatus();
        init(manoSettings, ref manoLicense);
        initialized = true;

        OnManoMotionLicenseInitialized?.Invoke();
    }

    /// Stops the SDK from processing.
    public void StopProcessing()
    {
        stop();
    }

    /// <summary>
    /// Gets the updated information from the SDK
    /// </summary>
    protected void ProcessFrame()
    {
        int frameNumber = copyHandInfo(ref handInfos[0], ref handInfos[1], ref manomotionSession);
    }

    /// Gives instruction where frame pixels are stored.
    protected void SetMRFrameArrays(Color32[] pixels, Color32[] pixels1)
    {
        //Debug.Log("Called setMRFrameArrays with " + pixels.Length + " & " + pixels1.Length);
        setMRFrameArrays(pixels, pixels1);
    }

#endregion

#region Awake/Start utils

    protected virtual void Awake()
    {
        visualizationInfo.rgbImage = new Texture2D(0, 0);
        visualizationInfo.rgbImageSecond = new Texture2D(0, 0);
        visualizationInfo.occlusionRGB = new Texture2D(0, 0);
        visualizationInfo.occlusionRGBsecond = new Texture2D(0, 0);
        visualizationInfo.occlusionRGB.filterMode = FilterMode.Point;
        visualizationInfo.occlusionRGBsecond.filterMode = FilterMode.Point;
    }

    protected void HandleManoMotionFrameInitializedPointer(Texture2D image, int splittingFactor)
    {
        SetTextureFromUnity(image.GetNativeTexturePtr(), image.width, image.height, splittingFactor);
    }

    protected void HandleManoMotionFrameInitializedPointers(Texture2D left, Texture2D right, int splittingFactor, int deviceType)
    {
        SetTexturesFromUnity(left.GetNativeTexturePtr(), right.GetNativeTexturePtr(), left.width, left.height, deviceType, splittingFactor);
    }

    /// <summary>
    /// Fills in the information needed in the manosettings Struct in order to initialize ManoMotion Tech.
    /// </summary>
    /// <param name="newPlatform">Requires the platform the app is going to be used in.</param>
    /// <param name="newImageFormat">Requires the image format that ManoMotion tech is going to process</param>
    /// <param name="newLicenseKey">Requires a Serial Key that is valid for ManoMotion tech and it linked with the current boundle ID used in the application.</param>
    public void SetManoMotionSettings(ImageFormat newImageFormat, string newLicenseKey)
    {
#if UNITY_IOS
        manoSettings.platform = Platform.UNITY_IOS;
#endif

#if UNITY_ANDROID
        manoSettings.platform = Platform.UNITY_ANDROID;
#endif
        manoSettings.imageFormat = newImageFormat;
        manoSettings.serialKey = newLicenseKey;
    }

    /// <summary>
    /// Handles changes to Manomotion frame
    /// </summary>
    /// <param name="newFrame">The new frame to process</param>
    protected void HandleManomotionFrameUpdated(ManoMotionFrame newFrame)
    {
        if (newFrame.texture == null)
            return;
        currentManomotionFrame = newFrame;
        SetResolutionValues(newFrame.texture.width, newFrame.texture.height);
    }

    /// <summary>
    /// Sets the resolution values used throughout the initialization methods of the arrays and textures.
    /// </summary>
    /// <param name="width">Requires a width value.</param>
    /// <param name="height">Requires a height value.</param>
    protected void SetResolutionValues(int width, int height)
    {
        this.width = width;
        this.height = height;
        setResolution(width, height);

        visualizationInfo.rgbImage.Reinitialize(width, height);
        visualizationInfo.rgbImageSecond.Reinitialize(width, height);

        MRframePixelColors = new Color32[width * height];
        MRframePixelColors1 = new Color32[width * height];
        SetMRFrameArrays(MRframePixelColors, MRframePixelColors1);

        visualizationInfo.occlusionRGB.Reinitialize(width, height);
        visualizationInfo.occlusionRGB.SetPixels32(MRframePixelColors);
        visualizationInfo.occlusionRGB.Apply();
        visualizationInfo.occlusionRGBsecond.Reinitialize(width, height);
        visualizationInfo.occlusionRGBsecond.SetPixels32(MRframePixelColors1);
        visualizationInfo.occlusionRGBsecond.Apply();
    }

    /// <summary>
    /// Gets the correct manomotion_session addon from the inputManager
    /// </summary>
    /// <param name="addon">The addon used with the inputManager</param>
    protected void HandleAddOnSet(AddOn addon)
    {
        manomotionSession.addOn = addon;
    }

    /// <summary>
    /// Initializes the values for the hand information.
    /// </summary>
    protected void InstantiateHandInfos()
    {
        handInfos = new HandInfo[2];
        for (int i = 0; i < handInfos.Length; i++)
        {
            handInfos[i] = new HandInfo();
            handInfos[i].gestureInfo = new GestureInfo();
            handInfos[i].gestureInfo.manoClass = ManoClass.NO_HAND;
            handInfos[i].gestureInfo.handSide = HandSide.None;
            handInfos[i].gestureInfo.leftRightHand = LeftOrRightHand.NO_HAND;
            handInfos[i].trackingInfo = new TrackingInfo();
            handInfos[i].trackingInfo.boundingBox = new BoundingBox();
            handInfos[i].trackingInfo.boundingBox.topLeft = new Vector3();
        }
    }

    protected void HandleCameraInfoUpdated(CameraInfo[] cameraInfo)
    {
#if !UNITY_IOS
        setCameraParameterRGBUnity(cameraInfo, cameraInfo.Length);
#endif
    }

#endregion

#region Update methods

    void Update()
    {
        CalculateFPSAndProcessingTime();
    }

    /// <summary>
    /// Calculates the Frames Per Second in the application and retrieves the estimated Processing time.
    /// </summary>
    protected void CalculateFPSAndProcessingTime()
    {
        fpsCooldown += Time.deltaTime;
        frameCount++;
        if (fpsCooldown >= 1)
        {
            fps = frameCount;
            frameCount = 0;
            fpsCooldown -= 1;
            CalculateProcessingTime();
        }
    }

    /// <summary>
    /// Calculates the elapses time needed for processing the frame.
    /// </summary>
    protected void CalculateProcessingTime()
    {
        int processing = 0;
        int image = 0;
        getPerformanceInfo(ref processing, ref image);
        processingTime = processing;
    }

#endregion

#region Manomotion feature settings
    /// <summary>
    /// Sets the ManoMotion tracking smoothing value throught the gizmo slider, the bigger the value the stronger the smoothing.
    /// </summary>
    /// <param name="slider">Slider</param>
    public void SetManoMotionSmoothingValue(Slider slider)
    {
        //manomotion_session.smoothing_controller = slider.value;
    }

    /// <summary>
    /// Sets the ManoMotion tracking smoothing value throught the gizmo slider, the bigger the value the stronger the smoothing.
    /// </summary>
    /// <param name="slider">Slider</param>
    public void SetOneEuroFilterSmoothingValue(Slider slider)
    {
        oneEuroFilterSmoothing = slider.value;
        OnSmoothingValueChanged?.Invoke(slider.value);
    }

    /// <summary>
    /// Enum to list the different skeleton models
    /// </summary>
    public enum SkeletonModel { SKEL_2D = 0, SKEL_3D = 1 }

    /// <summary>
    /// Lets the SDK know if it should calculate the Skeleton in 3D or 2D.
    /// And gives information to SkeletonManager which skeleton model to display.
    /// </summary>
    /// <param name="condition">run or not</param>
    public void ShouldCalculateSkeleton3D(bool condition)
    {
        int boolValue = condition ? 1 : 0;
        manomotionSession.enabledFeatures.skeleton3D = boolValue;
        OnSkeletonActivated?.Invoke((SkeletonModel)boolValue);
    }

    /// <summary>
    /// Lets the SDK know that it needs to calculate the Gestures.
    /// </summary>
    /// <param name="condition">run or not</param>
    public void ShouldCalculateGestures(bool condition)
    {
        manomotionSession.enabledFeatures.gestures = condition ? 1 : 0;
    }

    /// <summary>
    /// Lets the SDK know if it should run fast mode or not.
    /// </summary>
    /// <param name="condition">run or not</param>
    public void ShouldRunFastMode(bool condition)
    {
        manomotionSession.enabledFeatures.fastMode = condition ? 1 : 0;
    }

    /// <summary>
    /// Lets the SDK know if it should calculate wrist information.
    /// </summary>
    /// <param name="condition">run or not</param>
    public void ShouldRunWristInfo(bool condition)
    {
        manomotionSession.enabledFeatures.wristInfo = condition ? 1 : 0;
    }

    /// <summary>
    /// Lets the SDK know if it should calculate finger information.
    /// 4 will run the finger_info for the ring finger, 0 is off.
    /// </summary>
    /// <param name="condition">run or not</param>
    public void ShouldRunFingerInfo(bool condition)
    {
        manomotionSession.enabledFeatures.fingerInfo = condition ? 4 : 0;
    }

    /// <summary>
    /// Toggle wich finger to use for the finger information.
    /// 0 = off
    /// 1 = Thumb
    /// 2 = Index
    /// 3 = Middle
    /// 4 = Ring
    /// 5 = Pinky
    /// </summary>
    /// <param name="index">int between 0 and 5, 0 is off and 1-5 is the different fingers</param>
    public void ToggleFingerInfoFinger(int index)
    {
        int minIndex = 0;
        int maxIndex = 5;

        if (index >= minIndex && index <= maxIndex)
        {
            manomotionSession.enabledFeatures.fingerInfo = index;
        }
        else
        {
            Debug.Log("index needs to between 0 and 5, current index = " + index);
        }
    }

    /// <summary>
    /// Lets the SDK know if it should calculate hand contour.
    /// </summary>
    /// <param name="condition">run or not</param>
    public void ShouldRunContour(bool condition)
    {
        manomotionSession.enabledFeatures.contour = condition ? 1 : 0;
        OnContourToggle?.Invoke(condition);
    }

    /// <summary>
    /// Lets the SDK know if it should calculate detection 1 or 2 hands.
    /// </summary>
    /// <param name="condition">run or not</param>
    public void ShouldRunTwoHands(bool condition)
    {
        manomotionSession.enabledFeatures.twoHands = condition ? 1 : 0;
        OnTwoHandsToggle?.Invoke();
    }
#endregion

    /// <summary>
    /// Returns true and gives back the hand info of the left/right hand specified.
    /// </summary>
    public bool TryGetHandInfo(LeftOrRightHand leftRight, out HandInfo handInfo)
    {
        handInfo = default;

        for (LeftOrRightHand hand = LeftOrRightHand.LEFT_HAND; hand <= LeftOrRightHand.RIGHT_HAND; hand++)
        {
            HandInfo currentHand = handInfos[(int)hand];
            if (currentHand.gestureInfo.leftRightHand.Equals(leftRight) && currentHand.trackingInfo.skeleton.confidence == 1)
            {
                handInfo = currentHand;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true and gives back the hand info of the left/right hand specified.
    /// </summary>
    public bool TryGetHandInfo(LeftOrRightHand leftRight, out HandInfo handInfo, out int handIndex)
    {
        handInfo = default;
        handIndex = 0;

        for (LeftOrRightHand hand = LeftOrRightHand.LEFT_HAND; hand <= LeftOrRightHand.RIGHT_HAND; hand++)
        {
            HandInfo currentHand = handInfos[(int)hand];
            if (currentHand.gestureInfo.leftRightHand.Equals(leftRight) && currentHand.trackingInfo.skeleton.confidence == 1)
            {
                handInfo = currentHand;
                handIndex = (int)hand;
                return true;
            }
        }

        return false;
    }
}
