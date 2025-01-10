using UnityEngine;

public class SpiderDamageTrigger : MonoBehaviour
{
    public int explosionDamage = 50; // Patlama sırasında verilecek hasar

    // Bu fonksiyon patlama sırasında çağrılır
    public void ExplodeDamage()
    {
        // CircleCollider2D bileşenini al
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            Debug.LogError("SpiderDamageTrigger: CircleCollider2D not found!");
            return;
        }

        // Collider'ın yarıçapını al ve pozisyonunu kontrol et
        float explosionRadius = circleCollider.radius;
        Vector3 explosionPosition = transform.position;

        Debug.Log("Explosion radius (from CircleCollider2D): " + explosionRadius);
        Debug.Log("Explosion position: " + explosionPosition);

        // Çarpışma kontrolü
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            Debug.Log("Hit object: " + hitCollider.name); // Çarpışan objeyi logla

            if (hitCollider.CompareTag("Player"))
            {
                PlayerController playerController = hitCollider.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(explosionDamage);
                    Debug.Log("Player took explosion damage: " + explosionDamage);
                }
            }
        }
    }


    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            Gizmos.DrawWireSphere(transform.position, circleCollider.radius);
        }
    }


}