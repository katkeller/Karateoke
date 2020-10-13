using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
/// <summary>
/// This script takes in the values from the audio sample collectors and uses them to determine which player is
/// the winner of each phrase, as well as how much better they did than the loser.
/// </summary>
public class AudioComparisonManager : MonoBehaviour
{
    [Tooltip("Element zero should be the vocal source, one should be Player 1, and two should be Player 2.")]
    [SerializeField]
    private GameObject[] audioSampleObject = new GameObject[3];

    private AudioSampleCollector[] audioSampleCollector = new AudioSampleCollector[3];
    private int[] index = new int[3];
    private float[] highestValue = new float[3];

    private float[] differenceBetweenPlayerAndSource = new float[2];
    private float[] playerScore = new float[2];


    private void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            audioSampleCollector[i] = audioSampleObject[i].GetComponent<AudioSampleCollector>();
        }
    }

    void Update()
    {
        for (int e = 0; e < audioSampleCollector.Length; e++)
        {
            RecordSampleValues(e);
        }
    }

    public PhraseScore GetPhraseScore()
    {
        PhraseScore toReturn = new PhraseScore();

        if (playerScore[0] < playerScore[1])
        {
            toReturn.IndexOfWinner = 0;
            toReturn.IndexOfLoser = 1;
            toReturn.ScoreDisparity = playerScore[1] - playerScore[0];
        }
        else
        {
            toReturn.IndexOfWinner = 1;
            toReturn.IndexOfLoser = 0;
            toReturn.ScoreDisparity = playerScore[0] - playerScore[1];
        }

        playerScore[0] = 0;
        playerScore[1] = 1;

        return toReturn;
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
