﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartManager : MonoBehaviour
{
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private GameObject scrollingLyricsGameObject, backgroundMusicGameObject, discoFloorObject, player1AudioSampleCollectorGameObject, player2AudioSampleCollectorGameObject, sourceAudioGameObject, combatManagerGameObject;

    private bool hasStarted;

    private LyricScrolling lyricScrolling;
    private BackgroundMusic backgroundMusic;
    private DiscoFloorController discoFloor;
    private AudioSampleCollector player1SampleCollector, player2SampleCollector, sourceSampleCollector;
    private CombatManager combatManager;

    private void Start()
    {
        lyricScrolling = scrollingLyricsGameObject.GetComponent<LyricScrolling>();
        backgroundMusic = backgroundMusicGameObject.GetComponent<BackgroundMusic>();
        discoFloor = discoFloorObject.GetComponent<DiscoFloorController>();
        player1SampleCollector = player1AudioSampleCollectorGameObject.GetComponent<AudioSampleCollector>();
        player2SampleCollector = player2AudioSampleCollectorGameObject.GetComponent<AudioSampleCollector>();
        sourceSampleCollector = sourceAudioGameObject.GetComponent<AudioSampleCollector>();
        combatManager = combatManagerGameObject.GetComponent<CombatManager>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Player1Attack") && !hasStarted)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        hasStarted = true;
        startButton.enabled = false;
        startButton.gameObject.SetActive(false);
        lyricScrolling.StartScrolling();
        backgroundMusic.StartBackgroundMusic();
        discoFloor.StartFloor();
        player1SampleCollector.StartMusic();
        player2SampleCollector.StartMusic();
        sourceSampleCollector.StartMusic();
        combatManager.StartAnimations();
    }
}
