using UnityEngine;
using UnityEngine.UI;

public class DrawAppIconManager : MonoBehaviour
{
    [SerializeField] GameObject[] icons;

    /// <summary>
    /// Resets the cooldown of the buttons
    /// </summary>
    public void ResetIconProgressBar()
    {
        foreach(GameObject icon in icons) { 
            icon.GetComponent<Image>().fillAmount = 0f;
        }
    }
}