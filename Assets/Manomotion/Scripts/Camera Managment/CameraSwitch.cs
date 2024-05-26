using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public void SwitchCamera()
    {
        ManomotionManager.Instance.InputManager.SetFrontFacing(!ManomotionManager.Instance.InputManager.IsFrontFacing);
    }
}