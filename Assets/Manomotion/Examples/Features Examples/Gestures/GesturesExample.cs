using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GesturesExample : MonoBehaviour
{

    [SerializeField]
    private GameObject TriggerTextGizmo;
    // Start is called before the first frame update
    void Start() {
        InitializeTriggerPool();
    }

    // Update is called once per frame
    void Update() {
        GestureInfo gestureInfo = ManomotionManager.Instance.HandInfos[0].gestureInfo;
        TrackingInfo trackingInfo = ManomotionManager.Instance.HandInfos[0].trackingInfo;
        Warning warning = ManomotionManager.Instance.HandInfos[0].warning;
        Session session = ManomotionManager.Instance.ManomotionSession;

        DisplayTriggerGesture(gestureInfo.manoGestureTrigger, trackingInfo);
    }

    public List<GameObject> triggerObjectPool = new List<GameObject>();
    public int amountToPool = 20;

    /// <summary>
    /// Initializes the object pool for trigger gestures.
    /// </summary>
    private void InitializeTriggerPool() {
        for (int i = 0; i < amountToPool; i++) {
            GameObject newTriggerObject = Instantiate(TriggerTextGizmo);
            newTriggerObject.transform.SetParent(transform);
            newTriggerObject.SetActive(false);
            triggerObjectPool.Add(newTriggerObject);
        }
    }

    private ManoGestureTrigger previousTrigger;

    /// <summary>
    /// Display Visual information of the detected trigger gesture and trigger swipes.
    /// In the case where a click is intended (Open pinch, Closed Pinch, Open Pinch) we are clearing out the visual information that are generated from the pick/drop
    /// </summary>
    /// <param name="triggerGesture">Requires an input from ManoGestureTrigger.</param>
    /// <param name="trackingInfo">Requires an input of tracking info.</param>
    void DisplayTriggerGesture(ManoGestureTrigger triggerGesture, TrackingInfo trackingInfo) {

        if (triggerGesture != ManoGestureTrigger.NO_GESTURE) {

            if (triggerGesture == ManoGestureTrigger.PICK) {
                TriggerDisplay(trackingInfo, ManoGestureTrigger.PICK);
            }
            if (triggerGesture == ManoGestureTrigger.DROP) {
                if (previousTrigger != ManoGestureTrigger.CLICK) {
                    TriggerDisplay(trackingInfo, ManoGestureTrigger.DROP);
                }
            }

            if (triggerGesture == ManoGestureTrigger.CLICK) {
                TriggerDisplay(trackingInfo, ManoGestureTrigger.CLICK);
                if (GameObject.Find("PICK")) {
                    GameObject.Find("PICK").SetActive(false);
                }
            }

            if (triggerGesture == ManoGestureTrigger.SWIPE_LEFT) {
                TriggerDisplay(trackingInfo, ManoGestureTrigger.SWIPE_LEFT);
            }

            if (triggerGesture == ManoGestureTrigger.SWIPE_RIGHT) {
                TriggerDisplay(trackingInfo, ManoGestureTrigger.SWIPE_RIGHT);
            }

            if (triggerGesture == ManoGestureTrigger.SWIPE_UP) {
                TriggerDisplay(trackingInfo, ManoGestureTrigger.SWIPE_UP);
            }

            if (triggerGesture == ManoGestureTrigger.SWIPE_DOWN) {
                TriggerDisplay(trackingInfo, ManoGestureTrigger.SWIPE_DOWN);
            }

            if (triggerGesture == ManoGestureTrigger.GRAB_GESTURE) {
                TriggerDisplay(trackingInfo, ManoGestureTrigger.GRAB_GESTURE);
            }     
        }

        if (triggerGesture == ManoGestureTrigger.RELEASE_GESTURE) {
            TriggerDisplay(trackingInfo, ManoGestureTrigger.RELEASE_GESTURE);
        }

        previousTrigger = triggerGesture;
    }

    void TriggerDisplay(TrackingInfo trackingInfo, ManoGestureTrigger triggerGesture) {

        if (GetCurrentPooledTrigger()) {
            GameObject triggerVisualInformation = GetCurrentPooledTrigger();

            triggerVisualInformation.SetActive(true);
            triggerVisualInformation.name = triggerGesture.ToString();
            triggerVisualInformation.GetComponent<TriggerGizmo>().InitializeTriggerGizmo(triggerGesture);
            //triggerVisualInformation.GetComponent<RectTransform>().position = Camera.main.ViewportToScreenPoint(trackingInfo.palmCenter);

        }
    }

    /// <summary>
    /// Gets the current pooled trigger object.
    /// </summary>
    /// <returns>The current pooled trigger.</returns>
    private GameObject GetCurrentPooledTrigger() {
        for (int i = 0; i < triggerObjectPool.Count; i++) {
            if (!triggerObjectPool[i].activeInHierarchy) {
                return triggerObjectPool[i];
            }
        }
        return null;
    }
}
