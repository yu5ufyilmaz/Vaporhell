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
    [SerializeField] private float shootCooldown = 0.4f; // Ateş etme cooldown süresi
    private bool canShoot = true; // Ateş etmeye izin durumu

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

    // Ledge Climb Parameters (Dead Cells benzeri)
    [Header("Ledge Climb Parameters")]
    [SerializeField] private float redXOffset = 1.0f;    
    [SerializeField] private float redYOffset = 0.5f;    
    [SerializeField] private float redXSize = 0.5f;      
    [SerializeField] private float redYSize = 0.5f;      

    [SerializeField] private float greenXOffset = 1.0f;  
    [SerializeField] private float greenYOffset = 1.0f;  
    [SerializeField] private float greenXSize = 0.5f;    
    [SerializeField] private float greenYSize = 0.5f;    

    [SerializeField] private LayerMask groundMask;        
    [SerializeField] private float climbOffsetY = 2f;     
    [SerializeField] private float climbDuration = 0.6f;  
    private bool redBox, greenBox;                        
    private bool isClimbing = false;                      
    private bool isGrabbed = false;                       

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

        Debug.Log("PlayerController Start() tamamlandı. isClimbing başlangıç: " + isClimbing);
    }

    private void Update()
    {
        // Debug Purpose
        // Karakterin durumu, input, vs.
        // Debug.Log($"Update -- isClimbing: {isClimbing}, isRolling: {isRolling}, isDead: {_isDead}");

        if (_isDead || isRolling || isClimbing) 
        {
            // Eğer tırmanma aktifse, normal inputlar iptal olsun
            if (isClimbing) 
            {
                // Debug.Log("Climbing aşamasında, diğer inputlar iptal.");
                return;
            }
            // isRolling veya _isDead durumunda da normal input iptal
            return; 
        }

        if (!isCrouching && !isGrabbed)
        {
            HandleMovement();
        }
        
        CheckWall();
        HandleWallSlide();

        HandleJump();
        HandleShoot();
        HandleRoll();
        HandleCrouch();
        UpdateFirePointPosition();
        HandleTeleport();
        HandleLedgeGrab();

        HandleFalling();
        _isGrounded = IsGrounded();

        // Debug.Log($"_isGrounded: {_isGrounded}, _rb.vel: {_rb.velocity}, isFalling: {animator.GetBool(IsFalling)}");

        if (_isGrounded)
        {
            animator.SetBool(IsFalling, false);
        }
    }

    private void HandleFalling()
    {
        if (!_isGrounded && _rb.velocity.y < -1.5f)
        {
            if (!animator.GetBool(IsFalling))
            {
                animator.SetBool(IsFalling, true);
                Debug.Log("Falling animasyonu tetiklendi (HandleFalling).");
            }
        }
        else if (_isGrounded)
        {
            if (animator.GetBool(IsFalling))
            {
                animator.SetBool(IsFalling, false);
                Debug.Log("Falling animasyonu durduruldu, yere basıldı (HandleFalling).");
            }
        }
    }

    private void HandleLedgeGrab()
    {
        // Eğer zaten tırmanma veya tutunma durumundaysak tekrar yapma
        if (isClimbing || isGrabbed) return;

        float directionMultiplier = _spriteRenderer.flipX ? 1f : -1f;

        // Green Box
        greenBox = Physics2D.OverlapBox(
            new Vector2(transform.position.x + (greenXOffset * directionMultiplier),
                        transform.position.y + greenYOffset),
            new Vector2(greenXSize, greenYSize),
            0f, groundMask);

        // Red Box
        redBox = Physics2D.OverlapBox(
            new Vector2(transform.position.x + (redXOffset * directionMultiplier),
                        transform.position.y + redYOffset),
            new Vector2(redXSize, redYSize),
            0f, groundMask);

        // Debug loglarla hangi değerleri aldığımızı net görmek
        if (greenBox) Debug.Log("Green Box TRUE -- Kenar üst kısım collider bulundu.");
        if (redBox) Debug.Log("Red Box TRUE -- Kenar önü collider bulundu.");

        // Eğer kenar üst kısmı var (greenBox) ama ön kısmı boş (redBox yok) => Tırmanma
        if (greenBox && !redBox)
        {
            Debug.Log("Ledge algılandı, StartClimbing çağrılıyor...");
            isGrabbed = true;
            _rb.velocity = Vector2.zero;
            _rb.gravityScale = 0f;
            StartClimbing(new Vector2(
                transform.position.x + (greenXOffset * directionMultiplier),
                transform.position.y + greenYOffset));
        }
    }

    private void StartClimbing(Vector2 targetPosition)
    {
        isClimbing = true;
        isGrabbed = true;
        
        // Debug
        Debug.Log("StartClimbing --> isClimbing = true; animator.SetBool(isClimbingParam, true)");

        animator.SetBool(IsClimbingParam, true);
        animator.SetBool(IsJumping, false);

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;

        StartCoroutine(ClimbCoroutine(targetPosition));
    }

    private IEnumerator ClimbCoroutine(Vector2 targetPosition)
    {
        Debug.Log($"ClimbCoroutine başladı, {climbDuration} sn bekleyecek...");
        yield return new WaitForSeconds(climbDuration);

        // Animasyon bittiğinde karakteri yukarı konumlandır
        transform.position = new Vector3(targetPosition.x, targetPosition.y + climbOffsetY, transform.position.z);
        Debug.Log($"ClimbCoroutine bitti, karakter yeni konuma taşındı: {transform.position}");

        FinishClimbing();
    }

    private void FinishClimbing()
    {
        isClimbing = false;
        isGrabbed = false;

        Debug.Log("FinishClimbing --> isClimbing = false; animator.SetBool(isClimbingParam, false)");

        animator.SetBool(IsClimbingParam, false);
        _rb.gravityScale = normalGravityScale;
    }

    private void HandleJump()
    {
        // Tırmanma sırasında jump iptal
        if (isCrouching || !canShoot || isShooting || isClimbing) 
            return;

        if (jumpAction.triggered && remainingJumps > 0)
        {
            Debug.Log("Jump action tetiklendi!");
            _rb.velocity = new Vector2(_rb.velocity.x, fastJumpForce);
            remainingJumps--;
            hasJumped = true;
            _rb.gravityScale = fallingGravityScale;

            if (remainingJumps == maxJumps - 1)
            {
                Debug.Log("Ilk zıplama (remainingJumps == maxJumps - 1).");
                animator.SetBool(IsJumping, true);
            }
            else if (remainingJumps < maxJumps - 1)
            {
                Debug.Log("Double Jump ya da daha fazla atlama (remainingJumps < maxJumps - 1).");
                animator.SetBool(IsJumping, false);
                animator.SetBool(IsJumping, true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!_spriteRenderer) return;

        float directionMultiplier = _spriteRenderer.flipX ? 1f : -1f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            new Vector2(transform.position.x + (redXOffset * directionMultiplier),
                        transform.position.y + redYOffset),
            new Vector2(redXSize, redYSize));

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector2(transform.position.x + (greenXOffset * directionMultiplier),
                        transform.position.y + greenYOffset),
            new Vector2(greenXSize, greenYSize));

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private void CheckWall()
    {
        // Karakterin baktığı yön; flipX=true => sola bakıyorsa direction = -1, aksi 1
        float direction = _spriteRenderer.flipX ? -1f : 1f;

        // Raycast başlangıç noktası karakterin ortası ya da ellerinin biraz yukarısı olabilir
        Vector2 rayOrigin = transform.position;
        // Duvar var mı yok mu?
        RaycastHit2D wallHit = Physics2D.Raycast(rayOrigin, Vector2.right * direction, wallCheckDistance, wallLayer);

        // Ray’le duvar bulduysak
        if (wallHit.collider != null)
        {
            // Eğimli mi, tam dik mi, ek kontrol edilebilir
            StartWallSlide();
        }
        else
        {
            StopWallSlide();
        }
    }

    private void StartWallSlide()
    {
        // Yere basılı değilsek, tırmanma yapmıyorsak, vb. kontrol edin:
        if (_isGrounded || isClimbing) 
        {
            StopWallSlide();
            return;
        }

        // Eğer havadaysanız ve duvara değdiyseniz:
        isWallSliding = true;
        _rb.gravityScale = wallSlideGravity; // normalGravityScale’in yarısı vs.
    
        // (Animator parametresi varsa) animator.SetBool("IsWallSliding", true);
    }

    private void StopWallSlide()
    {
        if (!isWallSliding) return;

        isWallSliding = false;
        _rb.gravityScale = normalGravityScale;

        // (Animator parametresi varsa) animator.SetBool("IsWallSliding", false);
    }

    private void HandleWallSlide()
    {
        if (isWallSliding)
        {
            if (_rb.velocity.y < wallSlideSpeed)
            {
                // y hızını bir limitin altına düşürme
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

        // Flip mantığınız
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
            Debug.Log("Shoot action tetiklendi!");
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
            Debug.Log("Teleport action tetiklendi!");
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
        Debug.Log($"Teleporting to {targetPosition}");
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
        if (!_isGrounded || _isDead || Mathf.Abs(_rb.velocity.y) > 0.1f) return;

        if (rollAction.triggered && canRoll && !isRolling)
        {
            Debug.Log("Roll action tetiklendi!");
            StartCoroutine(PerformRoll());
        }
    }

    private IEnumerator PerformRoll()
    {
        isRolling = true;
        canRoll = false;
        animator.SetTrigger(RollTrigger);

        float rollDirection = _spriteRenderer.flipX ? 1f : -1f; 
        _rb.gravityScale = 0;
        _rb.velocity = new Vector2(rollDirection * rollSpeed, 0);

        yield return new WaitForSeconds(rollDuration);

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = normalGravityScale;
        isRolling = false;

        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    private void HandleCrouch()
    {
        if (!_isGrounded || isRolling || _isDead || Mathf.Abs(_rb.velocity.y) > 0.1f)
        {
            if (isCrouching)
            {
                ExitCrouch();
            }
            return;
        }

        if (crouchAction.IsPressed() && !isCrouching)
        {
            EnterCrouch();
        }
        else if (!crouchAction.IsPressed() && isCrouching)
        {
            ExitCrouch();
        }
    }

    private void EnterCrouch()
    {
        isCrouching = true;
        animator.SetBool(IsCrouching, true);
        _rb.velocity = Vector2.zero;
        Debug.Log("Crouch mode aktif.");
    }

    private void ExitCrouch()
    {
        isCrouching = false;
        animator.SetBool(IsCrouching, false);
        Debug.Log("Crouch mode kapatıldı.");
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
            _isGrounded = true;
            remainingJumps = maxJumps;
            hasJumped = false;
            _rb.gravityScale = normalGravityScale;

            animator.SetBool(IsFalling, false);
            animator.SetBool(IsJumping, false);
            Debug.Log($"OnCollisionEnter2D --> Ground'a temas: _isGrounded = {_isGrounded}");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
            Debug.Log($"OnCollisionExit2D --> Ground'dan ayrıldı: _isGrounded = {_isGrounded}");
        }
    }

    private bool IsGrounded()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask) != null;
        // Debug.Log($"IsGrounded check: {grounded}");
        return grounded;
    }

    public void TakeDamage(int damageAmount)
    {
        if (_isDead) return;
        currentHealth -= damageAmount;

        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }

        Debug.Log($"TakeDamage({damageAmount}), currentHealth = {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
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

        Debug.Log($"Heal({healAmount}), currentHealth = {currentHealth}");
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

        Debug.Log("Karakter Öldü! Die() tetiklendi.");

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(5f);
        Debug.Log("DeathSequence --> Karakter yok ediliyor.");
        Destroy(gameObject);
    }
}
