using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Base Health")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    private bool isAlerted = false;
    private FinalBossController finalBossController;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public void SetFinalBossController(FinalBossController controller)
    {
        finalBossController = controller;
        Debug.Log($"EnemyBase: FinalBossController ayarlandı. {gameObject.name}");
    }

    public virtual void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} hasar aldı: {damageAmount}, güncel sağlık: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Alert();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} öldü!");
        
        // FinalBossController'a bildir
        if (finalBossController != null)
        {
            Debug.Log($"{gameObject.name} öldü ve FinalBossController bilgilendiriliyor.");
            finalBossController.EnemyKilled();
        }

        Destroy(gameObject, 0.1f);
    }

    protected virtual void Alert()
    {
        if (!isAlerted)
        {
            isAlerted = true;
            Debug.Log($"{gameObject.name} alarma geçti!");
        }
    }
}