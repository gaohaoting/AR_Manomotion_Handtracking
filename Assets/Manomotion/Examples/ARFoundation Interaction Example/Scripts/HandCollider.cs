using UnityEngine;

public class HandCollider : MonoBehaviour
{
    #region Singleton
    private static HandCollider _instance;
    public static HandCollider Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }
    #endregion

    public Vector3 capPos = new Vector3(0, 0, 0);
    private TrackingInfo tracking;
    public Vector3 currentPosition;

    /// <summary>
    /// Set the hand collider tag.
    /// </summary>
    private void Start()
    {
        gameObject.tag = "Player";
    }

    /// <summary>
    /// Get the tracking information from the ManoMotionManager and set the position of the hand Collider according to that.
    /// </summary>
    void Update()
    {
        tracking = ManomotionManager.Instance.HandInfos[0].trackingInfo;
        //currentPosition = Camera.main.ViewportToWorldPoint(new Vector3(tracking.palmCenter.x, tracking.palmCenter.y, tracking.depthEstimation));
        transform.position = currentPosition;
    }
}