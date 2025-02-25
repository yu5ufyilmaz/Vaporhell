using UnityEngine;

public class Grenade : MonoBehaviour
{
    public int damage = 20; // Patlamanın vereceği hasar
    public float explosionRadius = 1.5f; // Patlama yarıçapı
    public LayerMask damageableLayers; // Hasar verilebilir katmanlar
    public GameObject explosionEffect; // Patlama efekti prefabı
    public float explosionDelay = 1f; // Yere çarptıktan sonra patlama gecikmesi

    private Rigidbody2D rb; // Rigidbody2D bileşeni
    private bool hasLanded = false; // Bomba yere çarptı mı?

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D bileşenini al
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Yere çarpma kontrolü (Zemin katmanına çarpıyorsa)
        if (collision.gameObject.CompareTag("Ground") && !hasLanded)
        {
            OnLand(); // Yere çarptığında işlemi başlat
        }
    }

    public void OnLand()
    {
        if (hasLanded) return; // Zaten yere çarpmışsa işlem yapma

        hasLanded = true;

        // Rigidbody'yi durdur ve hareketi devre dışı bırak
        rb.linearVelocity = Vector2.zero; // Hareketi durdur
        rb.bodyType = RigidbodyType2D.Static; // Statik yaparak sabitle

        // Patlamayı gecikmeli olarak tetikle
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Explode()
    {
        // Patlama efektini child olarak kullanıyorsanız
        if (explosionEffect != null)
        {
            explosionEffect.transform.parent = null; // Parent'tan ayır
            explosionEffect.SetActive(true); // Efekti aktif et

            ParticleSystem ps = explosionEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play(); // Particle System varsa başlat
            }

            Destroy(explosionEffect, 2f); // Efekt bittikten sonra yok et
        }

        // Patlama alanındaki objelere hasar ver
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, damageableLayers);
        foreach (Collider2D hit in hits)
        {
            PlayerController playerController = hit.GetComponentInParent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
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
