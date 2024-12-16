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
    private static readonly int IsFalling = Animator.StringToHash("isFalling");

    // Health Parameters
    [Header("Health Parameters")]
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;
    [SerializeField] private HealthBarUI healthBarUI;

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


    [Header("Ledge Climb Parameters")]
    private bool isGrabbed; // Kenara tutunduğunu belirten durum
    [SerializeField] private float redXOffset; // Kenar kontrol kutusu x mesafesi
    [SerializeField] private float redYOffset; // Kenar kontrol kutusu y mesafesi
    [SerializeField] private float redXSize, redYSize; // Kenar kontrol kutusu boyutları
    [SerializeField] private float greenXOffset; // Tutunma kontrol kutusu x mesafesi
    [SerializeField] private float greenYOffset; // Tutunma kontrol kutusu y mesafesi
    [SerializeField] private float greenXSize, greenYSize; // Tutunma kontrol kutusu boyutları

    [SerializeField] private LayerMask groundMask; // Kenarların yer aldığı layer mask
    [SerializeField] private float climbOffsetY = 2f; // Tırmanış sonrası karakterin Y pozisyonu için offset
    [SerializeField] private float climbDuration = 0.5f; // Tırmanma süresi
    private bool redBox, greenBox; // Kenar algılama kutuları

    private bool isClimbing = false; // Tırmanış durumu
    private bool isTouchingWall = false;

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
    private bool isShooting = false;
    private bool hasJumped = false;

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
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(_currentHealth, maxHealth);
        }
    }

    private void Update()
    {
        if (_isDead || isRolling || isClimbing) return;

        // Hareket ve diğer kontroller
        if (!isCrouching && !isGrabbed)
            HandleMovement();

        HandleJump();
        HandleShoot();
        HandleRoll();
        HandleCrouch();
        UpdateFirePointPosition();
        HandleLedgeGrab(); // Kenar tutunmayı kontrol et

        // Düşme durumunu kontrol et
        HandleFalling();
        
        _isGrounded = IsGrounded();

        // Debug için
        Debug.Log($"isGrounded: {_isGrounded}, velocityY: {_rb.velocity.y}");
    
        if (_isGrounded)
        {
            _animator.SetBool(IsFalling, false);
        }
    }
    private void HandleFalling()
    {
        // Eğer karakter havadaysa ve düşüyorsa (yere değmiyorsa ve hız negatifse)
        if (!_isGrounded && _rb.velocity.y < -1.5f)
        {
            // Düşme animasyonunu başlat
            if (!_animator.GetBool(IsFalling))
            {
                _animator.SetBool(IsFalling, true);
            }
        }
        else if (_isGrounded)
        {
            // Yere indiğinde düşme animasyonunu durdur
            if (_animator.GetBool(IsFalling))
            {
                _animator.SetBool(IsFalling, false);
            }
        }
    }

    


// Kenar tutunmayı kontrol eden fonksiyon
    private void HandleLedgeGrab()
    {
        if (isClimbing || isGrabbed || !hasJumped) return; // Eğer zıplama yapılmadıysa veya zaten tutunmuşsa işlem yapma

        // Kenarın üst kısmını kontrol et
        greenBox = Physics2D.OverlapBox(new Vector2(
                transform.position.x + (greenXOffset * transform.localScale.x),
                transform.position.y + greenYOffset),
            new Vector2(greenXSize, greenYSize), 0f, groundMask);

        // Kenarın önünü kontrol et
        redBox = Physics2D.OverlapBox(new Vector2(
                transform.position.x + (redXOffset * transform.localScale.x),
                transform.position.y + redYOffset),
            new Vector2(redXSize, redYSize), 0f, groundMask);

        // Eğer kenar algılandı ve tutunulabilir, ancak çarpışma yok
        if (greenBox && !redBox)
        {
            isGrabbed = true;
            _rb.velocity = Vector2.zero; // Hareketi durdur
            _rb.gravityScale = 0f;       // Yerçekimini devre dışı bırak
            StartClimbing(new Vector2(
                transform.position.x + (greenXOffset * transform.localScale.x),
                transform.position.y + greenYOffset)); // Tırmanış işlemini başlat
        }
    }

    

