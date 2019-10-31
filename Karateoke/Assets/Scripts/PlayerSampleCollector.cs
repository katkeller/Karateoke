using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

public class PlayerSampleCollector : MonoBehaviour
{
    #region Microphone Input

    [SerializeField]
    private bool useMicrophone = true;

    [SerializeField]
    private AudioClip clipToPlayIfNoMicrophone;

    [SerializeField]
    private AudioMixerGroup microphone1MixerGroup, masterMixerGroup;

    public string selectedMicrophoneDevice;

    #endregion

    [SerializeField]
    private GameObject pitchIndicator;

    private float[] samples = new float[2048];
    public static int indexOfHighestValue;
    public static float highestValue;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (useMicrophone)
        {
            if (Microphone.devices.Length > 0)
            {
                selectedMicrophoneDevice = Microphone.devices[0].ToString();
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
        else
        {
            audioSource.outputAudioMixerGroup = masterMixerGroup;
            audioSource.PlayOneShot(clipToPlayIfNoMicrophone);
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
        GetRelevantFrequency();
    }

    private void GetRelevantFrequency()
    {
        highestValue = samples.Max();
        indexOfHighestValue = samples.ToList().IndexOf(highestValue);
    }

    private void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
    }
}
