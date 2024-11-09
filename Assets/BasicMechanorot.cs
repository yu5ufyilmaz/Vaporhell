using UnityEngine;
using System.Collections;

public class BasicMechanorot : EnemyBase
{
    public int maxHealth = 100;
    private int currentHealth;

    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public int damage = 50;
    public float attackCooldown = 2f;
    public float patrolRange = 5f; 
    public float minPatrolDistance = 15f; // Minimum hareket mesafesi
    public float waitTimeAtPatrolPoint = 2f; 
    public float runSpeedMultiplier = 1.5f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isAttacking = false;
    private bool isPatrolling = true;
    private bool isWaiting = false;
    private Vector2 patrolStartPosition;
    private Vector2 patrolTarget;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        patrolStartPosition = transform.position;
        SetNewPatrolTarget();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange && !isAttacking)
        {
            MoveTowardsPlayer();
        }
        else if (distanceToPlayer <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }
        else if (isPatrolling && !isAttacking)
        {
            Patrol();
        }
        else
        {
            Idle(); // Hiçbir koşul sağlanmıyorsa Idle durumuna geç
        }
    }

    void MoveTowardsPlayer()
    {
        animator.SetBool("isWalking", true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed * runSpeedMultiplier, rb.velocity.y);

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(-0.14f, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(0.14f, transform.localScale.y, transform.localScale.z);
        }
    }

    void Patrol()
    {
        if (isWaiting) return;

        animator.SetBool("isWalking", true);

        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            StartCoroutine(WaitAndSetNewPatrolTarget());
        }

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(-0.1f, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(0.1f, transform.localScale.y, transform.localScale.z);
        }
    }

    IEnumerator WaitAndSetNewPatrolTarget()
    {
        isWaiting = true;
        rb.velocity = Vector2.zero;
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(waitTimeAtPatrolPoint);

        SetNewPatrolTarget();
        isWaiting = false;
    }

    void SetNewPatrolTarget()
    {
        float patrolOffset;
        do
        {
            patrolOffset = Random.Range(-patrolRange, patrolRange);
        }
        while (Mathf.Abs(patrolOffset) < minPatrolDistance); // Min mesafe kontrolü

        patrolTarget = patrolStartPosition + new Vector2(patrolOffset, 0);
    }

    void Idle()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("isWalking", false);
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        int randomAttackAnimation = Random.Range(0, 2);
        if (randomAttackAnimation == 0)
        {
            animator.SetTrigger("Damage1");
        }
        else
        {
            animator.SetTrigger("Damage2");
        }

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);
        currentHealth -= damageAmount;

        animator.SetTrigger("TakeDamage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetBool("isDead", true);
        rb.velocity = Vector2.zero;
        isPatrolling = false;
        enabled = false;
    }
}
