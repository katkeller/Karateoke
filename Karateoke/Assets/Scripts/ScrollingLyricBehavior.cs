using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingLyricBehavior : MonoBehaviour
{
    [SerializeField]
    private float lerpTime = 4.0f;

    [SerializeField]
    private Vector3 endingLocation;

    private Vector3 startingLocation;
    private float currentLerpTime;

    private void Start()
    {
        startingLocation = this.transform.position;
    }

    private void Update()
    {
        currentLerpTime += Time.deltaTime;

        if (currentLerpTime >= lerpTime)
        {
            Destroy(this.gameObject);
        }

        float percentage = currentLerpTime / lerpTime;
        this.transform.position = Vector3.Lerp(startingLocation, endingLocation, percentage);
    }
}
