using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [Header("Portal Bilgileri")]
    [SerializeField] private string portalID;        // Benzersiz portal ID
    [SerializeField] private bool isActivated;       // Portal aktif mi?
    [SerializeField] private Transform spawnPoint;   // Spawn noktası
    [SerializeField] private GameObject portalButton; // Canvas içindeki buton

    private bool isPlayerNearby = false;

    private void Start()
    {
        // Başlangıçta portal butonlarını gizle
        if (portalButton != null)
        {
            portalButton.SetActive(false);
            Debug.Log($"Portal {portalID}: Buton başlangıçta gizlendi.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Portal {portalID}: Oyuncu yaklaştı.");
            if (!isActivated)
            {
                isActivated = true;
                Debug.Log($"Portal {portalID}: İlk kez aktifleştirildi.");
                TeleportManager.Instance.PortalAktifEt(portalID);
            }
            isPlayerNearby = true; // Oyuncu bu portalın yakınında
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Portal {portalID}: Oyuncu uzaklaştı.");
            isPlayerNearby = false; // Oyuncu artık uzak
        }
    }

    public void ShowPortalButton(bool show)
    {
        if (portalButton != null)
        {
            portalButton.SetActive(show && isActivated);
            Debug.Log($"Portal {portalID}: Buton durumu güncellendi. Aktif mi: {isActivated}, Görünür mü: {show}");
        }
    }

    public Vector3 GetSpawnPosition()
    {
        return spawnPoint ? spawnPoint.position : transform.position;
    }

    public bool IsPlayerNearby()
    {
        Debug.Log($"Portal {portalID}: Oyuncu yakın mı? {isPlayerNearby}");
        return isPlayerNearby;
    }

    public string PortalID => portalID;
    public bool IsActivated { get => isActivated; set => isActivated = value; }
}
