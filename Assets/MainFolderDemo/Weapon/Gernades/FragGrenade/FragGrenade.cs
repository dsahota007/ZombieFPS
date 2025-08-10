using UnityEngine;
using System.Collections;

public class FragGrenade : MonoBehaviour
{
    [Header("Fuse & Explosion")]
    public float fuseTime = 5f;
    public float explosionRadius = 5f;
    public float explosionDamage = 999999f;
    public float explosionForce = 25f;
    public float upwardModifier = 0.5f;
    public LayerMask enemyMask;

    [Header("VFX")]
    public GameObject explosionVFX;
    public float vfxLifetime = 5f;

    [Header("Physics")]
    public float spinTorque = 5f;  // small spin for style

    private Rigidbody rb;
    private bool exploded = false;

    void Awake()       //Awake(): A Unity lifecycle method that runs before Start(),
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Start()
    {
        // Add a little spin so it rolls naturally
        if (spinTorque > 0f)
            rb.AddTorque(Random.onUnitSphere * spinTorque, ForceMode.Impulse);    //spin logic

        StartCoroutine(FuseRoutine());
    }

    IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseTime);   // wait for to explode
        Explode();
    }

    void Explode()
    {
        if (exploded) return;   //if already exploded leave this code
        exploded = true;

        Vector3 pos = transform.position;       //we get the positoinf of explosion for vfx

        if (explosionVFX != null)          // Spawn explosion VFX
            Destroy(Instantiate(explosionVFX, pos, Quaternion.identity), vfxLifetime);

        Collider[] hits = Physics.OverlapSphere(pos, explosionRadius, enemyMask);    //get the point of explosion, teh radius of whoever needs to be in enemyMask
        foreach (var hit in hits)
        {
            var health = hit.GetComponentInParent<EnemyHealthRagdoll>();
            if (health != null)
            {
                Vector3 dir = (hit.transform.position - pos).normalized;    //find direction of impact  
                health.TakeDamage(explosionDamage, dir);                //damage in that direction

                // Push ragdoll bodies if available
                if (health.ragdollRoot != null)
                {
                    foreach (var part in health.ragdollRoot.GetComponentsInChildren<Rigidbody>())           
                        part.AddExplosionForce(explosionForce, pos, explosionRadius, upwardModifier, ForceMode.Impulse);    
                }
            }
        }

        Destroy(gameObject);
    }
    public void ApplyThrow(Vector3 velocity)
    {
        if (rb != null) rb.linearVelocity = velocity;
    }
}
