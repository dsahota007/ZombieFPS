using UnityEngine;
using UnityEngine.UI; // for Image
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;


public class UI : MonoBehaviour
{
    public Transform player;            //we fetch player like this and not script in start like the magic manager.
    private ArmMovementMegaScript arm;       // we’ll assign grenade prefab on this
    private MagicManager magicManager;      // Direct reference instead of Instance
    private GrenadeManager grenadeManager;


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
    public Text fireMagicText;                   
    public Text crystalMagicText;
    public Text VoidMagicText;
    public Text IceMagicText;
    public Text VenomMagicText;
    public Text LightningMagicText;
    public Text WindMagicText;
    public Text MeteorMagicText;
    public Text CrimsonMagicText;
    public MagicStation fireStation;
    public MagicStation crystalStation;  
    public MagicStation VoidMagicStation;
    public MagicStation IceMagicStation;
    public MagicStation VenomMagicStation;
    public MagicStation LightningMagicStation;
    public MagicStation WindMagicStation;
    public MagicStation MeteorMagicStation;
    public MagicStation CrimsonMagicStation;

     

    [Header("Magic Cooldown UI")]
    public Slider magicCooldownSlider;
    public Text magicStatusText;
 
    [Header("Grenade Chest UI")]
 
    public GrenadeChest grenadeChest;   
    public Text grenadePrompt;     // "Press [E] to open"
    public GameObject grenadePanel;
    public Text grenadeAmountText;
    public Text grenadeStatusText;
    private Coroutine grenadeMsgCo;
    private bool grenadePanelOpen = false;
 

    void Start()
    {
        magicManager = FindFirstObjectByType<MagicManager>();  // Find it once at start
        grenadeManager = FindFirstObjectByType<GrenadeManager>();  
        arm = FindFirstObjectByType<ArmMovementMegaScript>();

        if (grenadePanel) grenadePanel.SetActive(false); //set panel to false off rip
        if (grenadePrompt) grenadePrompt.gameObject.SetActive(false);  //set text to false off rip
    }

    //bool chestPanelOpen = false;

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

        // ------------------------------------------------------------------ Magic Station UI

