//using Unity.Burst.Intrinsics;
//using Unity.VisualScripting;
//using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;
//using static UnityEditorInternal.ReorderableList;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Run/Jump Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float aimSpeed = 3.5f;

    private float _baseWalkSpeed;
    private float _baseSprintSpeed;


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

    [Header("Slam Attack Settings")]
    public float slamRadius = 5f;
    public float slamDamage = 100f;
    public LayerMask enemyMask;
    public GameObject slamImpactVFX; // blood splat + impact on enemies their VFX
    public GameObject KineticUnderneathSlamImpactVFX;
    public GameObject KineticUnderneathSlamImpactVFX2; //not in use
    public GameObject KineticUnderneathSlamImpactVFX3;
    public GameObject KineticUnderneathSlamImpactVFX4; //not in use
    public GameObject KineticUnderneathSlamImpactVFX5;

    //--------------------------------------------

    private CharacterController controller;
    private ArmMovementMegaScript armMover;
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
        armMover = GetComponent<ArmMovementMegaScript>();
        if (armMover == null) armMover = FindFirstObjectByType<ArmMovementMegaScript>();  //For some reason this allows to nto slide when drinking. 

        // Store normal controller dimensions so we can like reset
        normalControllerHeight = controller.height;
        normalControllerCenter = controller.center;

        _baseWalkSpeed = walkSpeed;             //-- for perk resetting
        _baseSprintSpeed = sprintSpeed;

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
                    ApplyKineticSlamDamage();   //we trigged kineticSlam so we apply this. 

                    if (KineticUnderneathSlamImpactVFX != null)         //this is the shit underneath the player
                    {
                        GameObject vfx1 = Instantiate(KineticUnderneathSlamImpactVFX, transform.position, Quaternion.identity);
                        Destroy(vfx1, 10f);
                        //Instantiate(KineticUnderneathSlamImpactVFX2, transform.position, Quaternion.identity);
                        GameObject vfx2 = Instantiate(KineticUnderneathSlamImpactVFX3, transform.position, Quaternion.identity);
                        Destroy(vfx2, 10f);
                        //Instantiate(KineticUnderneathSlamImpactVFX4, transform.position, Quaternion.identity);
                        GameObject vfx3 = Instantiate(KineticUnderneathSlamImpactVFX5, transform.position, Quaternion.identity);
                        Destroy(vfx3, 10f);
                    }

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

        Vector3 inputDirection = transform.right * x_input + transform.forward * z_input;  

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

        //------------------------------------------------------- Slam logic
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

    //------------------------------------------------------------ Kinetic Slam
    void StartKineticSlam()
    {
        isSlamming = true;
        lastSlamTime = Time.time;
        velocity.y = slamDownForce;

        // Optional FX trigger here
        // e.g. CameraShake.ShakeOnce(), play sound, etc.
    }



    void ApplyKineticSlamDamage()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, slamRadius, enemyMask);   //parameter(center of sphere, radiusOFSphere, a layermask defines which layers of colliders to include in the query)
        // ^^ It checks for all colliders that are on the enemyMask layer These are the enemies within range of the slam.


        foreach (Collider enemy in hitEnemies)  //For each enemy in range, this block will:
        {
            EnemyHealthRagdoll health = enemy.GetComponent<EnemyHealthRagdoll>();  //fetch script
            if (health != null)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized; //we find direction from us teh player to enemy 
                health.TakeDamage(slamDamage, direction);   //in enemyHealthRagdoll script 

                // Apply explosion force to all rigidbodies in the enemy
                Rigidbody[] rbs = enemy.GetComponentsInChildren<Rigidbody>();   //so now we get rigidBodies of every enemy in 
                foreach (Rigidbody rb in rbs) //For each rigidBody in range, this block will:
                {
                    if (rb != null)
                    {
                        float dist = Vector3.Distance(transform.position, rb.transform.position);    //check how far the rigidbody itself and the player is. 
                        float force = Mathf.Lerp(105f, 105f, dist / slamRadius);   // we use linear interpolation so the closer you are the more the damage and force 
                                                                                // If the bone is very close, dist / slamRadius ≈ 0 → force ≈ 45
                                                                                // If the bone is at the edge, dist / slamRadius ≈ 1 → force ≈ 5
                        rb.AddExplosionForce(force, transform.position, slamRadius, 1552.3f, ForceMode.Impulse); // Lower upward lift (how strong, expolision origin, hjow far explosion affects, upward modifer gives the bone vertical lift, ForceMode.Impulse is an instant kick like a punch.)

                    }
                }

                if (slamImpactVFX != null)
                {
                    GameObject deathVFXEnemy = Instantiate(slamImpactVFX, enemy.transform.position + Vector3.up * 1f, Quaternion.identity);
                    Destroy(deathVFXEnemy,5f);
                }
            }
        }
    }


    //------------------------------------------------------- Slideing Logic


    void HandleSlideInput()
    {
        if (armMover != null && armMover.DrinkingPerk)  
        {
            if (isSliding) EndSlide();
            return;
        }

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

    public void IncreaseSpeedFromMoreSpeedPerk(float WalkSpeed, float SprintSpeed)
    {
        walkSpeed = WalkSpeed;
        sprintSpeed = SprintSpeed;
    }

    //----------------------- Getters

    public void ResetSpeedsToBase()   //after death for speed perk
    {
        walkSpeed = _baseWalkSpeed;
        sprintSpeed = _baseSprintSpeed;
    }


    public bool IsGrounded() => isGrounded;
    public bool IsSliding()  // for Cam script so i can reset it 
    {
        return isSliding;
    }

    public float LastSlamTime => lastSlamTime;  // for UI cooldown

    public bool IsSprinting()
    {
        return Input.GetKey(KeyCode.LeftShift) && controller.velocity.magnitude > 0.1f;
    }

}
