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
    public float fireRate = 0.1f;     // for Auto
    public float burstDelay = 0.1f;   // for Burst

    private bool isFiring = false;

    void Update()
    {
        switch (fireType)
        {
            case FireType.Single:
                if (Input.GetMouseButtonDown(0))
                    Shoot();
                break;

            case FireType.Burst:
                if (Input.GetMouseButtonDown(0) && !isFiring)
                    StartCoroutine(BurstFire());
                break;

            case FireType.Auto:
                if (Input.GetMouseButton(0) && !isFiring)
                    StartCoroutine(AutoFire());
                break;
        }
    }

    public void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position + firePoint.forward * 0.2f, firePoint.rotation);
        }
    }

    private IEnumerator BurstFire()
    {
        isFiring = true;

        int burstCount = 3;
        for (int i = 0; i < burstCount; i++)
        {
            Shoot();
            yield return new WaitForSeconds(burstDelay);
        }

        isFiring = false;
    }

    private IEnumerator AutoFire()
    {
        isFiring = true;

        while (Input.GetMouseButton(0))
        {
            Shoot();
            yield return new WaitForSeconds(fireRate);
        }

        isFiring = false;
    }
}

