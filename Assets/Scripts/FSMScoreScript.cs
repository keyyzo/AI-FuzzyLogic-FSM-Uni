using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FSMScoreScript : MonoBehaviour
{
    private int scoreVal;
    private float currentTime;
    public Text scoreText;
    public Text timeText;
    public GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        scoreVal = 0;
        currentTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Shows current score and final time to user

        scoreText.text = scoreVal.ToString();

        if (!FindObjectOfType<FSMPlayer>().getIsFinished())
        {
            currentTime += 1.0f * Time.deltaTime;
        }

        timeText.text = currentTime.ToString();
    }

    public void IncreaseScore()
    {
        scoreVal++;
    }
}
