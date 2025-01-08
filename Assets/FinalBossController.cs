using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private int currentWaveIndex = 0;
    private int currentHealth;
    private bool isFinalWave = false;

    void Start()
    {
        currentHealth = waves.Count; // Dalga sayısına göre boss canı
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        while (currentWaveIndex < waves.Count)
        {
            Wave currentWave = waves[currentWaveIndex];
            Debug.Log($"Wave {currentWaveIndex + 1} başlıyor!");

            foreach (var enemyData in currentWave.enemiesToSpawn)
            {
                SpawnEnemies(enemyData);
            }

            // Dalga sonu bekleme
            yield return new WaitForSeconds(waveCooldown);

            // Dalga tamamlanınca can azalt
            currentHealth--;

            // Eğer son dalgadaysa final durumu başlat
            if (currentWaveIndex == waves.Count - 1)
            {
                isFinalWave = true;
                break;
            }

            currentWaveIndex++;
        }
    }

    private void SpawnEnemies(EnemySpawnData enemyData)
    {
        for (int i = 0; i < enemyData.spawnCount; i++)
        {
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
            Instantiate(enemyData.enemyPrefab, spawnPoints[randomSpawnIndex].position, Quaternion.identity);
        }
    }

    void Update()
    {
        if (isFinalWave && currentHealth <= 0)
        {
            // Son vuruş yapılabilir
            if (Input.GetMouseButtonDown(0)) // Sol tık
            {
                Die();
            }
        }
    }

    private void Die()
    {
        Debug.Log("Final Boss öldü!");
        Destroy(gameObject); // Boss yok olur
    }

    public void EnemyKilled()
    {
        if (!isFinalWave)
        {
            currentHealth--;
            Debug.Log($"Boss'un canı: {currentHealth}");
        }
    }
}
