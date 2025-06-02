using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Text ammoText;
    public Text nameText;

    void Update()
    {
        Weapon currentWeapon = WeaponManager.ActiveWeapon;

        if (currentWeapon != null)
        {
            if (ammoText != null)
                ammoText.text = currentWeapon.GetCurrentAmmo() + " / " + currentWeapon.GetAmmoReserve();

            if (nameText != null)
                nameText.text = currentWeapon.weaponName;
        }
        else
        {
            if (ammoText != null)
                ammoText.text = "-- / --";

            if (nameText != null)
                nameText.text = "No Weapon";
        }
    }
}
