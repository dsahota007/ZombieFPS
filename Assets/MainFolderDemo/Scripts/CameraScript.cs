using UnityEngine;

public class CameraScript : MonoBehaviour
{

    private CharacterController controller;
    private PlayerMovement playerMovement;


    [Header("Vertical Clamp")]
    public Transform playerBody;
    public Transform cam;                        // For rotation
    public float mouseSensitivity = 100f;
    public float verticalClamp = 90f;

    private float xRotation = 0f;

    [Header("FOV")]
    public Camera playerCamera;          // For FOV and effects
    public float defaultFOV = 90f;
    private float sprintFOV;
    public float fovTransitionSpeed = 5f;

    [Header("Head Bobbing")]
    public float bobSpeed = 10f;       // How fast the bobbing cycles
    public float bobAmount = 0.05f;    // How high the bobbing goes

    private float bobTimer = 0f;
    private Vector3 defaultCamPos;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)       //if cam doesnt exist make sure to put fov as default 
        {
            playerCamera.fieldOfView = defaultFOV;
        }

        playerMovement = FindObjectOfType<PlayerMovement>(); //ref to script for turning off bobbing midair 

        sprintFOV = defaultFOV + 25f;
        defaultCamPos = cam.localPosition;


    }

    void Update()
    {
        VertClamp();
        FOVTransition();
        HeadBobWhenSprint();



    }

    public void VertClamp()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;                            //we can reverse controls for whoever wants it. 
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);          //math.clamp (taregt, min, max)   

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);    // Look up/down  (x,y,z)
        playerBody.Rotate(Vector3.up * mouseX);                     // Rotate player left/right  ----  jhhhh        
    }

    public void FOVTransition()
        {
            if (playerCamera == null) return;       //if cam dont exist leave this code dont waste your time.

            bool isFiring = Input.GetMouseButton(1) && Input.GetMouseButton(1);
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isFiring;

            float targetFOV = isSprinting ? sprintFOV : defaultFOV;

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);     //(a,b,t)
        }
    void HeadBobWhenSprint()
    {
        if (!playerMovement.IsGrounded()) return;  //dont bob unless grounded

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") != 0)  //sprinting + moving
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float CamBobOffset = Mathf.Sin(bobTimer) * bobAmount;   //mathf.sin gives a wave (like up/down).

            cam.localPosition = new Vector3(defaultCamPos.x, defaultCamPos.y + CamBobOffset, defaultCamPos.z);   // x and z stay the same while y goes up and down by boboffset
        }
        else
        {
            bobTimer = 0f;
            cam.localPosition = Vector3.Lerp(cam.localPosition, defaultCamPos, Time.deltaTime * 5f);  //return cam pose to default in 5f speed (lerp.(a,b,t))
        }
    }

}

