using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AudioComparisonManager : MonoBehaviour
{
    [SerializeField]
    private GameObject player1AudioSampleObject, player2AudioSampleObject, sourceAudioSampleObject;

    [SerializeField]
    private TextMeshProUGUI player1ScoreText, player1DifferenceText;

    private AudioSampleCollector player1AudioSampleCollector, sourceAudioSampleCollector;
    private bool atEndOfPhrase;
    private float differenceBetweenPlayer1AndSource = 0.0f;
    private int player1Index, sourceIndex;

    private float player1Score = 0.0f, player2Score = 0.0f;

    private void Awake()
    {
        player1AudioSampleCollector = player1AudioSampleObject.GetComponent<AudioSampleCollector>();
        //player2AudioSampleCollector = player2AudioSampleObject.GetComponent<AudioSampleCollector>();
        sourceAudioSampleCollector = sourceAudioSampleObject.GetComponent<AudioSampleCollector>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (!atEndOfPhrase)
        {
            RecordSampleValues();
            UpdateText();
        }

    }

    private void RecordSampleValues()
    {
        player1Index = player1AudioSampleCollector.indexOfHighestValue;
        sourceIndex = sourceAudioSampleCollector.indexOfHighestValue;

        differenceBetweenPlayer1AndSource = Math.Abs(sourceIndex - player1Index);

        player1Score += differenceBetweenPlayer1AndSource;
    }

    private void UpdateText()
    {
        player1ScoreText.text = $"Player 1: {player1Score}";
        player1DifferenceText.text = $"Difference between player 1 and source: {differenceBetweenPlayer1AndSource}";
    }
}
