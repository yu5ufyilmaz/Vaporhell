using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public int health = 100;
    protected bool isAlerted = false; // Düşmanın alarm durumunda olup olmadığını kontrol eder

    // Hasar alma işlevi
    public virtual void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " hasar aldı: " + damageAmount);

        if (health <= 0)
        {
            Die();
        }
        else if (!isAlerted)
        {
            Alert(); // İlk defa hasar aldığında alarm durumuna geç
        }
    }

    // Ölüm işlevi
    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " öldü!");
        Destroy(gameObject, 1f);
    }

    // Alarm durumuna geçiş işlevi
    protected virtual void Alert()
    {
        isAlerted = true;
        Debug.Log(gameObject.name + " alarma geçti!");
        // Ekstra alarm durumları burada yönetilebilir
    }

    // Düşmanı alarm durumuna geçiren dış tetikleyici işlevi
    public void TriggerAlert()
    {
        if (!isAlerted)
        {
            Alert();
        }
    }
}