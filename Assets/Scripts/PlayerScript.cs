using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField]
    private float Orientation = 1; // The X scale factor for the player. -1 when faceing left, 1 when facing right

    [Header("Jump")]
    public float jumpHeight = 20f;                  // Hight of a jump
    public float jumpRatio = 10f;                   // True force of the jump
    public float airControlFactor = 0.002f;         // Amount of control in the air (0 = no control, 1 = full control)
    public bool isGrounded;                         // 1 when on the ground. 0 when not
    private float jumpForce;                        // Variable for unity to use

    [Header("Speed")]
    public float maxSpeedReachable;                 // Max speed reachable right now. Will change a lot (every time you do something to accelerate)
    public float acceleration;                      // "Speed" at wich you reach your maximum speed
    private float speedBuffer = 0;                       // Use to keep in memory a speed
    private float initialMaxSpeed = 20f;
    public float boostPerfectWallJump = 2f;

    [Header("JumpWall")]
    public bool isTouchingWall;                     // If the player is touching a wall
    private float wallTouchTime;                    // Last time the wall was touch
    public float wallJumpWindow = 3f;               // Time you have to do a perfect wall jump
    public float checkRadius = 0.2f;            // Radius of the circle for wall detection
    public LayerMask environementLayer;                     // The layer that represents walls and
    [SerializeField]
    private Transform wallCheck;
    [SerializeField]
    private Transform groundCheck;

    [Header("Dash")]
    public bool isDashing = false;
    public bool canDash = true;
    public float dashTime = 0.5f;                   //Duration of a dash
    public float dashRange = 10f;
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
        maxSpeedReachable = initialMaxSpeed; //Max speed init
        // !HAVE TO BE AFTER THE MAX SPEED INITIALIZATION!
        updateAcceleration(); // Initialize the acceleration based on your current max speed
        UpdateJumpForce(rb.gravityScale);
    }

    private bool IsTouchingWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, checkRadius, environementLayer);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, environementLayer);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        isTouchingWall = IsTouchingWall();
        isGrounded = IsGrounded();
        HandleAction(dt);
        HandleDash();
        if (!isDashing)
        {
            BufferTheSpeed();
            HandleMovement();
            HandleJump();
            //DetectNearestEnemy();
            //HandleAttack();
        }
        Flip();
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }

    private void Flip()
    {
        if (rb.velocity.x > 0.1f) Orientation = 1; 
        else if (rb.velocity.x < -0.1f) Orientation = -1;

        Vector3 newScale = transform.localScale;
        newScale.x = Orientation;
        transform.localScale = newScale;
    }

    private void HandleAction(float dt)
    {
        if (curDashCD <= 0) curDashCD -= dt;
        if (curDashCD < 0) curDashCD = 0;
    }

    private bool CanDash()
    {
        return curDashCD == 0;
    }
    
    private void HandleDash()
    {
        if (/*CanDash() && */(Input.GetMouseButtonDown(0)))
        {
            // Get one of the 8 direction possible depending on ZQSD/WASD
            Vector2 direction = new Vector2(
                Input.GetAxis("Horizontal") > 0 ? 1 : Input.GetAxis("Horizontal") < 0 ? -1 : 0,
                Input.GetAxis("Vertical") > 0 ? 1 : Input.GetAxis("Vertical") < 0 ? -1 : 0
                );
            StartCoroutine(DashCoroutine(direction, dashRange, dashTime)); // Dash
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

    private IEnumerator DashCoroutine(Vector2 direction, float distance, float dashTime)
    {
        isDashing = true;

        Vector2 bufferSpeed = rb.velocity; // Remeber the speed of the player before the dash
        rb.velocity = new Vector2(0,0);

        Vector2 startPosition = transform.position;
        Vector2 targetPosition = startPosition + direction * distance;
        
        float elapsedTime = 0f;

        while (elapsedTime < dashTime)
        {
            elapsedTime += Time.deltaTime; // Increment the elapsed time
            float t = elapsedTime / dashTime; // Normalized time (from 0 to 1)

            // Interpolate the player's position between the start and target
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);

            yield return null; // Wait for the next frame
        }
        // Ensure the final position is exactly at the target after the loop
        transform.position = targetPosition;
        rb.velocity = bufferSpeed; // Give back the speed to the player
        isDashing = false;
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
            rb.gravityScale = 7;
            float newXSpeed = rb.velocity.x + moveX * acceleration; // Increasing gradualy the speed depending on your acceleration
            newXSpeed = math.clamp(newXSpeed, maxSpeedReachable * -1, maxSpeedReachable); // Making sure you don't exceed your maximum speed
            if (isTouchingWall && newXSpeed * Orientation > 0) newXSpeed = 0;
            rb.velocity = new Vector2(newXSpeed, rb.velocity.y); // Updating the speed
        }/*
        else if (isTouchingWall)
        {


            rb.gravityScale = 0;
            rb.velocity = new Vector2(rb.velocity.x, 0);
            // If the player is touching the wall, stop vertical movement
            if (Input.GetAxis("Vertical") < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, -5); // Move downwards if the player presses down (optional)
            }
        }*/
        else // If in the air
        {
            rb.gravityScale = 7;
            float newXSpeed = rb.velocity.x + moveX * acceleration * airControlFactor; // Increasing gradualy the speed depending on your acceleration. The acceleration is diminued by airControlFactor cause the player is in the air
            newXSpeed = math.clamp(newXSpeed, maxSpeedReachable * -1, maxSpeedReachable); // Making sure you don't exceed your maximum speed
            if (isTouchingWall && newXSpeed * Orientation > 0) newXSpeed = 0;
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
        if (Input.GetButtonDown("Jump") && isGrounded && !isTouchingWall) // If space pressed and player on the ground
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
        maxSpeedReachable += 2;
    }

    void WallJump()
    {
        if (Time.time - wallTouchTime >= wallJumpWindow)
        {
            PerfectWallJump();
        }
        else
        {
            float moveX = Orientation;
            rb.velocity = new Vector2((maxSpeedReachable - 5) * -moveX, jumpForce);
        }
        speedBuffer = 0;
    }

    void PerfectWallJump()
    {
        float moveX = Orientation;
        rb.velocity = new Vector2((speedBuffer + boostPerfectWallJump) * -moveX, jumpForce);
        maxSpeedReachable = speedBuffer + 5;
    }

    void BufferTheSpeed()
    {
        if (isTouchingWall && speedBuffer == 0) // First frame the player touch a wall
        {
            speedBuffer = math.abs(rb.velocity.x); // Keep in memory the speed at which the player hit the wall
            wallTouchTime = Time.time; // Keep in memory the time when the player hit the wall
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