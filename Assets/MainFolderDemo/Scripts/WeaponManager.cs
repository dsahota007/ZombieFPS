using System;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform weaponHolder;  //empty where its gonna be 
    public GameObject[] weaponPrefabs;   //list empty
    public Transform leftArm;           // left arm for reloading
    public CharacterController controller;       //Reference to the player’s CharacterController (used for sprinting logic).

    private GameObject currentWeapon;    //These store the currently equipped weapon GameObject and its script
    private Weapon currentWeaponScript;

    public static Weapon ActiveWeapon; // 🔥 Global access to current weapon


    //func to d
    public bool IsReloading => currentWeaponScript != null && currentWeaponScript.IsReloading;

    void Start()
    {
        EquipWeapon(0);
    }

    void Update()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && controller.velocity.magnitude > 0.1f;

        if (Input.GetKeyDown(KeyCode.R) && !IsReloading && !isSprinting)
        {
            currentWeaponScript?.StartReload();
        }
    }

    public void EquipWeapon(int index)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);

        GameObject newWeapon = Instantiate(weaponPrefabs[index], weaponHolder);
        currentWeapon = newWeapon;
        currentWeaponScript = newWeapon.GetComponent<Weapon>();

        if (currentWeaponScript != null)
        {
            currentWeaponScript.leftArm = leftArm;
            currentWeaponScript.controller = controller;

            // Assign global reference
            ActiveWeapon = currentWeaponScript;

            // Position weapon
            if (currentWeaponScript.weaponOffset != null)
            {
                newWeapon.transform.localPosition = currentWeaponScript.weaponOffset.localPosition;
                newWeapon.transform.localRotation = currentWeaponScript.weaponOffset.localRotation;
                newWeapon.transform.localScale = currentWeaponScript.weaponOffset.localScale;
            }
            else
            {
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.localRotation = Quaternion.identity;
                newWeapon.transform.localScale = Vector3.one;
            }
        }
    }


}
