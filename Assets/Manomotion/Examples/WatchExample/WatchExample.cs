using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WatchExample : MonoBehaviour
{
    /// <summary>
    /// The wrist gizmo contains the finger information.
    /// </summary>
    /// 
    private WristInfoGizmo wristInfoGizmo;

    /// <summary>
    /// The position where the ring will be placed.
    /// </summary>
    private Vector3 watchPlacement;

    /// <summary>
    /// Gesture inforamtion.
    /// </summary>
    private GestureInfo gestureInfo;

    /// <summary>
    /// Palmside.
    /// </summary>
    private HandSide palm = HandSide.Palmside;

    /// <summary>
    /// The gameobject contaning the image of the outlined hand. 
    /// </summary>
    [SerializeField]
    private GameObject outlineImage;


    #region Singleton
    /// <summary>
    /// Creates instance of SkeletonManager
    /// </summary>
    public static WatchExample instance;

    private OneEuroFilter<Vector3> vector3FilterNormalVector1;
    private OneEuroFilter<Vector3> vector3FilterNormalVector2;
    private OneEuroFilter<Vector3> vector3FilterNormalVector3;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        /*else
        {
            this.gameObject.SetActive(false);
            Debug.LogWarning("More than 1 Watchexample in scene");      
        }*/
    }
    #endregion

    private void Start()
    {
        ///Sets the screen orienation to portrait mode.
        //Screen.orientation = ScreenOrientation.Portrait;

        if (wristInfoGizmo == null)
        {
            wristInfoGizmo = GizmoManager.Instance.wristInfo;
        }
    }

    private void SetManoMotionSettings()
    {
        ManomotionManager.Instance.ShouldRunWristInfo(true);
        ManomotionManager.Instance.ShouldCalculateGestures(true);
        ManomotionManager.Instance.ShouldCalculateSkeleton3D(true);
     //   GameObject.Find("Wrist").SetActive(false);
    }

    bool isWristRemoved = false;
    internal bool isWatchShowing;

    void Update()
    {
        ///Updates the gestureinfo
        gestureInfo = ManomotionManager.Instance.HandInfos[0].gestureInfo;
        if (!isWristRemoved)
        {
            SetManoMotionSettings();
            isWristRemoved = true;
        }

        if (gestureInfo.manoClass != ManoClass.NO_HAND)
        {
            if (!wristInfoGizmo)
            {
                wristInfoGizmo = GizmoManager.Instance.wristInfo;
            }
            wristInfoGizmo.ShowWristInformation(ManomotionManager.Instance.HandInfos[0].trackingInfo, gestureInfo);
            ShowWatch();
            isWatchShowing = true;
        }
        else
        {
            isWatchShowing = false;
            DontShowWatch();
        }
    }

    private void ShowWatch()
    {
        float scaleModifier = 1f;

        ///Gets the position between the 2 finger points from the finger gizmo.
        watchPlacement = GetCenter2Points(wristInfoGizmo.LeftWristPoint3DPosition, wristInfoGizmo.RightWristPoint3DPosition);

        ///Place the watch at the wrist placement position.
        transform.position = watchPlacement;

        ///Scale the ring with the width from the 2 finger points and multiplyed by a scaleModifier.
        transform.localScale = new Vector3(wristInfoGizmo.WidthBetweenWristPoints * scaleModifier, wristInfoGizmo.WidthBetweenWristPoints * scaleModifier, wristInfoGizmo.WidthBetweenWristPoints * scaleModifier);
        var Palm1 = SkeletonManager.instance.joints[0].transform.position;
        var Palm2 = SkeletonManager.instance.joints[5].transform.position;
        var Palm3 = SkeletonManager.instance.joints[17].transform.position;


        //Get the normal for the palm, used when titl functionality is enabled
        Vector3 handNormal = GetNormalTriangle(Palm1, Palm2, Palm3);

        if (gestureInfo.leftRightHand == LeftOrRightHand.LEFT_HAND)
        {
            handNormal = -handNormal;
            ///When Palm is showing the scale gets inverted to show the back of the ring.
            //if (gestureInfo.hand_side == palm)
            //{
            //    transform.localScale = new Vector3(-transform.localScale.x, -transform.localScale.y, -transform.localScale.z);
            //    transform.LookAt(wristInfoGizmo.RightWristPoint3DPosition, -handNormal);
            //}
            //else
            //{
                transform.LookAt(wristInfoGizmo.RightWristPoint3DPosition, handNormal);
            //}
        }
        else if (gestureInfo.leftRightHand == LeftOrRightHand.RIGHT_HAND)
        {
            ///When Palm is showing the scale gets inverted to show the back of the ring.
            //if (gestureInfo.hand_side == palm) 
            //{
                //transform.localScale = new Vector3(-transform.localScale.x, -transform.localScale.y, -transform.localScale.z);
                //transform.LookAt(wristInfoGizmo.LeftWristPoint3DPosition, -handNormal);
            //}
            //else
            //{
                transform.LookAt(wristInfoGizmo.LeftWristPoint3DPosition, handNormal);
            //}
        }
        ///Disables the outline image.
        outlineImage.SetActive(false);
    }

    /// <summary>
    /// Enabled the outline image and move the ring to -Vector3.one so its not visable.
    /// </summary>
    private void DontShowWatch()
    {
        outlineImage.SetActive(true);
        transform.position = -Vector3.one;
    }

    #region Vector Utils

    // Get the normal to a triangle from the three corner points, a, b and c.
    Vector3 GetNormalTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        // Find vectors corresponding to two of the sides of the triangle.
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;

        // Cross the vectors to get a perpendicular vector, then normalize it.
        return Vector3.Cross(side1, side2).normalized;
    }

    // Get the normal to a line from the two corner points, a, b
    Vector3 GetNormal2Points(Vector3 a, Vector3 b)
    {
        // Find vectors corresponding to two of the sides of the triangle.
        Vector3 side = b - a;

        // Cross the vectors to get a perpendicular vector, then normalize it.
        return Vector3.Cross(side, Vector3.up).normalized;
    }

    Vector3 GetCenterTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        var allX = a.x + b.x + c.x;
        var allY = a.y + b.y + c.y;
        var allZ = a.z + b.z + c.z;

        return new Vector3(allX / 3, allY / 3, allZ / 3);
    }

    Vector3 GetCenter2Points(Vector3 a, Vector3 b)
    {
        var allX = a.x + b.x;
        var allY = a.y + b.y;
        var allZ = a.z + b.z;

        return new Vector3(allX / 2, allY / 2, allZ / 2);
    }

    

    #endregion

}
