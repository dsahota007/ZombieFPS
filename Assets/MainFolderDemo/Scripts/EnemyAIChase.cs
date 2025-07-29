using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAIChase : MonoBehaviour
{
    private NavMeshAgent enemyAgent;
    public Transform target;
    private Animator animator;

    [Header("Combat Settings")]
    public float attackDistance = 2f;
    private bool isAttackingPlayer = false;

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

    IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(1f); // delay before can attack again
        isAttackingPlayer = false;
    }


    void Update()
    {
        if (target != null && enemyAgent.isOnNavMesh)
        {
            enemyAgent.SetDestination(target.position);   //keeps updating its destination to follow the target's current position

            float distanceFromPlayer = Vector3.Distance(transform.position, target.position);  //(the zomb, to player)
            if (distanceFromPlayer <= attackDistance && !isAttackingPlayer)
            {
                // Trigger attack
                isAttackingPlayer = true;
                animator.SetTrigger("Attack");
                Debug.Log($"Enemy {gameObject.name} is attacking the player!");

                // Optional: Reset attack after delay
                StartCoroutine(ResetAttackCooldown());
            }

        }
        if (animator != null)
        {
            float speed = enemyAgent.velocity.magnitude;   //gets the speed
            animator.SetFloat("Speed", speed);   // than we getting the speed set to that 

            // Assign RunIndex when starting to move
            if (speed > 0.1f && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                int randomRun = Random.Range(0, 10);
                animator.SetInteger("RunIndex", randomRun);
                //Debug.Log($"Switched to RunIndex: {randomRun}");
            }
        }
    }
}
