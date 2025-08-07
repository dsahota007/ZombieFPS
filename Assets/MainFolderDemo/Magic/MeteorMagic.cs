using UnityEngine;

public class MeteorMagic : MonoBehaviour
{
    [Header("Meteor Settings")]
    public float speed = 20f;
    public float lifeTime = 5f;
    public GameObject downwardPrefab;              // Your MeteorAsteroidMagic prefab
    public float spawnHeightAboveImpact = 80f;     // How far above the impact point to spawn
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

        //if (((1 << other.gameObject.layer) & deadLayerMask) != 0)
        //    return;

        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            TriggerMeteorEffect();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;

        //if (((1 << collision.gameObject.layer) & deadLayerMask) != 0)
        //    return;

        TriggerMeteorEffect();
    }

    void TriggerMeteorEffect()
    {
        hasImpacted = true;
        Vector3 impactPoint = transform.position;

        // Spawn VFX
        if (GroundImpactVFX != null)
        {
            GameObject vfx = Instantiate(GroundImpactVFX, impactPoint, Quaternion.identity);
            Destroy(vfx, 5f);
        }

        // Spawn asteroid above the impact point
        if (downwardPrefab != null)
        {

            Vector3 spawnPoint = impactPoint + Vector3.up * spawnHeightAboveImpact;               // Spawn position above impact
            Quaternion spawnRotation = Quaternion.Euler(0f,90f,0f);            // Face the prefab in that direction
            Instantiate(downwardPrefab, spawnPoint, spawnRotation);
        }

        Destroy(gameObject);
    }
}
