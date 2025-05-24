using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public Text ammoText;     // Drag your AmmoText UI object here
    public Text nameText;     // Drag your GunNameText UI object here

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
