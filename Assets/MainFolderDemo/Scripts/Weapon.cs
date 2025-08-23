using UnityEngine;
using System.Collections;
//using Unity.Mathematics;

public enum FireType { Single, Burst, Auto }
public enum InfusionType
{
    None,
    Fire,
    Crystal,
    Void,
    Ice,
    Venom,
    Lightning,
    Wind,
    Meteor,
    Crimson,
}

public class Weapon : MonoBehaviour
{
    [Header("Weapon Configuration/Setup")]
    public Transform weaponOffset;
    public Transform magazine;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public string weaponName;

    [Header("Fire Settings")]
    public FireType fireType = FireType.Single; //we start with this i think 
    public float fireRate = 0.1f;
    public float burstDelay = 0.1f;

    [Header("Reload Settings")]
    public float reloadMoveAmount = 0.2f;
    public float reloadDuration = 0.2f;
    public float reloadTime = 1.0f;

    [Header("Ammo")]
    public int clipSize = 30;
    public int maxReserve = 90;

    [Header("Tier 1 Upgrade")]
    public int Tier1clipSize = 40;
    public int Tier1maxReserve = 120;

    [Header("Tier 2 Upgrade")]
    public int Tier2clipSize = 50;
    public int Tier2maxReserve = 150;

    [Header("Tier 3 Upgrade")]
    public int Tier3clipSize = 60;
    public int Tier3maxReserve = 180;

    //[Header("Pack-A-Punch VFX")]
    //public GameObject packVFXPrefab;
    //public Vector3 packVFXOffset = Vector3.zero;
    //public Vector3 packVFXRotation = Vector3.zero;
    //public Vector3 packVFXScale = Vector3.one;
    //[HideInInspector] public bool hasPackVFX = false;


    [Header("Recoil Settings")]
    public float recoilAngle = 4f;
    public float recoilSnappiness = 12f;
    public float recoilReturnSpeed = 6f;

    [Header("Kickback")]
    public float kickbackAmount = 0.05f;
    public float kickbackReturnSpeed = 12f;

    [Header("Muzzle Flash")]
    public GameObject defaultMuzzleFlash;
    public GameObject papMuzzleFlash;
    public float muzzleFlashLifetime = 0.05f;


    [HideInInspector] public Transform leftArm;
    [HideInInspector] public CharacterController controller;

    private int currentAmmo;
    private int ammoReserve;
    private bool isReloading = false;
    private Coroutine fireRoutine;

    private Vector3 initialLeftArmPos;
    private Vector3 initialMagPos;


    // for kickback 
    private float currentRecoil = 0f;
    private float targetRecoil = 0f;
    private Transform cam; // Camera reference for recoil

    // Kickback fields
    private Vector3 currentKickbackOffset = Vector3.zero;

    [Header("VFX")]
    public Vector3 targetKickbackOffset = new Vector3(0.03f, -0.12f, 0f);

    private ArmMovementMegaScript armMover;
    private UI ui;

    private float nextFireTime = 0f; //controls delay for single fire ---

    public bool isWeaponBeingShowcased = false; // for script deleting for UI -- so the gun does not shoot when being displayed. 

    public bool IsReloading => isReloading;          //could delete -------------------------------------

    [Header("Arm Placement")]
    public Vector3 leftHoldPos;
    public Vector3 leftHoldRotEuler;
    public Vector3 rightHoldPos;
    public Vector3 rightHoldRotEuler;

    [Header("Global Weapon/Perk Variables")]
    public static float GlobalReloadSpeedMult = 1f;
    public static float GlobalFireRateMult = 1f;   //new multiplier for double tap concept
    private float ShotDelay => Mathf.Max(0.02f, fireRate / Mathf.Max(0.01f, GlobalFireRateMult));
    private float BurstDelayM => Mathf.Max(0.02f, burstDelay / Mathf.Max(0.01f, GlobalFireRateMult));

    [Header("PAP Upgrades")]
    public float bulletDamage = 10f;
    public int upgradeLevel = 0;
    public int maxUpgradeLevel = 3;

