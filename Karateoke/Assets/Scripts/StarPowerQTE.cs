using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarPowerQTE : MonoBehaviour
{
    // The player has to win 2 out of three, however if the defending player wins at all,
    // the amount of damage applied to them goes down. They are at a big disadvantage though.

    [SerializeField]
    private GameObject[] player = new GameObject[2];

    [SerializeField]
    private float secondsBetweenRounds = 1.0f;

    private PlayerQTEInput[] playerQteInput = new PlayerQTEInput[2];
    private List<int> buttonPressIndexOrder = new List<int>();
    private List<int> indexesOfWinners = new List<int>();
    private int indexOfAttacker;
    private int indexOfDefender;
    private int roundIndex;
    private bool playerHasWonThisRound;

    #region Events

    public static event Action<List<int>, int> QTEStart;
    public static event Action QTEEnd;

    #endregion

    public void StartQTE(int indexOfPlayerAttemptingSPMove)
    {
        indexOfAttacker = indexOfPlayerAttemptingSPMove;
        if (indexOfAttacker == 0)
        {
            indexOfDefender = 1;
        }
        else
        {
            indexOfDefender = 0;
        }

        playerHasWonThisRound = false;
        roundIndex = 0;
        indexesOfWinners.Clear();
        //buttonPressIndexOrder.Clear();
        //buttonPressIndexOrder.Add(0);
        //buttonPressIndexOrder.Add(1);
        //buttonPressIndexOrder.Add(2);

        // Shuffling the order of the QTE
        int count = buttonPressIndexOrder.Count;
        int last = count - 1;

        for (var i = 0; i < last; i++)
        {
            var random = UnityEngine.Random.Range(1, count);
            var temp = buttonPressIndexOrder[i];
            buttonPressIndexOrder[i] = buttonPressIndexOrder[random];
            buttonPressIndexOrder[random] = temp;
        }

        QTEStart?.Invoke(buttonPressIndexOrder, indexOfPlayerAttemptingSPMove);
        playerQteInput[0].ActivateQTEButtonAndAnimation(buttonPressIndexOrder[roundIndex]);
        playerQteInput[1].ActivateQTEButtonAndAnimation(buttonPressIndexOrder[roundIndex]);
        roundIndex++;
    }

    public void PlayerWonSingleQTE(int indexOfPlayer)
    {
        if (!playerHasWonThisRound)
        {
            playerHasWonThisRound = true;

            int indexOfOtherPlayer = 0;

            if (indexOfPlayer == 0)
            {
                indexOfOtherPlayer = 1;
            }

            playerQteInput[0].ExecuteQueuedAnimation();
            playerQteInput[1].ExecuteQueuedAnimation();

            indexesOfWinners.Add(indexOfPlayer);
            
            // Make sure fewer than 3 rounds have occured
            if (roundIndex <= 2)
            {
                //playerQteInput[0].ActivateQTEButtonAndAnimation(buttonPressIndexOrder[roundIndex]);
                //playerQteInput[1].ActivateQTEButtonAndAnimation(buttonPressIndexOrder[roundIndex]);
                //playerHasWonThisRound = false;
                //roundIndex++;
                StartCoroutine(DelayNextRound());
            }
            else
            {
                // We get here if the QTE is over (3 rounds have happened), so we determine the winner
                playerQteInput[0].DeactivateAnyQTEButtons();
                playerQteInput[1].DeactivateAnyQTEButtons();

                DetermineOverallWinner();
                
                QTEEnd?.Invoke();
            }
        }
    }


    void Start()
    {
        buttonPressIndexOrder.Add(0);
        buttonPressIndexOrder.Add(1);
        buttonPressIndexOrder.Add(2);

        playerQteInput[0] = player[0].GetComponent<PlayerQTEInput>();
        playerQteInput[1] = player[1].GetComponent<PlayerQTEInput>();
    }

    void Update()
    {
        
    }

    IEnumerator DelayNextRound()
    {
        //The next round is delayed in order to allow the result animations to play
        yield return new WaitForSeconds(secondsBetweenRounds);
        playerQteInput[0].ActivateQTEButtonAndAnimation(buttonPressIndexOrder[roundIndex]);
        playerQteInput[1].ActivateQTEButtonAndAnimation(buttonPressIndexOrder[roundIndex]);
        playerHasWonThisRound = false;
        roundIndex++;
    }

    private void DetermineOverallWinner()
    {
        Debug.Log($"Index of winners: {indexesOfWinners[0]}, {indexesOfWinners[1]}, {indexesOfWinners[2]}.");

    }
}
