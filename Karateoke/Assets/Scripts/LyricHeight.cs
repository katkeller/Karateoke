using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class LyricHeight : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerText;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private float delay = 18f;

    //[TextArea(5, 10)]
    //[SerializeField]
    //private string fullLyrics;

    [TextArea(5, 10)]
    [SerializeField]
    private string wordTimeString;

    private int indexOfHighestValue;
    private float highestValue;
    private float[] samples = new float[2048];
    private float timeElapsed;
    private int indexOfNextWordTime;
    private string outputText;
    private string currentTimeText;
    private bool isPlaying;

    private string[] splitLyrics = new string[298];
    private string[] splitWordTimeString = new string[298];
    private float[] wordTimes = new float[298];

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //splitLyrics = fullLyrics.Split(' ');
        splitWordTimeString = wordTimeString.Split(' ');

        for (int i = 0; i < splitWordTimeString.Length; i++)
        {
            wordTimes[i] = float.Parse(splitWordTimeString[i]);
        }
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        currentTimeText = timeElapsed.ToString("F2");
        timerText.text = currentTimeText;

        if (isPlaying)
        {
            GetSpectrumAudioSource();
            GetRelevantFrequency();
        }

        if (currentTimeText == wordTimes[indexOfNextWordTime].ToString("F2"))
        {
            PrintWordHeight();
        }
    }

    public void BeginPrinting()
    {
        timeElapsed = 0;
        //audioSource.Play();
        StartCoroutine(DelayAudio());
    }

    IEnumerator DelayAudio()
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
        isPlaying = true;
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

    private void PrintWordHeight()
    {
        outputText = outputText + $"{indexOfHighestValue} ";
        inputField.text = outputText;
        indexOfNextWordTime++;
    }
}
