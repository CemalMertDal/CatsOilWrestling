using System.Collections;
using UnityEngine;

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
    private Rigidbody2D enemyRb;
    private GameObject enemyObject;

    private float moveDirection = 0f;
    private bool isRunning = false;
    private bool isDodging = false;
    private bool canDodge = true;
    private float lastFacingDirection = 1f;
    private bool isGrappling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
            rb.freezeRotation = true;

        // Enemy objesini sahnede otomatik bul
        enemyObject = GameObject.Find("Enemy");

        if (enemyObject != null)
        {
            enemyRb = enemyObject.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
                enemyRb.freezeRotation = true;
        }
        else
        {
            Debug.LogWarning("Enemy GameObject bulunamadı! Sahnede adı tam olarak 'Enemy' olmalı.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartGrapple();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            ReleaseGrapple();
        }

        if (!isDodging && !isGrappling)
        {
            moveDirection = Input.GetAxis("Horizontal");

            if (moveDirection != 0)
                lastFacingDirection = Mathf.Sign(moveDirection);

            isRunning = Input.GetKey(KeyCode.LeftShift);

            if (moveDirection != 0 && spriteRenderer != null)
                spriteRenderer.flipX = moveDirection < 0;

            if (Input.GetKeyDown(KeyCode.Space) && canDodge)
                StartCoroutine(Dodge());
        }
    }

    void FixedUpdate()
    {
        if (!isDodging && !isGrappling)
            Move();
    }

    private void Move()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        Vector2 movement = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);
        rb.linearVelocity = movement;
    }

    private IEnumerator Dodge()
    {
        isDodging = true;
        canDodge = false;

        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + new Vector2(lastFacingDirection * dodgeDistance, 0);

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        float elapsedTime = 0f;

        while (elapsedTime < dodgeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dodgeDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            transform.position = Vector2.Lerp(startPosition, targetPosition, smoothProgress);
            yield return null;
        }

        transform.position = targetPosition;
        rb.bodyType = RigidbodyType2D.Dynamic;

        isDodging = false;

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    private void StartGrapple()
    {
        if (enemyRb == null) return;

        isGrappling = true;

        rb.linearVelocity = Vector2.zero;
        enemyRb.linearVelocity = Vector2.zero;

        rb.bodyType = RigidbodyType2D.Kinematic;
        enemyRb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void ReleaseGrapple()
    {
        if (enemyRb == null) return;

        isGrappling = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        enemyRb.bodyType = RigidbodyType2D.Dynamic;

        // Enemy'yi geri fırlat
        float throwForce = 10f;
        Vector2 throwDir = new Vector2(lastFacingDirection, 0).normalized;
        enemyRb.AddForce(throwDir * throwForce, ForceMode2D.Impulse);
    }
}
