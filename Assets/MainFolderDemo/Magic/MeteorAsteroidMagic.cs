using UnityEngine;

public class MeteorAsteroidMagic : MonoBehaviour   // THIS IS FIREBALL COPIED 
{
    [Header("Metor Movement")]
    public float speed = 15f;
    public float lifeTime = 5f;

    [Header("Slam Attack on Impact")]
    public float slamRadius = 5f;
    public float slamDamage = 100f;
    public LayerMask enemyMask;

    [Header("VFX Effects")]
    public GameObject EnemyImpactVFX;
    public GameObject EnemyImpactVFX2;
    public GameObject GroundEntitySlamVFX;
    public GameObject GroundEntitySlamVFX2;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.isKinematic = false;
        //rb.useGravity = false;
        // Set velocity using the correct Unity 6 API

        rb.linearVelocity = Vector3.down * speed;     //we going down not forward this is a meteor
        Destroy(gameObject, lifeTime);

        //Collider[] playerColliders = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<Collider>();  // we use this to make sure it does not hit us 

        //foreach (Collider col in playerColliders)
        //{
        //    Physics.IgnoreCollision(GetComponent<Collider>(), col);
        //}

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
        if (GroundEntitySlamVFX != null)
        {
            GameObject vfx1 = Instantiate(GroundEntitySlamVFX, transform.position, Quaternion.identity);
            Destroy(vfx1, 30f);
        }

        if (GroundEntitySlamVFX2 != null)
        {
            GameObject vfx1 = Instantiate(GroundEntitySlamVFX2, transform.position, Quaternion.identity);
            Destroy(vfx1, 10f);
        }

    }

    void ApplyFireballSlamDamage()
    {

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, slamRadius, enemyMask);     //parameter(center of sphere, radiusOFSphere, a layermask defines which layers of colliders to include in the query)
        // ^^ It checks for all colliders that are on the enemyMask layer These are the enemies within range of the slam.

        foreach (Collider enemy in hitEnemies)  //For each enemy in range, this block will:
        {
            EnemyHealthRagdoll health = enemy.GetComponent<EnemyHealthRagdoll>();  //fetch script
            if (health != null)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized; //we find direction from us teh player to enemy 
                health.TakeDamage(slamDamage, direction);  //in enemyHealthRagdoll script 

                Rigidbody[] rbs = enemy.GetComponentsInChildren<Rigidbody>();             // Apply explosion force to all rigidbodies in the enemy -- so we get rb for every enemy
                foreach (Rigidbody rb in rbs)  //For each rigidBody in range, this block will:
                {
                    if (rb != null)
                    {
                        float dist = Vector3.Distance(transform.position, rb.transform.position);
                        float force = Mathf.Lerp(35f, 45f, dist / slamRadius);
                        rb.AddExplosionForce(force, transform.position, slamRadius, 52.3f, ForceMode.Impulse); // Lower upward lift (how strong, expolision origin, hjow far explosion affects, upward modifer gives the bone vertical lift, ForceMode.Impulse is an instant kick like a punch.)

                    }
                }

                if (EnemyImpactVFX != null)
                {
                    GameObject deathVFXEnemy = Instantiate(EnemyImpactVFX, enemy.transform.position + Vector3.up * 1f, Quaternion.identity);   //spawn the vfx in 
                    Destroy(deathVFXEnemy, 5f);
                }

                if (EnemyImpactVFX2 != null)
                {
                    GameObject deathVFXEnemy = Instantiate(EnemyImpactVFX2, enemy.transform.position, Quaternion.Euler(-90f, 0f, -90f));   //spawn the vfx Again for the fire
                    Destroy(deathVFXEnemy, 25f);
                }
            }
        }
    }
}
