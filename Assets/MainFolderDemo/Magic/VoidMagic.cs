using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
//using static UnityEngine.InputManagerEntry;
//using Unity.Android.Gradle.Manifest;

public class VoidMagic : MonoBehaviour
{
    [Header("Void Magic Movement")]
    public float speed = 30f;
    public float lifeTime = 20f;

    [Header("Void Death Zone")]
    public float deathRadius = 5f;              //size of suction orb 
    public LayerMask enemyMask;

    [Header("Body Dragging Settings")]
    public float dragDuration = 10f;          //the length the circle will live for 
    public float dragForce = 150f;

    [Header("VFX Effects")]
    public GameObject GroundEntitySlamVFX;
    public GameObject VoidExplosionVFX;
    public GameObject EnemyImpactVFX;

    private Rigidbody rb;
    private bool hasImpacted = false;           //prevents the orb from activating multiple times.
    private bool deathZoneActive = false;      //true when the orb is sucking things in.
    private Vector3 impactPoint;            //where the orb landed.
    private List<EnemyHealthRagdoll> killedEnemies = new List<EnemyHealthRagdoll>();            //track which enemies have already died.!!!!!!!!!!!!!!!!
    private List<Rigidbody> deadBodies = new List<Rigidbody>();             //list of body parts to pull and explode later!!!!!!!!!!!!!!!!

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;  //on start we launch forward 
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
            TriggerVoidDeathZone();
            return;
        }

        //if (other.CompareTag("Enemy"))
        //{
        //    TriggerVoidDeathZone();
        //}
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;  //if already hit the grodun egt outta this code
        TriggerVoidDeathZone();
    }

    void TriggerVoidDeathZone()
    {
        if (hasImpacted) return;  //if already hit the grodun egt outta this code

        hasImpacted = true;
        impactPoint = transform.position;   //get position of where it landed we store that 

        //CancelInvoke("DestroyIfStillFlying");

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (GroundEntitySlamVFX != null)
        {
            GameObject vfx1 = Instantiate(GroundEntitySlamVFX, impactPoint, Quaternion.identity);   // spawning in the ground vfx
            Destroy(vfx1, 10f);
        }

        InstantKillEnemiesInRange();

        deathZoneActive = true;
        StartCoroutine(MonitorDeathZone());    //this is corutine bc we have to do it every second

        StartCoroutine(DragBodiesToCenter());   //this is corutine bc we have to do it every second
    }

    //void SpawnGroundEffects()
    //{
    //    if (GroundEntitySlamVFX != null)
    //    {
    //        GameObject vfx1 = Instantiate(GroundEntitySlamVFX, impactPoint, Quaternion.identity); 
    //        Destroy(vfx1, 10f);
    //    }
    //}

    void InstantKillEnemiesInRange()   //Runs once, right when the orb lands. Kills everyone who's already inside the death radius.
    {
        killedEnemies.Clear();     //Wipes out any old data from previous uses.
        deadBodies.Clear();      //Prepares fresh lists for tracking newly killed enemies and their body parts.

        Collider[] hits = Physics.OverlapSphere(impactPoint, deathRadius, enemyMask);  //parameter(center of sphere, radiusOFSphere, a layermask defines which layers of colliders to include in the query)

        foreach (Collider col in hits)    //Go through every collider found in the sphere
        {
            EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();   //Checks for colliders inside it
            if (enemy != null && !killedEnemies.Contains(enemy))    //Make sure the enemy is valid -- Avoid killing the same enemy multiple times
            {
                float distance = Vector3.Distance(enemy.transform.position, impactPoint);   //checks enemny to the center of the orb
                if (distance <= deathRadius)    
                {
                    killedEnemies.Add(enemy);   //mark enemey as killed
                    Vector3 direction = (enemy.transform.position - impactPoint).normalized;
                    enemy.TakeDamage(999999f, direction);    //the die function exist in this so they die here
                    CollectBodyParts(enemy);
                }
            }
        }
    }

    IEnumerator MonitorDeathZone()   //Runs every frame after the orb lands
    {
        while (deathZoneActive)
        {
            Collider[] hits = Physics.OverlapSphere(impactPoint, deathRadius, enemyMask);  //parameter(center of sphere, radiusOFSphere, a layermask defines which layers of colliders to include in the query)  -- FIND ALL ENEMIES WITHIN RADISU
            foreach (Collider col in hits)  //Go through every collider found in the sphere
            {
                EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();  //Checks for colliders inside it
                if (enemy != null && !killedEnemies.Contains(enemy))         //Make sure the enemy is valid -- Avoid killing the same enemy multiple times
                {
                    float distance = Vector3.Distance(enemy.transform.position, impactPoint);//checks enemny to the center of the orb
                    if (distance <= deathRadius)
                    {
                        killedEnemies.Add(enemy);
                        Vector3 direction = (enemy.transform.position - impactPoint).normalized;  // we go calc again orb to enemy and go noramlzid  unit direction (length = 1), so we only get direction, not distance
                        enemy.TakeDamage(999999f, direction);                   //the die function exist in this so they die here
                        CollectBodyParts(enemy);
                    }
                }
            }

            yield return null; //wait 1 frame, then repeat --this allows for death for when they enter the radius
        }
    }

    void CollectBodyParts(EnemyHealthRagdoll deadEnemy)
    {
        if (deadEnemy != null && deadEnemy.ragdollRoot != null)
        {
            Rigidbody[] ragdollRbs = deadEnemy.ragdollRoot.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in ragdollRbs)
            {
                if (rb != null && !deadBodies.Contains(rb))
                {
                    deadBodies.Add(rb);    //adding dead bodies to a list 
                }
            }
        }
    }

    IEnumerator DragBodiesToCenter()
    {
        float timer = 0f;   //start at 0

        while (timer < dragDuration)   //The dragging effect continues as long as the timer is less than the specified duration
        {
            timer += Time.deltaTime;    //Increases the timer by the time passed since the last frame

            foreach (Rigidbody bodyPart in deadBodies)    //loop thru adn find all enemies
            {
                if (bodyPart != null)  
                {
                    Vector3 directionToCenter = (impactPoint - bodyPart.position).normalized;    //we calc the middle fo the center where the orb is adn we do unit numebr to get direciotn 
                    float distance = Vector3.Distance(impactPoint, bodyPart.position);  //Measures how far the body part is from the center
                    float forceMultiplier = Mathf.Lerp(0.5f, 2f, distance / deathRadius); //lerp (a,b,t smoothly transition from a to b and than time  --- 
                    bodyPart.AddForce(directionToCenter * dragForce * forceMultiplier, ForceMode.Force);  //directionToCenter points towarsd the middle, strength of pull, than force multiplier, ForceMode.Force means: he force is applied gradually over time, like gravity or wind. 
                }
            }
            yield return null;   //wait 1 frame, then repeat --this allows for the draggint to continue
        }
        deathZoneActive = false; // Stop monitoring after drag completes
        TriggerVoidExplosion();
    }

    void TriggerVoidExplosion()
    {
        if (VoidExplosionVFX != null)
        {
            GameObject explosionVFX = Instantiate(VoidExplosionVFX, impactPoint, Quaternion.identity);   //spawn in the landing explosion
            Destroy(explosionVFX, 10f);
        }

        foreach (Rigidbody bodyPart in deadBodies)
        {
            if (bodyPart != null)
            {
                float dist = Vector3.Distance(impactPoint, bodyPart.transform.position);
                float force = Mathf.Lerp(50f, 60f, dist / deathRadius);  //rmbr radius -- circle mid way point -- think of pi

                bodyPart.AddExplosionForce(   // Lower upward lift (how strong, expolision origin, hjow far explosion affects, upward modifer gives the bone vertical lift, ForceMode.Impulse is an instant kick like a punch.)
                    force,
                    impactPoint,
                    deathRadius * 3f,
                    100f,
                    ForceMode.Impulse
                );
            }
        }

        foreach (EnemyHealthRagdoll deadEnemy in killedEnemies)
        {
            if (deadEnemy != null && EnemyImpactVFX != null)
            {
                GameObject deathVFXEnemy = Instantiate(EnemyImpactVFX, deadEnemy.transform.position + Vector3.up * 1f, Quaternion.identity);  //we spawn the small spark effects
                Destroy(deathVFXEnemy, 3f);
            }
        }

        Destroy(gameObject);
    }
}
