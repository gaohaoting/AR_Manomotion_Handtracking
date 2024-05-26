using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

/// <summary>
/// Gives information about the license being used.
/// </summary>
[AddComponentMenu("ManoMotion/ManoEvents")]
public class ManoEvents : MonoBehaviour
{
	#region Singleton

	protected static ManoEvents instance;
	public static ManoEvents Instance
	{
		get
		{
			return instance;
		}

		set
		{
			instance = value;
		}
	}

	#endregion

	/// <summary>
	/// Animator thats used to display license messages.
	/// </summary>
	[SerializeField]
	private Animator statusAnimator;

	/// <summary>
	/// Animator thats used to display lowPowerMode messages.
	/// </summary>
	[SerializeField]
	private Animator lowPowerAnimator;

	private void Awake()
	{
		if (!instance)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			Debug.Log("More than 1 Mano events instances in scene");
		}
	}

	void Update()
	{
		if (!statusAnimator)
		{
			GameObject.Find("statusAnimator").GetComponent<Animator>();
			Debug.LogError("The application needs the ManoMotion canvas in order to display status messages through the animator");
		}

		HandleManomotionMessages();
#if UNITY_IOS
		CheckForLowPowerMode();
#endif

#if UNITY_ANDROID
		CheckForPowerSaverMode();
#endif
	}

	/// <summary>
	/// Interprets the message from the Manomotion Manager class and assigns a string message to be displayed.
	/// </summary>
	void HandleManomotionMessages()
	{
		switch (ManomotionManager.Instance.ManoLicense.licenseStatus)
		{
			case LicenseAnswer.LICENSE_OK:
				break;
			case LicenseAnswer.LICENSE_KEY_NOT_FOUND:
				DisplayLogMessage("License key not found");
				break;
			case LicenseAnswer.LICENSE_EXPIRED:
				DisplayLogMessage("License expired");
				break;
			case LicenseAnswer.LICENSE_INVALID_PLAN:
				DisplayLogMessage("Invalid plan");
				break;
			case LicenseAnswer.LICENSE_KEY_BLOCKED:
				DisplayLogMessage("Licence key blocked");
				break;
			case LicenseAnswer.LICENSE_INVALID_ACCESS_TOKEN:
				DisplayLogMessage("Invalid access token");
				break;
			case LicenseAnswer.LICENSE_ACCESS_DENIED:
				DisplayLogMessage("License access denied");
				break;
			case LicenseAnswer.LICENSE_MAX_NUM_DEVICES:
				DisplayLogMessage("License out of credits\t");
				break;
			case LicenseAnswer.LICENSE_UNKNOWN_SERVER_REPLY:
				DisplayLogMessage("Unknown Server Reply");
				break;
			case LicenseAnswer.LICENSE_PRODUCT_NOT_FOUND:
				DisplayLogMessage("License product");
				break;
			case LicenseAnswer.LICENSE_INCORRECT_INPUT_PARAMETER:
				DisplayLogMessage("Incorrect License");
				break;
			case LicenseAnswer.LICENSE_INTERNET_REQUIRED:
				DisplayLogMessage("Internet Required");
				break;
			case LicenseAnswer.LICENSE_INCORRECT_BUNDLE_ID:
				DisplayLogMessage("Incorrect Bundle ID");
				break;
			default:
				break;
		}
	}

	/// <summary>
	/// Displays Log messages from the Manomotion Flags 
	/// </summary>
	/// <param name="message">Requires the string message to be displayed</param>
	void DisplayLogMessage(string message)
	{
		if (!statusAnimator)
		{
			statusAnimator = Resources.FindObjectsOfTypeAll<Animator>()[0];
		}
		statusAnimator.Play("SlideIn");
		statusAnimator.GetComponentInChildren<Text>().text = message;
	}


#if UNITY_IOS

private bool hasShowsLowPowerMode = false;
	/// <summary>
	/// Displays Log messages to notify user that LowPowerMode is enabled on the phone.
	/// </summary>
    void CheckForLowPowerMode()
    {
        if(Device.lowPowerModeEnabled && hasShowsLowPowerMode == false)
        {
            StartCoroutine("DispayLowPowerToast", "LowPowerMode Enabled");
            hasShowsLowPowerMode = true;
        }
        if(!Device.lowPowerModeEnabled)
        {
            hasShowsLowPowerMode = false;
        }
    }

#endif


#if UNITY_ANDROID

	private bool hasShowsLowPowerMode = false;
	/// <summary>
	/// Displays Log messages to notify user that PawerSaveMode is enabled on the phone.
	/// </summary>
	void CheckForPowerSaverMode()
	{
		if (ManomotionManager.Instance.ManomotionSession.flag == Flags.ANDROID_SAVE_BATTERY_ON && hasShowsLowPowerMode == false)
		{
			StartCoroutine("DispayLowPowerToast", "PowerSaveMode Enabled");
			hasShowsLowPowerMode = true;
		}
		if (ManomotionManager.Instance.ManomotionSession.flag != Flags.ANDROID_SAVE_BATTERY_ON)
		{
			hasShowsLowPowerMode = false;
		}
	}

#endif

	/// <summary>
	/// Displays the message about LowPowerMode enabled 3 times.
	/// </summary>
	/// <param name="message"></param>
	/// <returns></returns>
	public IEnumerator DispayLowPowerToast(string message)
	{
		lowPowerAnimator.Play("SlideInLow");
		lowPowerAnimator.GetComponentInChildren<Text>().text = message;
		yield return new WaitForSeconds(5);
		lowPowerAnimator.Play("SlideInLow");
		yield return new WaitForSeconds(5);
		lowPowerAnimator.Play("SlideInLow");
	}
}
