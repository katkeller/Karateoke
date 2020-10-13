using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;
/// <summary>
/// This script is used for the source vocals, the players' mic input, and the background music (for visualizer purposes).
/// It gets the most prominent (loundest) frequency that is current playing in each respective audio source.
/// </summary>
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

    [SerializeField]
    private bool isVisualizer;

    [Tooltip("This should only be checked if you're currently testing the game using covers you got off the internet.")]
    [SerializeField]
    private bool currentlyTestingWithVocalTracks;

    [Tooltip("This value is used to multiply the highest value to get the pitch indicator's alpha so that it's visible when the player is singing.")]
    [SerializeField]
    private float alphaMultiplier = 100.0f;

    [SerializeField]
    private float pitchIndicatorHeightMultiplier = 0.04f;

    [SerializeField]
    private float pitchIndicatorSpeed = 0.1f;

    public float[] NormalizedBufferedBands = new float[8];

    private AudioSource audioSource;

    private float pitchIndicatorHeight;
    public int indexOfHighestValue;
    public float highestValue;

    private float[] samples = new float[2048];
    private float[] samplesForVisualizer = new float[512];

    private static float[] audioFrequencyBand = new float[8];
    private static float[] audioFrequencyBandBuffer = new float[8];
    private float[] bufferDecrease = new float[8];
    private float[] bandHighestValue = new float[8];


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();


        if (useMicrophone && !isSource && !isVisualizer)
        {
            currentlyTestingWithVocalTracks = false; // Just in case.

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

                //TODO: Add user facing error
            }
        }
        else if (currentlyTestingWithVocalTracks)
        {
            audioSource.outputAudioMixerGroup = microphone1MixerGroup;
            audioSource.clip = clipToPlayIfNoMicrophone;
        }
        else if (isSource)
        {
            audioSource.outputAudioMixerGroup = sourceMixerGroup;
        }
    }

    public void StartMusic()
    {
        if (isSource)
        {
            StartCoroutine(DelayMusic());
        }
        else if (currentlyTestingWithVocalTracks)
        {
            audioSource.Play();
        }
    }

    IEnumerator DelayMusic()
    {
        yield return new WaitForSeconds(0.1f);
        audioSource.PlayOneShot(clipToPlayIfNoMicrophone);
    }

    void Update()
    {
        GetSpectrumAudioSource();
        
        if (!isVisualizer)
        {
            // The relevent frequency is only important to the vocal tracks, since the value it gets is the loudest frequency,
            // which isn't needed for the decorative audio visualizer.
            GetRelevantFrequency();

            if (!isSource)
            {
                ChangePitchIndicator();
                SetPitchIndicatorAlpha();
            }
        }
        else
        {
            CreateFrequencyBands();
            CreateBandBuffers();
            CreateNormalizedBandValues();
        }
    }

    private void GetSpectrumAudioSource()
    {
        if (!isVisualizer)
        {
            audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        }
        else
        {
            audioSource.GetSpectrumData(samplesForVisualizer, 0, FFTWindow.BlackmanHarris);
        }
    }

    private void GetRelevantFrequency()
    {
        highestValue = samples.Max();
        indexOfHighestValue = samples.ToList().IndexOf(highestValue);
    }

    private void CreateFrequencyBands()
    {
        int bandCount = 0;

        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;
            
            if (bandCount == 7)
            {
                sampleCount += 2;
            }

            for (int e = 0; e < sampleCount; e++)
            {
                average += samplesForVisualizer[bandCount] * (bandCount + 1);
                bandCount++;
            }

            average /= bandCount;
            audioFrequencyBand[i] = average * 10;
        }
    }

    private void CreateBandBuffers()
    {
        for (int j = 0; j < 8; j++)
        {
            if (audioFrequencyBand[j] > audioFrequencyBandBuffer[j])
            {
                audioFrequencyBandBuffer[j] = audioFrequencyBand[j];
                bufferDecrease[j] = 0.005f;
            }
            else
            {
                audioFrequencyBandBuffer[j] -= bufferDecrease[j];
                bufferDecrease[j] *= 1.2f;
            }
        }
    }

    private void CreateNormalizedBandValues()
    {
        for (int k = 0; k < 8; k++)
        {
            if (audioFrequencyBand[k] > bandHighestValue[k])
            {
                bandHighestValue[k] = audioFrequencyBand[k];
            }

            NormalizedBufferedBands[k] = (audioFrequencyBandBuffer[k] / bandHighestValue[k]);
        }
    }

    private void ChangePitchIndicator()
    {
        //element 6 contains 82 hz, which is the lowest note in the human vocal range.
        //element 97 contains 1047 hz, which is the highest note in the human vocal range.

        // I changed 97 to 96 because #1, whose voice actually goes that high?,
        // and #2, it makes it much easier to calculate word height for the lyrics
        // since then we only have 90 samples.
        if(indexOfHighestValue > 6 && indexOfHighestValue < 96)
        {
            if(highestValue > 0.01f)
            {
                Transform startingPosition = pitchIndicator.transform;
                pitchIndicatorHeight = ((indexOfHighestValue - 6) * pitchIndicatorHeightMultiplier) - 9.2f;
                Vector3 endingVector3 = new Vector3(startingPosition.position.x, pitchIndicatorHeight, startingPosition.position.z);

                pitchIndicator.transform.position = Vector3.Lerp(startingPosition.position, endingVector3, pitchIndicatorSpeed);
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
