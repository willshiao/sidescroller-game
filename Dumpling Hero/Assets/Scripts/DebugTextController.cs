using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTextController : MonoBehaviour
{
    public string playerModsText;
    Text debugText;

    // Start is called before the first frame update
    void Start()
    {
        debugText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        playerModsText = "Moves Unlocked:\n";
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>().GetBool("spinAttackUnlocked"))
        {
            playerModsText = playerModsText + "SpinAttack\n";
        }

        if (playerModsText == "Moves Unlocked:\n")
        {
            playerModsText = playerModsText + "none";
        }

        debugText.text = playerModsText;
    }
}
