using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeedMultiplier = 1.5f;
    public float jumpForce = 10f;
    private bool _isGrounded;
    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _runAction;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["Move"];
        _jumpAction = _playerInput.actions["Jump"];
        _runAction = _playerInput.actions["Run"];
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Karakterin sağa-sola hareketi
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        float currentSpeed = moveSpeed;

        // Koşma kontrolü
        if (_runAction.IsPressed())
        {
            currentSpeed *= runSpeedMultiplier;
        }

        _rb.velocity = new Vector2(moveInput.x * currentSpeed, _rb.velocity.y);

        // Karakterin yüzü hareket yönüne dönsün
        if (moveInput.x > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput.x < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }

        // Zıplama kontrolü
        if (_jumpAction.triggered && _isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        _isGrounded = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
        }
    }
}
