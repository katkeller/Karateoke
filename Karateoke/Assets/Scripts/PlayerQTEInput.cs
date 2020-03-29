using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerQTEInput : MonoBehaviour
{
    [SerializeField]
    private Image[] ringGraphic = new Image[3];

    [SerializeField]
    private Image[] buttonGrapic = new Image[3];

    [SerializeField]
    private string attackInputString, blockInputString, sweepInputString;

    [SerializeField]
    private GameObject qteManagerObject;

    [SerializeField]
    private float increaseOnPress = 0.2f, decreaseByTime = 0.02f;

    [SerializeField]
    private float AttackerStartingAdvantage = 0.25f;

    [SerializeField]
    private float timeBetweenValueDecrease = 0.1f;

    [SerializeField]
    private string highAnimationTrigger, midAnimationTrigger, lowAnimationTrigger, animationFloatName;

    [SerializeField]
    private string winQTEAnimationTriggger, loseQTEAnimationTrigger;

    [SerializeField]
    private string winOverallAnimationTrigger, loseOverallAnimationTrigger, reliefAnimationTrigger, frustratedAnimationTrigger;

    // do we need this? I think no since the QTEs should go until either someone wins or
    // the attacker fails (lets the QTE value go to 0). Or maybe if it goes to 0 it doesn't matter?
    [SerializeField]
    private float secondsPerMove = 3.0f;

    private float qtePressingFill;
    public float QtePressingFill
    {
        get => qtePressingFill;
        set
        {
            qtePressingFill = value;

            if (qtePressingFill < 0)
            {
                qtePressingFill = 0;
            }

            if (qtePressingFill >= 1)
            {
                qtePressingFill = 1;
                WinSingleQTE();
            }
        }
    }

    private Animator animator;
    private Image activeRingGraphic;
    private Image activeButtonGraphic;
    private StarPowerQTE qteManager;
    private bool qteIsHappening;
    private int indexAccordingToCombatManager;
    private float graphicStartingFill;
    private float timePassed;
    private int indexOfAttacker;

    private string queuedAnimation;

    public void ActivateQTEButtonAndAnimation(int indexOfMove)
    {
        //if (activeButtonGraphic != null)
        //{
        //    activeButtonGraphic.enabled = false;
        //    activeButtonGraphic = null;
        //}
        //if (activeRingGraphic != null)
        //{
        //    activeRingGraphic.enabled = false;
        //    activeRingGraphic = null;
        //}
        QtePressingFill = graphicStartingFill;
        activeRingGraphic = ringGraphic[indexOfMove];
        activeButtonGraphic = buttonGrapic[indexOfMove];
        activeRingGraphic.enabled = true;
        activeButtonGraphic.enabled = true;
        activeRingGraphic.fillAmount = QtePressingFill;
        queuedAnimation = loseQTEAnimationTrigger;

        switch (indexOfMove)
        {
            case 0:
                animator.SetTrigger(highAnimationTrigger);
                break;
            case 1:
                animator.SetTrigger(midAnimationTrigger);
                break;
            case 2:
                animator.SetTrigger(lowAnimationTrigger);
                break;
            default:
                Debug.Log("Player star power animation error: index not recognized.");
                break;
        }
    }

    public void ExecuteQueuedAnimationAndHideGraphics()
    {
        if (activeButtonGraphic != null)
        {
            activeButtonGraphic.enabled = false;
            activeButtonGraphic = null;
        }
        if (activeRingGraphic != null)
        {
            activeRingGraphic.enabled = false;
            activeRingGraphic = null;
        }

        animator.SetTrigger(queuedAnimation);
        Debug.Log($"{this.name} should be playing {queuedAnimation}");
    }

    public void ResolveQTEs()
    {
        // We get here if the other player has won the current QTE
        //activeRingGraphic.enabled = false;
        //activeButtonGraphic.enabled = false;
        //activeRingGraphic = null;
        //activeButtonGraphic = null;

        


        //do we want reaction animations here..?
    }

    public void StarPowerMoveWasSuccessful()
    {
        if (indexOfAttacker == indexAccordingToCombatManager)
        {
            //Perform Star Power move
            animator.SetTrigger(winOverallAnimationTrigger);
            Debug.Log($"{this.name} should perfrom star power move.");
        }
        else
        {
            //Get hit by Star Power move
            animator.SetTrigger(loseOverallAnimationTrigger);
            Debug.Log($"{this.name} should get hit by star power move.");
        }
    }

    public void StarPowerMoveWasUnsuccessful()
    {
        if (indexOfAttacker == indexAccordingToCombatManager)
        {
            animator.SetTrigger(frustratedAnimationTrigger);
            Debug.Log($"{this.name} should be dissapointed.");
        }
        else
        {
            animator.SetTrigger(reliefAnimationTrigger);
            Debug.Log($"{this.name} should be relieved.");
        }
    }

    void Start()
    {
        qteManager = qteManagerObject.GetComponent<StarPowerQTE>();
        indexAccordingToCombatManager = GetComponent<Player>().IndexAccordingToCombatManager;
        //qtePressingValue[0] = 0;
        //qtePressingValue[1] = 0;
        //qtePressingValue[2] = 0;
        QtePressingFill = 0;
        timePassed = 0;

        foreach (var image in ringGraphic)
        {
            image.enabled = false;
        }

        foreach (var image in buttonGrapic)
        {
            image.enabled = false;
        }

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (qteIsHappening)
        {
            if (Input.GetButtonDown(attackInputString))
            {
                CheckAndAddPressValue(0);
            }
            if (Input.GetButtonDown(blockInputString))
            {
                CheckAndAddPressValue(1);
            }
            if (Input.GetButtonDown(sweepInputString))
            {
                CheckAndAddPressValue(2);
            }

            timePassed += Time.deltaTime;

            if (timePassed > timeBetweenValueDecrease)
            {
                timePassed = 0;
                QtePressingFill -= decreaseByTime;
            }

            if (activeRingGraphic != null)
            {
                activeRingGraphic.fillAmount = QtePressingFill;
                //Debug.Log($"Fill should be {QtePressingFill}");
                animator.SetFloat(animationFloatName, QtePressingFill);
            }
        }
    }

    private void CheckAndAddPressValue(int indexOfMove)
    {
        //Debug.Log($"Active ring graphic: {activeRingGraphic}. Attempted move: {ringGraphic[indexOfMove]}");
        if (activeRingGraphic == ringGraphic[indexOfMove])
        {
            QtePressingFill += increaseOnPress;
            //Debug.Log($"Fill should increase: {QtePressingFill}");
        }
    }

    private void WinSingleQTE()
    {
        queuedAnimation = winQTEAnimationTriggger;
        qteManager.PlayerWonSingleQTE(indexAccordingToCombatManager);
        //Debug.Log($"{activeButtonGraphic.name} enabled?: {activeButtonGraphic.enabled}");
    }

    private void OnQTEStart(List<int> buttonPressIndexOrdered, int indexOfPlayerAttemptingSPMove)
    {
        qteIsHappening = true;
        indexOfAttacker = indexOfPlayerAttemptingSPMove;

        if (indexOfAttacker == indexAccordingToCombatManager)
        {
            graphicStartingFill = AttackerStartingAdvantage;
        }
        else
        {
            graphicStartingFill = 0;
        }
    }

    private void OnQTEEnd()
    {
        qteIsHappening = false;
    }

    private void OnEnable()
    {
        StarPowerQTE.QTEStart += OnQTEStart;
        StarPowerQTE.QTEEnd += OnQTEEnd;
    }

    private void OnDisable()
    {
        StarPowerQTE.QTEStart -= OnQTEStart;
        StarPowerQTE.QTEEnd -= OnQTEEnd;
    }
}
