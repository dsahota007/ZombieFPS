using UnityEngine;
using System.Collections;

public enum FireType { Single, Burst, Auto }

public class Weapon : MonoBehaviour
{
    [Header("Setup")]
    public Transform weaponOffset;
    public Transform magazine;
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Info")]
    public string weaponName;

    [Header("Fire Settings")]
    public FireType fireType = FireType.Single;
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
    public float kickbackAmount = 0.05f;     // How much to move the gun/arms back per shot
    public float kickbackReturnSpeed = 12f;  // How quickly it returns to original position

    [HideInInspector] public Transform leftArm;
    [HideInInspector] public CharacterController controller;

    private int currentAmmo;
    private int ammoReserve;
    private bool isReloading = false;
    private Coroutine fireRoutine;

    private Vector3 initialLeftArmPos;
    private Vector3 initialMagPos;

    private float currentRecoil = 0f;
    private float targetRecoil = 0f;
    private Transform cam; // Camera reference for recoil

    // Kickback fields
    private Vector3 currentKickbackOffset = Vector3.zero;
    private Vector3 targetKickbackOffset = Vector3.zero;
    private ArmMovementMegaScript armMover;

    private float nextFireTime = 0f; // --- NEW: controls delay for single fire ---

    public bool IsReloading => isReloading;

    void Start()
    {
        currentAmmo = clipSize;
        ammoReserve = maxReserve;

        if (leftArm != null) initialLeftArmPos = leftArm.localPosition;
        if (magazine != null) initialMagPos = magazine.localPosition;

        // Grab camera
        cam = Camera.main.transform;

        // Reference for arm movement (kickback)
        armMover = FindObjectOfType<ArmMovementMegaScript>();
    }

    void Update()
    {
        if (isReloading || IsSprinting())
        {
            StopFiring();
            return;
        }

        switch (fireType)
        {
            case FireType.Single:
                if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + fireRate;
                }
                break;

            case FireType.Burst:
                if (Input.GetMouseButtonDown(0) && fireRoutine == null)
                    fireRoutine = StartCoroutine(BurstFire());
                break;

            case FireType.Auto:
                if (Input.GetMouseButton(0) && fireRoutine == null)
                    fireRoutine = StartCoroutine(AutoFire());
                break;
        }

        // ---- Recoil logic (additive to MouseLook) ----
        targetRecoil = Mathf.Lerp(targetRecoil, 0f, recoilReturnSpeed * Time.deltaTime);
        currentRecoil = Mathf.Lerp(currentRecoil, targetRecoil, recoilSnappiness * Time.deltaTime);

        if (cam != null)
        {
            cam.localRotation *= Quaternion.Euler(-currentRecoil, 0f, 0f);
        }

        // ---- Kickback logic (additive only) ----
        currentKickbackOffset = Vector3.Lerp(currentKickbackOffset, targetKickbackOffset, Time.deltaTime * kickbackReturnSpeed);
        if (armMover != null)
            armMover.externalKickbackOffset = currentKickbackOffset;
    }

    public void Shoot()
    {
        if (!CanShoot() || IsSprinting()) return;

        currentAmmo--;

        if (bulletPrefab && firePoint)
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
        Invoke(nameof(ResetKickback), 0.03f); // Fast reset for punchy feel
    }

    private void ResetKickback()
    {
        targetKickbackOffset = Vector3.zero;
    }

    IEnumerator BurstFire()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!CanShoot() || IsSprinting()) break;
            Shoot();
            yield return new WaitForSeconds(burstDelay);
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
        StartCoroutine(Reload());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        ArmMovementMegaScript armMover = FindObjectOfType<ArmMovementMegaScript>();
        if (armMover) armMover.ReloadOffset(true);

        Vector3 magStart = magazine.localPosition;
        Vector3 armStart = leftArm.localPosition;
        Vector3 magDown = magStart + Vector3.down * reloadMoveAmount;
        Vector3 armDown = armStart + Vector3.down * reloadMoveAmount;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / reloadDuration;
            magazine.localPosition = Vector3.Lerp(magStart, magDown, t);
            leftArm.localPosition = Vector3.Lerp(armStart, armDown, t);
            yield return null;
        }

        yield return new WaitForSeconds(reloadTime);

        int needed = clipSize - currentAmmo;
        int toReload = Mathf.Min(needed, ammoReserve);
        currentAmmo += toReload;
        ammoReserve -= toReload;

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / reloadDuration;
            magazine.localPosition = Vector3.Lerp(magDown, magStart, t);
            leftArm.localPosition = Vector3.Lerp(armDown, armStart, t);
            yield return null;
        }

        isReloading = false;

        if (armMover) armMover.ReloadOffset(false);
    }

    public void CancelReload()
    {
        if (!isReloading) return;

        StopAllCoroutines();
        isReloading = false;

        if (leftArm != null) leftArm.localPosition = initialLeftArmPos;
        if (magazine != null) magazine.localPosition = initialMagPos;

        ArmMovementMegaScript armMover = FindObjectOfType<ArmMovementMegaScript>();
        if (armMover) armMover.ReloadOffset(false);
    }

    private bool IsSprinting()
    {
        return Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1);
    }

    private bool CanShoot()
    {
        return currentAmmo > 0;
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetAmmoReserve() => ammoReserve;
}





