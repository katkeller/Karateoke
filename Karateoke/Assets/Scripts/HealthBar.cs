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

    private int scalingFramesLeft;
    private float healthValue = 1.0f;
    private Vector3 newScale;

    private void Start()
    {
        if (isStarPowerBar)
            transform.localScale = new Vector3(0, transform.localScale.y);
    }

    private void Update()
    {
        if (scalingFramesLeft > 0)
        {
            Debug.Log($"{this.name} should scale. New scale is {newScale}");
            transform.localScale = Vector3.Lerp(transform.localScale, newScale, Time.deltaTime * 10);
            scalingFramesLeft--;
        }

        if (healthValue < 0.15 && !isStarPowerBar)
        {
            //have a glowing red warning animation
            Debug.Log($"{this.name} is below 15%!");
        }
    }

    public void ScaleHealthBar(float value, bool increase)
    {
        if (increase)
            healthValue += (value / dividingValue);
        else
            healthValue -= (value / dividingValue);

        newScale = new Vector3(healthValue, 1.0f);
        scalingFramesLeft = scalingFrames;
    }
}
