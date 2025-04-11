using UnityEngine;

public class hitable_object : MonoBehaviour
{
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetCurrentHP()
    {
        return currentHP;
    }

    public void ChangeHP(float amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0f, maxHP);
    }
}
