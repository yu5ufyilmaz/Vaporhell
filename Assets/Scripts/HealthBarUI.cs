using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Health Bar UI Elements")]
    [SerializeField] private Image healthFillImage; // Sağlık çubuğu doluluk kısmı (Fill Amount)
    [SerializeField] private float updateSpeed = 0.2f; // Güncelleme hızı

    private Coroutine currentHealthRoutine;

    /// <summary>
    /// Sağlık çubuğunu günceller.
    /// </summary>
    /// <param name="currentHealth">Mevcut sağlık değeri</param>
    /// <param name="maxHealth">Maksimum sağlık değeri</param>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFillImage == null) return;

        float targetFill = Mathf.Clamp(currentHealth / maxHealth, 0, 1);

        // Önceki coroutine varsa durdur
        if (currentHealthRoutine != null)
        {
            StopCoroutine(currentHealthRoutine);
        }

        // Fill Amount değerini animasyonla güncelle
        currentHealthRoutine = StartCoroutine(SmoothUpdate(targetFill));
    }

    private IEnumerator SmoothUpdate(float targetFill)
    {
        float currentFill = healthFillImage.fillAmount;
        float elapsed = 0f;

        while (elapsed < updateSpeed)
        {
            elapsed += Time.deltaTime;
            healthFillImage.fillAmount = Mathf.Lerp(currentFill, targetFill, elapsed / updateSpeed);
            yield return null;
        }

        healthFillImage.fillAmount = targetFill;
    }
}