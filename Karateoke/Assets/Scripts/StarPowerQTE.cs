using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    private int baseStarPowerDamage = 30, decreasedStarPowerDamage = 20;

    public int DamageDeltByStarPowerMove
    {
        get;
        private set;
    }

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

            playerQteInput[0].ExecuteQueuedAnimationAndHideGraphics();
            playerQteInput[1].ExecuteQueuedAnimationAndHideGraphics();

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
                DetermineOverallWinner();
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

        var winIndexGrouped = indexesOfWinners.GroupBy(i => i);

        //foreach (var group in numberOfAttackerWins)
        //{
        //    Debug.Log($"{group.Key}, {group.Count()}");
        //}

        if (winIndexGrouped.Count() < 1)
        {
            // We get here if only one person won all QTEs.
            if (winIndexGrouped.Any(item => item.Key == indexOfAttacker))
            {
                // This means our attacker won completely, so we deal full damage and play star power move animations.
                AttackerWins(damageReduced: false);
            }
            else
            {
                // This means the other player defended themself completely.
                AttackerLoses();
            }
        }
        else
        {
            var attackerItem = winIndexGrouped.Select(item => item.Key == indexOfAttacker);

            if (attackerItem.Count() < 2)
            {
                // We get here if the attacker only won one out of three, so they lose.
                AttackerLoses();
            }
            else
            {
                // And we get here if the attacker won twice, so they win, but damage is reduced.
                AttackerWins(damageReduced: true);
            }
        }

        //playerQteInput[0].ResolveQTEs();
        //playerQteInput[1].ResolveQTEs();
        QTEEnd?.Invoke();
    }

    private void AttackerWins(bool damageReduced)
    {
        if (damageReduced)
        {
            DamageDeltByStarPowerMove = decreasedStarPowerDamage;
        }
        else
        {
            DamageDeltByStarPowerMove = baseStarPowerDamage;
        }

        //playerQteInput[indexOfAttacker].WinOverallAsAttacker();
        //playerQteInput[indexOfDefender].LoseOverallAsDefender();
        playerQteInput[0].StarPowerMoveWasSuccessful();
        playerQteInput[1].StarPowerMoveWasSuccessful();
    }

    private void AttackerLoses()
    {
        playerQteInput[0].StarPowerMoveWasUnsuccessful();
        playerQteInput[1].StarPowerMoveWasUnsuccessful();
    }
}
