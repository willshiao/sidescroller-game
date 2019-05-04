using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroController : MonoBehaviour
{
    // Hero animator and body collider
    Animator heroAnimator;
    BoxCollider2D heroBodyCollider;
    Rigidbody2D heroBody;

    // Hero health bar, scales from 0 - 1.0
    Slider healthBar;

    // Working trackers and variables
    float  heroSpeed;
    string lastMoveDir;
    float  distToGround;
    float  heroGroundDetectExtends;
    float  heroWallDetectHeight;
    float  takeHitCooldownTime;
    bool   doubleJumpUsed;
    float  distToWall;
    bool   wallGrabKeyInputValid;
    float  climbSpeedModifier;
    int    heroHealth;
    bool   grounded, onWall;
    float  raycastCooldownTime;

    /* Teakable parameters */
    public static readonly float HERO_BASE_SPEED = 1.88F;
    public static readonly float HERO_ATTACKING_SPEED_PENALTY = 0.41F;
    public static readonly float HERO_SWAPPING_SPEED_PENALTY = 0.5F;
    public static readonly float HERO_FREERUN_SPEED_BOOST = 1.2F;
    public static readonly float HERO_JUMP_FORCE = 3.2F;
    public static readonly float HERO_DOUBLEJUMP_FORCE = 3.5F;
    public static readonly int   HERO_BASE_HEALTH = 5;

    public static readonly float HERO_BASE_CLIMB_SPEED = 1.0F;
    public static readonly float HERO_CLIMB_FREERUN_SPEED_MOD = 1.5F;
    public static readonly float HERO_MIDAIR_SPEED_PENALTY = 0.67F;

    public static readonly float HERO_TAKEHIT_COOLDOWN_TIME = 0.5F;
    public static readonly float HERO_GROUND_DETECT_RAY_LENGTH = 0.065F;
    public static readonly float HERO_WALL_DETECT_RAY_LENGTH = 0.02F;
    public static readonly float RAYCAST_COOLDOWN_TIME = 0.0046F;

    /* 
     * DUMPLING HERO CONTROLS 
     * left arrow  = move left / climb left wall
     * right arrow = move right / climb right wall
     * up arrow    = jump / double-jump
     * spacebar    = draw sword / attack
     * X           = stow sword / draw sword
     */

    // Start is called before the first frame update
    void Start()
    {
        // Initialize working variables
        lastMoveDir = "none";
        takeHitCooldownTime = Time.time;
        doubleJumpUsed = false;
        wallGrabKeyInputValid = true;
        heroHealth = HERO_BASE_HEALTH;
        raycastCooldownTime = Time.time;
        grounded = false;

        // Get components
        heroAnimator = GetComponent<Animator>();
        heroBodyCollider = GetComponent<BoxCollider2D>();
        heroBody = GetComponent<Rigidbody2D>();
        healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        healthBar.value = 1.0f;

        // Set ground & wall sensors starting point
        heroGroundDetectExtends = heroBodyCollider.bounds.extents.x * 1.0f;
        heroWallDetectHeight = heroBodyCollider.bounds.extents.y * 0.9f;

        // Set ground & wall sensors length
        distToGround = heroBodyCollider.bounds.extents.y + HERO_GROUND_DETECT_RAY_LENGTH;
        distToWall = heroBodyCollider.bounds.extents.x + HERO_WALL_DETECT_RAY_LENGTH;
    }

    // Update is called once per frame
    void Update()
    {

        /* ********************
         * GET PHYSICS PARAMS *
         * ****************** */
        // Check if grounded or on-a-wall ONCE every interval (not every frame)
        if (raycastCooldownTime <= Time.time)
        {
            grounded = IsGrounded();
            onWall = IsOnWall();
            raycastCooldownTime = Time.time + RAYCAST_COOLDOWN_TIME;
        }

        /******************
         * DEATH & DAMAGE *
         * ************** */
        // If hit cooldown finishes, change Hero hew back to white
        if (takeHitCooldownTime <= Time.time && gameObject.GetComponent<SpriteRenderer>().color == Color.red)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
        // If dead, stay dead
        if (heroHealth <= 0)
        {
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Dead"))
            {
                heroAnimator.SetTrigger("dead");
            }
            return;
        }


        /******************
         *    CLIMBING    *
         * ************** */
        // If midair and against a wall, climb!
        if (onWall && !grounded)
        {
            // Set modifier: go faster if sword is stowed
            climbSpeedModifier = HERO_CLIMB_FREERUN_SPEED_MOD;
            if (heroAnimator.GetBool("swordEquipped"))
                climbSpeedModifier = 1.0f;

            heroAnimator.SetBool("climbing", true);
            heroAnimator.SetBool("midair", false);

            doubleJumpUsed = false;
        }
        else // not climbing
        {
            heroAnimator.SetBool("climbing", false);

            // Reset double jump if grounded
            if (grounded)
            {
                doubleJumpUsed = false;
                heroAnimator.SetBool("midair", false);
            }
            else
            {
                heroAnimator.SetBool("midair", true);
            }
        }


        /******************
        *    DEBUG RAYS   *
         * ************** */
        // Debug Rays for ground detection
        Debug.DrawRay(transform.position + (new Vector3(-heroGroundDetectExtends, 0, 0)), Vector2.down * distToGround, Color.green);
        Debug.DrawRay(transform.position + (new Vector3( heroGroundDetectExtends, 0, 0)), Vector2.down * distToGround, Color.green);
        // Debug Rays for wall detection
        if (transform.rotation.y / 10 == 0)
        {
            Debug.DrawRay(transform.position + (new Vector3(0, -heroWallDetectHeight, 0)), new Vector2(1, -0.1f) * distToWall, Color.blue);
            Debug.DrawRay(transform.position + (new Vector3(0, heroWallDetectHeight, 0)), new Vector2(1, 0) * distToWall, Color.blue);
        }
        else
        {
            Debug.DrawRay(transform.position + (new Vector3(0, -heroWallDetectHeight, 0)), new Vector2(-1, -0.1f) * distToWall, Color.blue);
            Debug.DrawRay(transform.position + (new Vector3(0, heroWallDetectHeight, 0)), new Vector2(-1, 0) * distToWall, Color.blue);
        }


        /******************
         *     JUMPING    *
         * ************** */
        // Apply Jump Force if pressed & grounded
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            heroAnimator.SetBool("climbing", false);

            if (grounded) {
                heroBody.velocity = new Vector2(heroBody.velocity.x, 0.0F);
                heroBody.AddForce(Vector2.up * HERO_JUMP_FORCE, ForceMode2D.Impulse);
            } 
            else if (onWall)
            {
                // Set this so you can jump from a wall hold
                wallGrabKeyInputValid = false;
                heroBody.AddForce(Vector2.up * HERO_JUMP_FORCE, ForceMode2D.Impulse);
            }
            else if (!doubleJumpUsed)
            {
                heroBody.velocity = new Vector2(heroBody.velocity.x, 0.0F);
                doubleJumpUsed = true;
                heroAnimator.SetTrigger("doubleJump");
                heroBody.AddForce(Vector2.up * HERO_DOUBLEJUMP_FORCE, ForceMode2D.Impulse);
            }
        }

        /* -------------------------------------------------- */
        // If climbing, the rest of the mechanics are disabled
        if (onWall)
            return;
        /* -------------------------------------------------- */


        /******************
         *    MOVEMENT    *
         * ************** */
        // Set MOVEMENT animator params based on input
        if (Input.GetKey("right") && Input.GetKey("left"))
        {
            heroAnimator.SetBool("moving", true);
            bool flipX = false;
            if (lastMoveDir == "right")
            {
                heroSpeed = -HERO_BASE_SPEED;
                flipX = true;
            }
            else if (lastMoveDir == "left")
            {
                heroSpeed = HERO_BASE_SPEED;
            }
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
            {
                transform.right = new Vector2(((flipX) ? -1 : 1), 0);
            }
        }
        else if (!Input.GetKey("right") && !Input.GetKey("left"))
        {
            heroAnimator.SetBool("moving", false);
        }
        else if (Input.GetKey("right"))
        {
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
            {
                transform.right = new Vector2(1, 0);
            }
            heroAnimator.SetBool("moving", true);
            heroSpeed = HERO_BASE_SPEED;
            lastMoveDir = "right";
        }
        else if (Input.GetKey("left"))
        {
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
            {
                transform.right = new Vector2(-1, 0);
            }
            heroAnimator.SetBool("moving", true);
            heroSpeed = -HERO_BASE_SPEED;
            lastMoveDir = "left";
        }


        /***********************
         *   WEAPONS / ATTACK  *
         * ******************* */
        // Set WEAPON USE animator params based on input
        if (Input.GetKey(KeyCode.Space))
        {
            if (!heroAnimator.GetBool("swordEquipped") && !heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("DoubleJump"))
            {
                heroAnimator.SetBool("swordEquipped", true);
            }
            else if (heroAnimator.GetBool("swordEquipped"))
            {
                heroAnimator.SetBool("startAttack", true);
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
            {
                // Stow Sword or Draw Sword
                heroAnimator.SetBool("swordEquipped", !heroAnimator.GetBool("swordEquipped"));
            }
        }
        else
        {
            heroAnimator.SetBool("startAttack", false);
        }


        /***********************
         *       SET SPEED     *
         * ******************* */
        // Adjust speed based on hero's current state and Animator parameters
        if (heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            heroSpeed = heroSpeed * HERO_ATTACKING_SPEED_PENALTY; // decrease speed if attacking
        }
        else if (heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping") && IsGrounded())
        {
            heroSpeed = heroSpeed * HERO_SWAPPING_SPEED_PENALTY; // decrease speed when drawing weapons
        }
        else if (!heroAnimator.GetBool("swordEquipped"))
        {
            heroSpeed = heroSpeed * HERO_FREERUN_SPEED_BOOST; // increase speed if you do not have your sword out
        }

        if (!grounded)
        {
            heroSpeed = heroSpeed * HERO_MIDAIR_SPEED_PENALTY;
        }

    }

    // Called before all Physics checks!
    public void FixedUpdate()
    {
        // Actually Move the Hero object in here
        if (heroAnimator.GetBool("climbing"))
        {
            //heroBody.velocity = new Vector2(heroBody.velocity.x, 0.0F);
            //transform.Translate(Vector2.up * HERO_BASE_CLIMB_SPEED * climbSpeedModifier * Time.fixedDeltaTime);
            //heroBody.MovePosition(heroBody.position + (Vector2)transform.up * HERO_BASE_CLIMB_SPEED * climbSpeedModifier * Time.fixedDeltaTime);
            heroBody.velocity = new Vector2(0.0f, HERO_BASE_CLIMB_SPEED * climbSpeedModifier);
        }
        else if (heroAnimator.GetBool("moving"))
        {
            //transform.Translate(transform.right * heroSpeed * Time.deltaTime);
            //heroBody.MovePosition(heroBody.position +  (Vector2)transform.right * Mathf.Abs(heroSpeed) * Time.fixedDeltaTime);
            heroBody.velocity = new Vector2(heroSpeed, heroBody.velocity.y);
            
        } 
        else if (!heroAnimator.GetBool("moving") && heroBody.velocity.x != 0)
        {
            // Glide to stop at a controlled rate
            heroBody.velocity = new Vector2(heroBody.velocity.x * 0.7f, heroBody.velocity.y);
        }
    }

    // Called when hero body collides with object (col)
    public void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Item")
        {
            heroHealth = HERO_BASE_HEALTH;
            Destroy(col.gameObject);
            UpdateHealthBar();
        }
    }

    // Called when hero is damaged or healed
    public void UpdateHealthBar()
    {
        healthBar.value = ((float)heroHealth / (float)HERO_BASE_HEALTH);
    }

    // Called by other objects that can damage the hero
    public void TakeHit(int dmg)
    {
        if (takeHitCooldownTime <= Time.time && heroHealth > 0)
        {
            heroHealth -= dmg;
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            UpdateHealthBar();
            takeHitCooldownTime = Time.time + HERO_TAKEHIT_COOLDOWN_TIME;
        }
    }

    // Called by other objects that can inflict percentage damage on the hero
    public void TakeHitPerc(float perc)
    {
        if (heroHealth == 1)
            TakeHit(1);
        else if (heroHealth > 1)
            TakeHit((int)Mathf.Round((float)heroHealth * perc));
    }

    // Called once per frame in update function
    public bool IsOnWall()
    {
        bool wallDetected = DetectWall();

        /* When wall grab input is invalid, set it back to valid only if hero leaves wall for a moment 
         * this makes it possible for jump to interupt climbing */
        if (!wallGrabKeyInputValid)
            wallGrabKeyInputValid = !wallDetected;

        // Must be walking into wall to be considered "on-a-wall"
        if (transform.rotation.y / 10 == 0)
            return Input.GetKey(KeyCode.RightArrow) && wallGrabKeyInputValid && wallDetected;
        else
            return Input.GetKey(KeyCode.LeftArrow) && wallGrabKeyInputValid && wallDetected;
    }

    // Called once per frame to detect a wall in front of the hero
    public bool DetectWall()
    {
        // if facing right ( '/ 10' in case y rotation is close to 0 but not 0)
        if (transform.rotation.y / 10 == 0)
        {
            return (Physics2D.Raycast(transform.position + (new Vector3(0, heroWallDetectHeight, 0)), new Vector2(1, -0.1f), distToWall, LayerMask.GetMask("Groundable")) ||
                    Physics2D.Raycast(transform.position + (new Vector3(0, -heroWallDetectHeight, 0)), new Vector2(1, 0), distToWall, LayerMask.GetMask("Groundable")));
        }
        else // facing left (y rotation ~= 180)
        {
            return (Physics2D.Raycast(transform.position + (new Vector3(0, heroWallDetectHeight, 0)), new Vector2(-1, 0), distToWall, LayerMask.GetMask("Groundable")) ||
                    Physics2D.Raycast(transform.position + (new Vector3(0, -heroWallDetectHeight, 0)), new Vector2(-1, -0.1f), distToWall, LayerMask.GetMask("Groundable")));
        }
    }

    // Called once per frame in update function
    public bool IsGrounded()
    {
        RaycastHit2D rHit1 = Physics2D.Raycast(transform.position + (new Vector3( heroGroundDetectExtends, 0, 0)), -Vector2.up, distToGround, LayerMask.GetMask("Groundable") | LayerMask.GetMask("Movable"));
        RaycastHit2D rHit2 = Physics2D.Raycast(transform.position + (new Vector3(-heroGroundDetectExtends, 0, 0)), -Vector2.up, distToGround, LayerMask.GetMask("Groundable") | LayerMask.GetMask("Movable"));
        float colliderEndY = transform.position.y - heroBodyCollider.bounds.extents.y;

        //print("rHit1.point = " + rHit1.point + ", (thr = " + colliderEndY + ")");
        //print("rHit2.point = " + rHit2.point + ", (thr = " + colliderEndY + ")");

        return (rHit1.collider != null && rHit1.point.y <= colliderEndY) || (rHit2.collider != null && rHit2.point.y <= colliderEndY);
    }

}
