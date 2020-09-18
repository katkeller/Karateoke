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
    private bool isDebugging;

    [SerializeField]
    private Player[] player = new Player[2];

    [SerializeField]
    private TextMeshProUGUI countdownText, winnerText;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private float secondsForChoice = 1.5f;

    [SerializeField]
    private Color32 lastNumberColor;

    [SerializeField]
    private AudioClip[] countdownClip = new AudioClip[4];

    [SerializeField]
    private GameObject victorySprite;

    //[SerializeField]
    //private Sprite attackIcon, blockIcon, grappleIcon;

    //[SerializeField]
    //private GameObject[] playerModel = new GameObject[2];

    [SerializeField]
    private GameObject[] comboImage = new GameObject[2];

    [SerializeField]
    private ParticleSystem[] sparks = new ParticleSystem[2];

    [SerializeField]
    private ParticleSystem[] starPowerSparks = new ParticleSystem[2];

    [SerializeField]
    private TextMeshProUGUI[] comboText = new TextMeshProUGUI[2];

    //[SerializeField]
    //private GameObject player1Portrait, player2Portrait;

    //[SerializeField]
    //private Sprite player1RegularPortrait, player1NoChoicePortrait, player1ChoiceMadePortrait, player2RegularPortrait, player2NoChoicePortrait, player2ChoiceMadePortrait;

    [SerializeField]
    private float comboImageAnimationDelay = 1.5f;

    [SerializeField]
    private float getHitAnimationDelay = 1.0f, victoryAnimationDelay = 2.0f;

    //[SerializeField]
    //private GameObject[] playerHealthBarGameObject = new GameObject[2];

    //[SerializeField]
    //private GameObject[] playerStarPowerBarGameObject = new GameObject[2];

    [SerializeField]
    private GameObject[] starPowerBackgrounds = new GameObject[2];

    [SerializeField]
    private int baseAttackDamage = 5, baseParryDamage = 1, baseStarPowerIncrease = 10, starPowerMoveDamage = 20;

    [SerializeField]
    private float baseStarPowerDecrease = 5f, baseStarPowerComboIncrease = 2f;

    [SerializeField]
    private float starPowerMoveLengthInSeconds = 5.0f;

    [Tooltip("The number that the disparity average will be divided by to get the value added to damage.")]
    [SerializeField]
    private float bonusDividingFactor = 10.0f, bonusParryDividingValue = 2.0f;

    [Tooltip("The number of seconds between combat choices being made and the circle cam starting back up.")]
    [SerializeField]
    private float secondsBeforeResettingCircleCam = 3.0f, secondsBeforeStartingCircleCamFirstTime = 3.5f;

    //private int[] playerHealth = new int[2];
    //private int playerHealth1;
    
    //public int PlayerHealth1
    //{
    //    get
    //    {
    //        return playerHealth1;
    //    }
    //    set
    //    {
    //        playerHealth1 += value;
    //        healthBar[0].ScaleHealthBar(playerHealth1, false);

    //        if (playerHealth1 <= 0)
    //        {
    //            StartCoroutine(KillPlayer(0, 1));
    //        }
    //    }
    //}

    //private int playerHealth2;

    //public int PlayerHealth2
    //{
    //    get
    //    {
    //        return playerHealth2;
    //    }
    //    set
    //    {
    //        playerHealth2 += value;
    //        healthBar[1].ScaleHealthBar(playerHealth2, false);

    //        if (playerHealth2 <= 0)
    //        {
    //            StartCoroutine(KillPlayer(1, 0));
    //        }
    //    }
    //}

    private float[] playerStarPower = new float[2];
    private int indexOfWinner;
    private int indexOfLoser;
    private float scoreDisparityAveraged;
    private float damageDealt;
    private float phraseTimeElapsed;
    private int bonus;

    private int starPowerMovePastHitCount = 0;
    private float minorSPDamage;
    private float majorSPDamage;

    //use these to create star power bonuses
    private int timesWonInRowAfterFirst;
    private int indexOfLastRoundWinner;
    private Animator[] comboImageAnimator = new Animator[2];

    private Color32 countdownTextColor;
    //private Sprite player1NextImage, player2NextImage;
    //private SpriteRenderer player1PortraitRenderer, player2PortraitRenderer;
    //private string player1ActionTextNext, player2ActionTextNext;
    private AudioSource audioSource;
    //private AudioSource player1AudioSource, player2AudioSource;
    //private HealthBar[] healthBar = new HealthBar[2];
    //private HealthBar[] starPowerBar = new HealthBar[2];
    private Animator[] playerAnimator = new Animator[2];
    private AudioComparisonManager audioComparisonSript;
    private MainSceneCameraManager mainCameraManager;

    private StarPowerQTE starPowerManager;

    private bool canMakeChoice;
    private bool player1HasMadeChoice, player2HasMadeChoice;
    private string player1Choice, player2Choice;
    private string attack = "attack", dodge = "block", sweep = "grapple";

    private bool isPerformingStarPowerMove;
    private bool playerIsDead;
    private bool starPowerHasBeenPerformed;

    private bool circleCamHasStarted;

    #endregion

    public static event Action DecideWinner;
    //public static event Action StartCircleCam;

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

        starPowerManager = GetComponent<StarPowerQTE>();
        mainCameraManager = mainCameraManagerGameObject.GetComponent<MainSceneCameraManager>();
    }

    void Start()
    {
        //new stuff
        player[0].Health = 100;
        player[1].Health = 100;
        player[0].StarPower = 0;
        player[1].StarPower = 0;

        countdownText.text = "";
        //winnerText.text = "";
        countdownTextColor = countdownText.color;
        indexOfLastRoundWinner = 3;

        //comboImageAnimator[0] = comboImage[0].GetComponent<Animator>();
        //comboImageAnimator[1] = comboImage[1].GetComponent<Animator>();
        //comboText[0].text = "";
        //comboText[1].text = "";

        audioSource = GetComponent<AudioSource>();
        //audioComparisonSript = GetComponent<AudioComparisonManager>();
    }

    void Update()
    {
        phraseTimeElapsed += Time.deltaTime;

        if (isDebugging)
        {
            if (Input.GetButtonDown("DebugDownHalfPlayer1Health"))
            {
                //PlayerHealth1 -= 50;
            }
            if (Input.GetButtonDown("DebugUpHalfPlayer1StarPower"))
            {
                playerStarPower[0] += 50;
            }
        }
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
            //this is so the phrase values stay consistent even when a star power move is being performed
            DecideWinner?.Invoke();
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
            indexOfWinner = 2;
        }
        player[indexOfWinner].Win();
        StartCoroutine(ShowVictoryText());
    }

    IEnumerator ShowVictoryText()
    {
        victorySprite.SetActive(true);
        yield return new WaitForSeconds(5);
        victorySprite.SetActive(false);
    }

    private void OnPlayerHasFullStarPower(int indexOfPlayer)
    {
        isPerformingStarPowerMove = true;
        starPowerManager.StartQTE(indexOfPlayer);
        player[0].StarPowerMoveIsHappening = true;
        player[1].StarPowerMoveIsHappening = true;
        //Need to pause all other action during the star power move and then trigger star power manager
    }

    private void OnStarPowerMoveEnds(int indexOfAttackedPlayer, int indexOfOtherPlayer)
    {
        player[indexOfOtherPlayer].StarPower = 0;
        player[0].StarPowerMoveIsHappening = false;
        player[1].StarPowerMoveIsHappening = false;

        if (player[indexOfAttackedPlayer].Health <= 0)
        {
            //Kill PLayer
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
            //yield return new WaitForSeconds(1);
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
            //Add choosing countdown graphic, maybe a bar or a round pie chart type thing?
            yield return new WaitForSeconds(secondsForChoice);
            countdownText.text = "";
            countdownText.color = countdownTextColor;
            player[0].CanMakeChoice = false;
            player[1].CanMakeChoice = false;

            //DecideWinner?.Invoke();

            // The disparity average is based on how different the two players' scores were,
            // and how long the phrase in question was. The higher the score, the worse the
            // loser did compared to the winner during the phrase. This value is applied to bonuses.

            //scoreDisparityAveraged = audioComparisonSript.ScoreDisparity / phraseTimeElapsed;

            //disparityText.text = scoreDisparityAveraged.ToString();
            //testTimerText.text = phraseTimeElapsed.ToString();
            phraseTimeElapsed = 0.0f;

            ExecuteCombat();
        }
    }

    private void ExecuteCombat()
    {
        //indexOfWinner = audioComparisonSript.IndexOfWinner;
        //indexOfLoser = audioComparisonSript.IndexOfLoser;
        var rand = new System.Random();
        indexOfWinner = rand.Next(0, 2);
        scoreDisparityAveraged = rand.Next(5, 40);

        if (indexOfWinner == 0)
        {
            indexOfLoser = 1;
        }
        else
        {
            indexOfLoser = 0;
        }
        Debug.Log($"Index of winner: {indexOfWinner}");

        //StartCoroutine(DecideComboAndDisplayImage(indexOfWinner, indexOfLoser));

        // The - 1 is there so we can control for one player only doing a bit better than the other,
        // but I may take it out later after playtesting.
        bonus = (int)(scoreDisparityAveraged / bonusDividingFactor) - 1;
        if (bonus < 1)
        {
            // This is to make sure that a fraction bonus below 1 doesn't cause the attack damage to go below the base number.
            bonus = 1;
        }
        Debug.Log($"Bonus: {bonus}");

        player[0].ExecuteQueuedCombatMove();
        player[1].ExecuteQueuedCombatMove();

        StartCoroutine(WaitThenResetCamera());
    }

    private IEnumerator WaitThenResetCamera()
    {
        float seconds = circleCamHasStarted ? 
            secondsBeforeResettingCircleCam : secondsBeforeStartingCircleCamFirstTime;

        if (!circleCamHasStarted)
            circleCamHasStarted = true;

        yield return new WaitForSeconds(seconds);

        if (!isPerformingStarPowerMove)
        {
            mainCameraManager.StartCircleCam();
        }
    }

    //Old Stuff
    //private void Player1Attack()
    //{
    //    if (player2Choice == attack)
    //    {
    //        // Player with higher score wins, deals base damage.
    //        damageDealt = baseAttackDamage + bonus;
    //        //playerHealth[indexOfLoser] -= (int)damageDealt;
    //        if (indexOfWinner == 0)
    //        {
    //            PlayerHealth2 -= (int)damageDealt;
    //        }
    //        else
    //        {
    //            PlayerHealth1 -= (int)damageDealt;
    //        }

    //        //healthBar[indexOfLoser].ScaleHealthBar((int)damageDealt, false);
    //        playerAnimator[indexOfWinner].SetTrigger("kick");
    //        //playerAnimator[indexOfLoser].SetTrigger("getHit");
    //        StartCoroutine(DelayAnimation(getHitAnimationDelay, indexOfLoser, "getHit"));
    //    }
    //    else if (player2Choice == dodge)
    //    {
    //        // Blocking player negates attacking player's hit.
    //        // They deal a small amount of damage if they win singing-wise.
    //        if (indexOfWinner == 1)
    //        {
    //            damageDealt = baseParryDamage + (bonus / bonusParryDividingValue);
    //            //playerHealth[0] -= (int)damageDealt;
    //            //healthBar[0].ScaleHealthBar((int)damageDealt, false);
    //            PlayerHealth1 -= (int)damageDealt;
    //        }
    //        playerAnimator[0].SetTrigger("kick");
    //        playerAnimator[1].SetTrigger("dodge");
    //    }
    //    else if (player2Choice == sweep)
    //    {
    //        // Attacking player deals damage to grappling player,
    //        // with additional damage dealt if the attacking player wins singing-wise.
    //        damageDealt = baseAttackDamage;

    //        if (indexOfWinner == 0)
    //        {
    //            damageDealt += bonus;
    //        }
    //        PlayerHealth2 -= (int)damageDealt;
    //        //playerHealth[1] -= (int)damageDealt;
    //        //healthBar[1].ScaleHealthBar((int)damageDealt, false);
    //        playerAnimator[0].SetTrigger("kick");
    //        //playerAnimator[1].SetTrigger("getHit");
    //        StartCoroutine(DelayAnimation(getHitAnimationDelay, 1, "getHit"));
    //    }

    //    ResetValues();
    //}

    //private void Player1Dodge()
    //{
    //    if (player2Choice == attack)
    //    {
    //        if (indexOfWinner == 0)
    //        {
    //            damageDealt = baseParryDamage + (bonus / bonusParryDividingValue);
    //            //playerHealth[1] -= (int)damageDealt;
    //            //healthBar[1].ScaleHealthBar((int)damageDealt, false);
    //            playerHealth2 -= (int)damageDealt;
    //        }
    //        playerAnimator[0].SetTrigger("dodge");
    //        playerAnimator[1].SetTrigger("kick");
    //    }
    //    else if (player2Choice == dodge)
    //    {
    //        playerAnimator[indexOfLoser].SetTrigger("dodge");
    //        playerAnimator[indexOfWinner].SetTrigger("dodgeToSad");
    //    }
    //    else if (player2Choice == sweep)
    //    {
    //        damageDealt = baseStarPowerIncrease;
    //        if (indexOfWinner == 1)
    //        {
    //            damageDealt += bonus;
    //        }
    //        playerStarPower[1] += damageDealt;
    //        starPowerBar[1].ScaleHealthBar((int)damageDealt, true);
    //        playerAnimator[1].SetTrigger("sweep");
    //        playerAnimator[0].SetTrigger("fall");
    //    }

    //    ResetValues();
    //}

    //private void Player1Sweep()
    //{
    //    if (player2Choice == attack)
    //    {
    //        damageDealt = baseAttackDamage;
    //        if (indexOfWinner == 1)
    //        {
    //            damageDealt += bonus;
    //        }
    //        PlayerHealth1 -= (int)damageDealt;
    //        //playerHealth[0] -= (int)damageDealt;
    //        //healthBar[0].ScaleHealthBar((int)damageDealt, false);
    //        StartCoroutine(DelayAnimation(getHitAnimationDelay, 0, "getHit"));
    //        playerAnimator[1].SetTrigger("kick");
    //    }
    //    else if (player2Choice == dodge)
    //    {
    //        //damageDealt = baseStarPowerDecrease;
    //        //if (indexOfWinner == 0)
    //        //{
    //        //    damageDealt += bonus;
    //        //}
    //        //playerStarPower[1] -= damageDealt;
    //        //starPowerBar[1].ScaleHealthBar((int)damageDealt, false);
    //        //playerAnimator[0].SetTrigger("sweep");
    //        //playerAnimator[1].SetTrigger("fall");

    //        damageDealt = baseStarPowerIncrease;
    //        if (indexOfWinner == 0)
    //        {
    //            damageDealt += bonus;
    //        }
    //        playerStarPower[0] += damageDealt;
    //        starPowerBar[0].ScaleHealthBar((int)damageDealt, true);
    //        playerAnimator[0].SetTrigger("sweep");
    //        playerAnimator[1].SetTrigger("fall");
    //    }
    //    else if (player2Choice == sweep)
    //    {
    //        // player with higher singing score gets star power increased
    //        // note: chnaged to lowered

    //        //damageDealt = baseStarPowerIncrease + bonus;
    //        //playerStarPower[indexOfWinner] += damageDealt;
    //        //starPowerBar[indexOfWinner].ScaleHealthBar((int)damageDealt, true);
    //        //playerAnimator[indexOfWinner].SetTrigger("sweep");
    //        //playerAnimator[indexOfLoser].SetTrigger("fall");
    //        damageDealt = baseStarPowerDecrease + bonus;
    //        playerStarPower[indexOfLoser] -= damageDealt;
    //        starPowerBar[indexOfLoser].ScaleHealthBar((int)damageDealt, false);
    //        playerAnimator[indexOfWinner].SetTrigger("sweep");
    //        playerAnimator[indexOfLoser].SetTrigger("fall");
    //    }

    //    ResetValues();
    //}

    //private void Player1DidNotChoose()
    //{
    //    Debug.Log("Player 1 did not make a choice.");
    //    if (player2Choice == attack)
    //    {
    //        damageDealt = baseAttackDamage;

    //        if (indexOfWinner == 1)
    //        {
    //            damageDealt += bonus;
    //        }
    //        //playerHealth[0] -= (int)damageDealt;
    //        PlayerHealth1 -= (int)damageDealt;
    //        //healthBar[0].ScaleHealthBar((int)damageDealt, false);
    //        playerAnimator[1].SetTrigger("kick");
    //        StartCoroutine(DelayAnimation(getHitAnimationDelay, 0, "getHit"));
    //    }
    //    else if (player2Choice == dodge)
    //    {
    //        playerAnimator[1].SetTrigger("dodge");
    //        //playerAnimator[0].SetTrigger("dissapointed");
    //    }
    //    else if (player2Choice == sweep)
    //    {
    //        //damageDealt = baseStarPowerDecrease;
    //        //if (indexOfWinner == 1)
    //        //{
    //        //    damageDealt += bonus;
    //        //}
    //        //playerStarPower[0] -= damageDealt;
    //        //starPowerBar[0].ScaleHealthBar((int)damageDealt, false);
    //        //playerAnimator[1].SetTrigger("sweep");
    //        //playerAnimator[0].SetTrigger("fall");

    //        damageDealt = baseStarPowerIncrease;
    //        if (indexOfWinner == 1)
    //        {
    //            damageDealt += bonus;
    //        }
    //        playerStarPower[1] += damageDealt;
    //        starPowerBar[1].ScaleHealthBar((int)damageDealt, true);
    //        playerAnimator[1].SetTrigger("sweep");
    //        playerAnimator[0].SetTrigger("fall");
    //    }

    //    ResetValues();
    //}

    //private void Player2DidNotChoose()
    //{
    //    Debug.Log("Player 2 did not make a choice.");
    //    if (player1Choice == attack)
    //    {
    //        damageDealt = baseAttackDamage;

    //        if (indexOfWinner == 0)
    //        {
    //            damageDealt += bonus;
    //        }
    //        //playerHealth[1] -= (int)damageDealt;
    //        //healthBar[1].ScaleHealthBar((int)damageDealt, false);
    //        PlayerHealth2 -= (int)damageDealt;
    //        playerAnimator[0].SetTrigger("kick");
    //        StartCoroutine(DelayAnimation(getHitAnimationDelay, 1, "getHit"));
    //    }
    //    else if (player1Choice == dodge)
    //    {
    //        playerAnimator[0].SetTrigger("dodge");
    //        playerAnimator[1].SetTrigger("dissapointed");
    //    }
    //    else if (player1Choice == sweep)
    //    {
    //        //damageDealt = baseStarPowerDecrease;
    //        //if (indexOfWinner == 0)
    //        //{
    //        //    damageDealt += bonus;
    //        //}
    //        //playerStarPower[1] -= damageDealt;
    //        //starPowerBar[1].ScaleHealthBar((int)damageDealt, false);
    //        //playerAnimator[0].SetTrigger("sweep");
    //        //playerAnimator[1].SetTrigger("fall");

    //        damageDealt = baseStarPowerIncrease;
    //        if (indexOfWinner == 0)
    //        {
    //            damageDealt += bonus;
    //        }
    //        playerStarPower[0] += damageDealt;
    //        starPowerBar[0].ScaleHealthBar((int)damageDealt, true);
    //        playerAnimator[0].SetTrigger("sweep");
    //        playerAnimator[1].SetTrigger("fall");
    //    }

    //    ResetValues();
    //}

    //private void ResetValues()
    //{
    //    player1HasMadeChoice = false;
    //    player2HasMadeChoice = false;
    //    player1Choice = " ";
    //    player2Choice = " ";
    //    player1PortraitRenderer.sprite = player1RegularPortrait;
    //    player2PortraitRenderer.sprite = player2RegularPortrait;
    //    //Debug.Log($"Player1 health: {playerHealth[0]}");
    //    //Debug.Log($"Player2 health: {playerHealth[1]}");
    //    Debug.Log($"Player1 health: {PlayerHealth1}");
    //    Debug.Log($"Player2 health: {PlayerHealth2}");
    //    Debug.Log($"Player1 starpower: {playerStarPower[0]}");
    //    Debug.Log($"Player2 starpower: {playerStarPower[1]}");

    //    //CheckForStarPower();
    //    //CheckForDeath();
    //}

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
        sparks[indexOfWinner].Play();
        yield return new WaitForSeconds(comboImageAnimationDelay);
        comboImageAnimator[indexOfWinner].SetTrigger("comboOut");
        comboText[indexOfWinner].text = "";
    }

}
