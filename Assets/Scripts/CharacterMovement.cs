using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    // Hareket ve zıplama için gerekli değişkenler
    public float moveSpeed = 5f;
    public float runSpeedMultiplier = 1.5f;
    public float jumpForce = 10f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    private bool isGrounded;
    private float groundCheckTimer = 0.1f;
    private float groundCheckCounter;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction shootAction;
    private Vector2 cachedVelocity;
    private Gamepad cachedGamepad;

    void Awake()
    {
        // PlayerInput bileşenini al
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        runAction = playerInput.actions["Run"];
        shootAction = playerInput.actions["Shoot"];
    }

    void Start()
    {
        // Rigidbody ve Animator bileşenlerini al
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        groundCheckCounter = groundCheckTimer;
        cachedGamepad = Gamepad.current;
    }

    void Update()
    {
        // Karakterin sağa-sola hareketi
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float currentSpeed = moveSpeed;

        // Koşma kontrolü (Shift tuşu veya Gamepad RB tuşu)
        if (runAction.IsPressed())
        {
            currentSpeed *= runSpeedMultiplier;
        }

        cachedVelocity.Set(moveInput.x * currentSpeed, rb.velocity.y);
        rb.velocity = cachedVelocity;

        // Karakterin yüzü hareket yönüne dönsün
        if (moveInput.x > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        // Gamepad kontrolü - sol joystick, d-pad ve butonlar
        if (cachedGamepad != null)
        {
            Vector2 gamepadMove = cachedGamepad.leftStick.ReadValue();
            if (gamepadMove == Vector2.zero)
            {
                gamepadMove = new Vector2(cachedGamepad.dpad.x.ReadValue(), cachedGamepad.dpad.y.ReadValue());
            }

            float gamepadSpeed = moveSpeed;
            if (cachedGamepad.rightShoulder.isPressed)
            {
                gamepadSpeed *= runSpeedMultiplier;
            }

            cachedVelocity.Set(gamepadMove.x * gamepadSpeed, rb.velocity.y);
            rb.velocity = cachedVelocity;

            // Gamepad üzerinden zıplama - Button South (A tuşu)
            if (cachedGamepad.buttonSouth.isPressed && isGrounded)
            {
                rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                isGrounded = false;
            }
        }

        // Zıplama (klavye ve diğer giriş cihazları için)
        if (jumpAction.triggered && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;
        }

        // Ateş etme
        if (shootAction.triggered)
        {
            Shoot();
        }

        // Animator parametrelerini ayarla (isteğe bağlı)
        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Yere temas kontrolü (sürekli)
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundCheckCounter -= Time.deltaTime;
            if (groundCheckCounter <= 0)
            {
                isGrounded = true;
                groundCheckCounter = groundCheckTimer;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Yerden ayrılma durumu
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void Shoot()
    {
        // Mermi oluştur ve ateş et
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = new Vector2(transform.localScale.x * bulletSpeed, 0);
    }
}