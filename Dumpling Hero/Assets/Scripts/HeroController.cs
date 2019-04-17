using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public Animator heroAnimator;

    private string lastMoveDir;

    /* Teakable parameters */
    private const float HERO_BASE_SPEED = 1.6F;
    private const float HERO_ATTACKING_SPEED_PENALTY = 0.25F; // try 4.0F if you wanna be a jedi
    private const float HERO_SWAPPING_SPEED = 0.1F;
    private const float HERO_FREERUN_SPEED_BOOST = 1.4F;

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

        // Init animator params
        heroAnimator.SetBool("moving", false);
        heroAnimator.SetBool("startAttack", false);
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 0;

        // Set MOVEMENT animator params based on input
        if (Input.GetKey("right") && Input.GetKey("left"))
        {
            heroAnimator.SetBool("moving", true);
            var yAngle = 0.0F;
            if (lastMoveDir == "right")
            {
                speed = -HERO_BASE_SPEED;
                yAngle = 180;
            }
            else if (lastMoveDir == "left")
            {
                speed = HERO_BASE_SPEED;
                yAngle = 0;
            }
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
            {
                gameObject.transform.SetPositionAndRotation(transform.position, new Quaternion(transform.rotation.x, yAngle, transform.rotation.z, transform.rotation.w));
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
        else if (heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
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
}
