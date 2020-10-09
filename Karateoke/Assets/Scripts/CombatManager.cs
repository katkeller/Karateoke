using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

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

    [SerializeField]
    private ParticleSystem[] comboUISparks = new ParticleSystem[2];

    [SerializeField]
    private TextMeshProUGUI[] comboText = new TextMeshProUGUI[2];

    [SerializeField]
    private float comboImageAnimationDelay = 1.5f;

    [SerializeField]
    private int baseAttackDamage = 5, baseParryDamage = 1, baseStarPowerIncrease = 10;

    [SerializeField]
    private float baseStarPowerDecrease = 5f, baseStarPowerComboIncrease = 2f;

    [Tooltip("The number that the disparity average will be divided by to get the value added to damage.")]
    [SerializeField]
    private float bonusDividingFactor = 10.0f, bonusParryDividingValue = 2.0f;

    [Tooltip("The number of seconds between combat choices being made and the circle cam starting back up.")]
    [SerializeField]
    private float secondsBeforeResettingCircleCam = 3.0f, secondsBeforeStartingCircleCamFirstTime = 3.5f;

    private int indexOfWinner;
    private int indexOfLoser;
    private float scoreDisparityAveraged;
    private float damageDealt;
    private float phraseTimeElapsed;
    private int bonus;

    private int starPowerMovePastHitCount = 0;
    private float minorSPDamage;
    private float majorSPDamage;

    private int timesWonInRowAfterFirst;
    private int indexOfLastRoundWinner;
    private Animator[] comboImageAnimator = new Animator[2];

    private Color32 countdownTextColor;
    private AudioSource audioSource;
    private AudioComparisonManager audioComparisonManager;
    private MainSceneCameraManager mainCameraManager;
    private StarPowerQTE starPowerManager;

    private PhraseScore currentPhraseScore = new PhraseScore();

    private bool isPerformingStarPowerMove;
    private bool playerIsDead;
    private bool starPowerHasBeenPerformed;

    private bool circleCamHasStarted;

    #endregion

    public void StartAnimations()
    {
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
        indexOfLastRoundWinner = 3;

        //comboText[0].text = "";
        //comboText[1].text = "";
    }

    void Update()
    {
        phraseTimeElapsed += Time.deltaTime;
    }

    #region Event Triggers

    private void OnEndOfPhrase()
    {
        if (!isPerformingStarPowerMove && !playerIsDead)
        {
            StartCoroutine(AllowPlayersToMakeChoice());
            Debug.Log("Phrase end has been triggered!");
        }
        else
        {
            // We get the phrase score even when we won't be using it so that the phrase values stay consistent in the
            // audio comparison script (and we don't end up with two phrase's worth of values for the next active phrase).
            currentPhraseScore = audioComparisonManager.GetPhraseScore();
        }
    }

    public void OnPlayerAttacks(int indexOfOtherPlayer)
    {
        player[indexOfOtherPlayer].GetAttcked(baseAttackDamage, bonus, indexOfWinner);
    }

    public void OnPlayerBlocks(int indexOfOtherPlayer)
    {
        Debug.Log("Block event on combat manager triggered.");
        // This works a little differently than the other two animation triggers,
        // in that this event is on the block reaction animation. So we know if this
        // event is triggered, the block was successful and we can interrupt the animation.
        player[indexOfOtherPlayer].GetBlocked();
    }

    public void OnPlayerSweeps(int indexOfOtherPlayer)
    {
        var indexOfSweeper = 0;

        if (indexOfOtherPlayer == 0)
        {
            indexOfSweeper = 1;
        }

        bool successfullySweep = true;

        if (player[indexOfOtherPlayer].MoveToExecute == Player.MoveSet.Sweep &&
            indexOfWinner == indexOfOtherPlayer)
        {
            successfullySweep = false;
        }
        if (player[indexOfOtherPlayer].MoveToExecute == Player.MoveSet.Attack)
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
        yield return new WaitForSeconds(3);
        isPerformingStarPowerMove = false;
    }

    private void OnPlayerDealsStarPowerDamage(int indexOfOtherPlayer)
    {
        if (starPowerMovePastHitCount == 0)
        {
            //Minor SP damage is dealt twice, and major SP damage is delt once.
            minorSPDamage = starPowerManager.DamageDeltByStarPowerMove / 4;
            majorSPDamage = starPowerManager.DamageDeltByStarPowerMove / 2;

            player[indexOfOtherPlayer].Health -= (int)minorSPDamage;
            starPowerMovePastHitCount++;
            Debug.Log($"First hit dealing {minorSPDamage} to {player[indexOfOtherPlayer]}");
        }
        else if (starPowerMovePastHitCount == 1)
        {
            player[indexOfOtherPlayer].Health -= (int)minorSPDamage;
            starPowerMovePastHitCount++;
            Debug.Log($"Second hit dealing {minorSPDamage} to {player[indexOfOtherPlayer]}");
        }
        else if (starPowerMovePastHitCount == 2)
        {
            player[indexOfOtherPlayer].Health -= (int)majorSPDamage;
            Debug.Log($"Third hit dealing {majorSPDamage} to {player[indexOfOtherPlayer]}");

            minorSPDamage = 0;
            majorSPDamage = 0;
            starPowerMovePastHitCount = 0;
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
        // loser did compared to the winner during the phrase. This value is applied to bonuses.
        scoreDisparityAveraged = currentPhraseScore.ScoreDisparity / phraseTimeElapsed;
        Debug.Log($"Score Disparity Averaged before capping: {scoreDisparityAveraged}");
        phraseTimeElapsed = 0.0f;

        if (scoreDisparityAveraged > 200)
        {
            // We cap it at 200 so the winner can never deal more than 5 extra points of damage.
            scoreDisparityAveraged = 200;
        }

        bonus = (int)(scoreDisparityAveraged / 40);
        if (bonus < 1)
        {
            // This is to make sure that a fraction bonus below 1 doesn't cause the attack damage to go below the base number.
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

    IEnumerator DecideComboAndDisplayImage(int indexOfWinner, int indexOfLoser)
    {
        if (indexOfWinner == indexOfLastRoundWinner)
        {
            timesWonInRowAfterFirst++;
            if (timesWonInRowAfterFirst == 1)
            {
                //playerAnimator[indexOfWinner].SetBool("isOnFirstBonus", true);
                //playerAnimator[indexOfWinner].SetBool("isOnSecondBonus", false);
            }
            else if (timesWonInRowAfterFirst >= 2)
            {
                //playerAnimator[indexOfWinner].SetBool("isOnSecondBonus", true);
                //playerAnimator[indexOfWinner].SetBool("isOnFirstBonus", false);
            }

            int starPower = (int)baseStarPowerComboIncrease + (timesWonInRowAfterFirst * 2);
            //playerStarPower[indexOfWinner] += starPower;
            //starPowerBar[indexOfWinner].ScaleHealthBar(starPower, true);
            player[indexOfWinner].StarPower += starPower;
        }
        else
        {
            timesWonInRowAfterFirst = 0;
            //playerAnimator[indexOfLoser].SetBool("isOnFirstBonus", false);
            //playerAnimator[indexOfWinner].SetBool("isOnSecondBonus", false);
        }

        indexOfLastRoundWinner = indexOfWinner;

        comboImageAnimator[indexOfWinner].SetTrigger("comboIn");
        comboText[indexOfWinner].text = "star\npower";
        comboUISparks[indexOfWinner].Play();
        yield return new WaitForSeconds(comboImageAnimationDelay);
        comboImageAnimator[indexOfWinner].SetTrigger("comboOut");
        comboText[indexOfWinner].text = "";
    }

}
