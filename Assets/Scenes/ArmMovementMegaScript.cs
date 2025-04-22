using UnityEngine;

public class ArmMovementMegaScript : MonoBehaviour
{
    public Transform cameraTransform;
    public CharacterController controller;

    [Header("Offsets")]
    public Vector3 hipOffset = new Vector3(0.29f, -0.21f, 0.24f);
    public Vector3 hipRotation = new Vector3(0f, 17.98f, 0f);

    public Vector3 adsOffset = new Vector3(0.03f, -0.12f, 0f);
    public Vector3 adsRotation = new Vector3(0f, 16.4f, 0f);

    public Vector3 sprintOffset = new Vector3(0.25f, -0.4f, 0.4f);
    public Vector3 sprintRotation = new Vector3(20f, 0f, 0f);

    public Vector3 sprintBackOffset = new Vector3(0.2f, -0.3f, 0.6f);
    public Vector3 sprintBackRotation = new Vector3(30f, 0f, 0f);

    [Header("Bobbing")]
    public float bobSpeed = 6f;
    public float bobAmount = 0.05f;
    public float sprintBobSpeed = 10f;
    public float sprintSideBobAmount = 0.08f;
    public float smoothSpeed = 8f;

    [Header("Shoot Shake")]
    public float shootShakeAmount = 0.05f;
    public float shootShakeSpeed = 15f;

    private float bobTimer;
    private float shootTimer;

    void LateUpdate()
    {
        bool isAiming = Input.GetMouseButton(1);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isAiming;
        bool isBackpedaling = Input.GetKey(KeyCode.S);

        // Target state selection
        Vector3 targetOffset;
        Vector3 targetRotation;

        if (isSprinting && isBackpedaling)
        {
            targetOffset = sprintBackOffset;
            targetRotation = sprintBackRotation;
        }
        else if (isSprinting)
        {
            targetOffset = sprintOffset;
            targetRotation = sprintRotation;
        }
        else if (isAiming)
        {
            targetOffset = adsOffset;
            targetRotation = adsRotation;
        }
        else
        {
            targetOffset = hipOffset;
            targetRotation = hipRotation;
        }

        // Base follow position
        Vector3 basePos = cameraTransform.position + cameraTransform.TransformDirection(targetOffset);

        // Bobbing — only if NOT aiming
        float bobOffset = 0f;
        float sideBobOffset = 0f;

        if (!isAiming && controller.isGrounded)
        {
            float activeSpeed = isSprinting ? sprintBobSpeed : bobSpeed;
            bobTimer += Time.deltaTime * activeSpeed;
            bobOffset = Mathf.Sin(bobTimer) * bobAmount;

            if (isSprinting)
            {
                sideBobOffset = Mathf.Sin(bobTimer * 0.5f) * sprintSideBobAmount;
            }
        }
        else
        {
            bobTimer = 0f;
        }

        Vector3 finalPos = basePos +
                           cameraTransform.up * bobOffset +
                           cameraTransform.right * sideBobOffset;

        // Shooting shake (disabled while sprinting)
        if (Input.GetMouseButton(0) && !isSprinting)
        {
            shootTimer += Time.deltaTime * shootShakeSpeed;
            float shootShake = Mathf.Sin(shootTimer) * shootShakeAmount;
            finalPos += cameraTransform.forward * shootShake;
        }
        else
        {
            shootTimer = 0f;
        }

        // Apply position & rotation
        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);
        Quaternion targetRot = cameraTransform.rotation * Quaternion.Euler(targetRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
    }
}
