using UnityEngine;
using System.Collections;

public class ManoLogic : MonoBehaviour
{

    public GameObject ColorSelectionUI;


    public void ShowMenu(bool val)
    {
        ColorSelectionUI.SetActive(val);
        if (val == true)
        {
            DrawApplicationManager.Instance.currentState = DrawApplicationManager.ApplicationState.InMenu;
        }
    }




}
