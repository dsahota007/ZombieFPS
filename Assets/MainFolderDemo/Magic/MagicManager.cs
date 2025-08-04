using UnityEngine;
using System.Collections;

public enum MagicType
{
    None,
    Normal,
    Sulfuric
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
    public MagicData sulfuricMagic;

    private MagicType currentMagicType = MagicType.None;  // Start with no magic!
    private ArmMagicSpell armMagicSpell;                  // Reference to your casting script

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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && CanUseMagic())
        {
            CastCurrentMagic();
        }
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
            armMagicSpell.fireballPrefab = null;   //makes q uselsss
            armMagicSpell.armFireVFX = null;
        }
        else
        {
            MagicData currentData = GetCurrentMagicData();
            armMagicSpell.fireballPrefab = currentData.fireballPrefab;
            armMagicSpell.armFireVFX = currentData.handFireVFX;
        }
    }

    public MagicData GetCurrentMagicData()
    {
        if (currentMagicType == MagicType.Normal) return normalMagic;
        if (currentMagicType == MagicType.Sulfuric) return sulfuricMagic;
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
