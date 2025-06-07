using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bullet hit: " + other.name);

        if (other.CompareTag("Ground"))
        {
            Debug.Log("Bullet hit ground!");
            Destroy(gameObject);
        }
    }
}
