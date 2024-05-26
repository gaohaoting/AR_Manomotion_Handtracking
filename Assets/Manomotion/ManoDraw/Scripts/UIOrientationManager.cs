using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOrientationManager : MonoBehaviour
{
    [SerializeField]
    private GameObject PortraitUI;

    [SerializeField]
    private GameObject LandscapeUI;

    private void Start()
    {
        PortraitUI.SetActive(false);
        LandscapeUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.deviceOrientation == DeviceOrientation.Portrait)
        {
            if (PortraitUI) PortraitUI.SetActive(true);
            if (LandscapeUI)
            {
                LandscapeUI.SetActive(false);
                LandscapeUI.GetComponentInChildren<DrawAppIconManager>().ResetIconProgressBar();
            }
        }
        else
        {
            if (LandscapeUI) LandscapeUI.SetActive(true);
            if (PortraitUI)
            {
                PortraitUI.SetActive(false);
                PortraitUI.GetComponentInChildren<DrawAppIconManager>().ResetIconProgressBar();
            }
        }

    }
}
