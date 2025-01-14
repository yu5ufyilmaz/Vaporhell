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
            Debug.Log($"Dalga {currentWaveIndex + 1} başlıyor!");

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
            Debug.Log($"Dalga {currentWaveIndex + 1} tamamlandı!");

            // Dalga bitiminde can azalt
            currentHealth--;
            UpdateHealthUI();

            // Eğer son dalgaysa final durumuna geç
            if (currentWaveIndex == waves.Count - 1)
            {
                isFinalWave = true;
                Debug.Log("Son dalga! Final Boss vurulabilir durumda.");
                yield break; // Coroutine'den çık
            }

            // Dalga indexini artır
            currentWaveIndex++;
            Debug.Log($"Sıradaki dalga: {currentWaveIndex + 1}");

            // Yeni dalga başlamadan önce bekleme süresi
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
            
                Die();
        }
    }

    private void Die()
    {
        Debug.Log("Final Boss öldü!");
        Destroy(gameObject); // Boss yok olur
        SceneManager.LoadScene("Boss");
    }
}
