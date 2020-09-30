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

    [SerializeField]
    private GameObject starPowerTextObject;

    [SerializeField]
    private AudioClip starPowerTextSFX;

    public int DamageDeltByStarPowerMove
    {
        get;
        private set;
    }

    private AudioSource audioSource;
    private PlayerQTEInput[] playerQteInput = new PlayerQTEInput[2];
    private List<int> buttonPressIndexOrder = new List<int>();
    //private List<int> indexesOfWinners = new List<int>();
    private int indexOfAttacker;
    private int indexOfDefender;
    private int roundIndex;
    private int attackerWinCount;
    private int defenderWinCount;
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

        StartCoroutine(ShowThenHideSPText());

        playerHasWonThisRound = false;
        roundIndex = 0;
        attackerWinCount = 0;
        defenderWinCount = 0;
        //indexesOfWinners.Clear();
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

    private IEnumerator ShowThenHideSPText()
    {
        starPowerTextObject.SetActive(true);
        audioSource.PlayOneShot(starPowerTextSFX);
        yield return new WaitForSeconds(3);
        starPowerTextObject.SetActive(false);
    }

    public void PlayerWonSingleQTE(int indexOfPlayer)
    {
        if (!playerHasWonThisRound)
        {
            playerHasWonThisRound = true;

            playerQteInput[0].ExecuteQueuedAnimationAndHideGraphics();
            playerQteInput[1].ExecuteQueuedAnimationAndHideGraphics();
            StartCoroutine(playerQteInput[indexOfPlayer].CreateFireball());

            if (indexOfPlayer == indexOfAttacker)
            {
                attackerWinCount++;
            }
            
            // Make sure fewer than 3 rounds have occured
            if (roundIndex <= 2)
            {
                StartCoroutine(DelayNextRound());
            }
            else
            {
                // We get here if the QTE is over (3 rounds have happened), so we determine the winner
                StartCoroutine(DetermineOverallWinner());
            }
        }
    }


    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        buttonPressIndexOrder.Add(0);
        buttonPressIndexOrder.Add(1);
        buttonPressIndexOrder.Add(2);

        playerQteInput[0] = player[0].GetComponent<PlayerQTEInput>();
        playerQteInput[1] = player[1].GetComponent<PlayerQTEInput>();
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

    private IEnumerator DetermineOverallWinner()
    {
        // Delay winning/losing so final QTE animations can resolve
        yield return new WaitForSeconds(secondsBetweenRounds);

        Debug.Log($"Attacker win count: {attackerWinCount}");
        Debug.Log($"Defender win count: {defenderWinCount}");

        switch (attackerWinCount)
        {
            case 0:
                // This means the other player defended themself completely.
                Debug.Log("Attacker lost all QTEs.");
                AttackerLoses();
                break;
            case 1:
                // This means the attacker only won one out of three, so they lose.
                Debug.Log("Attacker won only once. They lose.");
                AttackerLoses();
                break;
            case 2:
                // This means the attacker won twice, so they win, but damage is reduced.
                Debug.Log("Attacker won twice. They win, damage is reduced.");
                AttackerWins(damageReduced: true);
                break;
            case 3:
                // This means the attacker won completely, so we deal full damage and play star power move animations.
                Debug.Log("Attacker won all QTEs.");
                AttackerWins(damageReduced: false);
                break;
            default:
                Debug.Log("Star Power QTE Error: attacker win count is invalid");
                break;
        }

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

        playerQteInput[0].StarPowerMoveWasSuccessful();
        playerQteInput[1].StarPowerMoveWasSuccessful();
    }

    private void AttackerLoses()
    {
        playerQteInput[0].StarPowerMoveWasUnsuccessful();
        playerQteInput[1].StarPowerMoveWasUnsuccessful();
    }
}
