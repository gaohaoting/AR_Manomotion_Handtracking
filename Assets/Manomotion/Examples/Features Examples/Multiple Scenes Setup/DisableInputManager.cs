using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DisableInputManager : MonoBehaviour
{
   // public InputManagerBaseClass inputManger;
    public GameObject ARMM;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int level = scene.buildIndex;

        if (level == 0)
        {
            //ARMM.SetActive(true);
            // inputManger.enabled = true;
        }

        if (level == 1)
        {
            ARMM.SetActive(false);
            // inputManger.enabled = false;
        }
        if (level == 2)
        {
            ARMM.SetActive(true);
            // inputManger.enabled = false;
        }
    }
}
