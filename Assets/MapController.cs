using UnityEngine;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour
{
    [Header("Harita UI Paneli")]
    [SerializeField] private GameObject mapUI; // Harita UI paneli veya Canvas

    [Header("Input Actions")]
    [SerializeField] private InputActionReference openMapAction; // Yeni Input System'deki "OpenMap" Action

    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput; // Action Map değiştirmek için

    private bool isMapOpen = false;

    private void OnEnable()
    {
        openMapAction.action.Enable();
        openMapAction.action.performed += OnOpenMapPerformed;
        Debug.Log("MapController: Input sistemi aktif.");
    }

    private void OnDisable()
    {
        openMapAction.action.performed -= OnOpenMapPerformed;
        openMapAction.action.Disable();
        Debug.Log("MapController: Input sistemi devre dışı.");
    }

    private void Start()
    {
        // Harita UI başlangıçta kapalı
        if (mapUI != null)
            mapUI.SetActive(false);

        // Zaman normal akışta
        Time.timeScale = 1f;

        // Varsayılan Action Map "Player" olmalı
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");

        Debug.Log("MapController: Başlangıç ayarları tamamlandı.");
    }

    private void OnOpenMapPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("MapController: R tuşuna basıldı.");

        // Sadece oyuncu bir teleporter'a yakınsa harita açılabilsin
        if (!TeleportManager.Instance.IsPlayerNearAnyPortal())
        {
            Debug.LogWarning("MapController: Oyuncu teleporter yakınında değil, harita açılamaz!");
            return;
        }

        // Harita açık mı? Kapalıysa aç, açıksa kapat
        ToggleMap();
    }

    private void ToggleMap()
    {
        isMapOpen = !isMapOpen; // Harita durumunu değiştir

        if (mapUI != null)
            mapUI.SetActive(isMapOpen);

        if (isMapOpen)
        {
            // Harita açıldı: Zaman durdurulur, Action Map "UI" yapılır
            Time.timeScale = 0f;
            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("UI");

            TeleportManager.Instance.ShowAllPortalButtons(true);
            Debug.Log("MapController: Harita açıldı. Zaman durduruldu.");
        }
        else
        {
            // Harita kapandı: Zaman devam eder, Action Map "Player" yapılır
            Time.timeScale = 1f;
            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Player");

            TeleportManager.Instance.ShowAllPortalButtons(false);
            Debug.Log("MapController: Harita kapandı. Zaman devam ediyor.");
        }
    }
}
