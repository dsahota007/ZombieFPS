using UnityEngine;
using UnityEngine.AI;

public class EnemyHealthRagdoll : MonoBehaviour
{
    public int maxHits = 3;
    public GameObject ragdollRoot;
    public float ragdollForce = 3f;
    public Collider rootCollider;

    private int currentHits = 0;
    private bool isDead = false;

    private Animator animator;
    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        if (rootCollider == null)
            rootCollider = GetComponent<Collider>();

        SetRagdollState(false);

        // Ignore collisions between PlayerBody and DeadBody layers
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerBody"), LayerMask.NameToLayer("DeadBody"));
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

        if (animator) animator.enabled = false;
        if (agent) agent.enabled = false;
        if (rootCollider) rootCollider.enabled = false;

        SetRagdollState(true);
        ApplyRagdollForce(hitDirection);

        // Change layer to DeadBody (no collision with player)
        SetLayerRecursively(ragdollRoot, LayerMask.NameToLayer("DeadBody"));

        // Dynamically ignore collisions between this ragdoll and the Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            foreach (var ragdollCol in ragdollRoot.GetComponentsInChildren<Collider>())
            {
                foreach (var playerCol in player.GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(ragdollCol, playerCol, true);
                }
            }
        }

        Destroy(gameObject, 9f);
    }


    void SetRagdollState(bool enabled)
    {
        foreach (var rb in ragdollRoot.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = !enabled;

        foreach (var col in ragdollRoot.GetComponentsInChildren<Collider>())
            col.enabled = enabled;
    }

    void ApplyRagdollForce(Vector3 direction)
    {
        var rbs = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        if (rbs.Length > 0)
            rbs[0].AddForce(direction * ragdollForce, ForceMode.Impulse);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}