        // Handle Normal Fire Station
        if (fireStation != null && fireMagicText != null)
        {
            float distanceToFireStation = Vector3.Distance(player.position, fireStation.transform.position);   //calc how close the player adn station

            if (distanceToFireStation <= fireStation.interactionRange)    //is it less than or equal the distanec ( we in range ?) 
            {
                if (magicManager != null)  // Using direct reference instead of Instance
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

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
        if (crystalStation != null && crystalMagicText != null)
        {
            float distanceToCrystalStation = Vector3.Distance(player.position, crystalStation.transform.position);

            if (distanceToCrystalStation <= crystalStation.interactionRange)
            {
                if (magicManager != null)  // Using direct reference instead of Instance
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

                    if (currentMagic == MagicType.Crystal)
                    {
                        crystalMagicText.text = "Crystal Magic Equipped";
                    }
                    else
                    {
                        crystalMagicText.text = "Press [E] to Equip Crystal Magic";
                    }

                    crystalMagicText.gameObject.SetActive(true);
                }
                else
                {
                    crystalMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                crystalMagicText.gameObject.SetActive(false);
            }
        }

        // Handle void magic
        if (VoidMagicStation != null && VoidMagicText != null)
        {
            float distanceToVoidMagicStation = Vector3.Distance(player.position, VoidMagicStation.transform.position);

            if (distanceToVoidMagicStation <= VoidMagicStation.interactionRange)
            {
                if (magicManager != null)
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

                    if (currentMagic == MagicType.Void)
                    {
                        VoidMagicText.text = "Void Magic Equipped";
                    }
                    else
                    {
                        VoidMagicText.text = "Press [E] to Equip Void Magic";
                    }

                    VoidMagicText.gameObject.SetActive(true);
                }
                else
                {
                    VoidMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                VoidMagicText.gameObject.SetActive(false);
            }
        }

        // Handle ice magic
        if (IceMagicStation != null && IceMagicText != null)
        {
            float distanceToIceMagicStation = Vector3.Distance(player.position, IceMagicStation.transform.position);

            if (distanceToIceMagicStation <= IceMagicStation.interactionRange)
            {
                if (magicManager != null)
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

                    if (currentMagic == MagicType.Ice)
                    {
                        IceMagicText.text = "Ice Magic Equipped";
                    }
                    else
                    {
                        IceMagicText.text = "Press [E] to Equip Ice Magic";
                    }

                    IceMagicText.gameObject.SetActive(true);
                }
                else
                {
                    IceMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                IceMagicText.gameObject.SetActive(false);
            }
        }

        // Handle Venom magic
        if (VenomMagicStation != null && VenomMagicText != null)
        {
            float distanceToVenomMagicStation = Vector3.Distance(player.position, VenomMagicStation.transform.position);

            if (distanceToVenomMagicStation <= VenomMagicStation.interactionRange)
            {
                if (magicManager != null)
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

                    if (currentMagic == MagicType.Venom)
                    {
                        VenomMagicText.text = "Venom Magic Equipped";
                    }
                    else
                    {
                        VenomMagicText.text = "Press [E] to Equip Venom Magic";
                    }

                    VenomMagicText.gameObject.SetActive(true);
                }
                else
                {
                    VenomMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                VenomMagicText.gameObject.SetActive(false);
            }
        }

        // Handle Lightining magic
        if (LightningMagicStation != null && LightningMagicText != null)
        {
            float distanceToLightningMagicStation = Vector3.Distance(player.position, LightningMagicStation.transform.position);

            if (distanceToLightningMagicStation <= LightningMagicStation.interactionRange)
            {
                if (magicManager != null)
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

                    if (currentMagic == MagicType.Lightning)
                    {
                        LightningMagicText.text = "Lightning Magic Equipped";
                    }
                    else
                    {
                        LightningMagicText.text = "Press [E] to Equip Lightning Magic";
                    }

                    LightningMagicText.gameObject.SetActive(true);
                }
                else
                {
                    LightningMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                LightningMagicText.gameObject.SetActive(false);
            }
        }

        // Handle Wind magic
        if (WindMagicStation != null && WindMagicText != null)
        {
            float distanceToWindMagicStation = Vector3.Distance(player.position, WindMagicStation.transform.position);

            if (distanceToWindMagicStation <= WindMagicStation.interactionRange)
            {
                if (magicManager != null)
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

                    if (currentMagic == MagicType.Wind)
                    {
                        WindMagicText.text = "Wind Magic Equipped";
                    }
                    else
                    {
                        WindMagicText.text = "Press [E] to Equip Wind Magic";
                    }

                    WindMagicText.gameObject.SetActive(true);
                }
                else
                {
                    WindMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                WindMagicText.gameObject.SetActive(false);
            }
        }

        // Handle Meteor magic
        if (MeteorMagicStation != null && MeteorMagicText != null)
        {
            float distanceToMeteorMagicStation = Vector3.Distance(player.position, MeteorMagicStation.transform.position);

            if (distanceToMeteorMagicStation <= MeteorMagicStation.interactionRange)
            {
                if (magicManager != null)
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

                    if (currentMagic == MagicType.Meteor)
                    {
                        MeteorMagicText.text = "Meteor Magic Equipped";
                    }
                    else
                    {
                        MeteorMagicText.text = "Press [E] to Equip Meteor Magic";
                    }

                    MeteorMagicText.gameObject.SetActive(true);
                }
                else
                {
                    MeteorMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                MeteorMagicText.gameObject.SetActive(false);
            }
        }

        // Handle Crimson magic
        if (CrimsonMagicStation != null && CrimsonMagicText != null)
        {
            float distanceToCrimsonMagicStation = Vector3.Distance(player.position, CrimsonMagicStation.transform.position);

            if (distanceToCrimsonMagicStation <= CrimsonMagicStation.interactionRange)
            {
                if (magicManager != null)
                {
                    MagicType currentMagic = magicManager.GetCurrentMagicType();

                    if (currentMagic == MagicType.Crimson)
                    {
                        CrimsonMagicText.text = "Crimson Magic Equipped";
                    }
                    else
                    {
                        CrimsonMagicText.text = "Press [E] to Equip Crimson Magic";
                    }

                    CrimsonMagicText.gameObject.SetActive(true);
                }
                else
                {
                    CrimsonMagicText.gameObject.SetActive(false);
                }
            }
            else
            {
                CrimsonMagicText.gameObject.SetActive(false);
            }
        }

        //--------------------------------------------------Magic Cooldown

        // --- Magic Cooldown UI ---
        if (magicManager != null && magicCooldownSlider != null)
        {
            magicCooldownSlider.value = magicManager.GetCooldownProgress01(); // 0..1
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

        //------grenade logic

        HandleGrenadeChestUI();

        if (grenadeManager != null && grenadeAmountText != null)
        {
            var t = grenadeManager.currentType;
            int have = grenadeManager.GetCount(t);
            int cap = grenadeManager.GetCap(t);
            grenadeAmountText.text = have + " / " + cap;   // e.g. "4 / 6"
        }


    }


    //------------------------------------ grenade logic
    void HandleGrenadeChestUI()
    {
        //if (grenadePanel == null || grenadePrompt == null || player == null || grenadeChest == null)
        //    return;  

        // if panel is open, force-hide the prompt and listen for close keys
        if (grenadePanelOpen)
        {
            //if (grenadePrompt.gameObject.activeSelf)  
            //    grenadePrompt.gameObject.SetActive(false);

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
                CloseGrenadePanel();
            return;
        }

        bool inRange = Vector3.Distance(player.position, grenadeChest.transform.position) <= grenadeChest.interactDistance;        //return true if we are in distance

        grenadePrompt.gameObject.SetActive(inRange);  //set active based on teh range so it wil be true if were in range bc its a bool

        if (inRange && Input.GetKeyDown(KeyCode.E))
            OpenGrenadePanel();  
    }

    void OpenGrenadePanel()
    {
        grenadePanelOpen = true;        
        grenadePanel.SetActive(true);
        grenadePrompt.gameObject.SetActive(false);  // hide prompt

        Cursor.lockState = CursorLockMode.None;         //turn the mouse on so we cam actauly select the grenade panel 
        Cursor.visible = true;

        var cam = FindFirstObjectByType<CameraScript>();  //fethc cam script
        if (cam) cam.cameraLocked = true;    //were disablign movment by setting this varibale as true so we can move around in teh menu -- look at cam script we put this eveyrwhere
    }

    void CloseGrenadePanel()
    {
        grenadePanelOpen = false;           //we turn eveyrhting off
        grenadePanel.SetActive(false);
        grenadePrompt.gameObject.SetActive(false);  // still hide after close

        Cursor.lockState = CursorLockMode.Locked;    //return back to normal ingame mouse
        Cursor.visible = false;

        var cam = FindFirstObjectByType<CameraScript>();        //fethc cam script
        if (cam) cam.cameraLocked = false;   //were enable cam movment by setting this varibale as false so we can move around in game with camera -- look at cam script we put this eveyrwhere
    }

    //--- Grenade type Dictionary setting
    public void OnPickFrag()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();  //fetch grenadeManager Script
        if (gm) gm.SetType(GrenadeType.Frag);                //set the key 
        CloseGrenadePanel();                                //close panel
    }

    public void OnPickImpact()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();
        if (gm) gm.SetType(GrenadeType.Impact);
        CloseGrenadePanel();
    }

