using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectFirstButton : MonoBehaviour
{
    public Button firstButton;

    public Button[] Buttons;

    public Color selectedColor;


    // Start is called before the first frame update
    void Start()
    {
        firstButton.image.color = selectedColor;
    }

    public void setSelectedButtonColror(Button thisButton)
    {
        foreach (var item in Buttons)
        {
            item.image.color = Color.white;
        }

        thisButton.image.color = selectedColor;
    }
}
