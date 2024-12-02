using System;
using System.Collections;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField]
    private Animator hudDashAnimator;
    [SerializeField]
    private Animator hudAttackAnimator;
    [SerializeField]
    private float Orientation = 1; // The X scale factor for the player. -1 when faceing left, 1 when facing right

    [Header("Jump")]
    public float jumpHeight = 20f;                  // Hight of a jump
    public float jumpRatio = 10f;                   // True force of the jump
    public float airControlFactor = 0.002f;         // Amount of control in the air (0 = no control, 1 = full control)
    public bool isGrounded;                         // 1 when on the ground. 0 when not
    private float jumpForce;                        // Variable for unity to use

    [Header("Speed")]
    public float maxSpeed;                          // Max speed reachable right now. Will change a lot (every time you do something to accelerate)
    public float acceleration;                      // "Speed" at wich you reach your maximum speed
    private float speedBuffer = 0;                  // Use to keep in memory a speed
    private float initialMaxSpeed = 20f;
    public float boostPerfectWallJump = 2f;
    public float termialFallingSpeed = -20f;
    public float ultimateMaxSpeed = 100f;           // The maxSpeed max
    public float FLOW = 0f;                         // FLOW of the player

    [Header("JumpWall")]
    public bool isTouchingWall;                     // If the player is touching a wall
    private float wallTouchTime;                    // Last time the wall was touch
    public float wallJumpWindow = 3f;               // Time you have to do a perfect wall jump
    public float checkRadius = 0.2f;                // Radius of the circle for wall detection
    public LayerMask environementLayer;             // The layer that represents walls and
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

    [Header("Dodge")]
    public bool canDodge = true;
    public bool isDodging = false;
    public bool isDamaged = false;
    public float damageTimer = 0f;
    public float damageTime = 1.5f;
    public float dodgeCD = 2f;
    public float dodgeDuration = 0.5f;
    public float dodgeTime = 0f;
    public float dodgingTime = 0f;
    public float dodgeSpeedIncrement = 5f;

    [Header("Attack")]
    public bool canAttack = false;                  //Boolean to know if the player can attack
    public float attackCD = 1f;                     //Cooldown of the attack
    private float curAttackCD = 0f;                 //Timer until next attack
    public float attackDistance = 5f;               //Maximum distance where the player can attach
    public bool isAttacking = false;

    [Header("Enemies")]
    public LayerMask enemyLayer;                    // Layer mask for enemies
    private GameObject nearestEnemy;                // Nearest enemy detected
    public float damageEjectionPower = 50;

    [Header("ToSort")]
    public float timeGrounded = 0f;
    public float timeNotGrounded = 0f;
    public float coyoteMaxTime = 0.1f; // Max coyote time in seconds. Timing when the player is still able to jump even if not grounded
    public float jumpWindow = 0.1f; // Time you have to do a perfect jump
    public float perfectJumpSpeedGain = 2f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        maxSpeed = initialMaxSpeed; //Max speed init
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

    private void UpdateFLOW()
    {
        FLOW = maxSpeed;
    }

    void UpdateDamage()
    {
        damageTimer = UpdateTimer(damageTimer);
        if (damageTimer == 0)
        {
            isDamaged = false;
        }
    }

    void UpdateDodge()
    {
        dodgeTime = UpdateTimer(dodgeTime);
        dodgingTime = UpdateTimer(dodgingTime);
        if (dodgeTime == 0) canDodge = true;
        if (dodgingTime == 0) isDodging = false;
    }

    float UpdateTimer(float timer)
    {
        timer -= Time.deltaTime;
        if (timer <= 0) 
        {
            timer = 0;
        }
        return timer;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        
        updateGroundedOrNotTime();
        UpdateDodge();
        if (isDamaged)
        {
            UpdateDamage();
        }
        else
        {
            isTouchingWall = IsTouchingWall();
            isGrounded = IsGrounded();
            HandleDash();
            if (!isDashing)
            {
                BufferTheSpeed();
                HandleMovement();
                HandleJump();
                DetectNearestEnemy();
                HandleAttack();
            }
        }
        Flip();
        HandleAnimators();
        UpdateFLOW();
    }

    void updateGroundedOrNotTime()
    {
        if (isGrounded)
        {
            timeGrounded += Time.deltaTime;
            timeNotGrounded = 0;
        }
        else
        {
            timeNotGrounded += Time.deltaTime;
            timeGrounded = 0;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player is dashing and collides with an enemy
        if (isAttacking && collision.gameObject.CompareTag("Enemy"))
        {
            isAttacking = false;
            Destroy(collision.gameObject); // Destroy the enemy
        }
    }

    private void HandleAnimators()
    {
        //Debug.Log(Mathf.Abs(rb.velocity.x) + " " + Mathf.Abs(rb.velocity.y) + " " + isGrounded);
        animator.SetFloat("XSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("YSpeed", Mathf.Abs(rb.velocity.y));
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isTouchingWall", isTouchingWall);
        animator.SetBool("isDashing", isDashing);
        animator.SetBool("isAttacking", isAttacking);

        hudDashAnimator.SetFloat("DashCD",(1-(curDashCD/dashCD))*100);
        hudAttackAnimator.SetFloat("AttackCD",(1-curAttackCD/attackCD)*100);
    }

    private void Flip()
    {
        if (rb.velocity.x > 0.1f) Orientation = 1;
        else if (rb.velocity.x < -0.1f) Orientation = -1;

        Vector3 newScale = transform.localScale;
        newScale.x = Orientation;
        transform.localScale = newScale;
    }

    private bool CanDash()
    {
        return curDashCD == 0;
    }

    private void HandleDash()
    {
        if (curDashCD > 0) curDashCD = UpdateTimer(curDashCD);
        if (CanDash() && (Input.GetMouseButtonDown(0)))
        {
            // Get one of the 8 direction possible depending on ZQSD/WASD
            Vector2 direction = new Vector2(
                Input.GetAxis("Horizontal") > 0 ? 1 : Input.GetAxis("Horizontal") < 0 ? -1 : 0,
                Input.GetAxis("Vertical") > 0 ? 1 : Input.GetAxis("Vertical") < 0 ? -1 : 0
                );
            if (direction == Vector2.zero) direction = (Orientation == 1) ? Vector2.right : Vector2.left;
            StartCoroutine(DashCoroutine(direction, dashRange, dashTime)); // Dash
            maxSpeed += 5;
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
        else if (Input.GetMouseButtonDown(1))
        {
            if (nearestEnemy != null)
            {
                Vector2 enemyPos = nearestEnemy.transform.position;
                isAttacking = true;
                float playerSpeed = rb.velocity.magnitude;
                Vector2 direction = (enemyPos - (Vector2)transform.position).normalized;
                rb.velocity = direction * (playerSpeed + 5);
                StartCoroutine(DashCoroutine(enemyPos - (Vector2)transform.position, dashRange, dashTime));
                maxSpeed += 10;
                canAttack = false;
                nearestEnemy = null;
            }
        }
    }

    public Vector2 GetValidTargetPosition(Vector2 startPosition, Vector2 direction, float distance)
    {
        // Calculate the initial target position
        Vector2 targetPosition = startPosition + direction.normalized * distance;
        Debug.DrawLine(startPosition, targetPosition, Color.red, 1f);

        // Perform a raycast from the start to the target position
        RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, distance, environementLayer);

        if (hit.collider != null)
        {
            // If the ray hits a wall, adjust the target position
            targetPosition = hit.point - direction.normalized * 0.6f;
        }
        
        /*
        float searchRadius = dashRange; // Rayon de recherche
        float stepSize = 0.5f; // Taille des pas pour la recherche
        float closestDistance = Mathf.Infinity; // Initialisation de la distance minimale
        Vector2 closestEmptyPoint = targetPosition; // Initialisation du point valide

        // Parcourir une grille autour de la position cible
        for (float x = -searchRadius; x <= searchRadius; x += stepSize)
        {
            for (float y = -searchRadius; y <= searchRadius; y += stepSize)
            {
                Vector2 testPoint = targetPosition + new Vector2(x, y);

                // Vérifiez si le point est vide
                if (IsPointEmpty(testPoint))
                {
                    float d = Vector2.Distance(targetPosition, testPoint);
                    if (d < closestDistance)
                    {
                        closestDistance = distance;
                        closestEmptyPoint = testPoint;
                    }
                }
            }
        }
        

        return closestEmptyPoint;
        */
        return targetPosition;
    }

    private bool IsPointEmpty(Vector2 point)
    {
        // Vérifier s'il y a un collider dans "EnvironmentLayer" à cet endroit
        Collider2D hit = Physics2D.OverlapPoint(point, environementLayer);
        return hit == null; // Le point est vide s'il n'y a pas de collision
    }

    private IEnumerator DashCoroutine(Vector2 direction, float distance, float dashTime)
    {
        if (!isAttacking)
        {
            isDashing = true;
            curDashCD = dashCD;
        }
        
        Vector2 bufferSpeed = rb.velocity; // Remember the speed of the player before the dash
        rb.velocity = new Vector2(0, 0);

        Vector2 startPosition = transform.position;
        Vector2 targetPosition;

        targetPosition = GetValidTargetPosition(startPosition, direction, distance);
        Debug.DrawLine(startPosition, targetPosition, Color.green, 1f);

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
        isAttacking = false;
    }

    private void DetectNearestEnemy()
    {
        // Detect all enemies within the detection radius
        Collider2D[] detectedEnemies = Physics2D.OverlapCircleAll(transform.position, attackDistance, enemyLayer);

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
                    float distanceToEnemy = Vector2.Distance(transform.position, enemy.transform.position);
                    if (distanceToEnemy < shortestDistance)
                    {
                        shortestDistance = distanceToEnemy;
                        nearestEnemy = enemy.gameObject;
                    }
                }
            }
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // Get -1, 1 or 0 depending on Q, D or nothing pressed

        float lastSpeed = math.abs(rb.velocity.x);

        if (isGrounded)
        {
            rb.gravityScale = 7;
            float newXSpeed = rb.velocity.x + moveX * acceleration; // Increasing gradualy the speed depending on your acceleration
            newXSpeed = math.clamp(newXSpeed, maxSpeed * -1, maxSpeed); // Making sure you don't exceed your maximum speed
            if (isTouchingWall && newXSpeed * Orientation > 0) newXSpeed = 0;
            rb.velocity = new Vector2(newXSpeed, rb.velocity.y); // Updating the speed
        }
        else // If in the air
        {
            rb.gravityScale = 7;
            float newXSpeed = rb.velocity.x + moveX * acceleration * airControlFactor; // Increasing gradualy the speed depending on your acceleration. The acceleration is diminued by airControlFactor cause the player is in the air
            newXSpeed = math.clamp(newXSpeed, maxSpeed * -1, maxSpeed); // Making sure you don't exceed your maximum speed
            if (isTouchingWall && newXSpeed * Orientation > 0) newXSpeed = 0;
            rb.velocity = new Vector2(newXSpeed, Mathf.Max(rb.velocity.y, termialFallingSpeed)); // Updating the speed
            //rb.velocity = new Vector2(newXSpeed, termialFallingSpeed);
        }

        UpdatemaxSpeed(math.abs(rb.velocity.x) > lastSpeed);
    }

    void UpdatemaxSpeed(bool isAccelerationg)
    {
        if (!isAccelerationg)
        {
            // Decrease the maxSpeed depending on the curent speed
            if (rb.velocity.x * Orientation < maxSpeed && maxSpeed > initialMaxSpeed)
            {
                maxSpeed -= Time.deltaTime * 5;
            }
            if (maxSpeed < initialMaxSpeed) maxSpeed = initialMaxSpeed;
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && (timeNotGrounded <= coyoteMaxTime) && !isTouchingWall) // If space pressed and player on the ground
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
        if (timeGrounded <= jumpWindow)
        {
            rb.velocity = new Vector2(rb.velocity.x + perfectJumpSpeedGain, jumpForce);
            maxSpeed += perfectJumpSpeedGain;
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
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
            rb.velocity = new Vector2((maxSpeed - 5) * -moveX, jumpForce);
        }
        speedBuffer = 0;
    }

    void PerfectWallJump()
    {
        float moveX = Orientation;
        rb.velocity = new Vector2((speedBuffer + boostPerfectWallJump) * -moveX, jumpForce);
        maxSpeed = speedBuffer + 10;
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

    public void HitByProjectile(Vector2 projectile)
    {
        if (isDodging)
        {
            IncreaseSpeed(dodgeSpeedIncrement);
        }
        else TakeDamage(projectile);
    }

    void TakeDamage(Vector2 projectile)
    {
        Vector2 pushBack = (rb.position - projectile).normalized;
        if (isGrounded) pushBack.y *= -1;

        rb.velocity = pushBack * damageEjectionPower;
        isDamaged = true;
        if (!isDamaged) damageTimer = damageTime;
    }

    void IncreaseSpeed(float increment)
    {
        Vector2 speed = rb.velocity;
        float magnitude = speed.magnitude;
        rb.velocity = speed.normalized * (magnitude + increment);
    }

    public void Respawn(Vector2 respawnPoint)
    {
        Teleport(respawnPoint);
        rb.velocity = Vector2.zero; // Set all speeds to 0
        maxSpeed = initialMaxSpeed;
    }

    public void Teleport(Vector2 coords)
    {
        transform.position = coords; // Teleport the player to the point
    }

    public void increaseMaxSpeed(float deltaSpeed)
    {
        maxSpeed += deltaSpeed;
        updateAcceleration();
    }

    void updateAcceleration()
    {
        acceleration = maxSpeed / 10; // If high speed, it's still reachable
    }
}