    public void OnPickSemtex()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();
        if (gm) gm.SetType(GrenadeType.Semtex);
        CloseGrenadePanel();
    }

    public void OnPickBio()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();
        if (gm) gm.SetType(GrenadeType.Bio);
        CloseGrenadePanel();
    }

    public void OnPickSulfuricNapalm()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();
        if (gm) gm.SetType(GrenadeType.SulfuricNapalm);
        CloseGrenadePanel();
    }

    public void OnPickCrystalCluster()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();
        if (gm) gm.SetType(GrenadeType.CrystalCluster);
        CloseGrenadePanel();
    }

    public void OnPickBastion()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();
        if (gm) gm.SetType(GrenadeType.Bastion);
        CloseGrenadePanel();
    }

    public void OnPickRagnarok()
    {
        var gm = FindFirstObjectByType<GrenadeManager>();
        if (gm) gm.SetType(GrenadeType.Ragnarok);
        CloseGrenadePanel();
    }

    public void ShowTemporaryGrenadeMessage(string message)
    {
        if (grenadeStatusText == null) return;              //if you alerady have the text GTFO this code
        if (grenadeMsgCo != null) StopCoroutine(grenadeMsgCo);      
        grenadeMsgCo = StartCoroutine(GrenadeMsgRoutine(message));  //start the message for a quick sec
    }

    private IEnumerator GrenadeMsgRoutine(string message)
    {
        grenadeStatusText.text = message;           //we pu rmessage into string of what we want to say 
        grenadeStatusText.gameObject.SetActive(true);       //set it true
        yield return new WaitForSeconds(1.2f);              //show for only this many seconds
        grenadeStatusText.gameObject.SetActive(false);          //turn it off
        grenadeMsgCo = null;
    }


    //-------------------------MAGIC functions

    public void ShowTemporaryMagicMessage(string message)
    {
        StopAllCoroutines(); // cancel any old message timers
        StartCoroutine(ShowMagicMessageRoutine(message));
    }

    private IEnumerator ShowMagicMessageRoutine(string message)
    {
        magicStatusText.text = message;
        magicStatusText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f); // how long to show

        magicStatusText.gameObject.SetActive(false);
    }
    //-------------------------health functions
    public void UpdateHealthBar(float value)
    {
        playerHealthSlider.value = value;
    }
}
