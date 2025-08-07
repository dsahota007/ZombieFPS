using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class LightningMagic : MonoBehaviour
{
    [Header("Lightning Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public float freezeRadius = 5f;
    public float chainRadius = 3f;
    public float freezeDuration = 6f;
    public int maxChainTargets = 10;
    public LayerMask enemyMask;

    [Header("VFX")]
    public GameObject GroundImpactVFX;
    public GameObject FrozenEnemyVFX;
    public GameObject LightningDeathVFX;

    private Rigidbody rb;
    private Vector3 impactPoint;
    private bool hasImpacted = false;

    private List<EnemyHealthRagdoll> frozenEnemies = new List<EnemyHealthRagdoll>();   
    private List<Rigidbody> frozenBodies = new List<Rigidbody>();     

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);

        Collider[] playerColliders = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<Collider>();  // we use this to make sure it does not hit us 
        foreach (Collider col in playerColliders)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasImpacted) return;      //if we already hit GTFO this code
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            TriggerLightningEffect();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;                 //if we already hit GTFO this code
        TriggerLightningEffect();
    }

    void TriggerLightningEffect()
    {
        hasImpacted = true;                     
        impactPoint = transform.position;           //find hti point so center of radius
        rb.linearVelocity = Vector3.zero;           //kill the movement
        rb.isKinematic = true;                  //kill the physics

        if (GroundImpactVFX != null)
        {
            GameObject vfx = Instantiate(GroundImpactVFX, impactPoint, Quaternion.identity);     //spawn slam vfx
            Destroy(vfx, 15f);
        }

        StartCoroutine(FreezeEnemiesAndChain());        //freeze adn chain enemies adn call this every second
        StartCoroutine(UnfreezeAndKillEnemies());       //we unfreeze adn kill em and these 2 functions we run every frame second -- Not called every second. These are each started once and run every frame internally with yield return null
    }

    IEnumerator FreezeEnemiesAndChain()
    {
        float timer = 0f;   //start a timer at 0 

        while (timer < freezeDuration && frozenEnemies.Count < maxChainTargets)      //as long as the timer is less than the specified duration and frozen enmeis is less than the max amount so we can chain more mofo
        {
            timer += Time.deltaTime;   //Increases the timer by the time passed since the last frame

            // First wave from impact
            Collider[] hits = Physics.OverlapSphere(impactPoint, freezeRadius, enemyMask);   //find nemeies within radius
            foreach (Collider col in hits)
            {
                TryFreezeEnemy(col);   //summon tryFreeze func and put in the col because we loop thru to find enemy collider
                if (frozenEnemies.Count >= maxChainTargets) break;    //if we have too many people being chained than leave this chunk of code
            }

            // Chain from already frozen
            List<EnemyHealthRagdoll> chainTargets = new List<EnemyHealthRagdoll>(frozenEnemies);     //ok so ealrier ^^ we make a list to add frozen enmeis now we loop thru that frozne list
            foreach (EnemyHealthRagdoll frozen in chainTargets)   
            {
                if (frozen == null) continue;  //if your not frozen - somehow an enemy got destroyed than move on  

                Collider[] chainHits = Physics.OverlapSphere(frozen.transform.position, chainRadius, enemyMask);  //instead of impactPoint we use it on the enemy now so we make a radisu on them
                foreach (Collider col in chainHits)  //now for each enemyCollider in the new radius
                {
                    TryFreezeEnemy(col);   //summon tryFreeze func and put in the col because we loop thru to find enemy collider
                    if (frozenEnemies.Count >= maxChainTargets) break;  //if we have too many people being chained than leave this chunk of code
                }

                if (frozenEnemies.Count >= maxChainTargets) break;    //After processing one frozen enemy, if we’re already full on max chains, break out early again.
            }
            yield return null;  //wait 1 frame, then repeat -- from time.delta
        }
    }

    void TryFreezeEnemy(Collider col)
    {
        if (frozenEnemies.Count >= maxChainTargets) return;   //if we have too many people being chained than leave this chunk of code -- Don’t freeze more

        EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();     //grab enemy script 
        if (enemy != null && !frozenEnemies.Contains(enemy) && !enemy.IsDead())    //continue if enemy exist, not already frozen and not dead
        {
            frozenEnemies.Add(enemy);   //We store them so we know who’s frozen

            // Disable movement
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();  
            if (agent != null) agent.enabled = false;

            Animator anim = enemy.GetComponentInChildren<Animator>();  //disable animaiton
            if (anim != null) anim.enabled = false;

            if (enemy.ragdollRoot != null)
            {
                Rigidbody[] rbs = enemy.ragdollRoot.GetComponentsInChildren<Rigidbody>();    //We make their body parts completely frozen using physics.
                foreach (Rigidbody rb in rbs) 
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                    frozenBodies.Add(rb);
                }
            }

            if (FrozenEnemyVFX != null)
            {
                GameObject fx = Instantiate(FrozenEnemyVFX, enemy.transform.position + Vector3.up, Quaternion.identity);
                fx.transform.SetParent(enemy.transform);    //This line attaches the visual effect (VFX) to the enemy’s body — like making it a child object of the enemy.
                Destroy(fx, freezeDuration);
            }
        }
    }

    IEnumerator UnfreezeAndKillEnemies()
    {
        yield return new WaitForSeconds(freezeDuration);    //Waits for the freeze effect to finish 

        foreach (Rigidbody rb in frozenBodies)                // Unfreeze all ragdolls
        {
            if (rb == null) continue;                       //no frozen bodies
            rb.constraints = RigidbodyConstraints.None;   //allows movement again
            rb.isKinematic = false;                   //enables physics
            rb.WakeUp();                             //ensures they respond to forces
        }

        yield return new WaitForFixedUpdate();   //Wait one physics frame so Unity can process the unfreezing before we kill them.

        // Kill all enemies
        foreach (EnemyHealthRagdoll enemy in frozenEnemies)   //Loop through all the frozen enemies
        {
            if (enemy == null) continue;
            Vector3 dir = (enemy.transform.position - impactPoint).normalized;  //calc enemy from the impact point but this is getting the direction because of .normalized
            enemy.TakeDamage(999999f, dir);         //kill em

            if (LightningDeathVFX != null)
            {
                GameObject fx = Instantiate(LightningDeathVFX, enemy.transform.position + Vector3.up, Quaternion.identity);
                Destroy(fx, 3f);
            }
        }

        Destroy(gameObject);
    }
}
