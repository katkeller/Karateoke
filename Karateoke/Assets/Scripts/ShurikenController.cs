using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShurikenController : MonoBehaviour
{
    private float rotationsPerMinute = 1.0f;

    void Update()
    {
        //transform.Rotate(Vector3.one * rotationsPerMinute * Time.deltaTime, 0, 0);
        transform.Rotate(0, 0, rotationsPerMinute);
    }
}
