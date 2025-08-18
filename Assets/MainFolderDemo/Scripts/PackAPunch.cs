using UnityEngine;
using System.Collections;

public class PackAPunch : MonoBehaviour
{
    [Header("Pack-A-Punch Settings")]
    public Transform showcasePoint;
    public float interactRange = 3f;
    public float cookTime = 3f;
    public int baseCost = 5000; // Base cost for first upgrade

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
                // Only allow upgrade if weapon can be upgraded
                Weapon currentWeapon = WeaponManager.ActiveWeapon;
                if (currentWeapon != null && currentWeapon.upgradeLevel < currentWeapon.maxUpgradeLevel)
                {
                    if (CanUpgradeWeapon())
                    {
                        StartCoroutine(SendWeaponToShowcase());
                    }
                }
            }
            else if (isReady)
            {
                RetrieveAndUpgradeWeapon();
            }
        }
    }

    private int GetUpgradeCost(int currentLevel)
    {
        // Incremental pricing: 5000, 15000, 30000
        switch (currentLevel)
        {
            case 0: return baseCost;        // 5000 for first upgrade
            case 1: return baseCost * 3;    // 15000 for second upgrade  
            case 2: return baseCost * 6;    // 30000 for third upgrade
            default: return baseCost * 6;   // Max cost if somehow higher
        }
    }

    private bool CanUpgradeWeapon()
    {
        if (WeaponManager.ActiveWeapon == null) return false;

        Weapon currentWeapon = WeaponManager.ActiveWeapon;
        int currentPoints = (PointManager.Instance != null) ? PointManager.Instance.GetPoints() : 0;
        int upgradeCost = GetUpgradeCost(currentWeapon.upgradeLevel);

        // Check if weapon is already fully upgraded
        if (currentWeapon.upgradeLevel >= currentWeapon.maxUpgradeLevel)
        {
            return false;
        }

        // Check if player has enough points
        if (currentPoints < upgradeCost)
        {
            return false;
        }

        return true;
    }

    IEnumerator SendWeaponToShowcase()
    {
        if (weaponManager == null || WeaponManager.ActiveWeapon == null) yield break;

        Weapon currentWeapon = weaponManager.GetWeaponScriptAtIndex(weaponManager.CurrentWeaponIndex);
        int upgradeCost = GetUpgradeCost(currentWeapon.upgradeLevel);

        // Deduct points for upgrade
        if (PointManager.Instance != null)
        {
            PointManager.Instance.SubtractPoints(upgradeCost);
        }

        isCooking = true;
        isReady = false;

        // 🔒 disable weapon switching while cooking
        weaponManager.disableSwitching = true;

        // store the index of the weapon being upgraded
        storedIndex = weaponManager.CurrentWeaponIndex;

        if (showcasedWeapon != null)
            Destroy(showcasedWeapon);

        // clone prefab for showcase (just for visuals)
        GameObject currentWeaponGO = weaponManager.GetWeaponObjectAtIndex(weaponManager.CurrentWeaponIndex);
        if (currentWeaponGO == null) yield break;

        showcasedWeapon = Instantiate(currentWeaponGO, showcasePoint.position, showcasePoint.rotation, showcasePoint.transform);

        // Remove MonoBehaviours so it's purely visual
        foreach (var comp in showcasedWeapon.GetComponentsInChildren<MonoBehaviour>())
            Destroy(comp);

        Weapon realWeapon = currentWeaponGO.GetComponent<Weapon>();


        Renderer[] renderers = showcasedWeapon.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            if (realWeapon.gunRenderer != null && r.name == realWeapon.gunRenderer.name && realWeapon.papGunMaterial != null)
            {
                r.material = realWeapon.papGunMaterial;
            }

            if (realWeapon.magazineRenderer != null && r.name == realWeapon.magazineRenderer.name && realWeapon.papMagMaterial != null)
            {
                r.material = realWeapon.papMagMaterial;
            }
        }


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

    void RetrieveAndUpgradeWeapon()
    {
        if (!isReady || storedIndex < 0) return;

        if (showcasedWeapon != null)
            Destroy(showcasedWeapon);

        // get the REAL weapon script at stored slot
        Weapon w = weaponManager.GetWeaponScriptAtIndex(storedIndex);

        // upgrade damage (only if not maxed)
        if (w.upgradeLevel < w.maxUpgradeLevel)
        {
            w.bulletDamage *= 3f;
            w.upgradeLevel++;

            // ammo size upgrade
            switch (w.upgradeLevel)
            {
                case 1:
                    w.clipSize = w.Tier1clipSize;
                    w.maxReserve = w.Tier1maxReserve;
                    break;
                case 2:
                    w.clipSize = w.Tier2clipSize;
                    w.maxReserve = w.Tier2maxReserve;
                    break;
                case 3:
                    w.clipSize = w.Tier3clipSize;
                    w.maxReserve = w.Tier3maxReserve;
                    break;
            }

            w.RefillFull(); // refill mag + reserve
            w.ApplyPackAPunchSkin();


            //if (!w.hasPackVFX && w.packVFXPrefab != null)
            //{
            //    GameObject vfx = Instantiate(w.packVFXPrefab, w.transform);
            //    vfx.transform.localPosition = w.packVFXOffset;
            //    vfx.transform.localEulerAngles = w.packVFXRotation;
            //    vfx.transform.localScale = w.packVFXScale;
            //    w.hasPackVFX = true;
            //}

            // switch back to upgraded slot
            StartCoroutine(weaponManager.SwitchWeaponWithDrop(storedIndex));

            isReady = false;
            storedIndex = -1;

            // 🔓 allow switching again
            weaponManager.disableSwitching = false;
        }
    }

    // Method that UI can call to get the prompt text
    public string GetPromptText()
    {
        if (weaponManager == null) return "";

        Transform player = weaponManager.transform;
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist > interactRange) return "";

        if (isCooking)
        {
            return "Upgrading weapon...";
        }
        else if (isReady)
        {
            return "Press [E] to retrieve upgraded weapon";
        }
        else
        {
            if (WeaponManager.ActiveWeapon == null) return "";

            Weapon currentWeapon = WeaponManager.ActiveWeapon;
            int currentPoints = (PointManager.Instance != null) ? PointManager.Instance.GetPoints() : 0;
            int upgradeCost = GetUpgradeCost(currentWeapon.upgradeLevel);

            if (currentWeapon.upgradeLevel >= currentWeapon.maxUpgradeLevel)
            {
                return "Weapon is already fully upgraded";
            }
            else if (currentPoints < upgradeCost)
            {
                return $"Need {upgradeCost} points to upgrade weapon";
            }
            else
            {
                return $"Press [E] to upgrade {currentWeapon.weaponName} ({upgradeCost} points)";
            }
        }
    }
}