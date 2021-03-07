using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This script is attacked to the start button and starts the gameplay (song, lyric scrolling, etc.)
/// </summary>
public class GameStartManager : MonoBehaviour
{
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private AudioSource accompanyingVocalTrack;

    private bool hasStarted;

    public static event Action StartGame;

    void Update()
    {
        // Once I 3D print a controller/mic stand combo, I'll replace this with a dedicated START button.
        if (Input.GetButtonDown("Player1Attack") && !hasStarted)
        {
            hasStarted = true;
            startButton.enabled = false;
            startButton.gameObject.SetActive(false);

            StartGame?.Invoke();
            accompanyingVocalTrack.Play();
        }
    }
}
