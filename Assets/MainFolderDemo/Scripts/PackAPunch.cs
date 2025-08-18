using UnityEngine;
using System.Collections;

public class PackAPunch : MonoBehaviour
{
    public Transform showcasePoint;
    public float interactRange = 3f;
    public float cookTime = 3f;

    private GameObject showcasedWeapon;
    private WeaponManager weaponManager;
    private bool isCooking = false;
    private bool isReady = false;
    private int storedIndex = -1;

    void Start()
    {
        weaponManager = FindFirstObjectByType<WeaponManager>();
    }

    void Update()
    {
        if (weaponManager == null || showcasePoint == null) return;

        Transform player = weaponManager.transform;
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isCooking && !isReady)
            {
                StartCoroutine(SendWeaponToShowcase());
            }
            else if (isReady)
            {
                RetrieveWeapon();
            }
        }
    }

    IEnumerator SendWeaponToShowcase()
    {
        if (weaponManager == null || WeaponManager.ActiveWeapon == null) yield break;

        isCooking = true;
        isReady = false;

        // 🔒 disable weapon switching while cooking
        weaponManager.disableSwitching = true;

        // store the index of the weapon being upgraded
        storedIndex = weaponManager.CurrentWeaponIndex;

        if (showcasedWeapon != null)
            Destroy(showcasedWeapon);

        // clone prefab for showcase
        GameObject prefab = weaponManager.weaponPrefabs[storedIndex];
        showcasedWeapon = Instantiate(prefab, showcasePoint.position, showcasePoint.rotation, showcasePoint);
        foreach (var comp in showcasedWeapon.GetComponentsInChildren<MonoBehaviour>())
            Destroy(comp);

        // disable player's active weapon
        WeaponManager.ActiveWeapon.DisableWeapon();

        // auto switch to the other weapon if available
        int nextIndex = (storedIndex == 0) ? 1 : 0;
        if (nextIndex >= 0 && nextIndex < weaponManager.weaponPrefabs.Length)
        {
            weaponManager.StartCoroutine(weaponManager.SwitchWeaponWithDrop(nextIndex));
        }

        yield return new WaitForSeconds(cookTime);

        isCooking = false;
        isReady = true;
    }

    void RetrieveWeapon()
    {
        if (!isReady || storedIndex < 0) return;

        if (showcasedWeapon != null)
            Destroy(showcasedWeapon);

        // re-enable weapon
        WeaponManager.ActiveWeapon.EnableWeapon();

        // switch back to upgraded slot
        weaponManager.StartCoroutine(weaponManager.SwitchWeaponWithDrop(storedIndex));

        // 🔓 re-enable switching now that upgrade is done
        weaponManager.disableSwitching = false;

        isReady = false;
        storedIndex = -1;
    }
}
