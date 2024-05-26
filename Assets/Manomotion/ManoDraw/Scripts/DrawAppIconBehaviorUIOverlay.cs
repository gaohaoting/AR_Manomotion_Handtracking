using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DrawAppIconBehaviorUIOverlay : MonoBehaviour
{
    #region IconVariables
    private Image progressBar;
    private Image iconColorImage;
    private Color myColor;
    private bool isInitialized;

    [SerializeField]
    private DrawAppIconManager iconManager;
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

    /// <summary>
    /// Logic that executes when activationTimeNeeded has been reached
    /// </summary>
    public void IconBehavior()
    {
        
        switch (myIconType)
        {
            case IconType.ColorSelection:
                iconManager.ResetIconProgressBar();
                UpdateColor();
                break;
            case IconType.EraseAll:
                DrawingWand.Instance.undoAllButtonDown = true;
                break;
            default:
                break;
        }
    }

    private ManoCursorUIOverlay cursor;
    private Settings settings;
    /// <summary>
    /// Initialize the values needed according to the icon type
    /// </summary>
    /// <param name="iconType"></param>
    void InitializeIcon(IconType iconType)
    {
        if (!settings)
            settings = Settings.GetInstance();
        if (!cursor)
            cursor = ManoCursorUIOverlay.Instance;

        if (cursor)
            Debug.Log("I have a cursor");
        progressBar = transform.parent.Find("bar").GetComponent<Image>();
        switch (iconType)
        {
            case IconType.ColorSelection:
                iconColorImage = GetComponent<Image>();
                myColor = iconColorImage.color;
                break;
            default:
                break;
        }


        isInitialized = true;
    }

    /// <summary>
    /// Sets the color of the drawing settings. Sets the color of the sphere and the particles to match that.
    /// </summary>
    void UpdateColor()
    {
        settings = Settings.GetInstance();
        settings.GetBrushById(0).color = myColor;

        if (!cursor)
            cursor = ManoCursorUIOverlay.Instance;
        cursor.SetDrawCursorColor(myColor);
        cursor.SetDrawParticleColor(myColor);
        progressBar.fillAmount = 1f;
    }

}
