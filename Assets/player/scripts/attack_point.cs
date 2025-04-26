using UnityEngine;
using UnityEngine.InputSystem;

public class attack_point : MonoBehaviour
{
    [SerializeField] private float attackRadius = 3f;
    private bool isHeavyPunch = false;
    private Combat combatScript;
    private Animator animator;
    private bool isOnCooldown = false;
    private float cooldownEndTime = 0f;
    private PlayerInput playerInput;

    // Input action references
    private Player_actions playerActions;
    private bool lightPunchPressed;
    private bool heavyPunchPressed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatScript = GetComponentInParent<Combat>();
        animator = GetComponentInParent<Animator>();
        playerInput = GetComponentInParent<PlayerInput>();

        // Initialize input actions
        playerActions = new Player_actions();
        playerActions.player_actions_map.Enable();
    }

    void OnEnable()
    {
        if (playerActions != null)
        {
            playerActions.player_actions_map.Enable();
        }
    }

    void OnDisable()
    {
        if (playerActions != null)
        {
            playerActions.player_actions_map.Disable();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only process input if it's from this player's device
        if (playerInput != null && playerInput.currentControlScheme != null)
        {
            var lightPunchAction = playerActions.player_actions_map.light_punch;
            if (lightPunchAction.activeControl != null &&
                lightPunchAction.activeControl.device == playerInput.devices[0])
            {
                lightPunchPressed = lightPunchAction.triggered;
            }

            var heavyPunchAction = playerActions.player_actions_map.heavy_punch;
            if (heavyPunchAction.activeControl != null &&
                heavyPunchAction.activeControl.device == playerInput.devices[0])
            {
                heavyPunchPressed = heavyPunchAction.triggered;
            }
        }

        // Check if cooldown has ended
        if (isOnCooldown && Time.time >= cooldownEndTime)
        {
            isOnCooldown = false;
        }

        if (!isOnCooldown)
        {
            // Check for both keyboard and input action attacks
            if (Input.GetKeyDown(KeyCode.K) || lightPunchPressed)
            {
                isHeavyPunch = false;
                PerformPunch();
            }
            else if (Input.GetKeyDown(KeyCode.L) || heavyPunchPressed)
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
