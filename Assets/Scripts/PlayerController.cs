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

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool canDodge = true;
    private bool isGrappling = false;
    private GameObject grabbedEnemy;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

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
            grabbedEnemy.transform.position = transform.position + new Vector3(0.5f * transform.localScale.x, 1.2f, 0);
        }

        Dodge();
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

    void GrappleThrow()
    {
        if (isGrappling && grabbedEnemy != null && Input.GetKeyDown(KeyCode.H))
        {
            Rigidbody2D enemyRb = grabbedEnemy.GetComponent<Rigidbody2D>();

            enemyRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            Vector2 throwDir = new Vector2(transform.localScale.x, 1).normalized;
            enemyRb.AddForce(throwDir * grappleThrowForce, ForceMode2D.Impulse);

            isGrappling = false;
            grabbedEnemy = null;
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
                isGrappling = true;
                grabbedEnemy = collision.gameObject;

                enemyRb.linearVelocity = Vector2.zero;
                enemyRb.angularVelocity = 0;
                enemyRb.constraints = RigidbodyConstraints2D.FreezeAll;

                Debug.Log("Enemy grabbed!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
