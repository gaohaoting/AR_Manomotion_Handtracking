using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class FuseButton : MonoBehaviour
{

    #region IconVariables
    public DrawingWand drawingwad;
    public ColorSettings colorSettings;
    Image progressBar;
    public float fuseTimeNeeded = 2f;
    float elapsedTime = 0f;
    public float coolDown = 0;
    bool userIsPointingAtMe;
    #endregion


    void Start()
    {
        InitializeIconValues();

    }

    void InitializeIconValues()
    {
        colorSettings = GetComponent<ColorSettings>();
        progressBar = transform.parent.Find("bar").GetComponent<Image>();
        userIsPointingAtMe = false;
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

    // Update is called once per frame
    void Update()
    {
        IconBehavior();

    }

    void IconBehavior()
    {
        if (coolDown > 0)
        {
            coolDown -= Time.deltaTime;
        }

        if (userIsPointingAtMe && coolDown <= 0)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= fuseTimeNeeded)
            {

                //DrawApplicationManager.Instance.ShowMenu(false);
                if (colorSettings)
                {
                    colorSettings.SetColor();
                }
                else
                {
                    drawingwad.undoAllButtonDown = true;
                }
                elapsedTime = 0;
                progressBar.fillAmount = 0f;
                userIsPointingAtMe = false;

            }

            progressBar.fillAmount = map(elapsedTime, 0.0f, fuseTimeNeeded, 0.0f, 1.0f);


        }
    }


    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
