using System.Collections;
using UnityEngine;

public class PlayerLedgeGrab : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;

    private bool redBox, greenBox;
    public bool isGrabbed;
    public bool isClimbing;
    public LayerMask groundMask;

    public float redXOffset, redYOffset, redXSize, redYSize;
    public float greenXOffset, greenYOffset, greenXSize, greenYSize;

    public Vector2 climbOffset; // Tırmanma sırasında karakterin taşınacağı offset
    public float climbDuration = 0.5f; // Tırmanma süresi

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isClimbing) return; // Tırmanma sırasında başka işlem yapma

        greenBox = Physics2D.OverlapBox(
            new Vector2(transform.position.x + (greenXOffset * transform.localScale.x), transform.position.y + greenYOffset),
            new Vector2(greenXSize, greenYSize), 
            0f, 
            groundMask
        );

        redBox = Physics2D.OverlapBox(
            new Vector2(transform.position.x + (redXOffset * transform.localScale.x), transform.position.y + redYOffset),
            new Vector2(redXSize, redYSize), 
            0f, 
            groundMask
        );

        if (greenBox && !redBox && !isGrabbed)
        {
            StartLedgeGrab();
        }
    }

    private void StartLedgeGrab()
    {
        isGrabbed = true;
        rb.velocity = Vector2.zero; // Hareketi durdur
        rb.gravityScale = 0; // Yerçekimini devre dışı bırak
        animator.SetBool("isHanging", true); // Tutunma animasyonu başlat
    }

    public void StartClimbing()
    {
        if (isClimbing || !isGrabbed) return;

        isClimbing = true;
        animator.SetTrigger("climb"); // Tırmanma animasyonu
        StartCoroutine(ClimbLedge());
    }

    private IEnumerator ClimbLedge()
    {
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + climbOffset;

        float elapsedTime = 0f;

        while (elapsedTime < climbDuration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / climbDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        // Tırmanma tamamlandıktan sonra durum sıfırlanır
        isClimbing = false;
        isGrabbed = false;
        rb.gravityScale = 1; // Yerçekimini geri getir
        animator.SetBool("isHanging", false); // Tutunma animasyonu sonlanır
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            new Vector2(transform.position.x + (redXOffset * transform.localScale.x), transform.position.y + redYOffset),
            new Vector2(redXSize, redYSize)
        );

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector2(transform.position.x + (greenXOffset * transform.localScale.x), transform.position.y + greenYOffset),
            new Vector2(greenXSize, greenYSize)
        );
    }
}
