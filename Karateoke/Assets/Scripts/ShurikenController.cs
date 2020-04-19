using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShurikenController : MonoBehaviour
{
    private float rotationsPerMinute = 1.0f;

    void Update()
    {
        transform.Rotate(0, 0, rotationsPerMinute);
    }
}
