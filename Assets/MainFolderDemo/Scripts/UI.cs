using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Text ammoText;

    void Start()
    {
        ammoText = GetComponent<Text>();
    }

    void Update()
    {
        Weapon currentWeapon = WeaponManager.ActiveWeapon;

        if (currentWeapon != null)
        {
            ammoText.text = currentWeapon.GetCurrentAmmo() + " / " + currentWeapon.GetAmmoReserve();
        }
        else
        {
            ammoText.text = "-- / --";
        }
    }
}
