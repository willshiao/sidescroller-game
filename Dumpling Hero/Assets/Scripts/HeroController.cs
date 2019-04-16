using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public Animator heroAnimator;

    private string lastMoveDir;

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
                speed = -3.0F;
                yAngle = 180.0F;
            }
            else if (lastMoveDir == "left")
            {
                speed = 3.0F;
                yAngle = 0.0F;
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
                gameObject.transform.SetPositionAndRotation(transform.position, new Quaternion(transform.rotation.x, 0.0F, transform.rotation.z, transform.rotation.w));
            }
            heroAnimator.SetBool("moving", true);
            speed = 3.0F;
            lastMoveDir = "right";
        }
        else if (Input.GetKey("left"))
        {
            if (!heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
            {
                gameObject.transform.SetPositionAndRotation(transform.position, new Quaternion(transform.rotation.x, 180.0F, transform.rotation.z, transform.rotation.w));
            }
            heroAnimator.SetBool("moving", true);
            speed = -3.0F;
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
            speed = speed / 4.0F; // decrease speed if attacking
        } 
        else if (heroAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Swapping"))
        {
            speed = 0.1F; // super decrease speed when drawing weapons ??
        }
        else if (!heroAnimator.GetBool("swordEquipped"))
        {
            speed = speed * 1.3F; // increase speed if you do not have your sword out
        }
        if (heroAnimator.GetBool("moving"))
        {
            transform.Translate(transform.right * speed * Time.deltaTime); // right is onward!
        }
    }
}
