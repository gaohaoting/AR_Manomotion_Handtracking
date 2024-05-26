using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandOutlineFadeWatch : MonoBehaviour
{
    private Image frameImage;
    private bool hasConfidence;
    private float skeletonConfidenceThreshold = 0.0001f;
    private SkeletonInfo skeletonInfo;

    // Start is called before the first frame update
    void Start()
    {
        frameImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        skeletonInfo = ManomotionManager.Instance.HandInfos[0].trackingInfo.skeleton;
        hasConfidence = skeletonInfo.confidence > skeletonConfidenceThreshold;

        if (!hasConfidence || !WatchExample.instance.isWatchShowing)
        {
            if (frameImage.color.a < 1)
            {

                Color tempColor = frameImage.color;
                tempColor.a += 0.1f;
                frameImage.color = tempColor;
            }
        }

        else
        {
            if (frameImage.color.a > 0)
            {
                Color tempColor = frameImage.color;
                tempColor.a -= 0.1f;
                frameImage.color = tempColor;
            }
        }
    }
}
