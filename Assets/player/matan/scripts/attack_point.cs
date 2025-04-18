using UnityEngine;

public class attack_point : MonoBehaviour
{
    [SerializeField] private float attackRadius = 3f;
    private bool isHeavyPunch = false;
    private Matan_Combat combatScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatScript = GetComponentInParent<Matan_Combat>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            isHeavyPunch = false;
            PerformPunch();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            isHeavyPunch = true;
            PerformPunch();
        }
    }

    private void PerformPunch()
    {
        // Get the current punch based on combo counter and punch type
        Punch currentPunch = isHeavyPunch ?
            heavyPunches[Mathf.Min(combatScript.GetComboCounter(), heavyPunches.Length - 1)] :
            regularPunches[Mathf.Min(combatScript.GetComboCounter(), regularPunches.Length - 1)];

        Debug.Log($"Scheduling attack for {currentPunch.hitSecond} seconds from now");
        // Schedule the attack for the hitSecond timing
        Invoke(nameof(PerformAttack), currentPunch.hitSecond);
    }

    private void PerformAttack()
    {
        int enemyLayer = LayerMask.GetMask("enemy");
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);
        Debug.Log("Number of colliders found: " + hitColliders.Length);

        // Get the current punch damage
        float damage = isHeavyPunch ?
            heavyPunches[Mathf.Min(combatScript.GetComboCounter(), heavyPunches.Length - 1)].damage :
            regularPunches[Mathf.Min(combatScript.GetComboCounter(), regularPunches.Length - 1)].damage;

        foreach (Collider2D collider in hitColliders)
        {
            if (isSameObject(collider)) continue;
            damageEnemy(collider, damage);
            knockBackEnemy(collider);
            Debug.Log("Damage done to enemy: " + damage);
        }
    }

    // Visualize the attack radius in the editor and during play
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    void damageEnemy(Collider2D collider, float damage)
    {
        hitable_object hitable = collider.GetComponent<hitable_object>();
        if (hitable != null)
        {
            hitable.ChangeHP(-damage);
        }
    }

    void knockBackEnemy(Collider2D collider)
    {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = (collider.transform.position - transform.position).normalized;
            //rb.linearVelocity = direction * 10f;
        }
    }

    bool isSameObject(Collider2D collider)
    {
        return collider.gameObject == gameObject;
    }

    // Regular punches array
    private Punch[] regularPunches = new Punch[]
    {
        new Punch(damage: 15, duration: 0.33f, hitSecond: 0.17f),
        new Punch(damage: 15, duration: 0.43f, hitSecond: 0.17f),
        new Punch(damage: 15, duration: 0.4f, hitSecond: 0.17f)
    };

    // Heavy punches array
    private Punch[] heavyPunches = new Punch[]
    {
        new Punch(damage: 30, duration: 1.05f, hitSecond: 0.47f),
        new Punch(damage: 35, duration: 1.35f, hitSecond: 0.47f),
        new Punch(damage: 40, duration: 1.35f, hitSecond: 0.47f)
    };
}
