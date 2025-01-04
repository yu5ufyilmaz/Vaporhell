using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MechaBombBehavior : EnemyBase
{
    private static readonly int isWalking = Animator.StringToHash("isWalking");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    [Header("Grenade Parameters")]
    public GameObject grenadePrefab;
    public Transform firePoint;
    public float shootCooldown = 2f;
    public float detectionRange = 8f;
    public float grenadeSpeed = 5f;
    public float arcHeight = 2f;

    [Header("Patrol Parameters")]
    public float patrolSpeed = 2f;
    public Transform groundCheck;
    public float groundCheckDistance = 2f;
    public LayerMask groundLayer;

    [Header("Health Bar Parameters")]
    public GameObject healthBarPrefab;           // Prefab ataması için
    private Slider healthBarSlider;
    private Canvas healthBarCanvas;
    public float healthBarDisplayDuration = 3f;
    private Coroutine hideHealthBarCoroutine;

    private GameObject player;
    private float lastShootTime;
    private bool movingRight = true;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // -------------------------------------------------------
    //  1) Start => MechaBomb için maxHealth, base.Start()
    // -------------------------------------------------------
    protected override void Start()
    {
        // Bu düşman türüne özel Max Health
        maxHealth = 70; 
        base.Start(); // EnemyBase.Start() => currentHealth = maxHealth (70)

        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Health Bar ayarları
        healthBarSlider = GetComponentInChildren<Slider>();
        healthBarCanvas = GetComponentInChildren<Canvas>();

        if (healthBarCanvas != null)
            healthBarCanvas.enabled = false;

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }


        if (healthBarCanvas != null)
            healthBarCanvas.enabled = false;

        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;     // 70
            healthBarSlider.value = currentHealth;    // 70
        }

        Debug.Log("MechaBomb initialized. Player found: " + (player != null));
    }

    // -------------------------------------------------------
    //  2) Update => Oyuncu menzildeyse ateş, değilse devriye
    // -------------------------------------------------------
    void Update()
    {
        if (PlayerInRange())
        {
            HandleShooting();
        }
        else
        {
            PatrolPlatform();
        }
    }

    private bool PlayerInRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.transform.position) <= detectionRange;
    }

    // -------------------------------------------------------
    //  3) Ateş Etme
    // -------------------------------------------------------
    void HandleShooting()
    {
        animator.SetBool(isWalking, false);

        // Oyuncu hangi tarafta ise, o tarafa bak
        if ((player.transform.position.x > transform.position.x && !spriteRenderer.flipX) ||
            (player.transform.position.x < transform.position.x && spriteRenderer.flipX))
        {
            Flip();
        }

        if (Time.time > lastShootTime + shootCooldown)
        {
            animator.SetTrigger(Shoot);
            lastShootTime = Time.time;
            Debug.Log("Shooting animation triggered at: " + Time.time);
        }
    }

    // -------------------------------------------------------
    //  4) Devriye Hareketi
    // -------------------------------------------------------
    private void PatrolPlatform()
    {
        animator.SetBool(isWalking, true);

        // İleride zemin yoksa veya engel varsa dön
        if (!IsGrounded())
        {
            Flip();
        }

        transform.Translate((movingRight ? Vector2.right : Vector2.left) * patrolSpeed * Time.deltaTime);
    }

    // -------------------------------------------------------
    //  5) Bomba Fırlatma - Animasyon Event
    // -------------------------------------------------------
    void ThrowGrenade()
    {
        if (player == null) return;

        Vector2 targetPosition = GetGroundPositionUnderPlayer();
        GameObject grenade = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);

        // Yay şeklinde hareket coroutine
        StartCoroutine(MoveGrenadeInArc(grenade, targetPosition));
    }

    private Vector2 GetGroundPositionUnderPlayer()
    {
        Vector2 raycastOrigin = new Vector2(player.transform.position.x, player.transform.position.y - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, 10f, groundLayer);

        if (hit.collider != null)
            return hit.point;

        return player.transform.position;
    }

    IEnumerator MoveGrenadeInArc(GameObject grenade, Vector2 targetPosition)
    {
        Vector2 startPosition = grenade.transform.position;
        float elapsedTime = 0f;
        float distance = Vector2.Distance(startPosition, targetPosition);
        float duration = distance / grenadeSpeed;

        while (elapsedTime < duration)
        {
            if (grenade == null) yield break;

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            float x = Mathf.Lerp(startPosition.x, targetPosition.x, progress);
            float y = Mathf.Lerp(startPosition.y, targetPosition.y, progress)
                      + arcHeight * Mathf.Sin(progress * Mathf.PI);

            grenade.transform.position = new Vector2(x, y);
            yield return null;
        }

        if (grenade != null)
        {
            grenade.transform.position = targetPosition;
            Grenade grenadeScript = grenade.GetComponent<Grenade>();
            if (grenadeScript != null)
            {
                grenadeScript.OnLand();
            }
        }
    }

    // -------------------------------------------------------
    //  6) Hasar Alma
    // -------------------------------------------------------
    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount); // EnemyBase -> currentHealth -= damageAmount

        // Health bar güncelle
        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

        ShowHealthBar();
    }

    // -------------------------------------------------------
    //  7) Health Bar Göster/Gizle
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
            healthBarCanvas.enabled = false;
    }

    // -------------------------------------------------------
    //  8) Yönü Çevirme
    // -------------------------------------------------------
    private void Flip()
    {
        movingRight = !movingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;

        // FirePoint da ters tarafa geçsin
        Vector3 firePointPosition = firePoint.localPosition;
        firePointPosition.x = -firePointPosition.x;
        firePoint.localPosition = firePointPosition;
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

    // -------------------------------------------------------
    //  9) Ölüm Mantığı (Animasyon vb.)
    // -------------------------------------------------------
    protected override void Die()
    {
        base.Die();
        Debug.Log("MechaBomb is dead.");
        // 0.1 saniye sonra yok et
        Destroy(gameObject, 0.1f);
    }
}
