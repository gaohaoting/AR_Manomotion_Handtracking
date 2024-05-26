using UnityEngine;
using UnityEngine.UI;

public class HandleFrameFadingWatch : MonoBehaviour
{
    public Image frameImage;

    void Update()
    {
        if (WatchExample.instance.isWatchShowing)
        {
            if (frameImage.material.GetFloat("_Distortion") < 6)
            {
                float tempColor = frameImage.material.GetFloat("_Distortion");
                tempColor += 0.3f;
                frameImage.material.SetFloat("_Distortion", tempColor);
            }
        }
        else
        {
            if (frameImage.material.GetFloat("_Distortion") > 0)
            {
                float tempColor = frameImage.material.GetFloat("_Distortion");
                tempColor -= 0.3f;
                frameImage.material.SetFloat("_Distortion", tempColor);
            }
        }
    }
}