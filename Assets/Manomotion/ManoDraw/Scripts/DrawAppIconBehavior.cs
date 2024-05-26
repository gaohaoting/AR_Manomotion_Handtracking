using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DrawAppIconBehavior : MonoBehaviour
{
    #region IconVariables
    public Image progressBar;
    public Image iconColorImage;
    public Color myColor;
    public float activationTimeNeeded = 2f;
    float elapsedTime = 0f;
    public float coolDown = 0;
    public bool userIsPointingAtMe;
    public bool isInitialized;
    #endregion


    public enum IconType
    {
        ColorSelection,
        EraseAll
    }

    public IconType myIconType;


    private void OnEnable()
    {
        if (!isInitialized)
        {
            InitializeIcon(myIconType);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (coolDown > 0)
        {
            coolDown -= Time.deltaTime;
        }
        IconBehavior();
    }

    /// <summary>
    /// Logic that executes when activationTimeNeeded has been reached
    /// </summary>
    public void IconBehavior()
    {

        if (userIsPointingAtMe && coolDown <= 0)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= activationTimeNeeded)
            {
                switch (myIconType)
                {
                    case IconType.ColorSelection:
                        UpdateColor();
                        break;
                    case IconType.EraseAll:
                        DrawingWand.Instance.undoAllButtonDown = true;
                        break;
                    default:
                        break;
                }
                elapsedTime = 0;
                progressBar.fillAmount = 0f;
                userIsPointingAtMe = false;
                //DrawApplicationManager.Instance.ShowMenu(false);


            }

            //Fill the progress Bar
            progressBar.fillAmount = map(elapsedTime, 0.0f, activationTimeNeeded, 0.0f, 1.0f);
        }

    }
    public ManoCursor cursor;
    public Settings settings;
    /// <summary>
    /// Initialize the values needed according to the icon type
    /// </summary>
    /// <param name="iconType"></param>
    void InitializeIcon(IconType iconType)
    {
        if (!settings)
            settings = Settings.GetInstance();
        if (!cursor)
            cursor = ManoCursor.Instance;

        if (cursor)
            Debug.Log("I have a cursor");
        Debug.Log("Initialized Icon");
        progressBar = transform.parent.Find("bar").GetComponent<Image>();
        switch (iconType)
        {
            case IconType.ColorSelection:
                iconColorImage = GetComponent<Image>();
                myColor = iconColorImage.color;
                break;
            case IconType.EraseAll:
                break;
            default:
                break;
        }


        isInitialized = true;
    }

    /// <summary>
    /// Sets the color of the drawing settings. Sets the color of the sphere and the particles to match that.
    /// </summary>
    public void UpdateColor()
    {
        settings = Settings.GetInstance();
        settings.GetBrushById(0).color = myColor;
        Debug.Log("Executed color assig");


        if (!cursor)
            cursor = ManoCursor.Instance;
        cursor.SetDrawCursorColor(myColor);
        Debug.Log("1");
        cursor.SetDrawParticleColor(myColor);
        Debug.Log("Executed Manocursror visuals");
        //ManoVisualization.Instance.SetContourColor(myColor);
        Debug.Log("Executed Manomotion outline visuals");
    }

    /// <summary>
    /// Serialize the values given to a new range 0-1
    /// </summary>
    /// <param name="s"></param>
    /// <param name="a1"></param>
    /// <param name="a2"></param>
    /// <param name="b1"></param>
    /// <param name="b2"></param>
    /// <returns></returns>
    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public void OnnEnter()
    {
        Debug.Log("User is pointing at me");
        userIsPointingAtMe = true;
    }

    public void OnnExit()
    {
        Debug.Log("User is no loger pointing at me");
        userIsPointingAtMe = false;
        elapsedTime = 0.0f;
        progressBar.fillAmount = 0f;
    }
}
