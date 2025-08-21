using UnityEngine;
using UnityEngine.UI; // for Image
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.EventSystems;
//using Unity.VisualScripting;

[System.Serializable]
public class MagicStationUI
{
    public MagicStation station; // the scene object
    public Text text;            // the UI Text to show
    public MagicType type;       // which magic this station equips
    public string label;         // nice name, e.g. "Crystal"
}


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

    //[Header("Magic Station UI")]
    //public Text fireMagicText;                   
    //public Text crystalMagicText;
    //public Text VoidMagicText;
    //public Text IceMagicText;
    //public Text VenomMagicText;
    //public Text LightningMagicText;
    //public Text WindMagicText;
    //public Text MeteorMagicText;
    //public Text CrimsonMagicText;
    //public MagicStation fireStation;
    //public MagicStation crystalStation;  
    //public MagicStation VoidMagicStation;
    //public MagicStation IceMagicStation;
    //public MagicStation VenomMagicStation;
    //public MagicStation LightningMagicStation;
    //public MagicStation WindMagicStation;
    //public MagicStation MeteorMagicStation;
    //public MagicStation CrimsonMagicStation;
    
    
    [Header("Magic Station UI")]
    public MagicStationUI[] magicStations;
    public Text NoMoneyStatusText;

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

    //-------------------------

    [Header("Perk Bar")]
    public RectTransform perkBar;       // Empty UI object with HorizontalLayoutGroup

    public Image perkIconPrefab;        // Simple Image prefab (no script) a tiny prefab whose root is an Image (no scripts needed). This is what gets cloned for each icon.
    public Sprite speedIcon;
    public Sprite healthIcon;
    public Sprite reviveIcon;
    public Sprite fireRateIcon;
    public Sprite fastHandsIcon;
    public Sprite magicCooldownIcon;

    private readonly List<PerkType> _perkOrder = new List<PerkType>();  //a list that remembers which perks were added and in what order (first → last). Useful if you ever need to read them back in order.
    private readonly Dictionary<PerkType, Image> _activePerkIcons = new Dictionary<PerkType, Image>();  //map/dict so we can perkkType --> img assign to that perk. 
    
    public Text fireRatePerkText;
    public Text speedPerkText;
    public Text healthPerkText;
    public Text revivePerkText;
    public Text fastHandsPerkText;
    public Text magicCooldownPerkText;

    //--------------------------
    [Header("Pack A Punch")]
    public Text packAPunchText;           // Text to show Pack-A-Punch prompts
    public PackAPunch packAPunchMachine;

    // timed power-up UI
    private Coroutine powerupTimerCo;
    public RectTransform statusStackRoot;
    public Text statusRowPrefab;

    private readonly System.Collections.Generic.Dictionary<string, UnityEngine.UI.Text> _timedRows =
    new System.Collections.Generic.Dictionary<string, UnityEngine.UI.Text>();
    private readonly System.Collections.Generic.Dictionary<string, Coroutine> _timedCoroutines =
        new System.Collections.Generic.Dictionary<string, Coroutine>();

    [Header("Infusion UI")]
    public GameObject infusePanel;
    public Text infusePromptText;
    public InfuseStation currentInfuseStation;
    private bool infusePanelOpen = false;
    public Text infuseStatusText;
    [HideInInspector] Weapon currentWeapon;  //we need for cur weapon.

    void Start()
    {
        magicManager = FindFirstObjectByType<MagicManager>();  // Find it once at start
        grenadeManager = FindFirstObjectByType<GrenadeManager>();  
        arm = FindFirstObjectByType<ArmMovementMegaScript>();

        if (grenadePanel) grenadePanel.SetActive(false); //set panel to false off rip
        if (grenadePrompt) grenadePrompt.gameObject.SetActive(false);  //set text to false off rip

        if (infusePanel) infusePanel.SetActive(false);
        if (infusePromptText) infusePromptText.gameObject.SetActive(false);

        if (infuseStatusText) infuseStatusText.text = "";   //start empty


    }

    //bool chestPanelOpen = false;

    private string GetWeaponDisplayName(Weapon weapon)
    {
        if (weapon.upgradeLevel <= 0)
        {
            return weapon.weaponName; // Normal weapon name
        }
        else
        {
            string tierName = GetTierSuffix(weapon.upgradeLevel);
            return $"{weapon.weaponName} {tierName}";
        }
    }

    private string GetTierSuffix(int level)
    {
        switch (level)
        {
            case 1: return "Tier I";
            case 2: return "Tier II";
            case 3: return "Tier III";
            default: return "Max Tier";
        }
    }

    void Update()
    {
        Weapon currentWeapon = WeaponManager.ActiveWeapon;

        //--------------------------------------------------------------- Weapon UI

        if (currentWeapon != null)
        {
            WeaponAmmoText.text = currentWeapon.GetCurrentAmmo() + " / " + currentWeapon.GetAmmoReserve();
            WeaponNameText.text = GetWeaponDisplayName(currentWeapon);
        }
        else
        {
            WeaponAmmoText.text = "-- / --";
            WeaponNameText.text = "No Weapon";
        }

        //--------------------------------------------------------------- Mystery Box UI
         
        const int BOX_COST = 950;  // always 950 points

        float distanceToBox = Vector3.Distance(player.position, mysteryBox.transform.position);   //we check distance from player and box

        bool PlayerIsCloseCanOpenBox = !mysteryBox.IsBoxOpen() && distanceToBox <= mysteryBox.minimumDistanceToOpen;      //box closed + close
        bool PlayerIsCloseCanGrabWeapon = mysteryBox.IsBoxOpen() && distanceToBox <= mysteryBox.minimumDistanceToOpen;   //box open  + close

        if (PlayerIsCloseCanOpenBox)
        {
            int pts = (PointManager.Instance != null) ? PointManager.Instance.GetPoints() : 0;

            if (pts < BOX_COST)
            {
                // not enough → show text + optional toast
                MysteryBoxText.text = "Not enough points";
                MysteryBoxText.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                    ShowTemporaryPerkMessage("Not enough points");
            }
            else
            {
                // enough → just show buy prompt; DO NOT subtract here
                MysteryBoxText.text = "Press [E] to Open Weapon Box 950 Points";
                MysteryBoxText.gameObject.SetActive(true);
            }
        }
        else if (PlayerIsCloseCanGrabWeapon && mysteryBox.GetCurrentPreview() != null)
        {
            Weapon weapon = mysteryBox.GetCurrentPreview().GetComponent<Weapon>();   //so we get the weapon adn than grab the Weapon.cs script 
            string weaponName = (weapon != null) ? weapon.weaponName : "Unknown";    //if we find weapon script use that name if we cant use unkown
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

        //// Handle Normal Fire Station
        //if (fireStation != null && fireMagicText != null)
        //{
        //    float distanceToFireStation = Vector3.Distance(player.position, fireStation.transform.position);   //calc how close the player adn station

        //    if (distanceToFireStation <= fireStation.interactionRange)    //is it less than or equal the distanec ( we in range ?) 
        //    {
        //        if (magicManager != null)  // Using direct reference instead of Instance
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Normal)
        //            {
        //                fireMagicText.text = "Normal Fireball Equipped";
        //            }
        //            else
        //            {
        //                fireMagicText.text = "Press [E] to Equip Normal Fireball";
        //            }

        //            fireMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            fireMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        fireMagicText.gameObject.SetActive(false);
        //    }
        //}

        //// Handle Sulfuric Fire Station
        //if (crystalStation != null && crystalMagicText != null)
        //{
        //    float distanceToCrystalStation = Vector3.Distance(player.position, crystalStation.transform.position);

        //    if (distanceToCrystalStation <= crystalStation.interactionRange)
        //    {
        //        if (magicManager != null)  // Using direct reference instead of Instance
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Crystal)
        //            {
        //                crystalMagicText.text = "Crystal Magic Equipped";
        //            }
        //            else
        //            {
        //                crystalMagicText.text = "Press [E] to Equip Crystal Magic";
        //            }

        //            crystalMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            crystalMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        crystalMagicText.gameObject.SetActive(false);
        //    }
        //}

        //// Handle void magic
        //if (VoidMagicStation != null && VoidMagicText != null)
        //{
        //    float distanceToVoidMagicStation = Vector3.Distance(player.position, VoidMagicStation.transform.position);

        //    if (distanceToVoidMagicStation <= VoidMagicStation.interactionRange)
        //    {
        //        if (magicManager != null)
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Void)
        //            {
        //                VoidMagicText.text = "Void Magic Equipped";
        //            }
        //            else
        //            {
        //                VoidMagicText.text = "Press [E] to Equip Void Magic";
        //            }

        //            VoidMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            VoidMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        VoidMagicText.gameObject.SetActive(false);
        //    }
        //}

        //// Handle ice magic
        //if (IceMagicStation != null && IceMagicText != null)
        //{
        //    float distanceToIceMagicStation = Vector3.Distance(player.position, IceMagicStation.transform.position);

        //    if (distanceToIceMagicStation <= IceMagicStation.interactionRange)
        //    {
        //        if (magicManager != null)
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Ice)
        //            {
        //                IceMagicText.text = "Ice Magic Equipped";
        //            }
        //            else
        //            {
        //                IceMagicText.text = "Press [E] to Equip Ice Magic";
        //            }

        //            IceMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            IceMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        IceMagicText.gameObject.SetActive(false);
        //    }
        //}

        //// Handle Venom magic
        //if (VenomMagicStation != null && VenomMagicText != null)
        //{
        //    float distanceToVenomMagicStation = Vector3.Distance(player.position, VenomMagicStation.transform.position);

        //    if (distanceToVenomMagicStation <= VenomMagicStation.interactionRange)
        //    {
        //        if (magicManager != null)
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Venom)
        //            {
        //                VenomMagicText.text = "Venom Magic Equipped";
        //            }
        //            else
        //            {
        //                VenomMagicText.text = "Press [E] to Equip Venom Magic";
        //            }

        //            VenomMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            VenomMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        VenomMagicText.gameObject.SetActive(false);
        //    }
        //}

        //// Handle Lightining magic
        //if (LightningMagicStation != null && LightningMagicText != null)
        //{
        //    float distanceToLightningMagicStation = Vector3.Distance(player.position, LightningMagicStation.transform.position);

        //    if (distanceToLightningMagicStation <= LightningMagicStation.interactionRange)
        //    {
        //        if (magicManager != null)
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Lightning)
        //            {
        //                LightningMagicText.text = "Lightning Magic Equipped";
        //            }
        //            else
        //            {
        //                LightningMagicText.text = "Press [E] to Equip Lightning Magic";
        //            }

        //            LightningMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            LightningMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        LightningMagicText.gameObject.SetActive(false);
        //    }
        //}

        //// Handle Wind magic
        //if (WindMagicStation != null && WindMagicText != null)
        //{
        //    float distanceToWindMagicStation = Vector3.Distance(player.position, WindMagicStation.transform.position);

        //    if (distanceToWindMagicStation <= WindMagicStation.interactionRange)
        //    {
        //        if (magicManager != null)
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Wind)
        //            {
        //                WindMagicText.text = "Wind Magic Equipped";
        //            }
        //            else
        //            {
        //                WindMagicText.text = "Press [E] to Equip Wind Magic";
        //            }

        //            WindMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            WindMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        WindMagicText.gameObject.SetActive(false);
        //    }
        //}

        //// Handle Meteor magic
        //if (MeteorMagicStation != null && MeteorMagicText != null)
        //{
        //    float distanceToMeteorMagicStation = Vector3.Distance(player.position, MeteorMagicStation.transform.position);

        //    if (distanceToMeteorMagicStation <= MeteorMagicStation.interactionRange)
        //    {
        //        if (magicManager != null)
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Meteor)
        //            {
        //                MeteorMagicText.text = "Meteor Magic Equipped";
        //            }
        //            else
        //            {
        //                MeteorMagicText.text = "Press [E] to Equip Meteor Magic";
        //            }

        //            MeteorMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            MeteorMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        MeteorMagicText.gameObject.SetActive(false);
        //    }
        //}

        //// Handle Crimson magic
        //if (CrimsonMagicStation != null && CrimsonMagicText != null)
        //{
        //    float distanceToCrimsonMagicStation = Vector3.Distance(player.position, CrimsonMagicStation.transform.position);

        //    if (distanceToCrimsonMagicStation <= CrimsonMagicStation.interactionRange)
        //    {
        //        if (magicManager != null)
        //        {
        //            MagicType currentMagic = magicManager.GetCurrentMagicType();

        //            if (currentMagic == MagicType.Crimson)
        //            {
        //                CrimsonMagicText.text = "Crimson Magic Equipped";
        //            }
        //            else
        //            {
        //                CrimsonMagicText.text = "Press [E] to Equip Crimson Magic";
        //            }

        //            CrimsonMagicText.gameObject.SetActive(true);
        //        }
        //        else
        //        {
        //            CrimsonMagicText.gameObject.SetActive(false);
        //        }
        //    }
        //    else
        //    {
        //        CrimsonMagicText.gameObject.SetActive(false);
        //    }
        //}

        UpdateMagicPrompts();

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
        // Example for 4 perks:

        HandlePerkStationUI(
            FindFirstObjectByType<MoreFireRatePerk>(),
            fireRatePerkText,
            "Faster Fire Rate",
            FindFirstObjectByType<MoreFireRatePerk>() != null && FindFirstObjectByType<MoreFireRatePerk>().hasMoreFireRatePerk,
            FindFirstObjectByType<MoreFireRatePerk>() != null ? FindFirstObjectByType<MoreFireRatePerk>().cost : 0
        );

        HandlePerkStationUI(
            FindFirstObjectByType<MoreSpeedPerk>(),
            speedPerkText,
            "More Running Speed",
            FindFirstObjectByType<MoreSpeedPerk>() != null && FindFirstObjectByType<MoreSpeedPerk>().hasMoreSpeedPerk,
            FindFirstObjectByType<MoreSpeedPerk>() != null ? FindFirstObjectByType<MoreSpeedPerk>().cost : 0
        );

        HandlePerkStationUI(
            FindFirstObjectByType<MoreHealthPerk>(),
            healthPerkText,
            "More Health",
            FindFirstObjectByType<MoreHealthPerk>() != null && FindFirstObjectByType<MoreHealthPerk>().hasMoreHealthPerk,
            FindFirstObjectByType<MoreHealthPerk>() != null ? FindFirstObjectByType<MoreHealthPerk>().cost : 0
        );

        HandlePerkStationUI(
            FindFirstObjectByType<MoreRevivePerk>(),
            revivePerkText,
            "Fast Revive",
            FindFirstObjectByType<MoreRevivePerk>() != null && FindFirstObjectByType<MoreRevivePerk>().hasMoreRevivePerk,
            FindFirstObjectByType<MoreRevivePerk>() != null ? FindFirstObjectByType<MoreRevivePerk>().cost : 0
        );
        HandlePerkStationUI(
            FindFirstObjectByType<MagicCooldownPerk>(),
            magicCooldownPerkText,
            "Faster Magic Cooldown",
            FindFirstObjectByType<MagicCooldownPerk>()?.hasMagicCooldownPerk ?? false,
            FindFirstObjectByType<MagicCooldownPerk>()?.cost ?? 0
        );

        if (packAPunchMachine != null && packAPunchText != null)
        {
            string promptText = packAPunchMachine.GetPromptText();

            if (string.IsNullOrEmpty(promptText))
            {
                packAPunchText.gameObject.SetActive(false);
            }
            else
            {
                packAPunchText.text = promptText;
                packAPunchText.gameObject.SetActive(true);
            }
        }

        HandleInfuseStationUI();




    }

    //-----------------------MAGIC prompt text
    void UpdateMagicPrompts()
    {
        if (magicStations == null || player == null || magicManager == null) return;  //get out if we dont have one of these

        int points = (PointManager.Instance != null) ? PointManager.Instance.GetPoints() : 0;    //grab the player’s current points once - if dont have points assume its 0

        foreach (var e in magicStations) //loop through all stations 
        {
            if (e == null || e.station == null || e.text == null) continue;   //skip any incomplete entry so we don’t crash

            float dist = Vector3.Distance(player.position, e.station.transform.position);  //find position from station to player
            bool inRange = dist <= e.station.interactionRange;  //in range bc we see dist and check if were within the interaction range

            if (!inRange)
            {
                e.text.gameObject.SetActive(false);         //turn off text when we aint in range
                continue;
            }

            e.text.gameObject.SetActive(true);      //show the prompt, then decide what it should say

            bool equipped = (magicManager.GetCurrentMagicType() == e.type); //if already equipped
            if (equipped)
            {
                e.text.text = $"{e.label} Magic Equipped";
            }
            else
            {
                int cost = e.station.cost;          //if statement to see if you can afford
                e.text.text = (points < cost)
                    ? $"Need {cost} Points for {e.label} Magic"
                    : $"Press [E] to buy {e.label} Magic ({cost} Points)";
            }

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

        if (statusStackRoot) statusStackRoot.gameObject.SetActive(false);   //so you dont see drops when your in the menu
    }

    public bool IsGrenadePanelOpen => grenadePanelOpen;   //this is for in weapon so we can not shoot when panel is open  -- getter

    void CloseGrenadePanel()
    {
        grenadePanelOpen = false;           //we turn eveyrhting off
        grenadePanel.SetActive(false);
        grenadePrompt.gameObject.SetActive(false);  // still hide after close

        Cursor.lockState = CursorLockMode.Locked;    //return back to normal ingame mouse
        Cursor.visible = false;

        var cam = FindFirstObjectByType<CameraScript>();        //fethc cam script
        if (cam) cam.cameraLocked = false;   //were enable cam movment by setting this varibale as false so we can move around in game with camera -- look at cam script we put this eveyrwhere

        if (statusStackRoot) statusStackRoot.gameObject.SetActive(true);  //so you can see drops if active

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

    //-------------------------- perk ICON logic
    public void ShowPerkIcon(PerkType type)
    {
        if (perkBar == null || perkIconPrefab == null) return;          
        if (_activePerkIcons.ContainsKey(type)) return;                 

        Sprite s = GetPerkSprite(type);
        if (s == null) return;

        Image img = Instantiate(perkIconPrefab, perkBar);
        img.sprite = s;
        img.SetNativeSize();                                            // optional
                                                                        // optional: clamp size
        var rt = img.rectTransform;
        rt.sizeDelta = new Vector2(24, 24);

        // pop-in anim
        StartCoroutine(PopIn(img));

        _activePerkIcons[type] = img;
        _perkOrder.Add(type);                                           // chronological order
    }

    public void RemovePerkIcon(PerkType type)
    {
        if (_activePerkIcons.TryGetValue(type, out var img) && img != null)
        {
            Destroy(img.gameObject);
        }
        _activePerkIcons.Remove(type);
        _perkOrder.Remove(type);
    }

    private Sprite GetPerkSprite(PerkType type)
    {
        switch (type)
        {
            case PerkType.Speed: return speedIcon;
            case PerkType.Health: return healthIcon;
            case PerkType.Revive: return reviveIcon;
            case PerkType.FireRate: return fireRateIcon;
            case PerkType.FastHands: return fastHandsIcon;
            case PerkType.MagicCooldown: return magicCooldownIcon;


            default: return null;
        }
    }

    private IEnumerator PopIn(Image img)
    {
        if (img == null) yield break;
        CanvasGroup cg = img.GetComponent<CanvasGroup>();
        if (cg == null) cg = img.gameObject.AddComponent<CanvasGroup>();

        float t = 0f, dur = 0.2f;
        cg.alpha = 0f;
        img.rectTransform.localScale = Vector3.one * 0.6f;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            float s = Mathf.Lerp(0.6f, 1f, t);
            img.rectTransform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
    }

    //----- Perk UI
    void HandlePerkStationUI(MonoBehaviour perkObj, UnityEngine.UI.Text uiText, string perkName, bool hasPerk, int cost)
    {
        if (uiText == null) return;             //if we dont have the text get out

        if (perkObj == null || player == null)      //capturte script and the player
        {
            uiText.gameObject.SetActive(false);     //turn off text ui if no script or player 
            return;
        }

        const float InteractionRange = 3f;     //determine the distance (interationRange)
            
        float dist = Vector3.Distance(player.position, perkObj.transform.position);  //find distance between player and the player
        if (dist > InteractionRange)        //if your not in range than make text ui false
        {
            uiText.gameObject.SetActive(false);
            return;
        }

        uiText.gameObject.SetActive(true);             //if you are than showcase that text 

        if (hasPerk)
        {
            uiText.text = $"{perkName} Perk Owned";     //if you already have that perk than show
            return;
        }

        PointManager pm = FindFirstObjectByType<PointManager>();
        int points = (pm != null) ? pm.GetPoints() : 0;   //get the points if you dont have a script assume its 0 

        if (points < cost)          
            uiText.text = $"Need {cost} pts for {perkName} Perk";
        else
            uiText.text = $"Press [E] to buy {perkName} Perk ({cost} pts)";

    }


    public void ShowTemporaryPerkMessage(string message)
    {
        StartCoroutine(ShowPerkMessageRoutine(message));
    }

    private IEnumerator ShowPerkMessageRoutine(string message)
    {
        Text t = (NoMoneyStatusText != null) ? NoMoneyStatusText : magicStatusText;
        if (t == null) yield break;

        t.text = message;
        t.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        t.gameObject.SetActive(false);
    }

    // ---------------- Drops text
    public void ShowToast(string message, float seconds = 1.5f)
    {
        // fallback to your old single text if stack not wired yet
        if (statusStackRoot == null || statusRowPrefab == null)
        {
            ShowTemporaryMagicMessage(message);
            return;
        }

        var row = Instantiate(statusRowPrefab, statusStackRoot);
        row.text = message;
        StartCoroutine(DestroyRowAfter(row, seconds));
    }

    private System.Collections.IEnumerator DestroyRowAfter(UnityEngine.UI.Text row, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (row != null) Destroy(row.gameObject);
    }

    // Timed badge that updates every frame (e.g., "INSTA-KILL (25s)"), one row per key.
    // If you call again with same key, it refreshes the same row instead of adding a duplicate.
    public void ShowTimedPowerup(string key, string label, float durationSeconds)
    {
        if (statusStackRoot == null || statusRowPrefab == null)
        {
            ShowTemporaryMagicMessage(label);
            return;
        }

        if (_timedCoroutines.TryGetValue(key, out var co) && co != null)
            StopCoroutine(co);

        if (!_timedRows.TryGetValue(key, out var row) || row == null)
        {
            row = Instantiate(statusRowPrefab, statusStackRoot);
            _timedRows[key] = row;
        }

        _timedCoroutines[key] = StartCoroutine(TimedPowerupRoutine(key, row, label, durationSeconds));
    }

    private System.Collections.IEnumerator TimedPowerupRoutine(string key, UnityEngine.UI.Text row, string label, float durationSeconds)
    {
        float end = Time.time + Mathf.Max(0f, durationSeconds);
        row.gameObject.SetActive(true);

        while (Time.time < end)
        {
            int secs = Mathf.CeilToInt(end - Time.time);
            row.text = $"{label} ({secs}s)";
            yield return null;
        }

        // cleanup
        if (_timedCoroutines.ContainsKey(key)) _timedCoroutines.Remove(key);
        if (_timedRows.ContainsKey(key)) _timedRows.Remove(key);
        if (row != null) Destroy(row.gameObject);
    }


    //---------------------------------INFUSING --------------------------- 
    void HandleInfuseStationUI()
    {
        if (infusePanelOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
                CloseInfusePanel();
            return;
        }

        InfuseStation[] allStations = FindObjectsOfType<InfuseStation>();
        foreach (var station in allStations)
        {
            float dist = Vector3.Distance(player.position, station.transform.position);
            if (dist <= station.interactDistance)
            {
                currentInfuseStation = station;
                infusePromptText.text = $"Press [E] to OPEN Infuse Station";
                infusePromptText.gameObject.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                    OpenInfusePanel();
                return;
            }
        }

        // Not near any
        infusePromptText.gameObject.SetActive(false);
        currentInfuseStation = null;
    }
    void OpenInfusePanel()
    {
        infusePanelOpen = true;
        infusePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var cam = FindFirstObjectByType<CameraScript>();
        if (cam) cam.cameraLocked = true;

        if (statusStackRoot) statusStackRoot.gameObject.SetActive(false);
    }

    public bool IsInfusePanelOpen => infusePanelOpen;


    void CloseInfusePanel()
    {
        infusePanelOpen = false;
        infusePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var cam = FindFirstObjectByType<CameraScript>();
        if (cam) cam.cameraLocked = false;

        if (statusStackRoot) statusStackRoot.gameObject.SetActive(true);
    }

    public void SetCurrentWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        if (infuseStatusText != null)
        {
            // Check the actual InfusionType enum instead of the string
            if (weapon.infusion != InfusionType.None)
            {
                infuseStatusText.text = $"{weapon.infusion} Magic Infused!";
            }
            else if (!string.IsNullOrEmpty(weapon.infusedElement))
            {
                // Fallback to string version if enum is None but string exists
                infuseStatusText.text = $"{weapon.infusedElement} Magic Infused!";
            }
            else
            {
                infuseStatusText.text = "";
            }
        }
    }

    public void OnClickInfuseFire()
    {
        InfuseWith(InfusionType.Fire);
    }

    public void OnClickInfuseIce()
    {
        InfuseWith(InfusionType.Ice);
    }

    public void OnClickInfuseVenom()
    {
        InfuseWith(InfusionType.Venom);
    }

    public void OnClickInfuseLightning()
    {
        InfuseWith(InfusionType.Lightning);
    }

    public void OnClickInfuseWind()
    {
        InfuseWith(InfusionType.Wind);
    }

    public void OnClickInfuseMeteor()
    {
        InfuseWith(InfusionType.Meteor);
    }

    public void OnClickInfuseCrimson()
    {
        InfuseWith(InfusionType.Crimson);
    }

    public void OnClickInfuseVoid()
    {
        InfuseWith(InfusionType.Void);
    }

    public void OnClickInfuseCrystal()
    {
        InfuseWith(InfusionType.Crystal);
    }

    public void InfuseWith(InfusionType type)
    {
        if (currentWeapon != null)
        {
            // Set both the enum and string for consistency
            currentWeapon.SetInfusion(type);
            currentWeapon.SetInfusedElement(type.ToString());

            if (infuseStatusText != null)
                infuseStatusText.text = $"{type} Magic Infused!";
        }
        else
        {
            Debug.LogWarning("No current weapon found to infuse.");
        }
        CloseInfusePanel();
    }
}



