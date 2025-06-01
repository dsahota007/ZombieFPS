using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public GameObject playerTarget;


    void Start()
    {
        if (!navMeshAgent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position); // Snap to nearest navmesh point
            }
            else
            {
                Debug.LogError("Enemy is not near any NavMesh!");
            }
        }
    }

    void Update()
    {
        navMeshAgent.SetDestination(playerTarget.transform.position);
    }

}
