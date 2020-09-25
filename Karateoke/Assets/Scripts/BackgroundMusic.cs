using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField]
    private GameObject visualizerBarPrefab;

    [SerializeField]
    private float barDistanceFromCenter = 10f;

    [SerializeField]
    private float maxBarScale = 5f, minBarScale = 0.25f;

    [SerializeField]
    private Light[] spotLight = new Light[8];

    [SerializeField]
    private float maxLightIntensity = 14.0f, minLightIntensity = 8.0f;

    private GameObject[] bars = new GameObject[80];

    private AudioSource audioSource;
    private AudioSampleCollector audioSampleCollector;

    private bool musicIsPlaying;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSampleCollector = GetComponent<AudioSampleCollector>();

        CreateBars();
    }

    private void CreateBars()
    {
        for (int e = 0; e < bars.Length; e++)
        {
            GameObject newBar = (GameObject)Instantiate(visualizerBarPrefab);
            newBar.transform.position = this.transform.position;
            newBar.transform.parent = this.transform;
            newBar.name = $"Bar {e}";
            this.transform.eulerAngles = new Vector3(0, -4.5f * e, 0);
            newBar.transform.position = Vector3.forward * barDistanceFromCenter;
            bars[e] = newBar;
        }
    }

    public void StartBackgroundMusic()
    {
        audioSource.Play();
        StartCoroutine(WaitThenStartVisualizer());
    }

    private IEnumerator WaitThenStartVisualizer()
    {
        yield return new WaitForSeconds(1);
        musicIsPlaying = true;
    }

    private void Update()
    {
        if (musicIsPlaying)
        {
            for (int i = 0; i < 8; i++)
            {
                SetBarHeight(i);
            }
        }
    }

    private void SetBarHeight(int index)
    {
        var barIndex = index;
        bool flipOrder = true;
        float yScale = (audioSampleCollector.NormalizedBufferedBands[index] * (maxBarScale - minBarScale)) + minLightIntensity;
        var flippedIndexDistance = (14 - (index * 2)) + 1;

        for (int j = 0; j < 10; j++)
        {
            bars[barIndex].transform.localScale = new Vector3(1, yScale, 1);

            if (flipOrder)
            {
                barIndex += flippedIndexDistance;
            }
            else
            {
                barIndex += (index * 2) + 1;
            }
            flipOrder = !flipOrder;
        }
    }

    private void SetLightValues(int index)
    {
        spotLight[index].intensity = (audioSampleCollector.NormalizedBufferedBands[index] * (maxLightIntensity - minLightIntensity)) + minLightIntensity;
    }
}
