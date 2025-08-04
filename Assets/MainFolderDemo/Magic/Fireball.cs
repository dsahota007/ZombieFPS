using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Fireball Movement")]
    public float speed = 15f;
    public float lifeTime = 5f;

    [Header("Slam Attack on Impact")]
    public float slamRadius = 5f;
    public float slamDamage = 100f;
    public LayerMask enemyMask;

    [Header("VFX Effects")]
    public GameObject slamImpactVFX;
    public GameObject KineticUnderneathSlamImpactVFX;
    public GameObject KineticUnderneathSlamImpactVFX3;
    public GameObject KineticUnderneathSlamImpactVFX5;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Fireball: No Rigidbody found! Adding one...");
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Make sure rigidbody isn't kinematic
        rb.isKinematic = false;
        rb.useGravity = true; // Add some gravity for realistic arc

        // Set velocity using the correct Unity 6 API
        rb.linearVelocity = transform.forward * speed;

        Debug.Log($"Fireball spawned! Direction: {transform.forward}, Speed: {speed}, Velocity: {rb.linearVelocity}");

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Fireball hit: {other.name} with tag: {other.tag}");

        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            ApplyFireballSlamDamage();
            SpawnGroundEffects();
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            ApplyFireballSlamDamage();
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Fireball collided with: {collision.gameObject.name}");

        // Backup collision detection in case trigger doesn't work
        ApplyFireballSlamDamage();
        SpawnGroundEffects();
        Destroy(gameObject);
    }

    void SpawnGroundEffects()
    {
        if (KineticUnderneathSlamImpactVFX != null)
        {
            GameObject vfx1 = Instantiate(KineticUnderneathSlamImpactVFX, transform.position, Quaternion.identity);
            Destroy(vfx1, 10f);
        }

        if (KineticUnderneathSlamImpactVFX3 != null)
        {
            GameObject vfx2 = Instantiate(KineticUnderneathSlamImpactVFX3, transform.position, Quaternion.identity);
            Destroy(vfx2, 10f);
        }

        if (KineticUnderneathSlamImpactVFX5 != null)
        {
            GameObject vfx3 = Instantiate(KineticUnderneathSlamImpactVFX5, transform.position, Quaternion.identity);
            Destroy(vfx3, 10f);
        }
    }

    void ApplyFireballSlamDamage()
    {
        Debug.Log($"Fireball exploding at: {transform.position}");

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, slamRadius, enemyMask);
        Debug.Log($"Fireball hit {hitEnemies.Length} enemies");

        foreach (Collider enemy in hitEnemies)
        {
            EnemyHealthRagdoll health = enemy.GetComponent<EnemyHealthRagdoll>();
            if (health != null)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                health.TakeDamage(slamDamage, direction);

                Rigidbody[] rbs = enemy.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rbs)
                {
                    if (rb != null)
                    {
                        float dist = Vector3.Distance(transform.position, rb.transform.position);
                        float force = Mathf.Lerp(105f, 105f, dist / slamRadius);
                        rb.AddExplosionForce(force, transform.position, slamRadius, 1552.3f, ForceMode.Impulse);
                    }
                }

                if (slamImpactVFX != null)
                {
                    GameObject deathVFXEnemy = Instantiate(slamImpactVFX, enemy.transform.position + Vector3.up * 1f, Quaternion.identity);
                    Destroy(deathVFXEnemy, 5f);
                }
            }
        }
    }
}
