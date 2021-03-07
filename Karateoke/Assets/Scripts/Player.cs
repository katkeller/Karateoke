using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static CombatState;
using static IPlayerCombatState;
/// <summary>
/// This class holds each individual player's health and star power values, while also managing their animations and input.
/// It communicates with the combat manager in order to determine if the chosen combat move will be successful or not, as well
/// as how much damage should be inflicted.
/// </summary>
public class Player : MonoBehaviour
{
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
    private string gettingReadyAnimationTrigger, deathAnimationTrigger, winAnimationTrigger;

    [SerializeField]
    private ParticleSystem getHitParticleSystem;

    [SerializeField]
    private AudioClip choiceMadeClip;

    [SerializeField]
    private string attackButton, blockButton, sweepButton;

    public IPlayerCombatState CurrentCombatState { get; set; }
    public AttackState AttackState { get; set; }
    public BlockState BlockState { get; set; }
    public SweepState SweepState { get; set; }
    public UndecidedState UndecidedState { get; set; }

    private CombatMove moveToExecute;
    public CombatMove MoveToExecute
    {
        get => moveToExecute;
        set
        {
            moveToExecute = value;
        }
    }

    public GameObject AttackTextObject => attackTextObject;
    public GameObject BlockTextObject => blockTextObject;
    public GameObject SweepTextObject => sweepTextObject;
    public Animator PlayerAnimator => animator;
    public ParticleSystem GetHitParticleSystem => getHitParticleSystem;
    public int IndexAccordingToCombatManager { get; set; }
    public bool IsDead { get; set; }
    public bool StarPowerMoveIsHappening { get; set; }

    private bool hasMadeChoiceThisPhrase;
    private bool canMakeChoice;
    public bool CanMakeChoice
    {
        get => canMakeChoice;
        set
        {
            canMakeChoice = value;

            if (canMakeChoice)
            {
                // This makes it so that every time the combat manager asserts that a phrase is about to end,
                // the player knows that it's time to make a choice again.
                ResetChoiceValuesAndStartChoiceAnimation();
            }
        }
    }

    private int health;
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

    private int starPower;
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
    public static event Action<int> Fall;
    public static event Action<int> DealStarPowerDamage;
    public static event Action<int, int> StarPowerMoveEnds;

    #endregion

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        portraitImage = portraitObject.GetComponent<Image>();
        starPowerRingGraphic = starPowerRingGraphicObject.GetComponent<StarPowerBar>();
        healthBar = healthBarObject.GetComponent<HealthBar>();
    }

    private void Start()
    {
        AttackState = (new GameObject($"Player{IndexAccordingToCombatManager}AttackState")).AddComponent<AttackState>();
        AttackState.SetStatePlayerObjects(this);
        BlockState = (new GameObject($"Player{IndexAccordingToCombatManager}BlockState")).AddComponent<BlockState>();
        BlockState.SetStatePlayerObjects(this);
        SweepState = (new GameObject($"Player{IndexAccordingToCombatManager}SweepState")).AddComponent<SweepState>();
        SweepState.SetStatePlayerObjects(this);
        UndecidedState = (new GameObject($"Player{IndexAccordingToCombatManager}UndecidedState")).AddComponent<UndecidedState>();
        UndecidedState.SetStatePlayerObjects(this);

        CurrentCombatState = UndecidedState;
    }

    void Update()
    {
        if (Input.GetButtonDown(attackButton))
        {
            CheckAndSetChoice(CombatMove.Attack);
        }
        if (Input.GetButtonDown(blockButton))
        {
            CheckAndSetChoice(CombatMove.Block);
        }
        if (Input.GetButtonDown(sweepButton))
        {
            CheckAndSetChoice(CombatMove.Sweep);
        }
    }

    private void CheckAndSetChoice(CombatMove move)
    {
        if (CanMakeChoice && !hasMadeChoiceThisPhrase && !IsDead)
        {
            MoveToExecute = move;

            switch (move)
            {
                case CombatMove.Attack:
                    CurrentCombatState = AttackState;
                    break;
                case CombatMove.Block:
                    CurrentCombatState = BlockState;
                    break;
                case CombatMove.Sweep:
                    CurrentCombatState = SweepState;
                    break;
            }

            portraitImage.sprite = activatedPortrait;
            audioSource.PlayOneShot(choiceMadeClip);
            Debug.Log($"{this.name} has chosen {move}");
        }
    }

    public void ExecuteMakingChoiceAnimations()
    {
        animator.SetTrigger(gettingReadyAnimationTrigger);
    }

    /// <summary>
    /// Plays the player's queued animation immediately. This should only be called at the end of a phrase.
    /// </summary>
    public void ExecuteQueuedCombatMove()
    {
        CurrentCombatState.ExecuteQueuedCombatMove();
    }

    private void ResetChoiceValuesAndStartChoiceAnimation()
    {
        hasMadeChoiceThisPhrase = false;
        CurrentCombatState = UndecidedState;
        portraitImage.sprite = unactivatedPortrait;
    }

    private IEnumerator SetUpStarPowerMove()
    {
        // Need to wait before invoking so that the combat animations have time to finish.
        yield return new WaitForSeconds(1.25f);
        PlayerHasFullStarPower?.Invoke(IndexAccordingToCombatManager);
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

    public void PlayerFallsEvent(int indexOfOtherPlayer)
    {
        Fall?.Invoke(indexOfOtherPlayer);
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
        CurrentCombatState.GetAttacked(possibleDamage, bonus, indexOfWinner);
    }

    public void GetBlocked()
    {
        CurrentCombatState.GetBlocked();
    }

    public void GetSwept(int indexOfWinner)
    {
        CurrentCombatState.GetSwept(indexOfWinner);
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
}
