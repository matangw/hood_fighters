using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float maxLife = 100f;
    [SerializeField] private float currentLife;
    [SerializeField] private int teamNumber;

    private void Start()
    {
        currentLife = maxLife;
    }

    public void TakeDamage(float damage)
    {
        currentLife -= damage;
        if (currentLife <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentLife = Mathf.Min(currentLife + amount, maxLife);
    }

    private void Die()
    {
        // Handle player death here
        Debug.Log("Player died!");
    }

    public float GetCurrentLife()
    {
        return currentLife;
    }

    public int GetTeamNumber()
    {
        return teamNumber;
    }

    public void SetTeamNumber(int team)
    {
        teamNumber = team;
    }
}
