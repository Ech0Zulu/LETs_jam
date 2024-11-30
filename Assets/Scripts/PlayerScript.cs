using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movements")]



    public float acceleration;                      // "Speed" at wich you reach your maximum speed
    public float airControlFactor = 0.002f;         // Amount of control in the air (0 = no control, 1 = full control)
    public float jumpHeight = 20f;                  // Hight of a jump
    public float jumpRatio = 10f;                   // True force of the jump
    private float jumpForce;                        // Variable for unity to use
    private Rigidbody2D rb;
    public bool isGrounded;                         // 1 when on the ground. 0 when not
    public bool isTouchingWall;                     // If the player is touching a wall
    public float maxSpeedReachable;                 // Max speed reachable right now. Will change a lot (every time you do something to accelerate)
    private float initialMaxSpeed = 20f;
    public float speedBuffer;                       // Use to keep in memory a speed
    private float wallTouchTime;                    // Last time the wall was touch
    public float wallJumpWindow = 3f;               // Time you have to do a perfect wall jump
    public WallCheckScript rightSide;
    public WallCheckScript leftSide;

    public bool canAttack = false;
    public float attackCD = 1f;
    public float curAttackCD = 0f;
    public float detectionRadius = 10f;             // Radius to check for enemies
    public LayerMask enemyLayer;                    // Layer mask for enemies
    private GameObject nearestEnemy;                // Nearest enemy detected
    public float attackDistance = 5f;
    public float dashTime = 0.5f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Initialization
        maxSpeedReachable = initialMaxSpeed; // Initialize your max speed
        // !HAVE TO BE AFTER THE MAX SPEED INITIALIZATION!
        updateAcceleration(); // Initialize the acceleration based on your current max speed
        UpdateJumpForce(rb.gravityScale);
    }

    private bool IsTouchingWall()
    {
        if (rightSide.IsTouchingWall())
        {
            leftSide.setEnable();
            return true;
        }
        else if (leftSide.IsTouchingWall())
        {
            rightSide.setEnable();
            return true;
        }
        return false;
    }

    void Update()
    {
        isTouchingWall = IsTouchingWall();
        HandleMovement(); 

        HandleJump();
        DetectNearestEnemy();
        HandleAttack();
    }

    private void HandleAttack()
    {
        if (!canAttack)
        {
            curAttackCD += Time.deltaTime;
            if (curAttackCD >= attackCD)
            {
                curAttackCD = 0f;
                canAttack = true;
            }
        }
        else if (Input.GetKey(KeyCode.E) && nearestEnemy != null)
        {
            Vector2 enemyPos = nearestEnemy.transform.position;
            Destroy(nearestEnemy);
            StartCoroutine(DashToward(enemyPos,attackDistance));
            canAttack = false;
        }
    }

    private IEnumerator DashToward(Vector2 direction, float distance)
    {
        float elapsedTime = 0f; // Tracks the time elapsed during the dash
        Vector2 startPosition = transform.position; // Player's starting position
        Vector2 targetPosition = startPosition + direction.normalized * distance; // Target position to dash toward

        while (elapsedTime < dashTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dashTime; // Normalized time (0 to 1)

            // Move the player smoothly toward the target position
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);

            yield return null; // Wait until the next frame
        }

        // Ensure the player ends up exactly at the target position
        transform.position = targetPosition;
    }

    private void DetectNearestEnemy()
    {
        // Detect all enemies within the detection radius
        Collider2D[] detectedEnemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

        float shortestDistance = Mathf.Infinity;
        nearestEnemy = null;
        if (detectedEnemies.Length < 0)
        {
            nearestEnemy = null;
        }
        else
        {
            foreach (Collider2D enemy in detectedEnemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    Debug.Log("Enemy found : " + enemy.gameObject.name);
                    float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distanceToEnemy < shortestDistance)
                    {
                        shortestDistance = distanceToEnemy;
                        nearestEnemy = enemy.gameObject;
                    }
                }
            }
        }
        if (nearestEnemy != null)
        {
            Debug.Log("Nearest enemy : " + nearestEnemy.name);
        }
        else Debug.Log("No enemy around");
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // Get -1, 1 or 0 depending on Q, D or nothing pressed

        float lastSpeed = math.abs(rb.velocity.x);

        if (isGrounded)
        {
            rightSide.setEnable();
            leftSide.setEnable();
            rb.gravityScale = 7;
            float newXSpeed = rb.velocity.x + moveX * acceleration; // Increasing gradualy the speed depending on your acceleration
            newXSpeed = math.clamp(newXSpeed, maxSpeedReachable * -1, maxSpeedReachable); // Making sure you don't exceed your maximum speed
            rb.velocity = new Vector2(newXSpeed, rb.velocity.y); // Updating the speed
        }
        else if (isTouchingWall) 
        {
            rb.gravityScale = 0;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            // If the player is touching the wall, stop vertical movement
            if (Input.GetAxis("Vertical") < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -5); // Move downwards if the player presses down (optional)
            }
        } 
        else // If in the air
        {
            rb.gravityScale = 7;
            float newXSpeed = rb.velocity.x + moveX * acceleration * airControlFactor; // Increasing gradualy the speed depending on your acceleration. The acceleration is diminued by airControlFactor cause the player is in the air
            newXSpeed = math.clamp(newXSpeed, maxSpeedReachable * -1, maxSpeedReachable); // Making sure you don't exceed your maximum speed
            rb.velocity = new Vector2(newXSpeed, rb.velocity.y); // Updating the speed
        }

        UpdateMaxSpeedReachable(math.abs(rb.velocity.x) > lastSpeed);
    }

    void UpdateMaxSpeedReachable(bool isAccelerationg)
    {
        if (!isAccelerationg)
        {
            // Decrease the maxSpeed depending on the curent speed
            maxSpeedReachable = math.clamp(math.abs(rb.velocity.x) + 3, initialMaxSpeed, maxSpeedReachable);
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded) // If space pressed and player on the ground
        {
            Jump();
        }
        else if (Input.GetButtonDown("Jump") && isTouchingWall) // If space pressed and player on the ground
        {
            WallJump();
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isGrounded = false;
        maxSpeedReachable += 2;
    }

    void WallJump()
    {
        float moveX = getOppositeDirectionFromWall();
        rb.velocity = new Vector2((maxSpeedReachable - 5) * moveX, jumpForce);
    }

    float getOppositeDirectionFromWall()
    {
        if (rightSide.IsTouchingWall())
        {
            return -1;
        }
        else 
        {
            return 1;
        }
    }

    // Change the speed at wich the player falls. Does not change the height he can jump
    void ChangeGravityTo(float newGravity)
    {
        rb.gravityScale = newGravity;
        UpdateJumpForce(newGravity);
    }

    void UpdateJumpForce(float gravity)
    {
        jumpForce = Mathf.Sqrt(jumpRatio * Mathf.Abs(gravity) * jumpHeight); // Making sure the player still jump at the same height
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        // Check if the player is touching the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // Usefull when you fall of without jumping
    void OnCollisionExit2D(Collision2D collision)
    {
        // Check if the player is no longer touching the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void LeaveWall()
    {
        speedBuffer = 0;
        wallTouchTime = 0;
        rb.gravityScale = 7;
    }

    public void Respawn(Vector2 respawnPoint)
    {
        Teleport(respawnPoint);
        rb.velocity = Vector2.zero; // Set all speeds to 0
        maxSpeedReachable = initialMaxSpeed;
    }

    public void Teleport(Vector2 coords)
    {
        transform.position = coords; // Teleport the player to the point
    }

    public void increaseMaxSpeed(float deltaSpeed)
    {
        maxSpeedReachable += deltaSpeed;
        updateAcceleration();
    }

    void updateAcceleration()
    {
        acceleration = maxSpeedReachable / 10; // If high speed, it's still reachable
    }
}
