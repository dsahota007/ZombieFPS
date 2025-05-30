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

    [Header("Reload Offset")]
    public Vector3 reloadOffset = new Vector3(0f, -0.05f, -0.05f);
    public Vector3 reloadRotation = new Vector3(4f, 0f, 0f);
    private bool isReloading = false;

    [Header("Kickback")]
    public float kickbackAmount = 0.05f;
    public float kickbackReturnSpeed = 10f;

    [Header("Bobbing")]
    public float sprintBobSpeed = 26.26f;
    public float sprintSideBobAmount = 0.26f;
    public float walkBobSpeed = 6f;
    public float walkBobAmount = 0.015f;
    public float idleBobSpeed = 2f;
    public float idleBobAmount = 0.005f;

    [Header("Sway Settings")]
    public float swayAmount = 2.5f;
    public float swaySmoothing = 6f;

    [Header("General")]
    public float smoothSpeed = 8f;

    private Vector3 defaultLocalPosition;
    private Vector3 defaultLocalRotation;

    private float bobTimer;
    private Vector3 currentKickbackOffset = Vector3.zero;
    private Vector3 targetKickbackOffset = Vector3.zero;
    private Vector3 swayRotation;

    void Start()
    {
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation.eulerAngles;
    }

    void LateUpdate()
    {
        bool isAiming = Input.GetMouseButton(1);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isAiming;
        bool isGrounded = controller.isGrounded;

        bool hasMovementInput =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.D);

        bool isWalking = !isSprinting && hasMovementInput && isGrounded;

        Vector3 targetOffset;
        Vector3 targetRotation;

        if (isReloading)
        {
            targetOffset = hipOffset + reloadOffset;
            targetRotation = hipRotation + reloadRotation;
        }
        else if (isSprinting && Input.GetKey(KeyCode.S))
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

        // Bobbing logic
        float verticalBob = 0f;
        float sideBob = 0f;

        if (isGrounded && !isAiming)
        {
            if (isSprinting)
            {
                bobTimer += Time.deltaTime * sprintBobSpeed;
                sideBob = Mathf.Sin(bobTimer * 0.5f) * sprintSideBobAmount;
            }
            else if (isWalking)
            {
                bobTimer += Time.deltaTime * walkBobSpeed;
                verticalBob = Mathf.Sin(bobTimer) * walkBobAmount;
            }
            else
            {
                bobTimer += Time.deltaTime * idleBobSpeed;
                verticalBob = Mathf.Sin(bobTimer) * idleBobAmount;
            }
        }
        else
        {
            bobTimer = 0f;
        }

        // Kickback logic
        currentKickbackOffset = Vector3.Lerp(currentKickbackOffset, targetKickbackOffset, Time.deltaTime * kickbackReturnSpeed);

        // Input sway (disabled when aiming)
        if (!isAiming)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float targetZTilt = -horizontal * swayAmount;
            swayRotation = Vector3.Lerp(swayRotation, new Vector3(0f, 0f, targetZTilt), Time.deltaTime * swaySmoothing);
        }
        else
        {
            swayRotation = Vector3.Lerp(swayRotation, Vector3.zero, Time.deltaTime * swaySmoothing);
        }

        Vector3 basePos = cameraTransform.position + cameraTransform.TransformDirection(targetOffset);
        Vector3 finalPos = basePos +
                           cameraTransform.up * verticalBob +
                           cameraTransform.right * sideBob +
                           cameraTransform.forward * currentKickbackOffset.z;

        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);

        Quaternion baseRot = cameraTransform.rotation * Quaternion.Euler(targetRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, baseRot * Quaternion.Euler(swayRotation), Time.deltaTime * smoothSpeed);
    }

    public void TriggerKickback()
    {
        targetKickbackOffset = new Vector3(0f, 0f, -kickbackAmount);
        Invoke(nameof(ResetKickback), 0.02f);
    }

    private void ResetKickback()
    {
        targetKickbackOffset = Vector3.zero;
    }

    public void ResetArmPosition()
    {
        transform.localPosition = defaultLocalPosition;
        transform.localRotation = Quaternion.Euler(defaultLocalRotation);
    }

    public void ReloadOffset(bool state)
    {
        isReloading = state;
    }
}

