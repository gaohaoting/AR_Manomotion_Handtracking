using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ManoMotionListener : MonoBehaviour
{
    private static ManoMotionListener _instance;
    public static ManoMotionListener Instance
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

    Vector3 pointerWorldPosition;
    [SerializeField]
    Text debugTextString;
    [SerializeField]
    Text depthValuetostring;

    [SerializeField]
    Text menuState;

    float menuCoolDown = 0.25f;
    float menuClosedDuration = 0.0f;
    public DrawingWand wandProxy = null;


    // public bool triggerWandDirectly = true;

    //Button pressed 
    private bool _drawButtonPressed = false;
    public bool drawButtonPressed
    {
        get
        {
            if (wandProxy != null)
                _drawButtonPressed = wandProxy.drawButtonPressed;
            return _drawButtonPressed;
        }
    }


    //Button Up
    private bool _drawButtonUp = false;
    public bool drawButtonUp
    {
        get
        {
            return _drawButtonUp;
        }
        set
        {
            _drawButtonUp = value;
            if (drawButtonUp)
            {
                _drawButtonPressed = false;
                _drawButtonDown = false;
            }
            if (wandProxy != null)
            {
                wandProxy.drawButtonUp = _drawButtonUp;
                _drawButtonPressed = wandProxy.drawButtonPressed;
            }
        }
    }


    //Button Down
    private bool _drawButtonDown = false;
    public bool drawButtonDown
    {
        get
        {
            return _drawButtonDown;
        }
        set
        {
            _drawButtonDown = value;
            if (_drawButtonDown)
            {
                _drawButtonPressed = true;
                _drawButtonUp = false;
            }
            if (wandProxy != null)
            {
                wandProxy.drawButtonDown = _drawButtonDown;
                _drawButtonPressed = wandProxy.drawButtonPressed;
            }
        }
    }







    public ManoClass[] drawGestureFamily = new ManoClass[] { ManoClass.POINTER_GESTURE };
    public int drawStateThreshold = 5;
    public int drawBeginFilterSize = 1;
    private int drawBeginFrameCount = 0;
    public int drawEndFilterSize = 1;
    private int drawEndFrameCount = 0;

    public ManoGestureTrigger[] menuDynamicGesture = new ManoGestureTrigger[] { ManoGestureTrigger.RELEASE_GESTURE };
    public int menuShowFilterSize = 5;
    private int menuShowFrameCount = 0;
    public int menuHideFilterSize = 5;
    private int menuHideFrameCount = 0;
    public float menuGestureWidthMultiplier = 0.5f;
    public bool showMenu = false;


    private GestureInfo gestureCurrent;
    private GestureInfo gesturePrevious;
    private Queue<GestureInfo> gestureHistory = new Queue<GestureInfo>();
    private int gestureHistorySize = 5;


    private HandInfo currentHandInfo;
    public HandInfo CurrentHandInfo
    {

        get
        {
            return currentHandInfo;
        }
        set
        {
            currentHandInfo = value;
        }
    }




    private void Start()
    {
        if (!_instance)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("More than 1 ManomotionListener instances in scene");
        }
    }

    public GameObject cursor;
    void Update()
    {
        menuState.text = showMenu.ToString();
        if (ManomotionManager.Instance != null)
        {
            if (ManomotionManager.Instance)
            {
                CurrentHandInfo = ManomotionManager.Instance.HandInfos[0];
                InterpretGesture(CurrentHandInfo);

            }
        }





    }

    protected void InterpretGesture(HandInfo handInformation)
    {
        if (true)
        {
            CalculateCursorPosition();
            HandleDrawing();
        }
        OpenTheMenu();
    }

    /// <summary>
    /// Smoothing
    /// </summary>
    /// <param name="gesture"></param>
    /// <param name="array"></param>
    /// <returns></returns>
    public static bool GestureInManoclassArray(ManoClass gesture, ManoClass[] array)
    {
        for (int i = 0; i < array.Length; ++i)
        {
            if (array[i] == gesture)
            {
                return true;
            }
        }
        return false;
    }

    public static bool GestureFamilyInArray(ManoClass gesture, ManoClass[] array)
    {
        return GestureFamilyInArray(gesture, array);
    }

    private IEnumerator CloseMenuAfter(float time)
    {
        yield return new WaitForSeconds(time);
        showMenu = false;
        DrawApplicationManager.Instance.currentState = DrawApplicationManager.ApplicationState.Drawing;


    }


    //done
    void CalculateCursorPosition()
    {
        float estimatedDepth = ManomotionManager.Instance.HandInfos[0].trackingInfo.depthEstimation;
        Vector3 pointerPosition = ManomotionManager.Instance.HandInfos[0].trackingInfo.skeleton.jointPositions[8];
        pointerWorldPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(pointerPosition, estimatedDepth);
    }

    [SerializeField]
    Text allowedToDrawText;
    //done
    void HandleDrawing()
    {
        //Check if I am allowed to draw
        //I am pointing, I am under the draw thershold and the menu is not open
        bool IamAllowedToDraw = (gestureCurrent.manoClass == ManoClass.POINTER_GESTURE && !showMenu);
        allowedToDrawText.text = "Bool status allowed to draw " + IamAllowedToDraw;
        if (IamAllowedToDraw && drawStateThreshold >= 0)
        {
            IamAllowedToDraw = gestureCurrent.state <= drawStateThreshold;
        }

        //Stop drawing
        if (!IamAllowedToDraw)
        {
            drawBeginFrameCount = 0;

            if (drawButtonPressed)
            {
                drawEndFrameCount++;
                if (drawEndFrameCount >= drawEndFilterSize)
                {
                    drawButtonUp = true;
                    debugTextString.text = "STOP DRAW\n";
                    drawEndFrameCount = 0;
#if !UNITY_STANDALONE
                    Handheld.Vibrate();
#endif
                }
            }
            else
            {
                drawEndFrameCount = 0;
            }
        }
        else if (IamAllowedToDraw)
        {
            if (!_drawButtonPressed)
            {
                drawBeginFrameCount++;
                if (drawBeginFrameCount >= drawBeginFilterSize)
                {
                    debugTextString.text = "BEGIN DRAW\n";
                    drawButtonDown = true;
                    drawBeginFrameCount = 0;
                    drawEndFrameCount = 0;
                }
            }
            else
            {
                debugTextString.text = "DRAW\n";
            }

        }
    }


    void OpenTheMenu()
    {
        //while the menu is closed increase the timer
        if (!showMenu)
        {
            menuClosedDuration += Time.deltaTime;
            //I have waited long enough for the counter to exceed the cooldown
            if (menuClosedDuration > menuCoolDown)
            {
                //I have performed a release gesture
                if (gestureCurrent.manoGestureTrigger == ManoGestureTrigger.RELEASE_GESTURE)
                {
                    FuseButton[] buttons = FindObjectsOfType<FuseButton>();
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        buttons[i].coolDown = 1;
                    }
                    GetComponent<ManoLogic>().ShowMenu(true);
                    showMenu = true;
                    menuClosedDuration = 0.0f;
                }
            }

        }



    }
    public enum ScreenPosition
    {
        Unknown = 0,
        Right = 1,
        Left = 2
    }
    ScreenPosition currentScreenPosition = ScreenPosition.Unknown;
    ScreenPosition previousScreenPosition = ScreenPosition.Unknown;
    public int rightLeftCounter;
    void ErasePainting()
    {
        //Erase should happen if ApplicationManager is in draw mode
        //Erase should happen with open hand so check the manoclass and probably the state 
        //Have an coroutine that goes every X time to check where am I 
        //check the palmcenter position in comparison to screenwidth/2
        //if position>screenwidth/2 I am right else left
        // do the check inside the Ienumerator
        if (currentScreenPosition != previousScreenPosition)
        {
            //I on the other side of the screen
            rightLeftCounter++;
            previousScreenPosition = currentScreenPosition;
        }
        if (rightLeftCounter > 3)
        {
            wandProxy.undoButtonDown = true;
            //perform undo steps
        }
        else
        {
            wandProxy.undoButtonDown = false;
        }

    }


}
