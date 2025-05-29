using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public Transform weaponHolder;
    public GameObject[] weaponPrefabs;
    public Transform leftArm;
    public CharacterController controller;

    private GameObject[] weapons = new GameObject[2];
    private Weapon[] weaponScripts = new Weapon[2];
    private int currentWeaponIndex = 0;

    public static Weapon ActiveWeapon;

    public bool IsReloading => ActiveWeapon != null && ActiveWeapon.IsReloading;

    void Start()
    {
        SpawnWeapons();
        EquipWeapon(0); // Start with weapon 0
    }

    void Update()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && controller.velocity.magnitude > 0.1f;

        if (Input.GetKeyDown(KeyCode.R) && !IsReloading && !isSprinting)
        {
            ActiveWeapon?.StartReload();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int nextIndex = (currentWeaponIndex == 0) ? 1 : 0;
            EquipWeapon(nextIndex);
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

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length || weapons[index] == null)
            return;

        // Cancel reload on the currently active weapon BEFORE switching
        if (ActiveWeapon != null)
            ActiveWeapon.CancelReload();

        // Deactivate all weapons first
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);
        }

        currentWeaponIndex = index;
        ActiveWeapon = weaponScripts[index];

        // Just in case the new weapon was stuck in reload (e.g., switched while inactive)
        if (ActiveWeapon != null)
            ActiveWeapon.CancelReload();
    }



}
