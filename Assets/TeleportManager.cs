using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;

    [Header("Referanslar")]
    [SerializeField] private GameObject player; // Oyuncu referansı (tag ile bulunabilir)

    private List<Portal> allPortals = new List<Portal>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("TeleportManager: Singleton oluşturuldu.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tüm portal scriptlerini bul ve listeye ekle
        allPortals = FindObjectsOfType<Portal>().ToList();
        Debug.Log($"TeleportManager: {allPortals.Count} portal bulundu.");
    }

    public void PortalAktifEt(string portalID)
    {
        Portal portal = allPortals.FirstOrDefault(p => p.PortalID == portalID);
        if (portal != null)
        {
            portal.IsActivated = true;
            Debug.Log($"TeleportManager: Portal {portalID} aktif hale getirildi.");
        }
        else
        {
            Debug.LogWarning($"TeleportManager: Portal {portalID} bulunamadı.");
        }
    }

    public void Teleport(string portalID)
    {
        Portal portal = allPortals.FirstOrDefault(p => p.PortalID == portalID && p.IsActivated);
        if (portal != null)
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                Debug.Log("TeleportManager: Player bulunup referanslandı.");
            }

            if (player != null)
            {
                player.transform.position = portal.GetSpawnPosition();
                Debug.Log($"TeleportManager: Oyuncu {portalID} portalına ışınlandı.");
            }
            else
            {
                Debug.LogError("TeleportManager: Oyuncu bulunamadı!");
            }
        }
        else
        {
            Debug.LogWarning($"TeleportManager: Portal {portalID} aktif değil veya bulunamadı.");
        }
    }

    public bool IsPlayerNearAnyPortal()
    {
        bool isNear = allPortals.Any(p => p.IsPlayerNearby());
        Debug.Log($"TeleportManager: Oyuncu herhangi bir portala yakın mı? {isNear}");
        return isNear;
    }

    public void ShowAllPortalButtons(bool show)
    {
        foreach (Portal portal in allPortals)
        {
            portal.ShowPortalButton(show);
        }
        Debug.Log($"TeleportManager: Tüm portal butonları güncellendi. Görünür mü: {show}");
    }
}
