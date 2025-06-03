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
    private Vector3 swayRotation;

    [HideInInspector] public Vector3 externalKickbackOffset = Vector3.zero;

    void Start()
    {
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation.eulerAngles;   //local pos but for rotation
    }

    void Update()  //LateUpdate()   -- i got rid of this bc idk
    {
        bool isAiming = Input.GetMouseButton(1);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isAiming;
        bool isGrounded = controller.isGrounded;    //we got ref to char controller so we know when grounded

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
        else if (isSprinting && Input.GetKey(KeyCode.S))   //back sprint - omni movement
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
                bobTimer += Time.deltaTime * sprintBobSpeed;          //
                sideBob = Mathf.Sin(bobTimer * 0.5f) * sprintSideBobAmount;             //makes wave pattern and than how much side to side (the 0.5 slows down for smoother)
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
            bobTimer = 0f;    //no bob if ur airborne
        }

        // Input sway (disabled when aiming) --- for the gun to turn slighlty 
        if (!isAiming)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float targetZTilt = -horizontal * swayAmount;    //the more the amount the more the tilt
            swayRotation = Vector3.Lerp(swayRotation, new Vector3(0f, 0f, targetZTilt), Time.deltaTime * swaySmoothing);         //vec3.lerp (a,b,t as in how fsat/smooth)  pre much math.lerp
        }
        else
        {
            swayRotation = Vector3.Lerp(swayRotation, Vector3.zero, Time.deltaTime * swaySmoothing);        //sway back to zero. when aiming
        }

        // left right up down bobbing   -- all there is
        Vector3 basePos = cameraTransform.position + cameraTransform.TransformDirection(targetOffset);
        Vector3 finalPos = basePos +
                           cameraTransform.up * verticalBob +
                           cameraTransform.right * sideBob;

        //kickback
        finalPos += transform.forward * externalKickbackOffset.z;    //how much we pish back this is in weapon.cs

        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);   //this is for gun to return


        //IDK This is only confusing part is it kickback idk -----------------------------------------------------------------------------
        Quaternion baseRot = cameraTransform.rotation * Quaternion.Euler(targetRotation);                                                     //--------------------------------------------????
        transform.rotation = Quaternion.Slerp(transform.rotation, baseRot * Quaternion.Euler(swayRotation), Time.deltaTime * smoothSpeed);    //--------------------------------------------????
    }

    public void ResetArmPosition()
    {
        transform.localPosition = defaultLocalPosition;
        transform.localRotation = Quaternion.Euler(defaultLocalRotation);
    }

    public void ReloadOffset(bool state)
    {
        isReloading = state;               //if u look up where reloading is happening we actually start animation. ^^^
    }
}
