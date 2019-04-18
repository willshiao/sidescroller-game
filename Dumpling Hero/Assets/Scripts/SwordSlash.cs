using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlash : MonoBehaviour
{
    public ScoreController sc;
    public SpriteRenderer heroSR;

    public PolygonCollider2D[] swordSlashes;
    public int numSwordSlashes;

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
        if (col.gameObject.tag == "Enemy")
        {
            sc.UpdateScore(10); //update score +10
            Destroy(col.gameObject);
        }
    }
}