using UnityEngine;
using System.Collections.Generic;

public enum GrenadeType { None, Frag, Impact, Semtex, Bio }  //enumeration fo all the gernades we can have

public class GrenadeManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject fragPrefab;       //we attach here so ui can fetch
    public GameObject impactPrefab;
    public GameObject semtexPrefab;
    public GameObject bioPrefab;

    [Header("State")]
    public GrenadeType currentType = GrenadeType.Frag;     //Default is Frag when the script starts

    private Dictionary<GrenadeType, GameObject> map;    //dictinary mapping for gernadetype KEY

    void Awake()   //Awake(): A Unity lifecycle method that runs before Start(),
    {
        map = new Dictionary<GrenadeType, GameObject>
        {
            { GrenadeType.Frag,  fragPrefab  },         //we assing prefabs to the type 
            { GrenadeType.Impact, impactPrefab },         //key:value ---python dictioary pretty much
            { GrenadeType.Semtex, semtexPrefab },
            { GrenadeType.Bio, bioPrefab }
        };
    }

    public void SetType(GrenadeType t) => currentType = t; 

    public GameObject GetCurrentPrefab()
    {
        return map != null && map.TryGetValue(currentType, out var prefab) ? prefab : null; //we try to get the value from caling the key 
    }
}
