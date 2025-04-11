using UnityEngine;
using System.Collections;

public class Matan_Combat : MonoBehaviour
{
    [Header("Combat Settings")]
    public float comboWindowTime = 1f; // Time window to continue a combo (in seconds)
    public int maxComboCount = 3; // Maximum number of hits in a combo

    private Animator animator;
    private int comboCounter = 0;
    private float lastPunchTime = 0f;
    private bool canAttack = true;

    // Reference to the movement script
    private Matan_Movements movementScript;

    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        movementScript = GetComponent<Matan_Movements>();

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
    
    private void GroundPunch()
    {
        // Update last punch time
        lastPunchTime = Time.time;
        
        // Set animator parameters with current combo counter (starting from 0)
        if (animator != null)
        {
            animator.SetInteger("combo_counter", comboCounter);
            animator.SetTrigger("punch");
            
            // Debug info
            Debug.Log("Ground punch triggered with combo: " + comboCounter);
        }
        
        // Increment combo counter
        comboCounter++;
        
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
            
            // Debug info
            Debug.Log("Air punch triggered");
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
} 