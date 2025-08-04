using UnityEngine;
using System.Collections;

public enum MagicType
{
    None,
    Normal,
    Sulfuric
}

[System.Serializable]
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
    private ArmMagicSpell armMagicSpell;

    public static MagicManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        armMagicSpell = FindFirstObjectByType<ArmMagicSpell>();
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
        if (currentMagicType == MagicType.None) return false;

        if (armMagicSpell != null && armMagicSpell.IsCasting()) return false;
        if (Input.GetKey(KeyCode.R)) return false;

        Weapon currentWeapon = WeaponManager.ActiveWeapon;
        if (currentWeapon != null && currentWeapon.IsReloading) return false;

        return true;
    }

    void CastCurrentMagic()
    {
        if (armMagicSpell != null)
        {
            StartCoroutine(armMagicSpell.CastMagicAnimation());
        }
    }

    public void SetMagicType(MagicType newType)
    {
        currentMagicType = newType;
        UpdateMagicType();

        if (newType == MagicType.None)
        {
            Debug.Log("No magic equipped - Q key disabled");
        }
        else
        {
            Debug.Log($"Magic equipped: {currentMagicType} - Q key enabled!");
        }
    }

    void UpdateMagicType()
    {
        if (armMagicSpell == null) return;

        if (currentMagicType == MagicType.None)
        {
            // Clear magic when none equipped
            armMagicSpell.fireballPrefab = null;
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
        return currentMagicType != MagicType.None;
    }

    public string GetCurrentMagicStatus()
    {
        if (currentMagicType == MagicType.None)
            return "No Magic Equipped";
        else
            return currentMagicType.ToString() + " Magic";
    }
}
