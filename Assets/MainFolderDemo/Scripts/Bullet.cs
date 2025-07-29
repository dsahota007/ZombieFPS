using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public GameObject[] bloodEffects;

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
        //Debug.Log("Bullet hit: " + other.name);

        if (other.CompareTag("Ground"))
        {
            //Debug.Log("Bullet hit ground!");
            Destroy(gameObject);
        }
        if (other.CompareTag("Enemy"))
            if (bloodEffects != null && bloodEffects.Length > 0)  //we need blood effects in the list
            {
                int index = Random.Range(0, bloodEffects.Length);   //randomly catch a blood vfx
                Vector3 BulletHitPoint = transform.position;  //  Get contact point (find approximate using bullet position)

                Instantiate(bloodEffects[index], BulletHitPoint, Quaternion.identity);   //Instantiate(whatToSpawn, whereToSpawn, whichRotation);    --- Quaternion.identity --- This is Unity's way of saying: “No rotation at all.”
                
                EnemyHealthRagdoll enemy = other.GetComponent<EnemyHealthRagdoll>();
                if (enemy != null)
                {
                    Vector3 bulletDirection = transform.forward;
                    enemy.RegisterHit(bulletDirection);
                }

            }

        Destroy(gameObject);
    }
}
