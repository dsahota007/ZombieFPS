using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 2f;
    public float damage = 1f;
   
    
    public GameObject[] bloodEffects;
    public GameObject groundHitEffect;


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
            if (groundHitEffect != null)
            {
                Vector3 hitPoint = transform.position;
                Instantiate(groundHitEffect, hitPoint, Quaternion.identity);
            }

            Destroy(gameObject);
            return;
        }
        if (other.CompareTag("Enemy"))
            if (bloodEffects != null && bloodEffects.Length > 0)  
            {
                int index = Random.Range(0, bloodEffects.Length);   
                Vector3 BulletHitPoint = transform.position;  

                Instantiate(bloodEffects[index], BulletHitPoint, Quaternion.identity);   //Instantiate(whatToSpawn, whereToSpawn, whichRotation);    --- Quaternion.identity --- This is Unity's way of saying: “No rotation at all.”
                
                EnemyHealthRagdoll enemy = other.GetComponent<EnemyHealthRagdoll>();
                if (enemy != null)
                {
                    Vector3 bulletDirection = transform.forward;   //the direction the zombie will go after shot.
                    enemy.TakeDamage(damage, bulletDirection);    //from EnemyHealthScript
                }

            }

        Destroy(gameObject);
    }
}
