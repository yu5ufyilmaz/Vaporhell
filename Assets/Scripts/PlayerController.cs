using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 100;
    private int _currentHealth;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float moveSpeed = 10f;
    public float runSpeedMultiplier = 1.5f;
    public float jumpForce = 3f;
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

    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    public Vector3 offsetRight = new Vector3(2f, 0, 0); // Sağa bakarken ofset
    public Vector3 offsetLeft = new Vector3(-2f, 0, 0); // Sola bakarken ofset
    private Vector3 targetOffset; // Kameranın ulaşmaya çalışacağı ofset
    public float transitionDuration = 0.5f; // Geçiş süresi
    private Coroutine offsetTransitionCoroutine;

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
        _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        float currentSpeed = moveSpeed;

        if (_runAction.IsPressed())
        {
            currentSpeed *= runSpeedMultiplier;
        }

        _rb.velocity = new Vector2(moveInput.x * currentSpeed, _rb.velocity.y);

        // Hareket yönüne göre karakterin bakış yönünü ayarla
        if (moveInput.x > 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (moveInput.x < 0)
        {
            _spriteRenderer.flipX = false;
        }

        // Yeni hedef ofseti belirle
        Vector3 newTargetOffset = isMoving ? Vector3.zero : (_spriteRenderer.flipX ? offsetRight : offsetLeft);

        // Eğer hedef ofset değiştiyse geçişi başlat
        if (newTargetOffset != targetOffset)
        {
            targetOffset = newTargetOffset;

            // Önceki coroutine'i durdur (varsa)
            if (offsetTransitionCoroutine != null)
            {
                StopCoroutine(offsetTransitionCoroutine);
            }

            // Yeni geçiş coroutine'ini başlat
            offsetTransitionCoroutine = StartCoroutine(SmoothTransitionToOffset(targetOffset, transitionDuration));
        }

        _animator.SetBool("isMoving", isMoving);

        if (_jumpAction.triggered && _isGrounded)
        {
            Jump();
        }

        if (_shootAction.triggered)
        {
            Shoot();
        }
    }

    private IEnumerator SmoothTransitionToOffset(Vector3 targetOffset, float duration)
    {
        var framingTransposer = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        Vector3 initialOffset = framingTransposer.m_TrackedObjectOffset;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(initialOffset, targetOffset, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Geçiş tamamlandıktan sonra tam hedef pozisyona ayarlayın
        framingTransposer.m_TrackedObjectOffset = targetOffset;
    }

    private void Shoot()
    {
        if (_isDead) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        Vector2 shootDirection = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
        bulletRb.velocity = shootDirection * bulletSpeed;
    }

    private void Jump()
    {
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        _isGrounded = false;
        _animator.SetBool("isJumping", true); // Zıplama animasyonunu tetikle
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
            _animator.SetBool("isJumping", false); // Yere indiğinde zıplama animasyonunu kapat
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
