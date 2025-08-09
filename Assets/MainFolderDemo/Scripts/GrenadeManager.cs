using UnityEngine;
using System.Collections.Generic;

public enum GrenadeType { None, Frag, Smoke, Flash }

public class GrenadeManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject fragPrefab;
    public GameObject smokePrefab;
    public GameObject flashPrefab;

    [Header("State")]
    public GrenadeType currentType = GrenadeType.Frag;

    private Dictionary<GrenadeType, GameObject> map;

    void Awake()
    {
        map = new Dictionary<GrenadeType, GameObject>
        {
            { GrenadeType.Frag,  fragPrefab  },
            { GrenadeType.Smoke, smokePrefab },
            { GrenadeType.Flash, flashPrefab }
        };
    }

    public void SetType(GrenadeType t) => currentType = t;

    public GameObject GetCurrentPrefab()
    {
        return map != null && map.TryGetValue(currentType, out var prefab) ? prefab : null;
    }
}
