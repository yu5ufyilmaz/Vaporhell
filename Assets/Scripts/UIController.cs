using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject minimapSmallUI;
    public GameObject minimapLargeUI;
    public GameObject pauseMenuUI;
    public GameObject optionsPanel;
    private PlayerController playerController;

    private bool isPaused = false;
    private bool isMinimapLarge = false;
    private InputActions controls;
    private static UIController instance;
    private bool inputLocked = false;
    private float inputLockCooldown = 0.1f;
    private float inputLockCooldownTimer = 0f;
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            controls = new InputActions();
            controls.UI.MinimapToggle.performed += ctx => ToggleMinimap();
            controls.UI.Cancel.performed += ctx => TryHandleUIControl();
            controls.UI.PauseToggle.performed += ctx => TryHandleUIControl();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (inputLocked)
        {
            inputLockCooldownTimer -= Time.unscaledDeltaTime;
            if (inputLockCooldownTimer <= 0f)
            {
                inputLocked = false;
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
        if (inputLocked) return;
        inputLocked = true;
        inputLockCooldownTimer = inputLockCooldown;

        HandleUIControl();
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

            if (playerController != null)
            {
                playerController.enabled = false;
            }

            controls.Player.Disable();
            controls.UI.Enable();
        }
        else
        {
            Time.timeScale = isPaused ? 0 : 1;

            if (playerController != null)
            {
                playerController.enabled = true;
            }

            controls.Player.Enable();
            controls.UI.Enable();
        }
    }

    private void HandleUIControl()
    {
        if (isMinimapLarge)
        {
            ToggleMinimap();
        }
        else if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        if (EventSystem.current != null && pauseMenuUI != null)
        {
            GameObject resumeButton = pauseMenuUI.transform.Find("Buttons/Resume")?.gameObject;
            if (resumeButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(resumeButton);
            }
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        isPaused = true;
        Time.timeScale = 0;
        controls.Player.Disable();
        controls.UI.Enable();
        pauseMenuUI.SetActive(true);
    }

    private void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = isMinimapLarge ? 0 : 1;

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        controls.Player.Enable();
        controls.UI.Enable();
        pauseMenuUI.SetActive(false);
        optionsPanel.SetActive(false);
    }
    
}