    [Header("Pack-A-Punch Skins")]
    public Renderer gunRenderer;
    public Renderer magazineRenderer;
    public Material papGunMaterial;
    public Material papMagMaterial;
    private bool isSkinned = false;

    [Header("Infusion State")]
    public InfusionType infusion = InfusionType.None;

    //----------------------------------------- IMBUING SYSTTEM ------------------------

    [Header("Infusion VFX & Materials")]
    [Header("Fire Infusion")]
    public GameObject fireInfusionVFXPrefab;
    public Vector3 fireVFXOffset = Vector3.zero;
    public Vector3 fireVFXRotation = Vector3.zero;
    public Vector3 fireVFXScale = Vector3.one;
    public Material fireGunMaterial;
    public Material fireMagMaterial;
    public GameObject fireMuzzleFlash;

    [Header("Crystal Infusion")]
    public GameObject crystalInfusionVFXPrefab;
    public Vector3 crystalVFXOffset = Vector3.zero;
    public Vector3 crystalVFXRotation = Vector3.zero;
    public Vector3 crystalVFXScale = Vector3.one;
    public Material crystalGunMaterial;
    public Material crystalMagMaterial;
    public GameObject crystalMuzzleFlash;

    [Header("Void Infusion")]
    public GameObject voidInfusionVFXPrefab;
    public Vector3 voidVFXOffset = Vector3.zero;
    public Vector3 voidVFXRotation = Vector3.zero;
    public Vector3 voidVFXScale = Vector3.one;
    public Material voidGunMaterial;
    public Material voidMagMaterial;
    public GameObject voidMuzzleFlash;

    [Header("Ice Infusion")]
    public GameObject iceInfusionVFXPrefab;
    public Vector3 iceVFXOffset = Vector3.zero;
    public Vector3 iceVFXRotation = Vector3.zero;
    public Vector3 iceVFXScale = Vector3.one;
    public Material iceGunMaterial;
    public Material iceMagMaterial;
    public GameObject iceMuzzleFlash;

    [Header("Venom Infusion")]
    public GameObject venomInfusionVFXPrefab;
    public Vector3 venomVFXOffset = Vector3.zero;
    public Vector3 venomVFXRotation = Vector3.zero;
    public Vector3 venomVFXScale = Vector3.one;
    public Material venomGunMaterial;
    public Material venomMagMaterial;
    public GameObject venomMuzzleFlash;

    [Header("Lightning Infusion")]
    public GameObject lightningInfusionVFXPrefab;
    public Vector3 lightningVFXOffset = Vector3.zero;
    public Vector3 lightningVFXRotation = Vector3.zero;
    public Vector3 lightningVFXScale = Vector3.one;
    public Material lightningGunMaterial;
    public Material lightningMagMaterial;
    public GameObject lightningMuzzleFlash;

    [Header("Wind Infusion")]
    public GameObject windInfusionVFXPrefab;
    public Vector3 windVFXOffset = Vector3.zero;
    public Vector3 windVFXRotation = Vector3.zero;
    public Vector3 windVFXScale = Vector3.one;
    public Material windGunMaterial;
    public Material windMagMaterial;
    public GameObject windMuzzleFlash;

    [Header("Meteor Infusion")]
    public GameObject meteorInfusionVFXPrefab;
    public Vector3 meteorVFXOffset = Vector3.zero;
    public Vector3 meteorVFXRotation = Vector3.zero;
    public Vector3 meteorVFXScale = Vector3.one;
    public Material meteorGunMaterial;
    public Material meteorMagMaterial;
    public GameObject meteorMuzzleFlash;

    [Header("Crimson Infusion")]
    public GameObject crimsonInfusionVFXPrefab;
    public Vector3 crimsonVFXOffset = Vector3.zero;
    public Vector3 crimsonVFXRotation = Vector3.zero;
    public Vector3 crimsonVFXScale = Vector3.one;
    public Material crimsonGunMaterial;
    public Material crimsonMagMaterial;
    public GameObject crimsonMuzzleFlash;

