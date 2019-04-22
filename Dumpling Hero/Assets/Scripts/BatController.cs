using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatController : MonoBehaviour
{

    // Animator & body colliders
    Animator batAnimator;
    PolygonCollider2D[] batBodies;

    // Combat Text object
    public GameObject CBTprefab;

    // Friendly Layer  Mask
    LayerMask friendlyLayer;

    // Score Controller
    ScoreController sc;

    // Player Transform (to track)
    Transform playerTransform;

    // Bat's current stats
    int batHealth;
    float batSpeed;

    // Working trackers and variables
    int framesToRandoMove;
    float batRandoMoveCooldownTime;
    float batAttackCooldownTime;
    float batStunTimer;
    float batChangeDirectionTimer;

    // Bat Tweakables
    public static readonly float BAT_BASE_MOVE_SPEED = 0.8F;
    public static readonly int   BAT_BASE_HEALTH_POOL = 10;
    public static readonly float BAT_DETECT_PROXIMITY = 3.0F;
    public static readonly float BAT_ATTACK_PROXIMITY = 0.2F;
    public static readonly float BAT_MELEE_RANGE = 0.15F;
    public static readonly float BAT_STUN_TIME = 0.5F;
    public static readonly float BAT_STUN_MOVE_SPEED = 0.1F;
    public static readonly float BAT_ATTACK_COOLDOWN_TIME = 2.5F;
    public static readonly int[] BAT_BASE_DAMAGE = {1, 3};
    public static readonly float BAT_ATTACK_MOVE_SPEED = 0.4F;
    public static readonly float BAT_WALL_DETECT_RANGE = 0.25F;
    public static readonly float BAT_CHANGE_DIR_TIME = 0.75F;
    public static readonly float BAT_RANDOMOVE_CHANCE = 0.0006F;
    public static readonly float BAT_RANDOMOVE_SECONDS = 0.7F;
    public static readonly float BAT_RANDOMOVE_MAX_COOLDOWN_TIME = 10; // in seconds

    public static readonly int   BAT_POINTS_PER_KILL = 10;

    // Start is called before the first frame update
    void Start()
    {
        batHealth = BAT_BASE_HEALTH_POOL;

        batAnimator = GetComponent<Animator>();
        batBodies = GetComponents<PolygonCollider2D>();

        friendlyLayer = LayerMask.GetMask("Friendly");

        framesToRandoMove = 0;
        batAttackCooldownTime = Time.time;
        batStunTimer = 0;
        batChangeDirectionTimer = 0;

        // Get reference to score updater
        if (GameObject.Find("ScoreText") == null)
        {
            print("Friendly Error: Cannot find ScoreText Game Object (import GameData object from Prefabs)");
        }
        else
        {
            sc = GameObject.Find("ScoreText").GetComponent<ScoreController>();
        }

        // Get reference to player transform
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            print("BatController: Cannot find a Player Game Object (make sure GameObject.Tag = Player)");
        }
        else
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {

        /*-------------------------------*/
        /*           CHECK STATE         */
        /*-------------------------------*/
        // If Bat sprite has change to Genric_Empty, destroy itself
        if (batAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Vanished"))
        {
            batBodies[0].enabled = false;
            batBodies[1].enabled = false;
            batBodies[2].enabled = false;

            sc.UpdateScore(BAT_POINTS_PER_KILL);
            Destroy(gameObject);

            return;
        }

        // If Bat is currently stunned and alive
        if (batHealth > 0 && batStunTimer > Time.time)
        {
            batSpeed = BAT_STUN_MOVE_SPEED * batAnimator.GetInteger("xHeading"); ;
            return;
        }
        else if (batHealth <= 0) // Die, if dead
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

        // If Bat is in attack animation
        if (batAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {

            // wait for the attack to finish
            BatMeleeActive();
            return;
        }

        /*-------------------------------*/
        /*         ACTION CHOICE         */
        /*-------------------------------*/
        if (framesToRandoMove > 0) // Check if Bat is hesitating already
        {
            framesToRandoMove--;
        }
        // Check if Bat is in attack proximity & check attack cooldown
        else if (Vector2.Distance((Vector2)playerTransform.position, (Vector2)transform.position) < BAT_ATTACK_PROXIMITY  && Time.time >= batAttackCooldownTime)
        {
            // Set trigger for animation
            batAnimator.SetTrigger("attack");

            // Init bat attack params
            batSpeed = BAT_ATTACK_MOVE_SPEED * batAnimator.GetInteger("xHeading"); ;
            batAttackCooldownTime = Time.time + BAT_ATTACK_COOLDOWN_TIME;
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
        else 
        {

            // Go idle if out of range of player
            if (Vector2.Distance(playerTransform.position, transform.position) > BAT_DETECT_PROXIMITY)
            {
                batAnimator.SetInteger("xHeading", 0);
            }
            else if (batChangeDirectionTimer <= Time.time) // else change direction if ready
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
                batChangeDirectionTimer = Time.time + BAT_CHANGE_DIR_TIME;
            }

            // Set Bat speed
            batSpeed = batAnimator.GetInteger("xHeading") * BAT_BASE_MOVE_SPEED;

        }

        // Go idle if run into a wall
        if (DetectAWall())
        {
            batAnimator.SetInteger("xHeading", 0);
            batSpeed = 0;
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

            batStunTimer = Time.time + BAT_STUN_TIME;

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

    private bool DetectAWall()
    {
        int heading = batAnimator.GetInteger("xHeading");
        //Vector2 startSpot = (Vector2)transform.position + new Vector2(heading * GetComponent<PolygonCollider2D>().bounds.extents.x, 0.0F);
        Vector2 startSpot = transform.position;
        Debug.DrawRay(startSpot, heading * Vector2.right * BAT_WALL_DETECT_RANGE, Color.blue);
        return Physics2D.Raycast(startSpot, heading * Vector2.right, BAT_WALL_DETECT_RANGE, LayerMask.GetMask("Groundable"));
    }

    private void BatMeleeActive()
    {
        Debug.DrawRay(transform.position, batAnimator.GetInteger("xHeading") * Vector2.right * BAT_MELEE_RANGE, Color.red);
        var raycastMelee = Physics2D.Raycast(transform.position, batAnimator.GetInteger("xHeading") * Vector2.right, BAT_MELEE_RANGE, friendlyLayer);

        if (raycastMelee.collider != null)
        {
            if (raycastMelee.collider.tag == "Player")
            {
                int batDmg = (int)Mathf.Round(Random.Range(BAT_BASE_DAMAGE[0], BAT_BASE_DAMAGE[1]));
                raycastMelee.collider.gameObject.GetComponent<HeroController>().TakeHit(batDmg);
            }
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
