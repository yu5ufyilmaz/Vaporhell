using UnityEngine;

public class MechaBombBehavior : MonoBehaviour
{
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
    public float groundCheckDistance = 1f; // Zemin kontrol mesafesi
    public LayerMask groundLayer; // Zemin katmanı

    private GameObject player; // Oyuncu referansı
    private float lastShootTime; // Son ateş zamanı
    private bool movingRight = true; // Yön kontrolü
    private Animator animator; // Animasyon kontrolcüsü
    private SpriteRenderer spriteRenderer; // Sprite kontrolcüsü

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

    // Oyuncu menzil içinde mi?
    bool PlayerInRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.transform.position) <= detectionRange;
    }

    // Oyuncuya bomba at
    void HandleShooting()
    {
        animator.SetBool("isWalking", false);

        if (Time.time > lastShootTime + shootCooldown)
        {
            animator.SetTrigger("shoot");
            ThrowGrenade();
            lastShootTime = Time.time;
        }
    }

    // Devriye hareketi
    void PatrolPlatform()
    {
        animator.SetBool("isWalking", true);

        if (!IsGrounded())
        {
            Flip();
        }

        transform.Translate((movingRight ? Vector2.right : Vector2.left) * patrolSpeed * Time.deltaTime);
    }

    // Bomba atışı
    void ThrowGrenade()
    {
        if (player == null) return;

        // Bombayı oluştur
        GameObject grenade = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);
        StartCoroutine(MoveGrenadeInArc(grenade, player.transform.position));
    }

    // Bombayı yay çizerek hedefe taşı
    System.Collections.IEnumerator MoveGrenadeInArc(GameObject grenade, Vector2 targetPosition)
    {
        Vector2 startPosition = grenade.transform.position;
        float elapsedTime = 0f;
        float duration = Vector2.Distance(startPosition, targetPosition) / grenadeSpeed; // Hareket süresi

        while (elapsedTime < duration)
        {
            if (grenade == null) yield break; // Eğer bomba yok edilmişse Coroutine'i sonlandır

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // X ekseni: Doğrusal hareket
            float x = Mathf.Lerp(startPosition.x, targetPosition.x, progress);

            // Y ekseni: Yay yüksekliğini sinus eğrisi ile ayarla
            float y = Mathf.Lerp(startPosition.y, targetPosition.y, progress) + arcHeight * Mathf.Sin(progress * Mathf.PI);

            // Bombanın konumunu güncelle
            if (grenade != null) // Tekrar kontrol
            {
                grenade.transform.position = new Vector2(x, y);
            }

            yield return null;
        }

        if (grenade != null) // Tekrar kontrol
        {
            grenade.transform.position = targetPosition;
            Explode(grenade);
        }
    }

    void Explode(GameObject grenade)
    {
        // Buraya patlama efekti ve hasar verme sistemi ekleyebilirsiniz
        Destroy(grenade);
    }


    // Yönü değiştir ve firePoint'i çevir
    void Flip()
    {
        movingRight = !movingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;

        Vector3 firePointPosition = firePoint.localPosition;
        firePointPosition.x *= -1;
        firePoint.localPosition = firePointPosition;
    }

    // Zemin kontrolü
    bool IsGrounded()
    {
        Vector2 origin = groundCheck.position;
        Vector2 direction = Vector2.down;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, groundCheckDistance, groundLayer);

        Debug.DrawRay(origin, direction * groundCheckDistance, Color.red);
        return hit.collider != null;
    }
}
