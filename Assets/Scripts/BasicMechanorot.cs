using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BasicMechanorot : EnemyBase
{
    // Animation Hashes
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int Damage1 = Animator.StringToHash("Damage1");
    private static readonly int Damage2 = Animator.StringToHash("Damage2");
    private static readonly int Damage = Animator.StringToHash("TakeDamage");
    private static readonly int IsDead = Animator.StringToHash("isDead");

    [Header("Combat Parameters")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public int damage = 10;
    public float attackCooldown = 2f;

    [Header("Patrol Parameters")]
    public float patrolRange = 5f;
    public float minPatrolDistance = 1.5f;
    public float waitTimeAtPatrolPoint = 2f;
    public float runSpeedMultiplier = 1.5f;
    public float groundCheckDistance = 2f;
    public Transform groundCheck;
    public LayerMask groundLayer;


    [Header("Health Bar Parameters")]
    public GameObject healthBarPrefab;
    private Slider healthBarSlider;
    private Canvas healthBarCanvas;
    public float healthBarDisplayDuration = 3f;
    private Coroutine hideHealthBarCoroutine;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private bool isAttacking = false;
    private bool isPatrolling = true;
    private bool isWaiting = false;
    private Vector2 patrolStartPosition;
    private Vector2 patrolTarget;
    
    // Base Scale for consistent flipping
    private float baseScaleX;

    // ---------------------------------------------------------
    //  1) Start'ta maxHealth'i özelleştirip base.Start() çağır
    // ---------------------------------------------------------
    protected override void Start()
    {
        maxHealth = 120;  // Bu düşman için mesela 120 olsun
        base.Start();     // EnemyBase.Start() → currentHealth = maxHealth = 120

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("BasicMechanorot: Player not found. Ensure the player has the 'Player' tag.");
        }

        healthBarSlider = GetComponentInChildren<Slider>();
        healthBarCanvas = GetComponentInChildren<Canvas>();

        if (healthBarCanvas != null)
            healthBarCanvas.enabled = false;

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }

        // Store the initial X scale for consistent flipping
        baseScaleX = Mathf.Abs(transform.localScale.x);

        // Devriye başlangıç konumu
        patrolStartPosition = transform.position;

        // Initialize patrol target
        SetNewPatrolTarget();

        Debug.Log("BasicMechanorot initialized. Player found: " + (player != null));
    }

    // ---------------------------------------------------------
    //  2) Update
    // ---------------------------------------------------------
    void Update()
    {
        // Parent (EnemyBase) içindeki currentHealth kontrolü
        if (player == null || currentHealth <= 0) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log($"BasicMechanorot: Distance to player is {distanceToPlayer}");

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
            Debug.Log("BasicMechanorot: Player within attack range. Starting attack.");
            StartCoroutine(AttackPlayer());
        }
        else if (isPatrolling && !isAttacking)
        {
            Patrol();
        }
        else
        {
            Idle();
        }
    }

    // ---------------------------------------------------------
    //  3) EnemyBase'den gelen TakeDamage'i Override Et
    // ---------------------------------------------------------
    public override void TakeDamage(int damageAmount)
    {
        // Bu satır, EnemyBase.currentHealth -= damageAmount işlemini yapar
        base.TakeDamage(damageAmount);
        Debug.Log($"BasicMechanorot: Took {damageAmount} damage. Current health: {currentHealth}");

        // Sağlık barı güncelle
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth; // EnemyBase'in currentHealth'i
            Debug.Log("BasicMechanorot: Health bar updated.");
        }

        ShowHealthBar();
    }

    // ---------------------------------------------------------
    //  4) Die'ı Override Et (Kendi animasyon vb. istersek)
    // ---------------------------------------------------------
    protected override void Die()
    {
        animator.SetBool(IsDead, true);
        rb.velocity = Vector2.zero;
        isPatrolling = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        Debug.Log("BasicMechanorot: Died.");

        // Base'de Destroy işlemi yapıyorsanız ve 
        // anında script devre dışı olsun istiyorsanız:
        enabled = false;

        // Dilerseniz base.Die() çağırıp 5 sn sonra yok edebilir
        // ya da buraya Destroy(gameObject, 1f) vs. ekleyebilirsiniz.
        base.Die();
    }

    // ---------------------------------------------------------
    //  Hareket, Patrol, Saldırı vs...
    // ---------------------------------------------------------
    void MoveTowardsPlayer()
    {
        animator.SetBool(IsWalking, true);

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed * runSpeedMultiplier, rb.velocity.y);

        // Sprite flip
        if (direction.x > 0)
            transform.localScale = new Vector3(-0.14f, transform.localScale.y, transform.localScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(0.14f, transform.localScale.y, transform.localScale.z);

        Debug.Log("BasicMechanorot: Moving towards player.");
    }

    void Patrol()
    {
        if (isWaiting) 
        {
            Debug.Log("BasicMechanorot: Currently waiting at patrol point.");
            return;
        }

        animator.SetBool(IsWalking, true);

        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            StartCoroutine(WaitAndSetNewPatrolTarget());
        }

        if (direction.x > 0)
            transform.localScale = new Vector3(-0.1f, transform.localScale.y, transform.localScale.z);
        else if (direction.x < 0)
            transform.localScale = new Vector3(0.1f, transform.localScale.y, transform.localScale.z);
        
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
        while (Mathf.Abs(patrolOffset) < minPatrolDistance);

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
        

        int randomAttackAnimation = Random.Range(0, 2);
        if (randomAttackAnimation == 0)
            animator.SetTrigger(Damage1);
        else
            animator.SetTrigger(Damage2);
        

        yield return new WaitForSeconds(0.5f);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("BasicMechanorot: PlayerController component not found on Player.");
            }
        }
        else
        {
            Debug.Log("BasicMechanorot: Player moved out of attack range.");
        }

        yield return new WaitForSeconds(attackCooldown - 0.5f);
        isAttacking = false;
    }

    private void ShowHealthBar()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.enabled = true;

            if (hideHealthBarCoroutine != null)
                StopCoroutine(hideHealthBarCoroutine);

            hideHealthBarCoroutine = StartCoroutine(HideHealthBarAfterDelay());
        }
    }

    private IEnumerator HideHealthBarAfterDelay()
    {
        yield return new WaitForSeconds(healthBarDisplayDuration);
        if (healthBarCanvas != null)
        {
            healthBarCanvas.enabled = false;
        }
    }

    // TriggerEnter, vb...
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            { 
                playerController.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("BasicMechanorot: PlayerController component not found on Player.");
            }
        }
    }
    
    private bool IsGrounded()
    {
        Vector2 originLeft = groundCheck.position + Vector3.left * 0.2f;
        Vector2 originRight = groundCheck.position + Vector3.right * 0.2f;

        bool groundedLeft = Physics2D.Raycast(originLeft, Vector2.down, groundCheckDistance, groundLayer);
        bool groundedRight = Physics2D.Raycast(originRight, Vector2.down, groundCheckDistance, groundLayer);

        Debug.DrawRay(originLeft, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(originRight, Vector2.down * groundCheckDistance, Color.red);

        return groundedLeft || groundedRight;
    }
}
