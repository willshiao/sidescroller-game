using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombCrate : MonoBehaviour
{
    //Constants (tweakables)
    public static readonly int   BOMBCRATE_BASE_HEALTH = 3;
    public static readonly int   BOMBCRATE_ENEMY_DAMAGE = 4;
    public static readonly float BOMBCRATE_PERC_DAMAGE = 0.3f;
    public static readonly float EXPLOSION_RADIUS = 0.55f;
    public static readonly Color[] RED_COLOR_SHADES = { Color.white, new Color32(251, 17, 0, 255), new Color32(255, 116, 90, 255)};

    // Working trackers and variables
    int  bcHealth;
    bool destructionProcessed;

    // Start is called before the first frame update
    void Start()
    {
        bcHealth = BOMBCRATE_BASE_HEALTH;
        destructionProcessed = false;
    }

    // Update is called once per frame
    // FixedUpdate() ?? hmmm...
    void Update()
    {
        if (bcHealth <= 0 && !destructionProcessed)
        {
            GetComponent<Animator>().SetBool("busted", true);  // triggers explosion animation
            GetComponent<Transform>().rotation = new Quaternion(0, 0, 0, 0); // fix object to be upright for explosion animation
            GetComponent<BoxCollider2D>().enabled = false;    // turn off bomb crate collider
            GetComponent<Rigidbody2D>().gravityScale = 0; // prevent object from moving

            Explode();

            Destroy(gameObject, 1.0f);                       // destroy game object shortly after       

            destructionProcessed = true;                    // so we only do this if statement once
        }

    }

    public void TakeHit(int dmg)
    {
        if (bcHealth > 0)
        {
            bcHealth--; //just 3 hits to 'splode it

            // Update color shade to be more red after each hit
            gameObject.GetComponent<SpriteRenderer>().color = RED_COLOR_SHADES[bcHealth];

        }

    }
    
    public void Explode()
    {
        var contacts = Physics2D.OverlapCircleAll(transform.position, EXPLOSION_RADIUS);

        // Draw debug circle... somehow...
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, EXPLOSION_RADIUS);
        Debug.DrawRay(transform.position, Vector2.up * EXPLOSION_RADIUS, Color.yellow);

        // Parse Friendlies hit
        for (int i = 0; i < contacts.Length; i++)
        {
            if (contacts[i].tag == "Player")
            {
                contacts[i].GetComponent<HeroController>().TakeHitPerc(BOMBCRATE_PERC_DAMAGE);
            }
            else if (contacts[i].name == "Bat")
            {
                contacts[i].GetComponent<BatController>().TakeHit((int)(BOMBCRATE_ENEMY_DAMAGE));
            }
            else if (contacts[i].tag == "Explodable")
            {
                contacts[i].GetComponent<BombCrate>().TakeHit(1);
            }
        }

    }
}
