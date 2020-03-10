using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
    private string attackButtonString, dodgeButtonString, sweepButtonString;

    [SerializeField]
    private string attackAnimationTrigger, dodgeAnimationTrigger, sweepAnimationTrigger, sittingDuckAnimationTrigger;

    public enum MoveSet
    {
        Attack,
        Dodge,
        Sweep,
        Undecided
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
                ResetChoiceValues();
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
                ExecuteStarPowerMove();
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

            switch (value)
            {
                case MoveSet.Attack:
                    animationTrigger = attackAnimationTrigger;
                    break;
                case MoveSet.Dodge:
                    animationTrigger = dodgeAnimationTrigger;
                    break;
                case MoveSet.Sweep:
                    animationTrigger = sweepAnimationTrigger;
                    break;
                case MoveSet.Undecided:
                    break;
                default:
                    Debug.Log($"{value} unsupported.");
                    break;
            }

            hasMadeChoiceThisPhrase = true;
        }
    }

    private string animationTrigger;

    private AudioSource audioSource;
    private Animator animator;

    #endregion


    /// <summary>
    /// Plays the player's queued animation immediately. This should only be called at the end of a phrase.
    /// </summary>
    /// <param name="overrideAnimation"></param>
    /// <param name="newAnimationTrigger"></param>
    public void ExecuteQueuedCombatMove(int turnDamage, int turnBonus, bool overrideAnimation = false, string newAnimationTrigger = null)
    {
        if (overrideAnimation)
        {
            //do we ever need to override animations?
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
    }

    private void Awake()
    {
        if(string.IsNullOrEmpty(attackButtonString) || string.IsNullOrEmpty(dodgeButtonString) ||
            string.IsNullOrEmpty(sweepButtonString))
        {
            Debug.LogError($"{this.name} does not have input strings assigned to it.");
        }

        animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetButtonDown(attackButtonString))
        {
            SetChoice(MoveSet.Attack);
        }
        if (Input.GetButtonDown(dodgeButtonString))
        {
            SetChoice(MoveSet.Dodge);
        }
        if (Input.GetButtonDown(sweepButtonString))
        {
            SetChoice(MoveSet.Sweep);
        }
    }

    private void SetChoice(MoveSet move)
    {
        if (CanMakeChoice && !hasMadeChoiceThisPhrase && !IsDead)
        {
            MoveToExecute = move;
        }
    }

    private void ResetChoiceValues()
    {
        hasMadeChoiceThisPhrase = false;
        animationTrigger = null;
        //reset portrait and action text
    }

    private void Attack()
    {

    }

    private void Dodge()
    {

    }

    private void Sweep()
    {

    }

    private void ExecuteStarPowerMove()
    {
        Debug.Log($"{this.name} is executing a star power move.");
        //tell combat manager about this here
    }

    private void Die()
    {
        Debug.Log($"{this.name} is dead.");
        IsDead = true;
    }
}
