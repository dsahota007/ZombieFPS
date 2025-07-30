using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float Lifetime = 4f;
    void Start()
    {
        Destroy(gameObject, Lifetime); 
    }
}
