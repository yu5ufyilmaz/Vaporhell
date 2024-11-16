using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    // Animation Parameters
    [Header("Animation Parameters")]
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int DieTrigger = Animator.StringToHash("DieTrigger");

    // Health Parameters
    [Header("Health Parameters")]
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;

    // Combat Parameters
    [Header("Combat Parameters")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;

    // Movement Parameters
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;
    [SerializeField] private float fastJumpForce = 8f; // Daha güçlü zıplama
    [SerializeField] private float normalGravityScale = 1f;
    [SerializeField] private float fallingGravityScale = 5f; // Daha hızlı düşüş
    [SerializeField] private int maxJumps = 2;
    private int remainingJumps;
    private bool _isGrounded;
    private bool _isDead = false;

    // Cinemachine Offset Parameters
    [Header("Cinemachine Offset Parameters")]
    [SerializeField] private Vector3 offsetRight = new Vector3(2f, 0, 0);
    [SerializeField] private Vector3 offsetLeft = new Vector3(-2f, 0, 0);
    [SerializeField] private float transitionDuration = 0.5f;
    private Vector3 targetOffset;
    private Coroutine offsetTransitionCoroutine;

    // Components
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private PlayerInput _playerInput;
    private CinemachineVirtualCamera _cinemachineVirtualCamera;

    // Input Actions
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
        _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        remainingJumps = maxJumps;
        _rb.gravityScale = normalGravityScale; // Yerçekimi varsayılan ayar
    }

    private void Update()
    {
        if (_isDead) return;

        HandleMovement();
        HandleJump();
        HandleShoot();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        float currentSpeed = _runAction.IsPressed() ? moveSpeed * runSpeedMultiplier : moveSpeed;

        _rb.velocity = new Vector2(moveInput.x * currentSpeed, _rb.velocity.y);

        if (moveInput.x != 0)
        {
            _spriteRenderer.flipX = moveInput.x > 0;
        }

        UpdateCinemachineOffset(isMoving);
        _animator.SetBool(IsMoving, isMoving);
    }

    private void HandleJump()
    {
        if (_jumpAction.triggered && remainingJumps > 0)
        {
            // Hızlı zıplama kuvveti uygula
            _rb.velocity = new Vector2(_rb.velocity.x, fastJumpForce);
            remainingJumps--;

            // Zıplama sırasında yerçekimini artır
            _rb.gravityScale = fallingGravityScale;

            if (remainingJumps == maxJumps - 1)
            {
                // İlk zıplamada isJumping true yap
                _animator.SetBool(IsJumping, true);
            }
            else if (remainingJumps < maxJumps - 1)
            {
                // İkinci zıplamada animasyonu sıfırla ve tekrar tetikle
                _animator.SetBool(IsJumping, false);
                _animator.SetBool(IsJumping, true);
            }
        }
    }


    private void HandleShoot()
    {
        if (_shootAction.triggered && !_isDead)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            Vector2 shootDirection = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
            bulletRb.velocity = shootDirection * bulletSpeed;
        }
    }

    private void UpdateCinemachineOffset(bool isMoving)
    {
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
            remainingJumps = maxJumps;

            // Yerçekimini varsayılan hale getir
            _rb.gravityScale = normalGravityScale;

            _animator.SetBool(IsJumping, false);
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
        _animator.SetTrigger(DieTrigger);

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0.0f;
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
