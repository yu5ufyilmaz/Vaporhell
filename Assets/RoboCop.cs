using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RoboCop : EnemyBase
{
    // Animation Hashes
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsChasing = Animator.StringToHash("isChasing");
    private static readonly int IsDead = Animator.StringToHash("isDead");

    [Header("Movement Parameters")]
    public float patrolSpeed = 2f;        // Devriye hızı
    public float chaseSpeed = 3.5f;       // Kovalamaca hızı
    public float waitTimeAtEdge = 1f;     // Kenara ulaşıldığında bekleme süresi

    [Header("Detection Parameters")]
    public float detectionRange = 10f;     // Oyuncu algılama menzili
    public int damage = 15;                // Verilecek hasar miktarı
    public float attackCooldown = 1.5f;    // Hasar verme soğuma süresi

    [Header("Ground Check Parameters")]
    public Transform groundCheck;          // Zemin kontrolü için referans
    public float groundCheckDistance = 2f; // Zemin kontrol mesafesi
    public LayerMask groundLayer;          // Zemin layer'ı
    public LayerMask ropeLayer;

    [Header("Health Bar Parameters")]
    public GameObject healthBarPrefab;     // Sağlık çubuğu prefab'ı
    private Slider healthBarSlider;
    private Canvas healthBarCanvas;
    public float healthBarDisplayDuration = 3f; // Sağlık çubuğunun gösterim süresi
    private Coroutine hideHealthBarCoroutine;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private SpriteRenderer spriteRenderer;

    private bool isChasing = false;        // Kovalamaca durumu
    private bool isWaiting = false;        // Bekleme durumu
    private bool facingRight = true;       // Karakterin yönü
    private float lastAttackTime;          // Son hasar verme zamanı
    
    private bool isAttacking = false;   // Saldırı modunu belirler
    private float attackDirection = 1; // Saldırı yönünü belirler (1: sağ, -1: sol)


    // -------------------------------------------------------
    // 1) Başlangıç Ayarları
    // -------------------------------------------------------
    protected override void Start()
    {
        // RoboCop için özel maksimum sağlık
        maxHealth = 100;
        base.Start(); // EnemyBase.Start() => currentHealth = maxHealth (100)

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError("RoboCop: Player not found. Ensure the player has the 'Player' tag.");
        }

        // Sağlık Çubuğu Ayarları
        healthBarSlider = GetComponentInChildren<Slider>();
        healthBarCanvas = GetComponentInChildren<Canvas>();

        if (healthBarCanvas != null)
            healthBarCanvas.enabled = false;

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }

        Debug.Log("RoboCop initialized. Player found: " + (player != null));
    }

    // -------------------------------------------------------
    // 2) Güncelleme Döngüsü
    // -------------------------------------------------------
    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (isAttacking)
        {
            HandleAttackMode();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            StartAttack();
        }
        else
        {
            HandlePatrol();
        }
    }



    // -------------------------------------------------------
    // 3) Kovalamayı Yönetme
    // -------------------------------------------------------
    private void HandleChase()
    {
        if (!isChasing)
        {
            isChasing = true;
            animator.SetBool(IsChasing, true);
            animator.SetBool(IsWalking, false);
            Debug.Log("RoboCop: Player detected. Starting to chase.");
        }
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        // Yön Ayarı: Oyuncuya doğru hareket ederken sprite'ı döndür
        float chaseDirection = player.position.x > transform.position.x ? 1f : -1f;
        FlipSprite(chaseDirection);

        // Zemin kontrolü yaparak platformdan düşmemesini sağla
        if (IsGroundAhead(chaseDirection))
        {
            rb.velocity = new Vector2(chaseDirection * chaseSpeed, rb.velocity.y);
        }
        else
        {
            // Zeminde boşluk varsa, yönü değiştir ve devam et
            Flip();
            Debug.Log("RoboCop: No ground ahead while chasing. Flipping direction.");
        }
    }
    
    public void StartAttack()
    {
        isAttacking = true;

        // Saldırı yönü belirlenir
        attackDirection = facingRight ? 1 : -1;

        FlipSprite(attackDirection); // Karakterin yüzünü doğru yöne çevir
        animator.SetBool(IsChasing, true); // Saldırı animasyonu başlat
    }

    
    private void HandleAttackMode()
    {
        // Zemin kontrolü ve duvar kontrolü
        if (IsGroundAhead(attackDirection) && !IsWallAhead(attackDirection))
        {
            // Platform boyunca koş
            rb.velocity = new Vector2(attackDirection * chaseSpeed, rb.velocity.y);
        }
        else
        {
            // Platformun sonuna gelindiğinde veya duvar varsa yön değiştir
            attackDirection *= -1; // Yönü ters çevir
            Flip(); // Sprite yönünü değiştir
            Debug.Log("RoboCop: Wall detected during attack. Flipping direction.");
        }
    }


    
    public void StopAttack()
    {
        isAttacking = false;
        rb.velocity = Vector2.zero; // Hareketi durdur
        animator.SetBool(IsChasing, false); // Saldırı animasyonunu durdur
    }





    // -------------------------------------------------------
    // 4) Devriye Hareketini Yönetme
    // -------------------------------------------------------
    private void HandlePatrol()
    {
        if (isChasing)
        {
            isChasing = false;
            animator.SetBool(IsChasing, false);
            Debug.Log("RoboCop: Player out of range. Resuming patrol.");
        }

        if (!isWaiting)
        {
            PatrolPlatform();
        }
    }

    private void PatrolPlatform()
    {
        animator.SetBool(IsWalking, true);

        // İleride zemin yoksa veya duvar varsa dön
        if (!IsGrounded() || IsWallAhead(facingRight ? 1f : -1f))
        {
            StartCoroutine(TurnAround());
        }
        else
        {
            // Hareket ettir
            transform.Translate((facingRight ? Vector2.right : Vector2.left) * patrolSpeed * Time.deltaTime);
        }
    }

    // -------------------------------------------------------
    // 5) Yönü Çevirme Fonksiyonları
    // -------------------------------------------------------
    void Flip()
    {
        facingRight = !facingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;

        Debug.Log("RoboCop: Flipped direction during attack. Now facing " + (facingRight ? "Right" : "Left"));
    }


    void FlipSprite(float direction)
    {
        if (direction > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction < 0 && facingRight)
        {
            Flip();
        }
    }
    
    // Duvara çarpmayı kontrol eder
    private bool IsWallAhead(float direction)
    {
        Vector2 origin = groundCheck.position + Vector3.right * direction * 0.5f; // İleriye doğru biraz kaydır
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * direction, 0.5f, groundLayer);

        Debug.DrawRay(origin, Vector2.right * direction * 0.5f, Color.green);

        return hit.collider != null;
    }


    // -------------------------------------------------------
    // 6) Hasar Alma Fonksiyonu
    // -------------------------------------------------------
    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount);
        Debug.Log($"RoboCop: Took {damageAmount} damage. Current health: {currentHealth}");

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
            Debug.Log("RoboCop: Health bar updated.");
        }

        ShowHealthBar();
    }

    // -------------------------------------------------------
    // 7) Sağlık Çubuğunu Göster/Gizle
    // -------------------------------------------------------
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

    // -------------------------------------------------------
    // 8) Çarpışma ile Hasar Verme
    // -------------------------------------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController playerController = collision.collider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage); // Oyuncuya hasar ver
                Debug.Log("RoboCop: Player collided and took damage.");
            }
        }
    }


    // -------------------------------------------------------
    // 9) Zemin Kontrol Fonksiyonları
    // -------------------------------------------------------
    // Zemin ve rope kontrolü: Yerde veya rope layer'da olma durumu
    private bool IsGrounded()
    {
        Vector2 originLeft = groundCheck.position + Vector3.left * 0.2f;
        Vector2 originRight = groundCheck.position + Vector3.right * 0.2f;

        bool groundedLeft = Physics2D.Raycast(originLeft, Vector2.down, groundCheckDistance, groundLayer | ropeLayer);
        bool groundedRight = Physics2D.Raycast(originRight, Vector2.down, groundCheckDistance, groundLayer | ropeLayer);

        Debug.DrawRay(originLeft, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(originRight, Vector2.down * groundCheckDistance, Color.red);

        return groundedLeft || groundedRight;
    }

// İleriye doğru zemin veya rope olup olmadığını kontrol eder
    private bool IsGroundAhead(float direction)
    {
        Vector2 origin = groundCheck.position + Vector3.right * direction * 0.5f; // İleriye doğru biraz kaydır
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer | ropeLayer);

        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.blue);

        return hit.collider != null;
    }


    // -------------------------------------------------------
    // 10) Dönerken Bekleme (Turn Around)
    // -------------------------------------------------------
    IEnumerator TurnAround()
    {
        isWaiting = true;
        rb.velocity = Vector2.zero;
        animator.SetBool(IsWalking, false);

        Flip();
        Debug.Log("RoboCop: Turning around.");

        yield return new WaitForSeconds(waitTimeAtEdge);

        isWaiting = false;
    }

    // -------------------------------------------------------
    // 11) Ölüm Mantığı (Animasyon vb.)
    // -------------------------------------------------------
    protected override void Die()
    {
        if (currentHealth <= 0)
        {
            animator.SetBool(IsDead, true);
            rb.velocity = Vector2.zero;
            isChasing = false;
            enabled = false;

            Debug.Log("RoboCop: Died.");

            // Ölüm animasyonunu oynatmayı tamamladıktan sonra yok et
            // Örneğin, animasyonun bittiği bir event ile yok etme yapılabilir
            Destroy(gameObject, 1f); // 1 saniye bekleyerek yok et
        }
    }
}
