using UnityEngine;
//using System.Collections;

public enum MagicType
{
    None,
    Normal,
    Crystal,
    Void,
    Ice,
    Venom,
    Lightning,
    Wind,
    Meteor,
    Crimson
}

[System.Serializable] //show up in inspector
public class MagicData
{
    public GameObject fireballPrefab;
    public GameObject handFireVFX;
}

public class MagicManager : MonoBehaviour
{
    [Header("Magic Types")]
    public MagicData normalMagic;
    public MagicData crystalcMagic;
    public MagicData VoidMagic;
    public MagicData IceMagic;
    public MagicData VenomMagic;
    public MagicData LightningMagic;
    public MagicData WindMagic;
    public MagicData MeteorMagic;
    public MagicData CrimsonMagic;

    private MagicType currentMagicType = MagicType.None;  // Start with no magic!
    private ArmMagicSpell armMagicSpell;                  // Reference to your casting script

    [Header("Cooldown")]
    public float cooldown = 8f;     // seconds until next cast
    private float lastCastTime;     // when we last started a cast

    public bool IsReady() => Time.time >= lastCastTime + EffectiveCooldown;  //+ cooldown;  //calc till ur cooldown ready
    public static float GlobalCooldownMult = 1f;
    private float EffectiveCooldown => cooldown / Mathf.Max(0.01f, GlobalCooldownMult);

    public void RestoreCooldownNow()
    {
        // pretend we casted long enough ago that we're ready *now*
        lastCastTime = Time.time - EffectiveCooldown;  // uses your GlobalCooldownMult-safe value
    }

    public float GetCooldownProgress01()
    {
        float since = Time.time - lastCastTime;
        return Mathf.Clamp01(since / Mathf.Max(0.0001f, EffectiveCooldown));
    }

    //public static MagicManager Instance { get; private set; }

    //void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    void Start()
    {
        armMagicSpell = FindFirstObjectByType<ArmMagicSpell>();    // Find your casting script
        UpdateMagicType();

        lastCastTime = EffectiveCooldown;   //-cooldown; // start off as "ready"

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // If no magic is equipped, show UI message and exit
            if (currentMagicType == MagicType.None)
            {
                UI ui = FindFirstObjectByType<UI>();
                if (ui != null)
                    ui.ShowTemporaryMagicMessage("No Magic Equipped");
                return;
            }

            // Try casting — handles cooldown + requirements
            if (!TryCast())
            {
                // Optional: show cooldown message
                UI ui = FindFirstObjectByType<UI>();
                if (ui != null)
                    ui.ShowTemporaryMagicMessage("Magic on Cooldown");
            }
        }
    }

    public bool TryCast()
    {
        if (!CanUseMagic()) return false;
        if (!IsReady()) return false;

        CastCurrentMagic();          // do your animation + spawn via ArmMagicSpell
        lastCastTime = Time.time;    // start cooldown now
        return true;
    }


    bool CanUseMagic()
    {
        // Can't use magic if none is equipped!
        if (currentMagicType == MagicType.None) return false;     //no magic so return false

        if (armMagicSpell != null && armMagicSpell.IsCasting()) return false;    //if ur already casting thast bad  --  Prevents spam clicking Q (magic)
        if (Input.GetKey(KeyCode.R)) return false;              //if your reloading thats also bad 

        Weapon currentWeapon = WeaponManager.ActiveWeapon;
        if (currentWeapon != null && currentWeapon.IsReloading) return false;   //if your reloading ur gun
        
        return true;  // All checks passed - can cast magic
    }

    void CastCurrentMagic()
    {
        if (armMagicSpell != null)
        {
            StartCoroutine(armMagicSpell.CastMagicAnimation());     //this controls and disables that animation from teh armMagic script so we dont get him raising his hand all stupid
        }
    }

    public void SetMagicType(MagicType newType)
    {
        currentMagicType = newType;  // Change current magic
        UpdateMagicType();           // Apply the change

        //if (newType == MagicType.None)
        //{
        //    Debug.Log("No magic equipped - Q key disabled");
        //}
        //else
        //{
        //    Debug.Log($"Magic equipped: {currentMagicType} - Q key enabled!");
        //}
    }

    void UpdateMagicType()
    {
        if (armMagicSpell == null) return;

        if (currentMagicType == MagicType.None)
        {
            // Clear magic when none equipped
            armMagicSpell.MagicBallEntityPrefab = null;   //makes q uselsss
            armMagicSpell.MagicArmFireVFX = null;
        }
        else
        {
            MagicData currentData = GetCurrentMagicData();
            armMagicSpell.MagicBallEntityPrefab = currentData.fireballPrefab;
            armMagicSpell.MagicArmFireVFX = currentData.handFireVFX;

            switch (currentMagicType)
            {
                case MagicType.Normal: cooldown = 6f; break;
                case MagicType.Crystal: cooldown = 8f; break;
                case MagicType.Ice: cooldown = 10f; break;
                case MagicType.Void: cooldown = 5f; break;
                case MagicType.Venom: cooldown = 9f; break;
                case MagicType.Lightning: cooldown = 5f; break;
                case MagicType.Wind: cooldown = 6f; break;
                case MagicType.Meteor: cooldown = 2f; break;
                case MagicType.Crimson: cooldown = 8f; break;
            }
        }
    }

    public MagicData GetCurrentMagicData()
    {
        if (currentMagicType == MagicType.Normal) return normalMagic;
        if (currentMagicType == MagicType.Crystal) return crystalcMagic;
        if (currentMagicType == MagicType.Void) return VoidMagic;
        if (currentMagicType == MagicType.Ice) return IceMagic;
        if (currentMagicType == MagicType.Venom) return VenomMagic;
        if (currentMagicType == MagicType.Lightning) return LightningMagic;
        if (currentMagicType == MagicType.Wind) return WindMagic;
        if (currentMagicType == MagicType.Meteor) return MeteorMagic;
        if (currentMagicType == MagicType.Crimson) return CrimsonMagic;
        return null; // No magic equipped
    }

    public MagicType GetCurrentMagicType()
    {
        return currentMagicType;
    }

    public bool HasMagicEquipped()
    {
        return currentMagicType != MagicType.None;  //if its not none than i have some sort of magic.
    }

    public string GetCurrentMagicStatus()
    {
        if (currentMagicType == MagicType.None)
            return "No Magic Equipped";
        else
            return currentMagicType.ToString() + " Magic";
    }
}
