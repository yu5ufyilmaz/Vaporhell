using UnityEngine;

public class Grenade : MonoBehaviour
{
    public int damage = 20; // Merminin vereceği hasar
    public float explosionRadius = 1.5f; // Patlama yarıçapı
    public LayerMask playerLayer; // Hangi layer'daki objelere hasar vereceği

    public GameObject explosionEffect; // Patlama efekti prefabı

    void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
    }

    void Explode()
    {
        // Patlama efekti oluştur
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Patlama yarıçapındaki oyuncuyu bul
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Oyuncuya hasar ver
                hit.GetComponent<PlayerController>().TakeDamage(damage);
            }
        }

        // Mermiyi yok et
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}