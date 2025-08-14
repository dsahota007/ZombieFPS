using UnityEngine;
using System.Collections;
//using System.Collections.Generic;

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

    private PlayerMovement playerMovement;          //fetching scripts
    private Weapon currentWeapon;
    private ArmMovementMegaScript armMover;

    [Header("Spell Firepoint VFX")]
    public Transform vfxAttachPoint;        

    [Header("Magic Attack - Set by MagicManager")]      //we spawn in with nothgin ---------------------------------------------------!!!
    public GameObject MagicBallEntityPrefab;
    public Transform MagicfirePoint;
    public GameObject MagicArmFireVFX;

    void Start()
    {
        originalPos = transform.localPosition;      //current pos of the arms 
        originalRot = transform.localRotation.eulerAngles;

        playerMovement = FindFirstObjectByType<PlayerMovement>();           //fetch scripts
        armMover = FindFirstObjectByType<ArmMovementMegaScript>();
    }

    //void Update()
    //{
    //    currentWeapon = WeaponManager.ActiveWeapon;    //Gets the currently equipped weapon so the system knows what weapon the player is holding
    //    // Note: Magic casting is now handled by MagicManager
    //}

    bool CanCastSpell()
    {
        if (isCasting) return false;                //if your already casting magic than get outt this code
        if (armMover.IsPerkAnimating) return false;

        if (Input.GetKey(KeyCode.R)) return false;              //if your already reloading than get out of this code
        if (currentWeapon != null && currentWeapon.IsReloading) return false;
        return true;
    }
    public IEnumerator CastMagicAnimation()
    {
        //// Extra safety - don't cast if no fireball prefab
        //if (fireballPrefab == null)
        //{
        //    Debug.Log("No magic equipped! Visit a magic station first.");
        //    yield break;
        //}

        if (!CanCastSpell()) yield break;

        isCasting = true;

        if (armMover != null)
            armMover.SetCastingState(true);    //Tells the arm movement system to enter casting mode

        Vector3 targetPos = originalPos + raiseOffset;                              //move arm
        Quaternion targetRot = Quaternion.Euler(originalRot + raiseRotation);       //move arm rotation

        GameObject spawnedVFX = null;
        if (MagicArmFireVFX != null && vfxAttachPoint != null)    //hand magic animation
        {
            spawnedVFX = Instantiate(MagicArmFireVFX, vfxAttachPoint.position, vfxAttachPoint.rotation, vfxAttachPoint);
        }

        yield return LerpTransform(transform, originalPos, targetPos, Quaternion.Euler(originalRot), targetRot, raiseDuration); //lerp (gameObject, ogPos, targetPos, OGRotation, TargetRotation, time to hold it)

        yield return new WaitForSeconds(holdDuration * 0.5f);   //hold duration before firing 

        if (MagicBallEntityPrefab != null && MagicfirePoint != null)
        {
            Instantiate(MagicBallEntityPrefab, MagicfirePoint.position, MagicfirePoint.rotation);   //spawn what, where and the roation of it so the magic bullet entitiy
        }

        yield return new WaitForSeconds(holdDuration * 0.5f);  // holds again when casting fire  

        yield return LerpTransform(transform, targetPos, originalPos, targetRot, Quaternion.Euler(originalRot), returnDuration);   //returnign the hand back to og position

        if (spawnedVFX != null)
            StartCoroutine(FadeOutParticlesProperly(spawnedVFX, 1f)); // fade over 0.4 seconds  -- we took out destory 
                                                                             //destroy the hand on magic animation

        isCasting = false;

        if (armMover != null)
            armMover.SetCastingState(false);
    }


    IEnumerator LerpTransform(Transform t, Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float duration)
    {
        float time = 0f;   //Start a timer that will go from 0 to 1 (normalized time).
        while (time < 1f)   //Repeat the loop until time reaches or exceeds 1f, which means the animation is done.   -- when time is greater than 1 
        {
            time += Time.deltaTime / duration;   //Calculates animation progress (0 to 1)
            t.localPosition = Vector3.Lerp(fromPos, toPos, time);
            t.localRotation = Quaternion.Slerp(fromRot, toRot, time);
            yield return null;    //Waits one frame before continuing (creates smooth animation)
        }
    }
    IEnumerator FadeOutParticlesProperly(GameObject fx, float duration)
    {
        ParticleSystem[] particleSystems = fx.GetComponentsInChildren<ParticleSystem>();    //Finds all particle systems inside fx, including children, so we can apply the fade effect to all particles

        foreach (var ps in particleSystems)   //Loop through each particle system individually
        {
            var colorOverLifetime = ps.colorOverLifetime;   //Grab the colorOverLifetime module from the particle system
            colorOverLifetime.enabled = true;               //Enable it so it starts affecting particles

            Gradient gradient = new Gradient();    //Create a new Gradient, which lets you define how color and alpha change over time
            gradient.SetKeys(
                new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f), // full alpha at start
                new GradientAlphaKey(0f, 1f)  // fade to zero alpha over life
                }
            );

            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);    //Apply the fade effect (gradient) to the particle system

            ps.Stop(); // stop emitting new particles
        }

        yield return new WaitForSeconds(duration);   //Waits for a bit so the fade can finish (typically 1–2 seconds is enough for most VFX).
        Destroy(fx);
    }

    public bool IsCasting() => isCasting;   // getter to see if im casting magic for manger adn other scripts
}
