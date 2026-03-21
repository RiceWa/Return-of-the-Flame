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

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isCharging;
    private float currentChargeTime;
    private float queuedJumpDirection;
    private float lastPressedHorizontalDirection;
    private bool hasLockedJumpDirection;
    private float lastWallBounceTime = float.NegativeInfinity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckIfGrounded();
        UpdateHorizontalDirectionPriority();
        HandleMovementInput();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        HandleGroundMovement();
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
        if (!isGrounded)
        {
            moveInput = 0f;
            return;
        }

        if (isCharging)
        {
            moveInput = 0f;
            return;
        }

        moveInput = GetHeldHorizontalDirection();
    }

    private void HandleGroundMovement()
    {
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
            queuedJumpDirection = 0f;
            hasLockedJumpDirection = false;

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

        TryLockJumpDirection();

        if (Input.GetKey(KeyCode.Space))
        {
            currentChargeTime = Mathf.Min(currentChargeTime + Time.deltaTime, maxChargeTime);
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

    private void TryLockJumpDirection()
    {
        if (hasLockedJumpDirection)
        {
            return;
        }

        bool pressedLeft = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
        bool pressedRight = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);

        if (pressedLeft && !pressedRight)
        {
            queuedJumpDirection = -1f;
            hasLockedJumpDirection = true;
            return;
        }

        if (pressedRight && !pressedLeft)
        {
            queuedJumpDirection = 1f;
            hasLockedJumpDirection = true;
            return;
        }

        if (pressedLeft && pressedRight)
        {
            queuedJumpDirection = lastPressedHorizontalDirection;
            hasLockedJumpDirection = true;
        }
    }

    private void Jump()
    {
        float chargePercent = maxChargeTime > 0f ? currentChargeTime / maxChargeTime : 1f;
        float verticalJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargePercent);
        float jumpDirectionX = queuedJumpDirection * horizontalJumpForce;

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = new Vector2(jumpDirectionX, verticalJumpForce);

        CancelCharge();
    }

    private void CancelCharge()
    {
        isCharging = false;
        currentChargeTime = 0f;
        queuedJumpDirection = 0f;
        hasLockedJumpDirection = false;
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

            rb.linearVelocity = new Vector2(normal.x * horizontalImpactSpeed * wallBounceMultiplier, rb.linearVelocity.y);
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
