using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Vector3 lastMoveDirection;  //for float movement continuation

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Prevent sprint while firing
        bool isFiring = Input.GetMouseButton(1) && Input.GetMouseButton(1);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isFiring;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = transform.right * x + transform.forward * z;

        if (isGrounded)
        {
            // If grounded, use current input and save it
            lastMoveDirection = inputDirection.normalized;
        }
        else
        {
            // If airborne and no input, continue last direction
            if (inputDirection.magnitude == 0)
            {
                inputDirection = lastMoveDirection;
            }
            else
            {
                // If player provides new input in air, update last direction
                lastMoveDirection = inputDirection.normalized;
            }
        }

        controller.Move(inputDirection * currentSpeed * Time.deltaTime);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
