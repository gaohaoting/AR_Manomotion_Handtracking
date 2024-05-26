using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoHandsToggle : MonoBehaviour
{
    public void ToggleTwoHands() {
        int currentTwoHandsMode = ManomotionManager.Instance.ManomotionSession.enabledFeatures.twoHands;

        if (currentTwoHandsMode == 0) {
            ManomotionManager.Instance.ShouldRunTwoHands(true);
        }
        else {
            ManomotionManager.Instance.ShouldRunTwoHands(false);
        }
    }
}
