using UnityEngine;
using System.Collections;

public class WindMagic : MonoBehaviour
{
    [Header("Wind Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public float pushRadius = 5f;
    //public float pushForce = 40f;
    public LayerMask enemyMask;


    [Header("VFX")]
    public GameObject GroundImpactVFX;
    public GameObject EnemyImpactFX;

    private Rigidbody rb;
    private Vector3 impactPoint;
    private bool hasImpacted = false;

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
        if (hasImpacted) return;     //if we already hit GTFO this code
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            TriggerWindEffect();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;  //if we already hit GTFO this code
        TriggerWindEffect();   
    }

    void TriggerWindEffect()
    {
        hasImpacted = true;         
        impactPoint = transform.position;    //find the impact origin of radius
        //rb.linearVelocity = Vector3.zero;
        //rb.isKinematic = true;                  //we dont peirce into the ball bc we made the radisu huge (invisable ball!)

        if (GroundImpactVFX != null)
        {
            GameObject vfx = Instantiate(GroundImpactVFX, impactPoint, Quaternion.identity);
            Destroy(vfx, 5f);

        }
         

        StartCoroutine(PushAndKillEnemies());    //handles knockback and death so we run this every frame
    }

    IEnumerator PushAndKillEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(impactPoint, pushRadius, enemyMask);    //Find all enemies within the blast radius
        foreach (Collider col in hits)          
        {
            EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();     //get all enemy script so we can interact with health / ragdoll.
            if (enemy != null && !enemy.IsDead())      //if enemy exist and they aint dead
            {
                Vector3 dir = (enemy.transform.position - impactPoint).normalized;   //Figure out which direction to push the enemy (away from explosion). with .normalzied
                if (enemy.ragdollRoot != null)
                {
                    Rigidbody[] rbs = enemy.ragdollRoot.GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody body in rbs)
                    {
                        body.isKinematic = false;
                        body.AddForce(dir * 40f, ForceMode.Impulse);    //this is where the force is we got rid of the variable and just wrote the float her directly
                    }
                }
                if (EnemyImpactFX != null)      // VFX on enemy
                {
                    GameObject fx = Instantiate(EnemyImpactFX, enemy.transform.position + Vector3.up, Quaternion.identity);
                    Destroy(fx, 3f);
                }
                 
                enemy.TakeDamage(999999f, dir);         // Kill enemy
            }
        }

        yield return null;  //this runs every frame to ensure kiling is consistent
        Destroy(gameObject);
    }
}
