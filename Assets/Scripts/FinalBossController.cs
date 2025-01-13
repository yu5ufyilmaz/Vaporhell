using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Yeni Input System için gerekli
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Wave
{
    public List<EnemySpawnData> enemiesToSpawn; // Dalga için düşman bilgileri
}

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab; // Hangi düşman
    public int spawnCount; // Kaç tane spawnlanacak
}

public class FinalBossController : MonoBehaviour
{
    [Header("Boss Parameters")]
    [SerializeField] private List<Wave> waves; // Dalga listesi
    [SerializeField] private Transform[] spawnPoints; // Spawn noktaları
    [SerializeField] private float waveCooldown = 5f; // Dalga arası bekleme süresi
    [SerializeField] private int finalWaveHealth = 1; // Son dalga canı

    [Header("UI Parameters")]
    [SerializeField] private Slider healthBar; // Can göstergesi için Slider

    [Header("Input System")]
    [SerializeField] private InputActionReference attackAction; // Attack eylemi için referans

    private int currentWaveIndex = 0;
    private int currentHealth;
    private bool isFinalWave = false;
    private int remainingEnemiesInWave = 0; // O anki dalgadaki kalan düşman sayısı

    void Start()
    {
        currentHealth = waves.Count; // Dalga sayısına göre boss canı
        Debug.Log($"Başlangıç: Final Boss canı: {currentHealth}, toplam dalga: {waves.Count}");

        // UI ayarları
        if (healthBar != null)
        {
            healthBar.maxValue = waves.Count;
            healthBar.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("FinalBossController: Health bar Slider atanmadı!");
        }

        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        while (currentWaveIndex < waves.Count)
        {
            Wave currentWave = waves[currentWaveIndex];
            Debug.Log($"Dalga {currentWaveIndex + 1} başlıyor! Dalga için düşman sayısı hesaplanıyor...");

            // Dalga için toplam düşman sayısını hesapla
            remainingEnemiesInWave = 0;
            foreach (var enemyData in currentWave.enemiesToSpawn)
            {
                remainingEnemiesInWave += enemyData.spawnCount;
                SpawnEnemies(enemyData);
            }
            Debug.Log($"Dalga {currentWaveIndex + 1}: Toplam {remainingEnemiesInWave} düşman spawnlandı.");

            // Dalganın bitmesini bekle
            yield return new WaitUntil(() => remainingEnemiesInWave <= 0);
            Debug.Log($"Dalga {currentWaveIndex + 1} tamamlandı! Kalan düşman: {remainingEnemiesInWave}");

            // Dalga bitiminde can azalt
            currentHealth--;
            Debug.Log($"Final Boss canı azaldı: {currentHealth}");
            UpdateHealthUI();

            // Eğer son dalgaysa final durumuna geç
            if (currentWaveIndex == waves.Count - 1)
            {
                isFinalWave = true;
                Debug.Log("Son dalga! Final Boss vurulabilir durumda.");
                break;
            }

            currentWaveIndex++;
            yield return new WaitForSeconds(waveCooldown);
        }
    }

    private void SpawnEnemies(EnemySpawnData enemyData)
    {
        for (int i = 0; i < enemyData.spawnCount; i++)
        {
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
            GameObject spawnedEnemy = Instantiate(enemyData.enemyPrefab, spawnPoints[randomSpawnIndex].position, Quaternion.identity);

            // Spawn edilen düşmana FinalBossController'ı bildir
            EnemyBase enemyBase = spawnedEnemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.SetFinalBossController(this);
            }

            Debug.Log($"Spawn edilen düşman: {enemyData.enemyPrefab.name} | Spawn noktası: {spawnPoints[randomSpawnIndex].position}");
        }
    }

    public void EnemyKilled()
    {
        remainingEnemiesInWave--; // Ölen düşmanı say
        Debug.Log($"Bir düşman öldü! Kalan düşman sayısı: {remainingEnemiesInWave}");
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
            Debug.Log($"Health bar güncellendi: {currentHealth}/{waves.Count}");
        }
    }

    void Update()
    {
        if (isFinalWave && currentHealth <= 0)
        {
            // Yeni Input System üzerinden sol tık kontrolü
            if (attackAction.action.triggered) // InputActionReference üzerinden tetikleme kontrolü
            {
                Debug.Log("Final Boss öldürülmeye hazır! Sol tık ile vurabilirsiniz.");
                Die();
            }
        }
    }

    private void Die()
    {
        Debug.Log("Final Boss öldü!");
        Destroy(gameObject); // Boss yok olur
        SceneManager.LoadScene("MainMenu");
    }
}
