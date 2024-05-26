using ManoMotion.RunTime;
using ManoMotion.TermsAndServices;
using System;
using UnityEngine;

public class ApplicationManager : MonoBehaviour
{
    private static ApplicationManager instance;
    public static ApplicationManager Instance
    {
        get
        {
            return instance;
        }
    }

    private PrivacyPolicyDisclaimer privacyPolicy;
    public RunTimeApplication runTimeApplication;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("More than 1 ApplicationManagers in the scene");
            Destroy(this.gameObject);
        }
        InitializeComponents();
    }

    /// <summary>
    /// Initializes the components needed in order to operate the application.
    /// </summary>
    void InitializeComponents()
    {
        #region Privacy Policy

        try
        {
            privacyPolicy = this.GetComponent<PrivacyPolicyDisclaimer>();

        }
        catch (Exception)
        {
            privacyPolicy = new PrivacyPolicyDisclaimer();
        }

        privacyPolicy.OnHasApprovedPrivacyPolicy += HandlePrivacyPolicyAccepted;

        #endregion

        #region RunTimeApplication

        try
        {
            runTimeApplication = this.GetComponent<RunTimeApplication>();
        }
        catch (Exception)
        {
            runTimeApplication = new RunTimeApplication();
        }

        runTimeApplication.InitializeRuntimeComponents();

        #endregion
    }

    private void OnDestroy()
    {
        privacyPolicy.OnHasApprovedPrivacyPolicy -= HandlePrivacyPolicyAccepted;
    }

    private void Start()
    {
        privacyPolicy.InitializeUsageDisclaimer();
    }

    /// <summary>
    /// Handles the privacy policy accepted.
    /// </summary>
    void HandlePrivacyPolicyAccepted()
    {
        Debug.Log("Privacy Policy Accepted");
        runTimeApplication.HideApplicationComponents();
    }

    /// <summary>
    /// Forces the instructions to be seen even if seen in the past. Used from within the main menu.
    /// </summary>
    public void ForceInstructions()
    {
        runTimeApplication.SaveDefalutFeaturesToDisplay();
        runTimeApplication.SetMenuIconVisibility();
        runTimeApplication.HideApplicationComponents();
        runTimeApplication.ShouldShowGestures(true);
    }

    /// <summary>
    /// Handles the logic and what happens after the player has seen all of the instructions.
    /// </summary>
    void HandleHowToInstructionsFinished()
    {
        runTimeApplication.StartMainApplicationWithDefaultSettings();
    }

    /// <summary>
    /// Handles the logic and what happens after the player has skipped the instructions. For now it calls the same method as if seen them all.
    /// </summary>
    void HandleHowToInstructionsSkipped()
    {
        HandleHowToInstructionsFinished();
    }
}