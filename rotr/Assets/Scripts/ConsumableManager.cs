using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConsumableManager : NetworkBehaviour
{
    // Enum of different types of consumables
    public enum ConsumableType { Strength, Overshield, Jump, Speed, Heart }
    public ConsumableType consumableType;    // The type of consumable this object represents

    public float boostMultiplier = 1.5f;     // Multiplier for power-ups (e.g., strength, speed)
    public float boostDuration = 10f;        // Duration of the consumable effect

    private HealthBar healthBar;             // Reference to the player's HealthBar component
    private Movement playerMovement;         // Reference to the player's Movement component
    private HeartCount heartCount;           // Reference to the player's HeartCount component
    private ConsumableTimer consumableTimer; // Reference to the ConsumableTimer component to handle timer logic
    private ItemLifetime itemLifetime;       // Reference to the ItemLifetime component to handle item destruction

    [SerializeField] private AudioClip consumableSound; // The sound of a consumable

    // Trigger when the player collides with the consumable item
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!NetworkManager.Singleton.IsServer) return; //Only the server should handle pickups and apply effects:

        if (collision.CompareTag("Player"))  // Check if the object colliding is the player
        {
            // Disable the visual sprite of the consumable item
            gameObject.GetComponent<SpriteRenderer>().enabled = false;

            // Disable all colliders on this consumable to prevent further triggering
            foreach (Collider2D col in gameObject.GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            // We get the player's NetworkObject script
            NetworkObject playerNetworkObject = collision.GetComponent<NetworkObject>();
            if (playerNetworkObject == null) return;

            // 1. Notify the server to apply the effect on the specific player
            ApplyEffectToPlayerServerRpc(playerNetworkObject.OwnerClientId);

            /*
            // Disable the visual sprite of the consumable item
            gameObject.GetComponent<SpriteRenderer>().enabled = false;

            // Disable all colliders on this consumable to prevent further triggering
            foreach (Collider2D col in gameObject.GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            // Retrieve the necessary player components
            playerMovement = collision.GetComponent<Movement>();
            healthBar = collision.GetComponent<HealthBar>();
            heartCount = collision.GetComponent<HeartCount>();
            consumableTimer = GetComponent<ConsumableTimer>();
            itemLifetime = GetComponent<ItemLifetime>();

            // Check if all required components are available
            if (playerMovement != null && healthBar != null && heartCount != null)
            {
                // Notify the ItemLifetime script that the item has been picked up
                if (itemLifetime != null)
                {
                    itemLifetime.OnPickedUp();
                }

                // Apply the effect based on the consumable type
                HandleConsumableEffect();
                
                // Start the coroutine to destroy the consumable item after the effect duration
                StartCoroutine(DestroyConsumableAfterEffect());
            }
            else
            {
                Debug.LogError("Required player components missing!");  // Log an error if necessary components are missing
            }
            */
        }
    }

    // Handle the effect based on the consumable type
    private void HandleConsumableEffect()
    {
        // Apply the appropriate effect based on the consumable type
        switch (consumableType)
        {
            case ConsumableType.Strength:
                ApplyStrengthBoost();
                break;
            case ConsumableType.Overshield:
                ApplyOvershield();
                break;
            case ConsumableType.Jump:
                ApplyJumpBoost();
                break;
            case ConsumableType.Speed:
                ApplySpeedBoost();
                break;
            case ConsumableType.Heart:
                ApplyHeartBoost();
                break;
            default:
                Debug.LogError("Unknown consumable type!");  // Log an error if the consumable type is unknown
                break;
        }
    }

    // Apply a strength boost to the player (currently not implemented)
    private void ApplyStrengthBoost()
    {
        if (playerMovement != null)
        {
            // Start the consumable timer for the strength boost
            consumableTimer.TimerOnConsumable(true, boostDuration, boostDuration, playerMovement.WASD);
            
            // Play the strength boost sound 
            SoundManager.instance.PlaySound(consumableSound);

            //TODO: Implement strength boost logic
            Debug.Log("Strength Boosted over the network!");

            // Start a coroutine to reset the strength boost after the duration
            StartCoroutine(ResetStrengthAfterDuration());
        }
    }

    // Coroutine to reset strength after the boost duration ends (currently not implemented)
    private IEnumerator ResetStrengthAfterDuration()
    {
        yield return new WaitForSeconds(boostDuration);  // Wait for the boost duration to finish
        //TODO: Reset strength to original value
        Debug.Log("Strength Boost Ended over the network!");
    }

    // Apply an overshield to the player, which temporarily shields the players health
    private void ApplyOvershield()
    {
        if (healthBar != null)
        {
            // Start the consumable timer for the overshield
            consumableTimer.TimerOnConsumable(true, boostDuration, boostDuration, playerMovement.WASD);

            // Grant the player an overshield
            healthBar.GainShield();

            // Play the overshield sound once its granted
            SoundManager.instance.PlaySound(consumableSound);


            // Start a coroutine to remove the overshield after the duration
            StartCoroutine(RemoveOvershieldAfterDuration());
        }
    }

    // Coroutine to remove the overshield after the boost duration ends
    private IEnumerator RemoveOvershieldAfterDuration()
    {
        yield return new WaitForSeconds(boostDuration);  // Wait for the boost duration to finish
        healthBar.LoseShield();  // Remove the overshield from the player
    }

    // Apply a jump boost to the player, increasing jumping power
    private void ApplyJumpBoost()
    {
        if (playerMovement != null)
        {
            // Start the consumable timer for the jump boost
            consumableTimer.TimerOnConsumable(true, boostDuration, boostDuration, playerMovement.WASD);

            // Multiply the player's jump power by the boost multiplier
            playerMovement.jumpingPower *= boostMultiplier;
            Debug.Log("Jump Boosted over the network!");
            SoundManager.instance.PlaySound(consumableSound);

            // Start a coroutine to reset the jump power after the duration
            StartCoroutine(ResetJumpPowerAfterDuration());
        }
    }

    // Coroutine to reset jump power to its original value after the boost duration ends
    private IEnumerator ResetJumpPowerAfterDuration()
    {
        yield return new WaitForSeconds(boostDuration);  // Wait for the boost duration to finish
        playerMovement.jumpingPower /= boostMultiplier;  // Restore the original jump power
        Debug.Log("Jump Boost Ended over the network!");
    }

    // Apply a speed boost to the player, increasing movement speed
    private void ApplySpeedBoost()
    {
        if (playerMovement != null)
        {
            // Start the consumable timer for the speed boost
            consumableTimer.TimerOnConsumable(true, boostDuration, boostDuration, playerMovement.WASD);

            // Play the speed boost sound
            SoundManager.instance.PlaySound(consumableSound);


            // Multiply the player's speed by the boost multiplier
            playerMovement.speed *= boostMultiplier;
            Debug.Log("Speed Boosted over the network!");

            // Start a coroutine to reset the speed after the duration
            StartCoroutine(ResetSpeedAfterDuration());
        }
    }

    // Coroutine to reset speed to its original value after the boost duration ends
    private IEnumerator ResetSpeedAfterDuration()
    {
        yield return new WaitForSeconds(boostDuration);  // Wait for the boost duration to finish
        playerMovement.speed /= boostMultiplier;  // Restore the original speed
        Debug.Log("Speed Boost Ended over the network!");
    }

    // Apply a heart boost, which adds an additional heart (life) to the player
    private void ApplyHeartBoost()
    {
        if (heartCount != null && heartCount.livesRemaining < 3)
        {
            // Add a heart if the player has fewer than 3 hearts
            heartCount.AddHeart();

            // Play heart boost sound after being added to player
            SoundManager.instance.PlaySound(consumableSound);

            Debug.Log("Heart Added over the network!");
        }
        else
        {
            // Output message if the player already has the max number of hearts
            Debug.Log("Player already has full hearts!");
        }
    }

    // Coroutine to destroy the consumable item after the boost effect ends
    private IEnumerator DestroyConsumableAfterEffect()
    {
        yield return new WaitForSeconds(boostDuration);  // Wait for the boost duration to finish
        //Destroy(gameObject);  // Destroy the consumable item after the effect has ended
        if (NetworkManager.Singleton.IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }

    }

    // Let any client call this method , even if they don’t own the object this script is on.
    // Without this if a client tries to trigger the consumable pickup and call the RPC, 
    // Unity would throw an error
    [ServerRpc(RequireOwnership = false)]
    private void ApplyEffectToPlayerServerRpc(ulong playerId)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(playerId))
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.gameObject;

            // Now safely fetch components from that player
            playerMovement = player.GetComponent<Movement>();
            healthBar = player.GetComponent<HealthBar>();
            heartCount = player.GetComponent<HeartCount>();
            consumableTimer = GetComponent<ConsumableTimer>();
            itemLifetime = GetComponent<ItemLifetime>();

            if (playerMovement != null && healthBar != null && heartCount != null)
            {

                if (itemLifetime != null)
                    itemLifetime.OnPickedUp();
                    

                // Apply the appropriate effect
                HandleConsumableEffect();
                StartCoroutine(DestroyConsumableAfterEffect());

            }
            else
            {
                Debug.LogWarning("Missing one or more player components when applying consumable.");
            }
        }
    }

}