using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    public float interactDistance = 3f;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            RefillCurrentWeaponAmmo();
        }
    }

    void RefillCurrentWeaponAmmo()
    {
        WeaponManager wm = FindObjectOfType<WeaponManager>();
        if (wm == null || WeaponManager.ActiveWeapon == null) return;

        Weapon current = WeaponManager.ActiveWeapon;

        // Refill logic for current weapon only
        var maxClip = current.clipSize;
        var maxReserve = current.maxReserve;

        typeof(Weapon).GetField("currentAmmo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(current, maxClip);

        typeof(Weapon).GetField("ammoReserve", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(current, maxReserve);

        Debug.Log("Refilled ammo for: " + current.weaponName);
    }
}
