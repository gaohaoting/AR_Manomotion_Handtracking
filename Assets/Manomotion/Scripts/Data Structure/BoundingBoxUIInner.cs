using UnityEngine;

public class BoundingBoxInner : MonoBehaviour
{
    [SerializeField] LeftOrRightHand leftRightHand;
    [SerializeField] TextMesh topLeft, width, height;
    public LineRenderer boundingLineRenderer;
    float textDepthModifier = 4;
    float textAdjustment = 0.01f;
    float backgroundDepth = 8;

    private void Start()
    {
        boundingLineRenderer.positionCount = 4;
    }

    float normalizedTopLeftX;
    float normalizedTopLeftY;
    float normalizedBBWidth;
    float normalizedHeight;

    Vector3 normalizedTopLeft;
    Vector3 normalizedTopRight;
    Vector3 normalizedBotRight;
    Vector3 normalizedBotLeft;
    Vector3 normalizedTextHeightPosition;
    Vector3 normalizedTextWidth;

    public void UpdateInfo(BoundingBox boundingBox)
    {
        if (!boundingLineRenderer)
        {
            Debug.LogError("Bounding Box missing line renderer Component.");
            return;
        }

        normalizedTopLeftX = boundingBox.topLeft.x;
        normalizedTopLeftY = boundingBox.topLeft.y;
        normalizedBBWidth = boundingBox.width;
        normalizedHeight = boundingBox.height;

        normalizedTopLeft = new Vector3(normalizedTopLeftX, normalizedTopLeftY);
        normalizedTopRight = new Vector3(normalizedTopLeftX + normalizedBBWidth, normalizedTopLeftY);
        normalizedBotRight = new Vector3(normalizedTopLeftX + normalizedBBWidth, normalizedTopLeftY - normalizedHeight);
        normalizedBotLeft = new Vector3(normalizedTopLeftX, normalizedTopLeftY - normalizedHeight);

        boundingLineRenderer.SetPosition(0, ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTopLeft, backgroundDepth));
        boundingLineRenderer.SetPosition(1, ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTopRight, backgroundDepth));
        boundingLineRenderer.SetPosition(2, ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedBotRight, backgroundDepth));
        boundingLineRenderer.SetPosition(3, ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedBotLeft, backgroundDepth));


        normalizedTopLeft.y += textAdjustment * 3;
        topLeft.gameObject.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTopLeft, backgroundDepth / textDepthModifier);
        topLeft.text = "Top Left: " + "X: " + normalizedTopLeftX.ToString("F2") + " Y: " + normalizedTopLeftY.ToString("F2");

        normalizedTextHeightPosition = new Vector3(normalizedTopLeftX + normalizedBBWidth + textAdjustment, (normalizedTopLeftY - normalizedHeight / 2f));
        height.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTextHeightPosition, backgroundDepth / textDepthModifier);
        height.GetComponent<TextMesh>().text = "Height: " + normalizedHeight.ToString("F2");

        normalizedTextWidth = new Vector3(normalizedTopLeftX + normalizedBBWidth / 2f, (normalizedTopLeftY - normalizedHeight) - textAdjustment);
        width.transform.position = ManoUtils.Instance.CalculateNewPositionWithDepth(normalizedTextWidth, backgroundDepth / textDepthModifier);
        width.GetComponent<TextMesh>().text = "Width: " + normalizedBBWidth.ToString("F2");
    }
}
