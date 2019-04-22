using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    int scoreValue = 0;
    Text score;

    // Start is called before the first frame update
    void Start()
    {
        score = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        score.text = "Score: " + scoreValue;
    }

    // increase score function
    public void UpdateScore(int points)
    {
        scoreValue += points;
    }

    // When "Reset Scene" button is pressed
    public void ResetScene()
    {
        scoreValue = 0;
    }
}

