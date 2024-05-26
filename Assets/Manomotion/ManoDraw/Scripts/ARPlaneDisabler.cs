using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARPlaneDisabler : MonoBehaviour
{
    private ARPlaneManager planeManager;

    // Start is called before the first frame update
    void Start()
    {
        planeManager = GetComponent<ARPlaneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }
}
