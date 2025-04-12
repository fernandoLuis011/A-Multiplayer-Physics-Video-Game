using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    public float influenceRange;                // Radius of effect for the black hole
    public float gravitationalConstant = 10f;   // Strength of the black hole's gravitational pull

    private bool canTakeDamage = true;          // Flag to prevent immediate continuous damage (controls damage delay)
    public int damagePerSec = 10;               // Amount of damage dealt per second to players in the black hole's range
    public float damageSecDelay = 1.0f;        // Delay between damage ticks (to prevent constant damage)
    public float blackHoleDuration = 6f;        // Duration for how long the black hole exists before being destroyed

    private Rigidbody2D blackHoleBody;          // Rigidbody2D component of the black hole (used for gravitational force calculations)

    void Start () {
        // Initialize the Rigidbody2D component of the black hole
        blackHoleBody = GetComponent<Rigidbody2D>();
        
        // Start the coroutine that will destroy the black hole after the specified duration
        StartCoroutine(DestroyBlackHole());
    }

    private void Update() {
        // Apply gravity to all objects within the influence range of the black hole
        foreach (Collider2D obj in Physics2D.OverlapCircleAll(transform.position, influenceRange)) {
            // If the object has a Rigidbody2D, apply gravitational force
            if (obj.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb)) {
                ApplyGravity(rb); // Apply the calculated gravity force to the object
            }
        }
    }

    // Apply gravitational force to a Rigidbody2D based on the inverse square law
    private void ApplyGravity(Rigidbody2D rb) {
        Vector2 difference = (Vector2)transform.position - rb.position;  // Direction vector from the black hole to the object
        float distance = difference.magnitude;  // Calculate the distance between the black hole and the object

        if (distance > 0) {
            // Calculate the force using the formula F = G * (m1 * m2) / r^2
            float forceMagnitude = (blackHoleBody.mass * rb.mass) / (distance * distance) * gravitationalConstant; 
            Vector2 force = difference.normalized * forceMagnitude;  // Apply force in the direction of the difference
            rb.AddForce(force);  // Apply the gravitational force to the object's Rigidbody2D
        }
    }

    // This function visually draws the black hole's influence radius in the Unity editor for debugging purposes
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, influenceRange);  // Draw the black hole's effect radius
    }

    // Trigger function that is called when a collider stays within the black hole's radius
    private void OnTriggerStay2D(Collider2D other) {
        // Only apply damage if the object is the player
        if (other.CompareTag("Player")) {
            HealthBar playerHealth = other.GetComponent<HealthBar>();  // Get the HealthBar component attached to the player

            // If the black hole is allowed to deal damage and the player has health left
            if (canTakeDamage && playerHealth.currentHealth > 0) {
                StartCoroutine(TakeDamage());  // Start the coroutine to delay damage ticks
                playerHealth.TakenDamage(damagePerSec);  // Apply the damage to the player
            }
        }
    }

    // Coroutine that manages the delay between consecutive damage ticks
    private IEnumerator TakeDamage() {
        canTakeDamage = false;  // Disable damage to player
        yield return new WaitForSeconds(damageSecDelay);  // Wait for the set delay time before allowing damage again
        canTakeDamage = true;  // Re-enable damage to player
    }

    // Coroutine that destroys the black hole after a set duration
    private IEnumerator DestroyBlackHole() {
        yield return new WaitForSeconds(blackHoleDuration);  // Wait the black hole's duration
        Destroy(gameObject);  // Destroy the black hole 
    }
}