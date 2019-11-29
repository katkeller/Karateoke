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

    private int scalingFramesLeft;
    private float healthValue;
    private Vector3 newScale;
    private bool isDead;

    private void Start()
    {
        if (isStarPowerBar)
        {
            healthValue = 0.0f;
            transform.localScale = new Vector3(0, transform.localScale.y);
        }
        else
        {
            healthValue = 1.0f;
        }
    }

    private void Update()
    {
        if (scalingFramesLeft > 0 && !isDead)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, newScale, Time.deltaTime * 10);
            scalingFramesLeft--;
        }

        if (healthValue < 0.15 && !isStarPowerBar)
        {
            bar.GetComponent<SpriteRenderer>().color = alertColor;
            Debug.Log($"{this.name} is below 15%!");
        }
    }

    public void ScaleHealthBar(float value, bool increase)
    {
        if (increase)
        {
            healthValue += (value / dividingValue);
        }
        else
        {
            healthValue -= (value / dividingValue);
        }

        if (healthValue < 0)
            healthValue = 0;
        else if (healthValue > 1)
            healthValue = 1;

        newScale = new Vector3(healthValue, 1.0f);
        scalingFramesLeft = scalingFrames;
    }
}
