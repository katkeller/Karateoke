using System.Collections;
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

    [SerializeField]
    private float fillSpeed = 3.0f;

    private float ringFill;
    public float RingFill
    {
        get => ringFill;
        set
        {
            ringFill = value;

            if (ringFill > glowingLowerLimit && !isGlowing)
            {
                //need to add sound
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
    private bool shouldScale;

    public void ScaleFill(float value)
    {
        RingFill = (value / dividingValue);
        shouldScale = true;
        Debug.Log($"{name} should be scaled to {RingFill}");
    }

    void Start()
    {
        RingFill = 0.0f;
    }

    void Update()
    {
        if (shouldScale)
        {
            Scale();
        }
    }

    private void Scale()
    {
        if (RingFill != ringGraphic.fillAmount)
        {
            ringGraphic.fillAmount = Mathf.Lerp(ringGraphic.fillAmount, RingFill, Time.deltaTime * fillSpeed);
        }
        else
        {
            shouldScale = false;
        }
    }
}
