using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private int scalingFrames = 50;

    private int scalingFramesLeft;
    private float healthValue = 1.0f;
    private Vector3 newScale;

    private void Update()
    {
        if (scalingFramesLeft > 0)
        {
            Debug.Log($"{this.name} should scale. New scale is {newScale}");
            transform.localScale = Vector3.Lerp(transform.localScale, newScale, Time.deltaTime * 10);
            scalingFramesLeft--;
        }
    }

    public void ScaleHealthBar(float damage)
    {
        healthValue -= (damage / 100);

        newScale = new Vector3(healthValue, 1.0f);
        scalingFramesLeft = scalingFrames;
    }
}
