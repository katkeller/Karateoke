using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowingFloor : MonoBehaviour
{
    [SerializeField]
    private Color secondColor;

    private Color originalColor;
    private Material material;

    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        originalColor = material.GetColor("_EmissionColor");
    }

    void Update()
    {
        LerpColor();
    }

    private void LerpColor()
    {
        var color = Color.Lerp(originalColor, secondColor, Mathf.PingPong(Time.time, 1));
        material.SetColor("_EmissionColor", color);
    }
}
