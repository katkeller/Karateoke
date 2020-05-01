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
    private GameObject starPowerRingGraphicObject;

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
    private string gettingReadyAnimationTrigger, fallAnimationTrigger, getHitAnimationTrigger, getHitFromBlockAnimationTrigger;

    [SerializeField]
    private string getBlockedAnimationTrigger;

    [SerializeField]
    private ParticleSystem getHitParticleSystem, blockParticleSystem;

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
            //healthBar.ScaleHealthBar(health, false);

            if (health <= 0)
            {
                Die();
            }
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
            starPower += value;

            if (starPower < 0)
            {
                starPower = 0;
            }

            if (starPower >= 100)
            {
                SetUpStarPowerMove();
                //remember to set star power back to a 0
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

    private bool gameHasStarted;

    private AudioSource audioSource;
    private Animator animator;
    private SpriteRenderer portraitRenderer;
    private StarPowerBar starPowerRingGraphic;

    #endregion

    #region Events

    public static event Action<int> PlayerDies;
    public static event Action<int> PlayerHasFullStarPower;
    public static event Action<int> AttemptAttack;
    public static event Action<int> AttemptBlock;
    public static event Action<int> AttemptSweep;
    public static event Action<int> DealStarPowerDamage;

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
        //StartCoroutine(ShowActionText());
        
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

    private void Awake()
    {
        if(string.IsNullOrEmpty(attackButtonString) || string.IsNullOrEmpty(dodgeButtonString) ||
            string.IsNullOrEmpty(sweepButtonString))
        {
            Debug.LogError($"{this.name} does not have input strings assigned to it.");
        }

        //actionText.text = " ";

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        //portraitRenderer = portraitObject.GetComponent<SpriteRenderer>();
        starPowerRingGraphic = starPowerRingGraphicObject.GetComponent<StarPowerBar>();
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
            //portraitRenderer.sprite = activatedPortrait;
            //audioSource.PlayOneShot(choiceMadeClip);
            Debug.Log($"{this.name} has chosen {move}");
        }
    }

    private void ResetChoiceValuesAndStartChoiceAnimation()
    {
        hasMadeChoiceThisPhrase = false;
        animationTrigger = null;
        //portraitRenderer.sprite = unactivatedPortrait;
        queuedActionText = " ";

        //animator.SetTrigger(gettingReadyAnimationTrigger);
    }

    private void SetUpStarPowerMove()
    {
        Debug.Log($"{this.name} is executing a star power move.");
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
