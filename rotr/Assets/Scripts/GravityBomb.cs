using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBomb : FallingObject
{
    public GameObject blackHolePrefab; // Reference to the Black Hole prefab to spawn when the bomb triggers
    private bool hasTriggered = false; // Flag to ensure the black hole is triggered only once

    // Update is called once per frame
    private new void Update()
    {
        // If the bomb has already triggered, spawn the black hole and destroy the gravity bomb
        if (hasTriggered)
        {
            // Spawn the black hole at the current position of the gravity bomb
            SpawnBlackHole(transform.position);

            // Destroy the gravity bomb object after triggering the black hole
            Destroy(gameObject);
        }
    }

    // Override the FallingObjects OnTriggerEnter2D method for specific collision logic
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // Only trigger the black hole effect if the bomb has been thrown and hasn't triggered yet
        if (!isThrown || hasTriggered) { return; }

        // Ignore collisions with the player who threw the bomb, so they don't trigger it
        if (collision.gameObject != throwingPlayer)
        {
            hasTriggered = true;
        }
    }

    // Use OnCollisionEnter2D to handle collisions with non-Rigidbody2D objects, like the ground/walls
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only trigger the black hole effect if the bomb has been thrown and hasn't triggered yet
        if (!isThrown || hasTriggered) { return; }

        // Ignore collisions with the player who threw the bomb, so they don't trigger it
        if (collision.gameObject != throwingPlayer)
        {
            hasTriggered = true;
        }
    }

    // Function to spawn the black hole prefab at a specified position
    private void SpawnBlackHole(Vector2 spawnPosition)
    {
        // Check if the black hole prefab has been assigned
        if (blackHolePrefab != null)
        {
            // Instantiate the black hole at the position of the gravity bomb
            Instantiate(blackHolePrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // Output warning if the black hole prefab is not assigned
            Debug.LogWarning("Black hole prefab is not assigned to the GravityBomb.");
        }
    }
}