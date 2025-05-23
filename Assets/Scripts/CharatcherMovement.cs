using UnityEngine;
using System.Collections;

public class CharatcherMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    
    [Header("Dodge Settings")]
    [SerializeField] private float dodgeDistance = 1f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Rigidbody2D rb;
    private float moveDirection = 0f;
    private bool isRunning = false;
    private bool isDodging = false;
    private bool canDodge = true;
    private float lastFacingDirection = 1f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // If spriteRenderer wasn't assigned in inspector, try to get it
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Set gravity scale to ensure character stays on ground
        if (rb != null)
        {
            rb.freezeRotation = true; // Prevent character from rotating
        }
    }

    void Update()
    {
        // Skip regular movement input when dodging
        if (!isDodging)
        {
            // Get input for horizontal movement
            moveDirection = Input.GetAxis("Horizontal");
            
            // Update facing direction when there's input
            if (moveDirection != 0)
            {
                lastFacingDirection = Mathf.Sign(moveDirection);
            }
            
            // Check if the run button is pressed (Left Shift by default)
            isRunning = Input.GetKey(KeyCode.LeftShift);
            
            // Flip sprite based on movement direction
            if (moveDirection != 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = moveDirection < 0;
            }
            
            // Check for dodge input
            if (Input.GetKeyDown(KeyCode.Space) && canDodge)
            {
                StartCoroutine(Dodge());
            }
        }
    }
    
    void FixedUpdate()
    {
        // Skip regular movement when dodging
        if (!isDodging)
        {
            Move();
        }
    }
    
    private void Move()
    {
        // Calculate movement speed based on whether running or walking
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        // Apply horizontal movement while preserving vertical velocity
        Vector2 movement = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
        rb.linearVelocity = movement;
    }
    
    private IEnumerator Dodge()
    {
        // Set flags
        isDodging = true;
        canDodge = false;
        
        // Store original position
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + new Vector2(lastFacingDirection * dodgeDistance, 0);
        
        // Store current velocity to restore later
        Vector2 originalVelocity = rb.linearVelocity;
        
        // Disable physics during dodge
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        
        float elapsedTime = 0f;
        
        // Create dodge visual effect - optional
        // You can add a trail renderer here or particle effect
        
        // Perform the dodge using Lerp for smooth animation
        while (elapsedTime < dodgeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dodgeDuration;
            
            // Use Smooth Step for more dynamic movement (slow start, fast middle, slow end)
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            
            // Move character
            transform.position = Vector2.Lerp(startPosition, targetPosition, smoothProgress);
            
            yield return null;
        }
        
        // Ensure we reach exact target position
        transform.position = targetPosition;
        
        // Re-enable physics
        rb.isKinematic = false;
        
        // Reset dodge state
        isDodging = false;
        
        // Apply cooldown before allowing another dodge
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }
}