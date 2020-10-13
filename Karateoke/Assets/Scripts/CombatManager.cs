using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
/// <summary>
/// This script is the "main brain" of the game's combat. It recieves info from the two player scripts, the phrase end trigger,
/// the star power QTE manager, and the audio comparison manager. It also handles activting the various elements of those scripts 
/// (such as the star power moves) at the right time.
/// </summary>
public class CombatManager : MonoBehaviour
{
    #region Properties/Fields
    [SerializeField]
    private GameObject mainCameraManagerGameObject;

    [SerializeField]
    private GameObject audioComparisonManagerGameObject;

    [SerializeField]
    private Player[] player = new Player[2];

    [SerializeField]
    private TextMeshProUGUI countdownText;

    [SerializeField]
    private float secondsForChoice = 1.5f;

    [SerializeField]
    private Color32 lastNumberColor;

    [SerializeField]
    private AudioClip[] countdownClip = new AudioClip[4];

    [SerializeField]
    private GameObject[] winnerIndicationUI = new GameObject[2];

    // In the future, I might want to have the blocking player deal a small amont of parry damage on a successfully block
    // if they won the phrase singing-wise. But for now, the parry values aren't being used.
    [SerializeField]
    private int baseAttackDamage = 5, baseParryDamage = 1, baseStarPowerIncrease = 10;

    [Tooltip("The number of seconds between combat choices being made and the circle cam starting back up.")]
    [SerializeField]
    private float secondsBeforeResettingCircleCam = 3.0f, secondsBeforeStartingCircleCamFirstTime = 3.5f;

    private int indexOfWinner;
    private int indexOfLoser;
    private float scoreDisparityAveraged;
    private float phraseTimeElapsed;
    private int bonus;

    private int starPowerMovePastHitCount = 0;
    private float minorSPDamage;
    private float majorSPDamage;

    private Color32 countdownTextColor;
    private AudioSource audioSource;
    private AudioComparisonManager audioComparisonManager;
    private MainSceneCameraManager mainCameraManager;
    private StarPowerQTE starPowerManager;

    private PhraseScore currentPhraseScore = new PhraseScore();

    private bool isPerformingStarPowerMove;
    private bool playerIsDead;

    private bool circleCamHasStarted;

    #endregion

    public void StartAnimations()
    {
        // This is called from the game start manager when the gameplay begins.
        player[0].StartGame();
        player[1].StartGame();
        StartCoroutine(WaitThenResetCamera());
    }

    private void Awake()
    {
        player[0].IndexAccordingToCombatManager = 0;
        player[1].IndexAccordingToCombatManager = 1;

        audioSource = GetComponent<AudioSource>();
        audioComparisonManager = audioComparisonManagerGameObject.GetComponent<AudioComparisonManager>();
        starPowerManager = GetComponent<StarPowerQTE>();
        mainCameraManager = mainCameraManagerGameObject.GetComponent<MainSceneCameraManager>();
    }

    void Start()
    {
        player[0].Health = 100;
        player[1].Health = 100;
        player[0].StarPower = 0;
        player[1].StarPower = 0;

        countdownText.text = "";
        countdownTextColor = countdownText.color;
    }

    void Update()
    {
        phraseTimeElapsed += Time.deltaTime;
    }

    #region Event Triggers

    private void OnEndOfPhrase()
    {
        // This is triggered by the phrase end trigger script when a lyric that is marked as the end of a phrase passses through the trigger.
        if (!isPerformingStarPowerMove && !playerIsDead)
        {
            // If the star power move is happening, normal combat is paused so that we can resolve the QTE and the cutscene.
            StartCoroutine(AllowPlayersToMakeChoice());
            Debug.Log("Phrase end has been triggered.");
        }
        else
        {
            // We want to get the phrase score even when we won't be using it so that the phrase values stay consistent in the
            // audio comparison script (and we don't end up with two phrase's worth of values for the next active phrase).
            currentPhraseScore = audioComparisonManager.GetPhraseScore();
        }
    }

    public void OnPlayerAttacks(int indexOfOtherPlayer)
    {
        // These events are triggered from either player's script when they attempt a combat move.
        // Once the window for choosing a combat move ends, each player's chosen animations will play. Those
        // animations have event triggers in them that then communicate to these events so that the combat manager
        // can pass along the info needed for each move. In the case of attacking, it tells the other player's 
        // script that they are being attacked, what the possible damage is, and who won the phrase singing-wise.
        // That player's script then knows if the attack was successful (and if they should be damaged) or not 
        // based on if they're blocking or if they've performed the best singing-wise.
        player[indexOfOtherPlayer].GetAttcked(baseAttackDamage, bonus, indexOfWinner);
    }

