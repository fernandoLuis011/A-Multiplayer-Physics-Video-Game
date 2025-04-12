using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRespawn : MonoBehaviour
{
    private HealthBar healthBar;            // Reference to the HealthBar script (handles health logic)
    private Movement playerMovement;        // Reference to the Movement script (handles player movement)

    public Transform intitialSpawnPosition; // The position where the player spawns initially
    public GameObject intitialSpawnPoint;   // The object (platform or point) where the player starts, should be activated when the game begins

    public Transform[] respawnPositions;    // Array of respawn positions where the player could respawn (randomized)
    public GameObject[] temporaryPlatforms; // Array of temporary platforms that will appear at random respawn positions
    public float respawnDelay = 4f;         // Delay before the platform disappears after respawning (not currently used in the logic)

    private int rand;                       // Stores the random index used to pick a respawn position (between 0 and 2)

    public float intitialSpawnDelay = 1f;   // Delay before hiding the initial spawn point after the player has spawned

    private bool playerRespawning = false;  // Flag that tracks whether the player is in the respawning state
    private float platformDuration = 4f;    // Duration for how long the temporary platform should stay active (in seconds)
    private float platformTimer = 0f;       // Timer used to track platform duration after respawn

    private void Awake()
    {
        // Get the references to other components on the player object
        healthBar = GetComponent<HealthBar>();
        playerMovement = GetComponent<Movement>();
    }

    private void Start()
    {
        // Set the player to the initial spawn position
        transform.position = intitialSpawnPosition.position;

        // Activate the initial spawn point object (for visual feedback)
        intitialSpawnPoint.SetActive(true);

        // Start the coroutine to handle the initial spawn delay (hides the spawn point after a set time)
        StartCoroutine(intitialSpawn());
    }

    private void Update()
    {
        // If the player is currently respawning (i.e., temporary platform is active)
        if (playerRespawning)
        {
            // Decrease the platform timer by the time passed in the current frame
            platformTimer -= Time.deltaTime;

            // If the platform timer ends or the player starts moving, deactivate the platform
            if (platformTimer <= 0 || playerMovement.getHorizontal() != 0)
            {
                // Disable the temporary platform that the player is standing on
                temporaryPlatforms[rand].SetActive(false);

                // Stop the respawn logic
                playerRespawning = false;
            }
        }
    }

    public void RespawnPlayer()
    {
        // Select a random respawn position (between 0 and 2, inclusive)
        rand = Random.Range(0, 3);  // Generate a random index (0, 1, or 2) to pick a respawn position

        // Set the player's position to the selected random respawn position
        transform.position = respawnPositions[rand].position;

        // Activate the corresponding temporary platform where the player will respawn
        temporaryPlatforms[rand].SetActive(true);

        // Call the OnRespawn to reset health and invulnerability
        healthBar.OnRespawn(); // Call the OnRespawn to reset health and invulnerability

        // Reset the platform timer so the platform will stay active for the correct duration
        platformTimer = platformDuration;

        // Set the respawning flag to true to start the countdown for disabling the platform
        playerRespawning = true;
    }

    // Coroutine to handle the delay before the initial spawn point disappears after the player spawns
    private IEnumerator intitialSpawn()
    {
        yield return new WaitForSeconds(intitialSpawnDelay); // Wait for the specified delay before hiding the initial spawn point
        intitialSpawnPoint.SetActive(false); // Deactivate the initial spawn point object after the delay
    }
}