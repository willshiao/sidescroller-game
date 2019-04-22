using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    // Ground Layer Mask
    public LayerMask groundLayer;

    // Hero animator and body collider
    Animator heroAnimator;
    BoxCollider2D heroBodyCollider;

    // Working trackers and variables
    string lastMoveDir;
    float distToGround;
    float takeHitCooldownTime;
    
    // Hero current stats
    int heroHealth;

    /* Teakable parameters */
    public static readonly float HERO_BASE_SPEED = 1.6F;
    public static readonly float HERO_ATTACKING_SPEED_PENALTY = 0.25F; // try 4.0F if you wanna be a jedi
    public static readonly float HERO_SWAPPING_SPEED = 0.1F;
    public static readonly float HERO_FREERUN_SPEED_BOOST = 1.4F;
    public static readonly float HERO_JUMP_FORCE = 3.2F;
    public static readonly int   HERO_BASE_HEALTH = 5;
    public static readonly float HERO_TAKEHIT_COOLDOWN_TIME = 0.5F;
    public static readonly float HERO_GROUND_DETECT_RAY_LENGTH = 0.08F;

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

        heroHealth = HERO_BASE_HEALTH;

        heroAnimator = GetComponent<Animator>();
        heroBodyCollider = GetComponent<BoxCollider2D>();

        // Init animator params
        heroAnimator.SetBool("moving", false);
        heroAnimator.SetBool("startAttack", false);

        // Set Y extent of collider, use to check if hero is on ground
        distToGround = heroBodyCollider.bounds.extents.y + HERO_GROUND_DETECT_RAY_LENGTH;
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 0;

        if (heroHealth <= 0)
        {
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Dead"))
            {
                heroAnimator.SetTrigger("dead");
            }
            return;
        }

        // Apply Jump Force if pressed & grounded
        Debug.DrawRay(transform.position, Vector2.down * distToGround, Color.green);
        if (Input.GetKeyDown(KeyCode.UpArrow) && IsGrounded())
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * HERO_JUMP_FORCE, ForceMode2D.Impulse);
        }

        // Change Hero hew back to white if hero can be hit again
        if (takeHitCooldownTime <= Time.time && gameObject.GetComponent<SpriteRenderer>().color == Color.red)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
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

        // Move hero based on animator params
        if (heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            speed = speed * HERO_ATTACKING_SPEED_PENALTY; // decrease speed if attacking
        } 
        else if (heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping") && IsGrounded())
        {
            speed = HERO_SWAPPING_SPEED; // super decrease speed when drawing weapons ??
        }
        else if (!heroAnimator.GetBool("swordEquipped"))
        {
            speed = speed * HERO_FREERUN_SPEED_BOOST; // increase speed if you do not have your sword out
        }
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

    public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, -Vector2.up, distToGround, groundLayer);
    }

}
