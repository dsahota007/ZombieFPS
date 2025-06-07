using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    public Transform weaponHolder;
    public Transform leftArm;
    public Transform rightArm;
    public GameObject[] weaponPrefabs;          //infinte list for weapons we can add
    private CharacterController controller;

    private GameObject[] weapons = new GameObject[2];     //--------------------------------------------????
    private Weapon[] weaponScripts = new Weapon[2];       //--------------------------------------------????
    private int currentWeaponIndex = 0;
    private bool isSwitching = false;                   //FOR SPAM ??

    //OG Positions for both arms and where gun is suppose to be
    private Vector3 weaponHolderOriginalPos;
    private Vector3 leftArmOriginalPos;
    private Vector3 rightArmOriginalPos;

    public static Weapon ActiveWeapon;

    private bool IsReloading = false;   //deleted line below this is yapping i think
    //public bool IsReloading => ActiveWeapon != null && ActiveWeapon.IsReloading;

    void Start()
    {
        weaponHolderOriginalPos = weaponHolder.localPosition;
        leftArmOriginalPos = leftArm.localPosition;
        rightArmOriginalPos = rightArm.localPosition;

        SpawnWeapons();
        EquipWeaponNow(0); // Start with weapon 0
    }

    void Update()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && controller.velocity.magnitude > 0.1f;

        if (Input.GetKeyDown(KeyCode.R) && !IsReloading)  //fix sprint when reloading ------------------ 
        {
            ActiveWeapon.StartReload();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !isSwitching)
        {
            int nextIndex = (currentWeaponIndex == 0) ? 1 : 0;              //toggle between weapon 0 and 1 
            StartCoroutine(SwitchWeaponWithDrop(nextIndex));                
        }
    }

    void SpawnWeapons()
    {
        for (int i = 0; i < 2 && i < weaponPrefabs.Length; i++)     //wew get first 2 guns
        {
            GameObject weapon = Instantiate(weaponPrefabs[i], weaponHolder);
            //weapon.SetActive(false);   did not see any overlapping so i delete this line.

            weapons[i] = weapon;
            weaponScripts[i] = weapon.GetComponent<Weapon>();

            if (weaponScripts[i] != null)
            {
                weaponScripts[i].leftArm = leftArm;
                weaponScripts[i].controller = controller;

                if (weaponScripts[i].weaponOffset != null)                  //Apply offset
                {
                    weapon.transform.localPosition = weaponScripts[i].weaponOffset.localPosition;
                    weapon.transform.localRotation = weaponScripts[i].weaponOffset.localRotation;
                    weapon.transform.localScale = weaponScripts[i].weaponOffset.localScale;
                }
                else
                {
                    weapon.transform.localPosition = Vector3.zero;              //or deafult
                    weapon.transform.localRotation = Quaternion.identity;
                    weapon.transform.localScale = Vector3.one;
                    //Debug.Log("offset aint working");
                }
            }
        }
    }

    IEnumerator SwitchWeaponWithDrop(int index)
    {
        if (index < 0 || index >= weapons.Length || index == currentWeaponIndex)   //If the weapon index is invalid OR you're already holding that weapon, stop here
            yield break;

        isSwitching = true;

        // Animate down
        Vector3 dropOffset = new Vector3(0f, -0.3f, 0f);
        Vector3 weaponDown = weaponHolderOriginalPos + dropOffset;
        Vector3 leftDown = leftArmOriginalPos + dropOffset;
        Vector3 rightDown = rightArmOriginalPos + dropOffset;

        yield return StartCoroutine(MoveAll(weaponHolder, weaponHolderOriginalPos, weaponDown,
                                            leftArm, leftArmOriginalPos, leftDown,
                                            rightArm, rightArmOriginalPos, rightDown, 0.2f));

        // Cancel reload & switch
        ActiveWeapon.CancelReload();

        for (int i = 0; i < weapons.Length; i++)
            weapons[i].SetActive(i == index);

        currentWeaponIndex = index;
        ActiveWeapon = weaponScripts[index];
        ActiveWeapon.CancelReload();

        // Animate back up
        yield return StartCoroutine(MoveAll(weaponHolder, weaponDown, weaponHolderOriginalPos,
                                            leftArm, leftDown, leftArmOriginalPos,
                                            rightArm, rightDown, rightArmOriginalPos, 0.2f));

        isSwitching = false;
    }

    IEnumerator MoveAll(Transform w, Vector3 fromW, Vector3 toW,    //weapon      
                        Transform l, Vector3 fromL, Vector3 toL,    //left arm
                        Transform r, Vector3 fromR, Vector3 toR,    //right arm
                        float duration)                             //time to finish
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            w.localPosition = Vector3.Lerp(fromW, toW, time);
            l.localPosition = Vector3.Lerp(fromL, toL, time);
            r.localPosition = Vector3.Lerp(fromR, toR, time);
            yield return null;
        }
    }

    void EquipWeaponNow(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == index);
        }

        currentWeaponIndex = index;
        ActiveWeapon = weaponScripts[index];
    }


    public void GiveWeapon(GameObject weaponPrefab)         //----------------kind of confusing to me - the last part, begining easy 
    {
        int slotToReplace = 0; // You can change this logic (e.g. always replaces current)          -- currently holding
        //if (weaponPrefabs.Length > 1 && weapons[1] == null) slotToReplace = 1;                    -- we always spawn in with 2 guns so no need.

        //if (weapons[slotToReplace] != null)       --        // Destroy old weapon in slot (if any)
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
        EquipWeaponNow(slotToReplace);
    }

}
