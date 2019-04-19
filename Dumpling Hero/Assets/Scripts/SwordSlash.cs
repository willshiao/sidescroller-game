using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlash : MonoBehaviour
{

    public SpriteRenderer heroSR;

    public PolygonCollider2D[] swordSlashes;
    public int numSwordSlashes;

    // Enemies
    public BatController bc;

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
        string currentSprite = heroSR.sprite.name;

        if (currentSprite == "adventurer_44")
        {
            swordSlashes[0].enabled = true;
        }
        else if (currentSprite == "adventurer_50")
        {
            swordSlashes[1].enabled = true;
        }
        else if (currentSprite == "adventurer_55")
        {
            swordSlashes[2].enabled = true;
        }
        else if (currentSprite == "adventurer_56")
        {
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

    // This is called when swords hit something
    public void OnCollisionEnter2D(Collision2D col)
    {
        var dmg = Random.Range(SWORD_BASE_ATTACK_DMG_MIN, SWORD_BASE_ATTACK_DMG_MAX);

        if (col.gameObject.name == "Bat")
        {
            col.gameObject.GetComponent<BatController>().TakeHit(dmg);
        }
        // or just have a script called TakeHit() for each enemy

        //if (col.gameObject.name == "???")
        //{

        //}
    }
}