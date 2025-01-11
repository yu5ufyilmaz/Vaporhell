using UnityEngine;
using System.Collections;

public class SpiderBehavior : EnemyBase
{
    private static readonly int isWalking = Animator.StringToHash("isWalking");
    private static readonly int IsExploding = Animator.StringToHash("isExploding");

    [Header("Spider Parameters")]
    public float detectionRange = 20f;       // Oyuncuyu algılama mesafesi
    public float patrolSpeed = 2f;           // Devriye hızı
    public float chaseSpeed = 4f;            // Kovalamaca hızı
    public float explosionDelay = 1f;        // Patlama gecikmesi
    public float waitTimeAtEdge = 1f;        // Kenarda bekleme süresi
    public Transform groundCheck;            // Zemin kontrolü için referans
    public float groundCheckDistance = 2f;   // Zemin kontrol mesafesi
    public LayerMask groundLayer;            // Zemin katmanı

    [Header("Wall Detection Parameters")]
    public float wallCheckDistance = 1f;     // Duvar algılama mesafesi
    public Transform wallCheck;              // Duvar kontrolü için referans
    public LayerMask wallLayer;              // Duvar katmanı

    private GameObject player;               // Oyuncu referansı
    private bool movingRight = true;         // Hareket yönü
    private Animator animator;               // Animatör
    private SpriteRenderer spriteRenderer;   // Sprite yönü kontrolü
    private bool isExploding = false;        // Patlama durumu kontrolü
    public SpiderDamageTrigger damageTrigger; // Hasar tetikleyici

    protected override void Start()
    {
        maxHealth = 50; // Spider için özel health
        base.Start(); // EnemyBase’in başlangıç fonksiyonu

        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageTrigger = GetComponentInChildren<SpiderDamageTrigger>();

        if (damageTrigger == null)
        {
            Debug.LogError("SpiderBehavior: DamageTrigger not found in children!");
        }

        if (wallCheck == null)
        {
            Debug.LogError("SpiderBehavior: WallCheck transform is not assigned!");
        }
    }

    void Update()
    {
        hasFlippedThisFrame = false; // Her frame başında resetle

        if (isExploding) return;

        if (PlayerInRange())
        {
            patrolSpeed = Mathf.Lerp(patrolSpeed, chaseSpeed, Time.deltaTime * 4f); // Kademeli hız artışı
            ChasePlayer();
        }
        else
        {
            patrolSpeed = Mathf.Lerp(patrolSpeed, 2f, Time.deltaTime * 4f); // Kademeli hız azalışı
            PatrolPlatform();
        }
    }


    private bool PlayerInRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.transform.position) <= detectionRange;
    }

    private void ChasePlayer()
    {
        animator.SetBool(isWalking, true);

        // Oyuncuya dön
        if ((player.transform.position.x > transform.position.x && !spriteRenderer.flipX) ||
            (player.transform.position.x < transform.position.x && spriteRenderer.flipX))
        {
            Flip();
        }

        // Oyuncuya doğru koş
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, chaseSpeed * Time.deltaTime);

        // Oyuncunun önünde mi kontrol et
        Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, 1.5f);

        Debug.DrawRay(transform.position, directionToPlayer * 1.5f, Color.green);

        if (hit.collider != null && hit.collider.CompareTag("Player") && IsGrounded())
        {
            Debug.Log("Spider detected the player directly ahead. Triggering explosion...");
            StartCoroutine(Explode());
        }
    }

    private bool hasFlippedThisFrame = false;

    private void PatrolPlatform()
    {
        if (isExploding) return;

        animator.SetBool(isWalking, true);

        bool grounded = IsGrounded();
        bool wallAhead = IsWallAhead();

        if (!grounded || wallAhead)
        {
            Flip();
        }

        // Hareket et
        transform.Translate((movingRight ? Vector2.right : Vector2.left) * patrolSpeed * Time.deltaTime);
    }


    private bool IsWallAhead()
    {
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, wallLayer);

        Debug.DrawRay(wallCheck.position, direction * wallCheckDistance, Color.blue);
        Debug.Log($"IsWallAhead called. Hit: {hit.collider != null}");

        return hit.collider != null;
    }


    private IEnumerator Explode()
    {
        isExploding = true;
        animator.SetTrigger(IsExploding);

        yield return new WaitForSeconds(explosionDelay);

        // Patlama hasarını uygula
        if (damageTrigger != null)
        {
            damageTrigger.ExplodeDamage(); // Hasar verme fonksiyonunu çağır
        }

        Debug.Log("Spider exploded!");
        Die(); // Kendini yok et
    }

    private bool IsGrounded()
    {
        Vector2 originLeft = groundCheck.position + Vector3.left * 0.2f;
        Vector2 originRight = groundCheck.position + Vector3.right * 0.2f;

        bool groundedLeft = Physics2D.Raycast(originLeft, Vector2.down, groundCheckDistance, groundLayer);
        bool groundedRight = Physics2D.Raycast(originRight, Vector2.down, groundCheckDistance, groundLayer);

        Debug.DrawRay(originLeft, Vector2.down * groundCheckDistance, Color.red);
        Debug.DrawRay(originRight, Vector2.down * groundCheckDistance, Color.red);
        Debug.Log($"IsGrounded called. groundedLeft: {groundedLeft}, groundedRight: {groundedRight}");

        return groundedLeft || groundedRight;
    }


    private void Flip()
    {
        movingRight = !movingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;
        Debug.Log($"Flip called. Now movingRight: {movingRight}");
    }


    protected override void Die()
    {
        base.Die();
        Debug.Log("Spider is dead.");
    }
}
