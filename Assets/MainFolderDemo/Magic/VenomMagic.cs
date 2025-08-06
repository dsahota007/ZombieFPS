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

    private HashSet<EnemyHealthRagdoll> killedEnemies = new HashSet<EnemyHealthRagdoll>();

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
            TriggerVenomEffect();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;
        TriggerVenomEffect();
    }

    void TriggerVenomEffect()
    {
        hasImpacted = true;
        impactPoint = transform.position;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (GroundImpactVFX != null)
        {
            Instantiate(GroundImpactVFX, impactPoint, Quaternion.identity);
        }

        if (PoisonCloudVFX != null)
        {
            poisonCloudInstance = Instantiate(PoisonCloudVFX, impactPoint, Quaternion.identity);
            Destroy(poisonCloudInstance, poisonDuration);
        }

        StartCoroutine(KillEnemiesInRadius());
    }

    IEnumerator KillEnemiesInRadius()
    {
        float timer = 0f;

        while (timer < poisonDuration)
        {
            timer += Time.deltaTime;

            Collider[] hits = Physics.OverlapSphere(impactPoint, poisonRadius, enemyMask);
            foreach (Collider col in hits)
            {
                EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();
                if (enemy != null && !killedEnemies.Contains(enemy))
                {
                    killedEnemies.Add(enemy);

                    Vector3 dir = (enemy.transform.position - impactPoint).normalized;
                    enemy.TakeDamage(9999f, dir); // Normal death
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
