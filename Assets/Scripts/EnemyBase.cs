using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public int health = 100;
    public float runSpeedMultiplier = 2f; // Düşmanın koşma hızı için çarpan
    private bool isAlerted = false; // Düşmanın alarma geçip geçmediğini kontrol eder

    public virtual void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " hasar aldı: " + damageAmount);

        if (health <= 0)
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
        Debug.Log(gameObject.name + " öldü!");
        Destroy(gameObject, 1f);
    }

    // Düşmanı alarma geçiren metod
    protected virtual void Alert()
    {
        if (!isAlerted)
        {
            isAlerted = true;
            Debug.Log(gameObject.name + " alarma geçti ve koşmaya başladı!");
            // Düşmanın hızını artır veya başka bir tepki ver
            GetComponent<BasicMechanorot>().IncreaseSpeed(runSpeedMultiplier);
        }
    }
}