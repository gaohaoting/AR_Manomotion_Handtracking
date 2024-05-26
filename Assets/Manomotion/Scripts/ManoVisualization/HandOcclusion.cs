using UnityEngine;

public class HandOcclusion : MonoBehaviour
{
    [SerializeField] MeshRenderer manomotionGenericTransparentLayer;
    [SerializeField] Texture2D noHandTexture;

    MeshRenderer[] occlusionRenderers = new MeshRenderer[2];
    Transform[] wristJoints = new Transform[2];
    Texture2D[] occlusionRGBTextures = new Texture2D[2];

    bool showHandOcclusion = true;

    private void Awake()
    {
        for (int i = 0; i < occlusionRenderers.Length; i++)
        {
            occlusionRenderers[i] = Instantiate(manomotionGenericTransparentLayer);
            occlusionRenderers[i].transform.name = $"Hand Occlusion {i}";
            occlusionRenderers[i].transform.SetParent(Camera.main.transform);
        }
    }

    private void Start()
    {
        wristJoints[0] = SkeletonManager.instance.joints[0].transform;
        wristJoints[1] = SkeletonManager.instance.jointsSecond[0].transform;
        occlusionRGBTextures[0] = ManomotionManager.Instance.VisualizationInfo.occlusionRGB;
        occlusionRGBTextures[1] = ManomotionManager.Instance.VisualizationInfo.occlusionRGBsecond;
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

    private void Update()
    {
        for (int i = 0; i < occlusionRenderers.Length; i++)
        {
            bool hand = ManomotionManager.Instance.HandInfos[i].gestureInfo.manoClass != ManoClass.NO_HAND;
            occlusionRenderers[i].enabled = showHandOcclusion && hand;

            if (hand)
            {
                // Depth
                float depth = wristJoints[i].position.z;
                occlusionRenderers[i].transform.localPosition = new Vector3(0, 0, depth);
                occlusionRenderers[i].material.mainTexture = occlusionRGBTextures[i];

                // Towards camera
                occlusionRenderers[i].transform.rotation = Camera.main.transform.rotation;

                // Fit to screen
                ManoUtils.Instance.OrientMeshRenderer(occlusionRenderers[i]);
                ManoUtils.Instance.AdjustBorders(occlusionRenderers[i], ManomotionManager.Instance.ManomotionSession);
            }
            else
            {
                occlusionRenderers[i].material.mainTexture = noHandTexture;
            } 
        }
    }
}