using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatController : MonoBehaviour
{
    public Animator batAnimator;
    public PolygonCollider2D[] batBodies;

    // Score Controller
    public ScoreController sc;

    // Player Transform
    public Transform playerTransform;

    // Combat Text object
    public GameObject CBTprefab;

    // Bat's current stats
    public int batHealth;
    public float batSpeed;

    // Counter of how many frames to stay in 'Hesitate'
    public int framesToRandoMove;

    private float batRandoMoveCooldownTime;

    // Bat Tweakables
    const float BAT_BASE_MOVE_SPEED = 0.8F;
    const int BAT_BASE_HEALTH_POOL = 10;
    const float BAT_DETECT_PROXIMITY = 3.0F;
    const float BAT_ATTACK_PROXIMITY= 0.5F;
    const float BAT_RANDOMOVE_CHANCE = 0.0001F;
    const float BAT_RANDOMOVE_SECONDS = 0.7F;
    const float BAT_RANDOMOVE_MAX_COOLDOWN_TIME = 5; // in seconds
    const int BAT_POINTS_PER_KILL = 10;

    // Start is called before the first frame update
    void Start()
    {
        batHealth = BAT_BASE_HEALTH_POOL;
        batBodies = GetComponents<PolygonCollider2D>();
        framesToRandoMove = 0;
    }

    // Update is called once per frame
    void Update()
    {

        // If Bat sprite has change to Genric_Empty, destroy itself
        if (batAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Vanished"))
        {
            sc.UpdateScore(BAT_POINTS_PER_KILL);
            Destroy(gameObject);
        }

        /*-------------------------------*/
        /*         ACTION CHOICE         */
        /*-------------------------------*/
        if (batHealth <= 0) // Die, if dead
        {
            if (batAnimator.GetInteger("xHeading") > 0)
            {
                batAnimator.SetBool("deadRight", true);
            }
            else
            {
                batAnimator.SetBool("deadLeft", true);
            }
            batSpeed = 0;
            return;
        }
        else if (framesToRandoMove > 0) // Check if Bat is hesitating already
        {
            framesToRandoMove--;
        }
        else if (Vector2.Distance((Vector2)playerTransform.position, (Vector2)transform.position) < BAT_ATTACK_PROXIMITY) // Check if Bat is in attack proximity
        {
            // charge in direction of player until Bat is out of range
            batSpeed = batAnimator.GetInteger("xHeading") * BAT_BASE_MOVE_SPEED * 2.5F; //speed boost = attack, i guess
        }
        else if (Time.time >= batRandoMoveCooldownTime && Random.Range(0, 1) < BAT_RANDOMOVE_CHANCE) // Roll chance to go random direction
        {
            float r = Mathf.Round(Random.Range(0, 3));
            var ri = 1;
            if (r < 1)
            {
                ri = -1;
            }
            else if (r < 2)
            {
                ri = 0;
            }
            batAnimator.SetInteger("xHeading", ri);
            batSpeed = ri * BAT_BASE_MOVE_SPEED;

            // Start frame counter for hesitate
            framesToRandoMove = (int)(BAT_RANDOMOVE_SECONDS / Time.deltaTime);

            // begin Rando Move cooldown timer
            batRandoMoveCooldownTime = Time.time + Random.Range(0.1F, 1.0F) * BAT_RANDOMOVE_MAX_COOLDOWN_TIME;

        }
        else // Go towards player if player is in range
        {

            if (Vector2.Distance(playerTransform.position, transform.position) > BAT_DETECT_PROXIMITY)
            {
                batAnimator.SetInteger("xHeading", 0);
            }
            else
            {
                if (IsLeftOrRightOfMe(playerTransform) == 'L')
                {
                    batAnimator.SetInteger("xHeading", -1);
                }
                else if (IsLeftOrRightOfMe(playerTransform) == 'R')
                {
                    batAnimator.SetInteger("xHeading", 1);
                }
                else
                {
                    batAnimator.SetInteger("xHeading", 0);
                }
            }

            // Set Bat speed
            batSpeed = batAnimator.GetInteger("xHeading") * BAT_BASE_MOVE_SPEED;

        }

        // Enable the correct bat body collider
        if (batAnimator.GetInteger("xHeading") > 0)
        {
            batBodies[0].enabled = true;
            batBodies[1].enabled = false;
            batBodies[2].enabled = false;
        }
        else if (batAnimator.GetInteger("xHeading") < 0)
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

        // Move the Bat
        transform.Translate(transform.right * batSpeed * Time.deltaTime);

    }

    public void TakeHit(int damage)
    {
        if (batHealth > 0)
        {
            // Take damage
            batHealth = batHealth - damage;

            // Instantiate Combat numbers prefab
            GameObject temp = Instantiate(CBTprefab) as GameObject;
            RectTransform tempRect = temp.GetComponent<RectTransform>();

            // Attach to Bat's CombatText Canvas
            temp.transform.SetParent(transform.Find("CombatTextCanvas"));

            // Set local position within canvas
            tempRect.transform.localPosition = CBTprefab.transform.localPosition;
            tempRect.transform.localScale = CBTprefab.transform.localScale;
            tempRect.transform.localRotation = CBTprefab.transform.localRotation;

            // Set damage string
            temp.GetComponent<Text>().text = damage.ToString();
        }
    }

    /*
    // When the bat hits something...
    public void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name == "SwordSlashes")
        {
            TakeHit(col.gameObject.GetComponent<SwordSlash>().GetSwordDamage());
        }
    }
    */

    public char IsLeftOrRightOfMe(Transform target)
    {
        return (transform.position.x > target.position.x) ? 'L' : 'R';
    }
}
