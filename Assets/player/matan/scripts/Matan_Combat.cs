using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Matan_Combat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float comboWindowTime = 1f; // Time window to continue a combo (in seconds)
    public int maxComboCount = 3; // Maximum number of hits in a combo

    private Animator animator;
    private int comboCounter = 0;
    private float lastPunchTime = 0f;
    private bool canAttack = true;
    private bool isHeavyAttacking = false;
    private Rigidbody2D rb;
    private float originalGravityScale;

    // Reference to the movement script
    private Matan_Movements movementScript;

    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        movementScript = GetComponent<Matan_Movements>();
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;

        if (animator == null)
        {
            Debug.LogWarning("No Animator component found on player. Combat animations won't work.");
        }
    }

    void Update()
    {
        // Check if combo should reset due to time passed
        if (Time.time - lastPunchTime > comboWindowTime && comboCounter > 0)
        {
            ResetCombo();
        }

        // Handle punch input
        if (Input.GetKeyDown(KeyCode.K) && canAttack)
        {
            Punch();
        }

        // Handle heavy punch input
        if (Input.GetKeyDown(KeyCode.L) && canAttack)
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

        if (Input.GetKey(KeyCode.W))
        {
            aimingValue = 1; // Aiming up
        }
        else if (Input.GetKey(KeyCode.S))
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
                    // Freeze vertical movement
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                    rb.gravityScale = 0;
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

        // Increment combo counter


        // Check if we've reached max combo
        if (comboCounter >= maxComboCount)
        {
            // Reset combo counter to 0
            comboCounter = 0;

            // Use longer cooldown for the final attack in the combo
            StartCoroutine(AttackCooldown(0.5f));
            Debug.Log("Max combo reached, resetting counter with longer cooldown");
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

    private IEnumerator HeavyAttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.5f);
        ResetCombo();
        canAttack = true;
        isHeavyAttacking = false;

        // Restore movement and gravity
        if (movementScript != null)
        {
            movementScript.enabled = true;
        }
        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }
    }

    private void ResetCombo()
    {
        comboCounter = 0;
        if (animator != null)
        {
            animator.SetInteger("combo_counter", comboCounter);
        }
        Debug.Log("Combo reset due to timeout");
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