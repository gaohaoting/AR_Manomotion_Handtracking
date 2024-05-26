using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class SceneOption : MonoBehaviour
{
    [SerializeField] GameObject sceneParent;
    [SerializeField] bool frontFacingCamera = false, handOcclusion = false, showSkeleton = true, lockToLandscape = false;
    [SerializeField] Color selectedColor;
    [SerializeField] UnityEvent OnSelected, OnDeselected;

    Image image;
    Color baseColor;

    public float Width { get; private set; }
    public float Height { get; private set; }
    private Image Image
    {
        get
        {
            if (image == null)
            {
                image = GetComponent<Image>();
                baseColor = image.color;
            }
            return image;
        }
    }

    void Awake()
    {
        if (!image)
        {
            image = GetComponent<Image>();
            baseColor = image.color;
        }

        Width = GetComponent<RectTransform>().rect.width;
        Height = GetComponent<RectTransform>().rect.height;
    }

    public virtual void Select()
    {
        Image.color = selectedColor;

        if (gameObject.activeInHierarchy)
        {
            sceneParent.SetActive(true);
            UpdateSessionFeatures();
            OnSelected?.Invoke();
        }
    }

    public virtual void Deselect()
    {
        Image.color = baseColor;

        if (gameObject.activeInHierarchy)
        {
            sceneParent.SetActive(false);
            OnDeselected?.Invoke();
        }
    }

    private void UpdateSessionFeatures()
    {
        ManomotionManager manager = ManomotionManager.Instance;
        manager.InputManager.SetFrontFacing(frontFacingCamera);
        manager.ShouldRunContour(handOcclusion);
        SkeletonManager.instance.ShouldShowSkeleton = showSkeleton;

        //Screen.autorotateToPortrait = !lockToLandscape;
        //Screen.autorotateToPortraitUpsideDown = !lockToLandscape;
        ManoUtils.Instance.updateOrientation = !lockToLandscape;
        if (lockToLandscape)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            ManoUtils.Instance.SetOrientation(SupportedOrientation.LANDSCAPE_LEFT);
        }
        else
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}