using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;     
    public Transform cam;            
    public float mouseSensitivity = 100f;
    public float verticalClamp = 90f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);    // Look up/down
        playerBody.Rotate(Vector3.up * mouseX);                     // Rotate player left/right
    }
}
