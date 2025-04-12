using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Image HealthFillBar;         // Green Health Bar Image
    public Image OvershieldBar;         // Overshield Bar Image
    
    public float Totalhealth = 100f;    // Maximum starting health
    public float ShieldHealth = 100f;   // Maximum shield health
    public float currentHealth;         // Current Health of the player
    public float currentShieldHealth;   // Current shield health
    
    public HeartCount heartCount;       // Reference to HeartCount script (handles lives)
    public RandomRespawn playerRespawn; // Reference to the Respawn script
    public Movement playerMovement;     // Reference to the Movement script (for disabling/enabling movement)
    public PickUp playerItem;           // Reference to the PickUp script (for handling item drop on death)
    private Animator anim;              // Animator component to control death and damage animations

    public bool isInvulnerable = false;         // Flag to track if the player is invulnerable (e.g. after respawn)
    private float invulnerabilityDuration = 4f; // How long the player stays invulnerable after respawning
    private float invulnerabilityTimer = 0f;    // Timer to count down the invulnerability duration

    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;

    [SerializeField] private AudioClip respawnSound;

    [SerializeField] private float deathDelaySec;



    private void Awake()
    {
        // Initialize current health and shield health
        currentHealth = Totalhealth;
        currentShieldHealth = 0f;
        
        // Get necessary references
        anim = GetComponent<Animator>();
        playerRespawn = GetComponent<RandomRespawn>();
        playerMovement = GetComponent<Movement>();
        playerItem = GetComponent<PickUp>();
    }

    private void Update()
    {
        // If the player is invulnerable, countdown the invulnerability timer
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;

            // If invulnerability duration has ended, or player starts moving, disable invulnerability
            if (invulnerabilityTimer <= 0 || playerMovement.getHorizontal() != 0)
            {
                isInvulnerable = false;  // Disable invulnerability
            }
        }
    }

    // Call this method when the player respawns
    public void OnRespawn()
    {
        // Add invulnerability flag after respawn and reset timer
        isInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;  // Reset timer

        // Reset health to maximum health
        currentHealth = Totalhealth;
        HealthFillBar.fillAmount = 1f;  // Update the health bar fill to full
    }

    // Call this method when the player gains a shield
    public void GainShield()
    {
        currentShieldHealth = ShieldHealth; // Set shield health to max value
        OvershieldBar.enabled = true; // Enable overshield bar UI
        OvershieldBar.fillAmount = 1f; // Update the overshield bar fill to full
    }

    // Call this method when the player loses their shield
    public void LoseShield()
    {
        currentShieldHealth = 0f; // Set shield health to 0
        OvershieldBar.fillAmount = 0f; // Set the overshield bar to empty
        OvershieldBar.enabled = false; // Disable overshield bar UI
    }

    // Call this method when the player takes damage
    public void TakenDamage(int damage)
    {
        // Skip damage if invulnerable or already dead
        if (isInvulnerable || currentHealth <= 0) return;

        // Check if the player has an active shield
        if (currentShieldHealth > 0)
        {
            // Trigger damage animation
            anim.SetTrigger("3_Damaged");

            // Reduce damage from the shield first
            float damageToShield = Mathf.Min(damage, currentShieldHealth);

            currentShieldHealth -= damageToShield;  // Reduce shield health

            // Play taken damage sound
            SoundManager.instance.PlaySound(damageSound);

            OvershieldBar.fillAmount = currentShieldHealth / ShieldHealth;  // Update the shield bar

            // If shield is depleted, the remaining damage will be applied to health
            damage -= (int)damageToShield;

        }
        else if (currentShieldHealth == 0)
        {
            // If shield is depleted, disable shield
            LoseShield();
        }

        // Reduce the player's health by the remaining damage
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, Totalhealth);
        HealthFillBar.fillAmount = currentHealth / Totalhealth;  // Update health bar

        // If health is still above 0, trigger damage animation
        if(currentHealth > 0)
        {
            anim.SetTrigger("3_Damaged");

            // Play taken damage sound
            SoundManager.instance.PlaySound(damageSound);

        }
        else
        {
            // If health is 0 or less, the player is dead
            // Disable movement
            playerMovement.DisableMovement();

            // Trigger death animation
            anim.SetTrigger("4_Death");

            SoundManager.instance.PlaySound(deathSound);

            // Drop item and reduce lives
            playerItem.dropItemOnDeath();
            heartCount.LoseLife();


            if (heartCount.livesRemaining >= 1)
            {
                // If there are lives remaining, respawn the player after the death animation
                StartCoroutine(deathDelay());
            }
            else
            {
                // If no lives remain, disable the player object
                gameObject.SetActive(false);
            }
        }
    }

    // Coroutine that waits for death animation to finish before respawning the player
    private IEnumerator deathDelay()
    {
        // Wait for the duration of the death animation
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length + 0.25f);

        // Respawn the player
        playerRespawn.RespawnPlayer();
        StartCoroutine(respawnDelay());
    }

    // Coroutine to delay respawn before enabling movement
    private IEnumerator respawnDelay()
    {
        // Play Respawn Sound
        SoundManager.instance.PlaySound(respawnSound);

        yield return new WaitForSeconds(deathDelaySec); // Wait for respawn to complete

        playerMovement.EnableMovement(); // Re-enable player movement after respawn

    }
}