using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 100;
    private int _currentHealth;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float moveSpeed = 5f;
    public float runSpeedMultiplier = 1.5f;
    public float jumpForce = 10f;
    public float deathGravityScale = 0.0f; // Ölüm sırasında yerinde kalması için düşük gravity

    private bool _isGrounded;
    private bool _isDead = false; // Ölüm durumu kontrolü
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private PlayerInput _playerInput;
    private InputAction _shootAction;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _shootAction = _playerInput.actions["Shoot"];
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _runAction = _playerInput.actions["Run"];
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentHealth = maxHealth;
    }

    void Update()
    {
        if (_isDead) return; // Eğer ölü ise hareketi durdur

        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        float currentSpeed = moveSpeed;

        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        if (_runAction.IsPressed())
        {
            currentSpeed *= runSpeedMultiplier;
        }

        _rb.velocity = new Vector2(moveInput.x * currentSpeed, _rb.velocity.y);

        if (moveInput.x > 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (moveInput.x < 0)
        {
            _spriteRenderer.flipX = false;
        }

        _animator.SetBool("isMoving", isMoving);

        if (_shootAction.triggered)
        {
            Shoot();
        }

        if (_jumpAction.triggered && _isGrounded)
        {
            Jump(isMoving);
        }
    }

    private void Shoot()
    {
        if (_isDead) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        Vector2 shootDirection = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
        bulletRb.velocity = shootDirection * bulletSpeed;
    }

    private void Jump(bool isMoving)
    {
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        _isGrounded = false;

        _animator.SetBool("isJumping", true);
        _animator.SetBool("isMoving", isMoving);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
            _animator.SetBool("isJumping", false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        _currentHealth -= damageAmount;

        if (_currentHealth <= 0 && !_isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead) return; // Eğer zaten ölü ise tekrar işlem yapma

        _isDead = true;
        _animator.SetTrigger("DieTrigger"); // Ölüm animasyonunu tetikle

        _rb.velocity = Vector2.zero; // Hareketi durdur
        _rb.gravityScale = deathGravityScale; // Yavaş düşüş için gravity ayarı
        _rb.constraints = RigidbodyConstraints2D.FreezePositionY; // Y ekseninde hareketi dondur

        _playerInput.enabled = false; // Oyuncu kontrolünü devre dışı bırak

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(5f); // Ölüm animasyonu süresince bekle

        Destroy(gameObject); // Karakteri yok et
    }

    public void Heal(int healAmount)
    {
        if (_isDead) return;

        _currentHealth += healAmount;
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
    }
}
