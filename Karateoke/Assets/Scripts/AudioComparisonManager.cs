using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AudioComparisonManager : MonoBehaviour
{
    //[SerializeField]
    //private GameObject player1AudioSampleObject, player2AudioSampleObject, sourceAudioSampleObject;

    [SerializeField]
    private GameObject[] audioSampleObject = new GameObject[3];

    [SerializeField]
    private TextMeshProUGUI player1ScoreText, player2ScoreText, player1DifferenceText, player2DifferenceText;

    [SerializeField]
    private float[] phraseEndTimes = new float[4];

    //private AudioSampleCollector player1AudioSampleCollector, player2AudioSampleCollector, sourceAudioSampleCollector;
    //private float differenceBetweenPlayer1AndSource = 0.0f, differenceBetweenPLayer2AndSource = 0.0f;
    //private int player1Index, player2Index, sourceIndex;
    //private float player1HighestValue, player2HighestValue, sourceHighestValue;

    private AudioSampleCollector[] audioSampleCollector = new AudioSampleCollector[3];
    private int[] index = new int[3];
    private float[] highestValue = new float[3];

    private float[] differenceBetweenPlayerAndSource = new float[2];
    private float[] playerScore = new float[2];
    private bool atEndOfPhrase;
    private bool recordingValues;

    private void Awake()
    {

        //player1AudioSampleCollector = player1AudioSampleObject.GetComponent<AudioSampleCollector>();
        //player2AudioSampleCollector = player2AudioSampleObject.GetComponent<AudioSampleCollector>();
        //sourceAudioSampleCollector = sourceAudioSampleObject.GetComponent<AudioSampleCollector>();

        for (int i = 0; i < 3; i++)
        {
            audioSampleCollector[i] = audioSampleObject[i].GetComponent<AudioSampleCollector>();
        }
    }

    void Start()
    {
        highestValue[0] = audioSampleCollector[0].highestValue;
        recordingValues = true;
    }

    void Update()
    {
        //if (sourceHighestValue < 0.1f)
        //    recordingValues = false;
        //else
        //    recordingValues = true;

        //if (!atEndOfPhrase && recordingValues)
        //{
        //    RecordSampleValues();
        //    UpdateText();
        //}

        //if (highestValue[0] < 0.1f)
        //    recordingValues = false;
        //else
        //    recordingValues = true;

        if (!atEndOfPhrase && recordingValues)
        {
            for (int e = 0; e < audioSampleCollector.Length; e++)
            {
                RecordSampleValues(e);
            }
        }

        UpdateText();
    }

    private void RecordSampleValues(int e)
    {
        index[e] = audioSampleCollector[e].indexOfHighestValue;
        highestValue[e] = audioSampleCollector[e].highestValue;
        //Debug.Log($"Highest Value of audio sample collector {e}: {highestValue[e]}");

        if (e > 0)
        {
            int playerSpecificIndex = e - 1;
            differenceBetweenPlayerAndSource[playerSpecificIndex] = Math.Abs(index[0] - index[e]);
            playerScore[playerSpecificIndex] += differenceBetweenPlayerAndSource[playerSpecificIndex];
            Debug.Log($"Difference between player {e} and source: {differenceBetweenPlayerAndSource[playerSpecificIndex]}");
        }

        //player1Index = player1AudioSampleCollector.indexOfHighestValue;
        //player1HighestValue = player1AudioSampleCollector.highestValue;
        //player2Index = player2AudioSampleCollector.indexOfHighestValue;

        //sourceIndex = sourceAudioSampleCollector.indexOfHighestValue;
        //sourceHighestValue = sourceAudioSampleCollector.highestValue;

        //differenceBetweenPlayer1AndSource = Math.Abs(sourceIndex - player1Index);

        //player1Score += differenceBetweenPlayer1AndSource;
    }

    private void UpdateText()
    {
        if (highestValue[1] > 0.02f)
        {
            player1ScoreText.text = $"Player 1 Score:\n{playerScore[0]}";
            Debug.Log($"Player 1 score is {playerScore[0]}");
        }

        if (highestValue[2] > 0.01f)
            player2ScoreText.text = $"PLayer 2 Score:\n{playerScore[1]}";

        player1DifferenceText.text = $"Difference between player 1 and source:\n{differenceBetweenPlayerAndSource[0]}";
        player2DifferenceText.text = $"Difference between player 2 and source:\n{differenceBetweenPlayerAndSource[1]}";
        //if (player1HighestValue > 0.02f)
        //{
        //    player1ScoreText.text = $"Player 1: {player1Score}";
        //}

            //player1DifferenceText.text = $"Difference between player 1 and source: {differenceBetweenPlayer1AndSource}";
    }
}
