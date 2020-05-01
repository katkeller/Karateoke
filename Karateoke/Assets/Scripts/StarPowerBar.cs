﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarPowerBar : MonoBehaviour
{
    [SerializeField]
    private Image ringGraphic;

    [SerializeField]
    private ParticleSystem glow;

    [Tooltip("If the graphic is full to or past this amount (scale of 0.0 to 1.0), then the graphic will begin to glow.")]
    [SerializeField]
    private float glowingLowerLimit = 0.8f;

    [SerializeField]
    private float dividingValue = 100;

    private float ringFill;
    public float RingFill
    {
        get => ringFill;
        set
        {
            ringFill = value;

            if (ringFill > glowingLowerLimit && !isGlowing)
            {
                //this is where we activate glowing and sound
                glow.Play();
                isGlowing = true;
            }
            else if (ringFill < glowingLowerLimit && isGlowing)
            {
                //this is where we deactivate it
                glow.Stop();
                isGlowing = false;
            }
        }
    }

    private bool isGlowing;

    public void ScaleFill(float value)
    {
        RingFill = (value / dividingValue);
        ringGraphic.fillAmount = RingFill;
        Debug.Log($"{name} should be scaled to {RingFill}");
    }

    void Start()
    {
        RingFill = 0.0f;
    }

    void Update()
    {
        
    }
}
