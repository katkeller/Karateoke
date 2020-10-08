using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Cinemachine;

/// <summary>
/// This script controls the main camera's dolly track movement as well as the camera switching between the dolly (circle) camera 
/// and the static camera that provides a steady view during combat. It's main fucntion is to make the circle camera's speed 
/// match up with the current phrase's length so that it will arrive back at the start (front of the scene) right as the phrase ends.
/// </summary>
public class MainSceneCameraManager : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField]
    private CinemachineVirtualCamera mainStaticCamera;

    [SerializeField]
    private CinemachineVirtualCamera circleCamera;

    [SerializeField]
    private float[] phraseLengths = new float[10];

    public bool CameraShouldCircleContinuously { get; set; }

    private CinemachineTrackedDolly circleDolly;

    private int phraseCount;
    private float timeSinceLastPhraseEnd;
    private float dollyDuration;
    private bool phraseIsResolving;
    private bool playerIsDead;

    #endregion

    private void Start()
    {
        circleDolly = circleCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        phraseCount = 0;
    }

    private void Update()
    {
        if (phraseIsResolving)
        {
            timeSinceLastPhraseEnd += Time.deltaTime;
        }
    }

    private void OnEndOfPhrase()
    {
        if (!playerIsDead)
        {
            phraseCount++;
            timeSinceLastPhraseEnd = 0;
            phraseIsResolving = true;
            mainStaticCamera.Priority = 100;
            circleCamera.Priority = 0;
        }
    }

    public void StartCircleCam()
    {
        // This is called by the combat manager when a phrase starts after combat has finished.
        if (!playerIsDead)
        {
            phraseIsResolving = false;
            dollyDuration = phraseLengths[phraseCount] - timeSinceLastPhraseEnd;
            Debug.Log($"Dolly Duration: {dollyDuration}");
            StartCoroutine(LerpDollyPosition());
            circleCamera.Priority = 100;
            mainStaticCamera.Priority = 0;
        }
    }

    private IEnumerator LerpDollyPosition()
    {
        float timeElapsed = 0;

        while (timeElapsed < dollyDuration)
        {
            circleDolly.m_PathPosition = Mathf.Lerp(0, 4, timeElapsed / dollyDuration);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        circleDolly.m_PathPosition = 4;

        if (CameraShouldCircleContinuously)
        {
            StartCoroutine(LerpDollyPosition());
        }
    }

    public void SetUpCameraForAfterPlayerWin()
    {
        // This is called from the player victory cutscene Timeline playables at the end of the cutscenes.
        Debug.Log($"{name} recieved a signal.");
        playerIsDead = true;
        CameraShouldCircleContinuously = true;
        phraseIsResolving = false;
        dollyDuration = 10;
        StartCoroutine(LerpDollyPosition());
        circleCamera.Priority = 100;
        mainStaticCamera.Priority = 0;
    }

    private void OnEnable()
    {
        PhraseEndTrigger.EndOfPhrase += OnEndOfPhrase;
    }

    private void OnDisable()
    {
        PhraseEndTrigger.EndOfPhrase -= OnEndOfPhrase;
    }
}
