using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float minJumpForce = 5f;
    [SerializeField] private float maxJumpForce = 12f;
    [SerializeField] private float maxChargeTime = 1.0f;
    [SerializeField] private float horizontalJumpForce = 5f;

    [Header("Wall Bounce")]
    [SerializeField] private float wallBounceMultiplier = 1f;
    [SerializeField] private float minimumWallBounceSpeed = 0.5f;
    [SerializeField] private float wallBounceCooldown = 0.1f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckBoxSize = new Vector2(0.6f, 0.15f);
    [SerializeField] private LayerMask groundLayer;

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;

    private bool wasGrounded;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isCharging;
    private float currentChargeTime;
    private float lastPressedHorizontalDirection;
    private float lastWallBounceTime = float.NegativeInfinity;

    // New
    private float chargedJumpDirection;
    private bool jumpedThisFrame;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        CheckIfGrounded();

        if (!wasGrounded && isGrounded)
        {
            if (audioSource != null && landClip != null)
            {
                audioSource.PlayOneShot(landClip);
            }
        }

        wasGrounded = isGrounded;

        UpdateHorizontalDirectionPriority();
        HandleMovementInput();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        HandleGroundMovement();

        // Clear after physics step
        jumpedThisFrame = false;
    }

    private void CheckIfGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck is not assigned on PlayerMovement.");
            return;
        }

        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckBoxSize, 0f, groundLayer);
    }

    private void UpdateHorizontalDirectionPriority()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            lastPressedHorizontalDirection = -1f;
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            lastPressedHorizontalDirection = 1f;
        }
    }

    private void HandleMovementInput()
    {
        if (!isGrounded || isCharging)
        {
            moveInput = 0f;
            return;
        }

        moveInput = GetHeldHorizontalDirection();
    }

    private void HandleGroundMovement()
    {
        // Do not let ground movement overwrite jump velocity
        if (jumpedThisFrame)
        {
            return;
        }

        if (isGrounded && !isCharging)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void HandleJumpInput()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentChargeTime = 0f;

            // Start with current held direction
            chargedJumpDirection = GetHeldHorizontalDirection();

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (!isCharging)
        {
            return;
        }

        if (!isGrounded)
        {
            CancelCharge();
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            currentChargeTime = Mathf.Min(currentChargeTime + Time.deltaTime, maxChargeTime);

            // Continuously remember latest held direction during charge
            float heldDirection = GetHeldHorizontalDirection();
            if (heldDirection != 0f)
            {
                chargedJumpDirection = heldDirection;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Jump();
        }
    }

    private float GetHeldHorizontalDirection()
    {
        bool movingLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool movingRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        if (movingLeft && !movingRight)
        {
            return -1f;
        }

        if (movingRight && !movingLeft)
        {
            return 1f;
        }

        if (movingLeft && movingRight)
        {
            return lastPressedHorizontalDirection;
        }

        return 0f;
    }

    private void Jump()
    {
        float chargePercent = maxChargeTime > 0f ? currentChargeTime / maxChargeTime : 1f;
        float verticalJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargePercent);
        float jumpDirectionX = chargedJumpDirection * horizontalJumpForce;

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(jumpDirectionX, verticalJumpForce);

        // Prevent ground movement from cancelling horizontal jump velocity
        jumpedThisFrame = true;

        // Force state away from grounded immediately
        isGrounded = false;

        if (audioSource != null && jumpClip != null)
        {
            audioSource.PlayOneShot(jumpClip);
        }

        CancelCharge();
    }

    private void CancelCharge()
    {
        isCharging = false;
        currentChargeTime = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryWallBounce(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryWallBounce(collision);
    }

    private void TryWallBounce(Collision2D collision)
    {
        if (isGrounded || isCharging)
        {
            return;
        }

        if (Time.time < lastWallBounceTime + wallBounceCooldown)
        {
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;

            if (Mathf.Abs(normal.x) < 0.75f)
            {
                continue;
            }

            float horizontalImpactSpeed = Mathf.Abs(collision.relativeVelocity.x);
            if (horizontalImpactSpeed < minimumWallBounceSpeed)
            {
                continue;
            }

            rb.linearVelocity = new Vector2(
                normal.x * horizontalImpactSpeed * wallBounceMultiplier,
                rb.linearVelocity.y
            );

            lastWallBounceTime = Time.time;
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckBoxSize);
    }
}