using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

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
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);

        // Avoid colliding with player
        Collider[] playerColliders = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<Collider>();
        foreach (Collider col in playerColliders)
            Physics.IgnoreCollision(GetComponent<Collider>(), col);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasImpacted) return;
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Enemy"))
            TriggerFreezeEffect();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;
        TriggerFreezeEffect();
    }

    void TriggerFreezeEffect()
    {
        hasImpacted = true;
        impactPoint = transform.position;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Spawn ground VFX
        if (GroundImpactVFX != null)
        {
            spawnedGroundVFX = Instantiate(GroundImpactVFX, impactPoint, Quaternion.identity);
        }

        // Start logic
        StartCoroutine(FreezeEnemiesOverTime());
        StartCoroutine(ExplosionAfterDelay());
    }

    IEnumerator FreezeEnemiesOverTime()
    {
        float timer = 0f;

        while (timer < freezeDuration)
        {
            timer += Time.deltaTime;

            Collider[] hits = Physics.OverlapSphere(impactPoint, freezeRadius, enemyMask);
            foreach (Collider col in hits)
            {
                EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();
                if (enemy != null && !frozenEnemies.Contains(enemy))
                {
                    frozenEnemies.Add(enemy);

                    // Freeze movement
                    NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
                    if (agent != null) agent.enabled = false;

                    Animator anim = enemy.GetComponentInChildren<Animator>();
                    if (anim != null) anim.enabled = false;

                    if (enemy.ragdollRoot != null)
                    {
                        Rigidbody[] rbs = enemy.ragdollRoot.GetComponentsInChildren<Rigidbody>();
                        foreach (Rigidbody rb in rbs)
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                            rb.isKinematic = true;
                            rb.constraints = RigidbodyConstraints.FreezeAll;
                            frozenBodies.Add(rb);
                        }
                    }

                    // VFX
                    if (FrozenEnemyVFX != null)
                    {
                        GameObject fx = Instantiate(FrozenEnemyVFX, enemy.transform.position + Vector3.up, Quaternion.identity);
                        fx.transform.SetParent(enemy.transform);
                        Destroy(fx, freezeDuration);
                    }
                }
            }

            yield return null;
        }
    }

    IEnumerator ExplosionAfterDelay()
    {
        yield return new WaitForSeconds(freezeDuration);

        if (spawnedGroundVFX != null)
            Destroy(spawnedGroundVFX);

        // STEP 1: Unfreeze bodies
        foreach (Rigidbody rb in frozenBodies)
        {
            if (rb == null) continue;
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.WakeUp();
        }

        yield return new WaitForFixedUpdate(); // Let physics wake up

        // STEP 2: Apply explosion
        foreach (Rigidbody rb in frozenBodies)
        {
            if (rb == null) continue;
            Vector3 dir = (rb.transform.position - impactPoint).normalized;
            rb.AddExplosionForce(explosionForce, impactPoint, freezeRadius * 2f, explosionUpward, ForceMode.Impulse);
        }

        // STEP 3: Wait briefly so physics "ragdoll" reacts before killing
        yield return new WaitForSeconds(0.05f);

        // STEP 4: Now kill enemies (DON’T turn animator back on!)
        foreach (EnemyHealthRagdoll enemy in frozenEnemies)
        {
            if (enemy == null) continue;

            // Do NOT enable animator anymore (keep them in ragdoll pose)

            // Kill enemy
            Vector3 dir = (enemy.transform.position - impactPoint).normalized;
            enemy.TakeDamage(999999f, dir);
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
