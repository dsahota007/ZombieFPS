using UnityEngine;
using System.Collections;

public enum FireType
{
    Single,
    Burst,
    Auto
}

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

    [Header("Recoil / Kickback")]
    public float kickbackAmount = 0.05f;
    public float cameraRecoilPerShot = 2f;
    public float recoilSmoothSpeed = 10f;
    public float recoilReturnSpeed = 5f;

    [HideInInspector] public Transform leftArm;
    [HideInInspector] public CharacterController controller;

    private int currentAmmo;
    private int ammoReserve;

    private bool isReloading = false;
    private Coroutine fireRoutine;

    private Vector3 initialLeftArmPos;
    private Vector3 initialMagPos;

    private Camera playerCamera;
    private float currentRecoil = 0f;
    private float targetRecoil = 0f;

    public bool IsReloading => isReloading;

    void Start()
    {
        currentAmmo = clipSize;
        ammoReserve = maxReserve;

        if (leftArm != null) initialLeftArmPos = leftArm.localPosition;
        if (magazine != null) initialMagPos = magazine.localPosition;

        playerCamera = Camera.main;
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
                if (Input.GetMouseButtonDown(0)) Shoot();
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

        HandleRecoil();
    }

    public void Shoot()
    {
        if (!CanShoot() || IsSprinting()) return;

        currentAmmo--;

        if (bulletPrefab && firePoint)
            Instantiate(bulletPrefab, firePoint.position + firePoint.forward * 0.2f, firePoint.rotation);

        ArmMovementMegaScript armMover = FindObjectOfType<ArmMovementMegaScript>();
        if (armMover) armMover.TriggerKickback();

        targetRecoil += cameraRecoilPerShot;
    }

    void HandleRecoil()
    {
        if (playerCamera == null) return;

        // Smooth recoil increase
        currentRecoil = Mathf.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilSmoothSpeed);
        playerCamera.transform.localRotation *= Quaternion.Euler(-currentRecoil * Time.deltaTime, 0f, 0f);

        // Gradually return
        targetRecoil = Mathf.Lerp(targetRecoil, 0f, Time.deltaTime * recoilReturnSpeed);
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
