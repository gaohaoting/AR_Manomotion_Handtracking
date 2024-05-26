using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawApplicationManager : MonoBehaviour
{
    /// <summary>
    /// Holds the information of the application state and other relevant pieces of info
    /// </summary>


    #region Singleton
    private static DrawApplicationManager _instance;

    public static DrawApplicationManager Instance
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


    private void Awake()
    {
        if (!_instance)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("More than 1 Application Manager Instances");
        }
    }
    #endregion

    #region Application Interaction Settings
    public ManoClass drawGestureManoclass = ManoClass.POINTER_GESTURE;
    [Range(1, 13)]
    public int drawStateThreshold = 5;
    public ManoGestureTrigger menuTriggerGesture = ManoGestureTrigger.RELEASE_GESTURE;

    #endregion

    public enum ApplicationState
    {
        Drawing = 0,
        InMenu = 1
    }

    public ApplicationState currentState;
    void Start()
    {
        currentState = ApplicationState.Drawing;
        //ShowMenu(currentState == ApplicationState.InMenu);
    }

    public void ShowMenu(bool val)
    {
        if (val == true)
        {
            Instance.currentState = ApplicationState.InMenu;
            ManoCursorUIOverlay.Instance.SetLerpSpeed(0.5f);
        }
        else
        {
            ManoCursorUIOverlay.Instance.SetLerpSpeed(1);
            StartCoroutine(AllowDrawingAFterDelay(1.5f));

        }
    }

    IEnumerator AllowDrawingAFterDelay(float delay)
    {

        yield return new WaitForSeconds(delay);
        Instance.currentState = ApplicationState.Drawing;
        ManoCursorUIOverlay.Instance.SetLerpSpeed(0.5f);
    }




}
