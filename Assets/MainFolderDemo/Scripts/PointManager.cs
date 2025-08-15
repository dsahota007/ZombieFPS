using UnityEngine;
//using UnityEngine.UI;

public class PointManager : MonoBehaviour
{
    public static PointManager Instance;

    public int points = 500;            //starting point for points
    public static float GlobalPointsMult = 1f;

    void Awake()  //Runs before Start(). Used to initialize things early. 
    {
        if (Instance == null)
            Instance = this;  //	Makes this the global PointManager.
        else
            Destroy(gameObject);
    }

    public void AddPoints(int ZombPoints)      // CHANGE THIS
    {
        points += Mathf.RoundToInt(ZombPoints * GlobalPointsMult);
    }
    public void SubtractPoints(int cost)
    {
        points -= cost;
    }

    public int GetPoints()    //ui
    {
        return points;
    }

    public bool CanAfford(int cost)
    {
        return points >= cost;
    }

    public bool TrySpend(int cost)
    {
        if (points < cost) return false;
        points -= cost;
        return true;
    }

}
