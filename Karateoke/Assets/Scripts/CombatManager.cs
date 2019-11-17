using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

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
    private GameObject[] playerHealthBarGameObject = new GameObject[2];

    [SerializeField]
    private int scalingFramesLeft = 10;

    [SerializeField]
    private float healthBarScaleRate = 2.0f, healthBarScaleSpeed = 2.0f;

    [SerializeField]
    private int baseAttackDamage = 5, baseParryDamage = 1;

    public int indexOfWinner { get; set; }
    public int indexOfLoser { get; set; }

    private int[] playerHealth = new int[2];

    private Color32 countdownTextColor;
    private AudioSource audioSource;
    private HealthBar[] healthBar = new HealthBar[2];

    private bool canMakeChoice;
    private bool player1HasMadeChoice, player2HasMadeChoice;
    private string player1Choice, player2Choice;
    private string attack = "attack", block = "block", grapple = "grapple";

    void Start()
    {
        playerHealth[0] = 100;
        playerHealth[1] = 100;
        countdownText.text = "";
        countdownTextColor = countdownText.color;

        healthBar[0] = playerHealthBarGameObject[0].GetComponent<HealthBar>();
        healthBar[1] = playerHealthBarGameObject[1].GetComponent<HealthBar>();

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (canMakeChoice)
        {
            //we should add a simple (but unique to each player) soundeffect for when they choose

            if (Input.GetButtonDown("Player1Attack") && !player1HasMadeChoice)
            {
                player1ActionText.text = "Player 1 attacks!";
                player1Choice = attack;
                player1HasMadeChoice = true;

                //for testing purposes:
                healthBar[1].ScaleHealthBar(25);
            }
            if (Input.GetButtonDown("Player1Block") && !player1HasMadeChoice)
            {
                player1Choice = block;
                player1HasMadeChoice = true;
            }
            if (Input.GetButtonDown("Player1Grapple") && !player1HasMadeChoice)
            {
                player1Choice = grapple;
                player1HasMadeChoice = true;
            }
            if (Input.GetButtonDown("Player2Attack") && !player2HasMadeChoice)
            {
                player2Choice = attack;
                player2ActionText.text = "Player 2 attacks!";
                player2HasMadeChoice = true;
            }
            if (Input.GetButtonDown("Player2Block") && !player2HasMadeChoice)
            {
                player2Choice = block;
                player2HasMadeChoice = true;
            }
            if (Input.GetButtonDown("Player2Grapple") && !player2HasMadeChoice)
            {
                player2Choice = grapple;
                player2HasMadeChoice = true;
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
        canMakeChoice = true;

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
        countdownText.text = "0";
        audioSource.PlayOneShot(countdownClip[3]);
        yield return new WaitForSeconds(1);
        countdownText.text = "";
        countdownText.color = countdownTextColor;
        //yield return new WaitForSeconds(secondsForChoice);
        canMakeChoice = false;

        DecideOnDamage();
    }

    private void DecideOnDamage()
    {
        if (player1Choice == attack && player2Choice == attack)
        {
            playerHealth[indexOfLoser] -= baseAttackDamage;
            //Come back to all this to add actual values
        }
        else if (player1Choice == attack && player2Choice == block)
        {
            //add special stuff

        }
        else if (player1Choice == attack && player2Choice == grapple)
        {
            //
        }
        else if (player1Choice == block && player2Choice == attack)
        {
            playerHealth[indexOfLoser] -= baseAttackDamage;
        }
        else if (player1Choice == block && player2Choice == block)
        {
            //nothin?
        }
        else if (player1Choice == block && player2Choice == grapple)
        {
            //
        }
        else if (player1Choice == grapple && player2Choice == attack)
        {

        }
        else if (player1Choice == grapple && player2Choice == block)
        {

        }
        else if (player1Choice == grapple && player2Choice == grapple)
        {
            //star power!
        }
        else
        {
            //This means someone didn't choose, so we have to figure out how to deal with what the other person chose,
            //or what to do if they also chose nothing
        }
    }
}
