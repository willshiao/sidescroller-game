using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public GameObject hero;

    // Start is called before the first frame update
    void Start()
    {
        hero = GameObject.FindGameObjectWithTag("Player");

        // Set unlocks and modifiers
        hero.GetComponent<Animator>().SetBool("spinAttackUnlocked", true);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
