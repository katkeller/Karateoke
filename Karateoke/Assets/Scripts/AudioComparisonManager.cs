using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class AudioComparisonManager : MonoBehaviour
{
    [Tooltip("Element zero should be the vocal source, one should be Player 1, and two should be Player 2.")]
    [SerializeField]
    private GameObject[] audioSampleObject = new GameObject[3];

    [SerializeField]
    private TextMeshProUGUI winnerText;

    [SerializeField]
    private float secondsWinnerTextOnScreen = 1.5f;

    public int IndexOfWinner;
    public int IndexOfLoser;
    public float ScoreDisparity;

    private AudioSampleCollector[] audioSampleCollector = new AudioSampleCollector[3];
    private int[] index = new int[3];
    private float[] highestValue = new float[3];

    private float[] differenceBetweenPlayerAndSource = new float[2];
    private float[] playerScore = new float[2];

    private bool recordingValues;


    private void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            audioSampleCollector[i] = audioSampleObject[i].GetComponent<AudioSampleCollector>();
        }

        winnerText.text = "";
    }

    void Update()
    {
        for (int e = 0; e < audioSampleCollector.Length; e++)
        {
            RecordSampleValues(e);
        }
    }

    private void OnDecideWinner()
    {
        if (playerScore[0] < playerScore[1])
        {
            IndexOfWinner = 0;
            IndexOfLoser = 1;
            ScoreDisparity = playerScore[1] - playerScore[0];
            // The winning disparity is how much the losing player's score differed from the better player's score.
            // A higher score is worse, since that means they were far off from the target by a larger amount.
            StartCoroutine(DisplayWinnerText("Player 1"));
        }
        else if (playerScore[0] > playerScore[1])
        {
            IndexOfWinner = 1;
            IndexOfLoser = 0;
            ScoreDisparity = playerScore[0] - playerScore[1];
            StartCoroutine(DisplayWinnerText("Player 2"));
        }

        playerScore[0] = 0.0f;
        playerScore[1] = 0.0f;
        //atEndOfPhrase = false;
    }

    private void OnEnable()
    {
        CombatManager.DecideWinner += OnDecideWinner;
    }

    private void OnDisable()
    {
        CombatManager.DecideWinner -= OnDecideWinner;
    }

    IEnumerator DisplayWinnerText(string winner)
    {
        winnerText.text = $"{winner} wins!";
        yield return new WaitForSeconds(secondsWinnerTextOnScreen);
        winnerText.text = "";
    }

    private void RecordSampleValues(int e)
    {
        index[e] = audioSampleCollector[e].indexOfHighestValue;
        highestValue[e] = audioSampleCollector[e].highestValue;

        if (e > 0)
        {
            int playerSpecificIndex = e - 1;
            differenceBetweenPlayerAndSource[playerSpecificIndex] = Math.Abs(index[0] - index[e]);
            playerScore[playerSpecificIndex] += differenceBetweenPlayerAndSource[playerSpecificIndex];
        }
    }
}
