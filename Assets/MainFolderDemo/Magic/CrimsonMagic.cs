using UnityEngine;
using System.Collections;
public class CrimsonMagic : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public float siphonRadius = 6f;   // Area around impact
    public LayerMask enemyMask;

    [Header("Life Steal")]
    public float healPerEnemy = 10f;  // Heal per zombie hit
    //public int maxHealVFX = 3;                 // optional cap
    private static int activeHealVFXCount = 0; // optional cap
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
        Vector3 impactPoint = transform.position;

        // damage & heal logic
        Collider[] hits = Physics.OverlapSphere(impactPoint, siphonRadius, enemyMask);
        foreach (var col in hits)
        {
            var enemy = col.GetComponentInParent<EnemyHealthRagdoll>();
            if (enemy == null || enemy.IsDead()) continue;

            Vector3 dir = (enemy.transform.position - impactPoint).normalized;
            enemy.TakeDamage(999999f, dir);

            if (player != null)
                player.Heal(healPerEnemy);

            if (EnemyImpactVFX != null)
            {
                var fx = Instantiate(EnemyImpactVFX, enemy.transform.position, Quaternion.identity);
                Destroy(fx, 2f);
            }
        }

        // ? Spawn heal VFX exactly ONCE per projectile impact (optional cap)
        if (player != null && PlayerHealVFX != null)
        {
            //if (activeHealVFXCount < 999999)           // remove this if you don't want a cap
            //{
                var healFx = Instantiate( PlayerHealVFX, player.transform.position + Vector3.up * 2.85f, Quaternion.Euler(180f, 0f, 0f));
                healFx.transform.SetParent(player.transform, true);
                activeHealVFXCount++;
                Destroy(healFx, 4f);
                StartCoroutine(DecHealFxAfter(4f));
            
        }

        Destroy(gameObject);
    }

    IEnumerator DecHealFxAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        activeHealVFXCount = Mathf.Max(0, activeHealVFXCount - 1);
    }
}
