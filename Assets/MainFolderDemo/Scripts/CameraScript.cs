using UnityEngine;
using UnityEngine.UI;
 
public class CameraScript : MonoBehaviour
{

    private CharacterController controller;
    private PlayerMovement playerMovement;
    private ArmMovementMegaScript armMover;


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
    public Image hitmarkerImage;           // drag a small X/dot sprite here (UI Image in Canvas)
    public float hitmarkerDuration = 0.08f;
    public float hitmarkerFadeSpeed = 14f;
    public Color hitColor = Color.white;   // normal hit color
    public Color killColor = Color.red;    // kill color
    public float hitmarkerTimer = 0f;
    public float hitmarkerPopScale = 1.3f; // how big it gets on hit
    public float hitmarkerScaleLerp = 12f; // how fast it returns to normal
    public Vector3 hitmarkerDefaultScale;
    public Vector3 hitmarkerTargetScale;
 

    // We’ll add this so our hit effects stack after your normal camera effects
    private Vector3 externalPosOffset = Vector3.zero;

    public bool cameraLocked = false;   //this is for when you open Grenade Menu

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)       //if cam doesnt exist make sure to put fov as default 
        {
            playerCamera.fieldOfView = defaultFOV;
        }

        playerMovement = FindObjectOfType<PlayerMovement>(); //ref to script for turning off bobbing midair 
        armMover = FindFirstObjectByType<ArmMovementMegaScript>();  

        sprintFOV = defaultFOV + 25f;
        defaultCamPos = cam.localPosition;   //we capture og spot of cam

        if (hitmarkerImage != null)
            hitmarkerDefaultScale = hitmarkerImage.rectTransform.localScale;  //get OG size
    }

    void Update()
    {
        if (cameraLocked)   // for grenade menu prompt
            return;

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

        // --- Hitmarker fade ---
        if (hitmarkerImage != null && hitmarkerImage.enabled)
        {
            if (hitmarkerTimer > 0f) hitmarkerTimer -= Time.deltaTime;   // when timer runs out, fade to 0

            float targetA = hitmarkerTimer > 0f ? 1f : 0f;  //when time runs our make it ffully viabkle (1) and after it ends make it transparetn(0)

            var c = hitmarkerImage.color;
            c.a = Mathf.Lerp(c.a, targetA, Time.deltaTime * hitmarkerFadeSpeed);    //we change the alpha -- smoothly increasing or decreasing -- power of the lerp 
            hitmarkerImage.color = c;    //Apply the updated color back to the Image — this is what actually updates what you see.


            // scale back to normal
            hitmarkerImage.rectTransform.localScale = Vector3.Lerp(
                hitmarkerImage.rectTransform.localScale,
                hitmarkerDefaultScale,
                Time.deltaTime * hitmarkerScaleLerp
            );

            if (c.a <= 0.02f) hitmarkerImage.enabled = false;   //Once it’s basically invisible (alpha ~0), disable the Image to stop drawing it until the next hit.
        }

    }

    public void VertClamp()
    {
        if (cameraLocked) return;  //for menu system when u open grenade menu or PAP
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;                            //we can reverse controls for whoever wants it. 
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);          //math.clamp (taregt, min, max)   

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);    // Look up/down  (x,y,z)
        playerBody.Rotate(Vector3.up * mouseX);                     // Rotate player left/right  ----  jhhhh        
    }

    public void FOVTransition()
    {
        if (cameraLocked) return;  //for menu system when u open grenade menu or PAP
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
            crosshairImage.enabled = !isAiming && !isSprinting;

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

 
        if (cameraLocked) return;  //for menu system when u open grenade menu or PAP
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

        // TILT      //we have to write UnityEngine.Random for some reason having issues namespace BS
        float dir = (UnityEngine.Random.value < 0.5f) ? -1f : 1f;  //we randomly get 1 or -1 -- This line picks either -1 or +1 randomly → decides tilt direction (left or right).
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

    public void ShowHitmarker(bool isKill)
    {
        if (hitmarkerImage == null) return;   //dip this code

        hitmarkerTimer = hitmarkerDuration;   //how long it stays

        // set color & make fully visible
        var c = isKill ? killColor : hitColor;   //if is kill is true make hitmarker red if not than jus showcase normal color
        c.a = 1f;           
        hitmarkerImage.color = c;         //make it 1 
        hitmarkerImage.enabled = true;   //make it enabled

        hitmarkerTargetScale = hitmarkerDefaultScale * hitmarkerPopScale;  
        hitmarkerImage.rectTransform.localScale = hitmarkerTargetScale;

    }



}
