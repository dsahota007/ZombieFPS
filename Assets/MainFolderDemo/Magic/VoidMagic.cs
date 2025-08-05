using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoidMagic : MonoBehaviour
{
    [Header("Void Magic Movement")]
    public float speed = 30f;
    public float lifeTime = 10f;

    [Header("Void Death Zone")]
    public float deathRadius = 5f;
    public LayerMask enemyMask;

    [Header("Body Dragging Settings")]
    public float dragDuration = 2f;
    public float dragForce = 20f;

    [Header("VFX Effects")]
    public GameObject GroundEntitySlamVFX;
    public GameObject VoidExplosionVFX;
    public GameObject EnemyImpactVFX;

    private Rigidbody rb;
    private bool hasImpacted = false;
    private bool deathZoneActive = false;
    private Vector3 impactPoint;
    private List<EnemyHealthRagdoll> killedEnemies = new List<EnemyHealthRagdoll>();
    private List<Rigidbody> deadBodies = new List<Rigidbody>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;

        Invoke("DestroyIfStillFlying", lifeTime);
    }

    void DestroyIfStillFlying()
    {
        if (!hasImpacted)
        {
            Debug.Log("Void projectile timed out without hitting anything");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasImpacted) return;

        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Debug.Log("Void hit ground/wall via trigger");
            TriggerVoidDeathZone();
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Void hit enemy via trigger");
            TriggerVoidDeathZone();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;
        Debug.Log($"Void collision with: {collision.gameObject.name}");
        TriggerVoidDeathZone();
    }

    void TriggerVoidDeathZone()
    {
        if (hasImpacted) return;

        Debug.Log("VOID DEATH ZONE TRIGGERED!");
        hasImpacted = true;
        impactPoint = transform.position;

        CancelInvoke("DestroyIfStillFlying");

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        SpawnGroundEffects();
        InstantKillEnemiesInRange();

        deathZoneActive = true;
        StartCoroutine(MonitorDeathZone());

        StartCoroutine(WaitThenDragBodies());
    }

    void SpawnGroundEffects()
    {
        if (GroundEntitySlamVFX != null)
        {
            GameObject vfx1 = Instantiate(GroundEntitySlamVFX, impactPoint, Quaternion.identity);
            Destroy(vfx1, 10f);
            Debug.Log("Spawned ground impact VFX");
        }
        else
        {
            Debug.Log("No ground VFX assigned!");
        }
    }

    void InstantKillEnemiesInRange()
    {
        killedEnemies.Clear();
        deadBodies.Clear();

        Collider[] hits = Physics.OverlapSphere(impactPoint, deathRadius, enemyMask);
        Debug.Log($"Found {hits.Length} colliders in radius");

        foreach (Collider col in hits)
        {
            EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();
            if (enemy != null && !killedEnemies.Contains(enemy))
            {
                float distance = Vector3.Distance(enemy.transform.position, impactPoint);
                if (distance <= deathRadius)
                {
                    killedEnemies.Add(enemy);
                    Vector3 direction = (enemy.transform.position - impactPoint).normalized;
                    enemy.TakeDamage(999999f, direction);
                    Debug.Log($"Killed enemy: {enemy.name} at distance {distance}");
                }
            }
        }

        Debug.Log($"Void Magic killed {killedEnemies.Count} enemies!");
    }

    IEnumerator MonitorDeathZone()
    {
        Debug.Log("Death zone is now active!");

        while (deathZoneActive)
        {
            Collider[] hits = Physics.OverlapSphere(impactPoint, deathRadius, enemyMask);

            foreach (Collider col in hits)
            {
                EnemyHealthRagdoll enemy = col.GetComponentInParent<EnemyHealthRagdoll>();
                if (enemy != null && !killedEnemies.Contains(enemy))
                {
                    float distance = Vector3.Distance(enemy.transform.position, impactPoint);
                    if (distance <= deathRadius)
                    {
                        killedEnemies.Add(enemy);
                        Vector3 direction = (enemy.transform.position - impactPoint).normalized;
                        enemy.TakeDamage(999999f, direction);
                        Debug.Log($"Late-entry enemy killed: {enemy.name} at distance {distance}");
                    }
                }
            }

            yield return null; // Check every frame
        }

        Debug.Log("Death zone deactivated.");
    }

    IEnumerator WaitThenDragBodies()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (EnemyHealthRagdoll deadEnemy in killedEnemies)
        {
            if (deadEnemy != null && deadEnemy.ragdollRoot != null)
            {
                Rigidbody[] ragdollRbs = deadEnemy.ragdollRoot.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in ragdollRbs)
                {
                    if (rb != null && !deadBodies.Contains(rb))
                    {
                        deadBodies.Add(rb);
                    }
                }
            }
        }

        Debug.Log($"Collected {deadBodies.Count} body parts for dragging");
        StartCoroutine(DragBodiesToCenter());
    }

    IEnumerator DragBodiesToCenter()
    {
        Debug.Log("Starting body dragging sequence");
        float timer = 0f;

        while (timer < dragDuration)
        {
            timer += Time.deltaTime;

            foreach (Rigidbody bodyPart in deadBodies)
            {
                if (bodyPart != null)
                {
                    Vector3 directionToCenter = (impactPoint - bodyPart.position).normalized;
                    float distance = Vector3.Distance(impactPoint, bodyPart.position);
                    float forceMultiplier = Mathf.Lerp(0.5f, 2f, distance / deathRadius);
                    bodyPart.AddForce(directionToCenter * dragForce * forceMultiplier, ForceMode.Force);
                }
            }

            yield return null;
        }

        Debug.Log("Dragging complete - triggering explosion!");
        TriggerVoidExplosion();
    }

    void TriggerVoidExplosion()
    {
        Debug.Log($"VOID EXPLOSION! Affecting {deadBodies.Count} body parts");

        deathZoneActive = false;

        if (VoidExplosionVFX != null)
        {
            GameObject explosionVFX = Instantiate(VoidExplosionVFX, impactPoint, Quaternion.identity);
            Destroy(explosionVFX, 10f);
            Debug.Log("Spawned explosion VFX");
        }
        else
        {
            Debug.Log("No explosion VFX assigned!");
        }

        foreach (Rigidbody bodyPart in deadBodies)
        {
            if (bodyPart != null)
            {
                float dist = Vector3.Distance(impactPoint, bodyPart.transform.position);
                float force = Mathf.Lerp(50f, 60f, dist / deathRadius);

                bodyPart.AddExplosionForce(
                    force,
                    impactPoint,
                    deathRadius * 3f,
                    100f,
                    ForceMode.Impulse
                );

                Debug.Log($"Applied MASSIVE explosion force {force} to {bodyPart.name}");
            }
        }

        foreach (EnemyHealthRagdoll deadEnemy in killedEnemies)
        {
            if (deadEnemy != null && EnemyImpactVFX != null)
            {
                GameObject deathVFXEnemy = Instantiate(EnemyImpactVFX, deadEnemy.transform.position + Vector3.up * 1f, Quaternion.identity);
                Destroy(deathVFXEnemy, 5f);
            }
        }

        Debug.Log("Void explosion complete!");
        Destroy(gameObject);
    }

}