    public void OnPlayerBlocks(int indexOfOtherPlayer)
    {
        // This event is triggered from the blocking player's block reaction animation. So we know if this
        // event was triggered, their block was successful, so we should tell the attacking player that their 
        // attack failed.
        player[indexOfOtherPlayer].GetBlocked();
    }

    public void OnPlayerSweeps(int indexOfOtherPlayer)
    {
        // This event is triggered from the players' sweep animation. So here, we determine if the other player is currently attacking,
        // or if the other player is also sweeping and they won the phrase. If so, then the sweep fails.

        int indexOfSweeper = 0;
        if (indexOfOtherPlayer == 0)
        {
            indexOfSweeper = 1;
        }

        bool successfullySweep = true;

        if ((player[indexOfOtherPlayer].MoveToExecute == Player.MoveSet.Sweep && indexOfWinner == indexOfOtherPlayer) ||
            player[indexOfOtherPlayer].MoveToExecute == Player.MoveSet.Attack)
        {
            successfullySweep = false;
        }

        if (successfullySweep)
        {
            player[indexOfSweeper].SuccessfullySweep(baseStarPowerIncrease, bonus, indexOfWinner);
            player[indexOfOtherPlayer].GetSwept();
        }
    }

    private void OnPlayerDies(int indexOfPlayer)
    {
        playerIsDead = true;
        var indexOfWinner = 1;
        if (indexOfPlayer == 1)
        {
            indexOfWinner = 0;
        }
        player[indexOfWinner].Win();
    }

    private void OnPlayerHasFullStarPower(int indexOfPlayer)
    {
        isPerformingStarPowerMove = true;
        starPowerManager.StartQTE(indexOfPlayer);
        player[0].StarPowerMoveIsHappening = true;
        player[1].StarPowerMoveIsHappening = true;
    }

    private void OnStarPowerMoveEnds(int indexOfAttackedPlayer, int indexOfOtherPlayer)
    {
        // When the SP move cutscene ends, we want to reset combat back to the default state and also check to see
        // if the SP move killed the player who was attacked.

        player[indexOfOtherPlayer].StarPower = 0;
        player[0].StarPowerMoveIsHappening = false;
        player[1].StarPowerMoveIsHappening = false;

        if (player[indexOfAttackedPlayer].Health <= 0)
        {
            // If the player dies during an SP move, we want to wait for the end of the SP cutscene before playing the death animation.
            player[indexOfAttackedPlayer].Die();

            Debug.Log("Player died from star power move.");
        }
        else
        {
            StartCoroutine(WaitThenReactivateCombat());
        }
    }

    private IEnumerator WaitThenReactivateCombat()
    {
        // We want to wait for a few seconds so that the animations can finish.
        yield return new WaitForSeconds(3);
        isPerformingStarPowerMove = false;
    }

    private void OnPlayerDealsStarPowerDamage(int indexOfOtherPlayer)
    {
        // This is triggered each time the attacker deals damage to the loser, which happens three times in the cutscene.
        switch (starPowerMovePastHitCount)
        {
            case 0:
                // We split the SP move into chunks so we can deal it to the losing player when they are hit by each 
                // punch in the cutscene. That way, the health bar animates alongside them getting hit.
                // Minor SP damage is dealt twice, and major SP damage is delt once.
                minorSPDamage = starPowerManager.DamageDeltByStarPowerMove / 4;
                majorSPDamage = starPowerManager.DamageDeltByStarPowerMove / 2;

                player[indexOfOtherPlayer].Health -= (int)minorSPDamage;
                starPowerMovePastHitCount++;
                Debug.Log($"First hit dealing {minorSPDamage} to {player[indexOfOtherPlayer]}");
                break;
            case 1:
                player[indexOfOtherPlayer].Health -= (int)minorSPDamage;
                starPowerMovePastHitCount++;
                Debug.Log($"Second hit dealing {minorSPDamage} to {player[indexOfOtherPlayer]}");
                break;
            case 2:
                player[indexOfOtherPlayer].Health -= (int)majorSPDamage;
                Debug.Log($"Third hit dealing {majorSPDamage} to {player[indexOfOtherPlayer]}");
                // Since this is the last hit, we can reset the values.
                minorSPDamage = 0;
                majorSPDamage = 0;
                starPowerMovePastHitCount = 0;
                break;
        }
    }

