using UnityEngine;

public class CameraScript : MonoBehaviour
{

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

 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)       //if cam doesnt exist make sure to put fov as default 
        {
            playerCamera.fieldOfView = defaultFOV;
        }

        sprintFOV = defaultFOV + 25f; 
    }

    void Update()
    {
        VertClamp();
        HandleFOVKick(); 
    }

    public void VertClamp()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void HandleFOVKick()
    {
        if (playerCamera == null) return;       //if cam dont exist leave this code dont waste your time.

        bool isFiring = Input.GetMouseButton(1) && Input.GetMouseButton(1);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isFiring;

        float targetFOV = isSprinting ? sprintFOV : defaultFOV;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);     //(a,b,t)
    }
}
