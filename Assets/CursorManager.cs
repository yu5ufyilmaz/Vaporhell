using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance; // Singleton yapı

    [Header("Cursor Settings")]
    public Image cursorImage; // UI Image referansı
    public Color defaultColor = Color.white; // Varsayılan renk
    public Color activeColor = Color.blue; // Aktif renk (örneğin teleport için)

    private RectTransform cursorRectTransform; // Cursor'un RectTransform referansı
    private Canvas canvas; // Cursor'un bağlı olduğu Canvas
    private bool isCursorActive = false; // Cursor aktif mi?

    private void Awake()
    {
        // Singleton kontrolü
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (cursorImage != null)
        {
            cursorRectTransform = cursorImage.GetComponent<RectTransform>();
            canvas = cursorImage.GetComponentInParent<Canvas>();

            if (cursorRectTransform == null || canvas == null)
            {
                Debug.LogError("Cursor Image için gerekli RectTransform veya Canvas eksik!");
            }

            // Cursor başlangıçta gizli
            HideCursor();
        }
        else
        {
            Debug.LogError("Cursor Image atanmadı!");
        }
    }

    private void Update()
    {
        if (!isCursorActive || cursorRectTransform == null || canvas == null) return;

        // Mouse pozisyonunu UI alanına dönüştür
        Vector2 screenPosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPosition
        );

        cursorRectTransform.localPosition = localPosition;
    }

    /// <summary>
    /// Cursor'u aktif hale getirir.
    /// </summary>
    public void ShowCursor()
    {
        if (cursorImage != null)
        {
            isCursorActive = true;
            cursorImage.enabled = true;
        }
    }

    /// <summary>
    /// Cursor'u gizler.
    /// </summary>
    public void HideCursor()
    {
        if (cursorImage != null)
        {
            isCursorActive = false;
            cursorImage.enabled = false;
        }
    }

    /// <summary>
    /// Cursor'un rengini değiştirir.
    /// </summary>
    /// <param name="color">Yeni renk</param>
    public void SetCursorColor(Color color)
    {
        if (cursorImage != null)
        {
            cursorImage.color = color;
        }
    }
}
