using UnityEngine;

public class ArmsFollowCameraWithADS : MonoBehaviour
{
    public Transform cameraTransform;
    public CharacterController controller;

    [Header("Offsets")]
    public Vector3 hipOffset = new Vector3(0.29f, -0.21f, 0.24f);
    public Vector3 hipRotation = new Vector3(0f, 17.98f, 0f);  // degrees

    public Vector3 adsOffset = new Vector3(0.03f, -0.12f, 0f);
    public Vector3 adsRotation = new Vector3(0f, 16.4f, 0f);  // ADS aim rotation

    [Header("Bobbing")]
    public float bobSpeed = 6f;
    public float bobAmount = 0.05f;
    public float smoothSpeed = 8f;

    [Header("Shoot Shake")]
    public float shootShakeAmount = 0.05f;
    public float shootShakeSpeed = 15f;

    private float bobTimer;
    private float shootTimer;

    void LateUpdate()
    {
        bool aiming = Input.GetMouseButton(1);

        // Choose target position/rotation
        Vector3 targetOffset = aiming ? adsOffset : hipOffset;
        Vector3 targetRotation = aiming ? adsRotation : hipRotation;

        // Step 1: Base position from camera
        Vector3 basePos = cameraTransform.position + cameraTransform.TransformDirection(targetOffset);

        // Step 2: Bobbing
        float bobOffset = 0f;
        if (controller.velocity.magnitude > 0.1f && controller.isGrounded)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            bobOffset = Mathf.Sin(bobTimer) * bobAmount;
        }
        else bobTimer = 0f;

        Vector3 finalPos = basePos + cameraTransform.up * bobOffset;

        // Step 3: Shooting shake
        if (Input.GetMouseButton(0))
        {
            shootTimer += Time.deltaTime * shootShakeSpeed;
            float shake = Mathf.Sin(shootTimer) * shootShakeAmount;
            finalPos += cameraTransform.forward * shake;
        }
        else shootTimer = 0f;

        // Step 4: Apply position & rotation smoothly
        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);

        Quaternion targetRot = cameraTransform.rotation * Quaternion.Euler(targetRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
    }
}
