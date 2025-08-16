using UnityEngine;
using System.Collections.Generic;

public enum GrenadeType { None, Frag, Impact, Semtex, Bio, SulfuricNapalm, CrystalCluster, Bastion, Ragnarok }  //enumeration fo all the gernades we can have

public class GrenadeManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject fragPrefab;       //we attach here so ui can fetch
    public GameObject impactPrefab;
    public GameObject semtexPrefab;
    public GameObject bioPrefab;
    public GameObject sulfuricNapalmPrefab;
    public GameObject crystalClusterPrefab;
    public GameObject BastionPrefab;
    public GameObject RagnarokPrefab;


    [Header("Caps (set these manually)")]
    public int fragCap = 7;
    public int impactCap = 6;
    public int semtexCap = 6;
    public int bioCap = 4;
    public int sulfuricNapalmCap = 5;
    public int crystalClusterCap = 3;
    public int bastionCap = 3;
    public int ragnarokCap = 3;


    [Header("State")]
    public GrenadeType currentType = GrenadeType.Frag;     //Default is Frag when the script starts

    private Dictionary<GrenadeType, GameObject> map;    //dictinary mapping for gernadetype KEY
    private Dictionary<GrenadeType, int> count;          // live counts for grenade amounts

    public void SetCount(GrenadeType t, int value)
    {
        // clamp to that type’s cap
        value = Mathf.Clamp(value, 0, GetCap(t));

        // write into your 'count' dictionary (not _counts)
        if (count.ContainsKey(t)) count[t] = value;
        else count.Add(t, value);
    }
    public void RefillAllToCap()
    {
        foreach (GrenadeType gt in System.Enum.GetValues(typeof(GrenadeType)))
        {
            if (gt == GrenadeType.None) continue;
            SetCount(gt, GetCap(gt));
        }
    }




    void Awake()   //Awake(): A Unity lifecycle method that runs before Start(),
    {
        map = new Dictionary<GrenadeType, GameObject>
        {
            { GrenadeType.Frag,  fragPrefab  },         //we assing prefabs to the type 
            { GrenadeType.Impact, impactPrefab },         //key:value ---python dictioary pretty much
            { GrenadeType.Semtex, semtexPrefab },
            { GrenadeType.Bio, bioPrefab },
            { GrenadeType.SulfuricNapalm, sulfuricNapalmPrefab },
            { GrenadeType.CrystalCluster, crystalClusterPrefab },
            { GrenadeType.Bastion, BastionPrefab },
            { GrenadeType.Ragnarok, RagnarokPrefab }
        };

        count = new Dictionary<GrenadeType, int>();             //empty dict will hold each nade type 
        foreach (GrenadeType gerType in System.Enum.GetValues(typeof(GrenadeType)))   //loop thru every enem for each t we set a cap
            count[gerType] = GetCap(gerType);
    }

    public void SetType(GrenadeType type) => currentType = type; //weWhen the player selects a grenade type in your UI panel, you call this. It simply records “the current grenade type” you’re using.

    public GameObject GetCurrentPrefab()
    {
        return map != null && map.TryGetValue(currentType, out var prefab) ? prefab : null; //we try to get the value from caling the key ---map is a dictionary built earlier (in Awake) that says which prefab belongs to which GrenadeType.
    }


    public int GetCount(GrenadeType type) => count.TryGetValue(type, out int amount) ? amount : 0;   //how many grenades you currently have 
    public int GetCap(GrenadeType type)
    {
        switch (type)
        {
            case GrenadeType.Frag: return Mathf.Max(0, fragCap);   //case GrenadeType.Frag:   return fragCap; --- could be this simple but we add math max so we never go negative 
            case GrenadeType.Impact: return Mathf.Max(0, impactCap);
            case GrenadeType.Semtex: return Mathf.Max(0, semtexCap);
            case GrenadeType.Bio: return Mathf.Max(0, bioCap);
            case GrenadeType.SulfuricNapalm: return Mathf.Max(0, sulfuricNapalmCap);
            case GrenadeType.CrystalCluster: return Mathf.Max(0, crystalClusterCap);
            case GrenadeType.Bastion: return Mathf.Max(0, bastionCap);
            case GrenadeType.Ragnarok: return Mathf.Max(0, ragnarokCap);
            default: return 0;
        }
    }

    public bool CanThrowCurrent() => GetCount(currentType) > 0; //will return True if you have at least 1 of the current grenade type.

    public bool ConsumeOneCurrent()
    {
        var type = currentType;
        if (GetCount(type) <= 0) return false;  //will return false if you got no nades 
        count[type] = GetCount(type) - 1;             //will take away one nade and return true.
        return true;
    }

    // Optional: add ammo for a type (caps respected) -----!!!!!!!!!!!!!! future stuff right here
    public void Give(GrenadeType type, int amount)
    {
        count[type] = Mathf.Clamp(GetCount(type) + amount, 0, GetCap(type));
    }
}