    //----------- Infusion tracking variables
    [HideInInspector] public bool hasInfusionVFX = false;
    [HideInInspector] public bool hasInfusionSkin = false;
    private GameObject currentInfusionVFX;
    [HideInInspector] public string infusedElement = "";


    [Header("Infusion Bullet Logic")]
    [Header("Fire Infusion VFX")]
    public GameObject fireOnEnemyVFXPrefab;   // VFX that sticks to enemies you burn
    public float fireDotPercentPerSec = 0.03f; // 3% per second
    public float fireDotDuration = 4f;         // seconds
    [HideInInspector] public GameObject enemyInfusionVFX;   // make bullets know which VFX to stick on enemies for this infusion
    public Vector3 fireOnEnemyVFXOffset = Vector3.zero;
    public Vector3 fireOnEnemyVFXEuler = Vector3.zero;
    public Vector3 fireOnEnemyVFXScale = Vector3.one;

    [Header("Venom On-Enemy VFX")]
    public GameObject venomOnEnemyVFXPrefab;
    public float venomDotPercentPerSec = 0.03f;  
    public float venomDotDuration = 4f;
    public Vector3 venomOnEnemyVFXOffset = Vector3.zero;
    public Vector3 venomOnEnemyVFXEuler = Vector3.zero;
    public Vector3 venomOnEnemyVFXScale = Vector3.one;


    public void ApplyPackAPunchSkin()
    {
        if (isSkinned) return;

        if (!hasInfusionSkin)   // Only apply PAP skin if no infusion skin is active (infusion has priority)
        {
            if (gunRenderer != null && papGunMaterial != null)
            {
                gunRenderer.material = papGunMaterial;
            }

            if (magazineRenderer != null && papMagMaterial != null)
            {
                magazineRenderer.material = papMagMaterial;
            }
        }

        isSkinned = true;
    }


    void Start()
    {
        currentAmmo = clipSize;   //we spawn inital ammo
        ammoReserve = maxReserve;

        if (leftArm != null)
            initialLeftArmPos = leftArm.localPosition;     //so initialLeftArmPos stores OG position bc of .localPositon
        if (magazine != null)
            initialMagPos = magazine.localPosition;

        cam = Camera.main.transform;          // Grab camera
        armMover = FindFirstObjectByType<ArmMovementMegaScript>();    // we gonna use this for kickback 
        ui = FindFirstObjectByType<UI>();   //fetch to not shoot while in grenade menu
    }

    void Update()
    {
        if (isReloading)    //if u sprint or reload no shooting 
        {
            StopFiring();
            return;             //stop shooting and exit this part of code -- also causes the clip to go to 0 for some reaosn if u reload (doesnt matter) 
        }

        switch (fireType)
        {
            case FireType.Single:
                if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + ShotDelay; //fireRate;  // current time + next time u can shoot 
                }
                break;

            case FireType.Burst:
                if (Input.GetMouseButtonDown(0) && fireRoutine == null)
                    fireRoutine = StartCoroutine(BurstFire());   //we cant jus call we need startCorotine bc of IEnumerator
                break;

            case FireType.Auto:
                if (Input.GetMouseButton(0) && fireRoutine == null)
                    fireRoutine = StartCoroutine(AutoFire());
                break;
        }

        // recoil logic -- linear interpolation math.lerp (a, b, t) --> smoothly transition from a to b and than time 
        targetRecoil = Mathf.Lerp(targetRecoil, 0f, recoilReturnSpeed * Time.deltaTime);
        currentRecoil = Mathf.Lerp(currentRecoil, targetRecoil, recoilSnappiness * Time.deltaTime);

        if (cam != null)   //did we asign cam? if no skip to avoid errors    
        {           //Quaternion.Euler(x, y, z) returns a rotation --- (up down, left right, roll - tilt)
            cam.localRotation *= Quaternion.Euler(-currentRecoil, 0f, 0f);    //we use *= not += bc rotation must be multiplied not added
        }

        // ---- Kickback logic (additive only) ----

