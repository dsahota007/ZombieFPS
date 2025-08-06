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
            TriggerLightningEffect();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;
        TriggerLightningEffect();
    }

    void TriggerLightningEffect()
    {
        hasImpacted = true;
        impactPoint = transform.position;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (GroundImpactVFX != null)
        {
            Instantiate(GroundImpactVFX, impactPoint, Quaternion.identity);
        }

        StartCoroutine(FreezeEnemiesAndChain());
        StartCoroutine(KillFrozenAfterDelay());
    }

    IEnumerator FreezeEnemiesAndChain()
    {
        float timer = 0f;

        while (timer < freezeDuration && frozenEnemies.Count < maxChainTargets)
        {
            timer += Time.deltaTime;

            // First wave from impact
            Collider[] hits = Physics.OverlapSphere(impactPoint, freezeRadius, enemyMask);
            foreach (Collider col in hits)
            {
                TryFreezeEnemy(col);
                if (frozenEnemies.Count >= maxChainTargets) break;
            }

            // Chain from already frozen
            List<EnemyHealthRagdoll> chainTargets = new List<EnemyHealthRagdoll>(frozenEnemies);
            foreach (EnemyHealthRagdoll frozen in chainTargets)
            {
                if (frozen == null) continue;

                Collider[] chainHits = Physics.OverlapSphere(frozen.transform.position, chainRadius, enemyMask);
                foreach (Collider col in chainHits)
                {
                    TryFreezeEnemy(col);
                    if (frozenEnemies.Count >= maxChainTargets) break;
                }

                if (frozenEnemies.Count >= maxChainTargets) break;
            }

            yield return null;
        }
    }

    void TryFreezeEnemy(Collider col)
    {
        if (frozenEnemies.Count >= maxChainTargets) return;

        EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();
        if (enemy != null && !frozenEnemies.Contains(enemy) && !enemy.IsDead())
        {
            frozenEnemies.Add(enemy);

            // Disable movement
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

    IEnumerator KillFrozenAfterDelay()
    {
        yield return new WaitForSeconds(freezeDuration);

        // Unfreeze all ragdolls
        foreach (Rigidbody rb in frozenBodies)
        {
            if (rb == null) continue;
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
            rb.WakeUp();
        }

        yield return new WaitForFixedUpdate();

        // Kill all enemies
        foreach (EnemyHealthRagdoll enemy in frozenEnemies)
        {
            if (enemy == null) continue;
            Vector3 dir = (enemy.transform.position - impactPoint).normalized;
            enemy.TakeDamage(999999f, dir);

            if (LightningDeathVFX != null)
            {
                GameObject fx = Instantiate(LightningDeathVFX, enemy.transform.position + Vector3.up, Quaternion.identity);
                Destroy(fx, 3f);
            }
        }

        Destroy(gameObject);
    }
}
