using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSoundbars : MonoBehaviour
{
    [SerializeField]
    private bool useBuffer;

    [SerializeField]
    private int[] bandNumber = new int[7];

    [SerializeField]
    private float startScale = .25f, scaleMultiplier = 10.0f;

    [SerializeField]
    private GameObject[] soundBars = new GameObject[7];

    void Update()
    {

        for (int i = 0; i < soundBars.Length; i++)
        {
            if (useBuffer)
            {
                soundBars[i].transform.localScale = new Vector3(transform.localScale.x, (AudioSampleCollector.audioBandBuffer[bandNumber[i]] * scaleMultiplier) + startScale, transform.localScale.z);
            }
            else if (!useBuffer)
            {
                soundBars[i].transform.localScale = new Vector3(transform.localScale.x, (AudioSampleCollector.audioBand[bandNumber[i]] * scaleMultiplier) + startScale, transform.localScale.z);
            }
        }
    }
}
