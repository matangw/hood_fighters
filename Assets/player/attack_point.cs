using UnityEngine;

public class attack_point : MonoBehaviour
{
    [SerializeField] private float attackRadius = 3f;
    private bool isHeavyPunch = false;
    private Combat combatScript;
    private Animator animator;
    private bool isOnCooldown = false;
    private float cooldownEndTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatScript = GetComponentInParent<Combat>();
        animator = GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if cooldown has ended
        if (isOnCooldown && Time.time >= cooldownEndTime)
        {
            isOnCooldown = false;
        }

        if (!isOnCooldown)
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
    }

    private void PerformPunch()
    {
        // Get the current punch based on combo counter, punch type, and air state
        Punch currentPunch;
        if (combatScript.IsGrounded())
        {
            currentPunch = isHeavyPunch ?
                PunchData.HeavyPunches[Mathf.Min(combatScript.GetComboCounter(), PunchData.HeavyPunches.Length - 1)] :
                PunchData.RegularPunches[Mathf.Min(combatScript.GetComboCounter(), PunchData.RegularPunches.Length - 1)];
        }
        else
        {
            if (isHeavyPunch)
            {
                // For air heavy attacks, use different attacks based on air_aiming
                int airAiming = animator != null ? animator.GetInteger("air_aiming") : 0;
                if (airAiming > 0)
                {
                    currentPunch = PunchData.HeavyPunches[0]; // First heavy attack
                }
                else if (airAiming < 0)
                {
                    currentPunch = PunchData.HeavyPunches[1]; // Second heavy attack
                }
                else
                {
                    currentPunch = PunchData.HeavyPunches[2]; // Last heavy attack
                }
            }
            else
            {
                // For air regular attacks, always use the first punch
                currentPunch = PunchData.RegularPunches[0];
            }
        }

        // Start cooldown based on punch duration
        isOnCooldown = true;
        cooldownEndTime = Time.time + currentPunch.duration;
        Debug.Log($"Starting cooldown for {currentPunch.duration} seconds");

        Debug.Log($"Current time: {Time.time}, Scheduling attack for {currentPunch.hitSecond} seconds from now");
        // Schedule the attack for the hitSecond timing
        Invoke(nameof(PerformAttack), currentPunch.hitSecond);
    }

    private void PerformAttack()
    {
        Debug.Log($"Attack executed at time: {Time.time}");
        int enemyLayer = LayerMask.GetMask("enemy");
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRadius, enemyLayer);
        Debug.Log("Number of colliders found: " + hitColliders.Length);

        // Get the current punch damage based on grounded state and air aiming
        float damage;
        if (combatScript.IsGrounded())
        {
            damage = isHeavyPunch ?
                PunchData.HeavyPunches[Mathf.Min(combatScript.GetComboCounter(), PunchData.HeavyPunches.Length - 1)].damage :
                PunchData.RegularPunches[Mathf.Min(combatScript.GetComboCounter(), PunchData.RegularPunches.Length - 1)].damage;
        }
        else
        {
            if (isHeavyPunch)
            {
                // For air heavy attacks, use different attacks based on air_aiming
                int airAiming = animator != null ? animator.GetInteger("air_aiming") : 0;
                if (airAiming > 0)
                {
                    damage = PunchData.HeavyPunches[0].damage; // First heavy attack
                }
                else if (airAiming < 0)
                {
                    damage = PunchData.HeavyPunches[1].damage; // Second heavy attack
                }
                else
                {
                    damage = PunchData.HeavyPunches[2].damage; // Last heavy attack
                }
            }
            else
            {
                // For air regular attacks, always use the first punch
                damage = PunchData.RegularPunches[0].damage;
            }
        }

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
            Punch currentPunch;
            if (combatScript.IsGrounded())
            {
                currentPunch = isHeavyPunch ?
                    PunchData.HeavyPunches[Mathf.Min(combatScript.GetComboCounter(), PunchData.HeavyPunches.Length - 1)] :
                    PunchData.RegularPunches[Mathf.Min(combatScript.GetComboCounter(), PunchData.RegularPunches.Length - 1)];
            }
            else
            {
                if (isHeavyPunch)
                {
                    int airAiming = animator != null ? animator.GetInteger("air_aiming") : 0;
                    if (airAiming > 0)
                    {
                        currentPunch = PunchData.HeavyPunches[0];
                    }
                    else if (airAiming < 0)
                    {
                        currentPunch = PunchData.HeavyPunches[1];
                    }
                    else
                    {
                        currentPunch = PunchData.HeavyPunches[2];
                    }
                }
                else
                {
                    currentPunch = PunchData.RegularPunches[0];
                }
            }

            float knockbackForce = isHeavyPunch ? 5f : 2f;
            Vector2 adjustedDirection = new Vector2(currentPunch.direction.x * transform.parent.localScale.x, currentPunch.direction.y);
            rb.linearVelocity = adjustedDirection * knockbackForce;
        }
    }

    bool isSameObject(Collider2D collider)
    {
        return collider.gameObject == gameObject;
    }
}
