using UnityEngine;
//using UnityEngine.UI;

public class PointManager : MonoBehaviour
{
    public static PointManager Instance;

    public int points = 500;            //starting point for points

    void Awake()  //Runs before Start(). Used to initialize things early. 
    {
        if (Instance == null)
            Instance = this;  //	Makes this the global PointManager.
        else
            Destroy(gameObject);
    }

    public void AddPoints(int ZombPoints)
    {
        points += ZombPoints;
    }

    public void SubtractPoints(int cost)
    {
        points -= cost;
    }

    public int GetPoints()    //ui
    {
        return points;
    }
}
