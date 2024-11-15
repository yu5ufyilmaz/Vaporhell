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
    public float deathGravityScale = 0.0f;

    public int maxJumps = 2; // Maksimum zıplama sayısı (çift zıplama için 2)
    private int remainingJumps;

    private bool _isGrounded;
    private bool _isDead = false;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private PlayerInput _playerInput;
    private InputAction _shootAction;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;

    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    public Vector3 offsetRight = new Vector3(2f, 0, 0);
    public Vector3 offsetLeft = new Vector3(-2f, 0, 0);
    private Vector3 targetOffset;
    public float transitionDuration = 0.5f;
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

        remainingJumps = maxJumps; // Başlangıçta maksimum zıplama hakkı
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

        if (moveInput.x > 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (moveInput.x < 0)
        {
            _spriteRenderer.flipX = false;
        }
        

        Vector3 newTargetOffset = isMoving ? Vector3.zero : (_spriteRenderer.flipX ? offsetRight : offsetLeft);

        if (newTargetOffset != targetOffset)
        {
            targetOffset = newTargetOffset;

            if (offsetTransitionCoroutine != null)
            {
                StopCoroutine(offsetTransitionCoroutine);
            }

            offsetTransitionCoroutine = StartCoroutine(SmoothTransitionToOffset(targetOffset, transitionDuration));
        }

        _animator.SetBool("isMoving", isMoving);

        if (_jumpAction.triggered && remainingJumps > 0)
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
        _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
        remainingJumps--;
        _animator.SetBool("isJumping", true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
            remainingJumps = maxJumps; // Yere temas ettiğinde zıplama hakkını sıfırla
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
        if (_isDead) return;

        _isDead = true;
        _animator.SetTrigger("DieTrigger");

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = deathGravityScale;
        _rb.constraints = RigidbodyConstraints2D.FreezePositionY;

        _playerInput.enabled = false;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(5f);

        Destroy(gameObject);
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
