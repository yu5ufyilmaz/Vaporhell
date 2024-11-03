using UnityEngine;
using System.Collections;

public class BasicMechanorot : EnemyBase
{
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float shootRange = 15f; // Ateş etme mesafesi
    public int damage = 50;
    public GameObject enemyBulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float shootCooldown = 2f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isAttacking = false;
    private bool isShooting = false;
    private float shootTimer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        shootTimer = shootCooldown;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Atış mesafesine ulaşıldığında hareketi durdurup ateş etmeye başla
        if (distanceToPlayer <= shootRange && distanceToPlayer > attackRange && !isAttacking)
        {
            StopAndShoot();
        }
        else if (distanceToPlayer <= detectionRange && distanceToPlayer > shootRange && !isAttacking)
        {
            MoveTowardsPlayer();
        }
        else if (distanceToPlayer <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }
        else
        {
            // Düşman hareket etmiyorsa animasyonu durdur
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

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

    void StopAndShoot()
    {
        rb.velocity = Vector2.zero;
        isShooting = true;

        // Animator varsa hareket animasyonunu durdur
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }

        // Ateş etme işlemi
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            Shoot();
            shootTimer = shootCooldown;
        }
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        Debug.Log("Player'a saldırıldı!");

        yield return new WaitForSeconds(1f);

        isAttacking = false;
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

    public void IncreaseSpeed(float multiplier)
    {
        moveSpeed *= multiplier;
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
}
