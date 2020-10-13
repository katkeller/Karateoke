using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Each player has a health bar object that gets sent info from their Player script, then lerps to the correct size.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image mainBar;

    [SerializeField]
    private float lerpSpeed = 2;

    [SerializeField]
    private Color warningColor;

    private float targetFillAmount;
    private bool shouldScale;

    private float currentValue;
    private Color originalColor;
    private Color currentColor;
    private Color targetColor;

    public void SetHealthBarToValue(float newHealthValue)
    {
        currentValue = newHealthValue;
        targetFillAmount = newHealthValue / 100;
        shouldScale = true;
        Debug.Log($"{name} should be scaling to {newHealthValue}, {targetFillAmount}.");
    }

    private void Start()
    {
        originalColor = mainBar.color;
        currentColor = originalColor;
        targetColor = warningColor;
        currentValue = 100;
    }

    private void Update()
    {
        if (shouldScale)
        {
            Scale();
        }
        if (currentValue < 35)
        {
            // If the player's health is below 35%, then we want the bar to flash red.
            Flash();
        }
    }

    private void Scale()
    {
        if (targetFillAmount != mainBar.fillAmount)
        {
            mainBar.fillAmount = Mathf.Lerp(mainBar.fillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
        }
        else
        {
            shouldScale = false;
        }
    }

    private void Flash()
    {
        mainBar.color = Color.Lerp(currentColor, targetColor, Mathf.PingPong(Time.time, 1));
    }
}
