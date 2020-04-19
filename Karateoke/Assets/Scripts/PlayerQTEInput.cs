using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class PlayerQTEInput : MonoBehaviour
{
    [SerializeField]
    private Image[] ringGraphic = new Image[3];

    [SerializeField]
    private Image[] buttonGrapic = new Image[3];

    [SerializeField]
    private GameObject[] qteShurikenFireballObject = new GameObject[3];

    [SerializeField]
    private GameObject fireballPrefab;

    [SerializeField]
    private Vector3[] fireballStartingPosition = new Vector3[3];

    [SerializeField]
    private Transform starPowerMovePosition;

    [SerializeField]
    private SkinnedMeshRenderer mainPlayerRenderer;

    [SerializeField]
    private GameObject starPowerModelObject;

    [SerializeField]
    private string attackInputString, blockInputString, sweepInputString;

    [SerializeField]
    private GameObject qteManagerObject;

    [SerializeField]
    private PlayableDirector playerWinsSPMoveDirector;

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

    private float shurikenScale
    {
        get
        {
            return QtePressingFill / 10;
        }
    }

    public Transform AttackStarPowerMovePosition { get => starPowerMovePosition; set => starPowerMovePosition = value; }

    private Animator mainAnimator;
    private Animator starPowerModelAnimator;

    private Image activeRingGraphic;
    private Image activeButtonGraphic;
    //private ParticleSystem activeParticleSystem;
    private GameObject activeShurikenFireballObject;
    private Vector3 currentMoveFireballStartingPosition;

    private StarPowerQTE qteManager;
    private bool qteIsHappening;
    private int indexAccordingToCombatManager;
    private float graphicStartingFill;
    private float timePassed;
    private int indexOfAttacker;

    private string queuedAnimation;

    #region Animation Events

    //public void SnapPlayerToStarPowerMovePosition()
    //{
    //    this.transform.position = starPowerMovePosition.position;
    //    this.transform.rotation = starPowerMovePosition.rotation;
    //    Debug.Log($"{this.name} should snap to {starPowerMovePosition}");
    //}

    public void ActivateSPMoveObjectForWin()
    {
        //mainPlayerRenderer.enabled = false;
        starPowerModelObject.SetActive(true);
        starPowerModelAnimator.SetTrigger("Win");
        StartCoroutine(WaitThenSetMainModelToInactive());
    }

    public void ActivateSPMoveObjectForLoss()
    {
        //mainPlayerRenderer.enabled = false;
        starPowerModelObject.SetActive(true);
        starPowerModelAnimator.SetTrigger("Lose");
        StartCoroutine(WaitThenSetMainModelToInactive());
    }

    public void ResetAfterStarPowerMove()
    {
        mainPlayerRenderer.enabled = true;
        mainAnimator.SetTrigger("SPMoveOutro");
        StartCoroutine(WaitThenSetSPModelToInactive());
    }

    #endregion

    public void ActivateQTEButtonAndAnimation(int indexOfMove)
    {
        QtePressingFill = graphicStartingFill;
        activeRingGraphic = ringGraphic[indexOfMove];
        activeButtonGraphic = buttonGrapic[indexOfMove];
        //activeParticleSystem = qteParticleSystem[indexOfMove];
        activeShurikenFireballObject = qteShurikenFireballObject[indexOfMove];
        activeRingGraphic.enabled = true;
        activeButtonGraphic.enabled = true;
        activeShurikenFireballObject.SetActive(true);
        activeRingGraphic.fillAmount = QtePressingFill;
        activeShurikenFireballObject.transform.localScale = new Vector3(shurikenScale, shurikenScale, shurikenScale);
        Debug.Log($"Shuriken Scale: {shurikenScale}");
        currentMoveFireballStartingPosition = fireballStartingPosition[indexOfMove];

        queuedAnimation = loseQTEAnimationTrigger;

        switch (indexOfMove)
        {
            case 0:
                mainAnimator.SetTrigger(highAnimationTrigger);
                break;
            case 1:
                mainAnimator.SetTrigger(midAnimationTrigger);
                break;
            case 2:
                mainAnimator.SetTrigger(lowAnimationTrigger);
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
        if (activeShurikenFireballObject != null)
        {
            activeShurikenFireballObject.SetActive(false);
            activeShurikenFireballObject = null;
        }
        //if (currentMoveFireballStartingPosition != null)
        //{
        //    currentMoveFireballStartingPosition = null;
        //}

        mainAnimator.SetTrigger(queuedAnimation);
        Debug.Log($"{this.name} should be playing {queuedAnimation}");
    }

    public void CreateFireball()
    {
        GameObject fireball = Instantiate(fireballPrefab) as GameObject;
        fireball.transform.position = currentMoveFireballStartingPosition;
    }

    public void StarPowerMoveWasSuccessful()
    {
        if (indexOfAttacker == indexAccordingToCombatManager)
        {
            //Perform Star Power move
            
            mainAnimator.SetTrigger(winOverallAnimationTrigger);
            playerWinsSPMoveDirector.Play();
        }
        else
        {
            //Get hit by Star Power move
            mainAnimator.SetTrigger(loseOverallAnimationTrigger);
            Debug.Log($"{this.name} should get hit by star power move.");
        }
    }

    public void StarPowerMoveWasUnsuccessful()
    {
        if (indexOfAttacker == indexAccordingToCombatManager)
        {
            mainAnimator.SetTrigger(frustratedAnimationTrigger);
            Debug.Log($"{this.name} should be dissapointed.");
        }
        else
        {
            mainAnimator.SetTrigger(reliefAnimationTrigger);
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
        foreach (var star in qteShurikenFireballObject)
        {
            star.SetActive(false);
        }

        mainAnimator = GetComponent<Animator>();
        starPowerModelAnimator = starPowerModelObject.GetComponent<Animator>();
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

                mainAnimator.SetFloat(animationFloatName, QtePressingFill);

                activeShurikenFireballObject.transform.localScale = new Vector3(shurikenScale, shurikenScale, shurikenScale);
            }
        }
    }

    private void CheckAndAddPressValue(int indexOfMove)
    {
        if (activeRingGraphic == ringGraphic[indexOfMove])
        {
            QtePressingFill += increaseOnPress;
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

    IEnumerator WaitThenSetMainModelToInactive()
    {
        yield return new WaitForSeconds(0.5f);
        mainPlayerRenderer.enabled = false;
    }

    IEnumerator WaitThenSetSPModelToInactive()
    {
        yield return new WaitForSeconds(1);
        starPowerModelAnimator.SetTrigger("Reset");
        starPowerModelObject.SetActive(false);
    }
}
