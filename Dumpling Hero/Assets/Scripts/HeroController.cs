using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    // Hero animator and body collider
    Animator heroAnimator;
    BoxCollider2D heroBodyCollider;

    // Working trackers and variables
    string lastMoveDir;
    float  distToGround;
    float  heroGroundDetectExtends;
    float  heroWallDetectHeight;
    float  takeHitCooldownTime;
    bool   doubleJumpUsed;
    float  distToWall;
    bool   wallGrabKeyInputValid;

    // Hero current stats
    int heroHealth;

    /* Teakable parameters */
    public static readonly float HERO_BASE_SPEED = 1.6F;
    public static readonly float HERO_ATTACKING_SPEED_PENALTY = 0.25F; // try 4.0F if you wanna be a jedi
    public static readonly float HERO_SWAPPING_SPEED_PENALTY = 0.5F;
    public static readonly float HERO_FREERUN_SPEED_BOOST = 1.4F;
    public static readonly float HERO_JUMP_FORCE = 3.2F;
    public static readonly float HERO_DOUBLEJUMP_FORCE = 3.5F;
    public static readonly int   HERO_BASE_HEALTH = 5;

    public static readonly float HERO_CLIMB_SPEED = 0.8F;
    public static readonly float HERO_MIDAIR_SPEED_PENALTY = 0.62F;

    public static readonly float HERO_TAKEHIT_COOLDOWN_TIME = 0.5F;
    public static readonly float HERO_GROUND_DETECT_RAY_LENGTH = 0.065F;
    public static readonly float HERO_WALL_DETECT_RAY_LENGTH = 0.02F;

    /* 
     * DUMPLING HERO CONTROLS 
     * left arrow  = move left
     * right arrow = move right
     * up arrow    = jump
     * spacebar    = draw sword / attack
     * X           = stow sword / draw sword
     */

    // Start is called before the first frame update
    void Start()
    {
        lastMoveDir = "none";
        takeHitCooldownTime = Time.time;
        doubleJumpUsed = false;
        wallGrabKeyInputValid = true;

        heroHealth = HERO_BASE_HEALTH;

        heroAnimator = GetComponent<Animator>();
        heroBodyCollider = GetComponent<BoxCollider2D>();

        // Set ground sensors based on the width of his box collider
        heroGroundDetectExtends = heroBodyCollider.bounds.extents.x/2.0F; // 0.0376 ish
        heroWallDetectHeight = heroBodyCollider.bounds.extents.y;

        // Init animator params
        heroAnimator.SetBool("moving", false);
        heroAnimator.SetBool("startAttack", false);

        // Set Y extent of collider, use to check if hero is on ground
        distToGround = heroBodyCollider.bounds.extents.y + HERO_GROUND_DETECT_RAY_LENGTH;
        distToWall = heroBodyCollider.bounds.extents.x + HERO_WALL_DETECT_RAY_LENGTH;
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 0;
        bool grounded = IsGrounded();
        bool onWall = IsOnWall();

        if (heroHealth <= 0)
        {
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Dead"))
            {
                heroAnimator.SetTrigger("dead");
            }
            return;
        }

        // Change Hero hew back to white if hero can be hit again
        if (takeHitCooldownTime <= Time.time && gameObject.GetComponent<SpriteRenderer>().color == Color.red)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }

        // If midair and against a wall, climb!
        if (onWall && !grounded)
        {
             gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(gameObject.GetComponent<Rigidbody2D>().velocity.x, 0.0F);
             heroAnimator.SetBool("climbing", true);
             heroAnimator.SetBool("midair", false);
             doubleJumpUsed = false;
             transform.Translate(Vector2.up * HERO_CLIMB_SPEED * Time.deltaTime);
        }
        else 
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

        // Debug Rays for ground detection and wall detection
        Debug.DrawRay(transform.position + (new Vector3(-heroGroundDetectExtends, 0, 0)), Vector2.down * distToGround, Color.green);
        Debug.DrawRay(transform.position + (new Vector3( heroGroundDetectExtends, 0, 0)), Vector2.down * distToGround, Color.green);
        if (transform.rotation.y / 10 == 0)
            Debug.DrawRay(transform.position + (new Vector3(0, heroWallDetectHeight, 0)), Vector2.right * distToWall, Color.blue);
        else
            Debug.DrawRay(transform.position + (new Vector3(0, heroWallDetectHeight, 0)), Vector2.left * distToWall, Color.blue);

        // Apply Jump Force if pressed & grounded
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            if (grounded || onWall) {
                var rigidbody = gameObject.GetComponent<Rigidbody2D>();
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0.0F);
                gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * HERO_JUMP_FORCE, ForceMode2D.Impulse);

                if (onWall && heroAnimator.GetBool("climbing"))
                    // Set this so you can jump from a wall hold
                    wallGrabKeyInputValid = false;

            } 
            else if (!doubleJumpUsed)
            {
                var rigidbody = gameObject.GetComponent<Rigidbody2D>();
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0.0F);
                doubleJumpUsed = true;
                heroAnimator.SetTrigger("doubleJump");
                rigidbody.AddForce(Vector2.up * HERO_DOUBLEJUMP_FORCE, ForceMode2D.Impulse);
            }
        } 

        // Set MOVEMENT animator params based on input
        if (Input.GetKey("right") && Input.GetKey("left"))
        {
            heroAnimator.SetBool("moving", true);
            bool flipX = false;
            if (lastMoveDir == "right")
            {
                speed = -HERO_BASE_SPEED;
                flipX = true;
            }
            else if (lastMoveDir == "left")
            {
                speed = HERO_BASE_SPEED;
            }
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
            {   
                gameObject.transform.SetPositionAndRotation(transform.position, new Quaternion(transform.rotation.x, ((flipX) ? 180 : 0), transform.rotation.z, transform.rotation.w));
                //gameObject.GetComponent<SpriteRenderer>().flipX = flipX;

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
                gameObject.transform.SetPositionAndRotation(transform.position, new Quaternion(transform.rotation.x, 0, transform.rotation.z, transform.rotation.w));
                //gameObject.GetComponent<SpriteRenderer>().flipX = false;
            }
            heroAnimator.SetBool("moving", true);
            speed = HERO_BASE_SPEED;
            lastMoveDir = "right";
        }
        else if (Input.GetKey("left"))
        {
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
            {
                gameObject.transform.SetPositionAndRotation(transform.position, new Quaternion(transform.rotation.x, 180, transform.rotation.z, transform.rotation.w));
                //gameObject.GetComponent<SpriteRenderer>().flipX = true;
            }
            heroAnimator.SetBool("moving", true);
            speed = -HERO_BASE_SPEED;
            lastMoveDir = "left";
        }

        // Set WEAPON USE animator params based on input
        if (Input.GetKey(KeyCode.Space))
        {
            if (!heroAnimator.GetBool("swordEquipped"))
            {
                heroAnimator.SetBool("swordEquipped", true);
            }
            heroAnimator.SetBool("startAttack", true);
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

        // Adjust speed based on hero's current state and Animator parameters
        if (heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            speed = speed * HERO_ATTACKING_SPEED_PENALTY; // decrease speed if attacking
        } 
        else if (heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping") && IsGrounded())
        {
            speed = speed * HERO_SWAPPING_SPEED_PENALTY; // decrease speed when drawing weapons
        }
        else if (!heroAnimator.GetBool("swordEquipped"))
        {
            speed = speed * HERO_FREERUN_SPEED_BOOST; // increase speed if you do not have your sword out
        }

        if (!grounded)
        {
            speed = speed * HERO_MIDAIR_SPEED_PENALTY;
        }

        // Move
        if (heroAnimator.GetBool("moving"))
        {
            transform.Translate(transform.right * speed * Time.deltaTime); // right is onward!
        }
    }

    public void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Item")
        {
            heroHealth = HERO_BASE_HEALTH;
            Destroy(col.gameObject);
            print("Got the steak! Current health = " + heroHealth);
        }
    }

    public void TakeHit(int dmg)
    {
        if (takeHitCooldownTime <= Time.time && heroHealth > 0)
        {
            heroHealth -= dmg;
            print("Hero took " + dmg + " damage! Current Health = " + heroHealth);
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            takeHitCooldownTime = Time.time + HERO_TAKEHIT_COOLDOWN_TIME;
        }
    }

    // Must be holding down corresponding arrow key to be onwall
    public bool IsOnWall()
    {
        bool wallDetected = DetectWall();

        // When wall grab input is invalid, set it back to valid only if hero leaves wall for a moment (otherwise its impossible to jump away from wall with arrow key down)
        if (!wallGrabKeyInputValid)
            wallGrabKeyInputValid = !wallDetected;

        if (transform.rotation.y / 10 == 0)
            return Input.GetKey(KeyCode.RightArrow) && wallGrabKeyInputValid && wallDetected;
        else
            return Input.GetKey(KeyCode.LeftArrow) && wallGrabKeyInputValid && wallDetected;
    }

    public bool DetectWall()
    {
        if (transform.rotation.y / 10 == 0)
            return (Physics2D.Raycast(transform.position + (new Vector3(0, heroWallDetectHeight, 0)), Vector2.right, distToWall, LayerMask.GetMask("Groundable")) ||
                    Physics2D.Raycast(transform.position + (new Vector3(0, -heroWallDetectHeight * 0.88f, 0)), Vector2.right, distToWall, LayerMask.GetMask("Groundable")));
        else
            return (Physics2D.Raycast(transform.position + (new Vector3(0, heroWallDetectHeight, 0)), Vector2.left, distToWall, LayerMask.GetMask("Groundable")) ||
                    Physics2D.Raycast(transform.position + (new Vector3(0, -heroWallDetectHeight * 0.88f, 0)), Vector2.left, distToWall, LayerMask.GetMask("Groundable")));
    }

public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position + (new Vector3( heroGroundDetectExtends, 0, 0)), -Vector2.up, distToGround, LayerMask.GetMask("Groundable")) ||
               Physics2D.Raycast(transform.position + (new Vector3(-heroGroundDetectExtends, 0, 0)), -Vector2.up, distToGround, LayerMask.GetMask("Groundable")) ||
               Physics2D.Raycast(transform.position + (new Vector3( heroGroundDetectExtends, 0, 0)), -Vector2.up, distToGround, LayerMask.GetMask("Movable"))    ||
               Physics2D.Raycast(transform.position + (new Vector3(-heroGroundDetectExtends, 0, 0)), -Vector2.up, distToGround, LayerMask.GetMask("Movable"));
    }

}
