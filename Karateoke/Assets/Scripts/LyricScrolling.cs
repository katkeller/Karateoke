using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class LyricScrolling : MonoBehaviour
{
    [SerializeField]
    private GameObject wordPrefab;

    [TextArea(5, 10)]
    [SerializeField]
    private string fullLyrics;

    [SerializeField]
    private float[] wordTimes = new float[298];

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

    //this value is 0.0333 because we have 90 samples to put into a 3 unit range, so 3 / 90 = 0.0333
    //We multiply each word's highest index value (that we got from the LyricPlacementScene) by 0.0333
    //(after subtracting 56, since the indexes we care about start at 7). Then we subtract 4.6 to get
    //the y-axis placement, since we're placing the lyrics between -1.6 and -4.6 (for the time being).
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
    //private AudioSource audioSource;

    private void Awake()
    {
        //audioSource = GetComponent<AudioSource>();
    }

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
                    //does this work?
                }
            }

            //have to add tag setting for words that are end of phrase triggers
        }

        scrollingTarget = new Vector3(-(transform.position.x * spacingMultiplier) - 1500, transform.position.y, transform.position.z);

        //audioSource.Play();
    }

    void Update()
    {
        if (shouldScroll)
        {
            timeElapsed += Time.deltaTime;
            //timerText.text = timeElapsed.ToString("F2");

            transform.position = Vector3.MoveTowards(transform.position, scrollingTarget, Time.deltaTime * speedMultiplier);
        }
    }

    public void StartScrolling()
    {
        shouldScroll = true;
    }
}
