using UnityEngine;
using UnityEngine.Events;

public class CameraChangeListener : MonoBehaviour
{
    [SerializeField] InputManagerBase inputManager;
    [SerializeField] UnityEvent OnBackfacing, OnFrontfacing;

    private void OnEnable()
    {
        InputManagerBase.OnChangeCamera += OnCameraChange;
    }

    private void OnDisable()
    {
        InputManagerBase.OnChangeCamera -= OnCameraChange;
    }

    private void OnCameraChange()
    {
        if (inputManager.IsFrontFacing)
            OnFrontfacing?.Invoke();
        else
            OnBackfacing?.Invoke();
    }
}