using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonToggle : MonoBehaviour
{
    public void ToggleSkeleton3D() {
        int currentSkeleton = ManomotionManager.Instance.ManomotionSession.enabledFeatures.skeleton3D;

        if (currentSkeleton == 0) {
            ManomotionManager.Instance.ShouldCalculateSkeleton3D(true);
        }
        else {
            ManomotionManager.Instance.ShouldCalculateSkeleton3D(false);
        }
    }
}
