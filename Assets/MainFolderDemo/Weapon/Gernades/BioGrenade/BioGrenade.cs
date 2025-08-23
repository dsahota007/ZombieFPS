using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BioGrenade : MonoBehaviour
{
    [Header("Bio Settings")]
    public float speed = 20f;                 // we can still launch forward if needed
    public float lifeTime = 5f;
    public float gasRadius = 5f;              // same idea as poisonRadius
    public float gasDuration = 8f;            // how long the gas lingers
    public LayerMask enemyMask;

    [Header("VFX")]
    public GameObject GroundImpactVFX;
    public GameObject GasCloudVFX;            // poison cloud but bio gas

    [Header("Physics")]
    public float spinTorque = 5f;             // small spin for style

    private Rigidbody rb;
    private Vector3 impactPoint;
    private bool hasImpacted = false;
    private GameObject gasCloudInstance;

    private List<EnemyHealthRagdoll> killedEnemies = new List<EnemyHealthRagdoll>();     //Keeps track of already-killed enemies so they don’t die twice.

    void Awake()       //Awake(): A Unity lifecycle method that runs before Start(),
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Start()
    {
        // Add a little spin so it rolls naturally
        if (spinTorque > 0f && rb != null)
            rb.AddTorque(Random.onUnitSphere * spinTorque, ForceMode.Impulse);    //spin logic

        // if you’re throwing via ApplyThrow() you can ignore this forward kick
        if (rb != null && speed > 0f)
            rb.linearVelocity = transform.forward * speed;  //we wanna launch str8 forward

        Destroy(gameObject, lifeTime);

        // we use this to make sure it does not hit us 
        Collider[] playerColliders = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<Collider>();
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
            TriggerBioEffect();   // same as venom, just bio gas 
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;   //if already HIT the ground or enemy leave this code
        TriggerBioEffect();         // Backup collision detection in case trigger doesn't work
    }

    void TriggerBioEffect()
    {
        hasImpacted = true;
        impactPoint = transform.position;           //find im pact point

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;       //kill the movement
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;                  //kill the physics
        }

        if (GroundImpactVFX != null)
        {
            GameObject vfx = Instantiate(GroundImpactVFX, impactPoint, Quaternion.identity);
            Destroy(vfx, 5f);
        }

        // spawn the gas cloud that lingers and kills
        if (GasCloudVFX != null)
        {
            gasCloudInstance = Instantiate(GasCloudVFX, impactPoint, Quaternion.identity);
            Destroy(gasCloudInstance, gasDuration);
        }

        StartCoroutine(KillEnemiesInRadius());   //Starts checking for enemies inside the area every frame
    }

    IEnumerator KillEnemiesInRadius()
    {
        float timer = 0f;    //start a timer at 0 

        while (timer < gasDuration)     //as long as the timer is less than the specified duration
        {
            timer += Time.deltaTime;     //Increases the timer by the time passed since the last frame

            //find enemeis within radius
            Collider[] hits = Physics.OverlapSphere(impactPoint, gasRadius, enemyMask);
            foreach (Collider col in hits)
            {
                // find colliders of enemies
                EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();
                if (enemy != null && !killedEnemies.Contains(enemy))
                {
                    killedEnemies.Add(enemy);   //add enemy to killed list so we dont kill again

                    //calc the direction bc we get the the enemy pos adn impact pos than .normalized to find direction bc fo unit 0-1
                    Vector3 dir = (enemy.transform.position - impactPoint).normalized;
                    enemy.TakeDamage(999999f, dir);   // Normal death  -- no crazy ragdoll
                }
            }

            yield return null;    //wait 1 frame, then repeat -- from time.delta
        }

        Destroy(gameObject);
    }

    // if you prefer to set velocity when you throw it from code
    public void ApplyThrow(Vector3 velocity)
    {
        if (rb != null) rb.linearVelocity = velocity;   // Unity 6 API you were using
    }
}
