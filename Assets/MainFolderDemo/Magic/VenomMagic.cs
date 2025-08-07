using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VenomMagic : MonoBehaviour
{
    [Header("Venom Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public float poisonRadius = 5f;
    public float poisonDuration = 8f;
    public LayerMask enemyMask;

    [Header("VFX")]
    public GameObject GroundImpactVFX;
    public GameObject PoisonCloudVFX;

    private Rigidbody rb;
    private Vector3 impactPoint;
    private bool hasImpacted = false;
    private GameObject poisonCloudInstance;

    private List<EnemyHealthRagdoll> killedEnemies = new List<EnemyHealthRagdoll>();     //Keeps track of already-killed enemies so they don’t die twice.

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;  //we wanna launch str8 forward
        Destroy(gameObject, lifeTime);

        Collider[] playerColliders = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<Collider>();  // we use this to make sure it does not hit us 
        foreach (Collider col in playerColliders)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasImpacted) return;
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            TriggerVenomEffect();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;   //if already HIT the ground or enemy leave this code
        TriggerVenomEffect();      
    }

    void TriggerVenomEffect()
    {
        hasImpacted = true;
        impactPoint = transform.position;           //find im pact point

        rb.linearVelocity = Vector3.zero;           //kill the movement
        rb.isKinematic = true;                  //kill the physics

        if (GroundImpactVFX != null)
        {
            GameObject vfx = Instantiate(GroundImpactVFX, impactPoint, Quaternion.identity); 
            Destroy(vfx, 5f);
        }

 
        if (PoisonCloudVFX != null)
        {
            poisonCloudInstance = Instantiate(PoisonCloudVFX, impactPoint, Quaternion.identity);
            Destroy(poisonCloudInstance, poisonDuration);
        }

        StartCoroutine(KillEnemiesInRadius());   //Starts checking for enemies inside the area every frame
    }

    IEnumerator KillEnemiesInRadius()
    {
        float timer = 0f;    //start a timer at 0 

        while (timer < poisonDuration)     //as long as the timer is less than the specified duration
        {
            timer += Time.deltaTime;     //Increases the timer by the time passed since the last frame

            Collider[] hits = Physics.OverlapSphere(impactPoint, poisonRadius, enemyMask);  //find enemeis within radius
            foreach (Collider col in hits)
            {
                EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();   // find colliders of enemies
                if (enemy != null && !killedEnemies.Contains(enemy))
                {
                    killedEnemies.Add(enemy);   //add enemy to killed list so we dont kill again

                    Vector3 dir = (enemy.transform.position - impactPoint).normalized;    //calc the direction bc we get the the enemy pos adn impact pos than .normalized to find direction bc fo unit 0-1
                    enemy.TakeDamage(999999f, dir);   // Normal death  -- no crazy ragdoll
                }
            }

            yield return null;    //wait 1 frame, then repeat -- from time.delta
        }

        Destroy(gameObject);
    }
}