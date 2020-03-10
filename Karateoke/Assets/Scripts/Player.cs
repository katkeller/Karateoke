using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private HealthBar healthBar;

    [SerializeField]
    private HealthBar starPowerBar;

    [SerializeField]
    private string attackButtonString, dodgeButtonString, sweepButtonString;

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
                hasMadeChoiceThisPhrase = false;
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
                    Attack();
                    break;
                case MoveSet.Dodge:
                    Dodge();
                    break;
                case MoveSet.Sweep:
                    Sweep();
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

    public void ExecuteQueuedAnimations(bool overrideAnimation = false, string newAnimation = null)
    {
        //this is where we'll trigger animations from the combat manager script
    }

    private void Awake()
    {
        if(string.IsNullOrEmpty(attackButtonString) || string.IsNullOrEmpty(dodgeButtonString) ||
            string.IsNullOrEmpty(sweepButtonString))
        {
            Debug.LogError($"{this.name} does not have input strings assigned to it.");
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (CanMakeChoice && !hasMadeChoiceThisPhrase && !IsDead)
        {
            if (Input.GetButtonDown(attackButtonString))
            {
                MoveToExecute = MoveSet.Attack;
            }
            if (Input.GetButtonDown(dodgeButtonString))
            {
                MoveToExecute = MoveSet.Dodge;
            }
            if (Input.GetButtonDown(sweepButtonString))
            {
                MoveToExecute = MoveSet.Sweep;
            }
        }
    }

    private void Attack()
    {
        Debug.Log($"{this.name} is attacking.");
    }

    private void Dodge()
    {
        Debug.Log($"{this.name} is dodging.");
    }

    private void Sweep()
    {
        Debug.Log($"{this.name} is sweeping.");
    }

    private void ExecuteStarPowerMove()
    {
        //tell combat manager about this here
    }

    private void Die()
    {
        Debug.Log($"{this.name} is dead.");
        IsDead = true;
    }
}
