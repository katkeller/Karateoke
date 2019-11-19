using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI player1ActionText, player2ActionText, countdownText;

    [SerializeField]
    private float secondsForChoice = 3.0f;

    [SerializeField]
    private Color32 lastNumberColor;

    [SerializeField]
    private AudioClip[] countdownClip = new AudioClip[4];

    [SerializeField]
    private Image player1ActionImage, player2ActionImage;

    [SerializeField]
    private Sprite attackIcon, blockIcon, grappleIcon;

    [SerializeField]
    private GameObject[] playerHealthBarGameObject = new GameObject[2];

    [SerializeField]
    private int scalingFramesLeft = 10;

    [SerializeField]
    private float healthBarScaleRate = 2.0f, healthBarScaleSpeed = 2.0f;

    [SerializeField]
    private int baseAttackDamage = 5, baseParryDamage = 1, baseStarPowerIncrease = 1;

    [SerializeField]
    private float baseStarPowerDecrease = 0.5f;

    private int[] playerHealth = new int[2];
    private float[] playerStarPower = new float[2];
    private int indexOfWinner;
    private int indexOfLoser;
    private float winningDisparity;
    private float damageDealt;

    //use these to create star power bonuses
    private int winsInARowCount;
    private int indexOfLastRoundWinner;

    private Color32 countdownTextColor;
    private Sprite player1NextImage, player2NextImage;
    private AudioSource audioSource;
    private HealthBar[] healthBar = new HealthBar[2];
    private AudioComparisonManager audioComparisonSript;

    private bool canMakeChoice;
    private bool player1HasMadeChoice, player2HasMadeChoice;
    private string player1Choice, player2Choice;
    private string attack = "attack", block = "block", grapple = "grapple";

    public static event Action DecideWinner;

    void Start()
    {
        playerHealth[0] = 100;
        playerHealth[1] = 100;
        playerStarPower[0] = 0;
        playerStarPower[1] = 0;
        countdownText.text = "";
        countdownTextColor = countdownText.color;
        player1ActionImage.enabled = false;
        player2ActionImage.enabled = false;

        healthBar[0] = playerHealthBarGameObject[0].GetComponent<HealthBar>();
        healthBar[1] = playerHealthBarGameObject[1].GetComponent<HealthBar>();

        audioSource = GetComponent<AudioSource>();
        audioComparisonSript = GetComponent<AudioComparisonManager>();
    }

    void Update()
    {
        //if (Input.GetButtonDown("Player1Attack"))
        //{
        //    Debug.Log("Player 1 Attacks!");
        //}
        //if (Input.GetButtonDown("Player1Block"))
        //{
        //    Debug.Log("Player 1 Blocks!");
        //}
        //if (Input.GetButtonDown("Player1Grapple"))
        //{
        //    Debug.Log("Player 1 Grapples!");
        //}
        //if (Input.GetButtonDown("Player2Attack"))
        //{
        //    Debug.Log("Player 2 Attacks!");
        //}
        //if (Input.GetButtonDown("Player2Block"))
        //{
        //    Debug.Log("Player 2 Blocks!");
        //}
        //if (Input.GetButtonDown("Player2Grapple"))
        //{
        //    Debug.Log("Player 2 Grapples!");
        //}

        if (canMakeChoice)
        {
            //we should add a simple (but unique to each player) soundeffect for when they choose

            if (Input.GetButtonDown("Player1Attack") && !player1HasMadeChoice)
            {
                player1ActionText.text = "Player 1 attacks!";
                player1Choice = attack;
                player1HasMadeChoice = true;
                player1NextImage = attackIcon;

                //for testing purposes:
                //healthBar[1].ScaleHealthBar(25);
            }
            if (Input.GetButtonDown("Player1Block") && !player1HasMadeChoice)
            {
                player1ActionText.text = "Player 1 blocks!";
                player1Choice = block;
                player1HasMadeChoice = true;
                player1NextImage = blockIcon;
            }
            if (Input.GetButtonDown("Player1Grapple") && !player1HasMadeChoice)
            {
                player1ActionText.text = "Player 1 grapples!";
                player1Choice = grapple;
                player1HasMadeChoice = true;
                player1NextImage = grappleIcon;
            }
            if (Input.GetButtonDown("Player2Attack") && !player2HasMadeChoice)
            {
                player2ActionText.text = "Player 2 attacks!";
                player2Choice = attack;
                player2HasMadeChoice = true;
                player2NextImage = attackIcon;
            }
            if (Input.GetButtonDown("Player2Block") && !player2HasMadeChoice)
            {
                player2ActionText.text = "Player 2 blocks!";
                player2Choice = block;
                player2HasMadeChoice = true;
                player2NextImage = blockIcon;
            }
            if (Input.GetButtonDown("Player2Grapple") && !player2HasMadeChoice)
            {
                player2ActionText.text = "Player 2 grapples!";
                player2Choice = grapple;
                player2HasMadeChoice = true;
                player2NextImage = grappleIcon;
            }
        }
    }

    private void OnEndOfPhrase()
    {
        StartCoroutine(AllowPlayersToMakeChoice());
        Debug.Log("Phrase end has been triggered!");
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
        yield return new WaitForSeconds(1);
        countdownText.color = lastNumberColor;
        canMakeChoice = true;
        countdownText.text = "CHOOSE!";
        audioSource.PlayOneShot(countdownClip[3]);
        //Add choosing countdown graphic, maybe a bar or a round pie chart type thing?
        yield return new WaitForSeconds(1);
        DecideWinner?.Invoke();
        countdownText.text = "";
        countdownText.color = countdownTextColor;
        canMakeChoice = false;

        UpdateActionImages();
        DecideOnDamage();
    }

    private void UpdateActionImages()
    {
        // We should add a small animation to these as a stretch goal

        if (player1NextImage != null && player1HasMadeChoice)
        {
            player1ActionImage.sprite = player1NextImage;
            player1ActionImage.enabled = true;
        }
        if (player2NextImage != null && player2HasMadeChoice)
        {
            player2ActionImage.sprite = player2NextImage;
            player2ActionImage.enabled = true;
        }
    }

    private void DecideOnDamage()
    {
        damageDealt = 0;
        indexOfWinner = audioComparisonSript.IndexOfWinner;
        indexOfLoser = audioComparisonSript.IndexOfLoser;
        winningDisparity = audioComparisonSript.WinningDisparity;

        if (player1Choice == attack && player2Choice == attack)
        {
            // Player with higher score wins, deals base damage.
            damageDealt = baseAttackDamage;
            playerHealth[indexOfLoser] -= (int)damageDealt;

            healthBar[indexOfLoser].ScaleHealthBar((int)damageDealt);
        }
        else if (player1Choice == attack && player2Choice == block)
        {
            // Blocking player negates attacking player's hit.
            // They deal a small amount of damage if they win singing-wise.
            if (indexOfWinner == 1)
            {
                damageDealt = baseParryDamage;
                playerHealth[0] -= (int)damageDealt;
                healthBar[0].ScaleHealthBar((int)damageDealt);
            }
        }
        else if (player1Choice == attack && player2Choice == grapple)
        {
            // Attacking player deals damger to grappling player,
            // with additional damage dealt if the attacking player wins singing-wise.
            damageDealt = baseAttackDamage;

            if (indexOfWinner == 0)
            {
                //placeholder value
                damageDealt += 1;
            }
            playerHealth[1] -= (int)damageDealt;
            healthBar[1].ScaleHealthBar((int)damageDealt);
        }
        else if (player1Choice == block && player2Choice == attack)
        {
            if (indexOfWinner == 0)
            {
                damageDealt = baseParryDamage;
                playerHealth[1] -= (int)damageDealt;
                healthBar[1].ScaleHealthBar((int)damageDealt);
            }
        }
        else if (player1Choice == block && player2Choice == block)
        {
            //Nothing, idiots!
        }
        else if (player1Choice == block && player2Choice == grapple)
        {
            // The blocking player has their star power lowered
            damageDealt = baseStarPowerDecrease;
            if (indexOfWinner == 1)
            {
                // Placeholder value
                damageDealt += 0.25f;
            }
            playerStarPower[0] -= damageDealt;
        }
        else if (player1Choice == grapple && player2Choice == attack)
        {
            damageDealt = baseAttackDamage;
            if (indexOfWinner == 1)
            {
                damageDealt += 1.0f;
            }
            playerHealth[0] -= (int)damageDealt;
            healthBar[0].ScaleHealthBar((int)damageDealt);
        }
        else if (player1Choice == grapple && player2Choice == block)
        {
            damageDealt = baseStarPowerDecrease;
            if (indexOfWinner == 0)
            {
                damageDealt += 0.25f;
            }
            playerStarPower[1] -= damageDealt;
        }
        else if (player1Choice == grapple && player2Choice == grapple)
        {
            // player with higher singing score gets star power increased
            damageDealt = baseStarPowerIncrease;
            playerStarPower[indexOfWinner] += damageDealt;
        }
        else
        {
            //This means someone didn't choose, so we have to figure out how to deal with what the other person chose,
            //or what to do if they also chose nothing

            if (player1HasMadeChoice == false && player2HasMadeChoice == true)
            {
                //player1NextImage = null;
            }
            else if (player1HasMadeChoice == true && player2HasMadeChoice == false)
            {
                //player2NextImage = null;
            }
            else
            {
                //this means NO ONE chose anything. Boooooo!
            }
        }
    }
}
