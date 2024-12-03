using UnityEngine;
using System.Collections;

public class BasicMechanorot : EnemyBase
{
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int Damage1 = Animator.StringToHash("Damage1");
    private static readonly int Damage2 = Animator.StringToHash("Damage2");
    private static readonly int Damage = Animator.StringToHash("TakeDamage");
    private static readonly int IsDead = Animator.StringToHash("isDead");
    public int maxHealth = 100;
    private int currentHealth;

    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public int damage = 10;
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
        animator.SetBool(IsWalking, true);

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
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage); // Oyuncuya hasar ver
            }
        }
    }


    void Patrol()
    {
        if (isWaiting) return;

        animator.SetBool(IsWalking, true);

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
        animator.SetBool(IsWalking, false);

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
        animator.SetBool(IsWalking, false);
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        // Rastgele bir saldırı animasyonu oynat
        int randomAttackAnimation = Random.Range(0, 2);
        if (randomAttackAnimation == 0)
        {
            animator.SetTrigger(Damage1);
        }
        else
        {
            animator.SetTrigger(Damage2);
        }

        yield return new WaitForSeconds(0.5f); // Saldırı animasyonunun yarısında hasar verir

        // Oyuncuya hasar ver
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage); // Oyuncunun canını azalt
            }
        }

        // Saldırı bekleme süresi
        yield return new WaitForSeconds(attackCooldown - 0.5f);
        isAttacking = false;
    }


    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);
        currentHealth -= damageAmount;

        animator.SetTrigger(Damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetBool(IsDead, true);
        rb.velocity = Vector2.zero;
        isPatrolling = false;
        enabled = false;
    }
}
