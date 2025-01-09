using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    public string nextSceneName; // Geçilecek sahne adı
    public InputActionReference interactionInput; // Interaction Input Action

    private bool isPlayerInRange = false;

    private void Start()
    {
        // Interaction Input'u etkinleştir
        interactionInput.action.Enable();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Oyuncu kapıya yaklaşırsa
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player kapıya yaklaştı.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Oyuncu kapıdan uzaklaşırsa
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player kapıdan uzaklaştı.");
        }
    }

    private void Update()
    {
        // Oyuncu menzildeyse ve Interaction tuşuna basılmışsa
        if (isPlayerInRange && interactionInput.action.triggered)
        {
            Debug.Log("Kapıya etkileşim gerçekleşti. Sahne değişiyor.");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}