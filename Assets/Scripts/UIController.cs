using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject minimapSmallUI;
    public GameObject minimapLargeUI;
    public GameObject pauseMenuUI;
    public GameObject optionsPanel;

    private bool isPaused = false;
    private bool isMinimapLarge = false;
    private InputActions controls;
    private static UIController instance;
    private bool inputLocked = false; // ESC tuşu kilidi için değişken
    private float inputLockCooldown = 0.1f; // ESC kilidinin devreye girmesi için gecikme süresi
    private float inputLockCooldownTimer = 0f; // Kilit zamanlayıcısı

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            controls = new InputActions();
            controls.UI.MinimapToggle.performed += ctx => ToggleMinimap();
            controls.UI.Cancel.performed += ctx => TryHandleUIControl(); // ESC kontrolü
            controls.UI.PauseToggle.performed += ctx => TryHandleUIControl(); // Gamepad kontrolü

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Kilitliyse zamanlayıcıyı güncelle
        if (inputLocked)
        {
            inputLockCooldownTimer -= Time.unscaledDeltaTime;
            if (inputLockCooldownTimer <= 0f)
            {
                inputLocked = false; // Kilidi aç
            }
        }
    }

    private void OnEnable()
    {
        controls.UI.Enable();
    }

    private void OnDisable()
    {
        controls.UI.Disable();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
        }
    }

    private void TryHandleUIControl()
    {
        if (inputLocked) return; // ESC tuşu kilitliyse işlem yapma
        inputLocked = true;
        inputLockCooldownTimer = inputLockCooldown; // Kilit süresini başlat

        HandleUIControl(); // Kontrol işlemini yap
    }

    public void ToggleMinimap()
    {
        if (isPaused) return;

        isMinimapLarge = !isMinimapLarge;
        minimapSmallUI.SetActive(!isMinimapLarge);
        minimapLargeUI.SetActive(isMinimapLarge);

        if (isMinimapLarge)
        {
            Time.timeScale = 0;
        }
        else
        {
            if (!isPaused)
            {
                Time.timeScale = 1;
            }
        }

        Debug.Log("Minimap durumu: " + (isMinimapLarge ? "Büyük" : "Küçük"));
    }

    private void HandleUIControl()
    {
        if (isMinimapLarge)
        {
            // Büyük minimap açık ise kapatalım
            ToggleMinimap();
            Debug.Log("Minimap kapandı");
        }
        else if (isPaused)
        {
            // Pause menüsü açık ise kapat
            ResumeGame();
            Debug.Log("Resume game");
        }
        else
        {
            // Pause menüsü kapalı ise aç
            PauseGame();
            Debug.Log("Pause game");
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        controls.Player.Disable();
        controls.UI.Enable();
        pauseMenuUI.SetActive(true);
    }

    private void ResumeGame()
    {
        isPaused = false;

        // Minimap büyük değilse oyunu devam ettir
        Time.timeScale = isMinimapLarge ? 0 : 1;

        controls.Player.Enable();
        controls.UI.Enable();
        pauseMenuUI.SetActive(false);
        optionsPanel.SetActive(false);
    }

    // Pause Menüsü Buton İşlevleri
    public void OnResumeButtonPressed()
    {
        ResumeGame();
    }

    public void OnRestartButtonPressed()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnOptionsButtonPressed()
    {
        optionsPanel.SetActive(true);
    }

    public void OnMainMenuButtonPressed()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    // Options Panelinde Ses Ayarı Değişimi
    public void OnVolumeChange(float volume)
    {
        AudioListener.volume = volume;
    }

    public void OnGraphicsChange(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void OnOptionsBackButtonPressed()
    {
        optionsPanel.SetActive(false);
    }
}
