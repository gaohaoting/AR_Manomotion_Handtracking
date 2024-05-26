using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DrawCursor : MonoBehaviour
{
    [SerializeField]
    Text currentItem;
    [SerializeField]
    Text previousItem;
    private static DrawCursor _instance;
    public static DrawCursor Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }

    [SerializeField]
    GameObject particleHolder;
    private ParticleSystem particles;



    public void SetDrawCursorColor(Color currentColor)
    {
        cursorRenderer.material.color = currentColor;
    }

    public void SetDrawParticleColor(Color currentColor)
    {
        particles.startColor = currentColor;
    }

    MeshRenderer cursorRenderer;
    GameObject lastHit = null;
    GameObject currentHit = null;

    PointerEventData pointer = new PointerEventData(EventSystem.current);

    bool userIsPointing;
    private void Start()
    {
        if (!_instance)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("More than 1 Drawcursor instances");
        }

        InitializeSettings();

    }
    void Update()
    {
        userIsPointing = ManomotionManager.Instance.HandInfos[0].gestureInfo.manoClass == ManoClass.POINTER_GESTURE && ManoMotionListener.Instance.showMenu;
        cursorRenderer.enabled = userIsPointing;
        if (userIsPointing)
        {
            switch (DrawApplicationManager.Instance.currentState)
            {
                case DrawApplicationManager.ApplicationState.Drawing:

                    break;
                case DrawApplicationManager.ApplicationState.InMenu:
                    InteractWithUIIcons();

                    break;
                default:
                    break;
            }
        }



    }

    private void InitializeSettings()
    {
        particles = particleHolder.GetComponent<ParticleSystem>();
        cursorRenderer = GetComponent<MeshRenderer>();
        Color initialcolor = new Color(145f / 255f, 94f / 255f, 167f / 255f, 1f);
        SetDrawCursorColor(initialcolor);
        SetDrawParticleColor(initialcolor);
    }



    void InteractWithUIIcons()
    {

        pointer.position = Camera.main.WorldToScreenPoint(transform.position);
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        bool hasHitFuseIcon = raycastResults[0].gameObject.tag == "fuse";



        //My list have things that I have hit
        if (hasHitFuseIcon)
        {
            currentHit = raycastResults[0].gameObject;
            currentItem.text = "Current item " + currentHit.name;


            //the current hit is not the same as the previous one
            if (currentHit != lastHit)
            {

                if (raycastResults[0].gameObject.tag == "fuse")
                {
                    raycastResults[0].gameObject.GetComponent<FuseButton>().OnnEnter();
                }
                if (lastHit != null)
                {
                    lastHit.GetComponent<FuseButton>().OnnExit();
                }

            }

            lastHit = currentHit;
            previousItem.text = "Previous item " + lastHit.name;



        }
        else if (!hasHitFuseIcon && lastHit != null)
        {
            if (lastHit.tag == "fuse")
            {
                lastHit.gameObject.GetComponent<FuseButton>().OnnExit();
            }
            lastHit = null;
        }

    }
}