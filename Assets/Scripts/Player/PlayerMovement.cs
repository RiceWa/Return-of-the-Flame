using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f; // Controls how fast the player moves left and right

    private Rigidbody2D rb;   // Reference to the Rigidbody2D on the player
    private float moveInput;  // Stores horizontal input: -1 = left, 1 = right, 0 = no movement

    private void Awake()
    {
        // Get the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Check whether left keys are currently being held
        bool movingLeft = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);

        // Check whether right keys are currently being held
        bool movingRight = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

        // If both directions are pressed at the same time, cancel movement
        if (movingLeft && movingRight)
        {
            moveInput = 0f;
        }
        // Move left only
        else if (movingLeft)
        {
            moveInput = -1f;
        }
        // Move right only
        else if (movingRight)
        {
            moveInput = 1f;
        }
        // No horizontal keys pressed
        else
        {
            moveInput = 0f;
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement while preserving the current vertical velocity
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}