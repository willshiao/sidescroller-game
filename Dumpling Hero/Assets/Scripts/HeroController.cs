using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public Animator heroAnimator;

    public BoxCollider2D heroBodyCollider;

    private string lastMoveDir;

    public LayerMask groundLayer;

    float distToGround;
    float takeHitCooldownTime;

    /* Teakable parameters */
    private const float HERO_BASE_SPEED = 1.6F;
    private const float HERO_ATTACKING_SPEED_PENALTY = 0.25F; // try 4.0F if you wanna be a jedi
    private const float HERO_SWAPPING_SPEED = 0.1F;
    private const float HERO_FREERUN_SPEED_BOOST = 1.4F;
    private const float HERO_JUMP_FORCE = 3.2F;

    public static readonly float HERO_TAKEHIT_COOLDOWN_TIME = 0.5F;

    private const float HERO_GROUND_DETECT_RAY_LENGTH = 0.08F;

    /* 
     * DUMPLING HERO CONTROLS 
     * left arrow  = move left
     * right arrow = move right
     * spacebar    = draw sword / attack
     * X           = stow sword / draw sword
     */

    // Start is called before the first frame update
    void Start()
    {
        // Don't need this line since it is set in Unity via drag-and-drop
        //heroAnimator = gameObject.GetComponent<Animator>();
        lastMoveDir = "none";
        takeHitCooldownTime = Time.time;

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

        // Apply Jump Force if pressed & grounded
        Debug.DrawRay(transform.position, Vector2.down * distToGround, Color.green);
        if (Input.GetKeyDown(KeyCode.UpArrow) && IsGrounded())
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * HERO_JUMP_FORCE, ForceMode2D.Impulse);
        }

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
            print("Got the steak!");
            Destroy(col.gameObject);
        }
    }

    public void TakeHit(int dmg)
    {
        if (takeHitCooldownTime <= Time.time)
        {
            print("Hero took " + dmg + "damage!");
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            takeHitCooldownTime = Time.time + HERO_TAKEHIT_COOLDOWN_TIME;
        }
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, -Vector2.up, distToGround, groundLayer);
    }

}
