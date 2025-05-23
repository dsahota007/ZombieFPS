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
    public Transform weaponOffset;
    public Transform magazine;
    public Transform firePoint;
    public GameObject bulletPrefab;

    [Header("Fire Settings")]
    public FireType fireType = FireType.Single;
    public float fireRate = 0.1f;
    public float burstDelay = 0.1f;

    [Header("Reload Settings")]
    public float reloadMoveAmount = 0.2f;
    public float reloadDuration = 0.2f;
    public float reloadTime = 1.0f;

    [HideInInspector] public Transform leftArm;
    [HideInInspector] public CharacterController controller;

    private bool isReloading = false;
    private Coroutine fireRoutine;

    public bool IsReloading => isReloading;

    void Update()
    {
        if (isReloading)
        {
            StopFiring();
            return;
        }

        if (IsSprinting())
        {
            if (fireRoutine != null)
            {
                Debug.LogWarning("Sprinting — stopped firing");
                StopFiring();
            }
            return;
        }

        switch (fireType)
        {
            case FireType.Single:
                if (Input.GetMouseButtonDown(0))
                    Shoot();
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
    }

    public void Shoot()
    {
        if (IsSprinting())
        {
            Debug.Log("❌ BLOCKED: Cannot shoot while sprinting (Shift held)");
            return;
        }

        if (bulletPrefab == null || firePoint == null) return;

        Instantiate(bulletPrefab, firePoint.position + firePoint.forward * 0.2f, firePoint.rotation);

        ArmMovementMegaScript armMover = FindObjectOfType<ArmMovementMegaScript>();
        if (armMover != null)
            armMover.TriggerRecoil();
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < 3; i++)
        {
            if (IsSprinting())
            {
                Debug.Log("BurstFire interrupted: sprinting");
                break;
            }

            Shoot();
            yield return new WaitForSeconds(burstDelay);
        }

        fireRoutine = null;
    }

    private IEnumerator AutoFire()
    {
        while (Input.GetMouseButton(0))
        {
            if (IsSprinting())
            {
                Debug.Log("AutoFire interrupted: sprinting");
                break;
            }

            Shoot();
            yield return new WaitForSeconds(fireRate);
        }

        fireRoutine = null;
    }

    private void StopFiring()
    {
        if (fireRoutine != null)
        {
            StopCoroutine(fireRoutine);
            fireRoutine = null;
        }
    }

    public void StartReload()
    {
        if (!isReloading && magazine != null && leftArm != null)
        {
            StopFiring();
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        Vector3 magStart = magazine.localPosition;
        Vector3 armStart = leftArm.localPosition;
        Vector3 magDown = magStart + new Vector3(0f, -reloadMoveAmount, 0f);
        Vector3 armDown = armStart + new Vector3(0f, -reloadMoveAmount, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / reloadDuration;
            magazine.localPosition = Vector3.Lerp(magStart, magDown, t);
            leftArm.localPosition = Vector3.Lerp(armStart, armDown, t);
            yield return null;
        }

        yield return new WaitForSeconds(reloadTime);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / reloadDuration;
            magazine.localPosition = Vector3.Lerp(magDown, magStart, t);
            leftArm.localPosition = Vector3.Lerp(armDown, armStart, t);
            yield return null;
        }

        isReloading = false;
    }

    private bool IsSprinting()
    {
        return Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton(1);

    }
}
