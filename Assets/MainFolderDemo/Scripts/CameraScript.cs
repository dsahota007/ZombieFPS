using UnityEngine;
using UnityEngine.UI;


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
    public float adsFOV = 70f;

    [Header("Head Bobbing")]
    public float bobSpeed = 10f;       // How fast the bobbing cycles
    public float bobAmount = 0.05f;    // How high the bobbing goes

    private float bobTimer = 0f;
    private Vector3 defaultCamPos;


    [Header("Slide Camera Effects")]                    // SLIDE MECHANIC
    public float slideCameraDropAmount = 0.5f;
    public float slideCameraTransitionSpeed = 8f;


    [Header("Slide Tilt Settings")]
    public float slideTiltAngle = 8f;
    public float slideTiltSpeed = 6f;

    private float currentTilt = 0f;



    [Header("Hit Feedback")]
    public Image bloodOverlay;            
    public float maxBloodAlpha = 0.65f;   // alpha when near 0 HP
    public float hitFlashExtra = 0.2f;    // short spike on damage
    public float bloodLerpSpeed = 5f;


    [Header("Hit Tilt")]
    public float hitTiltZ = 6f;           // degrees of quick roll on hit
    public float tiltRecoverSpeed = 8f;

    private float hitTargetTiltZ = 0f;
    private float hitCurrentTiltZ = 0f;
    private float bloodTargetAlpha = 0f;
    private float bloodCurrentAlpha = 0f;

    [Header("Hitmarker and Crosshair")]
    public Image crosshairImage;

    // We’ll add this so our hit effects stack after your normal camera effects
    private Vector3 externalPosOffset = Vector3.zero;



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
        defaultCamPos = cam.localPosition;   //we capture og spot of cam
    }

    void Update()
    {
        VertClamp();
        FOVTransition();
        //HeadBobWhenSprint();   // these two are in HandleCameraEffects();
        //HandleSlideCamera();
        HandleCameraEffects();



        // --------------------------------------- camera hit tilt
        hitCurrentTiltZ = Mathf.Lerp(hitCurrentTiltZ, hitTargetTiltZ, Time.deltaTime * tiltRecoverSpeed);  //current --> target adn than the speed 
        hitTargetTiltZ = Mathf.Lerp(hitTargetTiltZ, 0f, Time.deltaTime * tiltRecoverSpeed);

        // --------------------------------------- BLOOD OVERLAY when hit
        if (bloodOverlay != null)
        {
            bloodCurrentAlpha = Mathf.Lerp(bloodCurrentAlpha, bloodTargetAlpha, Time.deltaTime * bloodLerpSpeed);   //current --> target adn than the speed 
            var c = bloodOverlay.color;     // Copy the current color of the overlay
            c.a = Mathf.Clamp01(bloodCurrentAlpha);   // blood overlay toward the bloodTargetAlpha value. Clamp01 ensures alpha stays between 0 and 1.  
                                                      // Set the alpha channel to our updated value (0 = invisible, 1 = fully visible)
            bloodOverlay.color = c;      //is the moment it actually changes what you see on screen.
        }

        // Recalculate base alpha every frame so regen makes overlay fade
        if (bloodOverlay != null)
        {
            PlayerAttributes playerAttr = FindObjectOfType<PlayerAttributes>(); // fetch script
            if (playerAttr != null)     
            {
                float health01 = playerAttr.GetCurrentHealth01();    //get current hellath from the getter method we madee
                float visibleThreshold = 0.8f;    //80% HP we should not be seeing blood after this

                float baseAlpha = 0f;
                if (health01 <= visibleThreshold)     //if helath is 80 or below
                {
                    float t = Mathf.InverseLerp(visibleThreshold, 0f, health01);    
                    baseAlpha = Mathf.Lerp(0f, maxBloodAlpha, t);
                }

                // Blend toward whichever is higher: regen base or hit flash
                float target = Mathf.Max(baseAlpha, bloodTargetAlpha);
                bloodCurrentAlpha = Mathf.Lerp(bloodCurrentAlpha, target, Time.deltaTime * bloodLerpSpeed);

                var c = bloodOverlay.color;
                c.a = Mathf.Clamp01(bloodCurrentAlpha);
                bloodOverlay.color = c;

                // Decay hit flash toward base
                bloodTargetAlpha = Mathf.Lerp(bloodTargetAlpha, baseAlpha, Time.deltaTime * 2f);
            }
        }


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

        bool isAiming = Input.GetMouseButton(1);
        bool hasMovementInput = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;  //we do this so we dont get this when idle
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && hasMovementInput && !isAiming;
        bool isSliding = playerMovement != null && playerMovement.IsSliding();   //this is so u dont get ads FOv zoom when sliding


        float targetFOV;

        if (isAiming && !isSliding)
            targetFOV = adsFOV; // zoom in
        else if (isSprinting)
            targetFOV = sprintFOV; // zoom out a bit when sprinting
        else
            targetFOV = defaultFOV; // normal


        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);     //(a,b,t)

        // Simple crosshair toggle
        if (crosshairImage != null)
            crosshairImage.enabled = !isAiming;

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

    //===================================================== slide
    void HandleSlideCamera()
    {
        Vector3 slidePos = new Vector3(defaultCamPos.x, defaultCamPos.y - slideCameraDropAmount, defaultCamPos.z);  //we get default camPos in start()
        cam.localPosition = Vector3.Lerp(cam.localPosition, slidePos, Time.deltaTime * slideCameraTransitionSpeed);   //lerp (a,b,t) so we get the cam and move it to the slide position and than by the speed of the transition
        currentTilt = Mathf.Lerp(currentTilt, slideTiltAngle, Time.deltaTime * slideTiltSpeed);   //we use lerp again we need to get to slide tilt


        //Quaternion originalYRotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);
        cam.localRotation = Quaternion.Euler(xRotation, cam.localRotation.eulerAngles.y, currentTilt);  //LEARN THIS  111!!!!
        //takes new current tilt in the z coord ---- Quaternion.Euler(x, y, z) --- returns a rotation --- (up down, left right, roll - tilt) 
        //Debug.Log("Cam Y Pos: " + cam.localPosition.y);
    }

    void ReturnCameraToDefault()
    {
        bobTimer = 0f;                  //Resets the head bobbing animation timer to stop any further bob movement.
        cam.localPosition = Vector3.Lerp(cam.localPosition, defaultCamPos, Time.deltaTime * 5f); //resetting came back smoothly
        currentTilt = Mathf.Lerp(currentTilt, 0f, Time.deltaTime * slideTiltSpeed);   //we currently tilted lerp(a,b,t) --- so we have to reset it smoothly 
        cam.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);          //xRotation is 0 so adn with t being currentTitt we change in the line above so were smoothly reset tilt
    }

    void HandleCameraEffects()
    {
        if (!playerMovement.IsGrounded()) //not on ground than reset and GTFO
        {
            ReturnCameraToDefault();
            return;
        }
        if (playerMovement.IsSliding())  //if they are sliding than handleSlideCamera() and than GTFO
        {
            HandleSlideCamera();
            return;
        }
        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0))   // this is for bobbing -- forward back left right -- we take away HeadBobWhenSprint in update()
        {
            HeadBobWhenSprint();
            return;
        }
        ReturnCameraToDefault();   //when you do exit this you still have to reset the camera. 
                                   // ... your existing logic decides cam.localPosition and cam.localRotation

        // Apply position shake
        cam.localPosition += externalPosOffset;

        // Combine your existing tilt (currentTilt) with hit tilt
        var e = cam.localRotation.eulerAngles;
        cam.localRotation = Quaternion.Euler(e.x, e.y, currentTilt + hitCurrentTiltZ);

    }

    // Call this from PlayerAttributes when the player takes damage.
    // health01 should be currentHealth / maxHealth in [0..1]
    public void OnPlayerHit(float damage, float health01)
    {

        // TILT
        float dir = (Random.value < 0.5f) ? -1f : 1f;  //we randomly get 1 or -1 -- This line picks either -1 or +1 randomly → decides tilt direction (left or right).
        hitTargetTiltZ += hitTiltZ * dir;  

        // BLOOD FLASH
        float visibleThreshold = 0.8f; // above 80% health = invisible

        float baseAlpha = 0f;
        if (health01 <= visibleThreshold)
        {
            float t = Mathf.InverseLerp(visibleThreshold, 0f, health01);
            baseAlpha = Mathf.Lerp(0f, maxBloodAlpha, t);
        }

        // Add flash
        bloodTargetAlpha = Mathf.Clamp01(baseAlpha + hitFlashExtra);  //Takes the base alpha from health, adds hitFlashExtra (a small bump in opacity for dramatic hit feedback)

        // If new target alpha is higher than current, snap it up
        if (bloodTargetAlpha > bloodCurrentAlpha)
            bloodCurrentAlpha = bloodTargetAlpha;
    }



}
