using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleMenuCanvas : MonoBehaviour
{
    #region Singleton
    /// <summary>
    /// Creates instance of SkeletonManager
    /// </summary>
    public static ToggleMenuCanvas instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            this.gameObject.SetActive(false);
            Debug.LogWarning("More than 1 ToggleMenuCanvas in scene");
        }
    }
    #endregion

    public GameObject menuCanvas;

    public Slider sliderSmoothing;
    public TMP_Text sliderText;

    public Slider sliderFreq;
    public Slider sliderMinCutoff;
    public Slider sliderBeta;
    public Slider sliderDCutoff;

    public TMP_Text sliderFreqText;
    public TMP_Text sliderMinCutoffText;
    public TMP_Text sliderBetaText;
    public TMP_Text sliderDcutoffText;

    public bool isDemoStarted;

    public void ToggleCanvas()
    {
        menuCanvas.SetActive(!menuCanvas.activeInHierarchy);
    }

    private void Start()
    {
        sliderSmoothing.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        sliderText.text = "Smoothing: " + sliderSmoothing.value;

        sliderFreq.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        sliderFreqText.text = "Frequency: " + sliderFreq.value;
        sliderMinCutoff.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        sliderMinCutoffText.text = "MinCutoff: " + sliderMinCutoff.value;
        sliderBeta.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        sliderBetaText.text = "Beta: " + sliderBeta.value;
        sliderDCutoff.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        sliderDcutoffText.text = "DCutoff: " + sliderDCutoff.value;
    }

    public void ValueChangeCheck()
    {
        ManomotionManager.Instance.SetManoMotionSmoothingValue(sliderSmoothing);
        sliderText.text = "Smoothing: " + sliderSmoothing.value.ToString("f2");

        sliderFreqText.text = "Frequency: " + sliderFreq.value.ToString("f4");
        sliderMinCutoffText.text = "MinCutoff: " + sliderMinCutoff.value.ToString("f4");
        sliderBetaText.text = "Beta: " + sliderBeta.value.ToString("f4");
        sliderDcutoffText.text = "DCutoff: " + sliderDCutoff.value.ToString("f4");
    }

    public void StartDemo()
    {
        isDemoStarted = true;
    }

}