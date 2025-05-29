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
    public Vector3 sprintRotation = new Vector3(20f, 0f, 8.14f);

    public Vector3 sprintBackOffset = new Vector3(0.2f, -0.3f, 0.39f);
    public Vector3 sprintBackRotation = new Vector3(-26.6f, -1.35f, 0f);

    [Header("Recoil Settings")]
    public float recoilAmount = 0.05f;
    public float recoilReturnSpeed = 10f;

    [Header("Bobbing")]
    public float bobSpeed = 4f;
    public float bobAmount = 0.015f;
    public float sprintBobSpeed = 26.26f;
    public float sprintSideBobAmount = 0.26f;
    public float smoothSpeed = 8f;

    [Header("Idle Bobbing")]
    public float idleBobSpeed = 2f;
    public float idleBobAmount = 0.005f;

    private Vector3 defaultLocalPosition;
    private Vector3 defaultLocalRotation;



    private float bobTimer;
    private Vector3 currentRecoilOffset = Vector3.zero;
    private Vector3 targetRecoilOffset = Vector3.zero;

    void Start()
{
    defaultLocalPosition = transform.localPosition;
    defaultLocalRotation = transform.localRotation.eulerAngles;
}


    void LateUpdate()
    {
        bool isAiming = Input.GetMouseButton(1);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isAiming;
        bool isBackpedaling = Input.GetKey(KeyCode.S);

        // Target offset & rotation
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

        // Base position
        Vector3 basePos = cameraTransform.position + cameraTransform.TransformDirection(targetOffset);

        // Sprinting/idle bob
        float bobOffset = 0f;
        float sideBobOffset = 0f;

        if (controller.isGrounded)
        {
            if (isSprinting)
            {
                bobTimer += Time.deltaTime * sprintBobSpeed;
                bobOffset = Mathf.Sin(bobTimer) * bobAmount;
                sideBobOffset = Mathf.Sin(bobTimer * 0.5f) * sprintSideBobAmount;
            }
            else if (!isAiming) // idle bob only if not aiming or sprinting
            {
                bobTimer += Time.deltaTime * idleBobSpeed;
                bobOffset = Mathf.Sin(bobTimer) * idleBobAmount;
            }
            else
            {
                bobTimer = 0f;
            }
        }
        else
        {
            bobTimer = 0f;
        }


        // Recoil
        currentRecoilOffset = Vector3.Lerp(currentRecoilOffset, targetRecoilOffset, Time.deltaTime * recoilReturnSpeed);

        // Final position
        Vector3 finalPos = basePos +
                           cameraTransform.up * bobOffset +
                           cameraTransform.right * sideBobOffset +
                           cameraTransform.forward * currentRecoilOffset.z;

        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);
        Quaternion targetRot = cameraTransform.rotation * Quaternion.Euler(targetRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
    }

    public void TriggerRecoil()
    {
        targetRecoilOffset = new Vector3(0f, 0f, -recoilAmount);
        Invoke(nameof(ResetRecoil), 0.02f);
    }

    private void ResetRecoil()
    {
        targetRecoilOffset = Vector3.zero;
    }

    public void ResetArmPosition()
    {
        transform.localPosition = defaultLocalPosition;
        transform.localRotation = Quaternion.Euler(defaultLocalRotation);
    }

}
