using UnityEngine;

public class InfuseStation : MonoBehaviour
{
    public float interactDistance = 3f;

    public void OnInfusePressed()
    {
        Debug.Log($"Opened Infuse Chest.");
        // Later: Add logic to modify weapon properties based on this.elementName
    }
}
