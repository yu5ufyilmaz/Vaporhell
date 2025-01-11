using UnityEngine;
using UnityEngine.SceneManagement; // Sahne geçişleri için gerekli

public class DoorController : MonoBehaviour
{
    public string nextSceneName; // Geçilecek sahnenin adı
    private EnemyBase[] enemies; // Sahnedeki düşmanları tutar
    private Collider2D doorCollider; // Kapının Collider'ı

    void Start()
    {
        // Sahnedeki tüm EnemyBase script'lerini bul
        enemies = FindObjectsOfType<EnemyBase>();
        Debug.Log($"Kapı: {enemies.Length} düşman bulundu.");

        // Kapının Collider bileşenini al ve başlangıçta devre dışı bırak
        doorCollider = GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.enabled = false; // Trigger başlangıçta kapalı
        }
        else
        {
            Debug.LogError("Kapıya bir Collider bileşeni eklenmemiş!");
        }
    }

    void Update()
    {
        // Tüm düşmanların öldüğünü kontrol et
        if (AllEnemiesDefeated())
        {
            ActivateDoor();
        }
    }

    private bool AllEnemiesDefeated()
    {
        foreach (var enemy in enemies)
        {
            if (enemy != null && enemy.currentHealth > 0)
            {
                return false; // Sağ kalan bir düşman var
            }
        }
        return true; // Tüm düşmanlar öldü
    }

    private void ActivateDoor()
    {
        Debug.Log("Kapı açıldı! Tüm düşmanlar öldü, trigger aktif.");
        
        if (doorCollider != null)
        {
            doorCollider.enabled = true; // Kapı trigger'ını etkinleştir
        }

        // Artık Update'te kontrol etmeye gerek yok
        enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Oyuncu kapıya ulaştı. Sonraki sahneye geçiliyor.");
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName); // Belirtilen sahneyi yükle
        }
        else
        {
            Debug.LogError("Geçilecek sahne adı boş! 'nextSceneName' değişkenine bir sahne adı girin.");
        }
    }
}
