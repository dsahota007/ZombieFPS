using UnityEngine;
using System.Collections;

public class WeaponHandler : MonoBehaviour
{
    public Transform weaponHolder;           // Drag WeaponHolder (under arms)
    public GameObject[] weaponPrefabs;
    public Transform leftArm;                // Drag your global left arm object
    public CharacterController controller;   // Drag the player's CharacterController
    public float reloadMoveAmount = 0.2f;
    public float reloadDuration = 0.2f;

    private GameObject currentWeapon;
    private Weapon currentWeaponScript;
    private bool isReloading = false;

    public bool IsReloading => isReloading; // 🔥 for ArmMovement to check this

    void Start()
    {
        EquipWeapon(0); // default gun
    }

    void Update()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && controller.velocity.magnitude > 0.1f;

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && !isSprinting)
        {
            if (currentWeaponScript != null && currentWeaponScript.magazine != null && leftArm != null)
            {
                StartCoroutine(Reload(currentWeaponScript.magazine, leftArm));
            }
        }
    }

    public void EquipWeapon(int index)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);

        GameObject newWeapon = Instantiate(weaponPrefabs[index], weaponHolder);
        Weapon weapon = newWeapon.GetComponent<Weapon>();

        if (weapon != null && weapon.weaponOffset != null)
        {
            newWeapon.transform.localPosition = weapon.weaponOffset.localPosition;
            newWeapon.transform.localRotation = weapon.weaponOffset.localRotation;
            newWeapon.transform.localScale = weapon.weaponOffset.localScale;
        }
        else
        {
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localRotation = Quaternion.identity;
            newWeapon.transform.localScale = Vector3.one;
        }

        currentWeapon = newWeapon;
        currentWeaponScript = weapon;
    }

    private IEnumerator Reload(Transform mag, Transform arm)
    {
        isReloading = true;

        Vector3 magStart = mag.localPosition;
        Vector3 armStart = arm.localPosition;

        Vector3 magDown = magStart + new Vector3(0f, -reloadMoveAmount, 0f);
        Vector3 armDown = armStart + new Vector3(0f, -reloadMoveAmount, 0f);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / reloadDuration;
            mag.localPosition = Vector3.Lerp(magStart, magDown, t);
            arm.localPosition = Vector3.Lerp(armStart, armDown, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / reloadDuration;
            mag.localPosition = Vector3.Lerp(magDown, magStart, t);
            arm.localPosition = Vector3.Lerp(armDown, armStart, t);
            yield return null;
        }

        isReloading = false;
    }
}
