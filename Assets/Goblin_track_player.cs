using UnityEngine;
using UnityEngine.AI;

public class GoblinChase : MonoBehaviour
{
    public Transform player; // Assign the player GameObject in the Inspector
    private NavMeshAgent agent;
    private Animator animator;

    public float stopDistance = 2f; // Distance at which the goblin stops
    public float backUpSpeed = 1f;  // Speed at which the goblin backs up
    public float backUpDistance = 4f; // Distance to back up

    private bool isBackingUp = false;
    private Vector3 backUpTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Get NavMeshAgent component
        animator = GetComponent<Animator>();  // Get Animator component
    }

    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (isBackingUp)
            {
                // Move the goblin backward
                agent.SetDestination(backUpTarget);

                // If the goblin has backed up enough, stop
                if (Vector3.Distance(transform.position, backUpTarget) < 0.5f)
                {
                    isBackingUp = false;
                    agent.ResetPath(); // Stop moving
                }
            }
            else if (distanceToPlayer > stopDistance)
            {
                // Move towards the player
                agent.SetDestination(player.position);
            }
            else
            {
                // Stop in front of the player and start backing up
                agent.ResetPath();
                isBackingUp = true;
                Vector3 directionAway = (transform.position - player.position).normalized;
                backUpTarget = transform.position + directionAway * backUpDistance;
                agent.speed = backUpSpeed; // Slow down for the backing up
                agent.SetDestination(backUpTarget);
            }

            // Update animation based on speed
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
}
