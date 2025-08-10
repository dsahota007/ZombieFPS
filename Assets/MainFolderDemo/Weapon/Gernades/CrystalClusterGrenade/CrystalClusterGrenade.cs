using UnityEngine;
using System.Collections;

public class CrystalClusterGrenade : MonoBehaviour
  
{
    [Header("Fuse & Explosion")]
    public float fuseTime = 5f;
    public float explosionRadius = 5f;
    public float explosionDamage = 999999f;
    public float explosionForce = 45f;
    public float upwardModifier = 0.5f;
    public LayerMask enemyMask;

    [Header("VFX")]
    public GameObject explosionVFX;

    [Header("Physics")]
    public float spinTorque = 5f;  // small spin for style

    private Rigidbody rb;
    private bool exploded = false;

    [Header("Cluster")]
    public bool spawnChildren = true;          // parent spawns children; children won't
    public int clusterCount = 3;               // how many to spawn
    public float childFuseTime = 1.25f;        // quick fuse on the minis
    public float childLaunchSpeed = 8f;        // outward kick
    public float inheritVelocityFactor = 0.35f;// inherit some of parent speed
    public GameObject grenadePrefab;           // assign this same Grenade prefab in Inspector


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

        Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        Destroy(Instantiate(explosionVFX, pos, randomRot), 5f);

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
        SpawnChildren(pos);
        Destroy(gameObject); // remove the original grenade
    }



    void SpawnChildren(Vector3 pos)
    {
        if (!spawnChildren || !grenadePrefab) return;

        Vector3 parentVel = rb ? rb.linearVelocity : Vector3.zero;  //the grenade’s current velocity; minis will inherit some

        // pick a random flat (horizontal) direction to start from
        Vector3 baseDir = Random.insideUnitSphere; baseDir.y = 0f;
        if (baseDir.sqrMagnitude < 0.001f) baseDir = Vector3.forward;
        baseDir.Normalize();   //direction

        float step = 360f / Mathf.Max(1, clusterCount);

        for (int i = 0; i < clusterCount; i++)   //loop thru spawn 
        {
            Vector3 dir = Quaternion.Euler(0f, step * i, 0f) * baseDir;
            Vector3 vel = dir * childLaunchSpeed + Vector3.up * 2f + parentVel * inheritVelocityFactor;
            Vector3 spawnPos = pos + dir * 1.4f + Vector3.up * 0.2f;  

            GameObject child = Instantiate(grenadePrefab, spawnPos, Quaternion.identity);   
            var g = child.GetComponent<CrystalClusterGrenade>();
            if (g)
            {
                g.spawnChildren = false;
                g.fuseTime = childFuseTime;
                g.ApplyThrow(vel);   //automatically throws in teh direction of our calc
            }
        }
    }

    public void ApplyThrow(Vector3 velocity)
    {
        if (rb != null) rb.linearVelocity = velocity;
    }
}