        currentKickbackOffset = Vector3.Lerp(currentKickbackOffset, targetKickbackOffset, Time.deltaTime * kickbackReturnSpeed);
        if (armMover != null)
            armMover.externalKickbackOffset = currentKickbackOffset;

    }

    private GameObject GetMuzzleFlashPrefab()
    {
        // Priority: Infusion > PAP > Default

        // Check for infusion muzzle flash first (highest priority)
        switch (infusion)
        {
            case InfusionType.Fire:
                if (fireMuzzleFlash != null) return fireMuzzleFlash;
                break;

            case InfusionType.Crystal:
                if (crystalMuzzleFlash != null) return crystalMuzzleFlash;
                break;

            case InfusionType.Void:
                if (voidMuzzleFlash != null) return voidMuzzleFlash;
                break;

            case InfusionType.Ice:
                if (iceMuzzleFlash != null) return iceMuzzleFlash;
                break;

            case InfusionType.Venom:
                if (venomMuzzleFlash != null) return venomMuzzleFlash;
                break;

            case InfusionType.Lightning:
                if (lightningMuzzleFlash != null) return lightningMuzzleFlash;
                break;

            case InfusionType.Wind:
                if (windMuzzleFlash != null) return windMuzzleFlash;
                break;

            case InfusionType.Meteor:
                if (meteorMuzzleFlash != null) return meteorMuzzleFlash;
                break;

            case InfusionType.Crimson:
                if (crimsonMuzzleFlash != null) return crimsonMuzzleFlash;
                break;
        }

        // If no infusion muzzle flash, check PAP (medium priority)
        if (upgradeLevel >= 1 && papMuzzleFlash != null)
            return papMuzzleFlash;

        // Fallback to default (lowest priority)
        return defaultMuzzleFlash;
    }



    public void Shoot()
    {
        if (armMover.DrinkingPerk) return;
        if (ui.IsGrenadePanelOpen) return;      //we cant shoot if ur selecting your grenade.
        if (ui.IsInfusePanelOpen) return;
        if (isWeaponBeingShowcased || !CanShoot() || isReloading || IsSprinting()) return; //leave func if u cant

        var dm = FindFirstObjectByType<DropManager>();  
        if (dm == null || !dm.IsInfiniteAmmo)
            currentAmmo--;      //only reduce ammo if not in infiteAmmo drop

        if (bulletPrefab && firePoint)
        {
            GameObject b = Instantiate(bulletPrefab, firePoint.position + firePoint.forward * 0.2f, firePoint.rotation);     //Instantiate(whatToSpawn, whereToSpawn, whichRotation);
            Bullet bullet = b.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.damage = bulletDamage;   // <-- weapon-specific damage
                bullet.sourceWeapon = this;   // << add this so bullet knows infusion + VFX + DOT params

            }
        }



        //// --- Spawn muzzle flash ---
        GameObject muzzlePrefab = GetMuzzleFlashPrefab();

        if (muzzlePrefab != null && firePoint != null)
        {
            GameObject flash = Instantiate(muzzlePrefab, firePoint.position, firePoint.rotation);
            flash.transform.SetParent(firePoint);
            Destroy(flash, muzzleFlashLifetime);
        }

        ApplyRecoil();
        ApplyRecoil();
        ApplyKickback();
    }

    private void ApplyRecoil()
    {
        float recoilX = UnityEngine.Random.Range(recoilAngle * 0.8f, recoilAngle * 1.2f);   //we have to put UnityEngine.Random bc of some vs bug try to get rid of it (DJ from the past)
        targetRecoil += recoilX; // Add more recoil upwards
    }

    private void ApplyKickback()
    {
        targetKickbackOffset = new Vector3(0f, 0f, -kickbackAmount);
        Invoke(nameof(ResetKickback), 0.03f);                           // Fast reset for punchy feel this is liek delay how long till u call this so every 0.03 this func is calledf
    }

    private void ResetKickback()
    {
        targetKickbackOffset = Vector3.zero;
    }

    IEnumerator BurstFire()
    {
        for (int i = 0; i < 3; i++)                             //we want to loop till 3
        {
            if (!CanShoot() || IsSprinting())
                break;
            Shoot();
            yield return new WaitForSeconds(BurstDelayM);            //parameter to wait HOW LONG
        }
        fireRoutine = null;
    }

    IEnumerator AutoFire()
    {
        while (Input.GetMouseButton(0) && CanShoot() && !IsSprinting())
        {
            Shoot();
            yield return new WaitForSeconds(ShotDelay);  //fireRate
        }
        fireRoutine = null;
    }

    void StopFiring()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }
    }

    public void StartReload()
    {
        if (isReloading || currentAmmo == clipSize || ammoReserve <= 0 || (armMover != null && armMover.IsPerkAnimating))
            return;


        // NEW: block reload while grenade throw anim is playing
        var arms = FindFirstObjectByType<ArmMovementMegaScript>();
        if (arms != null && arms.IsGrenadeAnimating) return;

        ArmMagicSpell magicSpell = FindFirstObjectByType<ArmMagicSpell>();
        if (magicSpell != null && magicSpell.IsCasting())
        {
            return; // Can't reload while casting spell
        }


        StopFiring();

        //we have to add all this BS because of the recoil upwards yank when finish the reload. --- We do this to clear out any leftover recoil or kickback values before the reload animation starts  --- idk why but it works 
        currentRecoil = 0f;
        targetRecoil = 0f;
        currentKickbackOffset = Vector3.zero;
        targetKickbackOffset = Vector3.zero;

        StartCoroutine(PlayReload());
    }

    IEnumerator PlayReload()
    {
        isReloading = true;

        float reloadDur = reloadDuration / Mathf.Max(0.01f, GlobalReloadSpeedMult); // lerp duration
        float waitReload = reloadTime / Mathf.Max(0.01f, GlobalReloadSpeedMult); // mid “swap” wait


        ArmMovementMegaScript armMover = FindFirstObjectByType<ArmMovementMegaScript>();
        if (armMover) armMover.ReloadOffset(true);                           //play reload arm animation

        Vector3 magStart = magazine.localPosition;                          //store position than control hwo much it goes down 
        Vector3 armStart = leftArm.localPosition;
        Vector3 magDown = magStart + Vector3.down * reloadMoveAmount;
        Vector3 armDown = armStart + Vector3.down * reloadMoveAmount;

        //move mag down ----

        float time = 0f;                        //this is like a progress bar
        while (time < 1f)
        {
            time += Time.deltaTime / reloadDur;       // we have time than divide by how long you want to finish
            magazine.localPosition = Vector3.Lerp(magStart, magDown, time);     //(a,b,t)
            leftArm.localPosition = Vector3.Lerp(armStart, armDown, time);
            yield return null;      //means “wait for the next frame” before continuing the coroutine ???
        }

        yield return new WaitForSeconds(waitReload);   //Wait until the reload action is visually done (like the mag swap)

        int needed = clipSize - currentAmmo;
        int toReload = Mathf.Min(needed, ammoReserve);
        currentAmmo += toReload;            //take bullet from reserve put into clip    
        ammoReserve -= toReload;               //take bullets out of your reserve

        //move mag back up ----

        time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / reloadDur;
            magazine.localPosition = Vector3.Lerp(magDown, magStart, time);
            leftArm.localPosition = Vector3.Lerp(armDown, armStart, time);
            yield return null;
        }

        isReloading = false;

        if (armMover) armMover.ReloadOffset(false);             //stop animation
    }

    public void CancelReload()
    {
        if (!isReloading) return;           //if ur not reloading get outta this code

        StopAllCoroutines();             //immediately stops the reload coroutine that was running (the smooth mag/arm animation).
        isReloading = false;

        if (leftArm != null) leftArm.localPosition = initialLeftArmPos;  //instantly snaps the arm back to where it was before the reload started
        if (magazine != null) magazine.localPosition = initialMagPos;

        ArmMovementMegaScript armMover = FindFirstObjectByType<ArmMovementMegaScript>();
        if (armMover) armMover.ReloadOffset(false);
    }

    public void RefillFull()
    {
        // fills mag + reserve fully
        // (fields are private, but we're inside Weapon so it's allowed)
        // you already have: currentAmmo, ammoReserve, clipSize, maxReserve
        currentAmmo = clipSize;
        ammoReserve = maxReserve;
    }

    private bool IsSprinting()
    {
        var movement = FindFirstObjectByType<PlayerMovement>();             //fetch script
        bool isSliding = movement != null && movement.IsSliding();     //find sliding 
        return Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1) && !isSliding;  //retrun true --- when ur trying to sprint and your not trying to aim and ur not sliding. 
    }

    private bool CanShoot()  //check if have ammo
    {
        return currentAmmo > 0;   //we return false 
    }
    public void EnableWeapon()
    {
        gameObject.SetActive(true);
    }

    public void DisableWeapon()
    {
        gameObject.SetActive(false);
    }

    //----------------------------------------- IMBUING SYSTTEM ------------------------

    public void SetInfusedElement(string element)
    {
        infusedElement = element;
        Debug.Log($"[Weapon] Infused with {element} magic.");
    }

    //----------

    public void SetInfusion(InfusionType type)
    {
        infusion = type;
        ApplyInfusionEffects(type); // Apply VFX and materials immediately
        Debug.Log($"[Weapon] Infused with {type}");
    }


    public void ApplyInfusionEffects(InfusionType type)
    {
        // Remove old infusion effects first
        RemoveInfusionEffects();

        switch (type)
        {
            case InfusionType.Fire:
                ApplyFireInfusion();
                break;

            case InfusionType.Crystal:
                ApplyCrystalInfusion();
                break;

            case InfusionType.Void:
                ApplyVoidInfusion();
                break;

            case InfusionType.Ice:
                ApplyIceInfusion();
                break;

            case InfusionType.Venom:
                ApplyVenomInfusion();
                break;

            case InfusionType.Lightning:
                ApplyLightningInfusion();
                break;

            case InfusionType.Wind:
                ApplyWindInfusion();
                break;

            case InfusionType.Meteor:
                ApplyMeteorInfusion();
                break;

            case InfusionType.Crimson:
                ApplyCrimsonInfusion();
                break;

            case InfusionType.None:
                // Already removed above, might need to restore PAP skin
                RestorePAPSkinIfNeeded();
                break;
        }
    }


    private void ApplyFireInfusion()
    {
        // Apply Fire VFX
        if (fireInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(fireInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = fireVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(fireVFXRotation);
            currentInfusionVFX.transform.localScale = fireVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Fire Materials (overrides PAP materials)
        if (gunRenderer != null && fireGunMaterial != null)
        {
            gunRenderer.material = fireGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && fireMagMaterial != null)
        {
            magazineRenderer.material = fireMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Fire infusion effects applied!");

        enemyInfusionVFX = fireOnEnemyVFXPrefab;

    }

    private void ApplyCrystalInfusion()
    {
        // Apply Crystal VFX
        if (crystalInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(crystalInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = crystalVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(crystalVFXRotation);
            currentInfusionVFX.transform.localScale = crystalVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Crystal Materials (overrides PAP materials)
        if (gunRenderer != null && crystalGunMaterial != null)
        {
            gunRenderer.material = crystalGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && crystalMagMaterial != null)
        {
            magazineRenderer.material = crystalMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Crystal infusion effects applied!");
    }

    private void ApplyVoidInfusion()
    {
        // Apply Void VFX
        if (voidInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(voidInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = voidVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(voidVFXRotation);
            currentInfusionVFX.transform.localScale = voidVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Void Materials
        if (gunRenderer != null && voidGunMaterial != null)
        {
            gunRenderer.material = voidGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && voidMagMaterial != null)
        {
            magazineRenderer.material = voidMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Void infusion effects applied!");
    }

    private void ApplyIceInfusion()
    {
        // Apply Ice VFX
        if (iceInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(iceInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = iceVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(iceVFXRotation);
            currentInfusionVFX.transform.localScale = iceVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Ice Materials
        if (gunRenderer != null && iceGunMaterial != null)
        {
            gunRenderer.material = iceGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && iceMagMaterial != null)
        {
            magazineRenderer.material = iceMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Ice infusion effects applied!");
    }

    private void ApplyVenomInfusion()
    {
        // Apply Venom VFX
        if (venomInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(venomInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = venomVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(venomVFXRotation);
            currentInfusionVFX.transform.localScale = venomVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Venom Materials
        if (gunRenderer != null && venomGunMaterial != null)
        {
            gunRenderer.material = venomGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && venomMagMaterial != null)
        {
            magazineRenderer.material = venomMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Venom infusion effects applied!");
    }

    private void ApplyLightningInfusion()
    {
        // Apply Lightning VFX
        if (lightningInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(lightningInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = lightningVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(lightningVFXRotation);
            currentInfusionVFX.transform.localScale = lightningVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Lightning Materials
        if (gunRenderer != null && lightningGunMaterial != null)
        {
            gunRenderer.material = lightningGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && lightningMagMaterial != null)
        {
            magazineRenderer.material = lightningMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Lightning infusion effects applied!");
    }

    private void ApplyWindInfusion()
    {
        // Apply Wind VFX
        if (windInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(windInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = windVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(windVFXRotation);
            currentInfusionVFX.transform.localScale = windVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Wind Materials
        if (gunRenderer != null && windGunMaterial != null)
        {
            gunRenderer.material = windGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && windMagMaterial != null)
        {
            magazineRenderer.material = windMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Wind infusion effects applied!");
    }

    private void ApplyMeteorInfusion()
    {
        // Apply Meteor VFX
        if (meteorInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(meteorInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = meteorVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(meteorVFXRotation);
            currentInfusionVFX.transform.localScale = meteorVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Meteor Materials
        if (gunRenderer != null && meteorGunMaterial != null)
        {
            gunRenderer.material = meteorGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && meteorMagMaterial != null)
        {
            magazineRenderer.material = meteorMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Meteor infusion effects applied!");
    }

    private void ApplyCrimsonInfusion()
    {
        // Apply Crimson VFX
        if (crimsonInfusionVFXPrefab != null)
        {
            currentInfusionVFX = Instantiate(crimsonInfusionVFXPrefab, transform);
            currentInfusionVFX.transform.localPosition = crimsonVFXOffset;
            currentInfusionVFX.transform.localRotation = Quaternion.Euler(crimsonVFXRotation);
            currentInfusionVFX.transform.localScale = crimsonVFXScale;
            hasInfusionVFX = true;
        }

        // Apply Crimson Materials
        if (gunRenderer != null && crimsonGunMaterial != null)
        {
            gunRenderer.material = crimsonGunMaterial;
            hasInfusionSkin = true;
        }

        if (magazineRenderer != null && crimsonMagMaterial != null)
        {
            magazineRenderer.material = crimsonMagMaterial;
            hasInfusionSkin = true;
        }

        Debug.Log("[Weapon] Crimson infusion effects applied!");
    }


    private void RemoveInfusionEffects()
    {
        // Remove VFX
        if (currentInfusionVFX != null)
        {
            Destroy(currentInfusionVFX);
            currentInfusionVFX = null;
        }
        hasInfusionVFX = false;
        hasInfusionSkin = false;
    }

    private void RestorePAPSkinIfNeeded()
    {
        // If we had PAP but removed infusion, restore PAP materials
        if (isSkinned)
        {
            if (gunRenderer != null && papGunMaterial != null)
            {
                gunRenderer.material = papGunMaterial;
            }

            if (magazineRenderer != null && papMagMaterial != null)
            {
                magazineRenderer.material = papMagMaterial;
            }
        }
    }



    //for ui -- getter methods
    public int GetCurrentAmmo() => currentAmmo;
    public int GetAmmoReserve() => ammoReserve;

}