using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This script controls both the actual background music (which is the karaoke of the song), and the audio
/// visualizer sound bars that create the arena.
/// </summary>
public class BackgroundMusic : MonoBehaviour
{
    [SerializeField]
    private GameObject visualizerBarPrefab;

    [SerializeField]
    private float barDistanceFromCenter = 10f;

    [SerializeField]
    private float maxBarScale = 5f, minBarScale = 0.25f;

    [SerializeField]
    private Material[] barMaterials = new Material[8];

    [SerializeField]
    private Color[] barColors = new Color[8];

    [SerializeField]
    private Color materialColorDuringSPMove;

    [SerializeField]
    private AudioClip starPowerBarsRise;

    [SerializeField]
    private Light[] spotLight = new Light[8];

    [SerializeField]
    private float maxLightIntensity = 14.0f, minLightIntensity = 8.0f;

    private GameObject[] bars = new GameObject[64];

    private AudioSource audioSource;
    private AudioSampleCollector audioSampleCollector;

    private bool visualizerShouldPlay;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSampleCollector = GetComponent<AudioSampleCollector>();

        CreateBars();
        SetBarColors();
    }

    private void CreateBars()
    {
        for (int e = 0; e < bars.Length; e++)
        {
            GameObject newBar = (GameObject)Instantiate(visualizerBarPrefab);
            newBar.transform.position = this.transform.position;
            newBar.transform.parent = this.transform;
            newBar.name = $"Bar {e}";
            this.transform.eulerAngles = new Vector3(0, -5.625f * e, 0);
            newBar.transform.position = Vector3.forward * barDistanceFromCenter;
            bars[e] = newBar;
        }

        this.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    private void SetBarColors()
    {
        // The sound bars are set up to take in 8 sound values from the audio sample collector which will 
        // change the bars' height. Instead of having just 8 bars, I wanted to repeat them in a circle so they
        // would create a pattern. But I wanted this pattern to be refelected rather than just repeating (a.k.a., 
        // the first group of 8 going from sound values 0-7, then the next going from 7-0, then the next 0-7 again, etc.).
        // I also wanted each bar that was assocaiated with a specific sound value to be the same color (so all bars 
        // that were showing the height value for the audio sample #3 would be green, all the ones showing #4 would be blue,
        // and so on). So to achieve this, I made it so that the order switches every 8 bars.
        for (int k = 0; k < 8; k++)
        {
            // k represents each audio sample value that each bar in the group will representing, which is also the index of
            // the color they will be assigned.
            var barIndex = k;
            bool flipOrder = true;
            var flippedIndexDistance = (14 - (k * 2)) + 1;
            barMaterials[k].SetColor("_EmissionColor", barColors[k]);

            for (int l = 0; l < 8; l++)
            {
                // l represents each actual bar in a given audio sample group. To get the reflected pattern effect, instead
                // of just adding 8 to the current bar's index to get the next bar's index (which would end up creating a
                // normal 0-7, 0-7, 0-7 pattern), we switch back and forth between using the (index*2)+1 and the flipped index distance. 
                // This flipped distance is the distance between two bars when the first is in 7-0 order and the next is in 0-7 order.
                bars[barIndex].GetComponent<MeshRenderer>().material = barMaterials[k];

                if (flipOrder)
                {
                    barIndex += flippedIndexDistance;
                }
                else
                {
                    barIndex += (k * 2) + 1;
                }
                flipOrder = !flipOrder;
            }
        }
    }

    public void StartBackgroundMusic()
    {
        audioSource.Play();
        StartCoroutine(WaitThenStartVisualizer());
    }

    private IEnumerator WaitThenStartVisualizer()
    {
        yield return new WaitForSeconds(0.5f);
        visualizerShouldPlay = true;
    }

    private void Update()
    {
        if (visualizerShouldPlay)
        {
            for (int i = 0; i < 8; i++)
            {
                SetBarHeight(i);
            }
        }
    }

    private void SetBarHeight(int index)
    {
        // This function uses pretty much the same logic as the logic that assigns each bar its color.
        var barIndex = index;
        bool flipOrder = true;
        float yScale = (audioSampleCollector.NormalizedBufferedBands[index] * (maxBarScale - minBarScale)) + minBarScale;
        var flippedIndexDistance = (14 - (index * 2)) + 1;

        for (int j = 0; j < 8; j++)
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

    private void OnStarPowerMoveStarts(int index)
    {
        for (int m = 0; m < 8; m++)
        {
            barMaterials[m].SetColor("_EmissionColor", materialColorDuringSPMove);
        }

        visualizerShouldPlay = false;
        StartCoroutine(LerpBarHeight(0.25f, 12));
    }

    private void OnStarPowerMoveEnds(int index, int otherIndex)
    {
        for (int n = 0; n < 8; n++)
        {
            barMaterials[n].SetColor("_EmissionColor", barColors[n]);
        }

        StartCoroutine(LerpBarHeight(12, 0.25f, startVisualizer: true));
    }

    private IEnumerator LerpBarHeight(float startingScale, float endingScale, bool? startVisualizer = null)
    {
        audioSource.PlayOneShot(starPowerBarsRise);

        float timeElapsed = 0;
        float duration = 1f;

        while (timeElapsed < duration)
        {
            float yScale = Mathf.Lerp(startingScale, endingScale, timeElapsed / duration);
            foreach (var bar in bars)
            {
                bar.transform.localScale = new Vector3(1, yScale, 1);
            }
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        if (startVisualizer == true)
        {
            visualizerShouldPlay = true;
        }
    }

    private void OnEnable()
    {
        Player.PlayerHasFullStarPower += OnStarPowerMoveStarts;
        Player.StarPowerMoveEnds += OnStarPowerMoveEnds;
    }

    private void OnDisable()
    {
        Player.PlayerHasFullStarPower -= OnStarPowerMoveStarts;
        Player.StarPowerMoveEnds -= OnStarPowerMoveEnds;
    }

    private void SetLightValues(int index)
    {
        spotLight[index].intensity = (audioSampleCollector.NormalizedBufferedBands[index] * (maxLightIntensity - minLightIntensity)) + minLightIntensity;
    }
}
