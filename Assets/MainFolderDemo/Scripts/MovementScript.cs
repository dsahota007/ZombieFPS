using Unity.VisualScripting;
using UnityEngine;
using static UnityEditorInternal.ReorderableList;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Run/Jump Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("Slide Settings")]
    public float slideSpeed = 12f;
    public float slideDuration = 1f;
    public float slideDeceleration = 5f;
    public float slideControllerHeight = 1f;   // Height during slide


    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 lastMoveDirection;  //stores last movement direction

    // Slide variables
    private bool isSliding = false;
    private float slideTimer = 0f;
    private Vector3 slideDirection;
    private float normalControllerHeight;
    private Vector3 normalControllerCenter;


    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Store normal controller dimensions so we can like reset
        normalControllerHeight = controller.height;
        normalControllerCenter = controller.center;
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        HandleSlideInput();
        //HandleMovement();   ------   we could divide it up well
        //HandleJump();
        //ApplyGravity();

        // Prevent sprint while firing
        bool isFiring = Input.GetMouseButton(1) && Input.GetMouseButton(1);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isFiring;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        float x_input = Input.GetAxisRaw("Horizontal");
        float z_input = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = transform.right * x_input + transform.forward * z_input;  //------------

        if (isGrounded)
        {
            lastMoveDirection = inputDirection.normalized;  //if you jump right after walking forward, it "remembers" that direction
        }
        else
        {
            // If you aren’t pressing any movement keys and in the air we have that stored last direction and if u move mid air we update it
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

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //grav
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


    }

    void HandleSlideInput()
    {
        bool canSlide = Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C) && isGrounded && !isSliding;

        if (canSlide)
        {
            StartSlide(); }
        if (isSliding)
        {
            UpdateSlide();
        }

    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = 0f;   //Resets the slide timer to start counting from zero

        float x_input = Input.GetAxisRaw("Horizontal");
        float z_input = Input.GetAxisRaw("Vertical");
        slideDirection = (transform.right * x_input + transform.forward * z_input).normalized;   //we get horizontal adn back fourth input and normalized makes the vector lentgth 1 for consisten speed 

        if (slideDirection.magnitude == 0)   //this is sliding adn not moving which is wack -- i take this back
        {
            slideDirection = transform.forward;   //If no movement keys are pressed when slide starts Default to sliding forward
        }

        controller.height = slideControllerHeight;          //!!! COME BACK
        controller.center = new Vector3(normalControllerCenter.x, slideControllerHeight / 2f, normalControllerCenter.z);  //!!! COME BACK
    }

    void UpdateSlide()
    {
        slideTimer += Time.deltaTime;   //how long ive been sliding

        // Calculate slide speed with deceleration
        float currentSlideSpeed = Mathf.Lerp(slideSpeed, walkSpeed, slideTimer / slideDuration);   //Mathf.Lerp(startValue, endValue, t) we go from fast to walkspeed over 0 - 1 with slidetimer over how long the slide it (confusing tbh) 
         
        controller.Move(slideDirection * currentSlideSpeed * Time.deltaTime);  // move(where to go, how fast)

        // End slide when timer expires or player stops holding shift  !! COME BACK SO WE CAN EDIT THIS BEHAVIOUR
        if (slideTimer >= slideDuration) // || !Input.GetKey(KeyCode.LeftShift))
        {
            EndSlide();
        }
    }

    void EndSlide()
    {
        isSliding = false;
        slideTimer = 0f;
        controller.height = normalControllerHeight;        // Restore normal controller dimensions
        controller.center = normalControllerCenter;
    }



    public bool IsGrounded() => isGrounded;
    public bool IsSliding()  // for Cam script so i can reset it 
    {
        return isSliding;
    }


}
