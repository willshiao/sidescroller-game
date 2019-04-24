using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombCrate : MonoBehaviour
{
    //Constants (tweakables)
    public static readonly int BOMBCRATE_BASE_HEALTH = 3;

    // Working trackers and variables
    int bcHealth;

    // Start is called before the first frame update
    void Start()
    {
        bcHealth = BOMBCRATE_BASE_HEALTH;
    }

    // Update is called once per frame
    void Update()
    {
        if (bcHealth <= 0)
        {
            GetComponent<Animator>().SetBool("busted", true);  // triggers explosion animation
            GetComponent<Transform>().rotation = new Quaternion(0, 0, 0, 0); // fix object to be upright for explosion animation
            GetComponent<BoxCollider2D>().enabled = false;    // turn off bomb crate collider
            GetComponent<Rigidbody2D>().gravityScale = 0; // prevent object from moving

            Destroy(gameObject, 1.0f);                       // destroy game object shortly after                    
        }

    }

    public void TakeHit(int dmg) // You know what that means...
    {
        if (bcHealth > 0)
        {
            //bcHealth -= dmg;
            bcHealth--; //just 3 hits to 'splode it
        }

    }
}
