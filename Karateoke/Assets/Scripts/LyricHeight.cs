using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class LyricHeight : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI lyricText, timerText;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private float delay = 0.1f;

    [TextArea(5, 10)]
    [SerializeField]
    private string fullLyrics;

    [TextArea(5, 10)]
    [SerializeField]
    private string wordTimeString;

    private int indexOfHighestValue;
    private int indexToPrint;
    private float highestValue;
    private float[] samples = new float[2048];
    private float timeElapsed;
    private int indexOfNextWordTime;
    private string outputText;
    private string currentTimeText;
    private bool isPlaying;

    // Have to change the length of these with every song since this refers to lyric count.
    private string[] splitLyrics = new string[304];
    private string[] splitWordTimeString = new string[304];
    private float[] wordTimes = new float[304];

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        splitLyrics = fullLyrics.Split(' ');
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
        //isPlaying = true;
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
        if (indexOfHighestValue > 6 && indexOfHighestValue < 96)
        {
            indexToPrint = indexOfHighestValue;
        }
    }

    private void PrintWordHeight()
    {
        outputText = outputText + $"{indexToPrint} ";
        inputField.text = outputText;
        lyricText.text = splitLyrics[indexOfNextWordTime];
        indexOfNextWordTime++;
    }
}
