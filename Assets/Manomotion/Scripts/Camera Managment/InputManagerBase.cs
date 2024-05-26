using System;
using System.IO;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Base class for the input manager 
/// </summary>
public abstract class InputManagerBase : MonoBehaviour
{
    [SerializeField, Range(1, 10), Tooltip("Value to scale down image by in SDK")] protected int splittingFactor = 1;
    [SerializeField] protected bool isFrontFacing;

	protected ManoMotionFrame currentFrame;

    public static Action<ManoMotionFrame> OnFrameInitialized;
    public static Action<ManoMotionFrame> OnFrameResized;
    public static Action<ManoMotionFrame> OnFrameUpdated;

    public static FrameInitializedPointer OnFrameInitializedPointer;
	public delegate void FrameInitializedPointer(Texture2D image, int splittingFactor = 1);

    public static FrameInitializedPointers OnFrameInitializedPointers;
	public delegate void FrameInitializedPointers(Texture2D left, Texture2D right, int splittingFactor = 1, int deviceType = -1);

    public static Action<AddOn> OnAddonSet;
    public static Action OnChangeCamera;

	public bool IsFrontFacing => isFrontFacing;

	public virtual bool IsFrameUpdated() { return true; }

    /// <summary>
    /// Used to change between back- and front facing camera/scenario 
    /// Update with specific changes for each InputManager
    /// </summary>
    public void SetFrontFacing(bool isFrontFacing)
	{
		if (this.isFrontFacing != isFrontFacing)
		{
            this.isFrontFacing = isFrontFacing;
			UpdateFrontFacing(isFrontFacing);
        }
	}

	protected abstract void UpdateFrontFacing(bool isFrontFacing);

    /// <summary>
    /// Forces the application to ask for camera permissions and external storage read and writte.
    /// </summary>
    protected virtual void ForceApplicationPermissions()
	{

#if UNITY_ANDROID
		/* Since 2018.3, Unity doesn't automatically handle permissions on Android, so as soon as
            * the menu is displayed, ask for camera permissions. */
		if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
		{
			Permission.RequestUserPermission(Permission.Camera);
		}
		if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
		{
			Permission.RequestUserPermission(Permission.ExternalStorageWrite);
		}
		if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
		{
			Permission.RequestUserPermission(Permission.ExternalStorageRead);
		}
#endif
	}

	/// <summary>
    /// Checks if the app has storage permissions
    /// </summary>
	public void StoragePermissionCheck()
	{
#if UNITY_ANDROID
		if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
		{
			Permission.RequestUserPermission(Permission.ExternalStorageWrite);
			Debug.Log("I dont have external write");
		}
		if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
		{
			Permission.RequestUserPermission(Permission.ExternalStorageRead);
			Debug.Log("I dont have external read");
		}
#endif
	}

    protected void SaveImage(Texture2D image, string filename)
    {
        byte[] bytes = image.EncodeToPNG();
        File.WriteAllBytes(Application.persistentDataPath + "/" + filename, bytes);
    }

    #region Application on Background

    protected bool isPaused = false;

	/// <summary>
	/// Stops processing when application is put to background.
	/// </summary>
	/// <param name="hasFocus">If application is running or is in background</param>
	protected void OnApplicationFocus(bool hasFocus)
	{
		isPaused = !hasFocus;
		if (isPaused)
		{
			ManomotionManager.Instance.StopProcessing();
		}
	}

	/// <summary>
	/// Stops the processing if application is paused.
	/// </summary>
	/// <param name="pauseStatus">If application is paused or not</param>
	protected void OnApplicationPause(bool pauseStatus)
	{
		isPaused = pauseStatus;
		if (isPaused)
		{
			ManomotionManager.Instance.StopProcessing();
		}
	}

	#endregion
}