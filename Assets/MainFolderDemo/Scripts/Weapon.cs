using UnityEngine;
using System.Collections;

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
    private Vector3 targetKickbackOffset = Vector3.zero;
    private ArmMovementMegaScript armMover;

    private float nextFireTime = 0f; //controls delay for single fire ---
     
    public bool IsReloading = false;     //could delete -------------------------------------

    void Start()
    {
        currentAmmo = clipSize;   //we spawn inital ammo
        ammoReserve = maxReserve;

        if (leftArm != null) 
            initialLeftArmPos = leftArm.localPosition;     //so initialLeftArmPos stores OG position bc of .localPositon
        if (magazine != null) 
            initialMagPos = magazine.localPosition;

        cam = Camera.main.transform;          // Grab camera
        armMover = FindObjectOfType<ArmMovementMegaScript>();    // we gonna use this for kickback 
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
                    nextFireTime = Time.time + fireRate;  // current time + next time u can shoot 
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
        if (!CanShoot() ||isReloading || IsSprinting()) return; //leave func if u cant

        currentAmmo--;

        if (bulletPrefab && firePoint)                           //Instantiate(whatToSpawn, whereToSpawn, whichRotation);
            Instantiate(bulletPrefab, firePoint.position + firePoint.forward * 0.2f, firePoint.rotation);

        ApplyRecoil();
        ApplyKickback();
    }

    private void ApplyRecoil()
    {
        float recoilX = Random.Range(recoilAngle * 0.8f, recoilAngle * 1.2f);
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
            yield return new WaitForSeconds(burstDelay);            //parameter to wait HOW LONG
        }
        fireRoutine = null;
    }

    IEnumerator AutoFire()
    {
        while (Input.GetMouseButton(0) && CanShoot() && !IsSprinting())
        {
            Shoot();
            yield return new WaitForSeconds(fireRate);
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
        if (isReloading || currentAmmo == clipSize || ammoReserve <= 0) return;

        StopFiring();
        StartCoroutine(PlayReload());
    }

    IEnumerator PlayReload()
    {
        isReloading = true;

        ArmMovementMegaScript armMover = FindObjectOfType<ArmMovementMegaScript>();
        if (armMover) armMover.ReloadOffset(true);                           //play reload arm animation

        Vector3 magStart = magazine.localPosition;                          //store position than control hwo much it goes down 
        Vector3 armStart = leftArm.localPosition;
        Vector3 magDown = magStart + Vector3.down * reloadMoveAmount;
        Vector3 armDown = armStart + Vector3.down * reloadMoveAmount;
        
        //move mag down ----

        float time = 0f;                        //this is like a progress bar
        while (time < 1f)
        {
            time += Time.deltaTime / reloadDuration;       // we have time than divide by how long you want to finish
            magazine.localPosition = Vector3.Lerp(magStart, magDown, time);     //(a,b,t)
            leftArm.localPosition = Vector3.Lerp(armStart, armDown, time);
            yield return null;      //means “wait for the next frame” before continuing the coroutine ???
        }

        yield return new WaitForSeconds(reloadTime);   //Wait until the reload action is visually done (like the mag swap)

        int needed = clipSize - currentAmmo;
        int toReload = Mathf.Min(needed, ammoReserve);  
        currentAmmo += toReload;            //take bullet from reserve put into clip    
        ammoReserve -= toReload;               //take bullets out of your reserve
        
        //move mag back up ----

        time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / reloadDuration;
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

        ArmMovementMegaScript armMover = FindObjectOfType<ArmMovementMegaScript>();
        if (armMover) armMover.ReloadOffset(false);                         
    }

    private bool IsSprinting()
    {
        return Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1);
    }

    private bool CanShoot()  //check if have ammo
    {
        return currentAmmo > 0;   //we return false 
    }


    //for ui come back to this !!!!!!!!!!!!!!!!!---------------
    public int GetCurrentAmmo() => currentAmmo;
    public int GetAmmoReserve() => ammoReserve;
}





