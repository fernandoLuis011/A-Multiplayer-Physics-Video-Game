using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ItemDrop : NetworkBehaviour
{
    public float respawnTimerMin = 3f;  // Minimum respawn time
    public float respawnTimerMax = 10f; // Maximum respawn time

    public List<GameObject> items;      // List of items that can be spawned
    private Vector2 screenBounds;       // Stores the screen bounds in world space

    // Start is called before the first frame update
    void Start()
    {
        if (!IsServer) return; // only the host should spawn items

        // Convert the screen bounds to world space coordinates based on the camera
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        
        // Start the item respawn coroutine
        StartCoroutine(itemDrop());
    }

    void Update()
    {
        if (!IsServer) return; // block client input

        // Loop through keys 1-8 to spawn corresponding items
        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // Check if 1, 2, ..., 8 is pressed
            {
                spawnItem(items[i]);
                break; // Exit the loop
            }
        }
    }

    private void spawnItem(GameObject itemToClone)
    {
        float xPosition = Random.Range(-screenBounds.x, screenBounds.x); // Random X position within bounds
        float yPosition = screenBounds.y; // Fixed Y position (top of the screen)

        // Instantiate the item at the random position
        GameObject clonedItem = Instantiate(itemToClone, new Vector2(xPosition, yPosition), Quaternion.identity);
        
        // Activate it and spawn it as a networked object
        clonedItem.SetActive(true);
        
        var networkObject = clonedItem.GetComponent<NetworkObject>();
        if (networkObject != null && NetworkManager.Singleton.IsServer)
        {
            networkObject.Spawn(); // Only host should spawn
        }

        // Attach ItemLifetime to handle despawn if needed
        if (clonedItem.GetComponent<ItemLifetime>() == null)
        {
            clonedItem.AddComponent<ItemLifetime>();
        }
    }
    /*
    Before network implementation
    private void spawnItem(GameObject itemToClone)
    {
        float xPosition = Random.Range(-screenBounds.x, screenBounds.x); // Random X position within bounds
        float yPosition = screenBounds.y; // Fixed Y position (top of the screen)

        // Instantiate the item at the random position
        GameObject clonedItem = Instantiate(itemToClone, new Vector2(xPosition, yPosition), Quaternion.identity);
        clonedItem.SetActive(true);

        // Add the ItemLifetime script to handle item destruction
        clonedItem.AddComponent<ItemLifetime>();
    }
    */

    IEnumerator itemDrop()
    {
        while (true)
        {
            // Wait for a random time between respawnTimerMin and respawnTimerMax before spawning another item
            yield return new WaitForSeconds(Random.Range(respawnTimerMin, respawnTimerMax));

            // Spawn a random item from the list
            spawnItem(items[Random.Range(0, items.Count)]);
        }
    }
}
