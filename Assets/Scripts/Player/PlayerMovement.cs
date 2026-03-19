using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float moveSpeed = 5f; // Speed used only while the player is on the ground and not charging

    [Header("Jump")]
    [SerializeField] private float minJumpForce = 5f;        // Minimum upward force for a quick tap jump
    [SerializeField] private float maxJumpForce = 12f;       // Maximum upward force for a fully charged jump
    [SerializeField] private float maxChargeTime = 1.0f;     // Maximum amount of time the player can charge a jump
    [SerializeField] private float horizontalJumpForce = 5f; // Horizontal force applied when jumping left or right

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;            // Point placed slightly below the player
    [SerializeField] private float groundCheckRadius = 0.15f;  // Radius of the ground detection circle
    [SerializeField] private LayerMask groundLayer;            // Which layer counts as ground

    private Rigidbody2D rb;            // Reference to the player's Rigidbody2D
    private float moveInput;           // Horizontal movement input while on the ground
    private bool isGrounded;           // True when touching the ground
    private bool wasGrounded;          // Stores whether the player was grounded on the previous frame
    private bool isCharging;           // True while holding Space to charge a jump
    private float currentChargeTime;   // How long Space has been held

    private float queuedJumpDirection; // Stores intended jump direction while charging: -1 left, 0 straight, 1 right

    private void Awake()
    {
        // Get the Rigidbody2D attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckIfGrounded();
        HandleGroundStateTransitions();
        HandleMovementInput();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        HandleGroundMovement();
    }

    private void CheckIfGrounded()
    {
        // Prevent errors if GroundCheck was not assigned in the Inspector
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck is not assigned on PlayerMovement.");
            return;
        }

        // Check whether the circle under the player is touching the ground layer
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleGroundStateTransitions()
    {
        // If the player was grounded last frame and is now airborne,
        // they have just left the ground, so clear old queued jump direction
        if (wasGrounded && !isGrounded)
        {
            queuedJumpDirection = 0f;
        }

        // Store current grounded state for next frame
        wasGrounded = isGrounded;
    }

    private void HandleMovementInput()
    {
        // Read left/right input manually
        bool movingLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool movingRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // If the player is in the air, they should not be able to move horizontally
        if (!isGrounded)
        {
            moveInput = 0f;
            return;
        }

        // If the player is charging a jump, lock ground movement
        if (isCharging)
        {
            moveInput = 0f;
            return;
        }

        // Normal grounded movement only happens when standing on the ground and not charging
        if (movingLeft && movingRight)
        {
            moveInput = 0f;
        }
        else if (movingLeft)
        {
            moveInput = -1f;
        }
        else if (movingRight)
        {
            moveInput = 1f;
        }
        else
        {
            moveInput = 0f;
        }
    }

    private void HandleGroundMovement()
    {
        // Allow horizontal movement only while grounded and not charging
        if (isGrounded && !isCharging)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void HandleJumpInput()
    {
        // Start charging only if the player is grounded and just pressed Space
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            currentChargeTime = 0f;
            queuedJumpDirection = 0f; // Start with a straight-up jump unless a direction is chosen during charging

            // Stop all horizontal ground movement while charging
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // While charging, continuously update the intended jump direction
        if (isCharging)
        {
            UpdateQueuedJumpDirection();
        }

        // Increase charge while Space is being held
        if (isCharging && Input.GetKey(KeyCode.Space))
        {
            currentChargeTime += Time.deltaTime;
            currentChargeTime = Mathf.Clamp(currentChargeTime, 0f, maxChargeTime);
        }

        // Perform the jump when Space is released
        if (isCharging && Input.GetKeyUp(KeyCode.Space))
        {
            Jump();
        }
    }

    private void UpdateQueuedJumpDirection()
    {
        // Read left/right input while charging
        bool movingLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool movingRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // If only left is held, queue a left diagonal jump
        if (movingLeft && !movingRight)
        {
            queuedJumpDirection = -1f;
        }
        // If only right is held, queue a right diagonal jump
        else if (movingRight && !movingLeft)
        {
            queuedJumpDirection = 1f;
        }
        // If both or neither are held, queue a straight jump
        else
        {
            queuedJumpDirection = 0f;
        }
    }

    private void Jump()
    {
        // Convert charge time into a 0 to 1 percentage
        float chargePercent = currentChargeTime / maxChargeTime;

        // Calculate upward jump force between the minimum and maximum
        float verticalJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargePercent);

        // Convert queued direction into actual horizontal jump velocity
        float jumpDirectionX = queuedJumpDirection * horizontalJumpForce;

        // Replace current velocity so the jump is consistent and not affected by leftover movement
        rb.linearVelocity = Vector2.zero;

        // Apply the jump as a single instant velocity change
        rb.linearVelocity = new Vector2(jumpDirectionX, verticalJumpForce);

        // Reset charging state
        isCharging = false;
        currentChargeTime = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the ground check circle in the Scene view for debugging
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}