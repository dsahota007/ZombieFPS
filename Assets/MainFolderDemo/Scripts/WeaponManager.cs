using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    public Transform weaponHolder;
    public Transform leftArm;
    public Transform rightArm;
    public GameObject[] weaponPrefabs;
    public CharacterController controller;

    private GameObject[] weapons = new GameObject[2];
    private Weapon[] weaponScripts = new Weapon[2];
    private int currentWeaponIndex = 0;

    public static Weapon ActiveWeapon;

    public bool IsReloading => ActiveWeapon != null && ActiveWeapon.IsReloading;

    private Vector3 weaponHolderOriginalPos;
    private Vector3 leftArmOriginalPos;
    private Vector3 rightArmOriginalPos;
    private bool isSwitching = false;

    void Start()
    {
        weaponHolderOriginalPos = weaponHolder.localPosition;
        leftArmOriginalPos = leftArm.localPosition;
        rightArmOriginalPos = rightArm.localPosition;

        SpawnWeapons();
        EquipWeaponInstant(0);
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

        Vector3 downOffset = new Vector3(0f, -0.3f, 0f);
        Vector3 holderDown = weaponHolderOriginalPos + downOffset;
        Vector3 leftDown = leftArmOriginalPos + downOffset;
        Vector3 rightDown = rightArmOriginalPos + downOffset;

        yield return StartCoroutine(MoveAllThree(weaponHolder, leftArm, rightArm,
            weaponHolderOriginalPos, holderDown,
            leftArmOriginalPos, leftDown,
            rightArmOriginalPos, rightDown,
            0.1f));

        if (ActiveWeapon != null)
            ActiveWeapon.CancelReload();

        for (int i = 0; i < weapons.Length; i++)
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);

        currentWeaponIndex = index;
        ActiveWeapon = weaponScripts[index];

        if (ActiveWeapon != null)
            ActiveWeapon.CancelReload();

        yield return StartCoroutine(MoveAllThree(weaponHolder, leftArm, rightArm,
            holderDown, weaponHolderOriginalPos,
            leftDown, leftArmOriginalPos,
            rightDown, rightArmOriginalPos,
            0.1f));

        isSwitching = false;
    }

    IEnumerator MoveAllThree(Transform holder, Transform left, Transform right,
        Vector3 fromHolder, Vector3 toHolder,
        Vector3 fromLeft, Vector3 toLeft,
        Vector3 fromRight, Vector3 toRight,
        float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            holder.localPosition = Vector3.Lerp(fromHolder, toHolder, t);
            left.localPosition = Vector3.Lerp(fromLeft, toLeft, t);
            right.localPosition = Vector3.Lerp(fromRight, toRight, t);
            yield return null;
        }
    }

    void EquipWeaponInstant(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);
        }

        currentWeaponIndex = index;
        ActiveWeapon = weaponScripts[index];
    }
}
