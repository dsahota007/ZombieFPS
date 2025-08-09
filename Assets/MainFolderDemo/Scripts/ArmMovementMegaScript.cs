using UnityEngine;
using System.Collections;
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
    
    public bool isReloading = false;   //we change to public so we can access this in mysterybox logic

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

    [Header("Equip Animation")]
    public Vector3 equipOffset = new Vector3(0f, -0.8f, 0f); // tweak this if needed
    public float equipAnimationSpeed = 3f;

    private bool isEquipping = false;
    private float equipTimer = 0f;

    [Header("Quick Grenade Grab")]
    public Transform leftArm; // Assign your left arm transform
    public Vector3 grabLocalPos = new Vector3(-0.05f, -0.15f, 0f); // final grab pos
    public Vector3 grabLocalEuler = new Vector3(15f, -15f, -5f);
    public Vector3 dropArmPos = new Vector3(-1f, 0f, 0f);
    public float dropTime = 0.15f; // time to drop
    public float grabTime = 0.15f; // time to go from drop to grab
    public float returnTime = 0.2f; // time to return to default

    private Vector3 leftDefaultPos;
    private Quaternion leftDefaultRot;
    private bool isGrenadeGrabPlaying = false;

    [Header("Grenade Spawn")]
    public GameObject grenadePrefab;   // Assign in Inspector
    public Transform grenadeSpawn;     // Child transform on hand/camera
    public float throwForce = 14f;     // Speed forward


    //----------------------------

    private bool isCastingSpell = false;

    private Vector3 defaultLocalPosition;
    private Vector3 defaultLocalRotation;

    private float bobTimer;
    private Vector3 swayRotation;

    [HideInInspector] public Vector3 externalKickbackOffset = Vector3.zero;
    private PlayerMovement pm;


    void Start()
    {

        pm = FindFirstObjectByType<PlayerMovement>(); //fetch scipt 

        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation.eulerAngles;   //local pos but for rotation

        leftDefaultPos = leftArm.localPosition;   //for gernade throw -- find current positon so we can set default variable
        leftDefaultRot = leftArm.localRotation;


    }

    void Update()  //LateUpdate()   -- i got rid of this bc idk
    {
        bool hasMovementInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        bool isAiming = Input.GetMouseButton(1);
        
        bool isSliding = FindFirstObjectByType<PlayerMovement>().IsSliding();                                     //this is for slide hipFire offset. 
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && hasMovementInput && !isAiming && !isSliding && !isCastingSpell;        //we added hasMovementInput so i dont sprint in idle

        bool isGrounded = controller.isGrounded;    //we got ref to char controller so we know when grounded
        bool isWalking = !isSprinting && hasMovementInput && isGrounded;    

        Vector3 targetOffset;
        Vector3 targetRotation;
         


        if (isReloading)
        {
            targetOffset = hipOffset + reloadOffset;
            targetRotation = hipRotation + reloadRotation;
        }
        
        //----
        else if (isSliding)
        {
            targetOffset = hipOffset;
            targetRotation = hipRotation;
        }
        //----

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

        // Equip Drop Animation
        if (isEquipping)
        {
            equipTimer += Time.deltaTime * equipAnimationSpeed;

            float dropProgress = Mathf.PingPong(equipTimer, 1f);  // Goes down, then back up
            Vector3 dropOffset = Vector3.Lerp(Vector3.zero, equipOffset, dropProgress);

            targetOffset += dropOffset;

            if (equipTimer >= 2f) // total duration = 2 seconds
            {
                isEquipping = false;
            }
        }


        // Bobbing logic
        float verticalBob = 0f;
        float sideBob = 0f;

        if (isGrounded && !isAiming && !isSliding)
        {
            if (isSprinting)
            {
                bobTimer += Time.deltaTime * sprintBobSpeed;          
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


        //gernade throw logic
        if (!isGrenadeGrabPlaying && Input.GetKeyDown(KeyCode.G) && leftArm != null)
        {
            if (CanThrowGrenade())
                StartCoroutine(ThrowGernadeAnimation());

        }

    }

    public void ResetArmPosition()
    {
        transform.localPosition = defaultLocalPosition;
        transform.localRotation = Quaternion.Euler(defaultLocalRotation);
    }

    public void PlayEquipAnimation()
    {
        isEquipping = true;
        equipTimer = 0f;
    }

    public void SetCastingState(bool casting)   //this is for magic casting
    {
        isCastingSpell = casting;
    }


    public void ReloadOffset(bool state)
    {
        isReloading = state;               //if u look up where reloading is happening we actually start animation. ^^^
    }

    public bool IsGrenadeAnimating => isGrenadeGrabPlaying;


    bool CanThrowGrenade()      //make sure u cant run aim and all that when throwing gernade
    {
        // aiming?
        if (Input.GetMouseButton(1)) return false;
        if (Input.GetKeyDown(KeyCode.LeftShift)) return false;

        // casting magic?
        if (isCastingSpell) return false;

        // reloading? (either this arm state or weapon’s own)
        if (isReloading) return false;
        var w = WeaponManager.ActiveWeapon;
        if (w != null && w.IsReloading) return false;

        bool sprintingNow =
            (pm != null && pm.IsSprinting()) ||
            (Input.GetKey(KeyCode.LeftShift) && controller != null && controller.velocity.magnitude > 0.1f);

        if (sprintingNow) return false;

        return true;            // good to go
    }

    public IEnumerator ThrowGernadeAnimation()
    {
        isGrenadeGrabPlaying = true;

        // 1. Drop down
        Vector3 dropPos = leftDefaultPos + dropArmPos;   //This gives us the first target position where the arm dips down slightly before grabbing (vector on vector)
        float t = 0f;               //start a timer from 0 
        while (t < 1f)              //when its reaches over one
        {
            t += Time.deltaTime / dropTime;                                     //increases it over dropTime seconds.
            leftArm.localPosition = Vector3.Lerp(leftDefaultPos, dropPos, t);   //how fast we want to get their by t using linear interpolation
            yield return null;    //runs every frame
        }

        // 2. Move to grab spot
        t = 0f;         //start at 0
        while (t < 1f)   //when its reaches over one
        {
            t += Time.deltaTime / grabTime;  //increases it over grabtiem so how long are we gonna be holding onto before we lift our hands.
            leftArm.localPosition = Vector3.Lerp(dropPos, grabLocalPos, t);  //we go to grab position adn the time
            leftArm.localRotation = Quaternion.Slerp(leftDefaultRot, Quaternion.Euler(grabLocalEuler), t);  //and also the rotation which is whatever
            yield return null;   //runs every frame
        }

        ThrowGrenadeNow();  //throwing gernade here


        // 3. Return to default
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / returnTime;           //How long till you return to default
            leftArm.localPosition = Vector3.Lerp(grabLocalPos, leftDefaultPos, t);      //we go from grab back to default based on return time we have set
            leftArm.localRotation = Quaternion.Slerp(Quaternion.Euler(grabLocalEuler), leftDefaultRot, t);  //as well as the rotation
            yield return null;  //play every frame
        }

        isGrenadeGrabPlaying = false;       //and trigger this off.
    }
    void ThrowGrenadeNow()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();
        if (gm == null || grenadeSpawn == null) return;

        GameObject prefab = gm.GetCurrentPrefab();
        if (prefab == null) return;

        GameObject g = Instantiate(prefab, grenadeSpawn.position, grenadeSpawn.rotation);

        if (g.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = grenadeSpawn.forward + grenadeSpawn.up * 1.5f; // small arc
            rb.linearVelocity = dir.normalized * throwForce;
        }
    }

}

