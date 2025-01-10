using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    public int damageAmount = 15; // Verilecek hasar miktarı
    private RoboCop parentEnemy;  // Parent Enemy referansı

    private void Start()
    {
        // Parent Enemy referansını al
        parentEnemy = GetComponentInParent<RoboCop>();
        if (parentEnemy == null)
        {
            Debug.LogError("DamageTrigger: Parent RoboCop script not found!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damageAmount); // Oyuncuya hasar ver
                Debug.Log("DamageTrigger: Player took damage.");
            }
        }
    }
}