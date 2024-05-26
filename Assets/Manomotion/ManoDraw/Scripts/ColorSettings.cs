using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorSettings : MonoBehaviour
{

    Image img;

    Color myCol;


    // Use this for initialization
    void Start()
    {

        img = GetComponent<Image>();
        myCol = img.color;

    }

    public void SetColor()
    {
        Settings settings = Settings.GetInstance();
        settings.GetBrushById(0).color = myCol;

        ManoCursor.Instance.SetDrawCursorColor(myCol);
        ManoCursor.Instance.SetDrawParticleColor(myCol);
        //ManoVisualization.Instance.SetContourColor(myCol);
    }


}
