﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;

public class AudioSampleCollector : MonoBehaviour
{

    #region Microphone Input

    [SerializeField]
    private bool useMicrophone = false;

    [SerializeField]
    private AudioClip clipToPlayIfNoMicrophone;

    [SerializeField]
    private AudioMixerGroup microphone1MixerGroup, masterMixerGroup, sourceMixerGroup;

    [SerializeField]
    private int indexOfMicToUse;

    public string selectedMicrophoneDevice;


    #endregion

    [SerializeField]
    private GameObject pitchIndicator;

    [Tooltip("The source will use a seperate mixing group and will not use a pitch indicator.")]
    [SerializeField]
    private bool isSource;

    [Tooltip("This value is used to multiply the highest value to get the pitch indicator's alpha so that it's visible when the player is singing.")]
    [SerializeField]
    private float alphaMultiplier = 100.0f;

    [SerializeField]
    private float pitchIndicatorHeightMultiplier = 0.04f;

    [SerializeField]
    private float timeToMove = 0.1f;

    private AudioSource audioSource;

    public static float[] audioBand = new float[8];
    public static float[] audioBandBuffer = new float[8];
    private float pitchIndicatorHeight;

    public int indexOfHighestValue;
    public float highestValue;

    private float[] samples = new float[2048];

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();


        if (useMicrophone && !isSource)
        {
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                Debug.Log($"Microphone: {Microphone.devices[i].ToString()}");
            }

            if (Microphone.devices.Length > 0)
            {
                selectedMicrophoneDevice = Microphone.devices[indexOfMicToUse].ToString();
                Debug.Log($"{selectedMicrophoneDevice} is connected and is being used.");
                audioSource.outputAudioMixerGroup = microphone1MixerGroup;
                audioSource.clip = Microphone.Start(selectedMicrophoneDevice, true, 10, AudioSettings.outputSampleRate);
                while (!(Microphone.GetPosition(null) > 0)) { }
                audioSource.Play();
            }
            else
            {
                useMicrophone = false;
                Debug.Log("There is no microphone connected. Please connect a microphone to proceed.");

                //TODO: Add user facing error popup
            }
        }
        else if (!isSource)
        {
            audioSource.outputAudioMixerGroup = masterMixerGroup;
        }
        else
        {
            audioSource.outputAudioMixerGroup = sourceMixerGroup;
        }
    }

    public void StartMusic()
    {
        audioSource.PlayOneShot(clipToPlayIfNoMicrophone);
    }

    void Update()
    {
        GetSpectrumAudioSource();
        GetRelevantFrequency();

        if (!isSource)
        {
            ChangePitchIndicator();
            SetPitchIndicatorAlpha();
        }
    }

    private void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
    }

    private void GetRelevantFrequency()
    {
        highestValue = samples.Max();
        indexOfHighestValue = samples.ToList().IndexOf(highestValue);
    }

    private void ChangePitchIndicator()
    {
        //element 6 contains 82 hz, which is the lowest note in the human vocal range.
        //element 97 contains 1047 hz, which is the highest note in the human vocal range.

        // I changed 97 to 96 because #1, whose voice actually goes that high,
        // and #2, it makes it much easier to calculate word height for the lyrics
        // since then we only have 90 samples.
        if(indexOfHighestValue > 6 && indexOfHighestValue < 96)
        {
            if(highestValue > 0.01f)
            {
                Transform startingPosition = pitchIndicator.transform;
                pitchIndicatorHeight = ((indexOfHighestValue - 6) * pitchIndicatorHeightMultiplier) - 4.6f;
                Vector3 endingVector3 = new Vector3(startingPosition.position.x, pitchIndicatorHeight, startingPosition.position.z);

                pitchIndicator.transform.localPosition = Vector3.Lerp(startingPosition.position, endingVector3, timeToMove);
            }
        }

    }

    private void SetPitchIndicatorAlpha()
    {
        float alpha = highestValue * alphaMultiplier;

        Color color = pitchIndicator.GetComponent<SpriteRenderer>().color;
        color.a = alpha;
        pitchIndicator.GetComponent<SpriteRenderer>().color = color;
    }
}
