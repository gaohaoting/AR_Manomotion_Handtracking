using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the visualization of the skeleton joints.
/// </summary>
public class SkeletonManager : MonoBehaviour
{
    ///The list of joints used for visualization
    [HideInInspector] public List<GameObject> joints = new List<GameObject>();
    [HideInInspector] public List<GameObject> jointsSecond = new List<GameObject>();

    ///The linerenderes used on the joints in the jointPrefabs
    private LineRenderer[] lineRenderers = new LineRenderer[6];

    ///Skeleton confidence
    [SerializeField] private bool shouldShowSkeleton = true;
    [HideInInspector] private float skeletonConfidenceThreshold = 0.0001f;

    ///Use this to make the depth values smaler to fit the depth of the hand. 
    private int depthDivider = 6;

    /// The number of Joints the skeleton is made of.
    const int JointsLength = 21;

    private float depthEstimation;

    [SerializeField]
    private GameObject skeletonPrefab2D, skeletonPrefab3D;
    private GameObject skeletonModel, skeletonModelSecond;

    private ManomotionManager.SkeletonModel lastModelLoaded = ManomotionManager.SkeletonModel.SKEL_2D;
    private GameObject skeletonParent;

    private Session session;

    private Renderer[] renderers, renderersSecond;

    // Depth settings for the entire hand and joints
    private float jointDepthMin = 0;
    private float jointDepthMax = 0.3f;
    private float handDepthMin = 0;
    private float handDepthMax = 1f;
    /*SerializeField, Range(-1, 1)]*/ float depthExtraValue;
    /*SerializeField, Range(0, 0.3f)]*/ float handDepthTooCloseMin = 0f, handDepthTooCloseMax = 0.3f;

    private bool updateFilterParams = false;
    private OneEuroFilterSetting positionFilterSetting = new OneEuroFilterSetting(120, 0.0001f, 500f, 1f);
    private OneEuroFilterSetting rotationFilterSetting = new OneEuroFilterSetting(180, 5f, 0f, 1f);
    private OneEuroFilterSetting depthFilterSetting = new OneEuroFilterSetting(120, 0.0001f, 5f, 1f);

    private List<OneEuroFilter<Vector3>> positionFilter = new List<OneEuroFilter<Vector3>>();
    private List<OneEuroFilter<Vector3>> positionFilterSecond = new List<OneEuroFilter<Vector3>>();
    private List<OneEuroFilter<Quaternion>> rotationFilter = new List<OneEuroFilter<Quaternion>>();
    private List<OneEuroFilter<Quaternion>> rotationFilterSecond = new List<OneEuroFilter<Quaternion>>();
    private OneEuroFilter<Vector2>[] handDepthFilters = new OneEuroFilter<Vector2>[2];
    private Quaternion[] handRotations = new Quaternion[2];

    public int GetDepthDivider
    {
        get { return depthDivider; }
    }

    public float GetClampedDepthEstimation
    {
        get { return depthEstimation; }
    }

    public bool ShouldShowSkeleton
    {
        get { return shouldShowSkeleton; }
        set { shouldShowSkeleton = value; }
    }

