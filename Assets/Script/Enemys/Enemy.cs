using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer, whatIsWall;

    // Patroling 
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange;
    public bool playerInAttackRange;

    public float health;


    
    private bool isAttaking;

    private EnemyFire enemyFire; // Référence à EnemyFire

    private void Awake()
    {
        player = GameObject.Find("Character").transform;
        agent = GetComponent<NavMeshAgent>();
        enemyFire = GetComponent<EnemyFire>(); // Récupère la référence à EnemyFire
    }

    private void Update()
    {
        // Vérifie la position du joueur par rapport aux zones de détection
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Comportement en fonction de la position du joueur
        if (!playerInSightRange && !playerInAttackRange)
        {
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInSightRange && playerInAttackRange && HasClearLineOfSight())
        {
            Attack();
        }
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void Attack()
    {
        agent.SetDestination(transform.position);

        //Debug.Log("Attaque D point ");

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            enemyFire.Shoot();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }

    }


    public bool HasClearLineOfSight()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        Debug.DrawRay(transform.position, directionToPlayer.normalized * distanceToPlayer, Color.red); // Débogage du raycast

        if (!Physics.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, whatIsWall))
        {
            return true; // Pas d'obstacle détecté
        }
        return false; // Un obstacle est détecté
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        if (isAttaking) Attack();

    }

    public void TakeDamagee(int damage)
    {
        health -= damage;
        Debug.Log("Dégats reçus : " + damage + " - Vie restante : " + health);
        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    //Debug 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}