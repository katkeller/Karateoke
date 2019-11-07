using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class LyricScrolling : MonoBehaviour
{
    [SerializeField]
    private GameObject wordPrefab;

    [SerializeField]
    private Vector3 spawningPosition;

    [SerializeField]
    private TextMeshProUGUI timerText;

    [TextArea(5, 10)]
    [SerializeField]
    private string fullLyrics;

    [SerializeField]
    private float[] wordTimes = new float[14];

    [SerializeField]
    private float scrollTimeBuffer = 4.0f;

    //private List<string> splitLyrics = new List<string>();
    private string[] splitLyrics = new string[298];
    private string[] wordTimeStrings = new string[14];
    //private List<float> wordTimesList = new List<float>();
    private List<string> wordTimesList = new List<string>();

    private float timeElapsed;
    private bool spawnWord;
    private int indexOfWordToSpawn;
    private TextMeshPro text;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        splitLyrics = fullLyrics.Split(' ');
        //splitLyrics = splitLyricsArray.OfType<string>().ToList();

        for (int i = 0; i < wordTimes.Length; i++)
        {
            wordTimes[i] -= scrollTimeBuffer;
            wordTimeStrings[i] = wordTimes[i].ToString("F4");
        }

        //wordTimesList = wordTimes.OfType<float>().ToList();
        wordTimesList = wordTimeStrings.OfType<string>().ToList();

        audioSource.Play();
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        timerText.text = timeElapsed.ToString("F4");

        spawnWord |= wordTimesList.Contains(timeElapsed.ToString("F4"));

        if (spawnWord)
        {
            Debug.Log($"{splitLyrics[indexOfWordToSpawn]} should spawn");

            GameObject a = Instantiate(wordPrefab) as GameObject;
            a.transform.position = new Vector3(spawningPosition.x, spawningPosition.y, spawningPosition.z);
            text = a.GetComponent<TextMeshPro>();
            text.text = splitLyrics[indexOfWordToSpawn];
            indexOfWordToSpawn++;
        }
    }
}
