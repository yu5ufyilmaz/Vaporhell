using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    // --- [DEBUG OPTIONS] ---
    [Header("Debug Ray Options")]
    [SerializeField] private bool showDebugHorizontal = true; // Yatay Ray çizilsin mi?
    [SerializeField] private bool showDebugVertical = true;   // Dikey Ray çizilsin mi?

    // Animation Parameters
    [Header("Animation Parameters")]
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int DieTrigger = Animator.StringToHash("DieTrigger");
    private static readonly int RollTrigger = Animator.StringToHash("RollTrigger");
    private static readonly int IsCrouching = Animator.StringToHash("isCrouching");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int IsClimbingParam = Animator.StringToHash("isClimbing");

    // Health Parameters
    [Header("Health Parameters")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    [SerializeField] private HealthBarUI healthBarUI;

    // Combat Parameters
    [Header("Combat Parameters")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float shootCooldown = 0.4f;
    private bool canShoot = true;

    [Header("Fire Point Offsets")]
    [SerializeField] private Vector2 firePointOffsetRight = new Vector2(1f, 0f);
    [SerializeField] private Vector2 firePointOffsetLeft = new Vector2(-1f, 0f);

    // Teleport Parameters
    [Header("Teleport Parameters")]
    [SerializeField] private float teleportDistance = 5f;
    [SerializeField] private float teleportCooldown = 1f;
    [SerializeField] private LayerMask teleportObstacleMask;
    [SerializeField] private GameObject teleportIndicatorPrefab;
    private bool canTeleport = true;
    private InputAction teleportAction;
    private GameObject currentTeleportIndicator;

    [Header("Ledge Climb Parameters")]
    [SerializeField] private LayerMask groundMask;
    private bool isClimbing = false;
    private bool isGrabbed = false;
    [SerializeField] private float topRayOffsetY = 1.2f;      // Üst yatay ray Y offset
    [SerializeField] private float bottomRayOffsetY = 0.5f;   // Alt yatay ray Y offset
    private float verticalRayStartOffset = -0.2f;

    [SerializeField] private float horizontalRayOriginOffsetY = 1.2f; 
    [SerializeField] private float horizontalRayDistance = 1.2f;      
    [SerializeField] private float verticalRayOriginOffsetY = 0.2f;   
    [SerializeField] private float verticalRayDistance = 2.5f;        
    [SerializeField] private float climbDuration = 0.6f;              
    [SerializeField] private float climbTopOffset = 0.5f;            

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

    [Header("Ground Check Parameters")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Crouch Parameters
    [Header("Crouch Parameters")]
    [SerializeField] private Vector2 crouchColliderSize = new Vector2(1f, 0.5f);
    [SerializeField] private Vector2 normalColliderSize = new Vector2(1f, 1f);
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

    // Wall Slide Parameters
    [Header("Wall Slide Parameters")]
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float wallSlideSpeed = -1.5f;
    [SerializeField] private float wallSlideGravity = 0.5f;
    [SerializeField] private LayerMask wallLayer;
    private bool isWallSliding = false;

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
    private Animator animator;
    private PlayerInput playerInput;
    private CinemachineVirtualCamera cinemachineVirtualCamera;

    // Input Actions
    private InputAction shootAction;
    private InputAction jumpAction;
    private InputAction rollAction;
    private InputAction crouchAction;

    // Yeni Değişken: Önceki zemin durumu
    private bool previousIsGrounded;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions["Shoot"];
        jumpAction = playerInput.actions["Jump"];
        rollAction = playerInput.actions["Roll"];
        crouchAction = playerInput.actions["Crouch"];
        teleportAction = playerInput.actions["Teleport"];
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        remainingJumps = maxJumps;
        _rb.gravityScale = normalGravityScale;

        if (animator == null)
        {
            Debug.LogError("Animator bileşeni bulunamadı! Lütfen Player GameObject'inizde Animator olduğundan emin olun.");
        }
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }

        // Başlangıçta önceki zemin durumu
        previousIsGrounded = IsGrounded();
    }

    private void Update()
    {
        if (_isDead || isClimbing)
        {
            // Tırmanma veya roll sırasında normal inputlar iptal
            return;
        }

        // Önceki frame'deki zemin durumu
        previousIsGrounded = _isGrounded;
        // Şu anki zemin durumu
        _isGrounded = IsGrounded();

        if (_isGrounded && !previousIsGrounded)
        {
            // Karakter yere indiğinde zıplama sayısını sıfırla
            remainingJumps = maxJumps;
            animator.SetBool(IsFalling, false);
            animator.SetBool(IsJumping, false);
        }

        if (!isCrouching && !isGrabbed)
        {
            HandleMovement();
        }

        ShowVerticalRayForDebugAlways();
        CheckWall();
        HandleWallSlide();
        HandleJump();
        HandleShoot();
        HandleRoll();
        HandleCrouch();
        UpdateFirePointPosition();
        HandleTeleport();

        // Çoklu ray ledge grab
        HandleLedgeGrabRaycast_MultiRay();

        HandleFalling();
    }

    private void ShowVerticalRayForDebugAlways()
    {
        if (!showDebugVertical) return;

        float direction = _spriteRenderer.flipX ? 1f : -1f;
        Vector2 verticalRayOrigin = new Vector2(transform.position.x + 1.0f, transform.position.y + verticalRayStartOffset);
        Debug.DrawRay(verticalRayOrigin, Vector2.up * verticalRayDistance, Color.green);
    }

    private void HandleLedgeGrabRaycast_MultiRay()
    {
        if (isClimbing || isGrabbed)
            return;

        // Karakterin baktığı yön (flipX durumuna göre -1 veya 1)
        float direction = _spriteRenderer.flipX ? 1f : -1f;

        // Üst ray başlangıç noktası ve yön
        Vector2 topRayOrigin = new Vector2(transform.position.x + direction * horizontalRayDistance, transform.position.y + topRayOffsetY);
        Vector2 horizontalDir = Vector2.left * direction;

        RaycastHit2D topWallHit = Physics2D.Raycast(topRayOrigin, horizontalDir, horizontalRayDistance, groundMask);

        if (showDebugHorizontal)
        {
            Debug.DrawRay(topRayOrigin, horizontalDir * horizontalRayDistance, Color.yellow);
            Debug.Log("[DEBUG] Üst ray gönderildi: " + (topWallHit.collider != null ? "Çarptı: " + topWallHit.collider.name : "Çarpmadı"));
        }

        // Alt ray başlangıç noktası ve yön
        Vector2 bottomRayOrigin = new Vector2(transform.position.x + direction * horizontalRayDistance, transform.position.y + bottomRayOffsetY);
        RaycastHit2D bottomWallHit = Physics2D.Raycast(bottomRayOrigin, horizontalDir, horizontalRayDistance, groundMask);

        if (showDebugHorizontal)
        {
            Debug.DrawRay(bottomRayOrigin, horizontalDir * horizontalRayDistance, Color.cyan);
            Debug.Log("[DEBUG] Alt ray gönderildi: " + (bottomWallHit.collider != null ? "Çarptı: " + bottomWallHit.collider.name : "Çarpmadı"));
        }

        // Mantık Kontrolleri:
        if (topWallHit.collider != null && bottomWallHit.collider != null)
        {
            Debug.Log("[DEBUG] Hem üst ray hem de alt ray çarptı. Kayma başlatılıyor...");
            SlideOffWall(); // Kayma davranışını başlat
            return;
        }

        if (topWallHit.collider != null && bottomWallHit.collider == null)
        {
            Debug.Log("[DEBUG] Sadece üst ray çarptı. Kayma başlatılıyor...");
            SlideOffWall(); // Kayma davranışını başlat
            return;
        }

        if (topWallHit.collider == null && bottomWallHit.collider == null)
        {
            Debug.Log("[DEBUG] Ne üst ray ne de alt ray çarptı. Tırmanma iptal edildi.");
            return;
        }

        if (topWallHit.collider == null && bottomWallHit.collider != null)
        {
            Debug.Log("[DEBUG] Sadece alt ray çarptı. Tırmanma başlatılıyor...");
            Vector2 contactPoint = bottomWallHit.point;
            Vector2 snapPos = new Vector2(contactPoint.x + (0.05f * direction), transform.position.y);

            // ============ DİKEY RAY ============ 
            Vector2 verticalRayOrigin = new Vector2(transform.position.x + direction * horizontalRayDistance, transform.position.y + verticalRayStartOffset);
            RaycastHit2D topHit = Physics2D.Raycast(verticalRayOrigin, Vector2.up, verticalRayDistance, groundMask);

            if (showDebugVertical)
            {
                Debug.DrawRay(verticalRayOrigin, Vector2.up * verticalRayDistance, Color.green);
                Debug.Log("[DEBUG] Dikey ray gönderildi: " + (topHit.collider != null ? "Çarptı: " + topHit.collider.name : "Çarpmadı"));
            }

            if (topHit.collider == null)
            {
                Debug.Log("[DEBUG] Dikey ray üst kenar bulamadı. Tırmanma başlatılmadı.");
                return;
            }

            Vector2 topPoint = topHit.point;
            StartMultiRayClimbApproach(snapPos, topPoint);
        }
    }

    private void SlideOffWall()
    {
        // Karakterin duvardan aşağı kaymasını sağla
        _rb.velocity = new Vector2(0, -5f); // Aşağı doğru bir hız ver (kayma davranışı)
        _rb.gravityScale = fallingGravityScale; // Normal düşme hızına geç
        animator.SetBool(IsFalling, true); // Düşme animasyonu tetiklenebilir
    }

    [SerializeField] private float tileSize = 1.0f; // Tileset karelerinin boyutu (örneğin, 1 birim)

    private void StartMultiRayClimbApproach(Vector2 snapPos, Vector2 topPoint)
    {
        Debug.Log("[DEBUG] StartMultiRayClimbApproach çağrıldı. Tırmanma animasyonu tetikleniyor...");

        isClimbing = true;
        isGrabbed = true;

        animator.SetBool(IsClimbingParam, true);

        // Karakteri hizala (ilk pozisyona yerleştir)
        transform.position = new Vector3(snapPos.x, snapPos.y, transform.position.z);

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;

        // Duvarın üst noktasına hizala
        float alignedTopY = Mathf.Floor(topPoint.y / tileSize) * tileSize + tileSize; // En yakın üst hizaya ayarla
        StartCoroutine(MultiRayClimbCoroutine(new Vector2(topPoint.x, alignedTopY)));
    }

    private IEnumerator MultiRayClimbCoroutine(Vector2 topPoint)
    {
        Debug.Log("[DEBUG] MultiRayClimbCoroutine başladı. Karakter tırmanıyor...");

        float elapsedTime = 0f;
        Vector2 startPosition = transform.position;

        while (elapsedTime < climbDuration)
        {
            transform.position = Vector2.Lerp(startPosition, topPoint, elapsedTime / climbDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = topPoint;

        Debug.Log("[DEBUG] MultiRayClimbCoroutine sona erdi. Tırmanma tamamlandı.");
        FinishClimbing();

        // Raycast kontrollerini kısa bir süre devre dışı bırak
        yield return new WaitForSeconds(0.1f);
        isClimbing = false; // Yeniden raycast yapılabilir
    }

    private void FinishClimbing()
    {
        Debug.Log("[DEBUG] FinishClimbing çağrıldı. Tırmanma animasyonu bitiriliyor...");

        isClimbing = false;
        isGrabbed = false;
        animator.SetBool(IsClimbingParam, false);

        // Yerçekimi ve pozisyon ayarları
        _rb.gravityScale = 1f;

        // Raycast başlangıç pozisyonunu değiştir
        transform.position = new Vector3(transform.position.x + 0.2f, transform.position.y + 1.7f, transform.position.z); // X ekseninde bir miktar kaydır
    }

    private void HandleFalling()
    {
        if (!_isGrounded && !isClimbing)
        {
            // Yerçekimini artırarak karakterin daha hızlı düşmesini sağlar
            _rb.gravityScale = fallingGravityScale;

            // Düşme animasyonunu tetikler
            if (_rb.velocity.y < -1.5f)
            {
                if (!animator.GetBool(IsFalling))
                {
                    animator.SetBool(IsFalling, true);
                }
            }
        }
        else
        {
            // Yerçekimini normal seviyeye geri getirir
            _rb.gravityScale = normalGravityScale;

            // Düşme animasyonunu sıfırlar
            if (animator.GetBool(IsFalling))
            {
                animator.SetBool(IsFalling, false);
            }
        }
    }



    private void HandleJump()
    {
        if (isCrouching || !canShoot || isShooting || isClimbing)
            return;

        if (jumpAction.triggered && remainingJumps > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, fastJumpForce);
            remainingJumps--;
            hasJumped = true;
            _rb.gravityScale = fallingGravityScale;

            if (remainingJumps == maxJumps - 1)
            {
                animator.SetBool(IsJumping, true);
            }
            else if (remainingJumps < maxJumps - 1)
            {
                animator.SetBool(IsJumping, false);
                animator.SetBool(IsJumping, true);
            }
        }
    }

    private void CheckWall()
    {
        float direction = _spriteRenderer.flipX ? -1f : 1f;
        Vector2 rayOrigin = transform.position;
        RaycastHit2D wallHit = Physics2D.Raycast(rayOrigin, Vector2.right * direction, wallCheckDistance, wallLayer);

        if (wallHit.collider != null)
        {
            StartWallSlide();
        }
        else
        {
            StopWallSlide();
        }
    }

    private void StartWallSlide()
    {
        if (_isGrounded || isClimbing)
        {
            StopWallSlide();
            return;
        }
        isWallSliding = true;
        _rb.gravityScale = wallSlideGravity;
    }

    private void StopWallSlide()
    {
        if (!isWallSliding) return;
        isWallSliding = false;
        _rb.gravityScale = normalGravityScale;
    }

    private void HandleWallSlide()
    {
        if (isWallSliding)
        {
            if (_rb.velocity.y < wallSlideSpeed)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, wallSlideSpeed);
            }
        }
    }

    private void HandleMovement()
    {
        if (_isDead || isRolling || !canShoot || isShooting) return;

        Vector2 moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        bool isMoving = Mathf.Abs(moveInput.x) > 0.1f;
        float currentSpeed = playerInput.actions["Run"].IsPressed() ? moveSpeed * runSpeedMultiplier : moveSpeed;

        _rb.velocity = new Vector2(moveInput.x * currentSpeed, _rb.velocity.y);

        if (moveInput.x != 0)
        {
            _spriteRenderer.flipX = moveInput.x > 0;
        }

        animator.SetBool(IsMoving, isMoving);
        UpdateCinemachineOffset(isMoving);
    }

    private void HandleShoot()
    {
        if (shootAction.triggered && canShoot && !_isDead && _isGrounded && !isRolling && !isCrouching && !isShooting)
        {
            StartCoroutine(ShootCoroutine());
        }
    }

    private IEnumerator ShootCoroutine()
    {
        isShooting = true;
        canShoot = false;

        _rb.velocity = Vector2.zero;
        animator.SetTrigger(Shoot);

        float shootAnimationDuration = 0.5f;
        yield return new WaitForSeconds(shootAnimationDuration);

        Vector2 shootDirection = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = shootDirection * bulletSpeed;

        float remainingCooldown = shootCooldown - shootAnimationDuration;
        if (remainingCooldown > 0f)
        {
            yield return new WaitForSeconds(remainingCooldown);
        }

        isShooting = false;
        canShoot = true;
    }

    private void HandleTeleport()
    {
        if (!canTeleport || _isDead || isRolling || isClimbing) return;

        Vector2 moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        if (teleportAction.triggered && moveInput != Vector2.zero)
        {
            Vector2 teleportDirection = moveInput.normalized;
            Vector2 targetPosition = (Vector2)transform.position + (teleportDirection * teleportDistance);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, teleportDirection, teleportDistance, teleportObstacleMask);
            if (hit.collider != null)
            {
                targetPosition = hit.point - (teleportDirection * 0.5f);
            }

            StartCoroutine(TeleportSequence(targetPosition));
        }
    }

    private IEnumerator TeleportSequence(Vector2 targetPosition)
    {
        canTeleport = false;
        _rb.velocity = Vector2.zero;
        transform.position = targetPosition;
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }

    private void UpdateFirePointPosition()
    {
        if (_spriteRenderer.flipX)
        {
            firePoint.localPosition = firePointOffsetLeft;
        }
        else
        {
            firePoint.localPosition = firePointOffsetRight;
        }
    }

    private void HandleRoll()
    {
        if (_isDead || isRolling || !canRoll) return;

        if (rollAction.triggered && !isRolling)
        {
            StartCoroutine(PerformRoll());
        }
    }


    private IEnumerator PerformRoll()
    {
        isRolling = true;
        canRoll = false;
        animator.SetTrigger(RollTrigger);

        float rollDirection = _spriteRenderer.flipX ? 1f : -1f;
        
        _rb.velocity = new Vector2(rollDirection * rollSpeed, _rb.velocity.y);
        
        yield return new WaitForSeconds(rollDuration);
        
        _rb.velocity = new Vector2(0, _rb.velocity.y);
        isRolling = false;
        
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }


    private void HandleCrouch()
    {
        // Crouch yapılamayacak durumlar:
        if (!_isGrounded || isRolling || _isDead || Mathf.Abs(_rb.velocity.y) > 0.1f)
        {
            if (isCrouching)
            {
                ExitCrouch();
            }
            return;
        }

        // Crouch Action basılı tutuluyorsa EnterCrouch çağır
        if (crouchAction.IsPressed())
        {
            EnterCrouch();
        }
        else
        {
            ExitCrouch();
        }
    }

    private void EnterCrouch()
    {
        if (!isCrouching)
        {
            isCrouching = true;
            animator.SetBool(IsCrouching, true);
            _rb.velocity = Vector2.zero;
        }
    }

    private void ExitCrouch()
    {
        if (isCrouching)
        {
            isCrouching = false;
            animator.SetBool(IsCrouching, false);
        }
    }

    private void UpdateCinemachineOffset(bool isMoving)
    {
        Vector3 newTargetOffset = isMoving
            ? Vector3.zero
            : (_spriteRenderer.flipX ? offsetRight : offsetLeft);

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
        var framingTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
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
            hasJumped = false;
            _rb.gravityScale = normalGravityScale;

            animator.SetBool(IsFalling, false);
            animator.SetBool(IsJumping, false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Bu metot artık zıplama sayısını sıfırlamıyor
        }
    }

    private bool IsGrounded()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask) != null;
        return grounded;
    }

    public void TakeDamage(int damageAmount)
    {
        if (_isDead || isRolling) return; // Roll sırasında hasar almayı engelle

        currentHealth -= damageAmount;

        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    public void TeleportTo(Vector3 targetPosition)
    {
        // Opsiyonel: Teleport animasyonu, efekt vb.
        // Örnek: Karakterin velocity'sini sıfırla
        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
        }

        // Pozisyonu ayarla
        transform.position = targetPosition;

        // Opsiyonel: Bir animasyon (fade out / fade in) oynatılabilir
        // _animator.SetTrigger("Teleport");

        // vs. ek efektler
    }

    public void Heal(int healAmount)
    {
        if (_isDead) return;

        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        animator.SetTrigger(DieTrigger);
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        _rb.constraints = RigidbodyConstraints2D.FreezePositionY;
        playerInput.enabled = false;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}

