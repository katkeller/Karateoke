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
    private TextMeshProUGUI player1ScoreText, player2ScoreText, player1DifferenceText, player2DifferenceText, timeText, winnerText;

    [SerializeField]
    private List<int> phraseEndTimes = new List<int>();

    [SerializeField]
    private float secondsWinnerTextOnScreen = 1.5f;

    private AudioSampleCollector[] audioSampleCollector = new AudioSampleCollector[3];
    private int[] index = new int[3];
    private float[] highestValue = new float[3];

    private float[] differenceBetweenPlayerAndSource = new float[2];
    private float[] playerScore = new float[2];
    private bool atEndOfPhrase;
    private bool recordingValues;

    private float timeElapsed;

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
        UpdateTime();

        atEndOfPhrase |= phraseEndTimes.Contains((int)timeElapsed);

        if (!atEndOfPhrase)
        {
            for (int e = 0; e < audioSampleCollector.Length; e++)
            {
                RecordSampleValues(e);
            }

            UpdateText();
        }
        else if (atEndOfPhrase)
        {
            DecideWinner();
        }
    }

    private void DecideWinner()
    {
        if (playerScore[0] < playerScore[1])
        {
            StartCoroutine(DisplayWinnerText("Player 1"));
        }
        else if (playerScore[0] > playerScore[1])
        {
            StartCoroutine(DisplayWinnerText("Player 2"));
        }

        playerScore[0] = 0.0f;
        playerScore[1] = 0.0f;
        atEndOfPhrase = false;
    }

    IEnumerator DisplayWinnerText(string winner)
    {
        winnerText.text = $"{winner} wins!";
        yield return new WaitForSeconds(secondsWinnerTextOnScreen);
        winnerText.text = "";
    }

    private void UpdateTime()
    {
        timeElapsed += Time.deltaTime;
        timeText.text = timeElapsed.ToString("F1");
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

    private void UpdateText()
    {
        if (highestValue[1] > 0.02f)
        {
            player1ScoreText.text = $"Player 1 Score:\n{playerScore[0]}";
            //Debug.Log($"Player 1 score is {playerScore[0]}");
        }

        if (highestValue[2] > 0.01f)
            player2ScoreText.text = $"Player 2 Score:\n{playerScore[1]}";

        player1DifferenceText.text = $"Difference between player 1 and source:\n{differenceBetweenPlayerAndSource[0]}";
        player2DifferenceText.text = $"Difference between player 2 and source:\n{differenceBetweenPlayerAndSource[1]}";

        //if (highestValue[0] > 0.01f)
        //    recordingValues = true;
        //else
        //    recordingValues = false;
    }
}
