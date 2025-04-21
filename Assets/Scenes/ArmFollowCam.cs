using UnityEngine;

public class ArmsFollowCameraFull : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 positionOffset = new Vector3(0f, -0.3f, 0.5f); // tweak for best fit

    void LateUpdate()
    {
        // Follow camera's position with offset
        transform.position = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);

        // Follow ONLY the camera's up/down (pitch) rotation
        Vector3 currentRotation = transform.localEulerAngles;
        currentRotation.x = cameraTransform.eulerAngles.x;
        transform.localEulerAngles = currentRotation;
    }
}
