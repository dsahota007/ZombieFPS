using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
//using Unity.VisualScripting;

public class IceMagic : MonoBehaviour
{
    [Header("Ice Magic Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public float freezeRadius = 5f;
    public float freezeDuration = 10f;
    public float explosionForce = 50f;
    public float explosionUpward = 2f;
    public LayerMask enemyMask;

    [Header("VFX")]
    public GameObject GroundImpactVFX;
    public GameObject FrozenEnemyVFX;
    public GameObject FreezeExplosionVFX;

    private Rigidbody rb;
    private Vector3 impactPoint;
    private bool hasImpacted = false;
    private GameObject spawnedGroundVFX;

    private List<EnemyHealthRagdoll> frozenEnemies = new List<EnemyHealthRagdoll>();
    private List<Rigidbody> frozenBodies = new List<Rigidbody>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;      //we want it going forward
        Destroy(gameObject, lifeTime);

        Collider[] playerColliders = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<Collider>();  // we use this to make sure it does not hit us 

        foreach (Collider col in playerColliders)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasImpacted) return;        //if already hit the ground get outta this code
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            TriggerFreezeEffect();
        }
             
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;
        TriggerFreezeEffect();
    }

    void TriggerFreezeEffect()
    {
        hasImpacted = true;
        impactPoint = transform.position;   //find the center point

        rb.linearVelocity = Vector3.zero;       //dont move this is for the ball/orb
        rb.isKinematic = true;    //Disables physics on the orb so it sits still and doesn’t bounce or roll anymore.

        if (GroundImpactVFX != null)
        {
            spawnedGroundVFX = Instantiate(GroundImpactVFX, impactPoint + Vector3.up, Quaternion.identity);
        }

        // Start logic
        StartCoroutine(FreezeEnemiesOverTime());  
        StartCoroutine(ExplosionAfterDelay());    //Starts a timer to do the explosion after freezeDuration
    }

    IEnumerator FreezeEnemiesOverTime()
    {
        float timer = 0f;   //timer starts at 0 

        while (timer < freezeDuration)   //as long as the timer is less than the specified duration
        {
            timer += Time.deltaTime;    //Increases the timer by the time passed since the last frame

            Collider[] hits = Physics.OverlapSphere(impactPoint, freezeRadius, enemyMask);  //parameter(center of sphere, radiusOFSphere, a layermask defines which layers of colliders to include in the query)  -- FIND ALL ENEMIES WITHIN RADISU
            foreach (Collider col in hits)   //loop thru every collider in the sphere
            {
                EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();  //Checks for colliders inside it
                if (enemy != null && !frozenEnemies.Contains(enemy))   //Make sure the enemy is ot in list of frozen enemy -- Avoid freezing the same enemy multiple times 
                {
                    frozenEnemies.Add(enemy);       //Add to the list of frozen enemies

                    // Freeze movement
                    NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();    //grab the AI movement controller (NavMeshAgent) from the enemy.
                    if (agent != null)
                    {
                        agent.enabled = false;  //freezes their navigation system and stops chasing u 
                    }
                    
                    Animator anim = enemy.GetComponentInChildren<Animator>();  //find animator
                    
                    if (anim != null)
                    {
                        anim.enabled = false;  //turn the animator off so they stop like a statue
                    }
                    
                    if (enemy.ragdollRoot != null)  //check fi enemye has a root point
                    {
                        Rigidbody[] rbs = enemy.ragdollRoot.GetComponentsInChildren<Rigidbody>();  //Grab all the rigidbodies in the enemy’s body (arms, legs, etc.).
                        foreach (Rigidbody rb in rbs)
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                            rb.isKinematic = true;    //disable physics
                            rb.constraints = RigidbodyConstraints.FreezeAll;      //RigidbodyConstraints.FreezeAll	Prevents all movement and rotation (fully frozen in place)
                            frozenBodies.Add(rb);   //Add each frozen body part to a list so we can later unfreeze and blast them in the explosion phase.
                        }
                    }

                    if (FrozenEnemyVFX != null)
                    {
                        GameObject fx = Instantiate(FrozenEnemyVFX, enemy.transform.position + Vector3.up, Quaternion.identity);   //Quaternion.identity means no rotation (keep it upright).
                        fx.transform.SetParent(enemy.transform);   //It also ensures if the enemy is destroyed, the VFX goes with it.
                        Destroy(fx, freezeDuration);
                    }
                }
            }

            yield return null;  //wait 1 frame, then repeat  -- from time.delta
        }
    }

    IEnumerator ExplosionAfterDelay()
    {
        yield return new WaitForSeconds(freezeDuration); //Waits the full freeze time before doing anything -- basically saying hey dont start until freeze is done

        if (spawnedGroundVFX != null)
        {
            Destroy(spawnedGroundVFX);
        }


        // STEP 1: Unfreeze bodies
        foreach (Rigidbody rb in frozenBodies)
        {
            if (rb == null) continue;  //no frozen bodies
            rb.constraints = RigidbodyConstraints.None;   //allows movement again
            rb.isKinematic = false;   //enables physics
            rb.WakeUp();  //ensures they respond to forces
        }

        yield return new WaitForFixedUpdate(); // Let physics wake up

        // STEP 2: Apply explosion
        foreach (Rigidbody rb in frozenBodies)
        {
            if (rb == null) continue;  //no frozen bodies
            Vector3 dir = (rb.transform.position - impactPoint).normalized;   //find direction because of .normalized
            rb.AddExplosionForce(explosionForce, impactPoint, freezeRadius * 2f, explosionUpward, ForceMode.Impulse);  // Lower upward lift (how strong, expolision origin, hjow far explosion affects, upward modifer gives the bone vertical lift, ForceMode.Impulse is an instant kick like a punch.)
        }

        // STEP 3: Wait briefly so physics "ragdoll" reacts before killing
        yield return new WaitForSeconds(0.05f);

        // STEP 4: Now kill enemies (DON’T turn animator back on!)
        foreach (EnemyHealthRagdoll enemy in frozenEnemies) //loop thru all the frozen enemies 
        {
            if (enemy == null) continue;
            Vector3 dir = (enemy.transform.position - impactPoint).normalized;  //recalc to show them which direction to die in for ragdoll bc look at takeDamage parameters. 
            enemy.TakeDamage(999999f, dir);   //enemy dies
        }

        // Explosion VFX
        if (FreezeExplosionVFX != null)
        {
            GameObject fx = Instantiate(FreezeExplosionVFX, impactPoint + Vector3.up * 0.5f, Quaternion.identity);
            Destroy(fx, 5f);
        }

        Destroy(gameObject);
    }

}
