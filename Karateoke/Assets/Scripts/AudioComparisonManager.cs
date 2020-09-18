using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class AudioComparisonManager : MonoBehaviour
{
    [Tooltip("Element zero should be the vocal source, one should be Player 1, and two should be Player 2.")]
    [SerializeField]
    private GameObject[] audioSampleObject = new GameObject[3];

    [SerializeField]
    private Image player1WinImage, player2WinImage;

    [SerializeField]
    private float secondsWinnerImageOnScreen = 1.5f;

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

        //player1WinImage.enabled = false;
        //player2WinImage.enabled = false;
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
            //StartCoroutine(DisplayWinnerImage(player1WinImage));
        }
        else if (playerScore[0] > playerScore[1])
        {
            IndexOfWinner = 1;
            IndexOfLoser = 0;
            ScoreDisparity = playerScore[0] - playerScore[1];
            //StartCoroutine(DisplayWinnerImage(player2WinImage));
        }

        playerScore[0] = 0.0f;
        playerScore[1] = 0.0f;
    }

    private void OnEnable()
    {
        CombatManager.DecideWinner += OnDecideWinner;
    }

    private void OnDisable()
    {
        CombatManager.DecideWinner -= OnDecideWinner;
    }

    IEnumerator DisplayWinnerImage(Image image)
    {
        image.enabled = true;
        yield return new WaitForSeconds(secondsWinnerImageOnScreen);
        image.enabled = false;
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
