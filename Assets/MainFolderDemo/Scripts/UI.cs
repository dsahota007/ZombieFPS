// Step-by-step UI display for Mystery Box interaction

using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Transform player;

    [Header("Weapon Info UI")]
    public Text WeaponAmmoText;
    public Text WeaponNameText;

    [Header("Mystery Box Popup UI")]
    public Text MysteryBoxText;
    public MysteryBox mysteryBox;    //getting script

    [Header("Round UI")]
    public Text roundText;
    public ZombieSpawner zombieSpawner;  //getting script

    [Header("Kinetic Slam UI")]
    public Slider slamCooldownSlider;   
    public PlayerMovement playerMovement;

    [Header("Player Health UI")]
    public Slider playerHealthSlider;

    [Header("Points UI")]
    public Text pointsText;

    [Header("Ammo Box UI")]
    public Text ammoBoxText;
    public AmmoBox ammoBox; // reference to your ammo box object/script

    [Header("Magic Station UI")]
    public Text fireMagicText;           // For normal fireball station
    public Text sulfuricFireMagicText;   // For sulfuric fire station
    public MagicStation fireStation;     // Reference to normal fire station
    public MagicStation sulfuricStation; // Reference to sulfuric fire station

    void Update()
    {
        Weapon currentWeapon = WeaponManager.ActiveWeapon;

        //--------------------------------------------------------------- Weapon UI

        if (currentWeapon != null)
        {
            WeaponAmmoText.text = currentWeapon.GetCurrentAmmo() + " / " + currentWeapon.GetAmmoReserve();
            WeaponNameText.text = currentWeapon.weaponName;
        }
        else
        {
            WeaponAmmoText.text = "-- / --";
            WeaponNameText.text = "No Weapon";
        }

        //--------------------------------------------------------------- Mystery Box UI

        float distanceToBox = Vector3.Distance(player.position, mysteryBox.transform.position);   //we check distance from player and box

        bool PlayerIsCloseCanOpenBox = !mysteryBox.IsBoxOpen() && distanceToBox <= mysteryBox.minimumDistanceToOpen;      //box close
        bool PlayerIsCloseCanGrabWeapon = mysteryBox.IsBoxOpen() && distanceToBox <= mysteryBox.minimumDistanceToOpen;    //box close

        if (PlayerIsCloseCanOpenBox)
        {
            MysteryBoxText.text = "Press [E] to Open Mystery Box for 950 Points";
            MysteryBoxText.gameObject.SetActive(true);
        }
        else if (PlayerIsCloseCanGrabWeapon && mysteryBox.GetCurrentPreview() != null)
        {
            Weapon weapon = mysteryBox.GetCurrentPreview().GetComponent<Weapon>();   //so we get the weapon adn than grab the Weapon.cs script 
            string weaponName = (weapon != null) ? weapon.weaponName : "Unknown";  //if we find weapon script use that name if we cant use unkown
            MysteryBoxText.text = "Press [F] to pick up: " + weaponName;
            MysteryBoxText.gameObject.SetActive(true);
        }
        else
        {
            MysteryBoxText.gameObject.SetActive(false);   //if either variabel is not true we keep it false at all times.
        }
         

        // ------------------------------------------------------------------ Ammo Box UI

        if (ammoBox != null && ammoBoxText != null)
        {
            float distanceToAmmo = Vector3.Distance(player.position, ammoBox.transform.position);

            if (distanceToAmmo <= ammoBox.interactDistance)
            {
                currentWeapon = WeaponManager.ActiveWeapon;

                if (currentWeapon != null)
                {
                    bool clipFull = currentWeapon.GetCurrentAmmo() == currentWeapon.clipSize;
                    bool reserveFull = currentWeapon.GetAmmoReserve() == currentWeapon.maxReserve;

                    if (clipFull && reserveFull)
                    {
                        ammoBoxText.text = "Ammo is Full";
                    }
                    else
                    {
                        ammoBoxText.text = "Press [E] to Refill Ammo";
                    }

                    ammoBoxText.gameObject.SetActive(true);
                }
                else
                {
                    ammoBoxText.gameObject.SetActive(false);
                }
            }
            else
            {
                ammoBoxText.gameObject.SetActive(false);
            }

        }

        //--------------------------------------------------------------- Round system UI

        if (zombieSpawner != null)
        {
            roundText.text = "" + zombieSpawner.GetCurrentRound();
        }

        //--------------------------------------------------------------- KineticSlamCooldown UI

        if (playerMovement != null)
        {
            float slamTimePassed = Time.time - playerMovement.LastSlamTime;   //Time.time is the current time in seconds since the game started. we sub lastslameTime top calc how much time has passed
            float slamProgress = Mathf.Clamp01(slamTimePassed / playerMovement.slamCooldown);  //calmp01 makes it between 0 and 1 so when we get more than 1 cooldwon is done. 
                                                                                               //      slamTimePassed = 5, slamCooldown = 10
                                                                                               //       → 5 / 10 = 0.5
                                                                                               //       → So you're halfway through the cooldown.
            slamCooldownSlider.value = slamProgress;  //we have range from 0 to 1.
        }

        //--------------------------------------------------------------- health uI
        if (PointManager.Instance != null && pointsText != null)
        {
            pointsText.text = "" + PointManager.Instance.points;
        }



        // ------------------------------------------------------------------ Magic Station UI

        // Handle Normal Fire Station
        if (fireStation != null && fireMagicText != null)
        {
            float distanceToFireStation = Vector3.Distance(player.position, fireStation.transform.position);

            if (distanceToFireStation <= fireStation.interactionRange)
            {
                if (MagicManager.Instance != null)
                {
                    MagicType currentMagic = MagicManager.Instance.GetCurrentMagicType();

                    if (currentMagic == MagicType.Normal)
                    {
                        fireMagicText.text = "Normal Fireball Equipped";
                    }
                    else
                    {
                        fireMagicText.text = "Press [E] to Equip Normal Fireball";
                    }

                    fireMagicText.gameObject.SetActive(true);
                }
                else
                {
                    fireMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                fireMagicText.gameObject.SetActive(false);
            }
        }

        // Handle Sulfuric Fire Station
        if (sulfuricStation != null && sulfuricFireMagicText != null)
        {
            float distanceToSulfuricStation = Vector3.Distance(player.position, sulfuricStation.transform.position);

            if (distanceToSulfuricStation <= sulfuricStation.interactionRange)
            {
                if (MagicManager.Instance != null)
                {
                    MagicType currentMagic = MagicManager.Instance.GetCurrentMagicType();

                    if (currentMagic == MagicType.Sulfuric)
                    {
                        sulfuricFireMagicText.text = "Sulfuric Fireball Equipped";
                    }
                    else
                    {
                        sulfuricFireMagicText.text = "Press [E] to Equip Sulfuric Fireball";
                    }

                    sulfuricFireMagicText.gameObject.SetActive(true);
                }
                else
                {
                    sulfuricFireMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                sulfuricFireMagicText.gameObject.SetActive(false);
            }
        }
    }

        public void UpdateHealthBar(float value)
                {
                    playerHealthSlider.value = value;
                }
        }
