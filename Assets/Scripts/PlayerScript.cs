using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Jump")]
    public float jumpHeight = 20f;                  // Hight of a jump
    public float jumpRatio = 10f;                   // True force of the jump
    public float airControlFactor = 0.002f;         // Amount of control in the air (0 = no control, 1 = full control)
    public bool isGrounded;                         // 1 when on the ground. 0 when not
    private float jumpForce;                        // Variable for unity to use

    [Header("Speed")]
    public float maxSpeedReachable;                 // Max speed reachable right now. Will change a lot (every time you do something to accelerate)
    public float acceleration;                      // "Speed" at wich you reach your maximum speed
    private float speedBuffer;                       // Use to keep in memory a speed
    private float initialMaxSpeed = 20f;

    [Header("JumpWall")]
    public bool isTouchingWall;                     // If the player is touching a wall
    private float wallTouchTime;                    // Last time the wall was touch
    public float wallJumpWindow = 3f;               // Time you have to do a perfect wall jump
    [SerializeField]
    private WallCheckScript rightSide;
    [SerializeField]
    private WallCheckScript leftSide;

    [Header("Dash")]
    public bool canDash = true;
    public float dashTime = 0.5f;                   //Duration of a dash
    public float dashRange = 2f;
    public float dashCD = 3f;
    public float curDashCD = 3f;

    [Header("Attack")]
    public bool canAttack = false;                  //Boolean to know if the player can attack
    public float attackCD = 1f;                     //Cooldown of the attack
    private float curAttackCD = 0f;                 //Timer until next attack
    public float attackDistance = 5f;               //Maximum distance where the player can attach

    [Header("Enemies")]
    public float detectionRadius = 10f;             // Radius to check for enemies
    public LayerMask enemyLayer;                    // Layer mask for enemies
    private GameObject nearestEnemy;                // Nearest enemy detected


    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        maxSpeedReachable = initialMaxSpeed;//Max speed init
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
        float dt = Time.deltaTime;
        //HandleAction(dt);
        isTouchingWall = IsTouchingWall();
        HandleMovement();
        HandleJump();
        //DetectNearestEnemy();
        //HandleAttack();
        //HandleDash();
        Flip();
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }

    private void Flip()
    {
        if (rb.velocity.x > 0.1f) GetComponent<SpriteRenderer>().flipX = false;
        else if (rb.velocity.x < -0.1f) GetComponent<SpriteRenderer>().flipX = true;
    }

    /*private void HandleAction(float dt)
    {
        //curAttackCD += dt;
        if (curDashCD <= dashCD) curDashCD += dt;
        if (curDashCD >= dashCD && !canDash)
        {
            canDash = true;
            curDashCD = 0f;
        }
    }
    
    private void HandleDash()
    {

        if (canDash && Input.GetKeyDown(KeyCode.R))
        {
            canDash = false;
            DashToward(Vector2.right, dashRange, dashTime);
        }
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
        else if (Input.GetKeyDown(KeyCode.E) && nearestEnemy != null)
        {
            Vector2 enemyPos = nearestEnemy.transform.position;
            Destroy(nearestEnemy);
            //DashToward(enemyPos, attackDistance,);
            canAttack = false;
        }
    }

    private void DashToward(Vector2 direction, float distance, float dashTime)
    {
        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + direction.normalized * distance;
        float elapsedTime = 0f;

        while (elapsedTime < dashTime)
        {
            elapsedTime += Time.deltaTime; // Increment the elapsed time
            float t = elapsedTime / dashTime; // Normalized time (from 0 to 1)

            // Interpolate the player's position between the start and target
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
        }

        // Ensure the final position is exactly at the target after the loop
        transform.position = targetPosition;
        //rb.AddForce(Vector2.right,ForceMode2D.Impulse);
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
    */

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
