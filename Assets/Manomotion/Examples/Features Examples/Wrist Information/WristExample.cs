using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WristExample : MonoBehaviour
{
    /// <summary>
    /// The wrist gizmo contains the finger information.
    /// </summary>
    public WristInfoGizmo wristInfoGizmo;

    /// <summary>
    /// Warning text that tells the user to move the hand further away when hand is to close
    /// </summary>
    [SerializeField]
    private TMP_Text depthWarningText;

    /// <summary>
    /// Ring parts for only display half of the ring at a time.
    /// </summary>
    public GameObject[] watchPrefabParts;

    /// <summary>
    /// Used to set wich part of the ring prefabs to show.
    /// </summary>
    private bool isFront = true;

    /// <summary>
    /// The position where the ring will be placed.
    /// </summary>
    private Vector3 watchPlacement;

    /// <summary>
    /// Scale modifer.
    /// </summary>
    private float scaleModifier = 1f;

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

    private void Start()
    {
        ///Sets the screen orienation to portrait mode.
        Screen.orientation = ScreenOrientation.Portrait;

        if (wristInfoGizmo == null)
        {
            try
            {
                wristInfoGizmo = GizmoManager.Instance.wristInfo;
            }

            catch
            {
                Debug.Log("Cant find wristInfoGizmo GameObject");
            }
        }
    }

    private void SetManoMotionSettings()
    {
        ManomotionManager.Instance.ShouldRunWristInfo(true);
        ManomotionManager.Instance.ShouldCalculateGestures(true);
        GameObject.Find("Wrist").SetActive(false);
    }

    bool isWristRemoved = false;

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
            wristInfoGizmo.ShowWristInformation(ManomotionManager.Instance.HandInfos[0].trackingInfo, gestureInfo);
            ShowWatch();
        }

        else
        {
            DontShowWatch();
        }
    }

    private void ShowWatch()
    {
        float centerPosition = 0.5f;
        ///Gets the position between the 2 finger points from the finger gizmo.
        watchPlacement = Vector3.Lerp(wristInfoGizmo.LeftWristPoint3DPosition, wristInfoGizmo.RightWristPoint3DPosition, centerPosition);
        ///Place the ring at the ring placement position.
        transform.position = watchPlacement;
        ///Sets the rotation of the ring in relation the the finger position when hand is rotated 

        transform.LookAt(wristInfoGizmo.LeftWristPoint3DPosition);

        ///Scale the ring with the width from the 2 finger points and multiplyed by a scaleModifier.
        transform.localScale = new Vector3(wristInfoGizmo.WidthBetweenWristPoints * scaleModifier, wristInfoGizmo.WidthBetweenWristPoints * scaleModifier, wristInfoGizmo.WidthBetweenWristPoints * scaleModifier);

        ///When Palm is showing the scale gets inverted to show the back of the ring.
        if (gestureInfo.handSide == palm)
        {
            ActivateWatchParts(!isFront);
            transform.localScale = new Vector3(-transform.localScale.x, -transform.localScale.y, -transform.localScale.z);
        }
        else
        {
            ActivateWatchParts(isFront);
        }

        ///Disables the outline image.
        outlineImage.SetActive(false);
    }

    /// <summary>
    /// Activate or deactive parts of the ring depengind on wich half should show.
    /// </summary>
    /// <param name="front">bool to set the values.</param>
    private void ActivateWatchParts(bool front)
    {
        watchPrefabParts[0].SetActive(front);
        watchPrefabParts[1].SetActive(front);
        watchPrefabParts[2].SetActive(!front);
    }

    /// <summary>
    /// Enabled the outline image and move the ring to -Vector3.one so its not visable.
    /// </summary>
    private void DontShowWatch()
    {
        outlineImage.SetActive(true);
        transform.position = -Vector3.one;
    }
}
