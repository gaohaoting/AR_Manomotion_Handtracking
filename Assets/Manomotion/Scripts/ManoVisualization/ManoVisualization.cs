using UnityEngine;

/// <summary>
/// Component that shows the camera as a background and hand occlusion with depth
/// </summary>
public class ManoVisualization : MonoBehaviour
{
    [SerializeField] MeshRenderer manomotionGenericLayerPrefab;
    [SerializeField] bool showBackground;
    [SerializeField] float backgroundDepth;
    [SerializeField] Texture2D noHandTexture;

    MeshRenderer backgroundMeshRenderer;

    public bool ShowBackground
    {
        get { return showBackground; }
        set { showBackground = value; }
    }

    private void Awake()
    {
        backgroundMeshRenderer = Instantiate(manomotionGenericLayerPrefab);
        backgroundMeshRenderer.transform.name = "Background";
        backgroundMeshRenderer.transform.SetParent(Camera.main.transform);
        backgroundMeshRenderer.transform.localPosition = new Vector3(0, 0, backgroundDepth);
    }

    private void LateUpdate()
    {
        backgroundMeshRenderer.enabled = showBackground;
        if (showBackground)
        {
            // Make the background mesh fill the screen
            backgroundMeshRenderer.material.mainTexture = ManomotionManager.Instance.VisualizationInfo.rgbImage;
            ManoUtils.Instance.OrientMeshRenderer(backgroundMeshRenderer);
            ManoUtils.Instance.AdjustBorders(backgroundMeshRenderer, ManomotionManager.Instance.ManomotionSession);
        }

        SetContourActive(showBackground);
    }

    private void SetContourActive(bool value)
    {
        //TODO: Move out contour to separate object.

        if (GizmoManager.Instance)
        {
            GameObject contour = GizmoManager.Instance.GetContour(0);
            if (contour)
            {
                contour.GetComponent<LineRenderer>().enabled = value;
            }
            GameObject contourSecond = GizmoManager.Instance.GetContour(1);
            if (contourSecond)
            {
                contourSecond.GetComponent<LineRenderer>().enabled = value;
            }
        }
        else
        {
            //Debug.LogWarning("No GizmoManager on the scene");
        }
    }
}