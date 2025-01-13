using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    private void Start()
    {
        // Oyunun başında kursörü aktif hale getir
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        // Mevcut sahneyi kontrol et
        string currentScene = SceneManager.GetActiveScene().name;

        // Eğer ana menü sahnesindeyseniz veya pause menü aktifse kursörü göster
        if (currentScene == "MainMenu" || 
            (UIController.instance != null && UIController.instance.pauseMenuUI.activeSelf))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // Aksi halde kursörü gizle ve kilitle
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}