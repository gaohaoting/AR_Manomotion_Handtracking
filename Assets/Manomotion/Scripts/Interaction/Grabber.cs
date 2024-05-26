using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    [SerializeField, Range(0, 1)] int handIndex;
    [SerializeField]
    ManoGestureTrigger grabTriggerGesture = ManoGestureTrigger.GRAB_GESTURE,
                                        releaseGestureTrigger = ManoGestureTrigger.RELEASE_GESTURE,
                                        clickGestureTrigger = ManoGestureTrigger.CLICK,
                                        pickGestureTrigger = ManoGestureTrigger.PICK,
                                        dropGestureTrigger = ManoGestureTrigger.DROP;
    [SerializeField] int grabJointIndex = 9, pinchJointIndex = 4;

    [SerializeField] float castRadius = 0.1f;
    [SerializeField] LayerMask grabLayers;

    [SerializeField] float pickDragStartDistance;

    Grabbable hoveredGrabbable;
    Grabbable grabbedGrabbable;

    Grabbable pickedGrabbable;
    bool pickStarted;
    Vector3 pickStartedPosition;

    ManoGestureTrigger grabType;

    private void LateUpdate()
    {
        // Tries to grab and move a Rigidbody
        if (TryGetHandInfo(out HandInfo handInfo))
        {
            // Stop hover current object
            HoverStop();

            if (handInfo.warning != Warning.NO_WARNING)
                return;

            ManoGestureTrigger trigger = handInfo.gestureInfo.manoGestureTrigger;

            if (pickStarted)
            {
                PickUpdate(handInfo, GetWorldPosition(pinchJointIndex), trigger);
                return;
            }

            if (grabbedGrabbable)
            {
                int index = grabType == grabTriggerGesture ? grabJointIndex : pinchJointIndex;
                UpdateGrabbed(GetWorldPosition(index), SkeletonManager.instance.GetHandRotation(handIndex));

                ManoGestureTrigger releaseTrigger = grabType == grabTriggerGesture ? releaseGestureTrigger : dropGestureTrigger;
                if (trigger.Equals(releaseTrigger))
                {
                    Release();
                }
                return;
            }

            // Try to grab or pinch a grabbable
            if (!TryGrab(GetWorldPosition(grabJointIndex), trigger))
            {
                TryClickOrPick(GetWorldPosition(pinchJointIndex), trigger);
            }
        }
        else
        {
            HoverStop();
        }
    }

    /// <summary>
    /// Returns true and gives back the hand info of the left/right hand specified.
    /// </summary>
    private bool TryGetHandInfo(out HandInfo handInfo)
    {
        handInfo = ManomotionManager.Instance.HandInfos[handIndex];
        return !handInfo.gestureInfo.manoClass.Equals(ManoClass.NO_HAND);
    }

    private Vector3 GetWorldPosition(int jointIndex)
    {
        SkeletonManager skeleton = SkeletonManager.instance;
        List<GameObject> joints = handIndex == 0 ? skeleton.joints : skeleton.jointsSecond;
        return joints[jointIndex].transform.position;
    }

    private bool TryHover(Vector3 position, out Grabbable grabbable)
    {
        grabbable = null;

        Collider[] cols = Physics.OverlapSphere(position, castRadius);
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].TryGetComponent(out grabbable) && grabbable.CanBeGrabbed)
            {
                return true;
            }
        }
        return false;
    }

    private bool TryGrab(Vector3 position, ManoGestureTrigger trigger)
    {
        // Start hover grabbable if found
        if (TryHover(position, out Grabbable grabbable))
        {
            // Grab if gesture is triggered
            if (trigger.Equals(grabTriggerGesture))
            {
                Grab(grabbable, grabTriggerGesture);
            }
            else
            {
                HoverStart(grabbable);
            }
        }

        return grabbedGrabbable;
    }

    private bool TryClickOrPick(Vector3 position, ManoGestureTrigger trigger)
    {
        // Start hover grabbable if found
        if (TryHover(position, out Grabbable grabbable))
        {
            // Perform click / pick / hover
            if (trigger.Equals(clickGestureTrigger))
            {
                Click(grabbable);
            }
            else if (trigger.Equals(pickGestureTrigger))
            {
                Pick(grabbable, position);
                return true;
            }
            else
            {
                HoverStart(grabbable);
            }
        }
        return false;
    }

    /// <summary>
    /// Moves the grabbed Rigidbody to the hand with the same distance as when it was grabbed
    /// </summary>
    private void UpdateGrabbed(Vector3 position, Quaternion rotation)
    {
        grabbedGrabbable.Move(position);
        grabbedGrabbable.Rotate(rotation);
    }

    private void HoverStart(Grabbable grabbable)
    {
        HoverStop();
        hoveredGrabbable = grabbable;
        hoveredGrabbable.HoverStart(this);
    }

    private void HoverStop()
    {
        if (hoveredGrabbable)
        {
            hoveredGrabbable.HoverStop(this);
            hoveredGrabbable = null;
        }
    }

    public void Grab(Grabbable grabbable, ManoGestureTrigger gestureTrigger)
    {
        HoverStop();
        grabbedGrabbable = grabbable;
        grabbable.Grab(this);
        grabType = gestureTrigger;
    }

    private void Release()
    {
        HoverStop();
        if (grabbedGrabbable)
        {
            grabbedGrabbable.Release(this);
        }
        grabbedGrabbable = null;
        pickStarted = false;
    }

    private void Click(Grabbable grabbable)
    {
        grabbable.Click(this);
    }

    private void Pick(Grabbable grabbable, Vector3 position)
    {
        pickedGrabbable = grabbable;
        pickStarted = true;
        pickStartedPosition = position;
    }

    private void PickUpdate(HandInfo handInfo, Vector3 position, ManoGestureTrigger trigger)
    {
        if (trigger.Equals(dropGestureTrigger))
        {
            Release();
            return;
        }

        if (Vector3.Distance(pickStartedPosition, position) > pickDragStartDistance)
        {
            DragStart(pickedGrabbable);
        }
    }

    /// <summary>
    /// Called when a Grabbable has been picked and then hand has moved a certain distance.
    /// </summary>
    private void DragStart(Grabbable grabbable)
    {
        grabbable.DragStart(this);
        pickedGrabbable = null;
        pickStarted = false;
    }
}