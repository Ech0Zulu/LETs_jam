using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f; // Horizontal movement speed
    public float jumpHeight = 20f; // Hight of a jump
    private float jumpForce;
    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Move the player left and right
        float moveX = Input.GetAxis("Horizontal"); // Left (-1) and Right (+1)
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        UpdateJumpForce(rb.gravityScale);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void ChangeGravityTo(float newGravity)
    {
        rb.gravityScale = newGravity;
        UpdateJumpForce(newGravity);
    }

    void UpdateJumpForce(float gravity)
    {
        jumpForce = Mathf.Sqrt(2 * Mathf.Abs(gravity) * jumpHeight);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player is touching the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // Check if the player is no longer touching the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