// Tırmanmayı başlatan fonksiyon
    private void StartClimbing(Vector2 targetPosition)
    {
        isClimbing = true;
        _rb.velocity = Vector2.zero; // Hareketi durdur
        _rb.gravityScale = 0f;       // Yerçekimini devre dışı bırak
        _animator.SetTrigger("Climb"); // Tırmanma animasyonunu tetikleyin

        // Tırmanma işlemini başlat
        StartCoroutine(ClimbCoroutine(targetPosition));
    }

    private IEnumerator ClimbCoroutine(Vector2 targetPosition)
    {
        // Tırmanma animasyonu süresince bekle (örneğin, 1 saniye)
        float climbAnimationDuration = 0.23f; // Tırmanma animasyonunun süresi
        yield return new WaitForSeconds(climbAnimationDuration);

        // Karakteri tırmanış sonrası yerine yerleştirin
        transform.position = new Vector3(targetPosition.x, targetPosition.y + climbOffsetY, transform.position.z);

        // Tırmanma işlemini sonlandırın
        FinishClimbing();
    }

    private void FinishClimbing()
    {
        isClimbing = false;
        isGrabbed = false;
        _rb.gravityScale = normalGravityScale; // Yerçekimini geri getir
        _animator.ResetTrigger("Climb"); // Tetikleyiciyi sıfırla
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        float directionMultiplier = _spriteRenderer.flipX ? 1f : -1f;

        // Red Box çizimi
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(
                transform.position.x + (redXOffset * directionMultiplier),
                transform.position.y + redYOffset),
            new Vector2(redXSize, redYSize));

        // Green Box çizimi
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector2(
                transform.position.x + (greenXOffset * directionMultiplier),
                transform.position.y + greenYOffset),
            new Vector2(greenXSize, greenYSize));
    }

    private void HandleMovement()
    {
        if (_isDead || isRolling || !canShoot || isShooting) return; // Hareketi durdur

        Vector2 moveInput = _playerInput.actions["Move"].ReadValue<Vector2>();
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        float currentSpeed = _playerInput.actions["Run"].IsPressed() ? moveSpeed * runSpeedMultiplier : moveSpeed;

        _rb.velocity = new Vector2(moveInput.x * currentSpeed, _rb.velocity.y);

        if (moveInput.x != 0)
        {
            _spriteRenderer.flipX = moveInput.x > 0;
        }

        _animator.SetBool(IsMoving, isMoving);

        // Cinemachine offset'i güncelle
        UpdateCinemachineOffset(isMoving);
    }



    private void HandleShoot()
    {
        // Ateş etme, hareket veya diğer eylemleri durdurur
        if (_shootAction.triggered && canShoot && !_isDead && _isGrounded && !isRolling && !isCrouching && !isShooting)
        {
            StartCoroutine(ShootCoroutine());
        }
    }

    private IEnumerator ShootCoroutine()
    {
        isShooting = true; // Ateş etme sırasında diğer eylemleri engelle
        canShoot = false;  // Cooldown süresi boyunca tekrar ateş edilmesin

        _rb.velocity = Vector2.zero; // Hareketi durdur
        _animator.SetTrigger(Shoot); // Ateş animasyonunu tetikle

        // Animasyonun tamamlanma süresi kadar bekle
        float shootAnimationDuration = 1f; // Ateş animasyonu süresi
        yield return new WaitForSeconds(shootAnimationDuration);

        // Mermiyi oluştur ve ateş et
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        Vector2 shootDirection = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
        bulletRb.velocity = shootDirection * bulletSpeed;

        // Cooldown süresi boyunca bekle
        float remainingCooldown = shootCooldown - shootAnimationDuration; // Eğer cooldown animasyon süresinden fazlaysa
        if (remainingCooldown > 0f)
        {
            yield return new WaitForSeconds(remainingCooldown);
        }

        // Ateş etmeye ve hareket etmeye izin ver
        isShooting = false;
        canShoot = true;
    }


    private void HandleJump()
    {
        if (isCrouching || !canShoot || isShooting) return; // Ateş ederken veya zıplamaya uygun değilse engelle

        if (_jumpAction.triggered && remainingJumps > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, fastJumpForce);
            remainingJumps--;
            hasJumped = true; // Zıplama gerçekleştirildiği için işaretle

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
    
    private void HandleRoll()
    {
        // Eğer havadaysa veya ölü durumda roll yapılmasın
        if (!_isGrounded || _isDead || Mathf.Abs(_rb.velocity.y) > 0.1f) return;

        // Roll girişini kontrol et
        if (_rollAction.triggered && canRoll && !isRolling)
        {
            StartCoroutine(PerformRoll());
        }
    }

   
    private void HandleCrouch()
    {
        // Eğer havadaysa veya hareket eden roll veya ölüm durumundaysa crouch'u iptal et
        if (!_isGrounded || isRolling || _isDead || Mathf.Abs(_rb.velocity.y) > 0.1f)
        {
            if (isCrouching)
            {
                ExitCrouch(); // Eğilme durumundan çık
            }
            return; // İşlemden çık
        }

        // Crouch girişini kontrol et
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
            hasJumped = false;
            _rb.gravityScale = normalGravityScale;

            // Animasyonları sıfırla
            _animator.SetBool(IsFalling, false);
            _animator.SetBool(IsJumping, false);
        }
        
        Debug.Log($"Collision Enter with: {collision.gameObject.name}");
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
            Debug.Log("isGrounded set to TRUE");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"Collision Exit with: {collision.gameObject.name}");
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
            Debug.Log("isGrounded set to FALSE");
        }
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.5f; 
        Vector2 raycastOrigin = new Vector2(transform.position.x, transform.position.y - 0.5f); // Alt kısımdan başla
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, extraHeight, groundMask);

        // Debug için ray'ı çiz
        Debug.DrawRay(raycastOrigin, Vector2.down * extraHeight, Color.green);

        return hit.collider != null;
    }


    public void TakeDamage(int damageAmount)
    {
        if (_isDead) return;

        _currentHealth -= damageAmount;

        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(_currentHealth, maxHealth); // Sağlık Bar'ı güncelle
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }


    public void Heal(int healAmount)
    {
        if (_isDead) return;

        _currentHealth += healAmount;
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }

        // Sağlık barını güncelle
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(_currentHealth, maxHealth);
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

   
}