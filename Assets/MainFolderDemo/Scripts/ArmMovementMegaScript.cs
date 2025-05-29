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

    [Header("Recoil")]
    public float recoilAmount = 0.05f;
    public float recoilReturnSpeed = 10f;

    [Header("Bobbing")]
    public float idleBobSpeed = 2f;
    public float idleBobAmount = 0.005f;
    public float sprintBobSpeed = 26.26f;
    public float sprintSideBobAmount = 0.26f;

    public float smoothSpeed = 8f;

    private Vector3 defaultLocalPosition;
    private Vector3 defaultLocalRotation;

    private Vector3 currentRecoilOffset = Vector3.zero;
    private Vector3 targetRecoilOffset = Vector3.zero;
    private float bobTimer = 0f;
    private bool isReloading = false;

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

        Vector3 offset = hipOffset;
        Vector3 rotation = hipRotation;

        if (isReloading)
        {
            offset += reloadOffset;
            rotation += reloadRotation;
        }
        else if (isSprinting && isBackpedaling)
        {
            offset = sprintBackOffset;
            rotation = sprintBackRotation;
        }
        else if (isSprinting)
        {
            offset = sprintOffset;
            rotation = sprintRotation;
        }
        else if (isAiming)
        {
            offset = adsOffset;
            rotation = adsRotation;
        }

        Vector3 basePos = cameraTransform.position + cameraTransform.TransformDirection(offset);

        float bobOffset = 0f;
        float sideBobOffset = 0f;

        if (controller.isGrounded)
        {
            if (isSprinting)
            {
                bobTimer += Time.deltaTime * sprintBobSpeed;
                sideBobOffset = Mathf.Sin(bobTimer * 0.5f) * sprintSideBobAmount;
            }
            else if (!isAiming)
            {
                bobTimer += Time.deltaTime * idleBobSpeed;
                bobOffset = Mathf.Sin(bobTimer) * idleBobAmount;
            }
        }
        else
        {
            bobTimer = 0f;
        }

        currentRecoilOffset = Vector3.Lerp(currentRecoilOffset, targetRecoilOffset, Time.deltaTime * recoilReturnSpeed);

        Vector3 finalPos = basePos +
            cameraTransform.up * bobOffset +
            cameraTransform.right * sideBobOffset +
            cameraTransform.forward * currentRecoilOffset.z;

        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            cameraTransform.rotation * Quaternion.Euler(rotation),
            Time.deltaTime * smoothSpeed
        );
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

    public void ReloadOffset(bool active)
    {
        isReloading = active;
    }

    public void ResetArmPosition()
    {
        transform.localPosition = defaultLocalPosition;
        transform.localRotation = Quaternion.Euler(defaultLocalRotation);
    }
}
