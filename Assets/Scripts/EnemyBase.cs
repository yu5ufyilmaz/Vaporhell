using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public int health = 100;
    private bool isAlerted = false;

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
        Destroy(gameObject, 5f);
    }

    protected virtual void Alert()
    {
        if (!isAlerted)
        {
            isAlerted = true;
            Debug.Log(gameObject.name + " alarma geçti!");
        }
    }
}
