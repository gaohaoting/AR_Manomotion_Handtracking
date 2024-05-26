using UnityEngine;
using UnityEditor;

public class ManoMotionLicense : ScriptableObject
{
    [SerializeField] string licenseKey;
    [Tooltip("Insert the bundle ID gotten from the webpage here https://www.manomotion.com/my-account/licenses/")]
    [SerializeField] string bundleID;

    public string LicenseKey => licenseKey;
    public string BundleID => bundleID;

#if UNITY_EDITOR
    private void OnValidate()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, bundleID);
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleID);
    }
#endif
}