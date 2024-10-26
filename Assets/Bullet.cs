using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 5f;
    public bool isEnemyBullet = false; 

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isEnemyBullet && collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (!isEnemyBullet && collision.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}