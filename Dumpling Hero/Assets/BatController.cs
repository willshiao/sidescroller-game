using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatController : MonoBehaviour
{
    public Animator batAnimator;

    public PolygonCollider2D[] batBodies;

    // Start is called before the first frame update
    void Start()
    {
        batBodies = GetComponents<PolygonCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {

        // Handle movement decisions here
        // Set Animator params to choose which animation to play:
        //      batAnimator.SetBool("flyingRight", true|false);
        //      batAnimator.SetBool("flyingLeft", true|false);
        //      batAnimator.SetBool("deadRight", true|false);
        //      batAnimator.SetBool("deadLeft", true|false);
        //      if all of these are false, bat will Fly_Idle




        // Enable the correct bat body collider (only 3 for now)
        if (batAnimator.GetBool("flyingRight"))
        {
            batBodies[0].enabled = true;
            batBodies[1].enabled = false;
            batBodies[2].enabled = false;
        }
        else if (batAnimator.GetBool("flyingLeft"))
        {
            batBodies[2].enabled = true;
            batBodies[0].enabled = false;
            batBodies[1].enabled = false;
        }
        else
        {
            batBodies[1].enabled = true;
            batBodies[0].enabled = false;
            batBodies[2].enabled = false;
        }
    }
}
