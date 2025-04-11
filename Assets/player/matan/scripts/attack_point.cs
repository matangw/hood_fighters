using UnityEngine;

public class attack_point : MonoBehaviour
{
    [SerializeField] private float attackRadius = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("K key pressed, attack will trigger in 0.3 seconds");
            Invoke(nameof(PerformAttack), 0.3f);
        }
    }

    private void PerformAttack()
    {
        int enemyLayer = LayerMask.GetMask("enemy");        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);
        Debug.Log("Number of colliders found: " + hitColliders.Length);
        
        foreach (Collider2D collider in hitColliders)
        {
            if (isSameObject(collider)) continue;
            damageEnemy(collider);
            knockBackEnemy(collider);
        }
    }

    // Visualize the attack radius in the editor and during play
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    void damageEnemy(Collider2D collider)
    {
        hitable_object hitable = collider.GetComponent<hitable_object>();
        if (hitable != null)
        {
            hitable.ChangeHP(-10f);
        }
    }


    void knockBackEnemy(Collider2D collider)
    {
        Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = (collider.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * 10f;
        }
    }


    bool isSameObject(Collider2D collider)
    {
        return collider.gameObject == gameObject;
    }
}
