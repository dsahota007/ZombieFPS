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


    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        { 
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Make sure rigidbody isn't kinematic
        rb.isKinematic = false;
        rb.useGravity = true; // Add some gravity for realistic arc

        // Set velocity using the correct Unity 6 API
        rb.linearVelocity = transform.forward * speed;
         

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    { 

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

    }

    void ApplyFireballSlamDamage()
    {

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, slamRadius, enemyMask);
 
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
                        float force = Mathf.Lerp(35f, 45f, dist / slamRadius);
                        rb.AddExplosionForce(force, transform.position, slamRadius, 52.3f, ForceMode.Impulse);
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
