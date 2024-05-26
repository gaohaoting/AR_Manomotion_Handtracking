using UnityEngine;
using UnityEngine.UI;

public class HandOcclusionARFoundation : MonoBehaviour
{
    [SerializeField] bool showHandOcclusion = true;
    [SerializeField] Canvas[] cutoutCanvases = new Canvas[2];
    [SerializeField] RawImage[] handCutouts = new RawImage[2];
    [SerializeField] Texture2D noHandTexture;

    Transform[] wristJoints = new Transform[2];
    Texture2D[] occlusionRGBTextures = new Texture2D[2];

    public bool ShowHandOcclusion
    {
        get { return showHandOcclusion; }
        set { showHandOcclusion = value; }
    }

    private void Start()
    {
        // Cache references to textures
        occlusionRGBTextures[0] = ManomotionManager.Instance.VisualizationInfo.occlusionRGB;
        occlusionRGBTextures[1] = ManomotionManager.Instance.VisualizationInfo.occlusionRGBsecond;
    }

    private void LateUpdate()
    {
        wristJoints[0] = SkeletonManager.instance.joints[0].transform;
        wristJoints[1] = SkeletonManager.instance.jointsSecond[0].transform;

        for (int i = 0; i < cutoutCanvases.Length; i++)
        {
            Canvas cutoutCanvas = cutoutCanvases[i];
            cutoutCanvas.gameObject.SetActive(showHandOcclusion);
            HandInfo handInfo = ManomotionManager.Instance.HandInfos[i];
            Texture2D texture = occlusionRGBTextures[i];
            SetCutoutTexture(handInfo, cutoutCanvas, handCutouts[i], texture, i);
        }
    }

    private void SetCutoutTexture(HandInfo handInfo, Canvas canvas, RawImage image, Texture2D cutoutTexture, int index)
    {
        if (handInfo.gestureInfo.manoClass != ManoClass.NO_HAND)
        {
            image.texture = cutoutTexture;
            float depth = wristJoints[index].position.z;
            canvas.planeDistance = depth;
        }
        else
        {
            image.texture = noHandTexture;
        }
    }

    private void OnEnable()
    {
        ManomotionManager.OnContourToggle += ContourToggle;
    }

    private void OnDisable()
    {
        ManomotionManager.OnContourToggle -= ContourToggle;
    }

    private void ContourToggle(bool active)
    {
        showHandOcclusion = active;
    }
}