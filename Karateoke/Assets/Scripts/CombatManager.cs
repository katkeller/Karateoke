using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI player1ActionText, player2ActionText, countdownText, winnerText;

    [SerializeField]
    private Button startButton;

    [SerializeField]
    private float secondsForChoice = 1.5f;

    [SerializeField]
    private Color32 lastNumberColor;

    [SerializeField]
    private AudioClip[] countdownClip = new AudioClip[4];

    [SerializeField]
    private Sprite attackIcon, blockIcon, grappleIcon;

    [SerializeField]
    private GameObject[] playerModel = new GameObject[2];

    [SerializeField]
    private GameObject[] comboImage = new GameObject[2];

    [SerializeField]
    private ParticleSystem[] sparks = new ParticleSystem[2];

    [SerializeField]
    private TextMeshProUGUI[] comboText = new TextMeshProUGUI[2];

    [SerializeField]
    private float comboImageAnimationDelay = 1.5f;

    [SerializeField]
    private float getHitAnimationDelay = 1.0f, victoryAnimationDelay = 2.0f;

    [SerializeField]
    private GameObject[] playerHealthBarGameObject = new GameObject[2];

    [SerializeField]
    private GameObject[] playerStarPowerBarGameObject = new GameObject[2];

    [SerializeField]
    private int baseAttackDamage = 5, baseParryDamage = 1, baseStarPowerIncrease = 10;

    [SerializeField]
    private float baseStarPowerDecrease = 5f, baseStarPowerComboIncrease = 1.5f;

    [SerializeField]
    private float starPowerMoveLengthInSeconds = 5.0f;

    [Tooltip("The number that the disparity average will be divided by to get the value added to damage.")]
    [SerializeField]
    private float bonusDividingFactor = 100.0f, bonusParryDividingValue = 2.0f;

    private int[] playerHealth = new int[2];
    private float[] playerStarPower = new float[2];
    private int indexOfWinner;
    private int indexOfLoser;
    private float scoreDisparityAveraged;
    private float damageDealt;
    private float phraseTimeElapsed;

    //use these to create star power bonuses
    private int winsInARowCount;
    private int indexOfLastRoundWinner;
    private Animator[] comboImageAnimator = new Animator[2];

    private Color32 countdownTextColor;
    private Sprite player1NextImage, player2NextImage;
    private string player1ActionTextNext, player2ActionTextNext;
    private AudioSource audioSource;
    private HealthBar[] healthBar = new HealthBar[2];
    private HealthBar[] starPowerBar = new HealthBar[2];
    private Animator[] playerAnimator = new Animator[2];
    private AudioComparisonManager audioComparisonSript;

    private bool canMakeChoice;
    private bool player1HasMadeChoice, player2HasMadeChoice;
    private string player1Choice, player2Choice;
    private string attack = "attack", dodge = "block", sweep = "grapple";

    private bool isPerformingStarPowerMove;
    private bool playerIsDead;

    public static event Action DecideWinner;

    public void StartAnimations()
    {
        playerAnimator[0].SetTrigger("start");
        playerAnimator[1].SetTrigger("start");
        startButton.enabled = false;
        startButton.gameObject.SetActive(false);
    }

    void Start()
    {
        playerHealth[0] = 100;
        playerHealth[1] = 100;
        playerStarPower[0] = 0;
        playerStarPower[1] = 0;
        countdownText.text = "";
        winnerText.text = "";
        player1ActionText.text = "";
        player2ActionText.text = "";
        countdownTextColor = countdownText.color;

        healthBar[0] = playerHealthBarGameObject[0].GetComponent<HealthBar>();
        healthBar[1] = playerHealthBarGameObject[1].GetComponent<HealthBar>();
        starPowerBar[0] = playerStarPowerBarGameObject[0].GetComponent<HealthBar>();
        starPowerBar[1] = playerStarPowerBarGameObject[1].GetComponent<HealthBar>();
        playerAnimator[0] = playerModel[0].GetComponent<Animator>();
        playerAnimator[1] = playerModel[1].GetComponent<Animator>();
        comboImageAnimator[0] = comboImage[0].GetComponent<Animator>();
        comboImageAnimator[1] = comboImage[1].GetComponent<Animator>();
        comboText[0].text = "";
        comboText[1].text = "";

        audioSource = GetComponent<AudioSource>();
        audioComparisonSript = GetComponent<AudioComparisonManager>();
    }

    void Update()
    {
        phraseTimeElapsed += Time.deltaTime;

        CheckForStarPowerOrDeath();

        if (canMakeChoice && !isPerformingStarPowerMove && !playerIsDead)
        {
            //we should add a simple (but unique to each player) soundeffect for when they choose

            if (Input.GetButtonDown("Player1Attack") && !player1HasMadeChoice)
            {
                //player1ActionText.text = "Player 1 attacks!";
                player1Choice = attack;
                player1HasMadeChoice = true;
                //player1NextImage = attackIcon;
                player1ActionTextNext = "Attack";

            }
            if (Input.GetButtonDown("Player1Block") && !player1HasMadeChoice)
            {
               // player1ActionText.text = "Player 1 blocks!";
                player1Choice = dodge;
                player1HasMadeChoice = true;
                //player1NextImage = blockIcon;
                player1ActionTextNext = "Dodge";
            }
            if (Input.GetButtonDown("Player1Grapple") && !player1HasMadeChoice)
            {
                //player1ActionText.text = "Player 1 grapples!";
                player1Choice = sweep;
                player1HasMadeChoice = true;
                //player1NextImage = grappleIcon;
                player1ActionTextNext = "Sweep";
            }
            if (Input.GetButtonDown("Player2Attack") && !player2HasMadeChoice)
            {
                //player2ActionText.text = "Player 2 attacks!";
                player2Choice = attack;
                player2HasMadeChoice = true;
                //player2NextImage = attackIcon;
                player2ActionTextNext = "Attack";
            }
            if (Input.GetButtonDown("Player2Block") && !player2HasMadeChoice)
            {
                //player2ActionText.text = "Player 2 blocks!";
                player2Choice = dodge;
                player2HasMadeChoice = true;
                //player2NextImage = blockIcon;
                player2ActionTextNext = "Dodge";
            }
            if (Input.GetButtonDown("Player2Grapple") && !player2HasMadeChoice)
            {
                //player2ActionText.text = "Player 2 grapples!";
                player2Choice = sweep;
                player2HasMadeChoice = true;
                //player2NextImage = grappleIcon;
                player2ActionTextNext = "Sweep";
            }
        }
    }

    private void CheckForStarPowerOrDeath()
    {
        if (!playerIsDead)
        {
            if (playerHealth[0] <= 0)
            {
                StartCoroutine(KillPlayer(0, 1));
            }
            if (playerHealth[1] <= 0)
            {
                StartCoroutine(KillPlayer(1, 0));
            }
            if (playerStarPower[0] >= 100)
            {
                StartCoroutine(StarPowerMove(0));
            }
            if (playerStarPower[1] >= 100)
            {
                StartCoroutine(StarPowerMove(1));
            }
        }
    }

    IEnumerator StarPowerMove(int indexOfPlayer)
    {
        isPerformingStarPowerMove = true;
        //animation
        //damage
        yield return new WaitForSeconds(starPowerMoveLengthInSeconds);
        isPerformingStarPowerMove = false;
    }

    IEnumerator KillPlayer(int indexOfLoser, int indexOfWinner)
    {
        playerIsDead = true;
        playerAnimator[indexOfLoser].SetTrigger("die");
        StartCoroutine(DelayAnimation(victoryAnimationDelay, indexOfWinner, "victory"));
        winnerText.text = "VICTORY!";

        yield return new WaitForSeconds(5);
        winnerText.text = "";
        if (indexOfWinner == 0)
        {
            playerModel[0].transform.Rotate(0, 180, 0);
        }
        else
        {
            playerModel[1].transform.Rotate(0, -180, 0);
        }
        playerAnimator[indexOfWinner].SetTrigger("sing");
    }

    private void OnEndOfPhrase()
    {
        if (!isPerformingStarPowerMove && !playerIsDead)
        {
            StartCoroutine(AllowPlayersToMakeChoice());
            Debug.Log("Phrase end has been triggered!");
        }
        else
        {
            //this is so the phrase values stay consistent even when a star power move is performed
            DecideWinner?.Invoke();
        }
    }

    private void OnEnable()
    {
        PhraseEndTrigger.EndOfPhrase += OnEndOfPhrase;
    }

    private void OnDisable()
    {
        PhraseEndTrigger.EndOfPhrase -= OnEndOfPhrase;
    }

    IEnumerator AllowPlayersToMakeChoice()
    {
        player1HasMadeChoice = false;
        player1Choice = " ";
        player2HasMadeChoice = false;
        player2Choice = " ";

        yield return new WaitForSeconds(1);
        countdownText.text = "3";
        audioSource.PlayOneShot(countdownClip[0]);
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        audioSource.PlayOneShot(countdownClip[1]);
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        audioSource.PlayOneShot(countdownClip[2]);
        canMakeChoice = true;
        yield return new WaitForSeconds(1);
        countdownText.color = lastNumberColor;
        countdownText.text = "CHOOSE!";
        audioSource.PlayOneShot(countdownClip[3]);
        //Add choosing countdown graphic, maybe a bar or a round pie chart type thing?
        yield return new WaitForSeconds(secondsForChoice);
        countdownText.text = "";
        countdownText.color = countdownTextColor;
        canMakeChoice = false;

        UpdateActionText();
        DecideWinner?.Invoke();

        // The disparity average is based on how different the two players' scores were,
        // and how long the phrase in question was. The higher the score, the worse the
        // loser did compared to the winner during the phrase. This value is applied to bonuses.
        scoreDisparityAveraged = audioComparisonSript.ScoreDisparity / phraseTimeElapsed;
        //disparityText.text = scoreDisparityAveraged.ToString();
        //testTimerText.text = phraseTimeElapsed.ToString();
        phraseTimeElapsed = 0.0f;

        DecideOnDamage();
    }

    private void UpdateActionText()
    {
        // We should add a small animation to these as a stretch goal

        if (player1HasMadeChoice)
        {
            StartCoroutine(UpdatePlayer1ActionText(player1ActionTextNext));
        }
        if (player2HasMadeChoice)
        {
            StartCoroutine(UpdatePlayer2ActionText(player2ActionTextNext));
        }
    }

    IEnumerator UpdatePlayer1ActionText(string text)
    {
        player1ActionText.text = text;
        yield return new WaitForSeconds(2);
        player1ActionText.text = " ";
    }

    IEnumerator UpdatePlayer2ActionText(string text)
    {
        player2ActionText.text = text;
        yield return new WaitForSeconds(2);
        player2ActionText.text = " ";
    }

    private void DecideOnDamage()
    {
        damageDealt = 0;
        indexOfWinner = audioComparisonSript.IndexOfWinner;
        indexOfLoser = audioComparisonSript.IndexOfLoser;

        StartCoroutine(DecideComboAndDisplayImage(indexOfWinner, indexOfLoser));

        // The - 1 is there so we can control for one player only doing a bit better than the other,
        // but I may take it out later after playtesting.
        int bonus = (int)(scoreDisparityAveraged / bonusDividingFactor) - 1;
        Debug.Log($"Bonus: {bonus}");

        //scoreDisparity = audioComparisonSript.ScoreDisparity;

        if(player1HasMadeChoice && player2HasMadeChoice)
        {
            if (player1Choice == attack && player2Choice == attack)
            {
                // Player with higher score wins, deals base damage.
                damageDealt = baseAttackDamage + bonus;
                playerHealth[indexOfLoser] -= (int)damageDealt;

                healthBar[indexOfLoser].ScaleHealthBar((int)damageDealt, false);
                playerAnimator[indexOfWinner].SetTrigger("kick");
                //playerAnimator[indexOfLoser].SetTrigger("getHit");
                StartCoroutine(DelayAnimation(getHitAnimationDelay, indexOfLoser, "getHit"));
            }
            else if (player1Choice == attack && player2Choice == dodge)
            {
                // Blocking player negates attacking player's hit.
                // They deal a small amount of damage if they win singing-wise.
                if (indexOfWinner == 1)
                {
                    damageDealt = baseParryDamage + (bonus / bonusParryDividingValue);
                    playerHealth[0] -= (int)damageDealt;
                    healthBar[0].ScaleHealthBar((int)damageDealt, false);
                }
                playerAnimator[0].SetTrigger("kick");
                playerAnimator[1].SetTrigger("dodge");
            }
            else if (player1Choice == attack && player2Choice == sweep)
            {
                // Attacking player deals damage to grappling player,
                // with additional damage dealt if the attacking player wins singing-wise.
                damageDealt = baseAttackDamage;

                if (indexOfWinner == 0)
                {
                    damageDealt += bonus;
                }
                playerHealth[1] -= (int)damageDealt;
                healthBar[1].ScaleHealthBar((int)damageDealt, false);
                playerAnimator[0].SetTrigger("kick");
                //playerAnimator[1].SetTrigger("getHit");
                StartCoroutine(DelayAnimation(getHitAnimationDelay, 1, "getHit"));
            }
            else if (player1Choice == dodge && player2Choice == attack)
            {
                if (indexOfWinner == 0)
                {
                    damageDealt = baseParryDamage + (bonus / bonusParryDividingValue);
                    playerHealth[1] -= (int)damageDealt;
                    healthBar[1].ScaleHealthBar((int)damageDealt, false);
                }
                playerAnimator[0].SetTrigger("dodge");
                playerAnimator[1].SetTrigger("kick");
            }
            else if (player1Choice == dodge && player2Choice == dodge)
            {
                playerAnimator[indexOfLoser].SetTrigger("dodge");
                playerAnimator[indexOfWinner].SetTrigger("dodgeToSad");
            }
            else if (player1Choice == dodge && player2Choice == sweep)
            {
                // The blocking player has their star power lowered
                damageDealt = baseStarPowerDecrease;
                if (indexOfWinner == 1)
                {
                    // Placeholder value?
                    damageDealt += 5;
                }
                playerStarPower[0] -= damageDealt;
                //need to send a negative value to the scale star power bar so it will decrease?
                starPowerBar[0].ScaleHealthBar((int)damageDealt, false);
                Debug.Log($"damage dealt to player 1 star power: {-(int)damageDealt}");
                playerAnimator[0].SetTrigger("fall");
                playerAnimator[1].SetTrigger("sweep");
                Debug.Log("player 1 should be swept by player 2");
            }
            else if (player1Choice == sweep && player2Choice == attack)
            {
                damageDealt = baseAttackDamage;
                if (indexOfWinner == 1)
                {
                    damageDealt += bonus;
                }
                playerHealth[0] -= (int)damageDealt;
                healthBar[0].ScaleHealthBar((int)damageDealt, false);
                //playerAnimator[0].SetTrigger("getHit");
                StartCoroutine(DelayAnimation(getHitAnimationDelay, 0, "getHit"));
                playerAnimator[1].SetTrigger("kick");
            }
            else if (player1Choice == sweep && player2Choice == dodge)
            {
                damageDealt = baseStarPowerDecrease;
                if (indexOfWinner == 0)
                {
                    damageDealt += bonus;
                }
                playerStarPower[1] -= damageDealt;
                playerAnimator[0].SetTrigger("sweep");
                playerAnimator[1].SetTrigger("fall");
            }
            else if (player1Choice == sweep && player2Choice == sweep)
            {
                // player with higher singing score gets star power increased
                damageDealt = baseStarPowerIncrease + bonus;
                playerStarPower[indexOfWinner] += damageDealt;
                playerAnimator[indexOfWinner].SetTrigger("sweep");
                playerAnimator[indexOfLoser].SetTrigger("fall");
            }
        }
        else if (player1HasMadeChoice && !player2HasMadeChoice)
        {
            if (player1Choice == attack)
            {
                damageDealt = baseAttackDamage;

                if (indexOfWinner == 0)
                {
                    damageDealt += bonus;
                }
                playerHealth[1] -= (int)damageDealt;
                healthBar[1].ScaleHealthBar((int)damageDealt, false);
                playerAnimator[0].SetTrigger("kick");
                StartCoroutine(DelayAnimation(getHitAnimationDelay, 1, "getHit"));
            }
            else if (player1Choice == dodge)
            {
                playerAnimator[0].SetTrigger("dodge");
                playerAnimator[1].SetTrigger("dissapointed");
            }
            else if (player1Choice == sweep)
            {
                damageDealt = baseStarPowerDecrease;
                if (indexOfWinner == 0)
                {
                    damageDealt += bonus;
                }
                playerStarPower[1] -= damageDealt;
                playerAnimator[0].SetTrigger("sweep");
                playerAnimator[1].SetTrigger("fall");
            }
        }
        else if (!player1HasMadeChoice && player2HasMadeChoice)
        {
            if (player2Choice == attack)
            {
                damageDealt = baseAttackDamage;

                if (indexOfWinner == 1)
                {
                    damageDealt += bonus;
                }
                playerHealth[0] -= (int)damageDealt;
                healthBar[0].ScaleHealthBar((int)damageDealt, false);
                playerAnimator[1].SetTrigger("kick");
                StartCoroutine(DelayAnimation(getHitAnimationDelay, 0, "getHit"));
            }
            else if (player2Choice == dodge)
            {
                playerAnimator[1].SetTrigger("dodge");
                playerAnimator[0].SetTrigger("dissapointed");
            }
            else if (player2Choice == sweep)
            {
                damageDealt = baseStarPowerDecrease;
                if (indexOfWinner == 1)
                {
                    damageDealt += bonus;
                }
                playerStarPower[0] -= damageDealt;
                playerAnimator[1].SetTrigger("sweep");
                playerAnimator[0].SetTrigger("fall");
            }
        }
        else if(!player1HasMadeChoice && !player2HasMadeChoice)
        {
            //no one chose anything, so they both look dissapointed in themselves.
            playerAnimator[indexOfWinner].SetTrigger("dissapointed");
        }

        player1HasMadeChoice = false;
        player2HasMadeChoice = false;
        player1Choice = " ";
        player2Choice = " ";
        Debug.Log($"Player1 health: {playerHealth[0]}");
        Debug.Log($"Player2 health: {playerHealth[1]}");
    }

    IEnumerator DecideComboAndDisplayImage(int indexOfWinner, int indexOfLoser)
    {
        if (indexOfWinner == indexOfLastRoundWinner)
        {
            winsInARowCount++;
            if (winsInARowCount == 1)
            {
                playerAnimator[indexOfWinner].SetBool("isOnFirstBonus", true);
                //playerAnimator[indexOfWinner].SetBool("isOnSecondBonus", false);
            }
            else if (winsInARowCount >= 2)
            {
                //playerAnimator[indexOfWinner].SetBool("isOnFirstBonus", false);
                playerAnimator[indexOfWinner].SetBool("isOnSecondBonus", true);
            }

            playerStarPower[indexOfWinner] += (baseStarPowerComboIncrease * winsInARowCount);
            //change starpowerbar

        }
        else
        {
            winsInARowCount = 0;
            playerAnimator[indexOfLoser].SetBool("isOnFirstBonus", false);
            playerAnimator[indexOfWinner].SetBool("isOnSecondBonus", false);
        }
        indexOfLastRoundWinner = indexOfWinner;

        comboImageAnimator[indexOfWinner].SetTrigger("comboIn");
        comboText[indexOfWinner].text = "star\npower";
        sparks[indexOfWinner].Play();
        yield return new WaitForSeconds(comboImageAnimationDelay);
        comboImageAnimator[indexOfWinner].SetTrigger("comboOut");
        comboText[indexOfWinner].text = "";
    }

    IEnumerator DelayAnimation(float time, int indexOfPlayer, string animationTrigger)
    {
        yield return new WaitForSeconds(time);
        playerAnimator[indexOfPlayer].SetTrigger(animationTrigger);
    }
}
