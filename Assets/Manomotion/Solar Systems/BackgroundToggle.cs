using UnityEngine;

public class BackgroundToggle : MonoBehaviour
{
    [SerializeField] ManoGestureTrigger toggleBackgroundTrigger = ManoGestureTrigger.GRAB_GESTURE;
    [SerializeField] float toggleCooldown = 1f;
    [SerializeField] MeshRenderer background;
    [SerializeField] GameObject planets;
    [SerializeField] Material space, black;

    bool isSpaceBackground = true;
    float lastToggleTime = float.MinValue;

    private void LateUpdate()
    {
        for (int i = 0; i <= ManomotionManager.Instance.ManomotionSession.enabledFeatures.twoHands ; i++)
        {
            if (ManomotionManager.Instance.TryGetHandInfo((LeftOrRightHand)i, out HandInfo handInfo))
            {
                bool toggle = handInfo.gestureInfo.manoGestureTrigger.Equals(toggleBackgroundTrigger);
                if (toggle && lastToggleTime + toggleCooldown < Time.time)
                {
                    lastToggleTime = Time.time;
                    ToggleBackground();
                }
            }
        }
    }

    private void ToggleBackground()
    {
        isSpaceBackground = !isSpaceBackground;
        background.material = isSpaceBackground ? space : black;
        planets.SetActive(isSpaceBackground);
    }
}