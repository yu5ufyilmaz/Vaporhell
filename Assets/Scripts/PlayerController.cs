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
    private static readonly int RollTrigger = Animator.StringToHash("RollTrigger");
    private static readonly int IsCrouching = Animator.StringToHash("isCrouching");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    // Health Parameters
    [Header("Health Parameters")]
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;

    // Combat Parameters
    [Header("Combat Parameters")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float shootCooldown = 0.4f; // Ateş etme cooldown süresi
    private bool canShoot = true; // Ateş etmeye izin durumu
    
    [Header("Fire Point Offsets")]
    [SerializeField] private Vector2 firePointOffsetRight = new Vector2(1f, 0f); // Sağ bakışta merminin çıkış pozisyonu
    [SerializeField] private Vector2 firePointOffsetLeft = new Vector2(-1f, 0f); // Sol bakışta merminin çıkış pozisyonu


    // Movement Parameters
    [Header("Movement Parameters")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;
    [SerializeField] private float fastJumpForce = 8f;
    [SerializeField] private float normalGravityScale = 1f;
    [SerializeField] private float fallingGravityScale = 5f;
    [SerializeField] private int maxJumps = 2;
    private int remainingJumps;
    private bool _isGrounded;
    private bool _isDead = false;
    
    // Crouch Parameters
    [Header("Crouch Parameters")]
    [SerializeField] private Vector2 crouchColliderSize = new Vector2(1f, 0.5f); // Crouch sırasında collider boyutu
    [SerializeField] private Vector2 normalColliderSize = new Vector2(1f, 1f); // Normal collider boyutu

    private bool isCrouching = false;


    // Roll Parameters
    [Header("Roll Parameters")]
    [SerializeField] private float rollSpeed = 15f;
    [SerializeField] private float rollDuration = 0.4f;
    [SerializeField] private float rollCooldown = 1f;
    private bool isRolling = false;
    private bool canRoll = true;

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
    private InputAction _jumpAction;
    private InputAction _rollAction;
    private InputAction _crouchAction; 

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _shootAction = _playerInput.actions["Shoot"];
        _jumpAction = _playerInput.actions["Jump"];
        _rollAction = _playerInput.actions["Roll"]; 
        _crouchAction = _playerInput.actions["Crouch"];
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentHealth = maxHealth;
        _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        remainingJumps = maxJumps;
        _rb.gravityScale = normalGravityScale;
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator bileşeni bulunamadı! Lütfen Player GameObject'inizde Animator olduğundan emin olun.");
        }
    }

    private void Update()
    {
        if (_isDead || isRolling) return;

        if (!isCrouching)
            HandleMovement();

        HandleJump();
        HandleShoot();
        HandleRoll();
        HandleCrouch();
        UpdateFirePointPosition();
    }


    private void HandleMovement()
    {
        if (_isDead || isRolling || !canShoot) return; // Ateş ederken hareketi durdur

        Vector2 moveInput = _playerInput.actions["Move"].ReadValue<Vector2>();
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        float currentSpeed = _playerInput.actions["Run"].IsPressed() ? moveSpeed * runSpeedMultiplier : moveSpeed;

        _rb.velocity = new Vector2(moveInput.x * currentSpeed, _rb.velocity.y);

        if (moveInput.x != 0)
        {
            _spriteRenderer.flipX = moveInput.x > 0;
        }

        _animator.SetBool(IsMoving, isMoving);
    }

    private void HandleJump()
    {
        if (isCrouching || !canShoot) return; // Ateş ederken zıplamayı engelle

        if (_jumpAction.triggered && remainingJumps > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, fastJumpForce);
            remainingJumps--;

            _rb.gravityScale = fallingGravityScale;

            if (remainingJumps == maxJumps - 1)
            {
                _animator.SetBool(IsJumping, true);
            }
            else if (remainingJumps < maxJumps - 1)
            {
                _animator.SetBool(IsJumping, false);
                _animator.SetBool(IsJumping, true);
            }
        }
    }



    private void HandleShoot()
    {
        if (_shootAction.triggered && canShoot && !_isDead)
        {
            StartCoroutine(ShootCoroutine());
        }
    }
    
    private void UpdateFirePointPosition()
    {
        if (_spriteRenderer.flipX)
        {
            firePoint.localPosition = firePointOffsetLeft; // Sol tarafa bakarken fire point pozisyonu
        }
        else
        {
            firePoint.localPosition = firePointOffsetRight; // Sağ tarafa bakarken fire point pozisyonu
        }
    }


    private IEnumerator ShootCoroutine()
    {
        // Ateş etmeye izin verme
        canShoot = false;

        // Hareketi durdur
        _rb.velocity = Vector2.zero;

        // Ateş animasyonunu tetikle
        _animator.SetTrigger(Shoot); // Animator'da "Shoot" trigger'ı tanımladığını varsayıyorum.

        // Ateş etme işlemini gerçekleştirme zamanı
        yield return new WaitForSeconds(0.4f); // Animasyonun ateş etme anına göre ayarla

        // Mermiyi oluştur ve ateş et
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        Vector2 shootDirection = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
        bulletRb.velocity = shootDirection * bulletSpeed;

        // Cooldown süresi boyunca bekle
        yield return new WaitForSeconds(shootCooldown - 0.4f); // Toplam 1 saniyelik cooldown olacak

        // Tekrar ateş etmeye izin ver
        canShoot = true;
    }


    private void HandleRoll()
    {
        if (_rollAction.triggered && canRoll && !isRolling)
        {
            StartCoroutine(PerformRoll());
        }
    }
   
    
    private void HandleCrouch()
    {
        if (_crouchAction.IsPressed() && !isCrouching)
        {
            EnterCrouch();
        }
        else if (!_crouchAction.IsPressed() && isCrouching)
        {
            ExitCrouch();
        }
    }

    private void EnterCrouch()
    {
        isCrouching = true;
        _animator.SetBool(IsCrouching, true);

        // Crouch sırasında hareketi durdur (isteğe bağlı)
        _rb.velocity = Vector2.zero;
    }

    private void ExitCrouch()
    {
        isCrouching = false;
        _animator.SetBool(IsCrouching, false);
    }

    
    private IEnumerator PerformRoll()
    {
        isRolling = true;
        canRoll = false;

        // Roll animasyonu tetikle
        _animator.SetTrigger(RollTrigger);

        // Roll yönünü belirle
        float rollDirection = _spriteRenderer.flipX ? 1f : -1f;

        // Roll sırasında yerçekimini devre dışı bırak
        _rb.gravityScale = 0;

        // Roll sırasında hız uygula
        _rb.velocity = new Vector2(rollDirection * rollSpeed, 0);

        // Roll süresi boyunca bekle
        yield return new WaitForSeconds(rollDuration);

        // Roll tamamlandıktan sonra hareketi sıfırla ve yerçekimini etkinleştir
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = normalGravityScale;

        isRolling = false;

        // Cooldown süresi
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
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
