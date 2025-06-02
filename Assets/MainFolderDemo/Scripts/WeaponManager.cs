using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    public Transform weaponHolder;
    public Transform leftArm;
    public Transform rightArm;
    public GameObject[] weaponPrefabs;
    public CharacterController controller;

    private GameObject[] weapons = new GameObject[2];
    private Weapon[] weaponScripts = new Weapon[2];
    private int currentWeaponIndex = 0;
    private bool isSwitching = false;

    private Vector3 weaponHolderOriginalPos;
    private Vector3 leftArmOriginalPos;
    private Vector3 rightArmOriginalPos;

    public static Weapon ActiveWeapon;

    public bool IsReloading => ActiveWeapon != null && ActiveWeapon.IsReloading;

    void Start()
    {
        weaponHolderOriginalPos = weaponHolder.localPosition;
        leftArmOriginalPos = leftArm.localPosition;
        rightArmOriginalPos = rightArm.localPosition;

        SpawnWeapons();
        EquipWeaponInstant(0); // Start with weapon 0
    }

    void Update()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && controller.velocity.magnitude > 0.1f;

        if (Input.GetKeyDown(KeyCode.R) && !IsReloading && !isSprinting)
        {
            ActiveWeapon?.StartReload();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !isSwitching)
        {
            int nextIndex = (currentWeaponIndex == 0) ? 1 : 0;
            StartCoroutine(SwitchWeaponWithDrop(nextIndex));
        }




    }

    void SpawnWeapons()
    {
        for (int i = 0; i < 2 && i < weaponPrefabs.Length; i++)
        {
            GameObject weapon = Instantiate(weaponPrefabs[i], weaponHolder);
            weapon.SetActive(false);

            weapons[i] = weapon;
            weaponScripts[i] = weapon.GetComponent<Weapon>();

            if (weaponScripts[i] != null)
            {
                weaponScripts[i].leftArm = leftArm;
                weaponScripts[i].controller = controller;

                if (weaponScripts[i].weaponOffset != null)
                {
                    weapon.transform.localPosition = weaponScripts[i].weaponOffset.localPosition;
                    weapon.transform.localRotation = weaponScripts[i].weaponOffset.localRotation;
                    weapon.transform.localScale = weaponScripts[i].weaponOffset.localScale;
                }
                else
                {
                    weapon.transform.localPosition = Vector3.zero;
                    weapon.transform.localRotation = Quaternion.identity;
                    weapon.transform.localScale = Vector3.one;
                }
            }
        }
    }

    IEnumerator SwitchWeaponWithDrop(int index)
    {
        if (index < 0 || index >= weapons.Length || index == currentWeaponIndex)
            yield break;

        isSwitching = true;

        // Animate down
        Vector3 dropOffset = new Vector3(0f, -0.3f, 0f);
        Vector3 weaponDown = weaponHolderOriginalPos + dropOffset;
        Vector3 leftDown = leftArmOriginalPos + dropOffset;
        Vector3 rightDown = rightArmOriginalPos + dropOffset;

        yield return StartCoroutine(MoveAll(weaponHolder, weaponHolderOriginalPos, weaponDown,
                                            leftArm, leftArmOriginalPos, leftDown,
                                            rightArm, rightArmOriginalPos, rightDown, 0.1f));

        // Cancel reload & switch
        ActiveWeapon?.CancelReload();

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(i == index);

        currentWeaponIndex = index;
        ActiveWeapon = weaponScripts[index];
        ActiveWeapon?.CancelReload();

        // Animate back up
        yield return StartCoroutine(MoveAll(weaponHolder, weaponDown, weaponHolderOriginalPos,
                                            leftArm, leftDown, leftArmOriginalPos,
                                            rightArm, rightDown, rightArmOriginalPos, 0.1f));

        isSwitching = false;
    }

    IEnumerator MoveAll(Transform w, Vector3 fromW, Vector3 toW,
                        Transform l, Vector3 fromL, Vector3 toL,
                        Transform r, Vector3 fromR, Vector3 toR,
                        float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            w.localPosition = Vector3.Lerp(fromW, toW, t);
            l.localPosition = Vector3.Lerp(fromL, toL, t);
            r.localPosition = Vector3.Lerp(fromR, toR, t);
            yield return null;
        }
    }

    void EquipWeaponInstant(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == index);
        }

        currentWeaponIndex = index;
        ActiveWeapon = weaponScripts[index];
    }


    public void GiveWeapon(GameObject weaponPrefab)
    {
        // Find first empty slot or replace current
        int slotToReplace = 0; // You can change this logic (e.g. always replaces current)
        if (weaponPrefabs.Length > 1 && weapons[1] == null) slotToReplace = 1;

        // Destroy old weapon in slot (if any)
        if (weapons[slotToReplace] != null)
            Destroy(weapons[slotToReplace]);

        // Instantiate new weapon
        GameObject newWeapon = Instantiate(weaponPrefab, weaponHolder);
        weapons[slotToReplace] = newWeapon;
        weaponScripts[slotToReplace] = newWeapon.GetComponent<Weapon>();

        // Setup new weapon references
        if (weaponScripts[slotToReplace] != null)
        {
            weaponScripts[slotToReplace].leftArm = leftArm;
            weaponScripts[slotToReplace].controller = controller;
            if (weaponScripts[slotToReplace].weaponOffset != null)
            {
                newWeapon.transform.localPosition = weaponScripts[slotToReplace].weaponOffset.localPosition;
                newWeapon.transform.localRotation = weaponScripts[slotToReplace].weaponOffset.localRotation;
                newWeapon.transform.localScale = weaponScripts[slotToReplace].weaponOffset.localScale;
            }
            else
            {
                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.localRotation = Quaternion.identity;
                newWeapon.transform.localScale = Vector3.one;
            }
        }

        // Equip immediately
        EquipWeaponInstant(slotToReplace);
    }

}
