using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles wich features / gizmos that will be calculated and visualized.
/// </summary>
public class GizmoManager : MonoBehaviour
{
    #region Singleton
    private static GizmoManager _instance;
    public static GizmoManager Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }
    #endregion

    /// The color of the hand state UI.
    public Color disabledStateColor;

    [SerializeField]
    private Image[] stateImages, stateImages_second;

    [SerializeField]
    private GameObject leftRightGizmo, leftRightGizmo_second;
    [SerializeField]
    private GameObject handStatesGizmo, handStatesGizmo_second;
    [SerializeField]
    private GameObject manoClassGizmo, manoClassGizmo_second;
    [SerializeField]
    private GameObject handSideGizmo, handSideGizmo_second;
    [SerializeField]
    private GameObject continuousGestureGizmo, continuousGestureGizmo_second;
    [SerializeField]
    private GameObject contourGizmoPrefab;
    [SerializeField]
    private GameObject triggerTextPrefab;
    [SerializeField]
    private GameObject flagHolderGizmo, flagHolderGizmo_second;
    [SerializeField]
    private GameObject smoothingSliderControler;
    [SerializeField]
    private GameObject depthEstimationGizmo, depthEstimationGizmo_second;

    /// Contour 
    private GameObject contourInformationGizmo, contourInformationGizmo_second;
    public ContourGizmo contourGizmo, contourGizmo_second;


    /// Wrist information
    public WristInfoGizmo wristInfo, wristInfo_second;
    private GameObject wristInformationGizmo, wristInformationGizmo_second;
    [SerializeField]
    private GameObject wristInfoGizmoPrefab;

    /// Finger information
    public FingerInfoGizmo fingerInfo, fingerInfo_second;
    private GameObject fingerInformationGizmo, fingerInformationGizmo_second;
    [SerializeField]
    private GameObject fingerInfoGizmoPrefab;

    [SerializeField]
    private Text currentSmoothingValue;

    [SerializeField]
    private bool _showGestureAnalysis;

    [SerializeField]
    private bool _showPickDrop;
    [SerializeField]
    private bool _showGrabRelease;
    [SerializeField]
    private bool _showSwipes;

    private bool _showLeftRight;
    private bool _showHandStates;
    private bool _showManoClass;
    private bool _showHandSide;
    private bool _showContinuousGestures;

    [SerializeField]
    private bool _showWarnings;
    private bool _showPickTriggerGesture;
    private bool _showDropTriggerGesture;
    private bool _showSwipeHorizontalTriggerGesture;
    private bool _showSwipeVerticalTriggerGesture;
    [SerializeField]
    private bool _showClickTriggerGesture;
    private bool _showGrabTriggerGesture;
    private bool _showReleaseTriggerGesture;
    [SerializeField]
    private bool _showSmoothingSlider;
    [SerializeField]
    private bool _showDepthEstimation;

    [SerializeField]
    private bool _showWristInfo;
    [SerializeField]
    private bool _showFingerInfo;
    [SerializeField]
    private bool _showContour;
    [SerializeField]
    private bool skeleton3d;
    [SerializeField]
    private bool gestures;
    [SerializeField]
    private bool fastMode;
    [SerializeField]
    private bool _twoHands;

    private GameObject topFlag, topFlag_second;
    private GameObject leftFlag, leftFlag_second;
    private GameObject rightFlag, rightFlag_second;

    private Text leftRightText, leftRightText_second;
    private Text manoClassText, manoClassText_second;
    private Text handSideText, handSideText_second;
    private Text continuousGestureText, continuousGestureText_second;

    private TMP_Text fingerFlag, fingerFlag_second;
    private TMP_Text wristFlag, wristFlag_second;

    private TextMeshProUGUI depthEstimationValue;
    private TextMeshProUGUI depthEstimationValue_second;
    private Image depthFillAmmount;
    private Image depthFillAmmount_second;


    #region Properties

    public bool ShowLeftRight
    {
        get { return _showLeftRight; }
        set { _showLeftRight = value; }
    }

    public bool ShowGrabRelease
    {
        get { return _showGrabRelease; }
        set { _showGrabRelease = value; }
    }

    public bool ShowPickDrop
    {
        get { return _showPickDrop; }
        set { _showPickDrop = value; }
    }

    public bool ShowSwipes
    {
        get { return _showSwipes; }
        set { _showSwipes = value; }
    }

    public bool ShowGestureAnalysis
    {
        get
        {
            return _showGestureAnalysis;
        }

        set
        {
            _showGestureAnalysis = value;
        }
    }

    public bool ShowContinuousGestures
    {
        get
        {
            return _showContinuousGestures;
        }

        set
        {
            _showContinuousGestures = value;
        }
    }

    public bool ShowManoClass
    {
        get
        {
            return _showManoClass;
        }

        set
        {
            _showManoClass = value;
        }
    }

    public bool ShowHandSide
    {
        get
        {
            return _showHandSide;
        }

        set
        {
            _showHandSide = value;
        }
    }

    public bool ShowHandStates
    {
        get
        {
            return _showHandStates;
        }

        set
        {
            _showHandStates = value;
        }
    }

    public bool ShowWarnings
    {
        get
        {
            return _showWarnings;
        }

        set
        {
            _showWarnings = value;
        }
    }

    public bool ShowPickTriggerGesture
    {
        get
        {
            return _showPickTriggerGesture;
        }

        set
        {
            _showPickTriggerGesture = value;
        }
    }

    public bool ShowDropTriggerGesture
    {
        get
        {
            return _showDropTriggerGesture;
        }

        set
        {
            _showDropTriggerGesture = value;
        }
    }

    public bool ShowSwipeHorizontalTriggerGesture
    {
        get
        {
            return _showSwipeHorizontalTriggerGesture;
        }

        set
        {
            _showSwipeHorizontalTriggerGesture = value;
        }
    }

    public bool ShowSwipeVerticalTriggerGesture
    {
        get
        {
            return _showSwipeVerticalTriggerGesture;
        }

        set
        {
            _showSwipeVerticalTriggerGesture = value;
        }
    }

    public bool ShowClickTriggerGesture
    {
        get
        {
            return _showClickTriggerGesture;
        }

        set
        {
            _showClickTriggerGesture = value;
        }
    }

    public bool ShowGrabTriggerGesture
    {
        get
        {
            return _showGrabTriggerGesture;
        }

        set
        {
            _showGrabTriggerGesture = value;
        }
    }

    public bool ShowReleaseTriggerGesture
    {
        get
        {
            return _showReleaseTriggerGesture;
        }

        set
        {
            _showReleaseTriggerGesture = value;
        }
    }

    public bool ShowSmoothingSlider
    {
        get
        {
            return _showSmoothingSlider;
        }

        set
        {
            _showSmoothingSlider = value;
        }
    }

    public bool ShowDepthEstimation
    {
        get
        {
            return _showDepthEstimation;
        }
        set
        {
            _showDepthEstimation = value;
        }
    }

    public bool ShowSkeleton3d
    {
        get
        {
            return skeleton3d;
        }
        set
        {
            skeleton3d = value;
        }
    }

    public bool ShowGestures
    {
        get
        {
            return gestures;
        }
        set
        {
            gestures = value;
        }
    }

    public bool Fastmode
    {
        get
        {
            return fastMode;
        }
        set
        {
            fastMode = value;
        }
    }

    public bool ShowWristInfo
    {
        get
        {
            return _showWristInfo;
        }
        set
        {
            _showWristInfo = value;
        }
    }

    public bool ShowFingerInfo
    {
        get
        {
            return _showFingerInfo;
        }
        set
        {
            _showFingerInfo = value;
        }
    }

    public bool ShowContour
    {
        get
        {
            return _showContour;
        }
        set
        {
            _showContour = value;
        }
    }

    public bool TwoHands
    {
        get
        {
            return _twoHands;
        }
        set
        {
            _twoHands = value;
        }
    }

    #endregion

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(_instance);
        }
        _instance = this;

        SetGestureDescriptionParts();
        HighlightStatesToStateDetection(0, stateImages);
        HighlightStatesToStateDetection(0, stateImages_second);
        InitializeFlagParts();
        SetFlagDescriptionParts();
        InitializeTriggerPool();
        InstantiateTryOnGizmos();
    }

    private void Start()
    {
        SetFeaturesToCalculate();
    }

    /// <summary>
    /// Creates TryOnParent gameObjet and instantiates the try on and contour gizmos and place them under the TryOnParent.
    /// </summary>
    private void InstantiateTryOnGizmos()
    {
        GameObject tryOnParent = new GameObject();
        tryOnParent.name = "TryOnParent";
        InstantiateFingerGizmos(tryOnParent.transform);
        InstantiateWristGizmos(tryOnParent.transform);
        InstantiateContourGizmos(tryOnParent.transform);
    }

    /// <summary>
    /// Instantiates the wrist info gizmos.
    /// </summary>
    /// <param name="parent">The try on parent transform</param>
    private void InstantiateWristGizmos(Transform parent)
    {
        wristInformationGizmo = Instantiate(wristInfoGizmoPrefab, parent);
        wristInformationGizmo.name = "WristInfo";
        wristInformationGizmo_second = Instantiate(wristInfoGizmoPrefab, parent);
        wristInformationGizmo_second.name = "WristInfo_second";
        wristInfo = wristInformationGizmo.GetComponent<WristInfoGizmo>();
        wristInfo_second = wristInformationGizmo_second.GetComponent<WristInfoGizmo>();
    }

    /// <summary>
    /// Functio to get the gameobject of the contour 
    /// </summary>
    /// <param name="index">the index on the contour instance</param>
    /// <returns>the gameobject of the contour, it returns null if it does not exist</returns>
    public GameObject GetContour(int index)
    {
        if (index == 0)
            return contourInformationGizmo;
        else
            return contourInformationGizmo_second;
    }

    /// <summary>
    /// Instantiates the finger info gizmos.
    /// </summary>
    /// <param name="parent">The try on parent transform</param>
    private void InstantiateFingerGizmos(Transform parent)
    {
        fingerInformationGizmo = Instantiate(fingerInfoGizmoPrefab, parent);
        fingerInformationGizmo.name = "FingerInfo";
        fingerInformationGizmo_second = Instantiate(fingerInfoGizmoPrefab, parent);
        fingerInformationGizmo_second.name = "FingerInfo_second";
        fingerInfo = fingerInformationGizmo.GetComponent<FingerInfoGizmo>();
        fingerInfo_second = fingerInformationGizmo_second.GetComponent<FingerInfoGizmo>();
    }

    /// <summary>
    /// Instantiates the contour gizmos.
    /// </summary>
    /// <param name="parent">The try on parent transform</param>
    private void InstantiateContourGizmos(Transform parent)
    {
        contourInformationGizmo = Instantiate(contourGizmoPrefab, parent);
        contourInformationGizmo.name = "Contour";
        contourInformationGizmo_second = Instantiate(contourGizmoPrefab, parent);
        contourInformationGizmo_second.name = "Contour_second";
        contourGizmo = contourInformationGizmo.GetComponent<ContourGizmo>();
        contourGizmo_second = contourInformationGizmo_second.GetComponent<ContourGizmo>();
    }

    /// <summary>
    /// Sets which features that should be calculated
    /// </summary>
    private void SetFeaturesToCalculate()
    {
        ManomotionManager.Instance.ShouldCalculateGestures(ShowGestures);
        ManomotionManager.Instance.ShouldCalculateSkeleton3D(ShowSkeleton3d);
        ManomotionManager.Instance.ShouldRunFastMode(Fastmode);
        ManomotionManager.Instance.ShouldRunWristInfo(ShowWristInfo);
        ManomotionManager.Instance.ShouldRunFingerInfo(ShowFingerInfo);
        ManomotionManager.Instance.ShouldRunContour(ShowContour);
        ManomotionManager.Instance.ShouldRunTwoHands(TwoHands);
    }

    /// <summary>
    /// Updates the GestureInfo, TrackingInfo, Warning and Session every frame.
    /// Also updates all the display methods
    /// </summary>
    void Update()
    {
        GestureInfo gestureInfo = ManomotionManager.Instance.HandInfos[0].gestureInfo;
        TrackingInfo trackingInfo = ManomotionManager.Instance.HandInfos[0].trackingInfo;
        Warning warning = ManomotionManager.Instance.HandInfos[0].warning;
        Session session = ManomotionManager.Instance.ManomotionSession;

        if (ManomotionManager.Instance.ManomotionSession.enabledFeatures.twoHands == 1)
        {
            GestureInfo gestureInfo2 = ManomotionManager.Instance.HandInfos[1].gestureInfo;
            TrackingInfo trackingInfo2 = ManomotionManager.Instance.HandInfos[1].trackingInfo;
            Warning warning2 = ManomotionManager.Instance.HandInfos[1].warning;

            DisplayContinuousGestures(gestureInfo2.manoGestureContinuous, continuousGestureText_second, continuousGestureGizmo_second);
            DisplayManoclass(gestureInfo2.manoClass, manoClassText_second, manoClassGizmo_second);
            DisplayTriggerGesture(gestureInfo2.manoGestureTrigger, trackingInfo2);
            DisplayHandState(gestureInfo2.state, stateImages_second, handStatesGizmo_second);
            DisplayHandSide(gestureInfo2.handSide, handSideText_second, handSideGizmo_second);
            DisplayApproachingToEdgeFlags(warning2, flagHolderGizmo_second, rightFlag_second, leftFlag_second, topFlag_second);
            DisplayFingerFlags(trackingInfo2, gestureInfo2);
            DisplayWristFlags(trackingInfo2, gestureInfo2);
            DisplayLeftRight(gestureInfo2.leftRightHand, leftRightText_second, leftRightGizmo_second);
            DisplayDepthEstimation(trackingInfo2.depthEstimation, depthEstimationGizmo_second, depthEstimationValue_second, depthFillAmmount_second);
            DisplayWristInformation(wristInformationGizmo_second, wristInfo_second, trackingInfo2, gestureInfo2);
            DisplayFingerInformation(fingerInformationGizmo_second, fingerInfo_second, trackingInfo2, gestureInfo2);
            DisplayContour(contourInformationGizmo_second, trackingInfo2, contourGizmo_second);
        }

        //Disbale the two hand gizmos when using one hand.
        else
        {
            DisableGizmo(flagHolderGizmo_second);
            DisableGizmo(depthEstimationGizmo_second);
            DisableGizmo(continuousGestureGizmo_second);
            DisableGizmo(manoClassGizmo_second);
            DisableGizmo(handSideGizmo_second);
            DisableGizmo(leftRightGizmo_second);
            DisableGizmo(handStatesGizmo_second);
            DisableGizmo(wristInformationGizmo_second);
            DisableGizmo(fingerInformationGizmo_second);
            DisableGizmo(contourInformationGizmo_second);
        }

        DisplayContinuousGestures(gestureInfo.manoGestureContinuous, continuousGestureText, continuousGestureGizmo);
        DisplayManoclass(gestureInfo.manoClass, manoClassText, manoClassGizmo);
        DisplayTriggerGesture(gestureInfo.manoGestureTrigger, trackingInfo);
        DisplayHandState(gestureInfo.state, stateImages, handStatesGizmo);
        DisplayHandSide(gestureInfo.handSide, handSideText, handSideGizmo);
        DisplayApproachingToEdgeFlags(warning, flagHolderGizmo, rightFlag, leftFlag, topFlag);
        DisplayFingerFlags(trackingInfo, gestureInfo);
        DisplayWristFlags(trackingInfo, gestureInfo);
        DisplayCurrentsmoothingValue(session);
        DisplaySmoothingSlider();
        DisplayDepthEstimation(trackingInfo.depthEstimation, depthEstimationGizmo, depthEstimationValue, depthFillAmmount);
        DisplayWristInformation(wristInformationGizmo, wristInfo, trackingInfo, gestureInfo);
        DisplayFingerInformation(fingerInformationGizmo, fingerInfo, trackingInfo, gestureInfo);
        DisplayContour(contourInformationGizmo, trackingInfo, contourGizmo);
        DisplayLeftRight(gestureInfo.leftRightHand, leftRightText, leftRightGizmo);
    }

    #region Display Methods

    /// <summary>
    /// Disbale gizmo that will not be used at the moment
    /// </summary>
    /// <param name="gizmo">the gizmo to be disbaled.</param>
    private void DisableGizmo(GameObject gizmo)
    {
        gizmo.SetActive(false);
    }

    /// <summary>
    /// Displays the Wrist Information from WristInfoGizmo for the detected hand if feature is on.
    /// </summary>
    private void DisplayWristInformation(GameObject gizmo, WristInfoGizmo wristInfo, TrackingInfo trackingInfo, GestureInfo gestureInfo)
    {
        gizmo.SetActive(ShowWristInfo);

        try
        {
            if (ShowWristInfo)
            {
                wristInfo.ShowWristInformation(trackingInfo, gestureInfo);
            }

        }
        catch (Exception)
        {
            Debug.Log("Cant show wrist information");
        }
    }

    /// <summary>
    /// Dispalys the Finger information from FingerInfoGizmo for the detected hand if feature is on.
    /// </summary>
    private void DisplayFingerInformation(GameObject gizmo, FingerInfoGizmo fingerInfo, TrackingInfo trackingInfo, GestureInfo gestureInfo)
    {
        gizmo.SetActive(ShowFingerInfo);

        try
        {
            if (ShowFingerInfo)
            {
                fingerInfo.ShowFingerInformation(trackingInfo, gestureInfo);
            }
        }

        catch (Exception)
        {

        }

    }

    /// <summary>
    /// Displays the hand contour from ContourGizmo if feature is on.
    /// </summary>
    private void DisplayContour(GameObject gizmo, TrackingInfo trackingInfo, ContourGizmo contour)
    {
        gizmo.SetActive(ShowContour);


        if (ShowContour)
        {
            contour.ShowContour(trackingInfo);
        }
        else
        {
            // Debug.Log("Cant show hand contour");
        }
    }

    /// <summary>
    /// Displays the depth estimation of the detected hand.
    /// </summary>
    /// <param name="depthEstimation">Requires the float value of depth estimation.</param>

    /// <summary>
    /// Displays the depth estimation of the detected hand.
    /// </summary>
    /// <param name="depthEstimation">Requires the float value of depth estimation.</param>
    /// <param name="gizmo">The current deoth gizmo</param>
    /// <param name="estimationValue">Depth esitmation text field</param>
    /// <param name="fillAmount">Depth estimation image</param>
    void DisplayDepthEstimation(float depthEstimation, GameObject gizmo, TextMeshProUGUI estimationValue, Image fillAmount)
    {
        gizmo.SetActive(ShowDepthEstimation);

        if (!estimationValue)
        {
            estimationValue = gizmo.transform.Find("DepthValue").gameObject.GetComponent<TextMeshProUGUI>();
        }
        if (!fillAmount)
        {
            fillAmount = gizmo.transform.Find("CurrentLevel").gameObject.GetComponent<Image>();
        }

        if (ShowDepthEstimation)
        {
            estimationValue.text = depthEstimation.ToString("F2");
            fillAmount.fillAmount = depthEstimation;
        }
    }

    /// <summary>
    /// Displays in text value the current smoothing value of the session
    /// </summary>
    /// <param name="session">Requires a Session.</param>
    void DisplayCurrentsmoothingValue(Session session)
    {
        if (smoothingSliderControler.activeInHierarchy)
        {
            //currentSmoothingValue.text = "Tracking smoothing: " + session.smoothing_controller.ToString("F2");
            currentSmoothingValue.text = "Tracking smoothing: " + ManomotionManager.Instance.oneEuroFilterSmoothing.ToString("F2");
        }
    }

    /// <summary>
    /// Displays information regarding the detected manoclass
    /// </summary>
    /// <param name="manoclass">Requires a Manoclass.</param>
    void DisplayManoclass(ManoClass manoclass, Text textToModify, GameObject gizmo)
    {
        gizmo.SetActive(ShowGestureAnalysis);
        if (ShowGestureAnalysis)
        {
            switch (manoclass)
            {
                case ManoClass.NO_HAND:
                    textToModify.text = "Manoclass: No Hand";
                    break;
                case ManoClass.GRAB_GESTURE:
                    textToModify.text = "Manoclass: Grab Class";
                    break;
                case ManoClass.PINCH_GESTURE:
                    textToModify.text = "Manoclass: Pinch Class";
                    break;
                case ManoClass.POINTER_GESTURE:
                    textToModify.text = "Manoclass: Pointer Class";
                    break;
                default:
                    textToModify.text = "Manoclass: No Hand";
                    break;
            }
        }
    }

    /// <summary>
    /// Displays information regarding the detected manoclass
    /// </summary>
    /// <param name="manoGestureContinuous">Requires a continuous Gesture.</param>
    void DisplayContinuousGestures(ManoGestureContinuous manoGestureContinuous, Text textToModify, GameObject gizmo)
    {
        gizmo.SetActive(ShowGestureAnalysis);

        if (ShowGestureAnalysis)
        {
            switch (manoGestureContinuous)
            {
                case ManoGestureContinuous.CLOSED_HAND_GESTURE:
                    textToModify.text = "Continuous: Closed Hand";
                    break;
                case ManoGestureContinuous.OPEN_HAND_GESTURE:
                    textToModify.text = "Continuous: Open Hand";
                    break;
                case ManoGestureContinuous.HOLD_GESTURE:
                    textToModify.text = "Continuous: Hold";
                    break;
                case ManoGestureContinuous.OPEN_PINCH_GESTURE:
                    textToModify.text = "Continuous: Open Pinch";
                    break;
                case ManoGestureContinuous.POINTER_GESTURE:
                    textToModify.text = "Continuous: Pointing";
                    break;
                case ManoGestureContinuous.NO_GESTURE:
                    textToModify.text = "Continuous: None";
                    break;
                default:
                    textToModify.text = "Continuous: None";
                    break;
            }
        }
    }

    /// <summary>
    /// Displays wich hand side currently facing the camera.
    /// </summary>
    /// <param name="handside">Requires a ManoMotion Handside.</param>
    void DisplayHandSide(HandSide handside, Text textToModify, GameObject gizmo)
    {
        gizmo.SetActive(ShowGestureAnalysis);

        if (!handSideGizmo.activeInHierarchy && ShowHandSide)
        {
            handSideGizmo.SetActive(ShowHandSide);
        }

        if (ShowGestureAnalysis || ShowHandSide)
        {
            switch (handside)
            {
                case HandSide.Palmside:
                    textToModify.text = "Handside: Palm Side";
                    break;
                case HandSide.Backside:
                    textToModify.text = "Handside: Back Side";
                    break;
                case HandSide.None:
                    textToModify.text = "Handside: None";
                    break;
                default:
                    textToModify.text = "Handside: None";
                    break;
            }
        }
    }

    /// <summary>
    /// Displays wich hand currently facing the camera.
    /// </summary>
    /// <param name="isRight">Requiers the isRight from Gesture Inforamtion</param>
    void DisplayLeftRight(LeftOrRightHand isRight, Text textToModify, GameObject gizmo)
    {
        gizmo.SetActive(ShowGestureAnalysis);

        if (ShowGestureAnalysis)
        {
            switch (isRight)
            {
                case LeftOrRightHand.NO_HAND:
                    textToModify.text = "Hand: None";
                    break;
                case LeftOrRightHand.LEFT_HAND:
                    textToModify.text = "Hand: Left";
                    break;
                case LeftOrRightHand.RIGHT_HAND:
                    textToModify.text = "Hand: Right";
                    break;
                default:
                    textToModify.text = "Hand: None";
                    break;
            }
        }
    }

    /// <summary>
    /// Updates the visual information that showcases the hand state (how open/closed) it is
    /// </summary>
    /// <param name="handstate">Requires a handsate int</param>
    void DisplayHandState(int handstate, Image[] imageToModify, GameObject gizmo)
    {
        gizmo.SetActive(ShowGestureAnalysis);

        if (ShowGestureAnalysis)
        {
            HighlightStatesToStateDetection(handstate, imageToModify);
        }
    }

    private ManoGestureTrigger previousTrigger;

    /// <summary>
    /// Display Visual information of the detected trigger gesture and trigger swipes.
    /// In the case where a click is intended (Open pinch, Closed Pinch, Open Pinch) we are clearing out the visual information that are generated from the pick/drop
    /// </summary>
    /// <param name="triggerGesture">Requires an input from ManoGestureTrigger.</param>
    /// <param name="trackingInfo">Requires an input of tracking info.</param>
    void DisplayTriggerGesture(ManoGestureTrigger triggerGesture, TrackingInfo trackingInfo)
    {

        if (triggerGesture != ManoGestureTrigger.NO_GESTURE)
        {

            if (_showPickDrop)
            {
                if (triggerGesture == ManoGestureTrigger.PICK)
                {
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.PICK);
                }
            }

            if (_showPickDrop)
            {
                if (triggerGesture == ManoGestureTrigger.DROP)
                {
                    if (previousTrigger != ManoGestureTrigger.CLICK)
                    {
                        TriggerDisplay(trackingInfo, ManoGestureTrigger.DROP);
                    }
                }
            }

            if (_showClickTriggerGesture)
            {
                if (triggerGesture == ManoGestureTrigger.CLICK)
                {
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.CLICK);
                    if (GameObject.Find("PICK"))
                    {
                        GameObject.Find("PICK").SetActive(false);
                    }
                }
            }

            if (_showSwipes)
            {
                if (triggerGesture == ManoGestureTrigger.SWIPE_LEFT)
                {
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.SWIPE_LEFT);
                }
            }

            if (_showSwipes)
            {
                if (triggerGesture == ManoGestureTrigger.SWIPE_RIGHT)
                {
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.SWIPE_RIGHT);
                }
            }

            if (_showSwipes)
            {
                if (triggerGesture == ManoGestureTrigger.SWIPE_UP)
                {
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.SWIPE_UP);
                }
            }

            if (_showSwipes)
            {
                if (triggerGesture == ManoGestureTrigger.SWIPE_DOWN)
                {
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.SWIPE_DOWN);
                }
            }

            if (_showGrabRelease)
            {
                if (triggerGesture == ManoGestureTrigger.GRAB_GESTURE)
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.GRAB_GESTURE);
            }

            if (_showGrabRelease)
            {
                if (triggerGesture == ManoGestureTrigger.RELEASE_GESTURE)
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.RELEASE_GESTURE);
            }
        }

        previousTrigger = triggerGesture;
    }

    public List<GameObject> triggerObjectPool = new List<GameObject>();
    public int amountToPool = 20;

    /// <summary>
    /// Initializes the object pool for trigger gestures.
    /// </summary>
    private void InitializeTriggerPool()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject newTriggerObject = Instantiate(triggerTextPrefab);
            newTriggerObject.transform.SetParent(transform);
            newTriggerObject.SetActive(false);
            triggerObjectPool.Add(newTriggerObject);
        }
    }

    /// <summary>
    /// Gets the current pooled trigger object.
    /// </summary>
    /// <returns>The current pooled trigger.</returns>
    private GameObject GetCurrentPooledTrigger()
    {
        for (int i = 0; i < triggerObjectPool.Count; i++)
        {
            if (!triggerObjectPool[i].activeInHierarchy)
            {
                return triggerObjectPool[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Displays the visual information of the performed trigger gesture.
    /// </summary>
    /// <param name="bounding_box">Bounding box.</param>
    /// <param name="triggerGesture">Trigger gesture.</param>
    void TriggerDisplay(TrackingInfo trackingInfo, ManoGestureTrigger triggerGesture)
    {

        if (GetCurrentPooledTrigger())
        {
            GameObject triggerVisualInformation = GetCurrentPooledTrigger();

            triggerVisualInformation.SetActive(true);
            triggerVisualInformation.name = triggerGesture.ToString();
            triggerVisualInformation.GetComponent<TriggerGizmo>().InitializeTriggerGizmo(triggerGesture);
            //triggerVisualInformation.GetComponent<RectTransform>().position = Camera.main.ViewportToScreenPoint(trackingInfo.palmCenter);

        }
    }

    /// <summary>
    /// Visualizes the current hand state by coloring white the images up to that value and turning grey the rest
    /// </summary>
    /// <param name="stateValue">Requires a hand state value to assign the colors accordingly </param>
    void HighlightStatesToStateDetection(int stateValue, Image[] imagesToModify)
    {
        for (int i = 0; i < stateImages.Length; i++)
        {
            if (i > stateValue)
            {
                imagesToModify[i].color = disabledStateColor;
            }
            else
            {
                imagesToModify[i].color = Color.white;
            }
        }
    }

    /// <summary>
    /// Highlights the edges of the screen according to the warning given by the ManoMotion Manager
    /// </summary>
    /// <param name="warning">Requires a Warning.</param>
    void DisplayApproachingToEdgeFlags(Warning warning, GameObject gizmo, GameObject right, GameObject left, GameObject top)
    {
        if (_showWarnings)
        {
            if (!gizmo.activeInHierarchy)
            {
                gizmo.SetActive(true);
            }

            right.SetActive(warning == Warning.WARNING_APPROACHING_RIGHT_EDGE);
            top.SetActive(warning == Warning.WARNING_APPROACHING_UPPER_EDGE);
            left.SetActive(warning == Warning.WARNING_APPROACHING_LEFT_EDGE);
        }
        else
        {
            if (gizmo.activeInHierarchy)
            {
                gizmo.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Prints out the wrist information error codeif wrist information can´t be calculated correctly.
    /// </summary>
    /// <param name="trackingInfo"></param>
    private void DisplayWristFlags(TrackingInfo trackingInfo, GestureInfo gestureInfo)
    {
        if (wristFlag != null)
        {
            if (ShowWristInfo && gestureInfo.manoClass != ManoClass.NO_HAND)
            {
                switch (trackingInfo.wristInfo.wristWarning)
                {
                    case 0:
                        wristFlag.text = "";
                        break;
                    case 2000:
                        wristFlag.text = "WRIST ERROR[2000]";
                        break;
                    default:
                        wristFlag.text = "";
                        break;
                }
            }

            else
            {
                wristFlag.text = "";
            }
        }
    }

    /// <summary>
    /// Prints out the finger infor´mation error code if finger information can´t be calculated correctly.
    /// </summary>
    /// <param name="trackingInfo">tracking information</param>
    private void DisplayFingerFlags(TrackingInfo trackingInfo, GestureInfo gestureInfo)
    {
        if (fingerFlag != null)
        {
            if (ShowFingerInfo && gestureInfo.manoClass != ManoClass.NO_HAND)
            {
                switch (trackingInfo.fingerInfo.fingerWarning)
                {
                    case 0:
                        fingerFlag.text = "";
                        break;
                    case 1001:
                        fingerFlag.text = "FINGER ERROR[1001]";
                        break;
                    case 1002:
                        fingerFlag.text = "FINGER POINTS CLOSE TO EDGE[1002]";
                        break;
                    case 1003:
                        fingerFlag.text = "FINGER POINTS OUTSIDE FRAME[1003]";
                        break;
                    case 1004:
                        fingerFlag.text = "FINGER POINTS TO CLOSE TO PALM[1004]";
                        break;
                    case 1005:
                        fingerFlag.text = "FINGER ERROR[1005]";
                        break;
                    case 1006:
                        fingerFlag.text = "FINGER ERROR[1006]";
                        break;
                    case 1007:
                        fingerFlag.text = "FINGER ERROR[1007]";
                        break;
                    case 1008:
                        fingerFlag.text = "FINGER ERROR[1008]";
                        break;
                    case 1009:
                        fingerFlag.text = "FINGER ERROR[1009]";
                        break;
                    case 1010:
                        fingerFlag.text = "FINGER ERROR[1010]";
                        break;
                    case 1011:
                        fingerFlag.text = "FINGER ERROR[1011]";
                        break;
                    case 1012:
                        fingerFlag.text = "FINGER ERROR[1012]";
                        break;
                    case 1013:
                        fingerFlag.text = "FINGER ERROR[1013]";
                        break;
                    default:
                        fingerFlag.text = "";
                        break;
                }
            }

            else
            {
                fingerFlag.text = "";
            }
        }
    }

    /// <summary>
    /// Displayes the smoothing slider.
    /// </summary>
    /// <param name="display">If set to <c>true</c> display.</param>
    public void ShouldDisplaySmoothingSlider(bool display)
    {
        smoothingSliderControler.SetActive(display);
    }

    /// <summary>
    /// Displays the smoothing slider that controls the level of delay applied to the calculations for Tracking Information.
    /// </summary>
    public void DisplaySmoothingSlider()
    {
        smoothingSliderControler.SetActive(_showSmoothingSlider);
    }

    /// <summary>
    /// Initializes the components of the Manoclass,Continuous Gesture and Trigger Gesture Gizmos
    /// </summary>
    void SetGestureDescriptionParts()
    {
        manoClassText = manoClassGizmo.transform.Find("Description").GetComponent<Text>();
        handSideText = handSideGizmo.transform.Find("Description").GetComponent<Text>();
        continuousGestureText = continuousGestureGizmo.transform.Find("Description").GetComponent<Text>();
        leftRightText = leftRightGizmo.transform.Find("Description").GetComponent<Text>();

        manoClassText_second = manoClassGizmo_second.transform.Find("Description").GetComponent<Text>();
        handSideText_second = handSideGizmo_second.transform.Find("Description").GetComponent<Text>();
        continuousGestureText_second = continuousGestureGizmo_second.transform.Find("Description").GetComponent<Text>();
        leftRightText_second = leftRightGizmo_second.transform.Find("Description").GetComponent<Text>();
    }

    /// <summary>
    /// Initialized the text componets for displaying the finger and wrist flags.
    /// </summary>
    void SetFlagDescriptionParts()
    {
        try
        {
            fingerFlag = flagHolderGizmo.transform.Find("FingerFlag").GetComponent<TMP_Text>();
            fingerFlag_second = flagHolderGizmo_second.transform.Find("FingerFlag").GetComponent<TMP_Text>();
        }
        catch (Exception ex)
        {
            Debug.Log("Cant find finger flag TMP_Text.");
            Debug.Log(ex);
        }

        try
        {
            wristFlag = flagHolderGizmo.transform.Find("WristFlag").GetComponent<TMP_Text>();
            wristFlag_second = flagHolderGizmo_second.transform.Find("WristFlag").GetComponent<TMP_Text>();
        }
        catch (Exception ex)
        {
            Debug.Log("Cant find wrist flag TMP_Text.");
            Debug.Log(ex);
        }
    }

    /// <summary>
    /// Initializes the components for the visual illustration of warnings related to approaching edges flags.
    /// </summary>
    void InitializeFlagParts()
    {
        topFlag = flagHolderGizmo.transform.Find("Top").gameObject;
        rightFlag = flagHolderGizmo.transform.Find("Right").gameObject;
        leftFlag = flagHolderGizmo.transform.Find("Left").gameObject;

        topFlag_second = flagHolderGizmo_second.transform.Find("Top").gameObject;
        rightFlag_second = flagHolderGizmo_second.transform.Find("Right").gameObject;
        leftFlag_second = flagHolderGizmo_second.transform.Find("Left").gameObject;
    }

    #endregion

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
