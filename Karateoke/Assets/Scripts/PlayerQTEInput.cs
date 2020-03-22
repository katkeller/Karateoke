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
    private float AttackerStartingAdvantage = 0.25f;

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
                WinSingleQTE();
            }
        }
    }

    private Image activeRingGraphic;
    private Image activeButtonGraphic;
    private StarPowerQTE qteManager;
    private bool qteIsHappening;
    private int indexAccordingToCombatManager;
    private float graphicStartingFill;

    public void ActivateQTEButton(int indexOfMove)
    {
        QtePressingFill = graphicStartingFill;
        activeRingGraphic = ringGraphic[indexOfMove];
        activeButtonGraphic = buttonGrapic[indexOfMove];
        activeRingGraphic.enabled = true;
        activeButtonGraphic.enabled = true;
        activeRingGraphic.fillAmount = QtePressingFill;
    }

    public void DeactivateAnyQTEButtons()
    {
        // We get here if the other player has won the current QTE
        activeRingGraphic.enabled = false;
        activeButtonGraphic.enabled = false;
        activeRingGraphic = null;
        activeButtonGraphic = null;
    }

    void Start()
    {
        qteManager = qteManagerObject.GetComponent<StarPowerQTE>();
        indexAccordingToCombatManager = GetComponent<Player>().IndexAccordingToCombatManager;
        //qtePressingValue[0] = 0;
        //qtePressingValue[1] = 0;
        //qtePressingValue[2] = 0;
        QtePressingFill = 0;

        foreach (var image in ringGraphic)
        {
            image.enabled = false;
        }

        foreach (var image in buttonGrapic)
        {
            image.enabled = false;
        }
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

            if (activeRingGraphic != null)
            {
                activeRingGraphic.fillAmount = QtePressingFill;
                Debug.Log($"Fill should be {QtePressingFill}");
            }
        }
    }

    private void CheckAndAddPressValue(int indexOfMove)
    {
        Debug.Log($"Active ring graphic: {activeRingGraphic}. Attempted move: {ringGraphic[indexOfMove]}");
        if (activeRingGraphic == ringGraphic[indexOfMove])
        {
            QtePressingFill += .2f;
            Debug.Log($"Fill should increase: {QtePressingFill}");
        }
    }

    private void WinSingleQTE()
    {
        qteManager.PlayerWonSingleQTE(indexAccordingToCombatManager);
        activeRingGraphic.enabled = false;
        activeButtonGraphic.enabled = false;
        activeRingGraphic = null;
        activeButtonGraphic = null;
    }

    private void OnQTEStart(List<int> buttonPressIndexOrdered, int indexOfPlayerAttemptingSPMove)
    {
        qteIsHappening = true;

        if (indexOfPlayerAttemptingSPMove == indexAccordingToCombatManager)
        {
            graphicStartingFill = AttackerStartingAdvantage;
        }
        else
        {
            graphicStartingFill = 0;
        }

        //if (indexOfPlayerAttemptingSPMove != indexAccordingToCombatManager)
        //{
        //    // Find each move in the list
        //    var indexOfAttack = buttonPressIndexOrdered.FindIndex(item => item == 0);
        //    var indexOfBlock = buttonPressIndexOrdered.FindIndex(item => item == 1);
        //    var indexOfSweep = buttonPressIndexOrdered.FindIndex(item => item == 2);


        //    // Should we have them doing the same moves, or the corresponding move? Same will be easier.
        //    //Start with same I guess?

        //    // Switch order of list to the corresponding reactionary moves if this player is defending
        //    buttonPressIndexOrdered[indexOfAttack] = 1;
        //    buttonPressIndexOrdered[indexOfBlock] = 2;
        //    buttonPressIndexOrdered[indexOfSweep] = 0;
        //}

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
