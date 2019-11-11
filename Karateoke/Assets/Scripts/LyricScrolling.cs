using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class LyricScrolling : MonoBehaviour
{
    [SerializeField]
    private GameObject wordPrefab;

    //[SerializeField]
    //private TextMeshProUGUI timerText;

    [TextArea(5, 10)]
    [SerializeField]
    private string fullLyrics;

    [SerializeField]
    private float[] wordTimes = new float[298];

    [TextArea(5, 10)]
    [SerializeField]
    private string wordHeights;

    [SerializeField]
    private float spacingMultiplier = 4.0f, spacingSubtraction = 60.0f, speedMultiplier = 4.0f;

    //this value is 0.0333 because we have 90 samples to put into a 3 unit range, so 3 / 90 = 0.0333
    //We multiply each word's highest index value (that we got from the LyricPlacementScene) by 0.0333
    //(after subtracting 56, since the indexes we care about start at 7). Then we subtract 4.6 to get
    //the y-axis placement, since we're placing the lyrics between -1.6 and -4.6 (for the time being).
    [SerializeField]
    private float heightMultiplier = 0.04f, yAxisSubtraction = 4.6f;

    private string[] splitLyrics = new string[298];
    private string[] splitHeights = new string[298];
    private float[] wordHeightFloats = new float[298];

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

        for (int e = 0; e < splitHeights.Length; e++)
        {
            wordHeightFloats[e] = float.Parse(splitHeights[e]);
        }

        for (int i = 0; i < wordTimes.Length; i++)
        {
            wordTimes[i] = ((wordTimes[i] - 1.0f) * spacingMultiplier) - spacingSubtraction;
            wordHeightFloats[i] = ((wordHeightFloats[i] - 6) * heightMultiplier) - yAxisSubtraction;
            GameObject a = Instantiate(wordPrefab) as GameObject;
            text = a.GetComponent<TextMeshPro>();
            text.text = splitLyrics[i];
            a.transform.position = new Vector3(wordTimes[i], wordHeightFloats[i], 0);
            a.transform.SetParent(this.transform);
        }

        scrollingTarget = new Vector3(-(transform.position.x * spacingMultiplier) - 1000, transform.position.y, transform.position.z);

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
