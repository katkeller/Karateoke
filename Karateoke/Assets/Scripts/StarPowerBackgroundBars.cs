using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarPowerBackgroundBars : MonoBehaviour
{
    [SerializeField]
    private GameObject visualizerBarPrefab;

    [SerializeField]
    private float barDistanceFromCenter = 2f;

    [SerializeField]
    private Material barMaterial;

    private GameObject[] bars = new GameObject[64];
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = this.transform.position;
        CreateBars();
    }

    private void CreateBars()
    {
        this.transform.position = new Vector3(0, 0, -6);

        for (int e = 0; e < bars.Length; e++)
        {
            GameObject newBar = (GameObject)Instantiate(visualizerBarPrefab);
            newBar.transform.position = this.transform.position;
            newBar.transform.parent = this.transform;
            newBar.name = $"Bar {e}";
            this.transform.eulerAngles = new Vector3(0, -5.625f * e, 0);
            newBar.transform.position = Vector3.forward * barDistanceFromCenter;
            newBar.transform.localScale = new Vector3(1, 12, 1);
            newBar.GetComponent<MeshRenderer>().material = barMaterial;
            bars[e] = newBar;
        }

        this.transform.eulerAngles = new Vector3(0, 0, 0);
        this.transform.position = originalPosition;
    }
}
