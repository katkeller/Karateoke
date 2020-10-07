﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    /// <summary>
    /// This class holds each individual player's health and star power values, and manages their animations and input.
    /// This is not where damage is dealt/determined, nor is it where animation interruptions are determined. This script acts
    /// as if it has no knowledge of what the other player's choice may be, i.e. it will play the "dodge" animation even if
    /// it is going to be interrupted by a sweep from the other player. Reaction animations are mostly decided elsewhere.
    /// </summary>

    #region Fields/Properties

    [SerializeField]
    private GameObject healthBarObject;

    [SerializeField]
    private GameObject starPowerRingGraphicObject;

    [SerializeField]
    private GameObject portraitObject;

    [SerializeField]
    private Sprite activatedPortrait, unactivatedPortrait;

    [SerializeField]
    private GameObject attackTextObject, blockTextObject, sweepTextObject;

    [SerializeField]
    private Camera guiCamera;

    [SerializeField]
    private GameObject starPowerUi;

    [SerializeField]
    private PlayableDirector victoryCutSceneObject;

    [SerializeField]
    private string attackButtonString, dodgeButtonString, sweepButtonString;

    [SerializeField]
    private string attackAnimationTrigger, dodgeAnimationTrigger, sweepAnimationTrigger, sittingDuckAnimationTrigger;

    [SerializeField]
    private string gettingReadyAnimationTrigger, fallAnimationTrigger, getHitAnimationTrigger, getHitFromBlockAnimationTrigger;

    [SerializeField]
    private string getBlockedAnimationTrigger, deathAnimationTrigger, winAnimationTrigger;

    [SerializeField]
    private ParticleSystem getHitParticleSystem;

    [SerializeField]
    private AudioClip choiceMadeClip;

    public enum MoveSet
    {
        Attack,
        Dodge,
        Sweep,
        Undecided
    }

    public int IndexAccordingToCombatManager
    {
        get;
        set;
    }

    private bool canMakeChoice;
    public bool CanMakeChoice
    {
        get => canMakeChoice;
        set
        {
            canMakeChoice = value;

            if (canMakeChoice == true)
            {
                // This makes it so that every time the combat manager asserts that a phrase is about to end,
                // the player knows that it's time to make a choice again.
                ResetChoiceValuesAndStartChoiceAnimation();
            }
        }
    }

    public bool IsDead
    {
        get;
        set;
    }

    public bool StarPowerMoveIsHappening
    {
        get;
        set;
    }

    private bool hasMadeChoiceThisPhrase;

    private int health;
    private int starPower;

    /// <summary>
    /// This value starts at 100, and if it drops below 0, the player dies and the other player wins.
    /// </summary>
    public int Health
    {
        get
        {
            return health;
        }
        set
        {
            health = value;

            if (health <= 0)
            {
                health = 0;
                if (!StarPowerMoveIsHappening)
                {
                    //We check for the star power move so that the player dying doesn't interrupt the cut scene.
                    StartCoroutine(WaitForCombatThenDie());
                }
            }

            healthBar.SetHealthBarToValue(health);
        }
    }

    /// <summary>
    /// This is the value that control the player's "special move". If this value gets above 100,
    /// their move triggers. Pass in a negative value to decrease the player's star power.
    /// </summary>
    public int StarPower
    {
        get
        {
            return starPower;
        }
        set
        {
            starPower = value;

            if (starPower < 0)
            {
                starPower = 0;
            }

            if (starPower >= 100)
            {
                StartCoroutine(SetUpStarPowerMove());
            }

            starPowerRingGraphic.ScaleFill(starPower);
        }
    }

    private MoveSet moveToExecute;
    public MoveSet MoveToExecute
    {
        get
        {
            if (!hasMadeChoiceThisPhrase)
            {
                // If the player has not made a choice by the time this value is being accessed by the combat manager,
                // then that means they've run out of time and their Move is set to "Undecided".
                moveToExecute = MoveSet.Undecided;
                queuedActionTextObject = null;
            }

            return moveToExecute;
        }
        set
        {
            moveToExecute = value;

            switch (moveToExecute)
            {
                case MoveSet.Attack:
                    animationTrigger = attackAnimationTrigger;
                    queuedActionTextObject = attackTextObject;
                    break;
                case MoveSet.Dodge:
                    animationTrigger = dodgeAnimationTrigger;
                    queuedActionTextObject = blockTextObject;
                    break;
                case MoveSet.Sweep:
                    animationTrigger = sweepAnimationTrigger;
                    queuedActionTextObject = sweepTextObject;
                    break;
                case MoveSet.Undecided:
                    break;
                default:
                    Debug.Log($"MoveToExecute for {this.name}: {value} unsupported.");
                    break;
            }

            hasMadeChoiceThisPhrase = true;
        }
    }

    private string animationTrigger;
    private GameObject queuedActionTextObject;

    private bool gameHasStarted;

    private AudioSource audioSource;
    private Animator animator;
    private Image portraitImage;
    private StarPowerBar starPowerRingGraphic;
    private HealthBar healthBar;

    #endregion

    #region Events

    public static event Action<int> PlayerDies;
    public static event Action<int> PlayerHasFullStarPower;
    public static event Action<int> AttemptAttack;
    public static event Action<int> AttemptBlock;
    public static event Action<int> AttemptSweep;
    public static event Action<int> DealStarPowerDamage;
    public static event Action<int, int> StarPowerMoveEnds;

    #endregion

    public void ExecuteMakingChoiceAnimations()
    {
        animator.SetTrigger(gettingReadyAnimationTrigger);
    }

    /// <summary>
    /// Plays the player's queued animation immediately. This should only be called at the end of a phrase.
    /// </summary>
    /// <param name="overrideAnimation"></param>
    /// <param name="newAnimationTrigger"></param>
    public void ExecuteQueuedCombatMove(bool overrideAnimation = false, string newAnimationTrigger = null)
    {
        if (overrideAnimation && !string.IsNullOrEmpty(newAnimationTrigger))
        {
            // will we ever need to override animations?
            animationTrigger = newAnimationTrigger;
        }

        if (string.IsNullOrEmpty(animationTrigger))
        {
            // If we get here we can assume the player didn't make a choice,
            // so we assign the "sitting duck" animation trigger
            animationTrigger = sittingDuckAnimationTrigger;
        }

        animator.SetTrigger(animationTrigger);
        //also do action text
        StartCoroutine(ShowActionText());
        
    }

    public void PlayerAttacksEvent(int indexOfOtherPlayer)
    {
        AttemptAttack?.Invoke(indexOfOtherPlayer);
    }

    public void PlayerBlocksEvent(int indexOfOtherPlayer)
    {
        Debug.Log("Block animation event triggered.");
        AttemptBlock?.Invoke(indexOfOtherPlayer);
    }

    public void PlayerSweepsEvent(int indexOfOtherPlayer)
    {
        AttemptSweep?.Invoke(indexOfOtherPlayer);
    }

    public void PlayerDealsStarPowerDamageEvent(int indexOfOtherPlayer)
    {
        DealStarPowerDamage?.Invoke(indexOfOtherPlayer);
    }

    public void StarPowerMoveEndsEvent(int indexOfOtherPlayer)
    {
        // This event should only be triggered from the losing player's animation: either the kip up or the relief.
        guiCamera.enabled = true;
        starPowerUi.SetActive(true);
        StarPowerMoveEnds?.Invoke(IndexAccordingToCombatManager, indexOfOtherPlayer);
    }

    #region Damage & Animation Interruptions (Triggered From CombatManager)

    public void GetAttcked(float possibleDamage, int bonus, int indexOfWinner)
    {
        //this only gets called once choices have been locked in (after phrase end)

        int damage = 0;

        switch (MoveToExecute)
        {
            // do we interrupt animations in here?
            case MoveSet.Attack:
                if (indexOfWinner != IndexAccordingToCombatManager)
                {
                    animator.SetTrigger(getHitAnimationTrigger);
                    //we can move this to SFX VFX script right?
                    StartCoroutine(PlayVFX(getHitParticleSystem));
                    // Apply damage to this player
                    damage = (int)(possibleDamage + bonus);
                    Health -= damage;
                }
                break;
            case MoveSet.Dodge:
                animator.SetTrigger(getHitFromBlockAnimationTrigger);
                Debug.Log($"{this.name} dodged successfully.");
                break;
            case MoveSet.Sweep:
                animator.SetTrigger(getHitAnimationTrigger);
                StartCoroutine(PlayVFX(getHitParticleSystem));
                damage = (int)possibleDamage;
                if (indexOfWinner != IndexAccordingToCombatManager)
                {
                    // Apply bonus to damage value
                    damage += bonus;
                }
                Health -= damage;
                break;
            case MoveSet.Undecided:
                animator.SetTrigger(getHitAnimationTrigger);
                StartCoroutine(PlayVFX(getHitParticleSystem));
                damage = (int)possibleDamage;
                if (indexOfWinner != IndexAccordingToCombatManager)
                {
                    // Apply bonus to damage value
                    damage += bonus;
                }
                Health -= damage;
                break;
            default:
                Debug.Log($"Combat error: getting hit, {this.name}");
                break;
        }

    }

    private IEnumerator PlayVFX(ParticleSystem systemToPlay)
    {
        systemToPlay.Play();
        yield return new WaitForSeconds(0.5f);
        systemToPlay.Stop();
    }

    public void GetBlocked()
    {
        if(MoveToExecute == MoveSet.Attack)
        {
            animator.SetTrigger(getBlockedAnimationTrigger);
        }
    }

    public void GetSwept()
    {
        animator.SetTrigger(fallAnimationTrigger);
    }

    public void SuccessfullySweep(int starPowerIncrease, int bonus, int indexOfWinner)
    {
        if (indexOfWinner == IndexAccordingToCombatManager)
        {
            starPowerIncrease += (bonus / 4);
            Debug.Log($"Star power increase: {starPowerIncrease}");
        }

        StarPower += starPowerIncrease;
    }

    #endregion

    public void StartGame()
    {
        if (!gameHasStarted)
        {
            gameHasStarted = true;
            animator.SetTrigger("Start");
        }
    }

    private IEnumerator WaitForCombatThenDie()
    {
        // When the player dies from a regular combat move, we want to give that move a chance to finish before
        // we start the death cutscene. When death comes from an SP move, there's no need since the death won't
        // be activated by the combat manager until after the SP cutscene is finished.
        yield return new WaitForSeconds(1.5f);
        Die();
    }

    public void Die()
    {
        Debug.Log($"{this.name} is dead.");
        IsDead = true;
        animator.SetTrigger(deathAnimationTrigger);
        PlayerDies?.Invoke(IndexAccordingToCombatManager);
    }

    public void Win()
    {
        victoryCutSceneObject.Play();
        animator.SetTrigger(winAnimationTrigger);
    }

    private void Awake()
    {
        if(string.IsNullOrEmpty(attackButtonString) || string.IsNullOrEmpty(dodgeButtonString) ||
            string.IsNullOrEmpty(sweepButtonString))
        {
            Debug.LogError($"{this.name} does not have input strings assigned to it.");
        }

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        portraitImage = portraitObject.GetComponent<Image>();
        starPowerRingGraphic = starPowerRingGraphicObject.GetComponent<StarPowerBar>();
        healthBar = healthBarObject.GetComponent<HealthBar>();
    }

    void Update()
    {
        if (Input.GetButtonDown(attackButtonString))
        {
            CheckAndSetChoice(MoveSet.Attack);
        }
        if (Input.GetButtonDown(dodgeButtonString))
        {
            CheckAndSetChoice(MoveSet.Dodge);
        }
        if (Input.GetButtonDown(sweepButtonString))
        {
            CheckAndSetChoice(MoveSet.Sweep);
        }
    }

    private void CheckAndSetChoice(MoveSet move)
    {
        if (CanMakeChoice && !hasMadeChoiceThisPhrase && !IsDead)
        {
            MoveToExecute = move;
            portraitImage.sprite = activatedPortrait;
            audioSource.PlayOneShot(choiceMadeClip);
            Debug.Log($"{this.name} has chosen {move}");
        }
    }

    private void ResetChoiceValuesAndStartChoiceAnimation()
    {
        hasMadeChoiceThisPhrase = false;
        animationTrigger = null;
        portraitImage.sprite = unactivatedPortrait;
        queuedActionTextObject = null;
    }

    private IEnumerator SetUpStarPowerMove()
    {
        Debug.Log($"{this.name} is executing a star power move.");
        // Need to wait before invoking so that the combat animations have time to finish.
        yield return new WaitForSeconds(1.25f);
        PlayerHasFullStarPower?.Invoke(IndexAccordingToCombatManager);
    }

    IEnumerator ShowActionText()
    {
        if (queuedActionTextObject != null)
        {
            queuedActionTextObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            queuedActionTextObject.SetActive(false);
        }
    }
}
