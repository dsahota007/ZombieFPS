using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float bulletLifetime = 1.2f;
    void Start()
    {
        Destroy(gameObject, bulletLifetime); 
    }
}
