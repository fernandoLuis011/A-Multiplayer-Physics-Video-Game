using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpV2 : MonoBehaviour
{
    public Transform handTransform;           // Where the item will be held (the player's hand)
    public Transform throwSpot;               // Where the item will be thrown from
    public LayerMask pickupLayer;             // The layer used to detect items that can be picked up

    public float baseThrowForce = 10f;        // Base force applied to the thrown item, adjusted based on player speed and item mass

    private Movement playerMovement;          // Reference to the Movement script for player movement logic
    private GameObject throwingPlayer = null; // The player currently holding the item
    public GameObject currentItem = null;     // The item currently held by the player
    public Rigidbody2D itemRb;                // Reference to the Rigidbody2D of the current item

    private void Start()
    {
        // Get the Movement script reference to manage player movement
        playerMovement = GetComponent<Movement>();
    }

    private void Update()
    {
        // Check if the player presses the pick-up/throw key (E for WASD players or L for arrow keys)
        if ((Input.GetKeyDown(KeyCode.E) && playerMovement.WASD) || (Input.GetKeyDown(KeyCode.L) && !playerMovement.WASD))
        {
            // If the player is not holding an item, try to pick one up
            if (currentItem == null)
            {
                TryPickupItem();
            }
            else
            {
                // Otherwise, throw the current item
                ThrowItem();
            }
        }

        // If the player is holding an item, ensure the item doesn't collide with the player
        if (currentItem != null)
        {
            // Toggle collision between the player and the item using a method from FallingObject
            currentItem.GetComponent<FallingObject>().TogglePlayerCollisionsWithSpecificPlayer(gameObject, true);
        }
    }

    private void TryPickupItem()
    {
        // Cast a circle in front of the player to check for pickup-able items
        Vector2 castOrigin = (Vector2)transform.position + new Vector2(0, 0.5f); // Slight offset to ensure detection
        RaycastHit2D hit = Physics2D.CircleCast(castOrigin, 0.5f, transform.right, 0, pickupLayer);

        // If the raycast hits an item (i.e., the object has the "Item" tag), pick it up
        if (hit.collider != null && hit.collider.CompareTag("Item"))
        {
            // Set the current item to the hit object and get its Rigidbody2D
            currentItem = hit.collider.gameObject;
            itemRb = currentItem.GetComponent<Rigidbody2D>();

            // Temporarily disable physics simulation on the item while it's held
            itemRb.simulated = false;
            itemRb.isKinematic = true;

            // Parent the item to the player's hand and reset its position and rotation
            currentItem.transform.SetParent(handTransform);
            currentItem.transform.localPosition = Vector3.zero;
            currentItem.transform.localRotation = Quaternion.identity;

            // Set the item's sorting order (to ensure it renders above the player)
            currentItem.GetComponent<SpriteRenderer>().sortingOrder = 26;

            // Notify the item that it has been picked up
            currentItem.GetComponent<FallingObject>().Pickup(gameObject);

            // If the item has an ItemLifetime script, notify it that the item was picked up
            if (currentItem.GetComponent<ItemLifetime>() != null)
            {
                currentItem.GetComponent<ItemLifetime>().OnPickedUp();
            }

            // Set the throwing player to this player
            throwingPlayer = gameObject;
            currentItem.GetComponent<FallingObject>().SetThrowingPlayer(throwingPlayer);

            // Notify the Movement script that the player is holding an item (affecting movement speed)
            playerMovement.WeightChange(true, itemRb.mass);
        }
    }

    private void ThrowItem()
    {
        if (currentItem != null)
        {
            // Unparent the item and restore its physics properties
            currentItem.transform.SetParent(null);
            itemRb.isKinematic = false;
            itemRb.simulated = true;

            // Get the player's speed and acceleration
            float playerSpeed = Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x);

            // To calculate acceleration, we'll need to know the previous velocity.
            float playerAcceleration = playerSpeed / Time.deltaTime; // Approximate acceleration based on velocity change

            float scaledAcceleration = playerAcceleration * 0.005f; // Scale down by a factor of 0.05 for more manageable numbers
            float scaledMass = itemRb.mass * 0.5f; // Scale down by a factor of 0.05 for more manageable numbers

            // Calculate throw force based on mass, acceleration, and a base throw factor
            float throwForce = ((scaledMass * scaledAcceleration)) + baseThrowForce;

            Debug.Log("Mass: " + (scaledMass));
            Debug.Log("Acceleration: " + (scaledAcceleration));
            Debug.Log("Throw Force: " + throwForce);

            // Determine the direction to throw the item (left or right depending on player facing)
            Vector2 throwDirection = (transform.localScale.x < 0) ? transform.right : -transform.right;

            // Set the item's position at the throw spot
            currentItem.transform.position = throwSpot.position;

            // Apply the calculated throw force to the item
            itemRb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);

            // Notify the item that it has been thrown and is no longer held
            currentItem.GetComponent<FallingObject>().Drop();

            // If the item has an ItemLifetime script, notify it that the item has been thrown
            if (currentItem.GetComponent<ItemLifetime>() != null)
            {
                currentItem.GetComponent<ItemLifetime>().OnDropped();
            }

            // Re-enable collision handling for the item (based on throw velocity)
            currentItem.GetComponent<FallingObject>().SetThrowingPlayer(throwingPlayer);

            // Reset the current item reference (no item is held now)
            currentItem = null;

            // Notify the Movement script that the player is no longer holding an item
            playerMovement.WeightChange(false, 0);
        }
    }

    // This method is called when the player dies and needs to drop their held item
    public void dropItemOnDeath()
    {
        if (currentItem != null)
        {
            // Unparent the item and restore its physics properties
            currentItem.transform.SetParent(null);
            itemRb.isKinematic = false;
            itemRb.simulated = true;

            // Notify the item that it has been dropped
            currentItem.GetComponent<FallingObject>().Drop();

            // If the item has an ItemLifetime script, notify it that the item has been dropped
            if (currentItem.GetComponent<ItemLifetime>() != null)
            {
                currentItem.GetComponent<ItemLifetime>().OnDropped();
            }

            // Re-enable collision handling for the item
            currentItem.GetComponent<FallingObject>().SetThrowingPlayer(throwingPlayer);

            // Reset the current item reference
            currentItem = null;

            // Notify the Movement script that the player is no longer holding an item
            playerMovement.WeightChange(false, 0);
        }
    }
}