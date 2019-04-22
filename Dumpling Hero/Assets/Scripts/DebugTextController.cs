using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTextController : MonoBehaviour
{
    string playerModsText;
    Text debugText;
    Animator heroAnimator;

    // Start is called before the first frame update
    void Start()
    {
        debugText = GetComponent<Text>();

        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            print("DebugTextController: could not find an object with tag = 'Player'");
        }
        else
        {
            heroAnimator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Form the DebugText string
        playerModsText = "Moves Unlocked:\n";
        if (heroAnimator.GetBool("spinAttackUnlocked"))
        {
            playerModsText = playerModsText + "SpinAttack\n";
        }

        // add other stuff to string here...


        if (playerModsText == "Moves Unlocked:\n")
        {
            playerModsText = playerModsText + "none";
        }
        debugText.text = playerModsText;
    }
}
