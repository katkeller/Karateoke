using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
/// <summary>
/// This script controls the scrolling lyrics placement which denotes each lyrics timing and pitch.
/// </summary>
public class LyricScrolling : MonoBehaviour
{
    [SerializeField]
    private GameObject wordPrefab;

    [TextArea(5, 10)]
    [SerializeField]
    private string fullLyrics;

    // This is a string rather than an array so that I can easily copy and paste the values in from the tool
    // I created that allows me to get a string value with each lyric's time in it.
    [TextArea(5, 10)]
    [SerializeField]
    private string wordTimesString;

    [TextArea(5, 10)]
    [SerializeField]
    private string wordHeights;

    [SerializeField]
    private int[] indexesOfPhraseEndLyrics = new int[30];

    [SerializeField]
    private Color32 phraseEndColor;

    [SerializeField]
    private float spacingMultiplier = 4.0f, spacingSubtraction = 60.0f, speedMultiplier = 4.0f;

    [SerializeField]
    private float heightMultiplier = 0.04f, yAxisSubtraction = 4.6f;

    private string[] splitLyrics = new string[304];
    private string[] splitHeights = new string[304];
    private string[] splitWordTimesString = new string[304];
    private float[] splitWordTimes = new float[304];
    private float[] wordHeightFloats = new float[304];

    private float timeElapsed;
    private TextMeshPro text;
    private Vector3 scrollingTarget;
    private bool shouldScroll;

    void Start()
    {
        splitLyrics = fullLyrics.Split(' ');
        splitHeights = wordHeights.Split(' ');
        splitWordTimesString = wordTimesString.Split(' ');

        for (int e = 0; e < splitHeights.Length; e++)
        {
            wordHeightFloats[e] = float.Parse(splitHeights[e]);
        }

        for (int k = 0; k < splitWordTimesString.Length; k++)
        {
            splitWordTimes[k] = float.Parse(splitWordTimesString[k]);
        }

        for (int i = 0; i < splitWordTimes.Length; i++)
        {
            splitWordTimes[i] = ((splitWordTimes[i] - 1.0f) * spacingMultiplier) - spacingSubtraction;
            wordHeightFloats[i] = ((wordHeightFloats[i] - 6) * heightMultiplier) - yAxisSubtraction;
            GameObject a = Instantiate(wordPrefab) as GameObject;
            text = a.GetComponent<TextMeshPro>();
            text.text = splitLyrics[i];
            a.transform.position = new Vector3(splitWordTimes[i], wordHeightFloats[i], 0);
            a.transform.SetParent(this.transform);

            foreach (int element in indexesOfPhraseEndLyrics)
            {
                if (i == element)
                {
                    a.tag = "PhraseEnd";
                    text.color = phraseEndColor;
                }
            }
        }

        scrollingTarget = new Vector3(-(transform.position.x * spacingMultiplier) - 1500, transform.position.y, transform.position.z);
    }

    public void StartScrolling()
    {
        // This is called by the game start manager.
        shouldScroll = true;
    }

    void Update()
    {
        if (shouldScroll)
        {
            timeElapsed += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, scrollingTarget, Time.deltaTime * speedMultiplier);
        }
    }
}
