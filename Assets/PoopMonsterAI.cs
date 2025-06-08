using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PoopMonsterAI : MonoBehaviour
{
    [Header("Detection & Attack")]
    public float detectionRadius = 10f;
    public float attackRange = 2f;
    public float attackForce = 15f;

    [Header("Projectile")]
    public GameObject poopPrefab;
    public Transform firePoint;

    [Header("Animator")]
    [SerializeField] private Animator animator;  // Assign manually in Inspector

    private Transform player;
    private NavMeshAgent agent;
    private bool isAttacking;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackRange - 0.1f;  // Stop just before attack range

        if (animator == null)
        {
            Debug.LogError("Animator not assigned. Assign it in the Inspector.");
        }
        else if (animator.applyRootMotion)
        {
            Debug.LogWarning("Animator root motion is enabled. Disabling it.");
            animator.applyRootMotion = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found. Tag your player as 'Player'.");
        }
    }

    void Update()
    {
        if (player == null || animator == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Debug line
        Debug.DrawLine(transform.position, player.position, Color.red);

        if (isAttacking)
        {
            agent.SetDestination(transform.position); // Stop moving
            FaceTarget();
            return;
        }

        if (distance <= detectionRadius && distance > attackRange)
        {
            ChasePlayer();
        }
        else if (distance <= attackRange)
        {
            StartCoroutine(AttackPlayer());
        }
        else
        {
            Idle();
        }

        // Set animator speed for blend trees or locomotion
        animator.SetFloat("speed", agent.velocity.magnitude);
    }

    void ChasePlayer()
    {
        if (!agent.isOnNavMesh) return;

        agent.SetDestination(player.position);
        animator.SetBool("chase", true);
        animator.SetBool("attack", false);
    }

    void Idle()
    {
        if (!agent.isOnNavMesh) return;

        agent.SetDestination(transform.position); // Stay in place
        animator.SetBool("chase", false);
        animator.SetBool("attack", false);
    }

    void FaceTarget()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;

        if (!agent.isOnNavMesh) yield break;

        animator.SetBool("attack", true);
        animator.SetBool("chase", false);
        agent.SetDestination(transform.position); // Hold still
        FaceTarget();

        yield return new WaitForSeconds(0.3f); // Sync with animation

        if (poopPrefab && firePoint)
        {
            GameObject poop = Instantiate(poopPrefab, firePoint.position, firePoint.rotation);
            Rigidbody poopRb = poop.GetComponent<Rigidbody>();
            if (poopRb != null)
            {
                Vector3 dir = (player.position - firePoint.position).normalized;
                poopRb.AddForce(dir * attackForce, ForceMode.VelocityChange);
            }
        }

        yield return new WaitForSeconds(1f); // Cooldown
        isAttacking = false;
    }
}
