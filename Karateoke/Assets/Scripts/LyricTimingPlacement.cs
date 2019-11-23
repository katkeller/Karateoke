using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LyricTimingPlacement : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI lyricText, timerText;

    [SerializeField]
    private TMP_InputField inputField;

    [TextArea(5, 10)]
    [SerializeField]
    private string fullLyrics;

    private int currentWordIndex = 0;
    private bool hasStarted = false;
    private string[] splitLyrics = new string[100];
    private string outputText;

    private AudioSource audioSource;

    private float timeElapsed;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        splitLyrics = fullLyrics.Split(' ');
        lyricText.text = splitLyrics[currentWordIndex];
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        timerText.text = timeElapsed.ToString("F2");
    }

    public void AdvanceLyrics()
    {
        if (!hasStarted)
        {
            timeElapsed = 0.0f;
            audioSource.Play();
            hasStarted = true;
        }
        else
        {
            Debug.Log($"{splitLyrics[currentWordIndex]}: {timeElapsed}");
            //outputText = outputText + $"{splitLyrics[currentWordIndex]}: {timeElapsed}, ";
            outputText = outputText + $"{timeElapsed} ";
            inputField.text = outputText;
            currentWordIndex++;
            lyricText.text = splitLyrics[currentWordIndex];
        }
    }
}
