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
    [Header("Perk Drink (drop-only)")]
    public Vector3 perkDropOffset = new Vector3(-1f, 0f, 0f);  // how far the left hand dips
    public float perkDropTime = 0.15f;
    public float perkHoldTime = 0.70f;
    public float perkReturnTime = 0.20f;

    public Vector3 perkMidLocalPos = new Vector3(-0.25f, -0.10f, 0.10f); // the “specific point” you want to pass through
    public Vector3 perkMidLocalEuler = new Vector3(0f, 0f, 0f);            // leave 0s if you don’t want extra rotation

    public float perkToMidTime = 0.18f;  // drop → mid travel time
    public float perkMidHoldTime = 0.35f;  // hold at mid before returning

    public bool lockBobbingDuringPerk = true;
    private bool isPerkAnimPlaying = false;
    public bool IsPerkAnimating => isPerkAnimPlaying;


    private bool isCastingSpell = false;    //--magic

    private Vector3 defaultLocalPosition;
    private Vector3 defaultLocalRotation;

    private float bobTimer;
    private Vector3 swayRotation;

    [HideInInspector] public Vector3 externalKickbackOffset = Vector3.zero;
    //[HideInInspector] public Vector3 externalHipfireKickbackOffset = Vector3.zero;   -- we could add somethign like this -- open dc to look at the !isAiming code
    private PlayerMovement pm;

    public Transform rightArm;



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

        bool hasMovementInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        bool isAiming = !isPerkAnimPlaying && Input.GetMouseButton(1);

        bool freezeForPerk = lockBobbingDuringPerk && isPerkAnimPlaying;


        bool isSliding = FindFirstObjectByType<PlayerMovement>().IsSliding();                                     //this is for slide hipFire offset. 
        bool isSprinting = !isPerkAnimPlaying && Input.GetKey(KeyCode.LeftShift) && hasMovementInput && !isAiming && !isSliding && !isCastingSpell;        //we added hasMovementInput so i dont sprint in idle

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

        if (!freezeForPerk && isGrounded && !isAiming && !isSliding)
        {
            if (isSprinting)
            {
                bobTimer += Time.deltaTime * sprintBobSpeed;
                sideBob = Mathf.Sin(bobTimer * 0.5f) * sprintSideBobAmount;
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
            // freeze bobbing while drinking or airborne
            bobTimer = 0f;
            verticalBob = 0f;
            sideBob = 0f;
        }


        // Input sway (disabled when aiming) --- for the gun to turn slighlty 
        if (!isAiming && !freezeForPerk)
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
        //finalPos += transform.forward * externalKickbackOffset.z;     --- old code causing weird aim to the left
        finalPos += cameraTransform.TransformVector(new Vector3(0f, 0f, externalKickbackOffset.z));


        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * smoothSpeed);   //this is for gun to return


        //IDK This is only confusing part is it kickback idk -----------------------------------------------------------------------------
        Quaternion baseRot = cameraTransform.rotation * Quaternion.Euler(targetRotation);                                                     //--------------------------------------------????
        transform.rotation = Quaternion.Slerp(transform.rotation, baseRot * Quaternion.Euler(swayRotation), Time.deltaTime * smoothSpeed);    //--------------------------------------------????

        // --- Per-gun arm placement (only while holding a gun) ---
        var held = WeaponManager.ActiveWeapon;
        if (held != null && !isReloading && !isGrenadeGrabPlaying && !IsPerkAnimating && !isCastingSpell)

        {
            // we only touch the arm bones; all your root offsets/bob/sway stay the same
            float s = smoothSpeed;

            if (leftArm != null)
            {
                leftArm.localPosition = Vector3.Lerp(leftArm.localPosition, held.leftHoldPos, Time.deltaTime * s);
                leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, Quaternion.Euler(held.leftHoldRotEuler), Time.deltaTime * s);
            }
            if (rightArm != null)
            {
                rightArm.localPosition = Vector3.Lerp(rightArm.localPosition, held.rightHoldPos, Time.deltaTime * s);
                rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, Quaternion.Euler(held.rightHoldRotEuler), Time.deltaTime * s);
            }
        }
        // --- end per-gun arm placement ---


        //gernade throw logic
        if (!isGrenadeGrabPlaying && Input.GetKeyDown(KeyCode.G) && leftArm != null)
        {
            if (CanThrowGrenade())
            {
                var gm = FindFirstObjectByType<GrenadeManager>();
                if (gm && gm.CanThrowCurrent())         //checks if we also have a grenade to throw
                {
                    StartCoroutine(ThrowGernadeAnimation());
                }
                else
                {
                    var ui = FindFirstObjectByType<UI>();
                    if (ui) ui.ShowTemporaryMagicMessage("No Grenades Left");
                }
            }
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

        if (isCastingSpell) return false;        // casting magic?
        if (isPerkAnimPlaying) return false;

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


        if (!gm.CanThrowCurrent())
        {
            // optional: play "no grenade" SFX or flash UI here
            return;
        }

        gm.ConsumeOneCurrent();


        GameObject prefab = gm.GetCurrentPrefab();
        if (prefab == null) return;

        GameObject g = Instantiate(prefab, grenadeSpawn.position, grenadeSpawn.rotation);

        if (g.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = grenadeSpawn.forward + grenadeSpawn.up * 1.5f; // small arc
            rb.linearVelocity = dir.normalized * throwForce;
        }
    }

    public IEnumerator PerkDrinkDropOnly()
    {
        if (isPerkAnimPlaying || isGrenadeGrabPlaying || leftArm == null) yield break; //leave if any of these are true

        isPerkAnimPlaying = true;

        // cache defaults captured in Start()
        Vector3 startPos = leftDefaultPos;      //grab start 
        Quaternion startRot = leftDefaultRot;

        Vector3 dropPos = leftDefaultPos + perkDropOffset;   //add new position
        Quaternion dropRot = leftDefaultRot;

        Vector3 midPos = perkMidLocalPos;           //add mid stop for drinking
        Quaternion midRot = Quaternion.Euler(perkMidLocalEuler);

        // 1) Start → Drop
        float t = 0f, dur = Mathf.Max(0.01f, perkDropTime);   //start progress time so if 0 start 1 is done
        while (t < 1f) //when time is less than 1 
        {
            t += Time.deltaTime / dur;      //If you’re around 60 FPS, Time.deltaTime ≈ 1/60 ≈ 0.0167.
            leftArm.localPosition = Vector3.Lerp(startPos, dropPos, t); // t is time yk so this is LERP 
            leftArm.localRotation = Quaternion.Slerp(startRot, dropRot, t);
            yield return null;
        }

        // 2) Hold at Drop
        if (perkHoldTime > 0f) // if its more than 0 seconds hold that shit
            yield return new WaitForSeconds(perkHoldTime);

        // 3) Drop → Mid (the “specific point”)
        t = 0f; dur = Mathf.Max(0.01f, perkToMidTime);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            leftArm.localPosition = Vector3.Lerp(dropPos, midPos, t);   
            leftArm.localRotation = Quaternion.Slerp(dropRot, midRot, t);
            yield return null;
        }

        // 4) Hold at Mid
        if (perkMidHoldTime > 0f)
            yield return new WaitForSeconds(perkMidHoldTime);

        // 5) Mid → Start (return to OG)
        t = 0f; dur = Mathf.Max(0.01f, perkReturnTime);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            leftArm.localPosition = Vector3.Lerp(midPos, startPos, t);
            leftArm.localRotation = Quaternion.Slerp(midRot, startRot, t);
            yield return null;
        }

        isPerkAnimPlaying = false;
    }
    public bool DrinkingPerk => isPerkAnimPlaying;



}