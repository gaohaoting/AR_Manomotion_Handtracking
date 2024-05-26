using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowWatchConditions : MonoBehaviour
{
    private Vector3[] joint;
    private bool[] isJointInPlace;

    public bool showWatch;


    // Start is called before the first frame update
    void Start()
    {
        joint = new Vector3[4];
        isJointInPlace = new bool[4];
    }

    // Update is called once per frame
    void Update()
    {
        joint[0] = SkeletonManager.instance.joints[0].transform.position;
        joint[1] = SkeletonManager.instance.joints[4].transform.position;
        joint[2] = SkeletonManager.instance.joints[12].transform.position;
        joint[3] = SkeletonManager.instance.joints[17].transform.position;

        Debug.Log("Joint 0 Y position: " + joint[0].y + "  " + isJointInPlace[0]);
        Debug.Log("Joint 4 x position: " + joint[1].x + "  " + isJointInPlace[1]);
        Debug.Log("Joint 12 y position: " + joint[2].y + "  " + isJointInPlace[2]);
        Debug.Log("Joint 17 x position: " + joint[3].x + "  " + isJointInPlace[3]);

        isJointInPlace[0] = joint[0].y > -0.27f;
        isJointInPlace[1] = joint[1].x > -0.27f;
        isJointInPlace[2] = joint[2].x < 0.275f;
        isJointInPlace[3] = joint[3].y < 0.16f;

        if (isJointInPlace[0] && isJointInPlace[1] && isJointInPlace[2] && isJointInPlace[3])
        {
            showWatch = true;
        }

        else
        {
            showWatch = false;
        }
    }
}
