using UnityEngine;
using System.Collections;

public class MechaBombBehavior : EnemyBase
{
    private static readonly int isWalking = Animator.StringToHash("isWalking");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    [Header("Grenade Parameters")]
    public GameObject grenadePrefab; // Patlayıcı prefabı
    public Transform firePoint; // Bombanın çıkış noktası
    public float shootCooldown = 2f; // Atış bekleme süresi
    public float detectionRange = 8f; // Oyuncu algılama menzili
    public float grenadeSpeed = 5f; // Bombanın hareket hızı
    public float arcHeight = 2f; // Bombanın yay yüksekliği

    [Header("Patrol Parameters")]
    public float patrolSpeed = 2f; // Devriye hızı
    public Transform groundCheck; // Zemin kontrol noktası
    public float groundCheckDistance = 2f; // Zemin kontrol mesafesi
    public LayerMask groundLayer; // Zemin katmanı

    private GameObject player; // Oyuncu referansı
    private float lastShootTime; // Son ateş zamanı
    private bool movingRight = true; // Yön kontrolü
    private Animator animator; // Animasyon kontrolcüsü
    private SpriteRenderer spriteRenderer; // Sprite kontrolcüsü

    void Start()
    {
        base.health = 70; // EnemyBase'ten gelen sağlık değeri
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log("MechaBomb initialized. Player found: " + (player != null));
    }

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

    void HandleShooting()
    {
        animator.SetBool(isWalking, false);

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

    private void PatrolPlatform()
    {
        animator.SetBool(isWalking, true);

        if (!IsGrounded())
        {
            Flip();
        }

        transform.Translate((movingRight ? Vector2.right : Vector2.left) * patrolSpeed * Time.deltaTime);
    }

    void ThrowGrenade()
    {
        if (player == null) return;

        // Bombanın hedef pozisyonunu belirle (oyuncunun altındaki zemini bul)
        Vector2 targetPosition = GetGroundPositionUnderPlayer();

        // Bombayı oluştur
        GameObject grenade = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);

        // Bombayı hedef pozisyonuna hareket ettir
        StartCoroutine(MoveGrenadeInArc(grenade, targetPosition));

        Debug.Log("Grenade thrown towards: " + targetPosition);
    }



    private Vector2 GetGroundPositionUnderPlayer()
    {
        Vector2 raycastOrigin = new Vector2(player.transform.position.x, player.transform.position.y - 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, 10f, groundLayer);

        if (hit.collider != null)
        {
            return hit.point;
        }

        return player.transform.position;
    }

    System.Collections.IEnumerator MoveGrenadeInArc(GameObject grenade, Vector2 targetPosition)
    {
        Vector2 startPosition = grenade.transform.position;
        float elapsedTime = 0f;
        float duration = Vector2.Distance(startPosition, targetPosition) / grenadeSpeed;

        while (elapsedTime < duration)
        {
            if (grenade == null) yield break;

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // X ekseni: Doğrusal hareket
            float x = Mathf.Lerp(startPosition.x, targetPosition.x, progress);

            // Y ekseni: Yay yüksekliği
            float y = Mathf.Lerp(startPosition.y, targetPosition.y, progress) + arcHeight * Mathf.Sin(progress * Mathf.PI);

            grenade.transform.position = new Vector2(x, y);
            yield return null;
        }

        if (grenade != null)
        {
            grenade.transform.position = targetPosition;

            // Grenade scriptine inişi bildir
            Grenade grenadeScript = grenade.GetComponent<Grenade>();
            if (grenadeScript != null)
            {
                grenadeScript.OnLand();
            }
        }
    }
    public override void TakeDamage(int damageAmount)
    {
        base.TakeDamage(damageAmount); // EnemyBase sınıfının TakeDamage fonksiyonunu çağır
        if (health <= 0)
        {
            Die();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;

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

    protected override void Die()
    {
        base.Die(); // EnemyBase sınıfındaki Die metodunu çağır
        Debug.Log("MechaBomb is dead.");
        Destroy(gameObject, 0.1f); // 1 saniye sonra yok et
    }
}
