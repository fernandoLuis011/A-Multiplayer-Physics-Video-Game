using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObject : MonoBehaviour
{
    protected List<GameObject> players;                    // List of all players in the scene
    protected List<GameObject> playersCollidingWithObject; // List of players that are currently colliding with the object

    protected Collider2D objectCollider;        // Collider for the object to handle collisions
    protected GameObject throwingPlayer = null; // The player that is currently throwing the object

    public int damageAmount = 10;               // The damage the object deals when colliding
    public float velocityThreshold = 0.5f;      // Velocity threshold to consider the object "moving"
    public float damageMultiplier = 1f;         // Multiplier to scale the damage (Strength consumable)

    // States to track the object's movement
    protected bool isMoving = false;       // Whether the object is currently moving
    protected bool wasMoving = false;      // Whether the object was moving in the previous frame
    protected bool isThrown = false;       // Whether the object has been thrown

    // Called when the object is first created or enabled
    protected virtual void Start()
    {
        // Initialize the list of players and the list of players colliding with the object
        UpdatePlayers();
        
        // Get the object's collider component for collision handling
        objectCollider = GetComponent<Collider2D>();
        
        // Initialize the list of players that are colliding with the object
        playersCollidingWithObject = new List<GameObject>();

        // Disable collisions between the object and the players initially
        TogglePlayerCollisions(true);
    }

    // Called every frame to check the object's state and update accordingly
    protected virtual void Update()
    {
        // If there is no throwing player, skip this update
        if (throwingPlayer == null) return;

        // Get the Rigidbody2D component of the object to track its velocity
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Get the current velocity of the object
            var velocity = rb.velocity;

            // Check if the object is moving
            isMoving = velocity.sqrMagnitude > 0.01f;

            // If the object's movement state has changed (moving or not), handle the state transition
            if (isMoving != wasMoving)
            {
                if (isMoving)
                {
                    // If the object starts moving, disable collisions between the object and the throwing player
                    TogglePlayerCollisionsWithSpecificPlayer(throwingPlayer, true);
                }
                else
                {
                    // If the object stops moving, Disable collisions with all players
                    TogglePlayerCollisions(true);
                    isThrown = false;  // Mark the object as not thrown if it stops moving
                }
                wasMoving = isMoving;  // Update the previous movement state
            }
        }
    }

    // Method to set the player who threw the object
    public void SetThrowingPlayer(GameObject player)
    {
        throwingPlayer = player;
    }

    // Method to handle the pickup action by a player
    public void Pickup(GameObject player)
    {
        throwingPlayer = player;
        TogglePlayerCollisions(true);  // Disable collisions when the player picks up the object
    }

    // Method to handle the drop action (when the player drops the object)
    public virtual void Drop()
    {
        throwingPlayer = null;
        isThrown = true;
        TogglePlayerCollisions(false);  // Enable collisions when the object is dropped
    }

    // Method to toggle collision handling for all players in the scene
    public void TogglePlayerCollisions(bool ignore)
    {
        foreach (GameObject character in players)
        {
            Collider2D characterCollider = character.GetComponent<Collider2D>();
            if (characterCollider != null)
            {
                // Enable or disable collision between the object and each player
                Physics2D.IgnoreCollision(objectCollider, characterCollider, ignore);
            }
        }
    }

    // Method to toggle collision handling for a specific player
    public void TogglePlayerCollisionsWithSpecificPlayer(GameObject player, bool ignore)
    {
        if (player != null)
        {
            Collider2D characterCollider = player.GetComponent<Collider2D>();
            if (characterCollider != null)
            {
                // Enable or disable collision between the object and a specific player
                Physics2D.IgnoreCollision(objectCollider, characterCollider, ignore);
            }
        }
    }

    // Called when the object enters a trigger collider
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Get the velocity of the object
        var velocity = GetComponent<Rigidbody2D>().velocity;

        // Check if the object is thrown, and colliding with a player, and is still moving
        if (isThrown && collision.CompareTag("Player") && velocity.sqrMagnitude > velocityThreshold * velocityThreshold)
        {
            // If the player is not the one who threw the object and hasn't already collided with the object
            if (collision.gameObject != throwingPlayer && !playersCollidingWithObject.Contains(collision.gameObject))
            {
                // Add the player to the list of players colliding with the object
                playersCollidingWithObject.Add(collision.gameObject);

                // Apply damage to the player based on the velocity of the object
                ApplyDamageToPlayer(collision, velocity);
            }
        }
    }

    // Called when the object exits a trigger collider
    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        // Remove the player from the list when they stop colliding with the object
        if (collision.CompareTag("Player"))
        {
            playersCollidingWithObject.Remove(collision.gameObject);
        }
    }

    // Method to update the list of players in the scene
    public void UpdatePlayers()
    {
        // Find all game objects tagged as "Player" and store them in the list
        players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
    }

    // Helper method to apply damage to a player based on the object's velocity
    protected void ApplyDamageToPlayer(Collider2D playerCollider, Vector2 velocity)
    {
        // Get the player's health bar component
        HealthBar playerHealth = playerCollider.GetComponent<HealthBar>();
        if (playerHealth != null)
        {
            // Scale the damage based on the object's velocity and a multipler (Strength consumable)
            int scaledDamage = Mathf.FloorToInt(damageAmount * (velocity.magnitude / 3) * damageMultiplier);
            
            Debug.Log("Default Damage: " + damageAmount + " | Velocity: " + velocity.magnitude + " | Scaled Damage: " + scaledDamage);
            
            // Apply the damage to the player's health
            playerHealth.TakenDamage(scaledDamage);
        }
    }
}