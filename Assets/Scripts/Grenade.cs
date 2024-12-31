using UnityEngine;

public class Grenade : MonoBehaviour
{
    public int damage = 20; // Patlamanın vereceği hasar
    public float explosionRadius = 1.5f; // Patlama yarıçapı
    public LayerMask damageableLayers; // Hasar verilebilir katmanlar
    public GameObject explosionEffect; // Patlama efekti prefabı
    public float explosionDelay = 1f; // Yere çarptıktan sonra patlama gecikmesi

    private bool hasLanded = false;

    public void OnLand()
    {
        if (hasLanded) return;

        hasLanded = true;

        // Patlamayı gecikmeli olarak tetikle
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        // Patlama efekti oluştur
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Patlama alanındaki objelere hasar ver
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController playerController = hit.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(damage);
                }
            }
        }

        // Bombayı yok et
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Patlama alanını görsel olarak göster
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}