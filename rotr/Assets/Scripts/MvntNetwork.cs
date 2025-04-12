using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MvntNetwork : NetworkBehaviour // This class inherits from NetworkBehaviour now 
{
    private Animator animator;                  // Reference to the Animator component to control animations
    private Rigidbody2D playerRB;               // Reference to the Rigidbody2D component for physics
    private CapsuleCollider2D playerCollider;   // Reference to the CapsuleCollider2D for collision detection
    private PickUp playerPickUp;                // Reference to the PickUp component for item handling

    private float horizontal;                   // Stores the current horizontal input (left or right movement)
    private bool isFacingRight = true;          // Tracks whether the player is facing right
    private bool isOnPlatform = false;          // Tracks whether the player is on a moving platform

    public float speed;                         // Player's movement speed
    public float jumpingPower;                  // Player's jump power

    private float mass;                         // Player's mass (used for weight calculation)
    public float newMass;                       // Current mass of the player, which changes when picking up items
    private const float BASE_WEIGHT = 10f;      // Base mass of the player
    private const float BASE_SPEED = 5f;        // Base speed of the player
    private const float BASE_JP = 12.5f;        // Base jump power of the player

    public bool WASD;                           // If true, player uses WASD keys for movement, otherwise uses arrow keys
    private bool isMovementDisabled = false;    // Flag to disable movement during respawn or other situations

    private Vector2 boxSize = new Vector2(0.1f, 1f);    // Box collider size for ground check

    [SerializeField] private Transform groundCheck;     // Position where the ground check will happen
    [SerializeField] private LayerMask groundLayer;     // LayerMask for the ground objects

    [SerializeField] private AudioClip walkingSound;
    [SerializeField] private AudioClip jumpingSound;


    // Sets horizontal movement direction (used by other scripts)
    public void setHorizontal(float value)
    {
        horizontal = value;
    }

    // Gets current horizontal movement direction
    public float getHorizontal()
    {
        return horizontal;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Get references to required components
        playerRB = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        playerPickUp = GetComponent<PickUp>();

        // Initialize player attributes
        mass = BASE_WEIGHT;
        speed = BASE_SPEED;
        jumpingPower = BASE_JP;
        newMass = mass;

        transform.localScale = new Vector3(0.8f, 0.8f, 1f);

    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner) return; //Checks whether the local player owns this object and prevents other clients from controlling this object
        // If movement is disabled, don't process input or movement
        if (isMovementDisabled) { return; }

        horizontal = 0; // Reset horizontal movement

        // Check if the game is paused
        if (!PauseMenu.GamePaused)
        {
            // Check left movement (WASD or arrow keys)
            if ((Input.GetKey(KeyCode.LeftArrow) && !WASD) || (Input.GetKey(KeyCode.A) && WASD))
            {
                horizontal = -1; // Move left
                // Play walking sound
                //SoundManager.instance.PlaySound(walkingSound);

            }

            // Check right movement (WASD or arrow keys)
            if ((Input.GetKey(KeyCode.RightArrow) && !WASD) || (Input.GetKey(KeyCode.D) && WASD))
            {
                horizontal = 1; // Move right
                // Play walking sound
                //SoundManager.instance.PlaySound(walkingSound);
            }

            // Check for jump (WASD or arrow keys)
            if (((Input.GetKeyDown(KeyCode.UpArrow) && !WASD) || ((Input.GetKeyDown(KeyCode.W) && WASD))) && IsGrounded())
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, jumpingPower); // Apply jump force
                // Play jump sound
                SoundManager.instance.PlaySound(jumpingSound);

            }

            // Check for platform drop (down arrow key or S key)
            if ((Input.GetKey(KeyCode.DownArrow) && !WASD) || (Input.GetKey(KeyCode.S) && WASD))
            {
                // Disable the collider temporarily if on the platform and grounded
                if (IsGrounded() && isOnPlatform && playerCollider.enabled)
                {
                    playerCollider.enabled = false;
                    Invoke("EnableCollider", 0.25f); // Re-enable the collider after a small delay
                }
            }

            Flip(); // Flip the character's orientation if needed
            //transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            //Debug.Log("Current Scale: " + transform.localScale);

            animator.SetBool("1_Move", horizontal != 0); // Update animation state based on movement
        }
    }

    // Adjust the player's weight, speed, and jump power based on whether they are holding an item
    public void WeightChange(bool holdingItem, float itemMass)
    {
        newMass = holdingItem ? newMass + itemMass : BASE_WEIGHT;
        speed = CalculateStat(BASE_SPEED);
        jumpingPower = CalculateStat(BASE_JP);
    }

    // Helper method for calculating speed and jump power based on new mass
    private float CalculateStat(float baseStat)
    {
        return mass * baseStat / newMass;
    }
    
    // FixedUpdate is called at a fixed time step and is used for physics-based movement
    private void FixedUpdate()
    {
        // If movement is disabled, skip movement physics
        if (isMovementDisabled) { return; }

        // Apply horizontal velocity to the Rigidbody, maintaining current vertical velocity
        playerRB.velocity = new Vector2(horizontal * speed, playerRB.velocity.y);

        // Check if the player is on a moving platform
        MovingPlatformCheck();
    }

    // Flip the character's orientation based on movement direction
    private void Flip()
    {
        // Flip character's direction if changing movement direction
        if (isFacingRight && horizontal > 0f || !isFacingRight && horizontal < 0f)
        {
            
            isFacingRight = !isFacingRight; // Toggle facing direction
            Vector3 localScale = transform.localScale; // Get current local scale
            localScale.x *= -1f; // Flip the scale horizontally
            transform.localScale = localScale; // Apply the flipped scale
            


            /*
            isFacingRight = !isFacingRight; // Toggle facing direction
            Vector3 currentScale = transform.localScale;
            //currentScale.x = Mathf.Abs(currentScale.x); // Ensure positive X scale
            
            currentScale.x = -Mathf.Abs(currentScale.x); // Ensure negative X scale
            
            transform.localScale = currentScale;
            */

      
        }
    }



    // Check if the player is grounded (standing on the ground or platform)
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer); // Check if there's ground beneath the player
    }

    // Enable the player collider
    private void EnableCollider()
    {
        playerCollider.enabled = true;
    }

    // Handle collision with platforms (OneWay platforms)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWay"))
        {
            isOnPlatform = true; // Set platform status when the player collides with a "OneWay" platform
        }
    }

    // Handle when the player exits collision with platforms
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("OneWay"))
        {
            isOnPlatform = false; // Reset platform status when player leaves the platform
        }
    }

    // Allows the character to move with a moving platform
    void MovingPlatformCheck()
    {
        // Check for any colliders under the player to detect a moving platform
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, 0.2f, groundLayer);
        if (colliders.Length > 0)
        {
            foreach (var c in colliders)
            {
                if (c.tag == "Ground")
                {
                    transform.parent = c.transform; // Set the platform as the player's parent to move with it
                }
            }
        }
        else
        {
            transform.parent = null; // Remove parent if no platform is detected
        }
    }

    // Call this method to disable movement temporarily (e.g., during respawn)
    public void DisableMovement()
    {
        isMovementDisabled = true; // Disable movement
        setHorizontal(0); // Stop any current horizontal movement
        animator.SetBool("1_Move", horizontal != 0); // Update animation state
    }

    // Call this method to re-enable movement after respawn or other delay
    public void EnableMovement()
    {
        isMovementDisabled = false; // Re-enable movement
    }
}