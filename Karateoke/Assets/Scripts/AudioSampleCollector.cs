using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    private AudioSource audioSource;

    public static float[] audioBand = new float[8];
    public static float[] audioBandBuffer = new float[8];
    private float[] samples = new float[512];
    private float[] frequencyBand = new float[8];
    private float[] bandBuffer = new float[8];
    private float[] bufferDecrease = new float[8];
    private float[] frequencyBandHighest = new float[8];
    private float highDecreaseSpeed = 1.2f;
    private float lowDecreaseSpeed = 0.005f;

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
        MakeFrequencyBands();
        FrequencyBandBuffer();
        CreateAudioBands();
    }

    private void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (frequencyBand[i] > frequencyBandHighest[i])
            {
                frequencyBandHighest[i] = frequencyBand[i];
            }

            audioBand[i] = (frequencyBand[i] / frequencyBandHighest[i]);
            audioBandBuffer[i] = (bandBuffer[i] / frequencyBandHighest[i]);
        }
    }

    private void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }

    private void FrequencyBandBuffer()
    {
        for (int g = 0; g < 8; ++g)
        {
            if (frequencyBand[g] > bandBuffer[g])
            {
                bandBuffer[g] = frequencyBand[g];
                bufferDecrease[g] = lowDecreaseSpeed;
            }
            else if (frequencyBand[g] < bandBuffer[g])
            {
                bandBuffer[g] -= bufferDecrease[g];
                bufferDecrease[g] *= highDecreaseSpeed;
            }
        }
    }

    private void MakeFrequencyBands()
    {
        int count = 0;

        for (int i = 0; i < 8; i++)
        {
            float average = 0;
            int sampleCount = (int)Mathf.Pow(2, i) * 2;

            if (i == 7)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++;
            }

            average /= count;
            frequencyBand[i] = average * 10;
        }
    }
}
