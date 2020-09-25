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
        for (int k = 0; k < 8; k++)
        {
            var barIndex = k;
            bool flipOrder = true;
            var flippedIndexDistance = (14 - (k * 2)) + 1;
            barMaterials[k].SetColor("_EmissionColor", barColors[k]);

            for (int l = 0; l < 8; l++)
            {
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
            Debug.Log($"Y Scale: {yScale}");
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
