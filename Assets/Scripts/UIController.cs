using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public GameObject minimapUI;    // Minimap UI öğesi
    public GameObject pauseMenuUI;  // Pause menüsü UI öğesi

    private bool isPaused = false;  // Oyun duraklatma durumu
    private InputActions controls;  // Input Actions dosyasına referans

    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Bu objenin sahne geçişlerinde yok olmamasını sağlar

        // Input Actions dosyasını yükleyin
        controls = new InputActions();

        // UI Action Map içindeki aksiyonları bağlayın
        controls.UI.MinimapToggle.performed += ctx => ToggleMinimap();
        controls.UI.PauseToggle.performed += ctx => TogglePause();
    }

    private void OnEnable()
    {
        // UI Action Map’i aktif hale getirin
        controls.UI.Enable();
    }

    private void OnDisable()
    {
        // UI Action Map’i devre dışı bırakın
        controls.UI.Disable();
    }

    public void ToggleMinimap()
    {
        // Minimap’in aktiflik durumunu tersine çevir
        minimapUI.SetActive(!minimapUI.activeSelf);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);

        // Oyunun duraklatılması veya devam ettirilmesi
        Time.timeScale = isPaused ? 0 : 1;
    }
}