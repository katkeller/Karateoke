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
    private ParticleSystem[] starPowerSparks = new ParticleSystem[2];

    [SerializeField]
    private TextMeshProUGUI[] comboText = new TextMeshProUGUI[2];

    [SerializeField]
    private GameObject player1Portrait, player2Portrait;

    [SerializeField]
    private Sprite player1RegularPortrait, player1NoChoicePortrait, player1ChoiceMadePortrait, player2RegularPortrait, player2NoChoicePortrait, player2ChoiceMadePortrait;

    [SerializeField]
    private float comboImageAnimationDelay = 1.5f;

    [SerializeField]
    private float getHitAnimationDelay = 1.0f, victoryAnimationDelay = 2.0f;

    [SerializeField]
    private GameObject[] playerHealthBarGameObject = new GameObject[2];

    [SerializeField]
    private GameObject[] playerStarPowerBarGameObject = new GameObject[2];

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
    private float bonusDividingFactor = 100.0f, bonusParryDividingValue = 2.0f;

    private int[] playerHealth = new int[2];
    private float[] playerStarPower = new float[2];
    private int indexOfWinner;
    private int indexOfLoser;
    private float scoreDisparityAveraged;
    private float damageDealt;
    private float phraseTimeElapsed;
    private int bonus;

    //use these to create star power bonuses
    private int timesWonInRowAfterFirst;
    private int indexOfLastRoundWinner;
    private Animator[] comboImageAnimator = new Animator[2];

    private Color32 countdownTextColor;
    private Sprite player1NextImage, player2NextImage;
    private SpriteRenderer player1PortraitRenderer, player2PortraitRenderer;
    private string player1ActionTextNext, player2ActionTextNext;
    private AudioSource audioSource;
    private AudioSource player1AudioSource, player2AudioSource;
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
    private bool starPowerHasBeenPerformed;

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
        indexOfLastRoundWinner = 3;

        healthBar[0] = playerHealthBarGameObject[0].GetComponent<HealthBar>();
        healthBar[1] = playerHealthBarGameObject[1].GetComponent<HealthBar>();
        starPowerBar[0] = playerStarPowerBarGameObject[0].GetComponent<HealthBar>();
        starPowerBar[1] = playerStarPowerBarGameObject[1].GetComponent<HealthBar>();
        playerAnimator[0] = playerModel[0].GetComponent<Animator>();
        playerAnimator[1] = playerModel[1].GetComponent<Animator>();
        player1AudioSource = playerModel[0].GetComponent<AudioSource>();
        player2AudioSource = playerModel[1].GetComponent<AudioSource>();
        comboImageAnimator[0] = comboImage[0].GetComponent<Animator>();
        comboImageAnimator[1] = comboImage[1].GetComponent<Animator>();
        player1PortraitRenderer = player1Portrait.GetComponent<SpriteRenderer>();
        player2PortraitRenderer = player2Portrait.GetComponent<SpriteRenderer>();
        comboText[0].text = "";
        comboText[1].text = "";

        audioSource = GetComponent<AudioSource>();
        audioComparisonSript = GetComponent<AudioComparisonManager>();
    }

    void Update()
    {
        phraseTimeElapsed += Time.deltaTime;

        if (canMakeChoice && !isPerformingStarPowerMove && !playerIsDead)
        {
            if (Input.GetButtonDown("Player1Attack") && !player1HasMadeChoice)
            {
                player1Choice = attack;
                player1HasMadeChoice = true;
                player1ActionTextNext = "Attack";
                player1PortraitRenderer.sprite = player1ChoiceMadePortrait;
                player1AudioSource.Play();
            }
            if (Input.GetButtonDown("Player1Block") && !player1HasMadeChoice)
            {
                player1Choice = dodge;
                player1HasMadeChoice = true;
                player1ActionTextNext = "Dodge";
                player1PortraitRenderer.sprite = player1ChoiceMadePortrait;
                player1AudioSource.Play();
            }
            if (Input.GetButtonDown("Player1Grapple") && !player1HasMadeChoice)
            {
                player1Choice = sweep;
                player1HasMadeChoice = true;
                player1ActionTextNext = "Sweep";
                player1PortraitRenderer.sprite = player1ChoiceMadePortrait;
                player1AudioSource.Play();
            }
            if (Input.GetButtonDown("Player2Attack") && !player2HasMadeChoice)
            {
                player2Choice = attack;
                player2HasMadeChoice = true;
                player2ActionTextNext = "Attack";
                player2PortraitRenderer.sprite = player2ChoiceMadePortrait;
                player2AudioSource.Play();
            }
            if (Input.GetButtonDown("Player2Block") && !player2HasMadeChoice)
            {
                player2Choice = dodge;
                player2HasMadeChoice = true;
                player2ActionTextNext = "Dodge";
                player2PortraitRenderer.sprite = player2ChoiceMadePortrait;
                player2AudioSource.Play();
            }
            if (Input.GetButtonDown("Player2Grapple") && !player2HasMadeChoice)
            {
                player2Choice = sweep;
                player2HasMadeChoice = true;
                player2ActionTextNext = "Sweep";
                player2PortraitRenderer.sprite = player2ChoiceMadePortrait;
                player2AudioSource.Play();
            }
        }
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
        if (!playerIsDead && !isPerformingStarPowerMove)
        {
            //player1HasMadeChoice = false;
            //player1Choice = " ";
            //player2HasMadeChoice = false;
            //player2Choice = " ";

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
            player1PortraitRenderer.sprite = player1NoChoicePortrait;
            player2PortraitRenderer.sprite = player2NoChoicePortrait;
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
        bonus = (int)(scoreDisparityAveraged / bonusDividingFactor) - 1;
        Debug.Log($"Bonus: {bonus}");

        //scoreDisparity = audioComparisonSript.ScoreDisparity;

        if(player1HasMadeChoice && player2HasMadeChoice)
        {
            if (player1Choice == attack)
            {
                Player1Attack();
            }
            else if (player1Choice == dodge)
            {
                Player1Dodge();
            }
            else if (player1Choice == sweep)
            {
                Player1Sweep();
            }
            else
            {
                Debug.Log("Combat error.");
            }
        }
        else if (player1HasMadeChoice && !player2HasMadeChoice)
        {
            Player2DidNotChoose();
        }
        else if (!player1HasMadeChoice && player2HasMadeChoice)
        {
            Player1DidNotChoose();
        }
        else if(!player1HasMadeChoice && !player2HasMadeChoice)
        {
            playerAnimator[indexOfWinner].SetTrigger("dissapointed");
            ResetValues();
        }
    }

    private void Player1Attack()
    {
        if (player2Choice == attack)
        {
            // Player with higher score wins, deals base damage.
            damageDealt = baseAttackDamage + bonus;
            playerHealth[indexOfLoser] -= (int)damageDealt;

            healthBar[indexOfLoser].ScaleHealthBar((int)damageDealt, false);
            playerAnimator[indexOfWinner].SetTrigger("kick");
            //playerAnimator[indexOfLoser].SetTrigger("getHit");
            StartCoroutine(DelayAnimation(getHitAnimationDelay, indexOfLoser, "getHit"));
        }
        else if (player2Choice == dodge)
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
        else if (player2Choice == sweep)
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

        ResetValues();
    }

    private void Player1Dodge()
    {
        if (player2Choice == attack)
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
        else if (player2Choice == dodge)
        {
            playerAnimator[indexOfLoser].SetTrigger("dodge");
            playerAnimator[indexOfWinner].SetTrigger("dodgeToSad");
        }
        else if (player2Choice == sweep)
        {
            // The blocking player has their star power lowered
            //damageDealt = baseStarPowerDecrease;
            //if (indexOfWinner == 1)
            //{
            //    // Placeholder value?
            //    damageDealt += 5;
            //}
            //playerStarPower[0] -= damageDealt;
            //starPowerBar[0].ScaleHealthBar((int)damageDealt, false);
            //Debug.Log($"damage dealt to player 1 star power: {-(int)damageDealt}");
            //playerAnimator[0].SetTrigger("fall");
            //playerAnimator[1].SetTrigger("sweep");

            damageDealt = baseStarPowerIncrease;
            if (indexOfWinner == 1)
            {
                damageDealt += bonus;
            }
            playerStarPower[1] += damageDealt;
            starPowerBar[1].ScaleHealthBar((int)damageDealt, true);
            playerAnimator[1].SetTrigger("sweep");
            playerAnimator[0].SetTrigger("fall");
        }

        ResetValues();
    }

    private void Player1Sweep()
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
            StartCoroutine(DelayAnimation(getHitAnimationDelay, 0, "getHit"));
            playerAnimator[1].SetTrigger("kick");
        }
        else if (player2Choice == dodge)
        {
            //damageDealt = baseStarPowerDecrease;
            //if (indexOfWinner == 0)
            //{
            //    damageDealt += bonus;
            //}
            //playerStarPower[1] -= damageDealt;
            //starPowerBar[1].ScaleHealthBar((int)damageDealt, false);
            //playerAnimator[0].SetTrigger("sweep");
            //playerAnimator[1].SetTrigger("fall");

            damageDealt = baseStarPowerIncrease;
            if (indexOfWinner == 0)
            {
                damageDealt += bonus;
            }
            playerStarPower[0] += damageDealt;
            starPowerBar[0].ScaleHealthBar((int)damageDealt, true);
            playerAnimator[0].SetTrigger("sweep");
            playerAnimator[1].SetTrigger("fall");
        }
        else if (player2Choice == sweep)
        {
            // player with higher singing score gets star power increased
            // note: chnaged to lowered

            //damageDealt = baseStarPowerIncrease + bonus;
            //playerStarPower[indexOfWinner] += damageDealt;
            //starPowerBar[indexOfWinner].ScaleHealthBar((int)damageDealt, true);
            //playerAnimator[indexOfWinner].SetTrigger("sweep");
            //playerAnimator[indexOfLoser].SetTrigger("fall");
            damageDealt = baseStarPowerDecrease + bonus;
            playerStarPower[indexOfLoser] -= damageDealt;
            starPowerBar[indexOfLoser].ScaleHealthBar((int)damageDealt, false);
            playerAnimator[indexOfWinner].SetTrigger("sweep");
            playerAnimator[indexOfLoser].SetTrigger("fall");
        }

        ResetValues();
    }

    private void Player1DidNotChoose()
    {
        Debug.Log("Player 1 did not make a choice.");
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
            //damageDealt = baseStarPowerDecrease;
            //if (indexOfWinner == 1)
            //{
            //    damageDealt += bonus;
            //}
            //playerStarPower[0] -= damageDealt;
            //starPowerBar[0].ScaleHealthBar((int)damageDealt, false);
            //playerAnimator[1].SetTrigger("sweep");
            //playerAnimator[0].SetTrigger("fall");

            damageDealt = baseStarPowerIncrease;
            if (indexOfWinner == 1)
            {
                damageDealt += bonus;
            }
            playerStarPower[1] += damageDealt;
            starPowerBar[1].ScaleHealthBar((int)damageDealt, true);
            playerAnimator[1].SetTrigger("sweep");
            playerAnimator[0].SetTrigger("fall");
        }

        ResetValues();
    }

    private void Player2DidNotChoose()
    {
        Debug.Log("Player 2 did not make a choice.");
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
            //damageDealt = baseStarPowerDecrease;
            //if (indexOfWinner == 0)
            //{
            //    damageDealt += bonus;
            //}
            //playerStarPower[1] -= damageDealt;
            //starPowerBar[1].ScaleHealthBar((int)damageDealt, false);
            //playerAnimator[0].SetTrigger("sweep");
            //playerAnimator[1].SetTrigger("fall");

            damageDealt = baseStarPowerIncrease;
            if (indexOfWinner == 0)
            {
                damageDealt += bonus;
            }
            playerStarPower[0] += damageDealt;
            starPowerBar[0].ScaleHealthBar((int)damageDealt, true);
            playerAnimator[0].SetTrigger("sweep");
            playerAnimator[1].SetTrigger("fall");
        }

        ResetValues();
    }

    private void ResetValues()
    {
        player1HasMadeChoice = false;
        player2HasMadeChoice = false;
        player1Choice = " ";
        player2Choice = " ";
        player1PortraitRenderer.sprite = player1RegularPortrait;
        player2PortraitRenderer.sprite = player2RegularPortrait;
        Debug.Log($"Player1 health: {playerHealth[0]}");
        Debug.Log($"Player2 health: {playerHealth[1]}");
        Debug.Log($"Player1 starpower: {playerStarPower[0]}");
        Debug.Log($"Player2 starpower: {playerStarPower[1]}");

        CheckForStarPower();
        CheckForDeath();
    }

    IEnumerator DecideComboAndDisplayImage(int indexOfWinner, int indexOfLoser)
    {
        if (indexOfWinner == indexOfLastRoundWinner)
        {
            timesWonInRowAfterFirst++;
            if (timesWonInRowAfterFirst == 1)
            {
                playerAnimator[indexOfWinner].SetBool("isOnFirstBonus", true);
                playerAnimator[indexOfWinner].SetBool("isOnSecondBonus", false);
            }
            else if (timesWonInRowAfterFirst >= 2)
            {
                playerAnimator[indexOfWinner].SetBool("isOnSecondBonus", true);
                playerAnimator[indexOfWinner].SetBool("isOnFirstBonus", false);
            }

            int starPower = (int)baseStarPowerComboIncrease + (timesWonInRowAfterFirst * 2);
            playerStarPower[indexOfWinner] += starPower;
            starPowerBar[indexOfWinner].ScaleHealthBar(starPower, true);
        }
        else
        {
            timesWonInRowAfterFirst = 0;
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

    private void CheckForStarPower()
    {
        if (!playerIsDead && !isPerformingStarPowerMove && !starPowerHasBeenPerformed)
        {
            if (playerStarPower[0] >= 100)
            {
                StartCoroutine(StarPowerMove(0, 1));
            }
            if (playerStarPower[1] >= 100)
            {
                StartCoroutine(StarPowerMove(1, 0));
            }
        }
    }

    private void CheckForDeath()
    {
        if (!playerIsDead && !isPerformingStarPowerMove)
        {
            if (playerHealth[0] <= 0)
            {
                StartCoroutine(KillPlayer(0, 1));
                Debug.Log($"Player 1 dead health: {playerHealth[0]}");
            }
            if (playerHealth[1] <= 0)
            {
                Debug.Log($"Player 2 dead health: {playerHealth[1]}");
                StartCoroutine(KillPlayer(1, 0));
            }
        }
    }

    IEnumerator StarPowerMove(int indexOfWinner, int indexOfLoser)
    {
        isPerformingStarPowerMove = true;
        playerAnimator[indexOfWinner].SetTrigger("starPower");
        yield return new WaitForSeconds(4.0f);
        starPowerSparks[indexOfWinner].Play();
        playerAnimator[indexOfLoser].SetTrigger("getBlasted");
        yield return new WaitForSeconds(starPowerMoveLengthInSeconds);
        playerAnimator[indexOfWinner].SetTrigger("starPowerEnd");
        yield return new WaitForSeconds(1);
        playerAnimator[indexOfLoser].SetTrigger("getBlastedEnd");
        playerStarPower[indexOfWinner] = 0;
        starPowerBar[indexOfWinner].gameObject.SetActive(false);
        starPowerBackgrounds[indexOfWinner].gameObject.SetActive(false);
        playerHealth[indexOfLoser] -= starPowerMoveDamage;
        healthBar[indexOfLoser].ScaleHealthBar(starPowerMoveDamage, false);
        isPerformingStarPowerMove = false;
        starPowerHasBeenPerformed = true;
        CheckForDeath();
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
}
