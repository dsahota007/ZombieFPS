using UnityEngine;
using System.Collections;

public class ArmMagicSpell : MonoBehaviour
{
    [Header("Arm Position Setting")]
    public float raiseDuration = 0.3f;
    public float holdDuration = 0.5f;
    public float returnDuration = 0.3f;

    public Vector3 raiseOffset = new Vector3(0f, 1.0f, 1.0f);
    public Vector3 raiseRotation = new Vector3(-45f, 0f, 0f);

    private Vector3 originalPos;
    private Vector3 originalRot;
    private bool isCasting = false;

    private PlayerMovement playerMovement;
    private Weapon currentWeapon;
    private ArmMovementMegaScript armMover;

    [Header("Spell VFX")]
    public Transform vfxAttachPoint;

    [Header("Fireball Attack")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public GameObject armFireVFX;

    void Start()
    {
        originalPos = transform.localPosition;
        originalRot = transform.localRotation.eulerAngles;

        playerMovement = FindFirstObjectByType<PlayerMovement>();
        armMover = FindFirstObjectByType<ArmMovementMegaScript>();

        if (fireballPrefab == null) Debug.LogWarning("ArmMagicSpell: fireballPrefab is not assigned!");
        if (firePoint == null) Debug.LogWarning("ArmMagicSpell: firePoint is not assigned!");
    }

    void Update()
    {
        currentWeapon = WeaponManager.ActiveWeapon;

        if (Input.GetKeyDown(KeyCode.Q) && CanCastSpell())
        {
            StartCoroutine(CastMagicAnimation());
        }
    }

    bool CanCastSpell()
    {
        if (isCasting) return false;
        if (Input.GetKey(KeyCode.R)) return false;
        if (currentWeapon != null && currentWeapon.IsReloading) return false;
        return true;
    }

    IEnumerator CastMagicAnimation()
    {
        isCasting = true;

        if (armMover != null)
            armMover.SetCastingState(true);

        Vector3 targetPos = originalPos + raiseOffset;
        Quaternion targetRot = Quaternion.Euler(originalRot + raiseRotation);

        // 🔥 Spawn fire effect on hand
        GameObject spawnedVFX = null;
        if (armFireVFX != null && vfxAttachPoint != null)
        {
            spawnedVFX = Instantiate(armFireVFX, vfxAttachPoint.position, vfxAttachPoint.rotation, vfxAttachPoint);
        }

        // Raise arm
        yield return LerpTransform(transform, originalPos, targetPos, Quaternion.Euler(originalRot), targetRot, raiseDuration);

        // Hold
        yield return new WaitForSeconds(holdDuration * 0.5f);

        // Fireball spawn
        if (fireballPrefab != null && firePoint != null)
        {
            Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
        }

        yield return new WaitForSeconds(holdDuration * 0.5f);

        // Return arm
        yield return LerpTransform(transform, targetPos, originalPos, targetRot, Quaternion.Euler(originalRot), returnDuration);

        // Clean up VFX
        if (spawnedVFX != null)
            Destroy(spawnedVFX);

        isCasting = false;

        if (armMover != null)
            armMover.SetCastingState(false);
    }

    IEnumerator LerpTransform(Transform t, Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration)
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            t.localPosition = Vector3.Lerp(fromPos, toPos, time);
            t.localRotation = Quaternion.Slerp(fromRot, toRot, time);
            yield return null;
        }
    }

    public bool IsCasting() => isCasting;
}
