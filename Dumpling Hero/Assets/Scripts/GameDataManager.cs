using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{

    private string currentLevelScenePath;

    public GameObject hero;

    // Start is called before the first frame update
    void Start()
    {
        hero = GameObject.FindGameObjectWithTag("Player");

        // Set unlocks and modifiers
        if (hero != null)
        {
            hero.GetComponent<Animator>().SetBool("spinAttackUnlocked", true);
        }

        // so dev can start whatever scene, and Reset Button will point to it
        currentLevelScenePath = SceneManager.GetActiveScene().path;

        print("Active Scene = " + currentLevelScenePath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetScene()
    {
        print("Loading Scene: " + currentLevelScenePath);
        SceneManager.LoadScene(currentLevelScenePath, LoadSceneMode.Single);
    }
}
