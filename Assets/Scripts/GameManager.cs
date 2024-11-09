using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public GameObject optionsPanel;      
    public GameObject creditsPanel;     
    public GameObject controlsPanel;      
    public Slider volumeSlider;          
    public Button firstSelectedButton; // Ana menüde ilk seçilecek buton referansı
    private GameObject currentPanel;    // Açık olan UI panelini tutar

    private InputActions controls;      // Yeni Input Actions referansı

    private void Awake()
    {
        // Input Actions dosyasını başlatın
        controls = new InputActions();

        // Escape tuşunu ve Gamepad East tuşunu geri gitmek için ayarlayın
        controls.UI.Cancel.performed += ctx => GoBack();
    }

    private void Start()
    {
        // Başlangıç ayarlarını yükleme
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1.0f);

        // UI event listener ekleme
        volumeSlider.onValueChanged.AddListener(delegate { OnVolumeChange(); });

        // İlk açılışta belirli bir butona odaklanma
        SetFirstSelectedButton();
    }

    private void OnEnable()
    {
        controls.UI.Enable(); // Input Actions’ı etkinleştir
    }

    private void OnDisable()
    {
        controls.UI.Disable(); // Input Actions’ı devre dışı bırak
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level1"); // İlk bölümü başlatır
    }

    public void ToggleOptions()
    {
        OpenPanel(optionsPanel);
    }

    public void ToggleCredits()
    {
        OpenPanel(creditsPanel);
    }

    public void ToggleControls()
    {
        OpenPanel(controlsPanel);
    }

    private void OpenPanel(GameObject panel)
    {
        // Eğer başka bir panel zaten açıksa onu kapat
        if (currentPanel != null && currentPanel != panel)
        {
            currentPanel.SetActive(false);
        }

        // Yeni paneli aç ve ona odaklan
        panel.SetActive(true);
        currentPanel = panel;

        // Yeni aktif paneldeki ilk butona odaklan
        SetFirstSelectedButton();
    }

    private void GoBack()
    {
        // Eğer bir panel açıksa kapat ve ana menüye dön
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            currentPanel = null;
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject); // Ana menüdeki ilk butona odaklan
        }
        else
        {
            // Eğer açık bir panel yoksa çıkışı kontrol etmek için
            ExitGame();
        }
    }

    public void ExitGame()
    {
        Debug.Log("Game is exiting..."); // Çıkışı kontrol etmek için Debug.Log ekleyin
        Application.Quit();
    }

    public void OnVolumeChange()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        AudioListener.volume = volumeSlider.value; // Ses seviyesini uygula
    }

    private void SetFirstSelectedButton()
    {
        // Eğer bir panel açıksa o paneldeki ilk butona odaklan
        if (currentPanel != null)
        {
            Button firstButton = currentPanel.GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
            }
        }
        else
        {
            // Eğer panel yoksa ana menüdeki ilk butona odaklan
            if (firstSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
            }
        }
    }
}
