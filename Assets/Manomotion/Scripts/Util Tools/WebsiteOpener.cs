using UnityEngine;

public class WebsiteOpener : MonoBehaviour
{
    [SerializeField] string url;

    public void OpenWebsite()
    {
        Application.OpenURL(url);
    }
}