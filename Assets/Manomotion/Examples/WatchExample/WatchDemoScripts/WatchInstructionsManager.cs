using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WatchInstructionsManager : MonoBehaviour
{
    #region Singleton
    /// <summary>
    /// Creates instance of SkeletonManager
    /// </summary>
    public static WatchInstructionsManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            this.gameObject.SetActive(false);
            Debug.LogWarning("More than 1 WatchInstructionsManager in scene");
        }
    }
    #endregion

    private string firstInstruction = "Place your hand in front of the camera.";

    private string secondInstruction = "Hold your hand in open hand position.";

    private string thirdInstruction = "Place your whole hand inside the frame in open hand position.";

    private string fourthInstruction = "Hand is too far away, move it closer to the camera.";


    private int currentInstruction;

    public int CurrentInstruction
    {
        get { return currentInstruction; }
        set { currentInstruction = value; }
    }

    public TMP_Text instructionText;

    // Start is called before the first frame update
    void Start()
    {
        currentInstruction = 1;
        GizmoManager.Instance.ShowSkeleton3d = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentInstruction)
        {
            case 1:
                instructionText.text = firstInstruction;
                break;
            case 2:
                instructionText.text = secondInstruction;
                break;
            case 3:
                instructionText.text = thirdInstruction;
                break;
            case 4:
                instructionText.text = fourthInstruction;
                break;
            default:
                break;
        }
    }
}
