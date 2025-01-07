using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Base Health")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    private bool isAlerted = false;

    protected virtual void Start()
    {
        // Tüm düşmanlar, Start'ta kendi maxHealth değerine göre full can başlasın
        currentHealth = maxHealth;
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

