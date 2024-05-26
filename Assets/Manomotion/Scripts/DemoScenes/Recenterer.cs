using UnityEngine;

public class Recenterer : MonoBehaviour
{
    [SerializeField] Vector3 cameraOffset;

    private void OnEnable()
    {
        Transform camera = Camera.main.transform;
        float magnitude = cameraOffset.magnitude;
        Vector3 offset = camera.TransformDirection(cameraOffset);
        offset.y = 0;
        offset = offset.normalized * magnitude;
        transform.position = camera.position + offset;

        Vector3 forward = camera.forward;
        forward.y = 0;
        transform.LookAt(transform.position + forward);
    }
}