using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Health Bar UI Elements")]
    [SerializeField] private Image foregroundImage; // Sağlık barının dolu kısmı
    [SerializeField] private float updateSpeed = 0.2f; // Sağlık azalırken animasyon hızı

    private Coroutine currentHealthRoutine;

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (foregroundImage == null) return;

        // Sağlık oranını hesapla (0 ile 1 arasında)
        float healthPercent = Mathf.Clamp(currentHealth / maxHealth, 0, 1);

        // Animasyonla güncelle
        if (currentHealthRoutine != null)
        {
            StopCoroutine(currentHealthRoutine);
        }
        currentHealthRoutine = StartCoroutine(SmoothUpdate(healthPercent));
    }

    private IEnumerator SmoothUpdate(float targetFill)
    {
        float currentFill = foregroundImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < updateSpeed)
        {
            elapsed += Time.deltaTime;
            foregroundImage.fillAmount = Mathf.Lerp(currentFill, targetFill, elapsed / updateSpeed);
            yield return null;
        }

        foregroundImage.fillAmount = targetFill;
    }
}