    private void OnEnable()
    {
        //CombatTestingScript.EndOfPhrase += OnEndOfPhrase;
        PhraseEndTrigger.EndOfPhrase += OnEndOfPhrase;
        Player.AttemptAttack += OnPlayerAttacks;
        Player.AttemptBlock += OnPlayerBlocks;
        Player.AttemptSweep += OnPlayerSweeps;
        Player.DealStarPowerDamage += OnPlayerDealsStarPowerDamage;
        Player.PlayerDies += OnPlayerDies;
        Player.PlayerHasFullStarPower += OnPlayerHasFullStarPower;
        Player.StarPowerMoveEnds += OnStarPowerMoveEnds;
    }

    private void OnDisable()
    {
        //CombatTestingScript.EndOfPhrase -= OnEndOfPhrase;
        PhraseEndTrigger.EndOfPhrase -= OnEndOfPhrase;
        Player.AttemptAttack -= OnPlayerAttacks;
        Player.AttemptBlock -= OnPlayerBlocks;
        Player.AttemptSweep -= OnPlayerSweeps;
        Player.DealStarPowerDamage -= OnPlayerDealsStarPowerDamage;
        Player.PlayerDies -= OnPlayerDies;
        Player.PlayerHasFullStarPower -= OnPlayerHasFullStarPower;
        Player.StarPowerMoveEnds -= OnStarPowerMoveEnds;
    }

    #endregion

    IEnumerator AllowPlayersToMakeChoice()
    {
        // This is triggered at the end of each phrase, and only if no one is dead and no SP moves are occuring.
        if (!playerIsDead && !isPerformingStarPowerMove)
        {
            player[0].ExecuteMakingChoiceAnimations();
            player[1].ExecuteMakingChoiceAnimations();
            countdownText.text = "3";
            audioSource.PlayOneShot(countdownClip[0]);
            yield return new WaitForSeconds(1);
            countdownText.text = "2";
            audioSource.PlayOneShot(countdownClip[1]);
            yield return new WaitForSeconds(1);
            countdownText.text = "1";
            audioSource.PlayOneShot(countdownClip[2]);
            player[0].CanMakeChoice = true;
            player[1].CanMakeChoice = true;
            yield return new WaitForSeconds(1);
            countdownText.color = lastNumberColor;
            countdownText.text = "CHOOSE!";
            audioSource.PlayOneShot(countdownClip[3]);
            yield return new WaitForSeconds(secondsForChoice);
            countdownText.text = "";
            countdownText.color = countdownTextColor;
            player[0].CanMakeChoice = false;
            player[1].CanMakeChoice = false;

            currentPhraseScore = audioComparisonManager.GetPhraseScore();

            ExecuteCombat();
        }
    }

    public void ExecuteCombat()
    {
        indexOfWinner = currentPhraseScore.IndexOfWinner;
        indexOfLoser = currentPhraseScore.IndexOfLoser;
        Debug.Log($"Index of phrase winner: {indexOfWinner}");
        StartCoroutine(ShowPhraseWinnerUI());

        // The disparity average is based on how different the two players' scores were,
        // and how long the phrase in question was. The higher the score, the worse the
        // loser did compared to the winner. This value is applied to bonuses.
        scoreDisparityAveraged = currentPhraseScore.ScoreDisparity / phraseTimeElapsed;
        Debug.Log($"Score Disparity Averaged before capping: {scoreDisparityAveraged}");
        phraseTimeElapsed = 0.0f;

        if (scoreDisparityAveraged > 200)
        {
            // We cap it at 200 (and then divide it by 40 to get the bonus) so the winner can never deal more than 5 extra points of damage.
            scoreDisparityAveraged = 200;
        }

        bonus = (int)(scoreDisparityAveraged / 40);
        if (bonus < 1)
        {
            bonus = 0;
        }
        Debug.Log($"Bonus: {bonus}");

        player[0].ExecuteQueuedCombatMove();
        player[1].ExecuteQueuedCombatMove();

        StartCoroutine(WaitThenResetCamera());
    }

    private IEnumerator ShowPhraseWinnerUI()
    {
        yield return new WaitForSeconds(0.5f);
        winnerIndicationUI[indexOfWinner].SetActive(true);
        yield return new WaitForSeconds(2.2f);
        winnerIndicationUI[indexOfWinner].SetActive(false);
    }

    private IEnumerator WaitThenResetCamera()
    {
        // We want to give the combat a chance to resolve before starting the camera on its circle track again.

        float seconds = circleCamHasStarted ? 
            secondsBeforeResettingCircleCam : secondsBeforeStartingCircleCamFirstTime;

        if (!circleCamHasStarted)
            circleCamHasStarted = true;

        yield return new WaitForSeconds(seconds);

        if (!isPerformingStarPowerMove && !playerIsDead)
        {
            mainCameraManager.StartCircleCam();
        }
    }
}
