using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WatchSelector : MonoBehaviour
{
    public GameObject[] watches;

    public void ActivateObject(GameObject objectToActivate)
    {
        foreach (GameObject o in watches)
        {
            o.SetActive(false);
        }
        objectToActivate.SetActive(true);
    }
}
