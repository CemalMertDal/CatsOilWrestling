using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float dodgeMultiplier = 2f;
    public float dodgeDuration = 0.3f;
    public float dodgeCooldown = 1f;
    public float pushForce = 7f;
    public float grappleThrowForce = 12f;
    public float grappleRange = 2f;
    
    // Add offset parameters to control grabbed enemy position
    public Vector2 grabbedEnemyOffset = new Vector2(1.5f, 0.5f); // Change these values to position the enemy correctly

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool canDodge = true;
    private bool isGrappling = false;
    private GameObject grabbedEnemy;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckGrounded();

        if (!isGrappling)
        {
            Move();
            Jump();
        }
        else if (grabbedEnemy != null)
        {
            // Position the grabbed enemy with the new offset values
            // This positions the enemy in front of the player based on facing direction
            Vector3 holdPosition = transform.position + 
                new Vector3(grabbedEnemyOffset.x * transform.localScale.x, 
                            grabbedEnemyOffset.y, 0);
                            
            grabbedEnemy.transform.position = holdPosition;
        }

        Dodge();
        CheckGrappleInput();
        GrappleThrow();
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Move()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        // Update character facing direction
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void Dodge()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDodge)
        {
            StartCoroutine(DodgeCoroutine());
        }
    }

    IEnumerator DodgeCoroutine()
    {
        canDodge = false;
        float originalSpeed = moveSpeed;
        moveSpeed *= dodgeMultiplier;
        yield return new WaitForSeconds(dodgeDuration);
        moveSpeed = originalSpeed;
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    void CheckGrappleInput()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isGrappling)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, grappleRange, enemyLayer);
            
            float closestDistance = Mathf.Infinity;
            GameObject closestEnemy = null;
            
            foreach (Collider2D hit in hitColliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    float distance = Vector2.Distance(transform.position, hit.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = hit.gameObject;
                    }
                }
            }
            
            if (closestEnemy != null)
            {
                GrappleEnemy(closestEnemy);
            }
        }
    }
    
    void GrappleEnemy(GameObject enemy)
    {
        isGrappling = true;
        grabbedEnemy = enemy;
        
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            // Save the enemy's original layer
            enemy.layer = LayerMask.NameToLayer("Ignore Raycast"); // Temporarily change layer to avoid collision issues
            
            enemyRb.linearVelocity = Vector2.zero;
            enemyRb.angularVelocity = 0;
            enemyRb.constraints = RigidbodyConstraints2D.FreezeAll;
            Debug.Log("Enemy grabbed!");
        }
    }

    void GrappleThrow()
    {
        if (isGrappling && grabbedEnemy != null && Input.GetKeyDown(KeyCode.F))
        {
            Rigidbody2D enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                // Restore original layer if needed
                grabbedEnemy.layer = LayerMask.NameToLayer("Enemy");
                
                enemyRb.constraints = RigidbodyConstraints2D.FreezeRotation;
                
                Vector2 throwDir = new Vector2(transform.localScale.x, 1).normalized;
                enemyRb.AddForce(throwDir * grappleThrowForce, ForceMode2D.Impulse);
                
                isGrappling = false;
                grabbedEnemy = null;
                Debug.Log("Enemy thrown!");
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody2D enemyRb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (Input.GetKeyDown(KeyCode.F))
            {
                Vector2 pushDir = (collision.transform.position - transform.position).normalized;
                enemyRb.AddForce(pushDir * pushForce, ForceMode2D.Impulse);
            }

            if (Input.GetKeyDown(KeyCode.G) && !isGrappling)
            {
                GrappleEnemy(collision.gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, grappleRange);
        
        // Draw the position where grabbed enemies will be held
        Gizmos.color = Color.yellow;
        Vector3 holdPosition = transform.position + new Vector3(grabbedEnemyOffset.x, grabbedEnemyOffset.y, 0);
        Gizmos.DrawWireSphere(holdPosition, 0.2f);
    }
}