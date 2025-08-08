using UnityEngine;

public class CrimsonMagic : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public float siphonRadius = 6f;   // Area around impact
    public LayerMask enemyMask;

    [Header("Life Steal")]
    public float healPerEnemy = 10f;  // Heal per zombie hit

    [Header("VFX")]
    public GameObject EnemyImpactVFX;
    public GameObject PlayerHealVFX;  

    private Rigidbody rb;
    private bool hasImpacted = false;
    private PlayerAttributes player;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);

        // Cache player for healing
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.GetComponentInChildren<PlayerAttributes>();

        //// Ignore player collisions
        //if (p != null)
        //{
        //    Collider[] playerColliders = p.GetComponentsInChildren<Collider>();
        //    Collider myCol = GetComponent<Collider>();
        //    foreach (Collider col in playerColliders)
        //        Physics.IgnoreCollision(myCol, col);
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasImpacted) return;

        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Enemy"))
            TriggerCrimson();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;
        TriggerCrimson();
    }

    void TriggerCrimson()
    {
        hasImpacted = true;
        Vector3 impactPoint = transform.position;               //find point center of radius

        // Siphon from enemies
        Collider[] hits = Physics.OverlapSphere(impactPoint, siphonRadius, enemyMask);

        foreach (Collider col in hits)
        {
            EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();
            if (enemy != null && !enemy.IsDead())
            {
                // Damage enemy
                Vector3 dir = (enemy.transform.position - impactPoint).normalized;
                enemy.TakeDamage(999999f, dir);

                // Heal player
                if (player != null)
                {
                    player.Heal(healPerEnemy);

                    // Spawn heal VFX at player
                    if (PlayerHealVFX != null)
                    {
                        GameObject healFx = Instantiate(PlayerHealVFX, player.transform.position + Vector3.up * 1f, Quaternion.identity);
                        healFx.transform.SetParent(player.transform); // Follow the player
                        Destroy(healFx, 4f);
                    }
                }

                // Enemy hit VFX
                if (EnemyImpactVFX != null)
                {
                    GameObject fx = Instantiate(EnemyImpactVFX, enemy.transform.position, Quaternion.identity);
                    Destroy(fx, 2f);
                }
            }
        }

        Destroy(gameObject);
    }
}
