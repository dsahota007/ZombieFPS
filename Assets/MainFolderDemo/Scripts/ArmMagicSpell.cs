using UnityEngine;
using System.Collections;

public class ArmMagicSpell : MonoBehaviour
{
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

    void Start()
    {
        originalPos = transform.localPosition;
        originalRot = transform.localRotation.eulerAngles;

        playerMovement = FindFirstObjectByType<PlayerMovement>();
        armMover = FindFirstObjectByType<ArmMovementMegaScript>();
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
        // Check if already casting
        if (isCasting) return false;

        // Check if R is pressed (reload input)
        if (Input.GetKey(KeyCode.R)) return false;

        // Check if current weapon is reloading
        if (currentWeapon != null && currentWeapon.IsReloading) return false;

        return true;
    }

    IEnumerator CastMagicAnimation()
    {
        isCasting = true;

        // Tell arm movement to ignore sprint offset while casting
        if (armMover != null)
            armMover.SetCastingState(true);

        Vector3 targetPos = originalPos + raiseOffset;
        Quaternion targetRot = Quaternion.Euler(originalRot + raiseRotation);

        // Raise arm
        yield return LerpTransform(transform, originalPos, targetPos, Quaternion.Euler(originalRot), targetRot, raiseDuration);

        // Hold position
        yield return new WaitForSeconds(holdDuration);

        // Return arm
        yield return LerpTransform(transform, targetPos, originalPos, targetRot, Quaternion.Euler(originalRot), returnDuration);

        isCasting = false;

        // Allow sprint offset to resume
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
