using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlash : MonoBehaviour
{
    public SpriteRenderer heroSR;

    public GameObject hero;

    public PolygonCollider2D[] swordSlashes;
    public int numSwordSlashes;

    // Sword damage Teakables
    const int SWORD_BASE_ATTACK_DMG_MIN =  4;
    const int SWORD_BASE_ATTACK_DMG_MAX = 10;


    // Start is called before the first frame update
    void Start()
    {
        swordSlashes = GetComponents<PolygonCollider2D>();
        numSwordSlashes = swordSlashes.Length;

        // get reference to hero's sprite renderer
        heroSR = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Turn on the correct collider based on current sprite
        string currentSprite = heroSR.sprite.name;
        if (currentSprite == "adventurer_44")
        {
            transform.position = hero.transform.position;
            swordSlashes[0].enabled = true;
        }
        else if (currentSprite == "adventurer_50")
        {
            transform.position = hero.transform.position;
            swordSlashes[1].enabled = true;
        }
        else if (currentSprite == "adventurer_55")
        {
            transform.position = hero.transform.position;
            swordSlashes[2].enabled = true;
        }
        else if (currentSprite == "adventurer_56")
        {
            transform.position = hero.transform.position;
            swordSlashes[3].enabled = true;
        }
        else
        {
            for (int i=0; i<numSwordSlashes; i++)
            {
                swordSlashes[i].enabled = false;
            }
        }
    }

    // When the sword hits something...
    public void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name == "Bat")
        {
            col.gameObject.GetComponent<BatController>().TakeHit(GetSwordDamage());
        }
    }

    public int GetSwordDamage()
    {
        return (int)Random.Range(SWORD_BASE_ATTACK_DMG_MIN, SWORD_BASE_ATTACK_DMG_MAX);
    }

}