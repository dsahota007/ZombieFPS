using UnityEngine;

public class MeteorMagic : MonoBehaviour
{
    [Header("Meteor Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public GameObject downwardPrefab;           // Your MeteorAsteroidMagic prefab
    public Vector3 MeteorSpawnWorldPosition = new Vector3(5f, 20f, -3f);  // Set this in inspector or code
    public LayerMask deadLayerMask;

    [Header("VFX")]
    public GameObject GroundImpactVFX;

    private Rigidbody rb;
    private bool hasImpacted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);

        // Ignore player colliders
        Collider[] playerColliders = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<Collider>();
        foreach (Collider col in playerColliders)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), col);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasImpacted) return;

        if (((1 << other.gameObject.layer) & deadLayerMask) != 0)
            return;

        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            TriggerMeteorEffect();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;

        if (((1 << collision.gameObject.layer) & deadLayerMask) != 0)
            return;

        TriggerMeteorEffect();
    }

    void TriggerMeteorEffect()
    {
        hasImpacted = true;

        // Spawn VFX at impact
        if (GroundImpactVFX != null)
        {
            GameObject vfx = Instantiate(GroundImpactVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 5f);
        }

        // Spawn downward prefab (e.g., MeteorAsteroidMagic) at the chosen world position
        if (downwardPrefab != null)
        {
            Instantiate(downwardPrefab, MeteorSpawnWorldPosition, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
