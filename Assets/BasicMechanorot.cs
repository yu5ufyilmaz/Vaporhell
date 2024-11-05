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
    public Transform[] patrolPoints;
    public float waitTimeAtPatrolPoint = 2f;
    public float runSpeedMultiplier = 1.5f; // Player'a koşarken hız artışı için çarpan

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isAttacking = false;
    private bool isPatrolling = true;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange && !isAttacking)
        {
            MoveTowardsPlayer();
        }
        else if (distanceToPlayer <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }
        else if (isPatrolling)
        {
            Patrol();
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    void MoveTowardsPlayer()
    {
        // Player'a doğru koşarken animasyon hızını artır
        animator.speed = 1.5f; // Hızlandırılmış animasyon
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed * runSpeedMultiplier, rb.velocity.y);

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(-0.1f, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(0.1f, transform.localScale.y, transform.localScale.z);
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }

    void Patrol()
    {
        // Devriye sırasında animasyon hızını varsayılana döndür
        animator.speed = 1.0f;

        if (patrolPoints.Length == 0) return;
        if (isWaiting) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }

        if (direction.x > 0)
        {
            transform.localScale = new Vector3(-0.1f, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(0.1f, transform.localScale.y, transform.localScale.z);
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        int attackType = Random.Range(0, 2);
        if (animator != null)
        {
            animator.SetTrigger(attackType == 0 ? "Attack1" : "Attack2");
        }

        yield return new WaitForSeconds(1f);

        isAttacking = false;
    }

    IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        rb.velocity = Vector2.zero;
        animator.SetBool("isMoving", false);

        yield return new WaitForSeconds(waitTimeAtPatrolPoint);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        isWaiting = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }
            Destroy(other.gameObject);
        }
    }

    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);

        currentHealth -= damageAmount;

        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        rb.velocity = Vector2.zero;
        isPatrolling = false;
        enabled = false;
    }
}
