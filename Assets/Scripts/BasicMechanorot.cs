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
    public float patrolSpeed = 2f;

    // MechaBomb ile BİREBİR AYNI ground check alanları
    [Header("Ground Check Parameters")]
    public Transform groundCheck;
    public float groundCheckDistance = 2f;
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

    // MechaBombBehavior'daki "movingRight" gibi
    private bool movingRight = true;

    private bool isAttacking = false;

    // ---------------------------------------------------------
    //  1) Start'ta maxHealth'i özelleştirip base.Start() çağır
    // ---------------------------------------------------------
    protected override void Start()
    {
        maxHealth = 120;
        base.Start(); // (EnemyBase) currentHealth = 120

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        healthBarSlider = GetComponentInChildren<Slider>();
        healthBarCanvas = GetComponentInChildren<Canvas>();

        if (healthBarCanvas != null)
            healthBarCanvas.enabled = false;

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
    }

    // ---------------------------------------------------------
    //  2) Update - Yakınsa saldır, değilse devriye
    // ---------------------------------------------------------
    void Update()
    {
        if (player == null || currentHealth <= 0) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange && !isAttacking)
        {
            // Oyuncuya doğru hareket / kovalama
            MoveTowardsPlayer();
        }
        else if (distanceToPlayer <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }
        else
        {
            // Menzil dışında kaldığında devriye
            PatrolPlatform();
        }
    }

    // ---------------------------------------------------------
    //  3) MechaBombBehavior'dan BİREBİR ALINAN Metotlar
    // ---------------------------------------------------------

    private void PatrolPlatform()
    {
        animator.SetBool(IsWalking, true);

        // Önünde zemin bitmişse veya engelse yön değiştir
        if (!IsGrounded())
        {
            Flip();
        }

        // Sağ veya sola doğru sabit hızla ilerle
        transform.Translate((movingRight ? Vector2.right : Vector2.left) * patrolSpeed * Time.deltaTime);
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

    private void Flip()
    {
        movingRight = !movingRight;
        
        // Sprite'ı flipX ile döndürüyorsanız:
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.flipX = !sr.flipX;
        
        // Eğer firePoint vb. var ise burada da localPosition.x’i ters çevirebilirsiniz.
    }

    // ---------------------------------------------------------
    //  4) Saldırı, Hasar Alma, Ölüm vb. (Var olanlar korunabilir)
    // ---------------------------------------------------------
    private void MoveTowardsPlayer()
    {
        animator.SetBool(IsWalking, true);
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

        // Hangi yöne bakacak?
        if (direction.x > 0 && !movingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && movingRight)
        {
            Flip();
        }
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        // Basit saldırı animasyonu
        int randomAttackAnimation = Random.Range(0, 2);
        if (randomAttackAnimation == 0)
            animator.SetTrigger(Damage1);
        else
            animator.SetTrigger(Damage2);

        yield return new WaitForSeconds(0.5f);

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // Oyuncuya hasar ver
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(attackCooldown - 0.5f);
        isAttacking = false;
    }

    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);

        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

        ShowHealthBar();
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
            healthBarCanvas.enabled = false;
    }

    protected override void Die()
    {
        animator.SetBool(IsDead, true);
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        base.Die();
        // Yok etme vs...
    }
}
