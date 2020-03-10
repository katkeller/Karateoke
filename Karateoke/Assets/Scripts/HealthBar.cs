using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private int scalingFrames = 50;

    [Tooltip("If this is a health bar, this should be 100. If this is a star power bar, it should be 10.")]
    [SerializeField]
    private int dividingValue = 100;

    [SerializeField]
    private bool isStarPowerBar;

    [SerializeField]
    private GameObject bar;

    [SerializeField]
    private Color alertColor;


    private float healthValue;
    private float HealthValue
    {
        get
        {
            return healthValue;
        }
        set
        {
            // This is to keep the health bars from extending past their min or max scale.
            healthValue = value;
            if (healthValue < 0)
            {
                healthValue = 0;
            }
            else if (healthValue > 1)
            {
                healthValue = 1;
            }
        }
    }

    private int scalingFramesLeft;
    private Vector3 newScale;
    private bool isDead;

    private void Start()
    {
        if (isStarPowerBar)
        {
            HealthValue = 0.0f;
            transform.localScale = new Vector3(0, transform.localScale.y);
        }
        else
        {
            HealthValue = 1.0f;
        }
    }

    private void Update()
    {
        if (scalingFramesLeft > 0 && !isDead)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, newScale, Time.deltaTime * 10);
            scalingFramesLeft--;
        }

        if (HealthValue < 0.15 && !isStarPowerBar)
        {
            bar.GetComponent<SpriteRenderer>().color = alertColor;
            Debug.Log($"{this.name} is below 15%!");
        }
    }

    public void ScaleHealthBar(float value, bool increase)
    {
        HealthValue = value / dividingValue;

        newScale = new Vector3(HealthValue, 1.0f);
        scalingFramesLeft = scalingFrames;

        //change this to take in the new total health value instead
        //if (increase)
        //{
        //    healthValue += (value / dividingValue);
        //}
        //else
        //{
        //    healthValue -= (value / dividingValue);
        //}

        //if (healthValue < 0)
        //    healthValue = 0;
        //else if (healthValue > 1)
        //    healthValue = 1;

        //newScale = new Vector3(healthValue, 1.0f);
        //scalingFramesLeft = scalingFrames;
    }
}
