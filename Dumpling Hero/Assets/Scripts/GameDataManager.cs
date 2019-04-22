using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    string currentLevelScenePath;
    GameObject hero;

    // Start is called before the first frame update
    void Start()
    {
        // Set unlocks and modifiers
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hero = GameObject.FindGameObjectWithTag("Player");
            hero.GetComponent<Animator>().SetBool("spinAttackUnlocked", true);
        }
        else
        {
            print("GameDataManager: could not find object with tag 'Player'");
        }

        // so dev can start whatever scene, and Reset Button will point to it
        currentLevelScenePath = SceneManager.GetActiveScene().path;
        //print("Active Scene = " + currentLevelScenePath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called when Reset Scene button is pressed
    public void ResetScene()
    {
        //print("Loading Scene: " + currentLevelScenePath);
        SceneManager.LoadScene(currentLevelScenePath, LoadSceneMode.Single);
    }
}
