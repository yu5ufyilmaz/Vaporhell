using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class TeleportMapManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject teleportMapCanvas;
    [SerializeField] private Transform teleportButtonsParent;
    [SerializeField] private Image selectionHighlight;

    [Header("Teleport Points")]
    [SerializeField] private List<TeleportPoint> teleportPoints = new List<TeleportPoint>();

    private List<Button> teleportButtons = new List<Button>();
    private int currentSelectionIndex = 0;
    private bool isMapOpen = false;

    private PlayerController playerController;

    // Input Actions
    private PlayerInput playerInput;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    void Awake()
    {
        // Bu script'in PlayerInput component'ına sahip olduğundan emin olun.
        // Eğer değilse, PlayerController üzerinden referans alabilirsiniz.
        playerInput = FindObjectOfType<PlayerController>().GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("TeleportMapManager: PlayerInput component'i bulunamadı.");
        }
        else
        {
            navigateAction = playerInput.actions["Navigate"];
            submitAction = playerInput.actions["Submit"];
            cancelAction = playerInput.actions["Cancel"];
        }
    }

    void Start()
    {
        // Find all teleport points in the scene
        teleportPoints.AddRange(FindObjectsOfType<TeleportPoint>());

        // Initialize teleport buttons based on teleport points
        foreach (var teleportPoint in teleportPoints)
        {
            // Dinamik olarak buton oluşturmak için bir prefab kullanabilirsiniz.
            // Burada varsayılan olarak teleportButtonsParent'ın çocukları kullanılıyor.
            if (teleportButtonsParent.childCount > teleportButtons.Count)
            {
                Button btn = teleportButtonsParent.GetChild(teleportButtons.Count).GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => TeleportToPoint(teleportPoint));
                    teleportButtons.Add(btn);

                    // Butonun etiketini ayarla
                    Text btnText = btn.GetComponentInChildren<Text>();
                    if (btnText != null)
                    {
                        btnText.text = teleportPoint.TeleportID;
                    }
                }
                else
                {
                    Debug.LogWarning("TeleportMapManager: Buton bulunamadı.");
                }
            }
            else
            {
                Debug.LogWarning("TeleportMapManager: teleportButtonsParent'da yeterli buton yok.");
            }
        }

        // Initially hide the teleport map
        teleportMapCanvas.SetActive(false);

        // Find PlayerController
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("TeleportMapManager: PlayerController bulunamadı.");
        }
    }

    void OnEnable()
    {
        if (navigateAction != null)
            navigateAction.performed += OnNavigate;
        if (submitAction != null)
            submitAction.performed += OnSubmit;
        if (cancelAction != null)
            cancelAction.performed += OnCancel;
    }

    void OnDisable()
    {
        if (navigateAction != null)
            navigateAction.performed -= OnNavigate;
        if (submitAction != null)
            submitAction.performed -= OnSubmit;
        if (cancelAction != null)
            cancelAction.performed -= OnCancel;
    }

    public void OpenTeleportMap()
    {
        if (isMapOpen) return;

        isMapOpen = true;
        teleportMapCanvas.SetActive(true);
        Time.timeScale = 0f; // Pause the game

        // Select the first button by default
        if (teleportButtons.Count > 0)
        {
            currentSelectionIndex = 0;
            UpdateSelection();
            EventSystem.current.SetSelectedGameObject(teleportButtons[currentSelectionIndex].gameObject);
        }
    }

    public void CloseTeleportMap()
    {
        if (!isMapOpen) return;

        isMapOpen = false;
        teleportMapCanvas.SetActive(false);
        Time.timeScale = 1f; // Resume the game
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (!isMapOpen) return;

        Vector2 input = context.ReadValue<Vector2>();

        if (input.x > 0.5f)
        {
            MoveSelection(1);
        }
        else if (input.x < -0.5f)
        {
            MoveSelection(-1);
        }

        if (input.y > 0.5f)
        {
            MoveSelection(-1);
        }
        else if (input.y < -0.5f)
        {
            MoveSelection(1);
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!isMapOpen) return;

        if (teleportButtons.Count > 0)
        {
            teleportButtons[currentSelectionIndex].onClick.Invoke();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (!isMapOpen) return;

        CloseTeleportMap();
    }

    private void MoveSelection(int direction)
    {
        if (teleportButtons.Count == 0) return;

        currentSelectionIndex += direction;
        if (currentSelectionIndex < 0) currentSelectionIndex = teleportButtons.Count - 1;
        if (currentSelectionIndex >= teleportButtons.Count) currentSelectionIndex = 0;

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (teleportButtons.Count == 0) return;

        // Move the selection highlight to the selected button
        RectTransform selectedButtonRect = teleportButtons[currentSelectionIndex].GetComponent<RectTransform>();
        RectTransform parentRect = teleportButtonsParent.GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        selectedButtonRect.GetWorldCorners(corners);
        Vector3 buttonCenter = (corners[0] + corners[2]) / 2f;

        // Convert to local position relative to the parent
        Vector3 localPos = parentRect.transform.InverseTransformPoint(buttonCenter);
        selectionHighlight.transform.localPosition = localPos;
    }

    private void TeleportToPoint(TeleportPoint point)
    {
        // Close the teleport map
        CloseTeleportMap();

        // Teleport the player
        if (playerController != null)
        {
            playerController.TeleportTo(point.transform.position);
        }
        else
        {
            Debug.LogError("TeleportMapManager: PlayerController reference is missing.");
        }
    }
}
