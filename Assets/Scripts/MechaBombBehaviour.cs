using UnityEngine;

public class MechaBombBehavior : MonoBehaviour
{
    public GameObject grenadePrefab; // Patlayıcı prefabı
    public Transform firePoint; // Patlayıcının çıkış noktası
    public float grenadeSpeed = 10f;
    public float shootCooldown = 2f;

    private GameObject player;
    private float lastShootTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);

            if (distance < 10f && Time.time > lastShootTime + shootCooldown)
            {
                ThrowGrenade();
            }
        }
    }

    void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, firePoint.position, Quaternion.identity);

        // Yön belirleme
        Vector2 direction = (player.transform.position - firePoint.position).normalized;

        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();
        rb.velocity = direction * grenadeSpeed;

        lastShootTime = Time.time;
    }
}