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
 
    public float aimSpeed = 3.5f;


    [Header("Slide Settings")]
    public float slideSpeed = 12f;
    public float slideDuration = 1f;
    public float slideDeceleration = 5f;
    public float slideControllerHeight = 1f;   // Height during slide


    [Header("Kinetic Jump & Slam Settings")]
    public float KineticJumpForce = 12f;      
    public float slamDownForce = -50f; // How fast you fall
    public float slamCooldown = 10f;

    private bool isKineticJump = false;
    private bool isSlamming = false;
    private float lastSlamTime;

    //--------------------------------------------

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
            velocity.y = -2f;  //stay grounded so dont do 0
            if (isKineticJump)
            {
                isKineticJump = false;
                if (isSlamming)
                {
                    isSlamming = false;
                    // Optional: Trigger impact FX here
                }
            }
        }


        HandleSlideInput();
        //HandleMovement();   ------   we could divide it up well
        //HandleJump();
        //ApplyGravity();

        // Prevent sprint while firing
        bool isAiming = Input.GetMouseButton(1);

        bool isFiring = Input.GetMouseButton(1) && Input.GetMouseButton(1);
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !isFiring;

        float currentSpeed;

        if (isAiming)
            currentSpeed = aimSpeed;
        else if (isSprinting)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;

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

        //----- Slam logic
        bool canSlam = Time.time >= lastSlamTime + slamCooldown;   //for cooldown so u dont spam.
        if (isKineticJump && !isGrounded && !isSlamming && Time.time > lastSlamTime)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && canSlam) // make sure ur not on ground and are ALOUD TO SLAM based off the bool above
            {
                StartKineticSlam();
            }
        }


        controller.Move(inputDirection * currentSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (isSliding)
            {

                velocity.y = KineticJumpForce;               // Boosted jump while sliding
                EndSlide();
                isKineticJump = true;
            }
            else
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        //grav
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);


    }

    void StartKineticSlam()
    {
        isSlamming = true;
        lastSlamTime = Time.time;
        velocity.y = slamDownForce;

        // Optional FX trigger here
        // e.g. CameraShake.ShakeOnce(), play sound, etc.
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
