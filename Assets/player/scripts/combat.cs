using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Combat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float comboWindowTime = 1f; // Time window to continue a combo (in seconds)
    public int maxComboCount = 3; // Maximum number of hits in a combo

    private Animator animator;
    private int comboCounter = 0;
    private float lastPunchTime = 0f;
    private bool canAttack = true;
    private bool isHeavyAttacking = false;
    private bool isBlocking = false;
    private Rigidbody2D rb;
    private float originalGravityScale;
    private PlayerInput playerInput;

    // Reference to the movement script
    private Movements movementScript;

    // Input action references
    private bool lightPunchPressed;
    private bool heavyPunchPressed;
    private bool blockPressed;

    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        movementScript = GetComponent<Movements>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        originalGravityScale = rb.gravityScale;

        if (animator == null)
        {
            Debug.LogWarning("No Animator component found on player. Combat animations won't work.");
        }
    }

    void Update()
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        // Read input values from the correct device-bound actions
        var lightPunchAction = playerInput.actions["light_punch"];
        var heavyPunchAction = playerInput.actions["heavy_punch"];
        var blockAction = playerInput.actions["block"];

        lightPunchPressed = lightPunchAction.triggered;
        heavyPunchPressed = heavyPunchAction.triggered;
        blockPressed = blockAction.IsPressed();

        // Handle blocking input
        isBlocking = blockPressed;
        if (animator != null)
        {
            animator.SetBool("blocking", isBlocking);
        }

        // If blocking, disable movement and return early
        if (isBlocking)
        {
            if (movementScript != null)
            {
                movementScript.enabled = false;
            }
            return;
        }
        else if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        // Check if combo should reset due to time passed
        if (Time.time - lastPunchTime > comboWindowTime && comboCounter > 0)
        {
            ResetCombo();
        }

        // Handle punch input
        if (lightPunchPressed && canAttack)
        {
            Punch();
        }

        // Handle heavy punch input
        if (heavyPunchPressed && canAttack)
        {
            HeavyPunch();
        }

        // Handle air aiming
        if (movementScript != null && !movementScript.IsGrounded)
        {
            UpdateAirAiming();
        }
    }

    private void UpdateAirAiming()
    {
        if (animator == null) return;

        int aimingValue = 0;

        // Use input action for aiming
        var yAimingAction = playerInput.actions["y_aiming"];
        float yAimingInput = yAimingAction.ReadValue<float>();

        if (yAimingInput > 0)
        {
            aimingValue = 1; // Aiming up
        }
        else if (yAimingInput < 0)
        {
            aimingValue = -1; // Aiming down
        }

        animator.SetInteger("air_aiming", aimingValue);
    }

    private void Punch()
    {
        // Check if the player is grounded or in the air
        if (movementScript != null)
        {
            if (movementScript.IsGrounded)
            {
                // Ground attack with combo system
                GroundPunch();
            }
            else
            {
                // Air attack without combo system
                AirPunch();
            }
        }
    }

    private void HeavyPunch()
    {
        if (movementScript != null)
        {
            if (movementScript.IsGrounded)
            {
                // Ground heavy attack
                isHeavyAttacking = true;
                movementScript.enabled = false;
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    rb.linearVelocity = Vector2.zero;
                }

                if (animator != null)
                {
                    animator.SetTrigger("heavy_punch");
                }

                StartCoroutine(HeavyAttackCooldown());
            }
            else
            {
                // Air heavy attack
                isHeavyAttacking = true;
                if (rb != null)
                {
                    // Freeze vertical movement for hit second duration
                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 0;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                    movementScript.enabled = false;
                    StartCoroutine(VerticalFreezeCooldown(PunchData.HeavyPunches[animator != null ? animator.GetInteger("air_aiming") : 0].hitSecond));
                }

                if (animator != null)
                {
                    animator.SetTrigger("heavy_punch");
                }
                StartCoroutine(HeavyAttackCooldown());
            }
        }
    }

    private void GroundPunch()
    {
        comboCounter++;

        // Update last punch time
        lastPunchTime = Time.time;

        // Set animator parameters with current combo counter (starting from 0)
        if (animator != null)
        {
            animator.SetInteger("combo_counter", comboCounter);
            animator.SetTrigger("punch");
        }

        // Check if we've reached max combo
        if (comboCounter >= maxComboCount)
        {
            // Reset combo counter to 0
            comboCounter = 0;

            // Use longer cooldown for the final attack in the combo
            StartCoroutine(AttackCooldown(0.5f));
        }
        else
        {
            // Standard cooldown for normal attacks
            StartCoroutine(AttackCooldown(0.2f));
        }
    }

    private void AirPunch()
    {
        // Air attack doesn't use combo system
        if (animator != null)
        {
            // Use the same punch trigger, animator will choose animation based on isGrounded
            animator.SetTrigger("punch");
        }

        // Prevent attack spamming by using a small delay
        StartCoroutine(AttackCooldown(0.2f));
    }

    private IEnumerator AttackCooldown(float cooldownTime)
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldownTime);
        canAttack = true;
    }

    private IEnumerator VerticalFreezeCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            if (movementScript != null)
            {
                movementScript.enabled = true;
            }
        }
    }

    private IEnumerator HeavyAttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.5f);
        ResetCombo();
        canAttack = true;
        isHeavyAttacking = false;

        if (movementScript != null && rb != null && rb.gravityScale != 0)
        {
            movementScript.enabled = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void ResetCombo()
    {
        comboCounter = 0;
        if (animator != null)
        {
            animator.SetInteger("combo_counter", comboCounter);
        }
    }

    // Public method to allow other scripts to reset the combo if needed
    public void ForceResetCombo()
    {
        ResetCombo();
    }

    public int GetComboCounter()
    {
        return comboCounter;
    }

    public bool IsGrounded()
    {
        return movementScript != null && movementScript.IsGrounded;
    }
}