using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ManoCursor : MonoBehaviour
{


    #region Singleton Region
    private static ManoCursor _instance;
    public static ManoCursor Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion
    private void Awake()
    {
        if (!_instance)
        {
            _instance = this;
            Debug.Log(gameObject.name + "I am the instance");
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed " + gameObject.name + " .More than 1 Manocursor instances in scene");
        }
    }
    #region Draw Wand Reference Region
    public DrawingWand drawingWand;
    public enum DrawingStates
    {
        StartDraw,
        Drawing,
        StopDraw,
        NotPainting
    }
    public DrawingStates currentDrawingState;
    #endregion






    #region Application Interaction Settings
    ManoClass drawGestureFamily;
    int drawStateThreshold;
    ManoGestureTrigger menuTriggerGesture;
    #endregion

    #region Menu interaction settings
    public float menuCoolDown = 0.25f;
    float menuClosedDuration = 0.0f;
    #endregion


    #region Pointing variables
    bool IamPointing;
    bool usingAppropriateState;
    bool pointingProperly;
    bool singleFingertipWasDetected;
    int pointingCounter;
    #endregion

    #region Visual components of cursor
    public GameObject particleHolder;
    private ParticleSystem particles;
    public MeshRenderer cursorRenderer;
    #endregion

    #region Raycasting variables
    PointerEventData pointer = new PointerEventData(EventSystem.current);
    GameObject lastHit = null;
    GameObject currentHit = null;
    #endregion


    HandInfo handInformation;


    private void Start()
    {
        currentDrawingState = DrawingStates.NotPainting;
        InitializeVisuals();
        InitializeInteractionValues();

    }

    /// <summary>
    /// Showcases the correct cursor illustration
    /// </summary>
    void AssignCorrectCursor()
    {
        if (pointingProperly)
        {
            if (DrawApplicationManager.Instance.currentState == DrawApplicationManager.ApplicationState.Drawing)
            {
                if (cursorRenderer.enabled)
                {
                    cursorRenderer.enabled = false;
                }
                if (!particleHolder.activeInHierarchy)
                {
                    particleHolder.SetActive(true);
                }
            }
            else if (DrawApplicationManager.Instance.currentState == DrawApplicationManager.ApplicationState.InMenu)
            {
                if (!cursorRenderer.enabled)
                {
                    cursorRenderer.enabled = true;
                }
                if (particleHolder.activeInHierarchy)
                {
                    particleHolder.SetActive(false);
                }
            }

        }
        else
        {
            cursorRenderer.enabled = false;
            particleHolder.SetActive(false);
        }
    }

    #region Erasing Draw
    float maxTimeAllowed = 1.5f;
    float minTimeAllowed = 0.2f;
    float maxTimeLeftToSwitchSide = 1.5f;
    int swapCounter = 0;
    enum ScreenPosition
    {
        Unknown,
        Right,
        Left
    }
    ScreenPosition currentScreenPosition = ScreenPosition.Unknown;
    ScreenPosition previousScreenPosition = ScreenPosition.Unknown;

    void HandleErase(HandInfo handInformation)
    {
        drawingWand.undoButtonDown = false;
        maxTimeLeftToSwitchSide -= Time.deltaTime;
        //I have an open hand
        if (handInformation.gestureInfo.manoClass == ManoClass.GRAB_GESTURE)
        {
            //I have the fingers relatively open
            if (handInformation.gestureInfo.state < 6)
            {


                //I have still time to perform a swap
                if (maxTimeLeftToSwitchSide > 0)
                {

                    //Vector3 palmCenter = handInformation.trackingInfo.palmCenter;

                    //if (palmCenter.x >= 0.6f)
                    //{
                    //    currentScreenPosition = ScreenPosition.Right;
                    //}
                    //else if (palmCenter.x <= 0.4)
                    //{
                    //    currentScreenPosition = ScreenPosition.Left;
                    //}
                    //else
                    {
                        currentScreenPosition = ScreenPosition.Unknown;
                    }

                    //I dont wanna do checks for the unknown area
                    if (currentScreenPosition != ScreenPosition.Unknown)
                    {
                        if (previousScreenPosition != currentScreenPosition)
                        {
                            // swapCounter
                            previousScreenPosition = currentScreenPosition;
                            swapCounter++;

                            //allow the undo to happen
                            if (swapCounter >= 3)
                            {
                                drawingWand.undoButtonDown = true;
                                swapCounter--;
#if !UNITY_STANDALONE
                                Handheld.Vibrate();
#endif
                            }
                        }
                    }

                }
                else
                {
                    swapCounter = 0;
                    maxTimeLeftToSwitchSide = maxTimeAllowed;
                }
            }
        }
    }
    #endregion

    float lerpSpeed;
    /// <summary>
    /// Set what the lerp speed of the cursor should be
    /// </summary>
    /// <param name="newSpeed"> a new value for the linear interpolation</param>
    public void SetLerpSpeed(float newSpeed)
    {
        lerpSpeed = newSpeed;
    }

    void Update()
    {
        if (ManomotionManager.Instance)
        {
            handInformation = ManomotionManager.Instance.HandInfos[0];
            IamPointingCorrectly(handInformation);
            HandleMenu(handInformation);
            CalculateCursorPosition(handInformation, lerpSpeed);
            AssignCorrectCursor();


            switch (DrawApplicationManager.Instance.currentState)
            {
                case DrawApplicationManager.ApplicationState.Drawing:
                    HandleDrawing(handInformation);
                    HandleErase(handInformation);
                    break;
                case DrawApplicationManager.ApplicationState.InMenu:
                    InteractWithUIIcons();
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.LogWarning("The Manomotion Manager Instance is null");
        }
    }

    Vector3 pointerWorldPosition;
    Vector3 pointerPosition;
    bool pointerNotNull;
    /// <summary>
    /// Calculates the Cursor position based on the Index finger position and hand relative depth
    /// </summary>
    /// <param name="handInformation">Hand information provided from Manomotion SDK</param>
    void CalculateCursorPosition(HandInfo handInformation, float lerpValue)
    {

        float estimatedDepth = handInformation.trackingInfo.depthEstimation;
        pointerPosition = handInformation.trackingInfo.skeleton.jointPositions[8];
        Vector3 nullPosition = new Vector3(-1, 2, -1);

        //changed this
        if (pointerPosition == nullPosition)
        {
            pointerNotNull = false;
            return;
        }
        else
        {
            pointerNotNull = true;
            pointerWorldPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(pointerPosition, estimatedDepth);
        }

        if (pointingProperly)
        {
            transform.position = Vector3.Lerp(transform.position, pointerWorldPosition, lerpValue);
        }

    }

    /// <summary>
    /// Evaluate the hand information Manoclass/HandState/Number of fingertips to determine if I am pointing correctly
    /// </summary>
    /// <param name="handInformation"></param>
    /// <returns></returns>
    void IamPointingCorrectly(HandInfo handInformation)
    {
        //  DetectedASingleFinger(handInformation);
        //singleFingertipWasDetected
        IamPointing = (handInformation.gestureInfo.manoClass == ManoClass.POINTER_GESTURE);
        usingAppropriateState = handInformation.gestureInfo.state <= drawStateThreshold;

        pointingProperly = IamPointing && usingAppropriateState && pointerNotNull;
        cursorRenderer.enabled = pointingProperly;
    }

    /// <summary>
    /// Creates a buffer of 5 frames and checks if the criteria for proper pointing are met in order to Start/Stop drawing
    /// </summary>
    /// <param name="handInformation">Hand information provided from Manomotion SDK</param>
    void HandleDrawing(HandInfo handInformation)
    {

        //Keep a buffer of 5 frames
        if (pointingProperly)
        {
            if (pointingCounter < 5)
            {
                pointingCounter++;
            }
        }
        else
        {
            if (pointingCounter > 0)
            {
                pointingCounter--;
            }
        }

        //Inform the wand based on the buffer
        if (pointingCounter >= 2)
        {
            //I did not detect a down click
            if (!drawingWand.drawButtonDown)
            {
                SetDrawingState(DrawingStates.StartDraw);
            }
            else
            {
                if (!drawingWand.drawButtonPressed)
                {
                    SetDrawingState(DrawingStates.Drawing);
                }
            }

        }
        else
        {
            if (!drawingWand.drawButtonUp)
            {
                SetDrawingState(DrawingStates.StopDraw);
            }
            else
            {
                SetDrawingState(DrawingStates.NotPainting);
            }
        }
    }

    /// <summary>
    /// Sets the currentDrawingState to a new state and fixiates the wand interactions
    /// </summary>
    /// <param name="state">a new drawing state</param>
    void SetDrawingState(DrawingStates state)
    {
        currentDrawingState = state;
        switch (state)
        {
            case DrawingStates.StartDraw:
                drawingWand.drawButtonDown = true;
                drawingWand.drawButtonUp = false;
                break;
            case DrawingStates.Drawing:
                drawingWand.drawButtonPressed = true;
                drawingWand.drawButtonUp = false;
                break;
            case DrawingStates.StopDraw:
                drawingWand.drawButtonUp = true;
                drawingWand.drawButtonPressed = false;
                drawingWand.drawButtonDown = false;
                break;
            case DrawingStates.NotPainting:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Uses the assigned Trigger gesture to open the menu if its not on a cooldown
    /// </summary>
    /// <param name="handInformation">Hand information provided from Manomotion SDK</param>
    void HandleMenu(HandInfo handInformation)
    {
        //while I am not in the menu start the counter for the duration being closed
        if (DrawApplicationManager.Instance.currentState != DrawApplicationManager.ApplicationState.InMenu)
        {
            menuClosedDuration += Time.deltaTime;
        }
        //If I have waited long enough for the counter to exceed the cooldown
        if (menuClosedDuration > menuCoolDown)
        {
            if (handInformation.gestureInfo.manoGestureTrigger == ManoGestureTrigger.RELEASE_GESTURE)
            {
                //DrawApplicationManager.Instance.ShowMenu(true);
                //DrawAppIconManager.Instance.ResetIconCooldown();
                menuClosedDuration = 0.0f;
            }
        }
    }

    /// <summary>
    /// Raycast from the Index finger and interact with the fuse icons
    /// </summary>
    void InteractWithUIIcons()
    {
        if (pointingProperly)
        {
            pointer.position = Camera.main.WorldToScreenPoint(transform.position);
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            bool hasHitPaintingIcon = raycastResults[0].gameObject.tag == "DrawAppIcon";
            //My list have things that I have hit
            if (hasHitPaintingIcon)
            {
                currentHit = raycastResults[0].gameObject;

                //the current hit is not the same as the previous one
                if (currentHit != lastHit)
                {

                    if (raycastResults[0].gameObject.tag == "DrawAppIcon")
                    {

                        raycastResults[0].gameObject.GetComponent<DrawAppIconBehavior>().OnnEnter();
                    }
                    if (lastHit != null)
                    {
                        lastHit.GetComponent<DrawAppIconBehavior>().OnnExit();
                    }

                }

                lastHit = currentHit;
            }
            else if (!hasHitPaintingIcon && lastHit != null)
            {
                if (lastHit.tag == "DrawAppIcon")
                {
                    lastHit.gameObject.GetComponent<DrawAppIconBehavior>().OnnExit();
                }
                lastHit = null;
            }
        }
        else
        {
            if (lastHit != null)
            {
                lastHit.GetComponent<DrawAppIconBehavior>().OnnExit();
                lastHit = null;
            }
        }
    }

    /// <summary>
    /// Matches the material of the Cursor's mesh renderer to the selected color
    /// </summary>
    /// <param name="currentColor"></param>
    public void SetDrawCursorColor(Color currentColor)
    {
        cursorRenderer.material.color = currentColor;
    }

    /// <summary>
    /// Sets the Particle's color to the selected drawing color
    /// </summary>
    /// <param name="currentColor"></param>
    public void SetDrawParticleColor(Color currentColor)
    {
        particles.startColor = currentColor;
    }

    /// <summary>
    /// Assign the visual values of the Cursor and sets the components
    /// </summary>
    private void InitializeVisuals()
    {
        particleHolder = transform.GetChild(0).gameObject;
        particles = particleHolder.GetComponent<ParticleSystem>();
        cursorRenderer = GetComponent<MeshRenderer>();
        Color initialcolor = new Color(145f / 255f, 94f / 255f, 167f / 255f, 1f);
        SetDrawCursorColor(initialcolor);
        SetDrawParticleColor(initialcolor);
    }

    /// <summary>
    /// Assign the local interaction values based on the Application Manager
    /// </summary>
    private void InitializeInteractionValues()
    {
        drawGestureFamily = DrawApplicationManager.Instance.drawGestureManoclass;
        drawStateThreshold = DrawApplicationManager.Instance.drawStateThreshold;
        menuTriggerGesture = DrawApplicationManager.Instance.menuTriggerGesture;
    }
}