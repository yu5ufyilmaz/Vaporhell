using UnityEngine;
using System.Collections;

public class BasicMechanorot : EnemyBase
{
    public float moveSpeed = 3f;
    public float patrolRange = 5f; // Devriye alanı genişliği
    public float detectionRange = 20f; // Karakteri görebileceği mesafe
    public float shootRange = 15f; // Ateş etme mesafesi
    public float chaseSpeed = 5f; // Alarm durumunda koşma hızı
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float shootCooldown = 2f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isAlerted = false; // Düşmanın alarm durumunda olup olmadığını kontrol eder
    private bool isShooting = false;
    private float shootTimer;
    private Vector2 initialPosition; // Devriye başlangıç noktası
    private Vector2 patrolTarget;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        shootTimer = shootCooldown;
        initialPosition = transform.position;
        SetNewPatrolTarget();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isAlerted)
        {
            ChasePlayer();
        }
        else if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            StopAndShoot();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        // Devriye hareketi için hedefe doğru yumuşak geçiş
        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        // Hedefe ulaşıldığında yeni devriye hedefi belirle
        if (Vector2.Distance(transform.position, patrolTarget) < 0.2f)
        {
            SetNewPatrolTarget();
        }

        // Yüz yönünü devriye yönüne göre ayarla
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }

    void SetNewPatrolTarget()
    {
        // Rastgele bir devriye hedefi belirle
        float patrolX = initialPosition.x + Random.Range(-patrolRange, patrolRange);
        float patrolY = initialPosition.y;
        patrolTarget = new Vector2(patrolX, patrolY);
    }

    bool CanSeePlayer()
    {
        // Basit görüş algılaması (Görüş alanında ve engel yoksa oyuncuyu görür)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, detectionRange);
        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    void StopAndShoot()
    {
        rb.velocity = Vector2.zero;
        isShooting = true;

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            Shoot();
            shootTimer = shootCooldown;
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * chaseSpeed;

        // Düşmanın yüz yönünü oyuncuya göre ayarla
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }

    public void Alert()
    {
        isAlerted = true;
        Debug.Log("Düşman alarma geçti!");
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(enemyBulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        Vector2 shootDirection = (player.position - firePoint.position).normalized;
        bulletRb.velocity = shootDirection * bulletSpeed;

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.isEnemyBullet = true;
        }

        Debug.Log("Ateş edildi!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
                Alert(); // Oyuncu mermisi değdiğinde alarm durumuna geç
            }
            Destroy(other.gameObject);
        }
    }
}
