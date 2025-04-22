using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public Transform weaponHolder; // Drag the WeaponHolder (child of arms) here
    public GameObject[] weaponPrefabs;

    private GameObject currentWeapon;

    void Start()
    {
        EquipWeapon(0); // for now, equip the first weapon
    }

    public void EquipWeapon(int index)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);

        GameObject newWeapon = Instantiate(weaponPrefabs[index], weaponHolder);
        Weapon weapon = newWeapon.GetComponent<Weapon>();

        if (weapon != null && weapon.weaponOffset != null)
        {
            // Set position, rotation, and scale based on offset object in the prefab
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
    }
}