    #region Singleton
    /// <summary>
    /// Creates instance of SkeletonManager
    /// </summary>
    public static SkeletonManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            gameObject.SetActive(false);
            Debug.LogWarning("More than 1 SkeletonManager in scene");
        } 
    }
    #endregion

    private void Start()
    {
        CreateSkeletonParent();
        ChangeSkeletonModel(ManomotionManager.SkeletonModel.SKEL_3D);
        CreateOneEuroFilters();
    }

    private void OnEnable()
    {
        ManomotionManager.OnSkeletonActivated += ChangeSkeletonModel;
        ManomotionManager.OnSmoothingValueChanged += UpdateSmoothingFilters;
        ManomotionManager.OnTwoHandsToggle += HandleTwoHandsToggle;
    }

    private void OnDisable()
    {
        ManomotionManager.OnSkeletonActivated = null;
        ManomotionManager.OnSmoothingValueChanged = null;
        ManomotionManager.OnTwoHandsToggle = null;
    }

    private void CreateSkeletonParent()
    {
        skeletonParent = new GameObject();
        skeletonParent.name = "SkeletonParent";
    }

    /// <summary>
    /// Creates a OneEuroFilter for each of the joints
    /// </summary>
    private void CreateOneEuroFilters()
    {
        for (int i = 0; i < handDepthFilters.Length; i++)
        {
            handDepthFilters[i] = new OneEuroFilter<Vector2>(depthFilterSetting);
        }

        for (int i = 0; i < JointsLength; i++)
        {
            positionFilter.Add(new OneEuroFilter<Vector3>(positionFilterSetting));
            rotationFilter.Add(new OneEuroFilter<Quaternion>(rotationFilterSetting));
        }

        if (session.enabledFeatures.twoHands == 1)
        {
            for (int i = 0; i < JointsLength; i++)
            {
                positionFilterSecond.Add(new OneEuroFilter<Vector3>(positionFilterSetting));
                rotationFilterSecond.Add(new OneEuroFilter<Quaternion>(rotationFilterSetting));
            }
        }
    }

    /// <summary>
    /// Changes the current skeleton model for a new one
    /// </summary>
    /// <param name="skeleton">Type of skeleton (2D or 3D)</param>
    private void ChangeSkeletonModel(ManomotionManager.SkeletonModel skeleton)
    {
        session = ManomotionManager.Instance.ManomotionSession;
        GameObject modelToLoad;
        switch (skeleton)
        {
            case ManomotionManager.SkeletonModel.SKEL_2D:
                modelToLoad = skeletonPrefab2D;
                lastModelLoaded = ManomotionManager.SkeletonModel.SKEL_2D;
                break;
            case ManomotionManager.SkeletonModel.SKEL_3D:
                modelToLoad = skeletonPrefab3D;
                lastModelLoaded = ManomotionManager.SkeletonModel.SKEL_3D;
                break;
            default:
                modelToLoad = skeletonPrefab2D;
                lastModelLoaded = ManomotionManager.SkeletonModel.SKEL_2D;
                break;
        }

        joints.Clear();
        jointsSecond.Clear();
        Destroy(skeletonModel);
        Destroy(skeletonModelSecond);

        skeletonModel = Instantiate(modelToLoad, skeletonParent.transform);

        for (int i = 0; i < skeletonModel.transform.childCount; i++)
        {
            joints.Add(skeletonModel.transform.GetChild(i).gameObject);
        }

        lineRenderers = new LineRenderer[6];
        lineRenderers = skeletonModel.GetComponentsInChildren<LineRenderer>();
        ResetLineRenderers();

        renderers = joints[0].transform.parent.gameObject.GetComponentsInChildren<Renderer>();

        if (session.enabledFeatures.twoHands == 1)
        {
            skeletonModelSecond = Instantiate(modelToLoad, skeletonParent.transform);

            for (int i = 0; i < skeletonModelSecond.transform.childCount; i++)
            {
                jointsSecond.Add(skeletonModelSecond.transform.GetChild(i).gameObject);
            }

            lineRenderers = new LineRenderer[6];
            lineRenderers = skeletonModelSecond.GetComponentsInChildren<LineRenderer>();
            ResetLineRenderers();
            renderersSecond = jointsSecond[0].transform.parent.gameObject.GetComponentsInChildren<Renderer>();
        }
    }

    /// <summary>
    /// Reset the Linerenders when changing Skeleton Model 2D/3D
    /// </summary>
    private void ResetLineRenderers()
    {
        foreach (var item in lineRenderers)
        {
            item.enabled = true;
            item.positionCount = 0;
            item.positionCount = 4;
        }

        lineRenderers[1].positionCount = 6;
    }

    /// <summary>
    /// Updates the OneEuroFilters
    /// </summary>
    /// <param name="value">0-1 normalized value for denormalizing</param>
    private void UpdateSmoothingFilters(float value)
    {
        //positionFilterSetting.CalculateOneEuroSmoothing(value);

        for (int i = 0; i < handDepthFilters.Length; i++)
        {
            handDepthFilters[i].UpdateParams(depthFilterSetting);
        }

        for (int i = 0; i < JointsLength; i++)
        {
            positionFilter[i].UpdateParams(positionFilterSetting);
            rotationFilter[i].UpdateParams(rotationFilterSetting);
        }

        if (session.enabledFeatures.twoHands == 1)
        {
            for (int i = 0; i < JointsLength; i++)
            {
                positionFilterSecond[i].UpdateParams(positionFilterSetting);
                rotationFilterSecond[i].UpdateParams(rotationFilterSetting);
            }
        }
    }

    private void HandleTwoHandsToggle()
    {
        ChangeSkeletonModel(lastModelLoaded);
        CreateOneEuroFilters();
    }

    void Update()
    {
        if (updateFilterParams)
        {
            UpdateSmoothingFilters(ManomotionManager.Instance.oneEuroFilterSmoothing);
        }
        
        HandInfo handInfo0 = ManomotionManager.Instance.HandInfos[0];
        session = ManomotionManager.Instance.ManomotionSession;
        skeletonParent.transform.rotation = Camera.main.transform.rotation;
        UpdateJointsPosition(ref joints, handInfo0, positionFilter, handDepthFilters[0]);
        FadeSkeletonJoints(renderers, handInfo0);
        if (session.enabledFeatures.skeleton3D == 1)
        {
            UpdateJointsOrientation(ref joints, handInfo0, 0);
        }

        //Two hands
        if (session.enabledFeatures.twoHands == 1)
        {
            HandInfo handInfo1 = ManomotionManager.Instance.HandInfos[1];
            UpdateJointsPosition(ref jointsSecond, handInfo1, positionFilterSecond, handDepthFilters[1]);
            FadeSkeletonJoints(renderersSecond, handInfo1);
            if (session.enabledFeatures.skeleton3D == 1)
            {
                UpdateJointsOrientation(ref jointsSecond, handInfo1, 1);
            }
        }
    }

    private void UpdateJointsPosition(ref List<GameObject> skeletonJoints, HandInfo handInfo, List<OneEuroFilter<Vector3>> oneEuroFiltersToApply, OneEuroFilter<Vector2> depthFilter)
    {
        SkeletonInfo skeletonInfo = handInfo.trackingInfo.skeleton;
        WorldSkeletonInfo worldSkeletonInfo = handInfo.trackingInfo.worldSkeleton;
        depthEstimation = handInfo.trackingInfo.depthEstimation;

        float handDepth = Mathf.Clamp(depthEstimation, 0.1f, 1);

        //if (depthEstimation > handDepthTooCloseMax)
        //{
        //    handDepth = Mathf.Lerp(handDepthMin, handDepthMax, depthEstimation);
        //}
        //else // Depth gets too close at values below 0.3f
        //{
        //    float t = Mathf.InverseLerp(0, handDepthTooCloseMax, depthEstimation);
        //    handDepth = Mathf.Lerp(handDepthTooCloseMin, handDepthTooCloseMax, t);
        //}

        //handDepth += depthExtraValue;
        handDepth = depthFilter.Filter(new Vector2(handDepth, 0)).x;

        if (skeletonInfo.jointPositions != null)
        {
            for (int i = 0; i < skeletonInfo.jointPositions.Length; i++)
            {
                Vector3 jointDepth = skeletonInfo.jointPositions[i];
                jointDepth.z = worldSkeletonInfo.jointPositions[i].z * 0.1f;

                Vector3 newPosition3D = oneEuroFiltersToApply[i].Filter(ManoUtils.Instance.CalculateNewPositionWithDepth(jointDepth, handDepth));

                skeletonJoints[i].transform.position = newPosition3D;
            }
        }
    }

    /// <summary>
    /// Updates the orientation of the joints according to the orientation given by the SDK.
    /// </summary>
    private void UpdateJointsOrientation(ref List<GameObject> listToModify, HandInfo handInfo, int handIndex)
    {
        handRotations[handIndex] = rotationFilter[handIndex].Filter(RotationUtility.GetHandRotation(handInfo));

        //for (int i = 0; i < listToModify.Count; i++)
        //{
        //    Quaternion rotation = RotationUtility.GetFingerJointRotation(handInfo, i);
        //    listToModify[i].transform.localRotation = rotation;
        //}
        //SkeletonInfo skeletonInfo = handInfo.trackingInfo.skeleton;
        //if (skeletonInfo.jointNormalizedRotations == null) return;
        //for (int i = 0; i < skeletonInfo.jointNormalizedRotations.Length; i++)
        //{
        //    float xRotation = radianToDegrees(skeletonInfo.jointNormalizedRotations[i].x);
        //    float yRotation = radianToDegrees(skeletonInfo.jointNormalizedRotations[i].y);
        //    float zRotation = radianToDegrees(skeletonInfo.jointNormalizedRotations[i].z);

        //    ///Correct the joint orientation if left hand is used with back facing orienations
        //    if (handInfo.gestureInfo.leftRightHand == LeftOrRightHand.LEFT_HAND && session.orientation < SupportedOrientation.PORTRAIT_FRONT_FACING)
        //    {
        //        yRotation = radianToDegrees(3.14f + skeletonInfo.jointNormalizedRotations[i].y);
        //    }

        //    ///Correct the joint orientation if right hand is used with front facing orienations 
        //    if (handInfo.gestureInfo.leftRightHand == LeftOrRightHand.RIGHT_HAND && session.orientation > SupportedOrientation.FACE_DOWN)
        //    {
        //        yRotation = radianToDegrees(3.14f + skeletonInfo.jointNormalizedRotations[i].y);
        //    }

        //    if (handInfo.gestureInfo.handSide == HandSide.Palmside)
        //    {
        //        xRotation = -xRotation;
        //        zRotation = -zRotation;
        //    }

        //    Vector3 newRotation = new Vector3(xRotation, yRotation, zRotation);

        //    listToModify[i].transform.localEulerAngles = newRotation;
        //}
    }

    /// <summary>
    /// Fade the skeleton materials when no hand is detected.
    /// </summary>
    /// <param name="renderers">The renderers of the skeleton model</param>
    /// <param name="hand_info">the hand info using the skeleton model</param>
    private void FadeSkeletonJoints(Renderer[] renderers, HandInfo hand_info)
    {
        SkeletonInfo skeletonInfo_ = hand_info.trackingInfo.skeleton;
        bool hasConfidence = skeletonInfo_.confidence > skeletonConfidenceThreshold;

        if (hasConfidence && shouldShowSkeleton)
        {
            if (renderers[0].material.color.a < 1)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Color tempColor = renderers[i].material.color;
                    tempColor.a += 0.1f;
                    renderers[i].material.color = tempColor;
                }
            }
        }
        else
        {
            if (renderers[0].material.color.a > 0)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Color tempColor = renderers[i].material.color;
                    tempColor.a -= 0.1f;
                    renderers[i].material.color = tempColor;
                }
            }
        }
    }

    /// <summary>
    /// Calculates the radian value to degrees
    /// </summary>
    /// <param name="radiantValue">the radiant value</param>
    /// <returns></returns>
    private float radianToDegrees(float radiantValue)
    {
        float degreeValue;
        degreeValue = radiantValue * Mathf.Rad2Deg;
        return degreeValue;
    }

    public Quaternion GetHandRotation(int handIndex)
    {
        return handRotations[handIndex];
    }
}