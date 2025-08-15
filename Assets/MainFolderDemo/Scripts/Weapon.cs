using UnityEngine;
using System.Collections;
//using Unity.Mathematics;

public enum FireType { Single, Burst, Auto }

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

    [Header("Recoil Settings")]
    public float recoilAngle = 4f;
    public float recoilSnappiness = 12f;
    public float recoilReturnSpeed = 6f;

    [Header("Kickback")]
    public float kickbackAmount = 0.05f;
    public float kickbackReturnSpeed = 12f;

    [Header("VFX")]
    public GameObject muzzleFlashPrefab;
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


    [Header("Global Weapon/Perk Variables")]
    public static float GlobalReloadSpeedMult = 1f;
    public static float GlobalFireRateMult = 1f;   //new multiplier for double tap concept
    private float ShotDelay => Mathf.Max(0.02f, fireRate / Mathf.Max(0.01f, GlobalFireRateMult));
    private float BurstDelayM => Mathf.Max(0.02f, burstDelay / Mathf.Max(0.01f, GlobalFireRateMult));


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

    public void Shoot()
    {
        if (armMover.DrinkingPerk) return;

        if (ui.IsGrenadePanelOpen) return;      //we cant shoot if ur selecting your grenade.
        if (isWeaponBeingShowcased || !CanShoot() || isReloading || IsSprinting()) return; //leave func if u cant

        currentAmmo--;

        if (bulletPrefab && firePoint)                           //Instantiate(whatToSpawn, whereToSpawn, whichRotation);
            Instantiate(bulletPrefab, firePoint.position + firePoint.forward * 0.2f, firePoint.rotation);

        //// --- Spawn muzzle flash ---
        if (muzzleFlashPrefab != null && firePoint != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            flash.transform.SetParent(firePoint); // so it moves with the gun for that frame
            Destroy(flash, muzzleFlashLifetime);  // clean up automatically
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


    //for ui -- getter methods
    public int GetCurrentAmmo() => currentAmmo;
    public int GetAmmoReserve() => ammoReserve;

}