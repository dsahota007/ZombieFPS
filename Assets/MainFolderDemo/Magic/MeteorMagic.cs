using UnityEngine;
using System.Collections;

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

        if (((1 << collision.gameObject.layer) & deadLayerMask) != 0)    //---------------------------------------
            return;

        TriggerMeteorEffect();
    }

    IEnumerator SpawnMeteorShower(Vector3 impactPoint)
    {
        int meteorCount = Random.Range(3, 6);  // 3 to 5 meteors

        for (int i = 0; i < meteorCount; i++)
        {
            // Random offset in XZ within 5-unit radius
            Vector2 randomCircle = Random.insideUnitCircle * 15f;
            Vector3 offset = new Vector3(randomCircle.x, 0f, randomCircle.y);

            Vector3 spawnPoint = impactPoint + offset + Vector3.up * spawnHeightAboveImpact;

            // Slight random diagonal direction
            Vector3 randomDir = (Vector3.down + Random.insideUnitSphere * 0.4f).normalized;
            Quaternion spawnRotation = Quaternion.LookRotation(randomDir);

            Instantiate(downwardPrefab, spawnPoint, spawnRotation);
            Instantiate(downwardPrefab, spawnPoint, spawnRotation);
            Instantiate(downwardPrefab, spawnPoint, spawnRotation);
            Instantiate(downwardPrefab, spawnPoint, spawnRotation);
            Instantiate(downwardPrefab, spawnPoint, spawnRotation);
            Instantiate(downwardPrefab, spawnPoint, spawnRotation);

            // Random delay between 0.05 to 0.25 seconds
            yield return new WaitForSeconds(0.5f);

        }
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

        StartCoroutine(SpawnMeteorShower(impactPoint));



        // Spawn asteroid above the impact point
        //if (downwardPrefab != null)
        //{

        //    Vector3 spawnPoint = impactPoint + Vector3.up * spawnHeightAboveImpact;               // Spawn position above impact
        //    Vector3 randomDir = (Vector3.down + Random.insideUnitSphere * 0.4f).normalized;     // Random diagonal direction (mostly downward)
        //    Quaternion spawnRotation = Quaternion.LookRotation(randomDir);            // Face the prefab in that direction
        //    Instantiate(downwardPrefab, spawnPoint, spawnRotation);
        //}

        Destroy(gameObject);
    }
}
