using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLifetime : MonoBehaviour
{
    private const float velocityThreshold = 0.1f; // Minimum velocity threshold to consider as stationary
    public float despawnTimer = 10f;    // Time before item despawns (in seconds)
    private bool isPickedUp = false;    // Flag to track if the item has been picked up
    
    private Rigidbody2D rb;             // Reference to the Rigidbody2D component of the item
    private Coroutine despawnCoroutine; // Reference to the current coroutine managing the despawn timer

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        StartDespawnCoroutine(); // Start the despawn coroutine
    }

    // Update is called once per frame
    void Update()
    {
        // If the item is not picked up and its stationary
        if (!isPickedUp && rb.velocity.sqrMagnitude <= velocityThreshold * velocityThreshold)
        {
            // If the despawn coroutine isn't already running, start it
            if (despawnCoroutine == null)
            {
                StartDespawnCoroutine();
            }
        }
    }

    // Starts the despawn timer by initiating the despawn coroutine
    public void StartDespawnCoroutine()
    {
        // Clamp despawn timer to avoid zero or negative values
        despawnTimer = Mathf.Max(despawnTimer, 0.1f);
        
        // Start the coroutine to handle the despawn process
        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine); // Stop any existing coroutine before starting a new one
        }
        despawnCoroutine = StartCoroutine(despawn());
    }

    // Stops the despawn timer when an item is picked up
    public void StopDespawnCoroutine()
    {
        // If the despawn coroutine is running, stop it
        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine);
            despawnCoroutine = null; // Reset the reference
        }
    }

    // Coroutine to handle the item despawning after a set delay
    IEnumerator despawn()
    {
        yield return new WaitForSeconds(despawnTimer); // Wait for the duration of the despawn timer
        Destroy(gameObject); // Destroy the item after the specified delay
    }

    // Call this method when the item is picked up by the player
    public void OnPickedUp()
    {
        //Debug.Log("Item picked up: " + gameObject.name);
        isPickedUp = true;          // Mark the item as picked up
        StopDespawnCoroutine();     // Stop the despawn timer since the item is no longer idle
    }

    // Call this method when the item is dropped or thrown by the player
    public void OnDropped()
    {
        //Debug.Log("Item dropped: " + gameObject.name);
        isPickedUp = false;         // Mark the item as dropped
        StartDespawnCoroutine();    // Restart the despawn timer since the item is no longer in the player's possession
    }
}