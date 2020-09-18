﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Cinemachine;

public class MainSceneCameraManager : MonoBehaviour
{
    #region Fields and Properties

    [SerializeField]
    private CinemachineVirtualCamera mainStaticCamera;

    [SerializeField]
    private CinemachineVirtualCamera circleCamera;

    [SerializeField]
    private float[] phraseLengths = new float[10];

    private CinemachineTrackedDolly circleDolly;

    private int phraseCount;
    private float timeSinceLastPhraseEnd;
    private float dollyDuration;
    //private float dollySpeed;
    //private float lerpTimeElapsed;
    //private bool shouldDolly;
    private bool phraseIsResolving;

    #endregion

    private void Start()
    {
        circleDolly = circleCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        phraseCount = 0;
        //dollySpeed = 0;
    }

    private void Update()
    {
        if (phraseIsResolving)
        {
            timeSinceLastPhraseEnd += Time.deltaTime;
        }

        //if (shouldDolly && circleDolly.m_PathPosition < 4)
        //{
        //    dollySpeed += Time.deltaTime / dollyDuration;
        //    circleDolly.m_PathPosition = Mathf.Lerp(circleDolly.m_PathPosition, 4, dollySpeed);
        //}
    }

    private void OnEndOfPhrase()
    {
        phraseCount++;
        timeSinceLastPhraseEnd = 0;
        phraseIsResolving = true;
        mainStaticCamera.Priority = 100;
        circleCamera.Priority = 0;
    }

    private void OnStartCircleCam()
    {
        phraseIsResolving = false;

        dollyDuration = phraseLengths[phraseCount] - timeSinceLastPhraseEnd;
        Debug.Log($"Dolly Duration: {dollyDuration}");
        //shouldDolly = true;
        //lerpTimeElapsed = 0;
        StartCoroutine(LerpDollyPosition());
        circleCamera.Priority = 100;
        mainStaticCamera.Priority = 0;
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
    }

    //TODO: Add logic that switches back to correct camera after star power move.

    private void OnEnable()
    {
        CombatTestingScript.EndOfPhrase += OnEndOfPhrase;
        CombatManager.StartCircleCam += OnStartCircleCam;
    }

    private void OnDisable()
    {
        CombatTestingScript.EndOfPhrase -= OnEndOfPhrase;
        CombatManager.StartCircleCam -= OnStartCircleCam;
    }
}
