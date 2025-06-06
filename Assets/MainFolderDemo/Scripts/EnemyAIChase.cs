using UnityEngine;
using UnityEngine.AI;

public class EnemyAIChase : MonoBehaviour
{
    private NavMeshAgent enemyAgent;
    public Transform target;
    private Animator animator;

    void Start()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();


        if (!enemyAgent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))    // sample pos is a vec3 (startin point, result container?, max distance to search, which areas.
            {
                enemyAgent.Warp(hit.position);
                Debug.Log("Warped enemy to NavMesh.");
            }
            else
            {
                Debug.LogError("Enemy is not near a valid NavMesh area!");
            }
        }
    }

    void Update()
    {
        if (target != null && enemyAgent.isOnNavMesh)
        {
            enemyAgent.SetDestination(target.position);   //keeps updating its destination to follow the target's current position
        }

        if (animator != null)
        {
            float speed = enemyAgent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
            Debug.LogError("Animator component is missing!");
             }

        else
            {
                Debug.Log("Animator found on enemy.");
            }
        }
}
