using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    private PlayerInput _playerInput;
    private InputAction _shootAction;
    private InputAction _moveAction;
    private bool facingRight = true; // Oyuncunun başlangıç yönü sağ

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _shootAction = _playerInput.actions["Shoot"];
        _moveAction = _playerInput.actions["Move"];
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();

        // Hareket input'una göre oyuncunun yüz yönünü güncelle
        if (moveInput.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && facingRight)
        {
            Flip();
        }

        // Ateş etme aksiyonunu kontrol et
        if (_shootAction.triggered)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Mermiyi firePoint pozisyonunda oluştur
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        // Oyuncunun yüzüne dönük yönüne göre merminin hızını ayarla
        Vector2 shootDirection = facingRight ? Vector2.right : Vector2.left;
        bulletRb.velocity = shootDirection * bulletSpeed;

        Debug.Log("Ateş edildi!");
    }

    private void Flip()
    {
        // Oyuncunun yüzünü tersine çevir
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Player hasar aldı: " + damageAmount);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player öldü!");
        Destroy(gameObject);
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        Debug.Log("Player iyileşti: " + healAmount);
    }
}
