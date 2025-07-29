using UnityEngine;
using UnityEngine.AI;

public class EnemyHealthRagdoll : MonoBehaviour
{
    public int maxHits = 3;
    public GameObject ragdollRoot;
    public float ragdollForce = 1f;
    public Collider rootCollider; // assign in Inspector or auto-get

    private int currentHits = 0;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (rootCollider == null)
            rootCollider = GetComponent<Collider>(); // auto-assign

        SetRagdollState(false);
    }

    public void RegisterHit(Vector3 hitDirection)
    {
        if (isDead) return;

        currentHits++;

        if (currentHits >= maxHits)
        {
            Die(hitDirection);
        }
    }

    void Die(Vector3 hitDirection)
    {
        isDead = true;

        if (animator != null)
            animator.enabled = false;

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        if (rootCollider != null)
            rootCollider.enabled = false;

        SetRagdollState(true);
        AddRagdollForce(hitDirection);

        // Let ragdoll react for 1 second, then disable colliders and physics
        Invoke(nameof(DisableRagdollColliders), 1.7f);

        
        Destroy(gameObject, 9f);
    }

    void SetRagdollState(bool active)
    {
        foreach (var rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = !active;

        foreach (var col in ragdollRoot.GetComponentsInChildren<Collider>())
            col.enabled = active;
    }

    void AddRagdollForce(Vector3 direction)
    {
        Rigidbody[] rbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        if (rbs.Length > 0)
        {
            Rigidbody targetBone = rbs[0]; // pick chest or hips
            targetBone.AddForce(direction * ragdollForce, ForceMode.Impulse);
        }
    }

    void DisableRagdollColliders()
    {
        foreach (var col in ragdollRoot.GetComponentsInChildren<Collider>())
            col.enabled = false;

        foreach (var rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
