using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
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
    private HealthBar healthBar;

    [SerializeField]
    private HealthBar starPowerBar;

    [SerializeField]
    private Text actionText;

    [SerializeField]
    private GameObject portraitObject;

    [SerializeField]
    private Sprite activatedPortrait, unactivatedPortrait;

    [SerializeField]
    private string attackButtonString, dodgeButtonString, sweepButtonString;

    [SerializeField]
    private string attackAnimationTrigger, dodgeAnimationTrigger, sweepAnimationTrigger, sittingDuckAnimationTrigger;

    [SerializeField]
    private string gettingReadyAnimationTrigger, fallAnimationTrigger, getHitAnimationTrigger, getHitFromSweepAnimationTrigger;

    [SerializeField]
    private AudioClip choiceMadeClip, getHitClip, fallClip;

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
            health += value;
            healthBar.ScaleHealthBar(health, false);

            if (health <= 0)
            {
                Die();
            }
        }
    }

    /// <summary>
    /// This is the value that control the player's "special move". If this value gets above 10,
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
            starPower += value;
            if (value < 0)
            {
                starPowerBar.ScaleHealthBar(starPower, false);
            }
            else
            {
                starPowerBar.ScaleHealthBar(starPower, true);
            }

            if (starPower < 0)
            {
                starPower = 0;
            }

            if (starPower >= 10)
            {
                SetUpStarPowerMove();
                //remember to set star power back to a 0
            }
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
                    queuedActionText = "ATTACK";
                    break;
                case MoveSet.Dodge:
                    animationTrigger = dodgeAnimationTrigger;
                    queuedActionText = "DODGE";
                    break;
                case MoveSet.Sweep:
                    animationTrigger = sweepAnimationTrigger;
                    queuedActionText = "SWEEP";
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
    private string queuedActionText;

    private AudioSource audioSource;
    private Animator animator;
    private SpriteRenderer portraitRenderer;

    #endregion

    #region Events

    public static event Action<int> PlayerDies;
    public static event Action<int> PlayerHasFullStarPower;

    #endregion


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
                    // Apply damage to this player
                    damage = (int)(possibleDamage + bonus);
                    Health -= damage;
                }
                break;
            case MoveSet.Dodge:
                // Nothing for now?
                Debug.Log($"{this.name} dodged successfully.");

                break;
            case MoveSet.Sweep:
                animator.SetTrigger(getHitFromSweepAnimationTrigger);
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

    public void GetSwept(int indexOfWinner)
    {
        switch (MoveToExecute)
        {
            case MoveSet.Attack:
                // Nothing?
                break;
            case MoveSet.Dodge:
                animator.SetTrigger(fallAnimationTrigger);
                // do we want to reduce star power here maybe?
                break;
            case MoveSet.Sweep:
                if (indexOfWinner != IndexAccordingToCombatManager)
                {
                    animator.SetTrigger(fallAnimationTrigger);
                }
                break;
            case MoveSet.Undecided:
                animator.SetTrigger(fallAnimationTrigger);
                break;
            default:
                Debug.Log($"Combat error: getting swept, {this.name}");
                break;
        }
    }

    #endregion

    private void Awake()
    {
        if(string.IsNullOrEmpty(attackButtonString) || string.IsNullOrEmpty(dodgeButtonString) ||
            string.IsNullOrEmpty(sweepButtonString))
        {
            Debug.LogError($"{this.name} does not have input strings assigned to it.");
        }

        actionText.text = " ";

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        portraitRenderer = portraitObject.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        
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
            portraitRenderer.sprite = activatedPortrait;
            audioSource.PlayOneShot(choiceMadeClip);
        }
    }

    private void ResetChoiceValuesAndStartChoiceAnimation()
    {
        hasMadeChoiceThisPhrase = false;
        animationTrigger = null;
        //reset portrait
        portraitRenderer.sprite = unactivatedPortrait;
        queuedActionText = " ";

        animator.SetTrigger(gettingReadyAnimationTrigger);
    }

    private void SetUpStarPowerMove()
    {
        Debug.Log($"{this.name} is executing a star power move.");
        //tell combat manager about this here
        PlayerHasFullStarPower?.Invoke(IndexAccordingToCombatManager);
    }

    private void Die()
    {
        Debug.Log($"{this.name} is dead.");
        IsDead = true;
        PlayerDies?.Invoke(IndexAccordingToCombatManager);
    }

    IEnumerator ShowActionText()
    {
        actionText.text = queuedActionText;
        yield return new WaitForSeconds(3);
        actionText.text = " ";
    }
}
