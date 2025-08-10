using UnityEngine;
using UnityEngine.AI;   //we need this to channel NavMesh 
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ShieldGrenade : MonoBehaviour
{
    [Header("Shield")]
    public float fuseTime = 1.0f;              // wait before deploying the shield
    public float shieldRadius = 6f;            // navmesh carve radius (flat disk)

    //public bool continuousKill = true;         // keep killing anything inside while active

    [Header("Targeting & Damage")]
    public LayerMask enemyMask;                // enemies layer(s)
    public float killDamage = 999999f;         // insta-kill

    [Header("VFX")]
    public GameObject shieldVFX;               // spawned at center; YOU control scale in prefab
    public float vfxLifetime = 10f;            // how long VFX lives

    [Header("Physics")]
    public float spinTorque = 5f;              // small spin for style

    private Rigidbody rb;
    private bool deployed = false;

    // internal carve height so you don't have to set it in Inspector
    private const float kObstacleHeight = 10f;

    void Awake()
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
        if (spinTorque > 0f)
            rb.AddTorque(Random.onUnitSphere * spinTorque, ForceMode.Impulse);

        StartCoroutine(FuseRoutine());
    }

    IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode(); // "explode" == deploy shield
    }


    void Explode()
    {
        if (deployed) return;
        deployed = true;

        Vector3 pos = transform.position;
         
        if (shieldVFX != null)
            Destroy(Instantiate(shieldVFX, pos, Quaternion.identity), vfxLifetime);
         
        GameObject shieldRoot = new GameObject("ShieldZone");    // Create shield root (holds obstacle + optional trigger killer)
        shieldRoot.transform.position = pos;            //save postion of center

        // Carve NavMesh so agents can't path into the area
        var obstacle = shieldRoot.AddComponent<NavMeshObstacle>();
        obstacle.shape = NavMeshObstacleShape.Capsule; // circular footprint
        obstacle.radius = shieldRadius;
        obstacle.height = kObstacleHeight;             // internal, hidden from inspector
        obstacle.carving = true;
        obstacle.carveOnlyStationary = true;

        // One-time kill for anything already inside
        KillAgentsInsideNow(pos);

        //// Continuous kill while active (optional)
        //if (continuousKill)
        //{
        //    var trigger = shieldRoot.AddComponent<SphereCollider>();
        //    trigger.isTrigger = true;
        //    trigger.radius = shieldRadius + 0.05f; // tiny pad for edge cases

        //    var killer = shieldRoot.AddComponent<ShieldZoneKiller>();
        //    killer.radius = shieldRadius;
        //    killer.enemyMask = enemyMask;
        //    killer.killDamage = killDamage;
        //}

        // Clean up shield after duration
        Destroy(shieldRoot, 10f);

        // Remove the grenade object
        Destroy(gameObject);
    }

    void KillAgentsInsideNow(Vector3 center)
    {
        // fast path: only enemy layers if provided
        Collider[] hits = Physics.OverlapSphere(center, shieldRadius + 0.25f, enemyMask);  //eveyrone in the sphere
        if (hits.Length == 0)
        {
            // fallback: scan all if mask not set
            hits = Physics.OverlapSphere(center, shieldRadius + 0.5f);
        }

        foreach (var h in hits)
        {
            var health = h.GetComponentInParent<EnemyHealthRagdoll>();
            if (health != null)
            {
                Vector3 dir = (h.transform.position - center).normalized;   //kill em 
                health.TakeDamage(killDamage, dir);
            }
        }
    }

    public void ApplyThrow(Vector3 velocity)
    {
        if (rb != null) rb.linearVelocity = velocity;
    }
}



public class ShieldZoneKiller : MonoBehaviour
{
    public float radius = 6f;
    public LayerMask enemyMask;
    public float killDamage = 999999f;

    void OnTriggerStay(Collider other)
    {
        // optional layer filter
        if (enemyMask.value != 0)
        {
            if ((enemyMask.value & (1 << other.gameObject.layer)) == 0)
                return;
        }

        // confirm actually inside radial footprint (handles tall colliders)
        Vector3 c = transform.position;
        Vector3 p = other.transform.position;
        p.y = c.y;

        if ((p - c).sqrMagnitude > radius * radius)
            return;

        var health = other.GetComponentInParent<EnemyHealthRagdoll>();
        if (health == null) return;

        Vector3 dir = (other.transform.position - c).normalized;
        health.TakeDamage(killDamage, dir);
    }
}
