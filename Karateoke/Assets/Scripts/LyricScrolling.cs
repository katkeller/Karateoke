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
    private TextMeshProUGUI timerText;

    [TextArea(5, 10)]
    [SerializeField]
    private string fullLyrics;

    [SerializeField]
    private float[] wordTimes = new float[298];

    [SerializeField]
    private float spacingMultiplier = 4.0f, spacingSubtraction = 60.0f, speedMultiplier = 4.0f;

    private string[] splitLyrics = new string[298];

    private float timeElapsed;
    private TextMeshPro text;
    private Vector3 scrollingTarget;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        splitLyrics = fullLyrics.Split(' ');

        for (int i = 0; i < wordTimes.Length; i++)
        {
            wordTimes[i] = ((wordTimes[i] - 1.0f) * spacingMultiplier) - spacingSubtraction;
            GameObject a = Instantiate(wordPrefab) as GameObject;
            text = a.GetComponent<TextMeshPro>();
            text.text = splitLyrics[i];
            a.transform.position = new Vector3(wordTimes[i], -3.5f, 0);
            a.transform.SetParent(this.transform);
        }

        scrollingTarget = new Vector3(-(transform.position.x * spacingMultiplier) - 1000, transform.position.y, transform.position.z);
        Debug.Log($"scrolling target: {scrollingTarget}");

        audioSource.Play();
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        timerText.text = timeElapsed.ToString("F2");

        transform.position = Vector3.MoveTowards(transform.position, scrollingTarget, Time.deltaTime * speedMultiplier);
    }
}
