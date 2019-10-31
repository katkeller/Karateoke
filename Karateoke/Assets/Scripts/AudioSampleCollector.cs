using System.Collections;
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
    private AudioMixerGroup microphone1MixerGroup, masterMixerGroup;

    public string selectedMicrophoneDevice;

    #endregion

    [SerializeField]
    private GameObject pitchIndicator;

    [SerializeField]
    private TextMeshProUGUI rangeText;

    [SerializeField]
    private float startingYPosition = 0.25f;

    [SerializeField]
    private float dividingValue = 20;

    [SerializeField]
    private float alphaMultiplier = 100.0f;

    [SerializeField]
    private float timeToMove = 0.1f;

    private AudioSource audioSource;

    public static float[] audioBand = new float[8];
    public static float[] audioBandBuffer = new float[8];

    private int indexOfHighestValue;
    private float highestValue;

    private float[] samples = new float[2048];

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

    void Update()
    {
        GetSpectrumAudioSource();
        GetRelevantFrequency();
        ChangePitchIndicator();
        SetPitchIndicatorAlpha();
    }

    private void SetPitchIndicatorAlpha()
    {
        float alpha = highestValue * alphaMultiplier;

        Color color = pitchIndicator.GetComponent<SpriteRenderer>().color;
        color.a = alpha;
        pitchIndicator.GetComponent<SpriteRenderer>().color = color;
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
        if(indexOfHighestValue > 6 && indexOfHighestValue < 97)
        {
            if(highestValue > 0.01f)
            {
                Transform startingPosition = pitchIndicator.transform;

                Vector3 endingVector3 = new Vector3(0, (indexOfHighestValue / dividingValue) + startingYPosition, 0);

                pitchIndicator.transform.localPosition = Vector3.Lerp(startingPosition.position, endingVector3, timeToMove);
                //pitchIndicator.transform.localPosition = new Vector3(transform.localPosition.x, (indexOfHighestValue/dividingValue) + startingYPosition, transform.localPosition.z);
            }

            rangeText.text = highestValue.ToString();
        }

    }
}
