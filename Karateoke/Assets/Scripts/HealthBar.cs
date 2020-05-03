using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Image mainBar;

    [SerializeField]
    private float lerpSpeed = 2;

    private float targetFillAmount;
    private bool shouldScale;


    public void SetHealthBarToValue(float newHealthValue)
    {
        targetFillAmount = newHealthValue / 100;
        shouldScale = true;
        Debug.Log($"{name} should be scaling to {newHealthValue}, {targetFillAmount}.");
    }

    private void Update()
    {
        if (shouldScale)
        {
            Scale();
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
}
